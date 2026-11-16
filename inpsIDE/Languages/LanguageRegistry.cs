using inpsIDE.Languages.Php;
using inpsIDE.Languages.Python;

namespace inpsIDE.Languages;

public static class LanguageRegistry
{
    public static IReadOnlyList<LanguageSupport> All { get; } =
    [
        new PhpLanguage(),
        new PythonLanguage()
    ];
    public static LanguageSupport? Find(string? id) => id is null ? null : All.FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));
    public static LanguageSupport? ForFile(string path) => All.FirstOrDefault(l => l.Handles(path));
    public static LanguageSupport? Detect(string projectRoot) => All.FirstOrDefault(l => l.FindExecutable(projectRoot) is not null);
    public static string DescribeAll()
    {
        string[] names = All.Select(l => l.DisplayName).ToArray();
        return names.Length switch
        {
            0 => "",
            1 => names[0],
            _ => string.Join(", ", names[..^1]) + " and " + names[^1]
        };
    }
}
