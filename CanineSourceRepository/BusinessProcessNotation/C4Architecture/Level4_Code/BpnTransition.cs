using CanineSourceRepository.BusinessProcessNotation.Engine;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code;
public record MapField(string From, string To);/*consider build-in methods, ex. Today, ABS, MIN (of two values, or within a list), MAX, ANY(in list) */

public record BpnTransition(
    Guid FromBPN,
    Guid ToBPN,
    string Name,
    string ConditionExpression,
    params MapField[] Mappings
)
{
  private string GetTypeName() => $"Transition_{FromBPN:N}_{ToBPN:N}";
  public string ToCode(bool includeNamespace = true)
  {
    var usingAndNamespace = includeNamespace ? @$"using System;
namespace {BpnEngine.CodeNamespace};" : string.Empty;
    return @$"{usingAndNamespace}

public static class {GetTypeName()} {{
  public static bool Execute(dynamic input) {{
    return {ConditionExpression};
  }}
}}
      ";
  }
  public bool ConditionIsMeet(dynamic inputObject, Assembly assembly)
  {
    string typeName = BpnEngine.CodeNamespace + "." + GetTypeName();

    var type = assembly.GetType(typeName) ?? throw new Exception($"The type '{typeName}' was not found in the assembly, check code generation for the type.");
    var method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public) ?? throw new Exception($"Method 'Execute' not found in type '{typeName}'."); ;

    var res = method.Invoke(null, [inputObject]);

    return res != null && (bool)res;
  }



  public object? MapObject(object fromObj, Type toType)
  {
    var constructor = toType.GetConstructors().FirstOrDefault() ?? throw new InvalidOperationException("The target type does not have a valid constructor.");
    var parameters = constructor.GetParameters();
    object?[] parameterValues = new object[parameters.Length];

    for (int i = 0; i < parameters.Length; i++)
    {
      var param = parameters[i];
      if (param == null || param.Name == null) continue;

      var fromProp = fromObj.GetType().GetProperty(param.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

      if (fromProp != null)
      {
        var value = fromProp.GetValue(fromObj);
        parameterValues[i] = value;
      }
    }

    // Create an instance of the target type using the constructor and parameter values
    return Activator.CreateInstance(toType, parameterValues);
  }
}