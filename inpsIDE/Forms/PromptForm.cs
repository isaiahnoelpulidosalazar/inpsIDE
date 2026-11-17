namespace inpsIDE.Forms;

public sealed class PromptForm : Form
{
    readonly TextBox _input = new() { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

    PromptForm(string title, string prompt, string initial)
    {
        Text = title;
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 120);

        var label = new Label { Text = prompt, AutoSize = true, Location = new Point(12, 12) };
        _input.Location = new Point(12, 36);
        _input.Width = 396;
        _input.Text = initial;

        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(90, 30), Location = new Point(222, 76) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Size = new Size(90, 30), Location = new Point(318, 76) };

        Controls.AddRange(new Control[] { label, _input, ok, cancel });
        AcceptButton = ok;
        CancelButton = cancel;
        Shown += (_, _) =>
        {
            _input.Focus();
            _input.SelectAll();
        };
    }

    public static string? Ask(IWin32Window owner, string title, string prompt, string initial = "")
    {
        using var form = new PromptForm(title, prompt, initial);
        return form.ShowDialog(owner) == DialogResult.OK ? form._input.Text.Trim() : null;
    }
}
