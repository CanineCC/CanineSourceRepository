using CanineSourceRepository.BusinessProcessNotation.Engine;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code;

//TODO: Move to container / simplify task to be just code-tasks (and maybe manual-tasks) 
public class ApiInputTask_REMOVEME(string Name, string[] AccessScopes) : BpnTask(Name)
{
  [Required]
  public string[] AccessScopes { get; set; } = AccessScopes;
  public string ToCode(bool includeNamespace = true)
  {
    var records = string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    var usingAndNamespace = includeNamespace ? @$"using System; 
using static CanineSourceRepository.BusinessProcessNotation.Engine.BpnEngine;
namespace {BpnEngine.CodeNamespace};" : string.Empty;
    return @$"{usingAndNamespace}

/* 
<Name>{Name}</Name>
<Purpose>{BusinessPurpose}</Purpose>
<Goal>{BehavioralGoal}</Goal>
*/
public static class {GetTypeName()} {{
  {records}
  public static {Input} Execute({Input} input, UserContext userContext) {{
    if (!userContext.IsAuthenticated)
    {{
        throw new UnauthorizedAccessException(""User is not authenticated."");
    }}

    var requiredScopes =  new List<string> {{{string.Join(',', AccessScopes.Select(p => "\"" + p + "\""))}}};
    if (!requiredScopes.All(scope => userContext.AccessScopes.Contains(scope)))
    {{
        throw new UnauthorizedAccessException(""User does not have the required access scope."");
    }}

    return input;
  }}
}}
      ";

  }
}


