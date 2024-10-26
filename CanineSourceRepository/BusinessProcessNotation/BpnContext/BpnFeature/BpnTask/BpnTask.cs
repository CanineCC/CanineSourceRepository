using CanineSourceRepository.BusinessProcessNotation.Engine;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;


public abstract class BpnTask(Guid Id, string Name)
{
  //public enum ServiceInjection
  //{
  //  None,
  //  Sql,           // PostgreSql, (settings?)
  //  EventSourcing, // Marten (settings?)
  //  Smtp,          // Send email (settings?) --> maybe get-pop3, get-imap, send-imap, send-smtp
  //  Pop3,          // Get email
  //  WebApi,        // WebApi (settings?, including selecting THE http-verb) -> force to one endpoint?
  //  Ado,           // generic sql client for legacy integration
  //  //Global settings? (multiple named instances?!)
  //}

  [Required]
  public Guid Id { get; set; } = Id;
  [Required]
  public string Name { get; set; } = Name; //sanitize?

  /// <summary>
  /// Describe why this node exists from a business perspective. What is its value in the broader workflow
  /// </summary>
  /// <example>
  /// Validate that the user has a verified email address before allowing access to premium content.
  /// </example>
  [Required]
  public string BusinessPurpose { get; set; } = string.Empty;

  /// <summary>
  /// Focus on what behavior this node should enforce. It should emphasize the expected result or transformation without mentioning technical specifics. This will guide the LLM to create logic that fulfills the business behavior.
  /// </summary>
  /// <example>
  /// Ensure the email is verified and allow access to content.
  /// </example>
  [Required]
  public string BehavioralGoal { get; set; } = string.Empty;

  public string? Input { get; set; }
  public string? Output { get; set; }
  [Required]
  public string ServiceDependency { get; set; } = typeof(NoService).Name;
  [Required]
  public string NamedConfiguration { get; set; } = string.Empty;
  public ServiceInjection GetServiceDependency()
  {
    return ServiceInjection.ServiceLocator(ServiceDependency, NamedConfiguration);
  }

  public ImmutableList<RecordDefinition> RecordTypes { get; set; } = [];
  public string[] ValidDatatypes
  {
    get
    {
      return RecordTypes.Select(p => p.Name).ToArray()
           .Concat(["string", "bool", "long", "decimal", "DateTimeOffset", "DateOnly", "TimeOnly", "Guid", "byte[]"])
           .ToArray();
    }
  }
  public Type GetCompiledType(Assembly assembly)
  {
    var className = BpnEngine.CodeNamespace + "." + GetTypeName();
    var res = assembly.GetType(className + "+" + Input) ?? throw new InvalidOperationException($"The inputType '{Input}' does not exist in {Name}'s definition (Node.Id:{Id})");

    return res ?? throw new Exception($"The type '{className + "+" + Input}' was not found in the assembly, check code generation for the type.");
  }
  public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(ToCode());
  public abstract string ToCode(bool includeNamespace = true);
  public Task<dynamic?> Execute(dynamic input, object? serviceInjection, Assembly assembly) => Execute(JsonSerializer.Serialize(input), serviceInjection, assembly);
  public Task<dynamic?> Execute(string inputJson, object? serviceInjection, Assembly assembly)
  {
    return DynamicCompiler.ExecuteUserMethod(
    assembly,
    BpnEngine.CodeNamespace + "." + GetTypeName(),
    "Execute",
    GetCompiledType(assembly),
    inputJson,
    serviceInjection == null ? [] : [serviceInjection]);
  }

  public (bool IsOk, List<string> MissingFields) VerifyInputData(dynamic input, Assembly assembly) => VerifyInputData(JsonSerializer.Serialize(input), assembly);
  public (bool IsOk, List<string> MissingFields) VerifyInputData(string inputJson, Assembly assembly)
  {
    var missingFields = new List<string>();

    if (Input != null)
    {
      var definition = RecordTypes.First(p => p.Name == Input);
      var inputType = GetCompiledType(assembly);
      var instance = DynamicCompiler.GetInputDataInstance(inputType, inputJson);

      foreach (var field in definition.Fields)
      {
        var propertyInfo = inputType.GetProperty(field.Name) ?? throw new InvalidOperationException($"The field '{field.Name}' does not exist in the expected input type.");
        var value = propertyInfo.GetValue(instance);

        if (value == null)
        {
          missingFields.Add(field.Name);
        }
      }
    }
    return (missingFields.Count == 0, missingFields);
  }
  public BpnTask AddRecordType(RecordDefinition record)
  {
    record.Name = record.Name.SanitizeVariableName().ToPascalCase();
    foreach (var item in record.Fields)
    {
      item.Name = item.Name.SanitizeVariableName().ToPascalCase();
    }

    var records = RecordTypes.RemoveAll(p => p.Name == record.Name);//update instead?
    records = records.Add(record);
    this.RecordTypes = records; 

    return this;
  }

  public BpnTask RemoveRecordType(RecordDefinition record)
  {
    var name = record.Name.SanitizeVariableName().ToPascalCase();
    var records = RecordTypes.RemoveAll(p => p.Name == record.Name);//update instead?
    this.RecordTypes = records;

    return this;
  }

  public class DataDefinition(string Name, string Type, bool IsCollection = false, bool IsMandatory = true)
  {
    public string Name { get; set; } = Name;
    public string Type { get; set; } = Type;
    public bool IsCollection { get; set; } = IsCollection;
    public bool IsMandatory { get; set; } = IsMandatory;
  }
  public class RecordDefinition(string Name, params DataDefinition[] Fields)
  {
    [Required]
    public string Name { get; set; } = Name;
    [Required]
    public DataDefinition[] Fields { get; set; } = Fields;

    public string ToCode()
    {
      var fields = string.Join(",", Fields.Select(p =>
        p.IsCollection ? $"ImmutableList<{p.Type}{(p.IsMandatory ? "" : "?")}> {p.Name}"
        : $"{p.Type}{(p.IsMandatory ? "" : "?")} {p.Name}"
        ));
      return $"public record {Name} ({fields});";
    }
  }

  internal string GetTypeName()
  {
    return GetType().Name + "_" + Id.ToString("N");
  }
}


public class BpnConverter : JsonConverter<BpnTask>
{

  public override BpnTask Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    using var jsonDoc = JsonDocument.ParseValue(ref reader);
    var jsonObject = jsonDoc.RootElement;

    // Assuming there is a "Type" property to differentiate between Bpn types
    var typeProperty = jsonObject.GetProperty("Type").GetString();

    return typeProperty switch
    {
      "CodeTask" => JsonSerializer.Deserialize<CodeTask>(jsonObject.GetRawText(), options)!,
      "ApiInputTask" => JsonSerializer.Deserialize<ApiInputTask>(jsonObject.GetRawText(), options)!,
      _ => throw new JsonException($"Unknown Bpn type: {typeProperty}")
    };
  }

  public override void Write(Utf8JsonWriter writer, BpnTask value, JsonSerializerOptions options)
  {
    // Determine the actual type of the object
    var type = value.GetType();
    var typeName = type.Name;

    // Write start of the object
    writer.WriteStartObject();

    // Write the type information
    writer.WriteString("Type", typeName);

    // Serialize the properties of the object manually
    var typeProperties = type.GetProperties();
    foreach (var property in typeProperties)
    {
      var propertyValue = property.GetValue(value);
      if (propertyValue != null)
      {
        writer.WritePropertyName(property.Name);
        JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
      }
    }

    // Write end of the object
    writer.WriteEndObject();
  }
}