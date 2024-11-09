using CanineSourceRepository.BusinessProcessNotation.Engine;
using EngineEvents;
using Marten.Events.Projections;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;
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
    aggregate.Revision = @event.Data.Revision;
    aggregate.Tasks = @event.Data.Tasks;
    aggregate.Transitions = @event.Data.Transitions;
    aggregate.Diagram = @event.Data.Diagram;
  }
}

public class BpnFeatureProjection : SingleStreamProjection<BpnFeatureProjection.BpnFeature>
{
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/Feature/{featureId}/{revision}", async (HttpContext context, [FromServices] IQuerySession session, Guid featureId, long revision, CancellationToken ct) =>
    {
      var bpnFeature = await session.Query<BpnFeatureProjection.BpnFeature>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();
      var bpnRevision = bpnFeature.Revisions.FirstOrDefault(ver => ver.Revision == revision);
      if (bpnRevision == null) return Results.NotFound();

      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromDays(120)
      };

      return Results.Ok(bpnRevision);
    }).WithName("GetFeatureRevision")
  .Produces(StatusCodes.Status200OK, typeof(BpnFeatureProjection.BpnFeatureRevision))
  .WithTags("Feature");
    
    app.MapGet("BpnEngine/v1/Feature/DiagramSvg/{containerId}", async (HttpContext context, [FromServices] IQuerySession session,  Guid containerId, CancellationToken ct) =>
      {
        //TODO: /{revision} <-- kræver at vi får revision på container? eller ihvertfald at publish/versionering sker på tværs af features i en container!
        //TODO: Generate all C4 documentation "on events", so this will purely be a fetch from db instead of a rendering
        var bpnContext = await session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().Where(p=>p.Id == containerId).FirstAsync(ct);
        var bpnSystem = await session.Query<BpnSystemProjection.BpnSystem>().Where(p=>p.Id == bpnContext.SystemId).FirstAsync(ct);
        var bpnContexts = await session.Query<BpnBpnWebApiContainerProjection.BpnWebApiContainer>().Where(p=>p.SystemId == bpnContext.SystemId).ToListAsync(ct);
        var featureIds = bpnContext.Features.Select(p => p.Id);
        var bpnFeatures = (await session.Query<BpnFeatureProjection.BpnFeature>().ToListAsync(ct));
          
        bpnFeatures = bpnFeatures.Where(p => featureIds.Contains(p.Id)).ToList();//TODO: SUB OPTIMAL (DO FIX)
        var diagram = new C4ComponentDiagram(bpnSystem, bpnContext,bpnContexts.ToArray(), bpnFeatures.ToArray());
        var svg = C4DiagramHelper.GenerateC4(diagram);
        context.Response.ContentType = "image/svg+xml"; 
        await context.Response.WriteAsync(svg, ct);
      }).WithName("GetC4_level3DiagramSvg")
      .Produces(StatusCodes.Status200OK, typeof(string))
      .WithTags("Feature.Diagram");
    
    
    
  }
  public class BpnFeatureRevision
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

  //TODO: published date on feature
  public class BpnFeature
  {
    public static string ToCode(ImmutableList<BpnTask> tasks, ImmutableList<BpnTransition> transitions)
    {
      var sb = new StringBuilder();
      sb.AppendLine("using System;");
      sb.AppendLine("using System.Threading.Tasks;");
      sb.AppendLine("using System.Linq;");
      sb.AppendLine("using System.Collections.Generic;");
      sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level2_Container;");
      sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;");
      sb.AppendLine("using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level4_Code;");
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

    public Guid Id { get; set; }
    public List<BpnFeatureRevision> Revisions { get; set; } = [];
    public BpnFeature() { }
    public Assembly ToAssembly() => DynamicCompiler.PrecompileCode(ToCode(Revisions.SelectMany(revision => revision.Tasks).ToImmutableList(), Revisions.SelectMany(revision => revision.Transitions).ToImmutableList()));
    public void Apply(BpnFeature projection, EnvironmentsUpdated @event)
    {
      var revision = projection.Revisions.First(p => p.Revision == @event.FeatureRevision);
      revision.TargetEnvironments = [.. @event.Environment];
    }
    public static void Apply(BpnFeature projection, IEvent<FeatureReleased> @event)
    {
      var currentNewest = projection.Revisions.Count == 0 ? 0 : projection.Revisions.Max(p => p.Revision);
      if (currentNewest < @event.Data.Revision)
      {
        var newRevision = new BpnFeatureRevision()
        {
          Name = @event.Data.Name,
          Objective = @event.Data.Objective,
          FlowOverview = @event.Data.FlowOverview,
          Tasks = @event.Data.Tasks,
          Transitions = @event.Data.Transitions,
          ReleasedBy = @event.Data.ReleasedBy,
          ReleasedDate = @event.Timestamp,
          Revision = @event.Data.Revision,
          Diagram = @event.Data.Diagram,
          
        };

        projection.Id = @event.Data.FeatureId;
        projection.Revisions.Add(newRevision);
      }
    }
  }
}

public class BpnFeatureStatsProjection : MultiStreamProjection<BpnFeatureStatsProjection.BpnFeatureStat, Guid>
{
  public record DurationClassification(long FromMs, long ToMs, string HexColor, string Category);
  public static void RegisterBpnEventStore(WebApplication app)
  {
    app.MapGet("BpnEngine/v1/Server/DurationClassification", (HttpContext context, CancellationToken ct) =>
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
    }).WithName("GetDurationClassification")
      .Produces(StatusCodes.Status200OK, typeof(List<DurationClassification>))
      .WithTags("Server");

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

    app.MapGet("BpnEngine/v1/Feature/Stats/{featureId}/{revision}", async (HttpContext context, [FromServices] IQuerySession session, Guid featureId, long revision, CancellationToken ct) =>
    {
      var bpnFeature = await session.Query<BpnFeatureStatsProjection.BpnFeatureStat>().Where(p => p.Id == featureId).SingleOrDefaultAsync();
      if (bpnFeature == null) return Results.NotFound();

      context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
      {
        Public = true,
        MaxAge = TimeSpan.FromSeconds(10)
      };

      var filterResult = new BpnFeatureRevisionsStat()
      {
        Id = bpnFeature.Id,
        FeatureStats = bpnFeature.FeatureStats,
        TaskStats = bpnFeature.TaskStats.Where(p => p.Key.EndsWith("_"+revision)).Select(p=> p.Value).ToList(),
        RevisionStats = bpnFeature.RevisionStats.FirstOrDefault(p => p.Key.EndsWith("_" + revision)).Value
      };
      
      
      return Results.Ok(filterResult);
    }).WithName("GetFeatureRevisionStats")
  .Produces(StatusCodes.Status200OK, typeof(BpnFeatureStatsProjection.BpnFeatureRevisionsStat))
  .WithTags("Feature");
  }
  private static readonly object _lock = new object();
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
  public static void Apply(BpnFeatureStat view, IEvent<BpnFeatureStarted> @event)
  {
    lock (_lock)
    {
      var revisionKey = RevisionId(@event.Data);
      view.Id = @event.Data.FeatureId;
      //view.Revision = @event.Data.FeatureVersion;
      view.FeatureStats.InvocationCount++;
      view.FeatureStats.LastUsed = @event.Timestamp;
      view.FeatureStats.InvocationsInProgressCount = view.FeatureStats.InvocationCount - view.FeatureStats.InvocationErrorCount - view.FeatureStats.InvocationCompletedCount;

      if (!view.RevisionStats.ContainsKey(revisionKey))
        view.RevisionStats.Add(revisionKey, new Stats());

      view.RevisionStats[revisionKey].InvocationCount++;
      view.RevisionStats[revisionKey].LastUsed = @event.Timestamp;
      view.RevisionStats[revisionKey].InvocationsInProgressCount = view.RevisionStats[revisionKey].InvocationCount - view.RevisionStats[revisionKey].InvocationErrorCount - view.RevisionStats[revisionKey].InvocationCompletedCount;
    }
  }
  public static void Apply(BpnFeatureStat view, IEvent<BpnFeatureError> @event)
  {
      var revisionKey = RevisionId(@event.Data);
      view.FeatureStats.InvocationErrorCount++;
      view.FeatureStats.InvocationsInProgressCount = view.FeatureStats.InvocationCount - view.FeatureStats.InvocationErrorCount - view.FeatureStats.InvocationCompletedCount;

      view.RevisionStats[revisionKey].InvocationCount++;
      view.RevisionStats[revisionKey].InvocationsInProgressCount = view.RevisionStats[revisionKey].InvocationCount - view.RevisionStats[revisionKey].InvocationErrorCount - view.RevisionStats[revisionKey].InvocationCompletedCount;
  }
  public static void Apply(BpnFeatureStat view, IEvent<BpnFeatureCompleted> @event)
  {
      var revisionKey = RevisionId(@event.Data);
      view.FeatureStats.InvocationCompletedCount++;
      view.FeatureStats.InvocationsInProgressCount = view.FeatureStats.InvocationCount - view.FeatureStats.InvocationErrorCount - view.FeatureStats.InvocationCompletedCount;
      view.FeatureStats.MaxDurationMs = Math.Max(view.FeatureStats.MaxDurationMs, @event.Data.DurationMs);
      view.FeatureStats.MinDurationMs = view.FeatureStats.MinDurationMs == 0 ? @event.Data.DurationMs : Math.Min(view.FeatureStats.MinDurationMs, @event.Data.DurationMs);
      view.FeatureStats.TotalDurationMs += (decimal)@event.Data.DurationMs;
      view.FeatureStats.AvgDurationMs = (double)(view.FeatureStats.TotalDurationMs / view.FeatureStats.InvocationCompletedCount);


      view.RevisionStats[revisionKey].InvocationCompletedCount++;
      view.RevisionStats[revisionKey].InvocationsInProgressCount = view.RevisionStats[revisionKey].InvocationCount - view.RevisionStats[revisionKey].InvocationErrorCount - view.RevisionStats[revisionKey].InvocationCompletedCount;
      view.RevisionStats[revisionKey].MaxDurationMs = Math.Max(view.RevisionStats[revisionKey].MaxDurationMs, @event.Data.DurationMs);
      view.RevisionStats[revisionKey].MinDurationMs = view.RevisionStats[revisionKey].MinDurationMs == 0 ? @event.Data.DurationMs : Math.Min(view.RevisionStats[revisionKey].MinDurationMs, @event.Data.DurationMs);
      view.RevisionStats[revisionKey].TotalDurationMs += (decimal)@event.Data.DurationMs;
      view.RevisionStats[revisionKey].AvgDurationMs = (double)(view.RevisionStats[revisionKey].TotalDurationMs / view.RevisionStats[revisionKey].InvocationCompletedCount);
  }
  public static void Apply(BpnFeatureStat view, IEvent<BpnTaskInitialized> @event)
  {
    lock (_lock)
    {
      var key = RevisionTaskId(@event.Data, @event.Data.TaskId);
      if (!view.TaskStats.ContainsKey(key))
        view.TaskStats.Add(key, new TaskStats(@event.Data.TaskId));

      view.TaskStats[key].InvocationCount++;
      view.TaskStats[key].LastUsed = @event.Timestamp;
      view.TaskStats[key].InvocationsInProgressCount = view.TaskStats[key].InvocationCount - view.TaskStats[key].InvocationErrorCount - view.TaskStats[key].InvocationCompletedCount;
    }
  }
  public static void Apply(BpnFeatureStat view, IEvent<BpnTaskFailed> @event)
  {
    var key = RevisionTaskId(@event.Data, @event.Data.TaskId);
    view.TaskStats[key].InvocationErrorCount++;
    view.TaskStats[key].InvocationsInProgressCount = view.TaskStats[key].InvocationCount - view.TaskStats[key].InvocationErrorCount - view.TaskStats[key].InvocationCompletedCount;
  }
  public static void Apply(BpnFeatureStat view, IEvent<BpnTaskSucceeded> @event)
  {
    var key = RevisionTaskId(@event.Data, @event.Data.TaskId);
    view.TaskStats[key].InvocationCompletedCount++;
    view.TaskStats[key].InvocationsInProgressCount = view.TaskStats[key].InvocationCount - view.TaskStats[key].InvocationErrorCount - view.TaskStats[key].InvocationCompletedCount;

    view.TaskStats[key].MaxDurationMs = Math.Max(view.TaskStats[key].MaxDurationMs, @event.Data.ExecutionTimeMs);
    view.TaskStats[key].MinDurationMs = view.TaskStats[key].MinDurationMs == 0 ? @event.Data.ExecutionTimeMs : Math.Min(view.TaskStats[key].MinDurationMs, @event.Data.ExecutionTimeMs);
    view.TaskStats[key].TotalDurationMs += (decimal)@event.Data.ExecutionTimeMs;
    view.TaskStats[key].AvgDurationMs = (double)(view.TaskStats[key].TotalDurationMs / view.TaskStats[key].InvocationCompletedCount);
  }

  private static string RevisionTaskId(IEngineEvents @event, Guid taskId)
  {
    return $"{@event.FeatureId}_{taskId}_{@event.FeatureRevision}";
  }
  private static string RevisionId(IEngineEvents @event)
  {
    return $"{@event.FeatureId}_{@event.FeatureRevision}";
  }

  public class BpnFeatureStat
  {
    [Required]
    public Guid Id { get; set; }
    //public long Revision { get; set; } = 0;
    [Required]
    public Stats FeatureStats { get; set; } = new();
    [Required]
    public Dictionary<string, Stats> RevisionStats { get; set; } = [];
    [Required]
    public Dictionary<string, TaskStats> TaskStats { get; set; } = [];
  }
  public class BpnFeatureRevisionsStat
  {
    [Required]
    public Guid Id { get; set; }
    //public long Revision { get; set; } = 0;
    [Required]
    public Stats FeatureStats { get; set; } = new();
    [Required]
    public Stats RevisionStats { get; set; } = new();
    [Required]
    public List<TaskStats> TaskStats { get; set; } = new();
  }
  public class Stats(long InvocationCount = 0, long InvocationErrorCount = 0, long InvocationCompletedCount = 0, long InvocationsInProgressCount = 0, decimal TotalDurationMs = 0, double MaxDurationMs = 0, double AvgDurationMs = 0, double MinDurationMs = 0, DateTimeOffset? LastUsed = null)
  {
    [Required]
    public long InvocationCount { get; set; } = InvocationCount;
    [Required]
    public long InvocationErrorCount { get; set; } = InvocationErrorCount;
    [Required]
    public long InvocationCompletedCount { get; set; } = InvocationCompletedCount;
    [Required]
    public long InvocationsInProgressCount { get; set; } = InvocationsInProgressCount;
    [Required]
    public decimal TotalDurationMs { get; set; } = TotalDurationMs;
    [Required]
    public double MaxDurationMs { get; set; } = MaxDurationMs;
    [Required]
    public double AvgDurationMs { get; set; } = AvgDurationMs;
    [Required]
    public double MinDurationMs { get; set; } = MinDurationMs;
    public DateTimeOffset? LastUsed { get; set; } = LastUsed;
  }
  public class TaskStats(Guid Task, long InvocationCount = 0, long InvocationErrorCount = 0, long InvocationCompletedCount = 0, long InvocationsInProgressCount = 0, decimal TotalDurationMs = 0, double MaxDurationMs = 0, double AvgDurationMs = 0, double MinDurationMs = 0, DateTimeOffset? LastUsed = null)
  {
    [Required]
    public Guid Task { get; set; } = Task;
    [Required]
    public long InvocationCount { get; set; } = InvocationCount;
    [Required]
    public long InvocationErrorCount { get; set; } = InvocationErrorCount;
    [Required]
    public long InvocationCompletedCount { get; set; } = InvocationCompletedCount;
    [Required]
    public long InvocationsInProgressCount { get; set; } = InvocationsInProgressCount;
    [Required]
    public decimal TotalDurationMs { get; set; } = TotalDurationMs;
    [Required]
    public double MaxDurationMs { get; set; } = MaxDurationMs;
    [Required]
    public double AvgDurationMs { get; set; } = AvgDurationMs;
    [Required]
    public double MinDurationMs { get; set; } = MinDurationMs;
    public DateTimeOffset? LastUsed { get; set; } = LastUsed;
  }
}