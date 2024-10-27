using CanineSourceRepository.BusinessProcessNotation.Engine;
using System.ComponentModel.DataAnnotations;
using static CanineSourceRepository.DynamicCompiler;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;


public record TestCase
{
  [Required]
  public Guid Id { get; set; }
  [Required]
  public string Name { get; set; }
  [Required]
  public string Input { get; set; }
  [Required]
  public AssertDefinition[] Asserts { get; set; }
}
public record AssertDefinition
{
  public string Field { get; set; }
  public AssertOperation Operation { get; set; }
  public string? ExpectedValue { get; set; } 
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

public class CodeTask(string Name) : BpnTask(Guid.CreateVersion7(), Name)
{
  [Required]
  public string? Code { get; set; }





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

  public override string ToCode(bool includeNamespace = true)
  {
    if (Code == null) return "";
    var records = string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    var usingAndNamespace = includeNamespace ? @$"using System; using System.Threading.Tasks; using System.Linq; using CanineSourceRepository.BusinessProcessNotation.Engine;
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


  public (CompileError[] errors, bool success) VerifyCode() => DynamicCompiler.VerifyCode(ToCode(), CalcCodeOffset());




}


