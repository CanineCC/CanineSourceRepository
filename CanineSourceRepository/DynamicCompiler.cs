using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace CanineSourceRepository;

public class DynamicCompiler
{
  public record CompileError(string ErrorMessage, int LineNumber, int ColumnNumber);

  private static readonly string[] forbiddenTypes = [
      "System.IO",
      "System.Diagnostics",
      "System.Reflection",
      "System.Net",
      "System.Security",
      "System.Runtime.InteropServices",
      "System.Runtime.CompilerServices",
      "System.Runtime.Serialization"
    ];

  public static (CompileError[] errors, byte[] assembly) CompileCode(string code, int codeOffset = 0)
  {
    var syntaxTree = CSharpSyntaxTree.ParseText(code);

    var root = syntaxTree.GetRoot();
    var invocations = root.DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .Where(invocation => forbiddenTypes.Any(type => invocation.ToString().Contains(type)));

    foreach (var invocation in invocations)
    {
      throw new InvalidOperationException($"Forbidden API usage: {invocation}");
    }
    string netCoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    var executingAssemblyPath = Assembly.GetExecutingAssembly().Location;

    var compilation = CSharpCompilation.Create(
        "BusinessProcessNotationAssembly",
        [syntaxTree],
        [MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "mscorlib.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Runtime.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Private.CoreLib.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Console.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Linq.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Linq.Expressions.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Dynamic.Runtime.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Threading.dll")),
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "System.Threading.Tasks.dll")), 
             MetadataReference.CreateFromFile(Path.Combine(netCoreDir, "Microsoft.CSharp.dll")),
             MetadataReference.CreateFromFile(executingAssemblyPath) ,
          // Add more references as needed
        ],
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        .WithNullableContextOptions(NullableContextOptions.Enable)
    );

    using var ms = new MemoryStream();
    EmitResult result = compilation.Emit(ms);

    if (!result.Success)
    {// Handle compilation errors
      var errors = result.Diagnostics
        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
        .Select(diagnostic => new CompileError(
          ErrorMessage : diagnostic.GetMessage(),
          LineNumber : diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1 - codeOffset,  // Line number is zero-based
          ColumnNumber : diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1  // Column number is zero-based
      )).ToArray();

      
      return (errors, []);
    }

    ms.Seek(0, SeekOrigin.Begin);
    return ([], ms.ToArray());
  }

  public static Assembly PrecompileCode(string code)
  {
    return LoadAssembly(CompileCode(code).assembly);
  }

  public static void ExecuteCompiledCode(Assembly assembly, string typeName, string methodName)
  {
    var type = assembly.GetType(typeName) ?? throw new Exception($"The type '{typeName}' was not found in the assembly, check code generation for the type.");
    var method = type.GetMethod(methodName) ?? throw new Exception($"Method '{methodName}' not found in type '{typeName}'.");
    var instance = Activator.CreateInstance(type) ?? throw new Exception($"Instance of '{typeName}' is null - maybe missing a default constructor?.");
    method.Invoke(instance, null);
  }

  public static object? GetInputDataInstance(Type inputType, string inputJson)
  {
    return DeserializeJsonToType(inputJson, inputType);
  }
  public static async Task<object?> ExecuteUserMethod(Assembly assembly, string typeName, string methodName, Type inputType, string inputJson, object[] parameters)
  {
    var type = assembly.GetType(typeName) ?? throw new Exception($"The type '{typeName}' was not found in the assembly, check code generation for the type.");
    var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public) ?? throw new Exception($"Method '{methodName}' not found in type '{typeName}'.");

    object? result;

    if (inputType != null)
    {
      var inputInstance = GetInputDataInstance(inputType, inputJson) ?? throw new Exception($"Unable to create instance of '{typeName}+{inputType}' with '{inputJson}'.");
      object[] dynamicParameters = [inputInstance];
      result = method.Invoke(null, [.. dynamicParameters, .. parameters]);
    }
    else
    {
      result = method.Invoke(null, parameters);
    }

    // Check if the result is a Task
    if (result is Task task)
    {
      // Await the task
      await task;

      // If the task is Task<T>, get the result using reflection
      var taskType = task.GetType();
      if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
      {
        var resultProperty = taskType.GetProperty("Result");
        return resultProperty?.GetValue(task); // Return the value from Task<T>
      }

      return null; // If it's a non-generic Task, just return null
    }

    return result; // Return the result if it's a synchronous method
  }
  /*
  public static object? ExecuteUserMethod(Assembly assembly, string typeName, string methodName, Type inputType, string inputJson, object[] parameters)
  {
    var type = assembly.GetType(typeName) ?? throw new Exception($"The type '{typeName}' was not found in the assembly, check code generation for the type.");
    var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public) ?? throw new Exception($"Method '{methodName}' not found in type '{typeName}'."); ;

    if (inputType != null)
    {
      var inputInstance = GetInputDataInstance(assembly, inputType, inputJson) ?? throw new Exception($"Unable to create instance of '{typeName}+{inputType}' with '{inputJson}'.");
      object[] dynamicParameters = [inputInstance];
      return method.Invoke(null, [.. dynamicParameters, .. parameters]); // Capture the returned data
    }

    return method.Invoke(null, parameters); // Capture the returned data
  }*/

  private static readonly JsonSerializerOptions jsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
  };

  public static object? DeserializeJsonToType(string json, Type targetType)
  {
    return JsonSerializer.Deserialize(json, targetType, jsonOptions);
  }

  public static Assembly LoadAssembly(byte[] compiledAssembly)
  {
    return Assembly.Load(compiledAssembly);
  }

  public static (CompileError[] errors, bool success) VerifyCode(string code, int codeOffset = 0)
  {
    try
    {
      var (errors, assembly) = CompileCode(code, codeOffset);
      return (errors, errors.Length == 0);
    }
    catch (Exception ex)
    {
      return ([new CompileError(ex.Message, 0, 0)], false);
    }
  }
}
