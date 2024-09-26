using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using EngineEvents;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature;

public static class BpnFeatureRepository
{
  private static readonly Lock padLock = new();

  public static readonly JsonSerializerOptions bpnJsonSerialize = new()
  {
    Converters = { new JsonStringEnumConverter(), new BpnConverter(), new EventLogJsonConverter() }
  };

  private static List<BpnFeature> businessProcessNotations = [];
  public static void Load()
  {
    lock (padLock)
    {
      if (File.Exists("flows.bpn"))
      {
        var definitions = File.ReadAllText("flows.bpn");
        businessProcessNotations = JsonSerializer.Deserialize<List<BpnFeature>>(definitions, bpnJsonSerialize) ?? [];
      }
    }
  }
  public static void Save()
  {
    lock (padLock)
    {
      var bpnJson = JsonSerializer.Serialize(businessProcessNotations, bpnJsonSerialize);
      File.WriteAllText("flows.bpn", bpnJson);
    }
  }

  public static List<BpnFeature> All()
  {
    if (businessProcessNotations.Count == 0) Load();
    return businessProcessNotations
        .GroupBy(bpn => bpn.Id) // Group by Id
        .Select(group => group
            .OrderByDescending(bpn => bpn.Version) // Order by Version in descending order
            .First()) // Select the first (highest version) from each group
        .ToList(); // Convert to List
  }

  public static bool Exists(Guid id)
  {
    if (businessProcessNotations.Count == 0) Load();
    return businessProcessNotations.Where(p => p.Id == id).Any();
  }
  public static BpnFeature Load(Guid id)
  {
    if (businessProcessNotations.Count == 0) Load();
    return businessProcessNotations.Where(p => p.Id == id).OrderByDescending(p => p.Version).First();
  }
  public static BpnFeature Load(Guid id, long version)
  {
    if (businessProcessNotations.Count == 0) Load();
    return businessProcessNotations.First(p => p.Id == id && p.Version == version);
  }

  public static BpnFeature Add(BpnFeature feature)
  {
    if (businessProcessNotations.Count == 0) Load();
    if (businessProcessNotations.Where(p => p.Id == feature.Id && p.Version >= feature.Version).Any())
    {
      throw new InvalidOperationException($"Trying to add a deprecated version of the diagram to the repository Name:{feature.Name}, Version:{feature.Version}");
    }
    businessProcessNotations.Add(feature);
    return feature;
  }

}