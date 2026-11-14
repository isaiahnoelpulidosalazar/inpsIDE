using System.Text.RegularExpressions;
using inpsIDE.Models;

namespace inpsIDE.Languages.Php;

public sealed class PhpLanguage : LanguageSupport
{
    public const string LanguageId = "php";
    const string ServerAddress = "127.0.0.1:8000";

    static readonly Regex Tokens = new(
        @"(?<comment>//[^\r\n]*|#(?!\[)[^\r\n]*|/\*[\s\S]*?\*/)" +
        @"|(?<string>""(?:\\[\s\S]|[^""\\])*""|'(?:\\[\s\S]|[^'\\])*')" +
        @"|(?<tag><\?php|<\?=|\?>)" +
        @"|(?<variable>\$[A-Za-z_]\w*)" +
        @"|(?<keyword>\b(?:abstract|and|array|as|break|callable|case|catch|class|clone|const|continue|declare|default|do|echo|else|elseif|empty|enddeclare|endfor|endforeach|endif|endswitch|endwhile|enum|extends|false|final|finally|fn|for|foreach|function|global|if|implements|include|include_once|instanceof|insteadof|interface|isset|list|match|namespace|new|null|or|parent|print|private|protected|public|readonly|require|require_once|return|self|static|switch|throw|trait|true|try|unset|use|var|while|xor|yield)\b)" +
        @"|(?<number>\b\d+(?:\.\d+)?\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public override string Id => LanguageId;
    public override string DisplayName => "PHP";
    public override string RuntimeFolderName => "php";
    public override IReadOnlyList<string> FileExtensions { get; } = [".php", ".phtml", ".inc"];
    public override IReadOnlyList<string> InitialFileCandidates { get; } = ["main.php", Path.Combine("public", "index.php"), "index.php"];
    public override Regex? TokenPattern => Tokens;
    public override string IndentTriggers => "{:";

    public override string NewFileContent(string filePath) => "<?php\n\n";

    public override IReadOnlyList<ProjectTemplate> Templates { get; } =
    [
        new ProjectTemplate(
            "php-console",
            "Console Application",
            "A command-line PHP script with the bundled PHP 8.5 (thread safe) runtime. Run it with F5. Type into the console to answer fgets(STDIN).",
            LanguageId,
            ProjectKind.Console,
            new Dictionary<string, string>
            {
                ["main.php"] = """
                    <?php

                    declare(strict_types=1);

                    echo 'Welcome to $PROJECT_NAME_TEXT$ (PHP ' . PHP_VERSION . ')' . PHP_EOL;

                    echo 'What is your name? ';
                    $name = trim((string) fgets(STDIN));

                    echo "Hello, {$name}!" . PHP_EOL;
                    """
            },
            EntryFile: "main.php"),

        new ProjectTemplate(
            "php-web",
            "Web Application",
            "A small website served from the 'public' folder with PHP's built-in web server (Ctrl+F5).",
            LanguageId,
            ProjectKind.Web,
            new Dictionary<string, string>
            {
                ["public/index.php"] = """
                    <?php declare(strict_types=1); ?>
                    <!DOCTYPE html>
                    <html lang="en">
                    <head>
                        <meta charset="utf-8">
                        <title>$PROJECT_NAME$</title>
                    </head>
                    <body>
                        <h1>Hello from $PROJECT_NAME$</h1>
                        <p>Running on PHP <?= PHP_VERSION ?> (<?= PHP_ZTS ? 'thread safe' : 'non thread safe' ?>).</p>
                    </body>
                    </html>
                    """
            },
            EntryFile: "public/index.php"),

        new ProjectTemplate(
            "php-empty",
            "Empty Project",
            "An empty folder with a single index.php and the bundled PHP runtime.",
            LanguageId,
            ProjectKind.Empty,
            new Dictionary<string, string>
            {
                ["index.php"] = "<?php\n\n"
            },
            EntryFile: "index.php")
    ];

    public override string? FindExecutable(string projectRoot) => FindPhpExecutable(GetRuntimePath(projectRoot));

    public static string? FindPhpExecutable(string runtimeDirectory)
    {
        if (!Directory.Exists(runtimeDirectory)) return null;

        string direct = Path.Combine(runtimeDirectory, "php.exe");
        if (File.Exists(direct)) return direct;

        // Some zips wrap everything in a top-level folder.
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
        return Directory.EnumerateFiles(runtimeDirectory, "php.exe", options).FirstOrDefault();
    }

    public override string? InstallRuntime(string projectRoot, ProjectTemplate template) =>
        PhpRuntime.InstallInto(GetRuntimePath(projectRoot))
            ? null
            : "This build of inpsIDE has no PHP zip embedded, so the project was created without a runtime.\n\n" +
              "Put the PHP zip into inpsIDE\\Resources, rebuild, and create the project again.";

    public override string MissingRuntimeMessage(string projectRoot) =>
        $"No php.exe was found in:\n{GetRuntimePath(projectRoot)}\n\n" +
        "Create the project from a PHP template (with the PHP zip embedded) or copy a PHP build into that folder.";

    public override RunCommand CreateRunFileCommand(string executable, string projectRoot, string filePath) =>
        new(executable, ["-f", filePath], projectRoot, $"php -f \"{filePath}\"");

    // Opened folders (no template) may be websites too, so only console projects are excluded.
    public override bool SupportsServer(ProjectTemplate? template) => template is not { Kind: ProjectKind.Console };

    public override RunCommand? CreateServerCommand(string executable, string projectRoot, ProjectTemplate? template)
    {
        string pub = Path.Combine(projectRoot, "public");
        string documentRoot = Directory.Exists(pub) ? pub : projectRoot;

        return new RunCommand(
            executable,
            ["-S", ServerAddress, "-t", documentRoot],
            projectRoot,
            $"php -S {ServerAddress} -t \"{documentRoot}\"",
            Url: $"http://{ServerAddress}/");
    }
}
