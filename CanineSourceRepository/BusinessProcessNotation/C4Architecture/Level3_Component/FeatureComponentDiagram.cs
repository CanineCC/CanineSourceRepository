
using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;

/// <summary>
/// Visual representation of the BpnFeature, annotations for position on UI
/// </summary>
public class FeatureComponentDiagram
{

  public record Position
  {
    public Position(int x, int y)
    {
      X = x;
      Y = y;
    }
    [Required]
    public int X { get; set; }

    [Required]
    public int Y { get; set; }
  }
  public record BpnPosition
  {
    public BpnPosition(Guid id, Position position)
    {
      Id = id;
      Position = position;
    }
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Position Position { get; set; }
  }
  public List<BpnPosition> BpnPositions { get; set; } = [];

  public List<ConnectionWaypoints> BpnConnectionWaypoints { get; set; } = [];
  public record ConnectionWaypoints
  {
    public ConnectionWaypoints(Guid fromBPN, Guid toBPN, Position[] waypoints)
    {
      FromBPN = fromBPN;
      ToBPN = toBPN;
      Waypoints = waypoints ?? throw new ArgumentNullException(nameof(waypoints));
    }

    [Required]
    public Guid FromBPN { get; set; }

    [Required]
    public Guid ToBPN { get; set; }

    [Required]
    public Position[] Waypoints { get; set; }
  }
}


