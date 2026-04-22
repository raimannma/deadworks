using System.Text;

namespace DeadworksManaged.Commands;

/// <summary>Whitespace-separated tokens; double-quoted segments group, with <c>\"</c> and <c>\\</c> escapes.</summary>
internal static class CommandTokenizer
{
    public static string[] Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (inQuotes)
            {
                if (c == '\\' && i + 1 < input.Length)
                {
                    char next = input[i + 1];
                    if (next == '"' || next == '\\')
                    {
                        current.Append(next);
                        i++;
                        continue;
                    }
                }
                if (c == '"')
                {
                    inQuotes = false;
                    continue;
                }
                current.Append(c);
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }
                current.Append(c);
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens.ToArray();
    }
}
