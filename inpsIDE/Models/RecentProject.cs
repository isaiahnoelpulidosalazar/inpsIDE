namespace inpsIDE.Models;

public sealed record RecentProject(string Name, string Path, DateTime LastOpenedUtc);
