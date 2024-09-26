using System.Collections.Immutable;
using System.Reflection;
using System.Text.RegularExpressions;
using static CanineSourceRepository.DynamicCompiler;

namespace CanineSourceRepository.BusinessProcessNotation;

//TODO: (requires that we have the command=>event (verified by aggregate)=>projection in place, so we can target the projection)
//TODO: Let projection be more like a view, based on a specific aggregate, having a version number from that aggregate on each line
//TOOD: Let commands include the version number of the target aggregate(s), in order to be able to enforce idempotency on command execution
//i.e. always check that the command version number is equal to the aggregate that it want to affect in order to allow it to do so.
//creating new aggregates might be complex, as "new guid" solution will not be idempotent
/*
public record ApiOutputBlock(string Name, string Projection, string[] AccessScopes) : Bpn(Guid.NewGuid(), Name)
{
  //public string? Output { get; init; }
  public string[] AccessScopes { get; init; } = AccessScopes;
  //in diagram? or defined as annotation to projections?
  public string Projection { get; init; } = Projection;

  //Testcases / integration test?
}
*/
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

public record CodeBlock(string Name) : Bpn(Guid.CreateVersion7(), Name)
{
  public string? Output { get; init; }
  public string? Code { get; init; }
  public ImmutableDictionary<string, TestCase> TestCases { get; private set; } = ImmutableDictionary<string, TestCase>.Empty;

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


  public string RecordsAsCode { get {
      return string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    }
  }
  public string MethodSignatureAsCode {  get {
      return Output == null ? $"public static async Task Execute({Input} input)" : $"public static async Task<{Output}> Execute({Input} input) ";
    }
  }

  public override string ToCode(bool includeNamespace = true)
  {
    if (Code == null) return "";

    var records = string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    var usingAndNamespace = includeNamespace ? @$"using System; using System.Threading.Tasks; using System.Linq;
namespace {BpnFeature.CodeNamespace};" : string.Empty;
    return @$"{usingAndNamespace}

/* 
Name: {Name}
-----------------------------------
{Description}
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
    //TODO: inject "test implementation" of the service-to-be-injected!
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


