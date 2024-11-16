using CanineSourceRepository.BusinessProcessNotation.Engine;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code;


public class BpnTask(string name)
{
  public Guid Id { get; set; } = Guid.CreateVersion7();
  [Required] public string Name { get; set; } = name; //sanitize?
  /// <summary>
  /// Describe why this node exists from a business perspective. What is its value in the broader workflow
  /// </summary>
  /// <example>
  /// Validate that the user has a verified email address before allowing access to premium content.
  /// </example>
  [Required] public string BusinessPurpose { get; set; } = string.Empty;

  /// <summary>
  /// Focus on what behavior this node should enforce. It should emphasize the expected result or transformation without mentioning technical specifics. This will guide the LLM to create logic that fulfills the business behavior.
  /// </summary>
  /// <example>
  /// Ensure the email is verified and allow access to content.
  /// </example>
  [Required] public string BehavioralGoal { get; set; } = string.Empty;
  public string? Input { get; set; }
  public string? Output { get; set; }
  [Required] public string? Code { get; set; }
  [Required] public List<TestCase> TestCases { get; set; } = [];
  [Required] public string ServiceDependency { get; set; } = typeof(NoService).Name;
  [Required] public string NamedConfiguration { get; set; } = string.Empty;

  public TestCase[] UpsertTestCase(TestCase record)
  {
    TestCases.RemoveAll(p => p.Id == record.Id);
    TestCases.Add(record);
    return TestCases.OrderBy(p=>p.Name).ToArray();
  }

  public TestCase[] RemoveTestCase(Guid id)
  {
    TestCases.RemoveAll(p => p.Id == id);
    return TestCases.OrderBy(p => p.Name).ToArray();
  }

  public async Task<List<TestResult>> RunTests(object? serviceInjection, Assembly assembly)
  {

    var results = new List<TestResult>();
    foreach (var testcase in TestCases)
    {
      try
      {
        dynamic? result = await Execute(testcase.Input, serviceInjection, assembly);

        foreach (var assert in testcase.Asserts)
        {
          var field = result?.GetType().GetProperty(assert.Field)?.GetValue(result, null);
          var fieldAsStr = (string?)(field?.ToString());
          var expectedValueAsStr = assert.ExpectedValue;
          TestResult testResult = assert.Operation switch
          {
            AssertOperation.True => field == true
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.False => field == false
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.Equal => assert.ExpectedValue == fieldAsStr
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.NotEqual => assert.ExpectedValue != fieldAsStr
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.Empty => field == null || string.IsNullOrEmpty(fieldAsStr) == true
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.NotEmpty => field != null && string.IsNullOrEmpty(fieldAsStr) == false
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.StartWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.StartsWith(expectedValueAsStr) == true
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.DoesNotStartWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.StartsWith(expectedValueAsStr) == false
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.EndWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.EndsWith(expectedValueAsStr) == true
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.DoesNotEndWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.EndsWith(expectedValueAsStr) == false
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.Contains => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.Contains(expectedValueAsStr) == true
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.DoesNotContain => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.Contains(expectedValueAsStr) == false
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.MatchRegEx => fieldAsStr != null && expectedValueAsStr != null && Regex.IsMatch(fieldAsStr, expectedValueAsStr) == true
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.DoesNotMatchRegEx => fieldAsStr != null && expectedValueAsStr != null && Regex.IsMatch(fieldAsStr, expectedValueAsStr) == false
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.GreaterThan => field > assert.ExpectedValue//TODO: numbers?, dates?
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            AssertOperation.LessThan => field < assert.ExpectedValue//TODO: numbers?, dates?
                            ? new TestResult(testcase.Name, true)
                            : new TestResult(testcase.Name, false, "expected true"),
            _ => throw new NotImplementedException($"{assert.Operation} is not implemented"),
          };
          results.Add(testResult);
        }
      }
      catch (Exception e)
      {
        results.Add(new TestResult(testcase.Name, false, e.Message));
      }

    }
    return results;
  }
  public ServiceInjection GetServiceDependency()
  {
    return ServiceInjection.ServiceLocator(ServiceDependency, NamedConfiguration);
  }

  [Required] public ImmutableList<RecordDefinition> RecordTypes { get; set; } = [];
  [Required] public string[] ValidDatatypes
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
  
  public string RecordsAsCode
  {
    get
    {
      return string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    }
  }
  public string MethodSignatureAsCode
  {
    get
    {
      return Output == null ?
        $"public static async Task Execute({Input} input, {ServiceDependency} service)" :
        $"public static async Task<{Output}> Execute({Input} input, {ServiceDependency} service) ";
    }
  }

  public string ToCode(bool includeNamespace = true)
  {
    if (Code == null) return "";
    var records = string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    var usingAndNamespace = includeNamespace ? @$"
using System; 
using System.Threading.Tasks; 
using System.Linq; 
using CanineSourceRepository.BusinessProcessNotation.Engine;
namespace {BpnEngine.CodeNamespace};" : string.Empty;
    return @$"{usingAndNamespace}

/* 
<Name>{Name}</Name>
<Purpose>{BusinessPurpose}</Purpose>
<Goal>{BehavioralGoal}</Goal>
*/
public static class {GetTypeName()} {{
  {RecordsAsCode}
  {MethodSignatureAsCode} {{
    {Code}
  }}
}}
      ";
  }

  private int CalcCodeOffset()
  {
    if (Code == null) return 0;

    var fullCode = ToCode();
    int snippetStartIndex = fullCode.IndexOf(Code);

    if (snippetStartIndex == -1) return 0;

    string beforeSnippet = fullCode[..snippetStartIndex];
    int lineNumber = beforeSnippet.Split("\r\n").Length - 1; //, StringSplitOptions.RemoveEmptyEntries
    return lineNumber;
  }


  public (DynamicCompiler.CompileError[] errors, bool success) VerifyCode() => DynamicCompiler.VerifyCode(ToCode(), CalcCodeOffset());
  
}


/*
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
*/
public record TestCase
{
  //TODO::Role-base
  //admin: ["nameOfFeature". "nameOfAnotherFeature"] + method hasPermission
  //ex: "GetUserById" or "GetUserById:[id]" 
  //setup create role + check feature
  
  //TODO: USE SNAPSHOTTING
  //https://youtu.be/JG4zt9CnIl4
  
  [Required]
  public Guid Id { get; set; }
  [Required]
  public string Name { get; set; }
  [Required]
  public string Input { get; set; }
  [Required]
  public AssertDefinition[] Asserts { get; set; }
  //vs. expected output (ex. json)
  //variable from input, expected in output? (aka dynamiske værdier) -- f.eks. NewGuid() inde i metoden 
  //hvad med stream og byte[] ... store data?!
}
public record AssertDefinition
{
  [Required]  public string Field { get; set; }
  [Required]public AssertOperation Operation { get; set; }
  [Required]public string? ExpectedValue { get; set; } 
}

public record TestResult(string Name, bool Success, string Message = "");
public enum AssertOperation
{
  True,//bool
  False,//bool

  Equal,//ALL
  NotEqual,//ALL
  Empty,//also null     //ALL
  NotEmpty,//also null  //ALL

  StartWith,//string
  DoesNotStartWith,//string
  EndWith,//string
  DoesNotEndWith,//string
  Contains,//string
  DoesNotContain,//string
  MatchRegEx,//string
  DoesNotMatchRegEx,//string

  GreaterThan,//long, decimal, date time offset, date only, time only
  LessThan//long, decimal, date time offset, date only, time only
}
