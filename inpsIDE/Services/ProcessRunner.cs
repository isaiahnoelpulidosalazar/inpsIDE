using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using inpsIDE.Models;

namespace inpsIDE.Services;

public sealed class ProcessRunner : IDisposable
{
    static readonly UTF8Encoding Utf8NoBom = new(false);

    readonly object _gate = new();
    Process? _process;
    StreamWriter? _stdin;

    public event Action<string>? OutputReceived;
    public event Action<int>? Exited;

    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _process is not null;
            }
        }
    }

    public void Start(RunCommand command)
    {
        lock (_gate)
        {
            if (_process is not null)
            {
                throw new InvalidOperationException("A process is already running.");
            }
        }

        var info = new ProcessStartInfo(command.FileName)
        {
            WorkingDirectory = command.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = Utf8NoBom,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (string argument in command.Arguments)
        {
            info.ArgumentList.Add(argument);
        }
        if (command.Environment is { } environment)
        {
            foreach ((string key, string value) in environment)
            {
                info.Environment[key] = value;
            }
        }

        var process = new Process { StartInfo = info };
        process.Start();

        lock (_gate)
        {
            _process = process;
            _stdin = process.StandardInput;
        }

        Task stdout = PumpAsync(process.StandardOutput);
        Task stderr = PumpAsync(process.StandardError);

        _ = Task.Run(async () =>
        {
            int exitCode = -1;
            try
            {
                await process.WaitForExitAsync().ConfigureAwait(false);
                await Task.WhenAny(Task.WhenAll(stdout, stderr), Task.Delay(2000)).ConfigureAwait(false);
                exitCode = process.ExitCode;
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception) { }

            lock (_gate)
            {
                if (ReferenceEquals(_process, process))
                {
                    _process = null;
                    _stdin = null;
                }
            }

            try
            {
                process.Dispose();
            }
            catch (Exception ex) when (ex is InvalidOperationException or Win32Exception) { }
            Exited?.Invoke(exitCode);
        });
    }

    async Task PumpAsync(StreamReader reader)
    {
        char[] buffer = new char[4096];
        try
        {
            int count;
            while ((count = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                OutputReceived?.Invoke(new string(buffer, 0, count));
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException) { }
    }

    public bool SendLine(string text)
    {
        lock (_gate)
        {
            if (_stdin is null)
            {
                return false;
            }
            try
            {
                _stdin.Write(text + "\n");
                _stdin.Flush();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }

    public void CloseInput()
    {
        lock (_gate)
        {
            try
            {
                _stdin?.Close();
            }
            catch (IOException) { }
            _stdin = null;
        }
    }

    public void Stop()
    {
        Process? process;
        lock (_gate)
        {
            process = _process;
        }

        try
        {
            if (process is { HasExited: false })
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or NotSupportedException) { }
    }

    public void Dispose() => Stop();
}
