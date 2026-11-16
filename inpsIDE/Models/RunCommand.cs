namespace inpsIDE.Models;

public sealed record RunCommand(
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    string Display,
    IReadOnlyDictionary<string, string>? Environment = null,
    string? Url = null,
    int OpenBrowserDelayMs = 0
);
