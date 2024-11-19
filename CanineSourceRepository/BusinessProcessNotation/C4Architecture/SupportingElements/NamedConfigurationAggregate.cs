using CanineSourceRepository.BusinessProcessNotation.BpnEventStore.Features.NamedConfigurationFeatures;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public class NamedConfigurationAggregate(Guid id, Guid systemId, Guid serviceTypeId, string name, string description, Scope scope, Dictionary<string,string> configuration)
{
    [Required] public Guid Id { get; internal set; } = id;
    [Required] public Guid SystemId { get; internal set; } = systemId;
    [Required] public Guid ServiceTypeId { get; internal set; } = serviceTypeId;
    [Required] public string Name { get; internal set; } = name;
    [Required] public string Description { get; internal set; } = description;
    [Required] public Scope Scope { get; internal set; } = scope;
    [Required] public Dictionary<string,string> Configuration { get; internal set; } = configuration;
    
    
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
    
    //IMAP: ServerAddress,Port,SecurityType SSL/TLS | STARTTLS, AuthenticationMethod Password | OAuth2, Username, Password
    //SMTP: ServerAddress,Port,SecurityType SSL/TLS | STARTTLS, AuthenticationMethod Password | OAuth2, Username, Password
    //POP3: ServerAddress,Port,SecurityType SSL/TLS | STARTTLS, AuthenticationMethod Password | OAuth2, Username, Password
    //SQL: Hostname, Port, Databasename, Username, password
    //--> Tabels ?
    //REST: BaseUrl, Authentication APIKey | OAuth | Basic, Headers
    //--> Endpoints?
    
    
}

//TODO Projection...
//AddNamedConfigurationFeature.NamedConfigurationAdded
//RemoveNamedConfigurationFeature.NamedConfigurationRemoved
//UpdateNamedConfigurationFeature.NamedConfigurationUpdated