using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature;

/// <summary>
/// Visual representation of the BpnFeature, annotations for position on UI
/// </summary>
public class BpnFeatureDiagram
{
  public Guid FeatureId { get; set; }
  public long FeatureVersion { get; set; }
  public record Position(int X, int Y);
  public record BpnPosition(Guid Id, Position Position);
  public List<BpnPosition> BpnPositions { get; set; } = [];

  public List<ConnectionWaypoints> BpnConnectionWaypoints { get; set; } = [];
  public record ConnectionWaypoints(Guid FromBPN, Guid ToBPN, params Position[] Waypoints);

  public List<BpnTask> MissingElements()
  {
    var feature = BpnFeatureRepository.Load(FeatureId, FeatureVersion);
    var positions = BpnPositions.Select(n => n.Id).ToList();
    return feature.Tasks.Where(node => positions.Contains(node.Id) == false).ToList();
  }
  public List<Transition> MissingConnections()
  {
    var feature = BpnFeatureRepository.Load(FeatureId, FeatureVersion);
    var connectionProjections = feature.Transitions
        .Select(c => new { c.FromBPN, c.ToBPN });

    var waypointProjections = BpnConnectionWaypoints
        .Select(w => new { w.FromBPN, w.ToBPN });

    var difference = connectionProjections
        .Except(waypointProjections)
        .ToList();

    return feature.Transitions
        .Where(c => difference
            .Contains(new { c.FromBPN, c.ToBPN }))
        .ToList();
  }

  public List<BpnTask> OrphanElements()
  {
    var feature = BpnFeatureRepository.Load(FeatureId, FeatureVersion);
    return feature.Tasks.Where(node => feature.Transitions.Where(c => c.FromBPN == node.Id || c.ToBPN == node.Id).Any() == false).ToList();
  }
}


