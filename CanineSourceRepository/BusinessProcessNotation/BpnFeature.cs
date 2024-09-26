using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace CanineSourceRepository.BusinessProcessNotation;
public record StackTrace(Guid CorrelationId, StackElement[] Trace, string UserInformation, DateTimeOffset Timestamp);
public record StackElement(Guid BpnId, string Name, long Version, DateTimeOffset Timestamp, TimeSpan Duration, string DataInput);

public record UserContext(string UserId, string UserName, string[] AccessScopes, string IpAddress, bool IsAuthenticated, string AuthenticationType, DateTime? TokenExpiry);

public class BpnFeature
{
  public static readonly string CodeNamespace = "CanineSourceRepository";
  public enum Environment { Development, Testing, Staging, Production };
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public long Version { get; init; } = 0;
  public DateTimeOffset Timestamp { get; init; }
  public string User { get; init; } = string.Empty;
  public ImmutableList<Bpn> Nodes { get; init; } = [];
  public ImmutableList<Connection> Connections { get; init; } = [];
  public ImmutableList<Environment> TargetEnvironments { get; init; } = [];

  // Parameterless constructor required for model binding
  public BpnFeature() { }

  public BpnFeature(
      Guid id,
      string name,
      long version,
      DateTimeOffset timestamp,
      string user,
      ImmutableList<Bpn> nodes,
      ImmutableList<Connection> connections,
      ImmutableList<Environment> targetEnvironments
  )
  {
    Id = id;
    Name = name;
    Version = version;
    Timestamp = timestamp;
    User = user;
    Nodes = nodes;
    Connections = connections;
    TargetEnvironments = targetEnvironments;
  }

  public BpnFeature NewRevision(string user)
  {
    long revision = 0;
    if (BpnRepository.Exists(this.Id))
      revision = BpnRepository.Load(this.Id).Version;

    var nextVersion = revision + 1;
    var newVersion = new BpnFeature(this.Id, this.Name, nextVersion, DateTime.UtcNow, user, this.Nodes, this.Connections, this.TargetEnvironments);

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
    sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation;");
    sb.AppendLine($"namespace {BpnFeature.CodeNamespace};");
    sb.AppendLine();
//    sb.AppendLine("public record UserContext(string UserId, string UserName, string[] AccessScopes, string IpAddress, bool IsAuthenticated, string AuthenticationType, DateTime? TokenExpiry);");
    foreach (var node in this.Nodes)
    {
      switch (node)
      {
        case CodeBlock codeBlock:
          sb.Append(codeBlock.ToCode(false));
          sb.Append("\n\r");
          break;
        case ApiInputBlock apiInputBlock:
          sb.Append(apiInputBlock.ToCode(false));
          sb.Append("\n\r");
          break;
        default:
          throw new InvalidOperationException("Unsupported node type.");
      }
    }
    foreach (var connection in this.Connections)
    {
        sb.Append(connection.ToCode(false));
        sb.Append("\n\r");
    }
    return sb.ToString();
  }

  public List<Bpn> OrphanElements()
  {
    return Nodes.Where(node => Connections.Where(c => c.FromBPN == node.Id || c.ToBPN == node.Id).Any() == false).ToList();
  }
  public (bool Valid, string Reason) IsValid()
  {
    if (Nodes.Count == 0) return (false, "No nodes");
    if (Nodes.First().GetType() != typeof(ApiInputBlock)) return (false, $"First node can not be {Nodes.First().GetType()}");
    if (Connections.Count == 0) return (false, "No connections");
    if (OrphanElements().Count > 0) return (false, "Orphan elements: " + string.Join(',', OrphanElements().Select(p=>p.Name)));


    return (true, "");
  }
  public static BpnFeature CreateNew(string name, ImmutableList<Bpn> nodes, ImmutableList<Connection> connections, ImmutableList<Environment> targetEnvironments)
  {
    return new BpnFeature(Guid.CreateVersion7(), name, -1, DateTime.MinValue.ToUniversalTime(), "<-IN DEVELOPMENT->", nodes, connections, targetEnvironments);
  }
}