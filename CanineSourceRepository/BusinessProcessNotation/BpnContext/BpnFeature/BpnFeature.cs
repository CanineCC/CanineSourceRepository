using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using EngineEvents;
using Marten.Events.Projections;
using Microsoft.Net.Http.Headers;
using static CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature.BpnFeatureProjection.BpnFeature;

namespace CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;
public enum Environment { Development, Testing, Staging, Production };


public class BpnFeatureAggregate
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
  }
  public Guid Id { get; internal set; }
  public Guid ContextId { get; internal set; }
  public long Revision { get; internal set; } = 0;
  public BpnFeatureDiagram Diagram { get; internal set; } = new BpnFeatureDiagram();
  public ImmutableList<BpnTask> Tasks { get; internal set; } = [];
  public ImmutableList<BpnTransition> Transitions { get; internal set; } = [];

  public static void Apply(BpnFeatureAggregate aggregate, IEvent<FeatureReleased> @event)
  {
    aggregate.Id = @event.StreamId;
    aggregate.ContextId = @event.Data.ContextId;
    aggregate.Revision = @event.Data.Version;
    aggregate.Tasks = @event.Data.Tasks;
    aggregate.Transitions = @event.Data.Transitions;
    aggregate.Diagram = @event.Data.Diagram;
  }
}

public class BpnFeatureProjection : SingleStreamProjection<BpnFeatureProjection.BpnFeature>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/Feature/{featureId}/{version}", async (HttpContext context, [FromServices] IDocumentSession session, Guid featureId, long version, CancellationToken ct) =>
    {
      var bpnFeature = await session.Query<BpnFeatureProjection.BpnFeature>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();

      var bpnVersion = bpnFeature.Versions.FirstOrDefault(ver => ver.Revision == version);
      if (bpnVersion == null) return Results.NotFound();

      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromDays(120)
      };

      return Results.Ok(bpnVersion);
    }).WithName("GetFeatureVersion")
  .Produces(StatusCodes.Status200OK, typeof(BpnFeatureProjection.BpnFeatureVersion))
  .WithTags("Feature");
  }
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
    public record FeatureReleased(Guid ContextId, Guid FeatureId, string ReleasedBy, string Name, string Objective, string FlowOverview, ImmutableList<BpnTask> Tasks, ImmutableList<BpnTransition> Transitions,  BpnFeatureDiagram Diagram, long Version);
    public record EnvironmentsUpdated(Guid ContextId, Guid FeatureId, long FeatureVersion, Environment[] Environment);

    public Guid Id { get; set; }
    public List<BpnFeatureVersion> Versions { get; set; } = [];
    public BpnFeature() { }
    public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(ToCode(Versions.SelectMany(version => version.Tasks).ToImmutableList(), Versions.SelectMany(version => version.Transitions).ToImmutableList()));
    public void Apply(BpnFeature projection, EnvironmentsUpdated @event)
    {
      var version = projection.Versions.First(p => p.Revision == @event.FeatureVersion);
      version.TargetEnvironments = [.. @event.Environment];
    }
    public static void Apply(BpnFeature projection, IEvent<FeatureReleased> @event)
    {
      var currentNewest = projection.Versions.Count == 0 ? 0 : projection.Versions.Max(p => p.Revision);
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

public class BpnFeatureStatsProjection : MultiStreamProjection<BpnContextProjection.BpnContext, Guid>
{
  public record DurationClassification(long FromMs, long ToMs, string HexColor, string Category);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/Feature/DurationClassification", (HttpContext context, CancellationToken ct) =>
    {
      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromDays(1)
      };
      return Results.Ok(new List<DurationClassification> { 
        new DurationClassification(0, 30, PerformanceCategory.WorldClass.GetColor(), PerformanceCategory.WorldClass.ToString()),
        new DurationClassification(30, 50, PerformanceCategory.Excellent.GetColor(), PerformanceCategory.Excellent.ToString()),
        new DurationClassification(50, 100, PerformanceCategory.Good.GetColor(), PerformanceCategory.Good.ToString()),
        new DurationClassification(100, 300, PerformanceCategory.Average.GetColor(), PerformanceCategory.Average.ToString()),
        new DurationClassification(300, 1000, PerformanceCategory.BelowAverage.GetColor(), PerformanceCategory.BelowAverage.ToString()),
        new DurationClassification(1000, long.MaxValue, PerformanceCategory.Bad.GetColor(), PerformanceCategory.Bad.ToString()),
      });
    }).WithName("GetFeatureDurationClassification")
      .Produces(StatusCodes.Status200OK, typeof(List<DurationClassification>))
      .WithTags("Feature");

    app.MapGet("BpnEngine/v1/Task/DurationClassification", (HttpContext context, CancellationToken ct) =>
    {
      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromDays(1)
      };
      return Results.Ok(new List<DurationClassification> {
        new DurationClassification(0, 10, PerformanceCategory.WorldClass.GetColor(), PerformanceCategory.WorldClass.ToString()),
        new DurationClassification(10, 20, PerformanceCategory.Excellent.GetColor(), PerformanceCategory.Excellent.ToString()),
        new DurationClassification(20, 50, PerformanceCategory.Good.GetColor(), PerformanceCategory.Good.ToString()),
        new DurationClassification(50, 100, PerformanceCategory.Average.GetColor(), PerformanceCategory.Average.ToString()),
        new DurationClassification(100, 200, PerformanceCategory.BelowAverage.GetColor(), PerformanceCategory.BelowAverage.ToString()),
        new DurationClassification(200, long.MaxValue, PerformanceCategory.Bad.GetColor(), PerformanceCategory.Bad.ToString()),
      });
    }).WithName("GetTaskDurationClassification")
         .Produces(StatusCodes.Status200OK, typeof(List<DurationClassification>))
         .WithTags("Feature.Task");

    app.MapGet("BpnEngine/v1/Feature/Stats/{featureId}/{version}", async (HttpContext context, [FromServices] IDocumentSession session, Guid featureId, long version, CancellationToken ct) =>
    {
      var bpnFeature = await session.Query<BpnFeatureStatsProjection.BpnFeatureVersionStat>().Where(p => p.Id == featureId && p.Revision == version).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();

      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromSeconds(10)
      };
      return Results.Ok(bpnFeature);
    }).WithName("GetFeatureVersionStats")
  .Produces(StatusCodes.Status200OK, typeof(BpnFeatureProjection.BpnFeatureVersion))
  .WithTags("Feature");
  }
  public BpnFeatureStatsProjection()
  {
    Identity<BpnFeatureStarted>(x => x.FeatureId);
    Identity<BpnFeatureError>(x => x.FeatureId);
    Identity<BpnFeatureCompleted>(x => x.FeatureId);
    Identity<BpnTaskInitialized>(x => x.FeatureId);
    Identity<BpnTaskFailed>(x => x.FeatureId);
    Identity<BpnFailedTaskReInitialized>(x => x.FeatureId);
    Identity<BpnTaskSucceeded>(x => x.FeatureId);
  }
  public static void Apply(BpnFeatureVersionStat view, IEvent<BpnFeatureStarted> @event)
  {
    view.Id = @event.Data.FeatureId;
    view.Revision = @event.Data.FeatureVersion;
    view.FeatureStats.InvocationCount++;
    view.FeatureStats.InvocationsInProgressCount = view.FeatureStats.InvocationCount - view.FeatureStats.InvocationErrorCount - view.FeatureStats.InvocationCompletedCount;
  }
  public static void Apply(BpnFeatureVersionStat view, IEvent<BpnFeatureError> @event)
  {
    view.FeatureStats.InvocationErrorCount++;
    view.FeatureStats.InvocationsInProgressCount = view.FeatureStats.InvocationCount - view.FeatureStats.InvocationErrorCount - view.FeatureStats.InvocationCompletedCount;
  }
  public static void Apply(BpnFeatureVersionStat view, IEvent<BpnFeatureCompleted> @event)
  {
    view.FeatureStats.InvocationCompletedCount++;
    view.FeatureStats.InvocationsInProgressCount = view.FeatureStats.InvocationCount - view.FeatureStats.InvocationErrorCount - view.FeatureStats.InvocationCompletedCount;

    view.FeatureStats.MaxDurationMs = Math.Max(view.FeatureStats.MaxDurationMs, @event.Data.DurationMs);
    view.FeatureStats.MinDurationMs = view.FeatureStats.MinDurationMs == 0 ? @event.Data.DurationMs : Math.Min(view.FeatureStats.MinDurationMs, @event.Data.DurationMs);
    view.FeatureStats.TotalDurationMs += (decimal)@event.Data.DurationMs;
    view.FeatureStats.AvgDurationMs = (double)(view.FeatureStats.TotalDurationMs / view.FeatureStats.InvocationCompletedCount);
  }
  public static void Apply(BpnFeatureVersionStat view, IEvent<BpnTaskInitialized> @event)
  {
    if (!view.TaskStats.ContainsKey(@event.Data.TaskId))
      view.TaskStats.Add(@event.Data.TaskId, new Stats());

    view.TaskStats[@event.Data.TaskId].InvocationCount++;
    view.TaskStats[@event.Data.TaskId].InvocationsInProgressCount = view.TaskStats[@event.Data.TaskId].InvocationCount - view.TaskStats[@event.Data.TaskId].InvocationErrorCount - view.TaskStats[@event.Data.TaskId].InvocationCompletedCount;
  }
  public static void Apply(BpnFeatureVersionStat view, IEvent<BpnTaskFailed> @event)
  {
    view.TaskStats[@event.Data.TaskId].InvocationErrorCount++;
    view.TaskStats[@event.Data.TaskId].InvocationsInProgressCount = view.TaskStats[@event.Data.TaskId].InvocationCount - view.TaskStats[@event.Data.TaskId].InvocationErrorCount - view.TaskStats[@event.Data.TaskId].InvocationCompletedCount;
  }
  public static void Apply(BpnFeatureVersionStat view, IEvent<BpnTaskSucceeded> @event)
  {
    view.TaskStats[@event.Data.TaskId].InvocationCompletedCount++;
    view.TaskStats[@event.Data.TaskId].InvocationsInProgressCount = view.TaskStats[@event.Data.TaskId].InvocationCount - view.TaskStats[@event.Data.TaskId].InvocationErrorCount - view.TaskStats[@event.Data.TaskId].InvocationCompletedCount;

    view.TaskStats[@event.Data.TaskId].MaxDurationMs = Math.Max(view.TaskStats[@event.Data.TaskId].MaxDurationMs, @event.Data.ExecutionTimeMs);
    view.TaskStats[@event.Data.TaskId].MinDurationMs = view.TaskStats[@event.Data.TaskId].MinDurationMs == 0 ? @event.Data.ExecutionTimeMs : Math.Min(view.TaskStats[@event.Data.TaskId].MinDurationMs, @event.Data.ExecutionTimeMs);
    view.TaskStats[@event.Data.TaskId].TotalDurationMs += (decimal)@event.Data.ExecutionTimeMs;
    view.TaskStats[@event.Data.TaskId].AvgDurationMs = (double)(view.TaskStats[@event.Data.TaskId].TotalDurationMs / view.TaskStats[@event.Data.TaskId].InvocationCompletedCount);
  }

  public class BpnFeatureVersionStat
  {
    public Guid Id { get; set; }
    public long Revision { get; set; } = 0;
    public Stats FeatureStats { get; set; } = new();
    public Dictionary<Guid, Stats> TaskStats { get; set; } = [];
  }
  public class Stats(long InvocationCount = 0, long InvocationErrorCount = 0, long InvocationCompletedCount = 0, long InvocationsInProgressCount = 0, decimal TotalDurationMs = 0, double MaxDurationMs = 0, double AvgDurationMs = 0, double MinDurationMs = 0, DateTimeOffset? LastUsed = null)
  {
    public long InvocationCount { get; set; } = InvocationCount;
    public long InvocationErrorCount { get; set; } = InvocationErrorCount;
    public long InvocationCompletedCount { get; set; } = InvocationCompletedCount;
    public long InvocationsInProgressCount { get; set; } = InvocationsInProgressCount;
    public decimal TotalDurationMs { get; set; } = TotalDurationMs;
    public double MaxDurationMs { get; set; } = MaxDurationMs;
    public double AvgDurationMs { get; set; } = AvgDurationMs;
    public double MinDurationMs { get; set; } = MinDurationMs;
    public DateTimeOffset? LastUsed { get; set; } = LastUsed;
  }
}