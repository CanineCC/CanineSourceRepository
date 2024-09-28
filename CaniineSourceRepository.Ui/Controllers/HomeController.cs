using CanineSourceRepository.BusinessProcessNotation;
using CanineSourceRepository.BusinessProcessNotation.Context;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature;
using CanineSourceRepository.BusinessProcessNotation.Context.Feature.Task;
using CanineSourceRepository.Ui.Models;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using static CanineSourceRepository.BusinessProcessNotation.Context.Feature.BpnFeatureProjection;

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
    public IActionResult BusinessProcessNotation([FromServices] IDocumentSession session)
    {
      //TODO: versions!
      var diagrams = session.Query<BpnFeatureProjection.BpnFeature>().Select(d => new { id = d.Id, name = d.Versions.Last().Name }).ToList();

      ViewBag.Diagrams = JsonSerializer.Serialize(diagrams);
      return View();
    }

    


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
