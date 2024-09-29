//namespace CanineSourceRepository.BusinessProcessNotation.Context.Feature;

//public static class BpnDiagramRepository
//{
//  private static readonly Lock padLock = new();

//  private static readonly JsonSerializerOptions jsonSerializeOptions = new()
//  {
//    Converters = { new JsonStringEnumConverter() },
//  };

//  private static List<BpnFeatureDiagram> bpnFeatureDiagrams = [];
//  public static void Load()
//  {
//    lock (padLock)
//    {
//      if (File.Exists("diagrams.bpn"))
//      {
//        var diagrams = File.ReadAllText("diagrams.bpn");
//        bpnFeatureDiagrams = JsonSerializer.Deserialize<List<BpnFeatureDiagram>>(diagrams) ?? [];
//      }
//    }
//  }
//  public static void Save()
//  {
//    lock (padLock)
//    {
//      var diagramJson = JsonSerializer.Serialize(bpnFeatureDiagrams, jsonSerializeOptions);
//      File.WriteAllText("diagrams.bpn", diagramJson);
//    }
//  }
//  public static bool Exists(Guid id)
//  {
//    return bpnFeatureDiagrams.Where(p => p.FeatureId == id).Any();
//  }
//  public static BpnFeatureDiagram Load(Guid id)
//  {
//    return bpnFeatureDiagrams.Where(p => p.FeatureId == id).OrderByDescending(p => p.FeatureVersion).First();
//  }
//  public static BpnFeatureDiagram Load(Guid id, long version)
//  {
//    return bpnFeatureDiagrams.First(p => p.FeatureId == id && p.FeatureVersion == version);
//  }

//  public static BpnFeatureDiagram Add(BpnFeatureDiagram diagram)
//  {
//    if (bpnFeatureDiagrams.Where(p => p.FeatureId == diagram.FeatureId && p.FeatureVersion >= diagram.FeatureVersion).Any())
//    {
//      throw new InvalidOperationException($"Trying to add a deprecated version of the diagram to the repository Id:{diagram.FeatureId}, Version:{diagram.FeatureVersion}");
//    }
//    bpnFeatureDiagrams.Add(diagram);
//    return diagram;
//  }

//}