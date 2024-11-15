using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public class NamedConfigurationAggregate(Guid id, Guid systemId, Guid serviceTypeId, string name, string description, Scope scope, Dictionary<string,string> configuration)
{
    [Required] public Guid Id { get; internal set; } = id;
    [Required] public Guid SystemId { get; internal set; } = systemId;
    [Required] public Guid ServiceTypeId { get; internal set; } = serviceTypeId;
    [Required] public string Name { get; internal set; } = name;
    [Required] public string Description { get; internal set; } = description;
    [Required] public Scope Scope { get; } = scope;

    [Required] public Dictionary<string,string> Configuration { get; internal set; } = configuration;
    
    //TODO: AddNamedConfigurationFeature, also Remove and Update
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