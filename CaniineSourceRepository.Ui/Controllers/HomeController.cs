using CanineSourceRepository.BusinessProcessNotation;
using CanineSourceRepository.Ui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace CanineSourceRepository.Ui.Controllers
{
  public class HomeController : Controller
  {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
      _logger = logger;
    }


    public IActionResult Index()
    {
      return View();
    }

    //[Authorize]
    public IActionResult BusinessProcessNotation()
    {
      var diagrams = BpnRepository.All().Select(d => new { id = d.Id, name = d.Name }).ToList();

      ViewBag.Diagrams = JsonSerializer.Serialize(diagrams);
      return View();
    }

    
    public static void GenerateDefaultData(out BpnFeature feature, out BpnFeatureDiagram diagram)
    {
      var entryBlock = new ApiInputBlock("Create user endpoint", ["Anonymous"]);
      entryBlock = (entryBlock.AddRecordType(new Bpn.RecordDefinition("Api",
        new Bpn.DataDefinition("Name", "string")
        )) as ApiInputBlock)!;
      entryBlock = entryBlock with
      {
        Input = "Api",
      };

      var createUserBlock = new CodeBlock("Create user logic");
      createUserBlock = (createUserBlock.AddRecordType(
        new Bpn.RecordDefinition("Output",
        new Bpn.DataDefinition("Id", "Guid"),
        new Bpn.DataDefinition("Name", "string")
     //   new Bpn.DataDefinition("Accessscope", "string")
        )) as CodeBlock)!;
      createUserBlock = (createUserBlock.AddRecordType(
        new Bpn.RecordDefinition("Input",
        new Bpn.DataDefinition("Name", "string")
      //  new Bpn.DataDefinition("Accessscope", "string")
        )) as CodeBlock)!;
      createUserBlock = createUserBlock with
      {
        Description = "todo - longer description to help with overview in diagram",
        Input = "Input",
        Output = "Output",
        Code = @$"
    var userId = Guid.CreateVersion7();
    //TODO: Add the user to the user database
    return new Output(userId, input.Name/*, input.Accessscope*/);
    "
      };

      var logUserBlock = new CodeBlock("Log user");
      logUserBlock = (logUserBlock.AddRecordType(
        new Bpn.RecordDefinition("Input",
        new Bpn.DataDefinition("Id", "Guid"),
        new Bpn.DataDefinition("Name", "string")
        )) as CodeBlock)!;
      logUserBlock = logUserBlock with
      {
        Description = "todo - another description to help with overview in diagram",
        Input = "Input",
        Code = @$"
    Console.WriteLine(input.Id.ToString() + input.Name);
    "
      };

      var connection = new Connection(
        entryBlock.Id,
        createUserBlock.Id,
        "Call Accepted",
        "input.Name != string.Empty",
        new Map("input.Name", "Name")//,//issue with lists and multiple fields of same type, but with different mappings
       // new Map("input.Name ?? \"Anonymous\"", "Accessscope")
        );
      var logconnection = new Connection(
        createUserBlock.Id,
        logUserBlock.Id,
        "Log info",
        "true",
        new Map("output.Name", "Name"),//issue with lists and multiple fields of same type, but with different mappings
        new Map("output.Id", "Id")
        );

      feature = BpnFeature
        .CreateNew(
        name: "Test diagram",
        nodes: [entryBlock, createUserBlock, logUserBlock],
        connections: [connection, logconnection],
        targetEnvironments: [BpnFeature.Environment.Development, BpnFeature.Environment.Testing])
        .NewRevision("me");
      BpnRepository.Add(feature);

      feature = feature.NewRevision("System");

      diagram = new BpnFeatureDiagram();
      diagram.FeatureVersion = feature.Version;
      diagram.FeatureId = feature.Id;
      //unneeded? diagram.BpnConnectionWaypoints
      diagram.BpnPositions.Add(new BpnFeatureDiagram.BpnPosition(entryBlock.Id, new BpnFeatureDiagram.Position(50, 50)));
      diagram.BpnPositions.Add(new BpnFeatureDiagram.BpnPosition(createUserBlock.Id, new BpnFeatureDiagram.Position(300, 50)));
      diagram.BpnPositions.Add(new BpnFeatureDiagram.BpnPosition(logUserBlock.Id, new BpnFeatureDiagram.Position(550, 50)));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
