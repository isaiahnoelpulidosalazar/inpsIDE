using System.Text.Json;
using inpsNuGet;
using inpsIDE.Models;

namespace inpsIDE.Services;

/// <summary>Persists the recent project list in %AppData%\inpsIDE\recent.json.</summary>
public static class RecentProjectsStore
{
    const int MaxItems = 15;

    static readonly string StorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "inpsIDE", "recent.json");

    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static List<RecentProject> Load()
    {
        try
        {
            if (!File.Exists(StorePath)) return [];
            return JsonSerializer.Deserialize<List<RecentProject>>(SimpleFileHandler.Read(StorePath)) ?? [];
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    public static void Add(IdeProject project)
    {
        List<RecentProject> items = Load();
        items.RemoveAll(r => r.Path.Equals(project.RootPath, StringComparison.OrdinalIgnoreCase));
        items.Insert(0, new RecentProject(project.Name, project.RootPath, DateTime.UtcNow));
        if (items.Count > MaxItems) items.RemoveRange(MaxItems, items.Count - MaxItems);
        Save(items);
    }

    public static void Remove(string path)
    {
        List<RecentProject> items = Load();
        if (items.RemoveAll(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)) > 0)
            Save(items);
    }

    static void Save(List<RecentProject> items)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            SimpleFileHandler.Write(StorePath, JsonSerializer.Serialize(items, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The recent list is a convenience; never fail the user's action because of it.
        }
    }
}
