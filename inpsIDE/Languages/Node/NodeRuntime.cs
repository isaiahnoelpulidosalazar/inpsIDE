using System.Reflection;
using inpsNuGet;

namespace inpsIDE.Languages.Node;

public static class NodeRuntime
{
    const string ResourcePrefix = "inpsIDE.Resources.";
    static readonly Assembly Host = typeof(NodeRuntime).Assembly;

    public static string? FindEmbeddedZipName()
    {
        string? name = Host.GetManifestResourceNames().FirstOrDefault(n =>
            n.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            n.Contains("node", StringComparison.OrdinalIgnoreCase));

        if (name is null)
        {
            return null;
        }
        return name.StartsWith(ResourcePrefix, StringComparison.Ordinal) ? name[ResourcePrefix.Length..] : name;
    }

    public static bool InstallInto(string runtimeDirectory)
    {
        string? zip = FindEmbeddedZipName();
        if (zip is null)
        {
            return false;
        }

        SimpleFileHandler.ExtractEmbeddedZip(Host, zip, runtimeDirectory);
        return true;
    }
}
