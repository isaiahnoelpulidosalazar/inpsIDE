using System.Reflection;
using inpsNuGet;

namespace inpsIDE.Languages.Php;

/// <summary>Extracts the embedded PHP zip into a project using inpsNuGet's <see cref="SimpleFileHandler"/>.</summary>
public static class PhpRuntime
{
    const string ResourcePrefix = "inpsIDE.Resources.";
    static readonly Assembly Host = typeof(PhpRuntime).Assembly;

    // Commonly needed extensions that php.ini-development ships commented out.
    static readonly string[] EnabledExtensions = ["curl", "fileinfo", "mbstring", "openssl", "pdo_sqlite", "sqlite3"];

    /// <summary>File name of the embedded PHP zip (without the namespace prefix), or null if none was embedded.</summary>
    public static string? FindEmbeddedZipName()
    {
        string? name = Host.GetManifestResourceNames().FirstOrDefault(n =>
            n.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            n.Contains("php", StringComparison.OrdinalIgnoreCase));

        if (name is null) return null;
        return name.StartsWith(ResourcePrefix, StringComparison.Ordinal) ? name[ResourcePrefix.Length..] : name;
    }

    /// <returns>false if no PHP zip is embedded in this build.</returns>
    public static bool InstallInto(string runtimeDirectory)
    {
        string? zip = FindEmbeddedZipName();
        if (zip is null) return false;

        SimpleFileHandler.ExtractEmbeddedZip(Host, zip, runtimeDirectory);
        ConfigureIni(runtimeDirectory);
        return true;
    }

    static void ConfigureIni(string runtimeDirectory)
    {
        string? exe = PhpLanguage.FindPhpExecutable(runtimeDirectory);
        if (exe is null) return;

        string dir = Path.GetDirectoryName(exe)!;
        string template = Path.Combine(dir, "php.ini-development");
        string ini = Path.Combine(dir, "php.ini");
        if (!File.Exists(template) || File.Exists(ini)) return;

        string text = SimpleFileHandler.Read(template);
        text = text.Replace(";extension_dir = \"ext\"", "extension_dir = \"ext\"");
        foreach (string ext in EnabledExtensions)
            text = text.Replace(";extension=" + ext, "extension=" + ext);

        SimpleFileHandler.Write(ini, text);
    }
}
