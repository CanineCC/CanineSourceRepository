using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature;
public record StackTrace(Guid CorrelationId, StackElement[] Trace, string UserInformation, DateTimeOffset Timestamp);
public record StackElement(Guid BpnId, string Name, long Version, DateTimeOffset Timestamp, TimeSpan Duration, string DataInput);

public record UserContext(string UserId, string UserName, string[] AccessScopes, string IpAddress, bool IsAuthenticated, string AuthenticationType, DateTime? TokenExpiry);

public class BpnFeature
{
  public static readonly string CodeNamespace = "CanineSourceRepository";
  public enum Environment { Development, Testing, Staging, Production };
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;

  /// <summary>
  /// Describe the business purpose of the entire feature in business terms, not technical ones.
  /// </summary>
  /// <example>
  /// Enable users to register, validate their email, and gain access to premium content.
  /// </example>
  public string Objective { get; init; } = string.Empty;

  /// <summary>
  /// A high-level description of the business process from start to finish. 
  /// </summary>
  /// <example>
  /// The user enters their registration details, verifies their email, and is granted access to restricted areas.
  /// </example>
  public string FlowOverview { get; init; } = string.Empty;

  public long Version { get; init; } = 0;
  public DateTimeOffset Timestamp { get; init; }
  public string User { get; init; } = string.Empty;
  public ImmutableList<BpnTask> Tasks { get; init; } = [];
  public ImmutableList<Transition> Transitions { get; init; } = [];
  public ImmutableList<Environment> TargetEnvironments { get; init; } = [];

  // Parameterless constructor required for model binding
  public BpnFeature() { }

  public BpnFeature(
      Guid id,
      string name,
      long version,
      DateTimeOffset timestamp,
      string user,
      ImmutableList<BpnTask> nodes,
      ImmutableList<Transition> connections,
      ImmutableList<Environment> targetEnvironments
  )
  {
    Id = id;
    Name = name;
    Version = version;
    Timestamp = timestamp;
    User = user;
    Tasks = nodes;
    Transitions = connections;
    TargetEnvironments = targetEnvironments;
  }

  public BpnFeature NewRevision(string user)
  {
    long revision = 0;
    if (BpnFeatureRepository.Exists(Id))
      revision = BpnFeatureRepository.Load(Id).Version;

    var nextVersion = revision + 1;
    var newVersion = new BpnFeature(Id, Name, nextVersion, DateTime.UtcNow, user, Tasks, Transitions, TargetEnvironments);

    return newVersion;
  }
  public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(ToCode());

  public string ToCode()
  {
    var sb = new StringBuilder();

    sb.AppendLine("using System;");
    sb.AppendLine("using System.Threading.Tasks;");
    sb.AppendLine("using System.Linq;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.Context;");
    sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.Context.Feature;");
    sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;");
    sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.Engine;");
    sb.AppendLine($"namespace {CodeNamespace};");
    sb.AppendLine();
    //    sb.AppendLine("public record UserContext(string UserId, string UserName, string[] AccessScopes, string IpAddress, bool IsAuthenticated, string AuthenticationType, DateTime? TokenExpiry);");
    foreach (var node in Tasks)
    {
      switch (node)
      {
        case CodeTask codeBlock:
          sb.Append(codeBlock.ToCode(false));
          sb.Append("\n\r");
          break;
        case ApiInputTask apiInputBlock:
          sb.Append(apiInputBlock.ToCode(false));
          sb.Append("\n\r");
          break;
        default:
          throw new InvalidOperationException("Unsupported node type.");
      }
    }
    foreach (var connection in Transitions)
    {
      sb.Append(connection.ToCode(false));
      sb.Append("\n\r");
    }
    return sb.ToString();
  }

  public List<BpnTask> OrphanElements()
  {
    return Tasks.Where(node => Transitions.Where(c => c.FromBPN == node.Id || c.ToBPN == node.Id).Any() == false).ToList();
  }
  public (bool Valid, string Reason) IsValid()
  {
    if (Tasks.Count == 0) return (false, "No nodes");
    if (Tasks.First().GetType() != typeof(ApiInputTask)) return (false, $"First node can not be {Tasks.First().GetType()}");
    if (Transitions.Count == 0) return (false, "No connections");
    if (OrphanElements().Count > 0) return (false, "Orphan elements: " + string.Join(',', OrphanElements().Select(p => p.Name)));


    return (true, "");
  }
  public static BpnFeature CreateNew(string name, ImmutableList<BpnTask> tasks, ImmutableList<Transition> transitions, ImmutableList<Environment> targetEnvironments)
  {
    return new BpnFeature(Guid.CreateVersion7(), name, -1, DateTime.MinValue.ToUniversalTime(), "<-IN DEVELOPMENT->", tasks, transitions, targetEnvironments);
  }
}