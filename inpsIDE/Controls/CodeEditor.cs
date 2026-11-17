using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using inpsNuGet;
using inpsIDE.Languages;

namespace inpsIDE.Controls;

public sealed class CodeEditor : UserControl
{
    const int WM_SETREDRAW = 0x000B;
    const int EM_GETSCROLLPOS = 0x04DD;
    const int EM_SETSCROLLPOS = 0x04DE;
    const int MaxHighlightLength = 300_000;

    static readonly Color Background = Color.White;
    static readonly Color DefaultColor = Color.FromArgb(30, 30, 30);
    static readonly Color KeywordColor = Color.FromArgb(0, 0, 255);
    static readonly Color StringColor = Color.FromArgb(163, 21, 21);
    static readonly Color CommentColor = Color.FromArgb(0, 128, 0);
    static readonly Color VariableColor = Color.FromArgb(0, 16, 128);
    static readonly Color NumberColor = Color.FromArgb(9, 134, 88);
    static readonly Color TagColor = Color.FromArgb(175, 0, 219);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref Point lParam);

    readonly RichTextBox _text;
    readonly LineNumberGutter _gutter;
    readonly System.Windows.Forms.Timer _highlightTimer = new() { Interval = 350 };

    Regex? _tokens;
    string _indentTriggers = "{:";
    bool _loading, _highlighting, _pendingHighlight, _usesCrlf;

    public bool IsDirty { get; private set; }
    public event EventHandler? DirtyChanged;
    public event EventHandler? CaretMoved;

    public (int Line, int Column) CaretPosition
    {
        get
        {
            int index = _text.SelectionStart;
            int line = _text.GetLineFromCharIndex(index);
            return (line + 1, index - _text.GetFirstCharIndexFromLine(line) + 1);
        }
    }

    public CodeEditor()
    {
        _text = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            WordWrap = false,
            AcceptsTab = true,
            DetectUrls = false,
            HideSelection = false,
            ScrollBars = RichTextBoxScrollBars.Both,
            Font = new Font("Consolas", 11f),
            BackColor = Background,
            ForeColor = DefaultColor
        };
        _gutter = new LineNumberGutter(_text) { Dock = DockStyle.Left };

        Controls.Add(_text);
        Controls.Add(_gutter);
        _text.BringToFront();

        _text.TextChanged += OnTextChanged;
        _text.VScroll += (_, _) => _gutter.Invalidate();
        _text.KeyDown += OnKeyDown;
        _text.SelectionChanged += (_, _) =>
        {
            if (!_highlighting)
            {
                CaretMoved?.Invoke(this, EventArgs.Empty);
            }
        };

        _highlightTimer.Tick += (_, _) =>
        {
            _highlightTimer.Stop();
            if (Visible)
            {
                Highlight();
            }
            else
            {
                _pendingHighlight = true;
            }
        };
    }

    public new bool Focus() => _text.Focus();

    public void LoadFile(string path)
    {
        string content = SimpleFileHandler.Read(path);
        _usesCrlf = content.Contains("\r\n");

        LanguageSupport? language = LanguageRegistry.ForFile(path);
        _tokens = language?.TokenPattern;
        _indentTriggers = language?.IndentTriggers ?? "{:";

        _loading = true;
        try
        {
            _text.Text = content;
            _text.ClearUndo();
        }
        finally
        {
            _loading = false;
        }

        _gutter.UpdateWidth();
        SetDirty(false);
        _pendingHighlight = _tokens is not null;
    }

    public void SaveFile(string path)
    {
        string text = _text.Text.Replace("\r\n", "\n");
        if (_usesCrlf)
        {
            text = text.Replace("\n", "\r\n");
        }
        SimpleFileHandler.Write(path, text);
        SetDirty(false);
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible && _pendingHighlight)
        {
            _pendingHighlight = false;
            Highlight();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _highlightTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    void SetDirty(bool dirty)
    {
        if (IsDirty == dirty)
        {
            return;
        }

        IsDirty = dirty;
        DirtyChanged?.Invoke(this, EventArgs.Empty);
    }

    void OnTextChanged(object? sender, EventArgs e)
    {
        _gutter.UpdateWidth();
        _gutter.Invalidate();
        if (_loading || _highlighting)
        {
            return;
        }

        SetDirty(true);
        if (_tokens is not null)
        {
            _highlightTimer.Stop();
            _highlightTimer.Start();
        }
    }

    void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyData)
        {
            case Keys.Tab:
                _text.SelectedText = "    ";
                e.SuppressKeyPress = true;
                break;

            case Keys.Enter:
                InsertNewLineWithIndent();
                e.SuppressKeyPress = true;
                break;

            case Keys.Control | Keys.V:
            case Keys.Shift | Keys.Insert:
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        _text.SelectedText = Clipboard.GetText();
                    }
                }
                catch (ExternalException) { }
                e.SuppressKeyPress = true;
                break;
        }
    }

    void InsertNewLineWithIndent()
    {
        int position = _text.SelectionStart;
        int lineStart = _text.GetFirstCharIndexFromLine(_text.GetLineFromCharIndex(position));
        string before = _text.Text.Substring(lineStart, position - lineStart);

        string indent = new(before.TakeWhile(c => c == ' ' || c == '\t').ToArray());
        string trimmed = before.TrimEnd();
        if (trimmed.Length > 0 && _indentTriggers.Contains(trimmed[^1]))
        {
            indent += "    ";
        }

        _text.SelectedText = "\n" + indent;
    }

    void Highlight()
    {
        Regex? tokens = _tokens;
        if (tokens is null || _text.TextLength == 0 || _text.TextLength > MaxHighlightLength)
        {
            return;
        }

        _highlighting = true;
        var scroll = new Point();
        SendMessage(_text.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref scroll);
        int selectionStart = _text.SelectionStart, selectionLength = _text.SelectionLength;
        SendMessage(_text.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

        try
        {
            _text.SelectAll();
            _text.SelectionColor = DefaultColor;

            foreach (Match m in tokens.Matches(_text.Text))
            {
                _text.Select(m.Index, m.Length);
                _text.SelectionColor = ColorFor(m);
            }
        }
        finally
        {
            _text.Select(selectionStart, selectionLength);
            SendMessage(_text.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ref scroll);
            SendMessage(_text.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            _text.Invalidate();
            _highlighting = false;
        }
    }

    static Color ColorFor(Match m) =>
        m.Groups["comment"].Success ? CommentColor :
        m.Groups["string"].Success ? StringColor :
        m.Groups["tag"].Success ? TagColor :
        m.Groups["variable"].Success ? VariableColor :
        m.Groups["keyword"].Success ? KeywordColor :
        NumberColor;

    sealed class LineNumberGutter : Control
    {
        readonly RichTextBox _box;

        public LineNumberGutter(RichTextBox box)
        {
            _box = box;
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(243, 243, 243);
            ForeColor = Color.FromArgb(110, 110, 110);
            Font = box.Font;
            UpdateWidth();
        }

        public void UpdateWidth()
        {
            int lines = _box.GetLineFromCharIndex(_box.TextLength) + 1;
            int digits = Math.Max(3, lines.ToString().Length);
            int width = TextRenderer.MeasureText(new string('9', digits), Font).Width + 14;
            if (Width != width)
            {
                Width = width;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            int first = _box.GetLineFromCharIndex(_box.GetCharIndexFromPosition(Point.Empty));
            int total = _box.GetLineFromCharIndex(_box.TextLength) + 1;

            for (int line = first; line < total; line++)
            {
                Point p = _box.GetPositionFromCharIndex(_box.GetFirstCharIndexFromLine(line));
                if (p.Y > Height)
                {
                    break;
                }

                TextRenderer.DrawText(
                    e.Graphics, (line + 1).ToString(), Font,
                    new Rectangle(0, p.Y, Width - 6, Font.Height), ForeColor,
                    TextFormatFlags.Right | TextFormatFlags.NoPrefix);
            }
        }
    }
}
