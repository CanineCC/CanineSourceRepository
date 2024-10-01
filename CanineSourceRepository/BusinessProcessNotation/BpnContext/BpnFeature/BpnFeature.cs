using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureProjection.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
public enum Environment { Development, Testing, Staging, Production };


public class BpnFeatureAggregate
{
  public Guid Id { get; internal set; }
  public long Revision { get; internal set; } = 1;
  public BpnFeatureDiagram Diagram { get; internal set; } = new BpnFeatureDiagram();
  public ImmutableList<BpnTask> Tasks { get; internal set; } = [];
  public ImmutableList<BpnTransition> Transitions { get; internal set; } = [];

  public static void Apply(BpnFeatureAggregate aggregate, IEvent<FeatureReleased> @event)
  {
    aggregate.Id = @event.StreamId;
    aggregate.Revision = @event.Data.Version;
    aggregate.Tasks = @event.Data.Tasks;
    aggregate.Transitions = @event.Data.Transitions;
    aggregate.Diagram = @event.Data.Diagram;
  }
}

public class BpnFeatureProjection : SingleStreamProjection<BpnFeatureProjection.BpnFeature>
{

  public class BpnFeatureVersion
  {
    public BpnFeatureDiagram Diagram { get; set; } = new BpnFeatureDiagram();
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Describe the business purpose of the entire feature in business terms, not technical ones.
    /// </summary>
    /// <example>
    /// Enable users to register, validate their email, and gain access to premium content.
    /// </example>
    public string Objective { get; set; } = string.Empty;
    /// <summary>
    /// A high-level description of the business process from start to finish. 
    /// </summary>
    /// <example>
    /// The user enters their registration details, verifies their email, and is granted access to restricted areas.
    /// </example>
    public string FlowOverview { get; set; } = string.Empty;

    public string ReleasedBy { get;  set; } = string.Empty;
    public DateTimeOffset? ReleasedDate { get;  set; } = null;

    public long Revision { get;  set; } = 0;
    public ImmutableList<BpnTask> Tasks { get;  set; } = [];
    public ImmutableList<BpnTransition> Transitions { get;  set; } = [];
    public ImmutableList<Environment> TargetEnvironments { get;  set; } = [];
  }

  public class BpnFeature
  {
    public static string ToCode(ImmutableList<BpnTask> tasks, ImmutableList<BpnTransition> transitions)
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
      sb.AppendLine("using static CanineSourceRepository.BusinessProcessNotation.Engine.BpnEngine;");
      sb.AppendLine($"namespace {BpnEngine.CodeNamespace};");
      sb.AppendLine();
      foreach (var node in tasks)
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
      foreach (var connection in transitions)
      {
        sb.Append(connection.ToCode(false));
        sb.Append("\n\r");
      }
      return sb.ToString();
    }
    public record FeatureReleased(string ReleasedBy, string Name, string Objective, string FlowOverview, ImmutableList<BpnTask> Tasks, ImmutableList<BpnTransition> Transitions,  BpnFeatureDiagram Diagram, long Version);
    public record EnvironmentsUpdated(long FeatureVersion, Environment[] Environment);

    public Guid Id { get; set; }
    public List<BpnFeatureVersion> Versions { get; set; } = [];
    public BpnFeature() { }
    public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(ToCode(Versions.SelectMany(version => version.Tasks).ToImmutableList(), Versions.SelectMany(version => version.Transitions).ToImmutableList()));
    public void Apply(BpnFeature projection, EnvironmentsUpdated @event)
    {
      var version = projection.Versions.First(p => p.Revision == @event.FeatureVersion);
      version.TargetEnvironments = @event.Environment.ToImmutableList();
    }
    public static void Apply(BpnFeature projection, IEvent<FeatureReleased> @event)
    {
      var currentNewest = projection.Versions.Count() == 0 ? 0 : projection.Versions.Max(p => p.Revision);
      if (currentNewest < @event.Data.Version)
      {
        var newVersion = new BpnFeatureVersion()
        {
          Name = @event.Data.Name,
          Objective = @event.Data.Objective,
          FlowOverview = @event.Data.FlowOverview,
          Tasks = @event.Data.Tasks,
          Transitions = @event.Data.Transitions,
          ReleasedBy = @event.Data.ReleasedBy,
          ReleasedDate = @event.Timestamp,
          Revision = @event.Data.Version,
          Diagram = @event.Data.Diagram,
          
        };

        projection.Id = @event.StreamId;
        projection.Versions.Add(newVersion);
      }
    }
  }
}