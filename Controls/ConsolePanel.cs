using System.Text;
using System.ComponentModel;

namespace inpsIDE.Controls;

/// <summary>
/// The IDE's console: light themed output on top, an input line at the bottom.
/// Lines typed into the input line are raised through <see cref="InputSubmitted"/> (the owner forwards them to the
/// running program's stdin). <see cref="Write"/> is thread safe; text is shown by a UI timer so that programs that
/// print a lot don't flood the message queue.
/// </summary>
public sealed class ConsolePanel : UserControl
{
    const int MaxChars = 2_000_000, KeepChars = 1_000_000;

    static readonly Color Background = Color.White;
    static readonly Color Foreground = Color.FromArgb(30, 30, 30);
    static readonly Color HeaderColor = Color.FromArgb(243, 243, 243);

    readonly TextBox _output = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        WordWrap = false,
        ScrollBars = ScrollBars.Both,
        BorderStyle = BorderStyle.None,
        Font = new Font("Consolas", 10f),
        BackColor = Background,
        ForeColor = Foreground
    };

    readonly TextBox _input = new()
    {
        Dock = DockStyle.Fill,
        BorderStyle = BorderStyle.FixedSingle,
        Font = new Font("Consolas", 10f),
        BackColor = Background,
        ForeColor = Foreground,
        Enabled = false
    };

    readonly Button _stop = Toolbar("Stop"), _eof = Toolbar("Send EOF"), _clear = Toolbar("Clear");
    readonly System.Windows.Forms.Timer _flushTimer = new() { Interval = 40 };
    readonly StringBuilder _pending = new();
    readonly List<string> _history = [];

    int _historyIndex;
    bool _running;

    /// <summary>The user pressed Enter in the input line.</summary>
    public event Action<string>? InputSubmitted;
    /// <summary>"Send EOF" button or Ctrl+D.</summary>
    public event Action? EndOfInputRequested;
    public event Action? StopRequested;

    public ConsolePanel()
    {
        var title = new Label { Text = "  Console", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
        buttons.Controls.AddRange([_clear, _eof, _stop]);

        var header = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = HeaderColor };
        header.Controls.Add(title);   // fill first, edge-docked controls after it
        header.Controls.Add(buttons);

        var prompt = new Label
        {
            Text = ">", Dock = DockStyle.Left, Width = 18, TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Consolas", 10f, FontStyle.Bold), ForeColor = Foreground
        };
        var inputRow = new Panel { Dock = DockStyle.Bottom, Height = 30, Padding = new Padding(2, 4, 4, 4), BackColor = Background };
        inputRow.Controls.Add(_input);
        inputRow.Controls.Add(prompt);

        Controls.Add(_output);
        Controls.Add(inputRow);
        Controls.Add(header);

        _stop.Enabled = _eof.Enabled = false;
        _stop.Click += (_, _) => StopRequested?.Invoke();
        _eof.Click += (_, _) => EndOfInputRequested?.Invoke();
        _clear.Click += (_, _) => Clear();
        _input.KeyDown += OnInputKeyDown;

        _flushTimer.Tick += (_, _) => Flush();
        _flushTimer.Start();
    }

    static Button Toolbar(string text) => new()
    {
        Text = text,
        Size = new Size(76, 22),
        Margin = new Padding(2, 3, 2, 3),
        UseVisualStyleBackColor = true
    };

    /// <summary>Enables the input line and the Stop / EOF buttons while a program is running.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ProcessRunning
    {
        get => _running;
        set
        {
            if (_running == value) return;
            _running = value;
            _input.Enabled = _eof.Enabled = _stop.Enabled = value;
            if (value) _input.Focus();
        }
    }

    public bool InputFocused => _input.Focused;

    /// <summary>Thread safe. Text may arrive in arbitrary chunks; "\r\n" and "\n" are both treated as a line break.</summary>
    public void Write(string text)
    {
        if (text.Length == 0) return;
        lock (_pending) _pending.Append(text);
    }

    public void WriteLine(string text) => Write(text + "\n");

    public void Clear()
    {
        lock (_pending) _pending.Clear();
        _output.Clear();
    }

    void Flush()
    {
        string chunk;
        lock (_pending)
        {
            if (_pending.Length == 0) return;
            chunk = _pending.ToString();
            _pending.Clear();
        }

        // TextBox wants "\r\n"; dropping every '\r' first makes chunk boundaries inside a "\r\n" harmless.
        chunk = chunk.Replace("\r", "").Replace("\n", "\r\n");

        if (_output.TextLength + chunk.Length > MaxChars)
            _output.Text = _output.Text[^Math.Min(KeepChars, _output.TextLength)..];

        _output.AppendText(chunk);
        _output.SelectionStart = _output.TextLength;
        _output.ScrollToCaret();
    }

    void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Enter:
                string line = _input.Text;
                _input.Clear();
                if (line.Length > 0 && (_history.Count == 0 || _history[^1] != line)) _history.Add(line);
                _historyIndex = _history.Count;
                InputSubmitted?.Invoke(line);
                e.SuppressKeyPress = true;
                break;

            case Keys.Up when _history.Count > 0:
                _historyIndex = Math.Max(0, _historyIndex - 1);
                ShowHistory();
                e.SuppressKeyPress = true;
                break;

            case Keys.Down when _history.Count > 0:
                _historyIndex = Math.Min(_history.Count, _historyIndex + 1);
                ShowHistory();
                e.SuppressKeyPress = true;
                break;

            case Keys.D when e.Control:
                EndOfInputRequested?.Invoke();
                e.SuppressKeyPress = true;
                break;
        }
    }

    void ShowHistory()
    {
        _input.Text = _historyIndex < _history.Count ? _history[_historyIndex] : "";
        _input.SelectionStart = _input.TextLength;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _flushTimer.Dispose();
        base.Dispose(disposing);
    }
}
