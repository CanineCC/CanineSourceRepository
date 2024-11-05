namespace CanineSourceRepository.BusinessProcessNotation.BpnEventStore;

public static class LifetimeService
{
    public const string WaitingForPort = "Waiting for port to be free";
    //TODO: Release API... so we dont restart every time a feature is released!
    //TODO: What about updating the envoriments (if prod is selected, or is that just a header? - issue if not on same server)
    private static bool StartNewInstance()
    {
        var newProcessStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = Assembly.GetEntryAssembly()!.Location,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        try
        {
            var newProcess = Process.Start(newProcessStartInfo);
            if (newProcess == null) return false;
            using (StreamReader reader = newProcess.StandardOutput)
            {
                string? outputLine;
                while ((outputLine = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"New process output: {outputLine}"); // Log output for debugging
                    if (outputLine.Contains(LifetimeService.WaitingForPort))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    private static void ShutdownThisInstance(IHostApplicationLifetime lifetime)
    {
        lifetime.StopApplication();//Gracefull shutdown - IF CancellationToken have ben properly implemented throughout the system
    }

    public static void Restart(this IHostApplicationLifetime lifetime)
    {
        //do some domain logic, that updates the database, which is used when the process is started to dynamically enable endpoints and document via openapi

        if (StartNewInstance())
        {
            ShutdownThisInstance(lifetime);
        }
    }

}