using CanineSourceRepository.BusinessProcessNotation.Engine;
using static CanineSourceRepository.DynamicCompiler;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;

public record TestCase(string Name, dynamic Input, params AssertDefinition[] Asserts);
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
public record AssertDefinition(string Field, AssertOperation Operation, object? ExpectedValue = null);

public class CodeTask(string Name) : BpnTask(Guid.CreateVersion7(), Name)
{
  public string? Code { get; set; }
  public ImmutableDictionary<string, TestCase> TestCases { get; set; } = ImmutableDictionary<string, TestCase>.Empty;

  public TestCase[] AddTestCase(TestCase record)
  {
    if (TestCases.ContainsKey(record.Name))
    {
      TestCases = TestCases.SetItem(record.Name, record);
    }
    else
    {
      TestCases = TestCases.Add(record.Name, record);
    }


    return TestCases.Values.ToArray();
  }

  public TestCase[] RemoveTestCase(TestCase record)
  {
    if (TestCases.ContainsKey(record.Name))
    {
      TestCases = TestCases.Remove(record.Name);
    }

    return TestCases.Values.ToArray();
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

  public async Task<List<TestResult>> RunTests(object? serviceInjection, Assembly assembly)
  {
    
    var results = new List<TestResult>();
    foreach (var testcase in TestCases)
    {
      try
      {
        dynamic? result = await Execute(testcase.Value.Input, serviceInjection, assembly);

        foreach (var assert in testcase.Value.Asserts)
        {
          //var field = result[assert.Field];
          var field = result?.GetType().GetProperty(assert.Field)?.GetValue(result, null);

          var fieldAsStr = (string?)(field?.ToString());
          var expectedValueAsStr = assert.ExpectedValue?.ToString();
          TestResult testResult = assert.Operation switch
          {
            AssertOperation.True => field == true
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.False => field == false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.Equal => assert.ExpectedValue?.Equals(field) == true ?? false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.NotEqual => assert.ExpectedValue?.Equals(field) == false ?? false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.Empty => field == null || string.IsNullOrEmpty(field) == true
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.NotEmpty => field != null && string.IsNullOrEmpty(fieldAsStr) == false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.StartWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.StartsWith(expectedValueAsStr) == true
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.DoesNotStartWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.StartsWith(expectedValueAsStr) == false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.EndWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.EndsWith(expectedValueAsStr) == true
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.DoesNotEndWith => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.EndsWith(expectedValueAsStr) == false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.Contains => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.Contains(expectedValueAsStr) == true
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.DoesNotContain => fieldAsStr != null && expectedValueAsStr != null && fieldAsStr.Contains(expectedValueAsStr) == false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.MatchRegEx => fieldAsStr != null && expectedValueAsStr != null && Regex.IsMatch(fieldAsStr, expectedValueAsStr) == true
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.DoesNotMatchRegEx => fieldAsStr != null && expectedValueAsStr != null && Regex.IsMatch(fieldAsStr, expectedValueAsStr) == false
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.GreaterThan => field > assert.ExpectedValue
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            AssertOperation.LessThan => field < assert.ExpectedValue
                            ? new TestResult(testcase.Value.Name, true)
                            : new TestResult(testcase.Value.Name, false, "expected true"),
            _ => throw new NotImplementedException($"{assert.Operation} is not implemented"),
          };
          results.Add(testResult);
        }
      }
      catch (Exception e)
      {
        results.Add(new TestResult(testcase.Value.Name, false, e.Message));
      }

    }
    return results;
  }



}


