﻿using CanineSourceRepository.BusinessProcessNotation.Engine;
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code;

public class ApiInputTask(string Name, string[] AccessScopes) : BpnTask(Guid.CreateVersion7(), Name)
{
  [Required]
  public string[] AccessScopes { get; set; } = AccessScopes;
  public override string ToCode(bool includeNamespace = true)
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

