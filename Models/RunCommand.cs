namespace inpsIDE.Models;

/// <summary>Everything needed to start one child process. Built by a <see cref="Languages.LanguageSupport"/>.</summary>
/// <param name="Display">The command line as shown in the console ("&gt; python main.py").</param>
/// <param name="Environment">Extra environment variables for the child process.</param>
/// <param name="Url">For web servers: the address to show / open in the browser.</param>
/// <param name="OpenBrowserDelayMs">0 = open the browser immediately; otherwise wait that long (and only open if the server is still running).</param>
public sealed record RunCommand(
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    string Display,
    IReadOnlyDictionary<string, string>? Environment = null,
    string? Url = null,
    int OpenBrowserDelayMs = 0);
