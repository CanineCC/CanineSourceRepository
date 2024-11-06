using C4Sharp.Diagrams.Interfaces;
using C4Sharp.Diagrams.Plantuml;
using C4Sharp.Diagrams.Themes;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture;

public static class C4DiagramHelper
{
    public static string GenerateC4(IDiagramBuilder diagram)//this BpnSystemProjection.BpnSystem system)
    {
        var diagrams = new IDiagramBuilder[] { diagram };

        var workfolder = $"c4tmp/{Guid.NewGuid()}";
        try
        {
            new PlantumlContext()
                .UseDiagramSvgImageBuilder()
                .Export(workfolder, diagrams, new DefaultTheme());
            var outputfiles = System.IO.Directory.GetFiles(workfolder, "*.svg");
            if (outputfiles.Any())
            {
                return File.ReadAllText(outputfiles.First());
            }
        }
        finally
        {    
            System.IO.Directory.Delete(workfolder,true);
        }

        return string.Empty;
    }
}