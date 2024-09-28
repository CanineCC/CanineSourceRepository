using CanineSourceRepository.BusinessProcessNotation.Context;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task.Snippets;
using CanineSourceRepository.BusinessProcessNotation.Engine;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;
using static CanineSourceRepository.BusinessProcessNotation.Engine.FeatureInvocationProjection;

namespace CanineSourceRepository.Ui.Controllers
{
  public record UiModel(Guid FeatureId, BpnFeatureVersion Feature, BpnFeatureDiagram Diagram, FeatureInvocation? LastRun);

  //[Authorize]
  public class BusinessProcessNotationController : Controller
  {

    [HttpPost]
    public async Task<IActionResult> GetSnippetsForCodeBlock()
    {
      var json = await (new StreamReader(Request.Body).ReadToEndAsync());
      var fromClient = JsonSerializer.Deserialize<CodeTask>(json, BpnEngine.bpnJsonSerialize)!;

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
      var fromClient = JsonSerializer.Deserialize<CodeTask>(json, BpnEngine.bpnJsonSerialize)!;

      var res = fromClient.VerifyCode();
      if (res.success)
        return Ok();

      return BadRequest(res.errors);
    }


    [HttpPost]
    public async Task<IActionResult> Save([FromServices] IDocumentSession session, CancellationToken ct)
    {
      var json = await (new StreamReader(Request.Body).ReadToEndAsync());
      var fromClient = JsonSerializer.Deserialize<UiModel>(json, BpnEngine.bpnJsonSerialize)!;

      //TODO: this releases the feature... we also need to update the draft in seperate api call
      await BpnEventStore.ReleaseFeature(session, "BusinessController/Save", fromClient.FeatureId, "joe", ct);
      //var newFeature = fromClient.Feature; //TODO::: .NewRevision("todo-user");
      //var newDiagram = fromClient.Diagram;

      ////TODO: verify....
      ////TODO: run tests...
      //BpnFeatureRepository.Add(newFeature);
      //BpnFeatureRepository.Save();

      //newDiagram.FeatureVersion = newFeature.Revision;
      ////TODO: validate...
      //BpnDiagramRepository.Add(newDiagram);
      //BpnDiagramRepository.Save();

      //new revision...
      //var lastRun = session.Query<FeatureInvocation>().Where(p => p.FeatureId == newFeature.Id).OrderByDescending(p => p.StarTime).FirstOrDefault();
      return PartialView("Index", fromClient); // new UiModel(newFeature, newDiagram, lastRun));
    }

    //TODO: Version!!! [OutputCache(Duration = int.MaxValue, VaryByQueryKeys =["id"])]
    public IActionResult Index(Guid id, [FromServices] IDocumentSession session)//htmx
    {
      var lastRun = session.Query<FeatureInvocation>().Where(p=>p.FeatureId == id).OrderByDescending(p=>p.StarTime).FirstOrDefault();
      var feature = session.Query<BpnFeatureProjection.BpnFeature>().First(p => p.Id == id);
      return PartialView("Index", new UiModel(id, feature.Versions.Last(), BpnDiagramRepository.Load(id), lastRun));
    }


  }
}
