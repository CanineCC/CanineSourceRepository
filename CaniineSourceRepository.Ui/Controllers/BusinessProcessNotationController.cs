using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task.Snippets;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static CanineSourceRepository.BusinessProcessNotation.Engine.FeatureInvocationProjection;

namespace CanineSourceRepository.Ui.Controllers
{
  public record UiModel(BpnFeature Feature, BpnFeatureDiagram Diagram, FeatureInvocation? LastRun);

  //[Authorize]
  public class BusinessProcessNotationController : Controller
  {

    [HttpPost]
    public async Task<IActionResult> GetSnippetsForCodeBlock()
    {
      var json = await (new StreamReader(Request.Body).ReadToEndAsync());
      var fromClient = JsonSerializer.Deserialize<CodeBlock>(json, BpnFeatureRepository.bpnJsonSerialize)!;

      var input = fromClient.RecordTypes.FirstOrDefault(p => p.Name == fromClient.Input);
      var output = fromClient.RecordTypes.FirstOrDefault(p => p.Name == fromClient.Output);

      var snippets = new List<CodeSnippet>();
      if (input != null && output != null)
      {
        snippets.AddRange([
          new CodeSnippet("Auto construct output", AutoConstructorGenerator.GenerateMapping(input, output, fromClient.RecordTypes.ToArray())),
          new CodeSnippet("Auto mapper", AutoMapperGenerator.GenerateMapping(input, output, fromClient.RecordTypes.ToArray()))
        ]);
      }

      return Ok(snippets);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyCodeBlock()
    {
      var json = await (new StreamReader(Request.Body).ReadToEndAsync());
      var fromClient = JsonSerializer.Deserialize<CodeBlock>(json, BpnFeatureRepository.bpnJsonSerialize)!;

      var res = fromClient.VerifyCode();
      if (res.success)
        return Ok();

      return BadRequest(res.errors);
    }


    [HttpPost]
    public async Task<IActionResult> Save([FromServices] IDocumentSession session)
    {
      var json = await (new StreamReader(Request.Body).ReadToEndAsync());
      var fromClient = JsonSerializer.Deserialize<UiModel>(json, BpnFeatureRepository.bpnJsonSerialize)!;

      var newFeature = fromClient.Feature.NewRevision("todo-user");
      var newDiagram = fromClient.Diagram;

      //TODO: verify....
      //TODO: run tests...
      BpnFeatureRepository.Add(newFeature);
      BpnFeatureRepository.Save();

      newDiagram.FeatureVersion = newFeature.Version;
      //TODO: validate...
      BpnDiagramRepository.Add(newDiagram);
      BpnDiagramRepository.Save();

      //new revision...
      var lastRun = session.Query<FeatureInvocation>().Where(p => p.FeatureId == newFeature.Id).OrderByDescending(p => p.StarTime).FirstOrDefault();
      return PartialView("Index", new UiModel(newFeature, newDiagram, lastRun));
    }

    //TODO: Version!!! [OutputCache(Duration = int.MaxValue, VaryByQueryKeys =["id"])]
    public IActionResult Index(Guid id, [FromServices] IDocumentSession session)//htmx
    {
      var lastRun = session.Query<FeatureInvocation>().Where(p=>p.FeatureId == id).OrderByDescending(p=>p.StarTime).FirstOrDefault();
      return PartialView("Index", new UiModel(BpnFeatureRepository.Load(id), BpnDiagramRepository.Load(id), lastRun));
    }


  }
}
