namespace CanineSourceRepository.BusinessProcessNotation;

/// <summary>
/// Visual representation of the BpnFeature, annotations for position on UI
/// </summary>
public class BpnFeatureDiagram
{
  public Guid FeatureId { get; set;  }
  public long FeatureVersion { get; set; }
  public record Position(int X, int Y);
  public record BpnPosition(Guid Id, Position Position);
  public List<BpnPosition> BpnPositions { get; set; } = [];

  public List<ConnectionWaypoints> BpnConnectionWaypoints { get; set; } = [];
  public record ConnectionWaypoints(Guid FromBPN, Guid ToBPN, params Position[] Waypoints);

  public List<Bpn> MissingElements()
  {
    var feature = BpnRepository.Load(FeatureId, FeatureVersion);
    var positions = BpnPositions.Select(n => n.Id).ToList();
    return feature.Nodes.Where(node => positions.Contains(node.Id) == false).ToList();
  }
  public List<Connection> MissingConnections()
  {
    var feature = BpnRepository.Load(FeatureId, FeatureVersion);
    var connectionProjections = feature.Connections
        .Select(c => new { c.FromBPN, c.ToBPN});

    var waypointProjections = BpnConnectionWaypoints
        .Select(w => new { w.FromBPN, w.ToBPN });

    var difference = connectionProjections
        .Except(waypointProjections)
        .ToList();

    return feature.Connections
        .Where(c => difference
            .Contains(new { c.FromBPN, c.ToBPN }))
        .ToList();
  }

  public List<Bpn> OrphanElements()
  {
    var feature = BpnRepository.Load(FeatureId, FeatureVersion);
    return feature.Nodes.Where(node => feature.Connections.Where(c => c.FromBPN == node.Id || c.ToBPN == node.Id).Any() == false).ToList();
  }
}


