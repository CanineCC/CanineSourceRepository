namespace CanineSourceRepository;


public static class StringExtensions
{
  public static string ToCamelCase(this string str)
  {
    if (string.IsNullOrEmpty(str))
      return str;

    string pascalCase = str.ToPascalCase();

    return char.ToLower(pascalCase[0]) + pascalCase[1..];
  }

  public static string ToPascalCase(this string str)
  {
    if (string.IsNullOrEmpty(str))
      return str;

    string[] words = VariableRegex.WordsRegex().Split(str);

    string result = "";
    foreach (var word in words)
    {
      if (!string.IsNullOrEmpty(word))
      {
        result += char.ToUpper(word[0]) + word[1..].ToLower();
      }
    }

    return result;
  }

  public static string SanitizeVariableName(this string str)
  {
    if (string.IsNullOrEmpty(str))
      return str;

    str = VariableRegex.SanitizeVariableNameRegex().Replace(str, "");

    if (!char.IsLetter(str[0]) && str[0] != '_')
    {
      str = "_" + str;
    }

    return str;
  }

}

public static partial class VariableRegex
{
  [GeneratedRegex(@"[^a-zA-Z0-9_]")]
  public static partial Regex SanitizeVariableNameRegex();
  [GeneratedRegex(@"[^a-zA-Z0-9]+")]
  public static partial Regex WordsRegex();
}