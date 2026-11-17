using System.Text;

namespace inpsIDE.Services;

public static class ArgumentParser
{
    public static List<string> Split(string text)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        bool started = false;

        foreach (char c in text)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                started = true;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (started)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    started = false;
                }
            }
            else
            {
                current.Append(c);
                started = true;
            }
        }

        if (started)
        {
            result.Add(current.ToString());
        }

        return result;
    }

    public static string Join(IEnumerable<string> arguments) =>
        string.Join(' ', arguments.Select(a => a.Length == 0 || a.Any(char.IsWhiteSpace) ? "\"" + a + "\"" : a));
}
