
namespace CanineSourceRepository.BusinessProcessNotation.BpnContext.BpnFeature;

/// <summary>
/// Visual representation of the BpnFeature, annotations for position on UI
/// </summary>
public class BpnFeatureDiagram
{
  public record DraftFeatureDiagramPositionUpdated(BpnPosition Position);
  public record DraftFeatureDiagramWaypointUpdated(ConnectionWaypoints Waypoint);


  public record Position(int X, int Y);
  public record BpnPosition(Guid Id, Position Position);
  public List<BpnPosition> BpnPositions { get; set; } = [];

  public List<ConnectionWaypoints> BpnConnectionWaypoints { get; set; } = [];
  public record ConnectionWaypoints(Guid FromBPN, Guid ToBPN, params Position[] Waypoints);
}


