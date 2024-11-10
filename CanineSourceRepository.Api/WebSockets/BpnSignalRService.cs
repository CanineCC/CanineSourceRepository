using Npgsql;
using System.Net.Sockets;
using Microsoft.AspNetCore.SignalR;

public class BpnSignalRService : IHostedService
{
  private readonly IHubContext<BpnHub> _hubContext;
  private Timer _timer;
  private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(150);
  private readonly string _connectionString;

  public BpnSignalRService(IHubContext<BpnHub> hubContext, IConfiguration configuration)
  {
    _hubContext = hubContext;
    _connectionString = new NpgsqlConnectionStringBuilder
    {
      Host = configuration.GetSection("Marten:Host").Value,
      Port = int.Parse(configuration.GetSection("Marten:Port").Value!),
      Database = configuration.GetSection("Marten:Database").Value,
      Username = configuration.GetSection("Marten:Username").Value,
      Password = configuration.GetSection("Marten:Password").Value,
      MaxPoolSize = int.Parse(configuration.GetSection("Marten:MaxPoolSize").Value!),
      Pooling = bool.Parse(configuration.GetSection("Marten:Pooling").Value!)
    }.ConnectionString;
  }
  public async Task CreateOrReplaceGetProjectionDataStoredProcedure(string connectionString, CancellationToken cancellationToken)
  {
    await using var tenantConnection = new NpgsqlConnection(connectionString);
    await tenantConnection.OpenAsync(cancellationToken);
    var createFunctionSql = @"
        CREATE OR REPLACE FUNCTION get_projection_data(last_fetched TIMESTAMPTZ) 
        RETURNS TABLE(id UUID, resource_name text, mt_last_modified TIMESTAMPTZ) AS $$
        DECLARE
            tbl record;
            sql text := '';
        BEGIN
            -- Build the dynamic SQL with UNION ALL for each matching table
            FOR tbl IN
                SELECT t.table_name,
                       regexp_replace(t.table_name, '.*_', '') AS resource_name
                FROM information_schema.tables AS t
                JOIN information_schema.columns AS c
                  ON t.table_name = c.table_name
                WHERE t.table_schema = 'public'
                  AND t.table_name LIKE 'mt_doc_%'
                  AND t.table_name LIKE '%projection_%'
                  AND c.column_name = 'mt_version'
                  AND c.data_type = 'integer'
            LOOP
                -- Append each SELECT statement to the sql variable with a WHERE clause for mt_last_modified
                sql := sql || format(
                    'SELECT id, ''%s'' AS resource_name, mt_last_modified FROM public.%I WHERE mt_last_modified > %L UNION ALL ',
                    tbl.resource_name, tbl.table_name, last_fetched
                );
            END LOOP;

            -- Trim the last UNION ALL if any SQL was generated
            IF sql <> '' THEN
                sql := left(sql, length(sql) - 11);  -- Remove last UNION ALL
            ELSE
                RETURN; -- No tables found, return an empty set
            END IF;

            -- Return the result of the dynamically generated SQL
            RETURN QUERY EXECUTE sql;
        END $$ LANGUAGE plpgsql;
        ";

    // Execute the SQL command to create the function
    await using var command = tenantConnection.CreateCommand();
    command.CommandText = createFunctionSql;
    await command.ExecuteNonQueryAsync(cancellationToken);
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    // Start the timer when the service starts
    await CreateOrReplaceGetProjectionDataStoredProcedure(_connectionString, cancellationToken);
    _timer = new Timer(async _ => await CheckForUpdates(cancellationToken), null, TimeSpan.Zero, _pollingInterval);
  }
  private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

  private async Task CheckForUpdates(CancellationToken cancellationToken)
  {
    await _semaphore.WaitAsync(cancellationToken);
    try
    {
      await SendNewEvents(cancellationToken);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
    finally { _semaphore.Release(); }
  }
  private DateTimeOffset _lastUpdated = DateTimeOffset.Now;
  private record GetProjectionData(Guid Id, string Name, DateTimeOffset LastModified);
  private async Task SendNewEvents(CancellationToken ct)
  {
    try
    {
      await using var conn = new NpgsqlConnection(_connectionString);
      await conn.OpenAsync(ct);

      await using var cmd = new NpgsqlCommand(@"SELECT * FROM get_projection_data(@lastModified) order by mt_last_modified asc", conn);
      cmd.Parameters.AddWithValue("lastModified", _lastUpdated.ToUniversalTime());

      await using var reader = await cmd.ExecuteReaderAsync(ct);

      // Iterate over the result set
      while (await reader.ReadAsync(ct))
      {
        var data = new GetProjectionData(
          Id: await reader.GetFieldValueAsync<Guid>(0, ct),
          Name: await reader.GetFieldValueAsync<string>(1, ct),
          LastModified: await reader.GetFieldValueAsync<DateTimeOffset>(2, ct)
          );

        _lastUpdated = data.LastModified;
        await _hubContext.Clients.Group($"{data.Name}-{data.Id}")
            .SendAsync("onEntityUpdate", data, ct);

        await _hubContext.Clients.Group($"{data.Name}")
            .SendAsync("onGroupUpdate", data, ct);

      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    // Dispose of the timer when the service stops
    _timer?.Change(Timeout.Infinite, 0);
    _timer?.Dispose();
    return Task.CompletedTask;
  }
}