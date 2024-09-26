namespace CanineSourceRepository.BusinessProcessNotation;

public record ApiInputBlock(string Name, string[] AccessScopes) : Bpn(Guid.CreateVersion7(), Name)
{
  public string[] AccessScopes { get; init; } = AccessScopes;
  public override string ToCode(bool includeNamespace = true)
  {
    var records = string.Join("\r\n", RecordTypes.Select(p => p.ToCode()));
    var usingAndNamespace = includeNamespace ? @$"using System;
namespace {BpnFeature.CodeNamespace};" : string.Empty;
    return @$"{usingAndNamespace}

/* 
Name: {Name}
-----------------------------------
{Description}
*/
public static class {GetTypeName()} {{
  {records}
  public static {Input} Execute({Input} input, UserContext userContext) {{
    if (!userContext.IsAuthenticated)
    {{
        throw new UnauthorizedAccessException(""User is not authenticated."");
    }}

    var requiredScopes =  new List<string> {{{string.Join(',', AccessScopes.Select(p=> "\""+p+ "\""))}}};
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


