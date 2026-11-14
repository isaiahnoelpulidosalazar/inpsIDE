namespace inpsIDE.Services;

public static class FileUtil
{
    /// <summary>Heuristic: a file is binary if its first 8 KB contain a NUL byte.</summary>
    public static bool IsBinary(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] buffer = new byte[8192];
        int read = stream.Read(buffer, 0, buffer.Length);
        for (int i = 0; i < read; i++)
            if (buffer[i] == 0) return true;
        return false;
    }

    public static bool IsSameOrUnder(string path, string root)
    {
        root = Path.TrimEndingDirectorySeparator(root);
        return path.Equals(root, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
