using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.NamedConfigurationFeatures;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public class NamedConfigurationAggregate
{
    [Required] public Guid Id { get; internal set; } 
    [Required] public Guid SystemId { get; internal set; }
    [Required] public Guid ServiceTypeId { get; internal set; }
    [Required] public string Name { get; internal set; }
    [Required] public string Description { get; internal set; }
    [Required] public Scope Scope { get; internal set; } 
    [Required] public Dictionary<string,string> Configuration { get; internal set; } 


    public void Apply(
        NamedConfigurationAggregate aggregate,
        AddNamedConfigurationFeature.NamedConfigurationAdded @event
    )
    {
        aggregate.Id = @event.NamedConfigurationId;
        aggregate.SystemId = @event.SystemId;
        aggregate.ServiceTypeId = @event.ServiceTypeId;
        aggregate.Name = @event.Name;
        aggregate.Description = @event.Description;
        aggregate.Configuration = @event.Configuration;
        aggregate.Scope = @event.Scope;
    }
    
    //TODO: Create an SQL configuration in default data
    //Help for code (tables+columns in db, resources/endpoints in rest, etc.) <--Maybe call/scan and provide it?
    
    
    

    //--> Tabels ?
    //REST: BaseUrl, Authentication APIKey | OAuth | Basic, Headers
    //--> Endpoints?
}


public class NamedConfigurationProjection : SingleStreamProjection<NamedConfigurationProjection.NamedConfiguration>
{
    public static void RegisterBpnEventStore(WebApplication app)
    {
        app.MapGet($"BpnEngine/v1/NamedConfiguration/All", async (HttpContext context, [FromServices] IQuerySession session, CancellationToken ct) =>
            {
                var bpnContexts = await session.Query<NamedConfiguration>().ToListAsync(ct);
                return Results.Ok(bpnContexts);
            }).WithName("GetAllNamedConfigurations")
            .Produces(StatusCodes.Status200OK, typeof(List<NamedConfiguration>))
            .WithTags("NamedConfiguration");
    }
    public static void Apply(NamedConfiguration view, IEvent<RemoveNamedConfigurationFeature.NamedConfigurationRemoved> @event)
    {
        //TODO
    }
    public static void Apply(NamedConfiguration view, IEvent<UpdateNamedConfigurationFeature.NamedConfigurationUpdated> @event)
    {
        view.Name = @event.Data.Name;
        view.Description = @event.Data.Description;
        view.Scope = @event.Data.Scope;
        view.Configuration = @event.Data.Configuration;
    }

    public static void Apply(NamedConfiguration view, IEvent<AddNamedConfigurationFeature.NamedConfigurationAdded> @event)
    {
        view.Id = @event.Data.NamedConfigurationId;
        view.SystemId = @event.Data.SystemId;
        view.ServiceType = ServiceType.ServiceTypes.First(p=>p.Id == @event.Data.ServiceTypeId);
        view.Name = @event.Data.Name;
        view.Description = @event.Data.Description;
        view.Scope = @event.Data.Scope;
        view.Configuration = @event.Data.Configuration;
    }
    public class NamedConfiguration
    {
        [Required] public Guid Id { get; set; }
        [Required] public Guid SystemId { get; set; }
        [Required] public ServiceType ServiceType { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; }
        [Required] public Scope Scope { get; set; }
        [Required] public Dictionary<string,string> Configuration { get; set; }
    }
  
}