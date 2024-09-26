using static CanineSourceRepository.BusinessProcessNotation.Bpn;

namespace CanineSourceRepository.BusinessProcessNotation.Snippets;

public class AutoConstructorGenerator
{
  public static string GenerateMapping(RecordDefinition input, RecordDefinition output, RecordDefinition[] customDefinitions, string inputAlias = "input", int tabs = 0)
  {
    var code = $"new {output.Name}(\n";

    bool isFirst = true;
    foreach (var outputField in output.Fields)
    {
      var inputField = input.Fields.FirstOrDefault(f => string.Equals(NormalizeName(f.Name), NormalizeName(outputField.Name), StringComparison.OrdinalIgnoreCase));

      if (!isFirst)
        code += ",\n"; // Add a comma between assignments
      isFirst = false;
      code += $"{new string('\t', tabs + 1)}{outputField.Name}: ";

      if (inputField != null)
      {
        if (inputField.Type == outputField.Type)
        {
          code += $"{inputAlias}.{inputField.Name}";
        }
        else if (inputField.IsCollection && outputField.IsCollection)
        {
          // Handle list types like ImmutableList<T>
          code += HandleListMapping(inputAlias, inputField.Type, outputField.Type, inputField.Name, customDefinitions, tabs);
        }
        else if (IsCustomRecordDefinition(inputField.Type, customDefinitions, out var customInputDefinition) &&
                 IsCustomRecordDefinition(outputField.Type, customDefinitions, out var customOutputDefinition))
        {
          code += $"{inputAlias}.{inputField.Name}.Select(item => {GenerateMapping(customInputDefinition!, customOutputDefinition!, customDefinitions, "item", tabs + 1)}).ToImmutableList()";
        }
        else
        {
          // Handle scalar type conversions
          code += HandleTypeConversion(inputAlias, inputField.Type, outputField.Type, inputField.Name);
        }
      }
      else
      {
        code += HandleDefaultType(outputField.Type);
      }
    }

    code += $"\n{new string('\t', tabs)})";
    return code;
  }

  private static string NormalizeName(string name)
  {
    return name.Replace("_", "").ToLowerInvariant();
  }

  private static bool IsCustomRecordDefinition(string type, RecordDefinition[] customDefinitions, out RecordDefinition? customDefinition)
  {
    customDefinition = customDefinitions.FirstOrDefault(def => def.Name == type);
    return customDefinition != null;
  }

  private static string HandleListMapping(string inputName, string inputListType, string outputListType, string inputFieldName, RecordDefinition[] customDefinitions, int tabs)
  {
    if (inputListType == outputListType)
    {
      return $"{inputName}.{inputFieldName}";
    }
    else if (IsCustomRecordDefinition(inputListType, customDefinitions, out var customInputDefinition) &&
             IsCustomRecordDefinition(outputListType, customDefinitions, out var customOutputDefinition))
    {
      // Handle list of complex RecordDefinitions
      return $"{inputName}.{inputFieldName}.Select(item => {GenerateMapping(customInputDefinition!, customOutputDefinition!, customDefinitions, "item", tabs + 1)}).ToImmutableList()";
    }
    else
    {
      // Handle conversion between lists of scalar types
      return $"{inputName}.{inputFieldName}.Select(item => {HandleTypeConversion(inputName, inputListType, outputListType, "item")}).ToImmutableList()";
    }
  }

  private static string HandleTypeConversion(string inputName, string inputType, string outputType, string inputFieldName)
  {
    string conversionCode = outputType switch
    {
      "string" => $"Convert.ToString({inputName}.{inputFieldName}, CultureInfo.InvariantCulture)",
      "long" => inputType == "string"
                  ? $"Convert.ToInt64({inputName}.{inputFieldName}, CultureInfo.InvariantCulture)"
                  : $"Convert.ToInt64({inputName}.{inputFieldName})",
      "decimal" => inputType == "string"
                  ? $"Convert.ToDecimal({inputName}.{inputFieldName}, CultureInfo.InvariantCulture)"
                  : $"Convert.ToDecimal({inputName}.{inputFieldName})",
      "DateTimeOffset" => inputType == "string"
                  ? $"DateTimeOffset.Parse({inputName}.{inputFieldName}, CultureInfo.InvariantCulture)"
                  : $"(DateTimeOffset){inputName}.{inputFieldName}",
      "DateOnly" => inputType == "string"
                  ? $"DateOnly.Parse({inputName}.{inputFieldName}, CultureInfo.InvariantCulture)"
                  : $"(DateOnly){inputName}.{inputFieldName}",
      "TimeOnly" => inputType == "string"
                  ? $"TimeOnly.Parse({inputName}.{inputFieldName}, CultureInfo.InvariantCulture)"
                  : $"(TimeOnly){inputName}.{inputFieldName}",
      "Guid" => inputType == "string"
                  ? $"Guid.Parse({inputName}.{inputFieldName})"
                  : $"(Guid){inputName}.{inputFieldName}",
      "bool" => inputType == "string"
                  ? $"bool.Parse({inputName}.{inputFieldName})"
                  : $"(bool){inputName}.{inputFieldName}",
      _ => throw new NotSupportedException($"Conversion from {inputType} to {outputType} is not supported."),
    };
    return conversionCode;
  }
  private static string HandleDefaultType(string outputType)
  {
    string conversionCode = outputType switch
    {
      "string" => "String.Empty",
      "long" => "0L",
      "decimal" => "0M",
      "DateTimeOffset" => "DateTime.UtcNow",
      "DateOnly" => "DateTime.UtcNow.Date",
      "TimeOnly" => "DateTime.UtcNow.Time",
      "Guid" => "Guid.CreateVersion7()",
      "bool" => "false",
      _ => throw new NotSupportedException($"No default value for {outputType}."),
    };
    return conversionCode;
  }

}
