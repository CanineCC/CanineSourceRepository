using CanineSourceRepository.BusinessProcessNotation.Engine;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public record ServiceType(Guid Id, ServiceInjection InjectedComponent, params string[] Keys)
{
    public Guid Id { get; } = Id;
    //public string Name { get; } = Name;
    public ServiceInjection InjectedComponent { get; } = InjectedComponent;
    //public string Type { get; } = Type;
    public string[] Keys { get; } = Keys;

    public static ServiceType[] ServiceTypes => new[]
    {
        //NoService,PostgreSqlService
        
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50001"),
            InjectedComponent:  new PostgreSqlService(),
            //InjectedComponent: "?TODO - MARTEN?",
            //Type: "EventSourcing",
            Keys: ["Host","Port", "Database", "Username","Password"]            
            ),
        new ServiceType(
            Id: Guid.Empty,
            InjectedComponent:  new NoService(),
            //InjectedComponent: "?TODO - MARTEN?",
            //Type: "EventSourcing",
            Keys: [""]            
        ),
        
        /*
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50002"),
            Name: "Postgres",
            InjectedComponent:"PostgreSqlService",
            Type: "Database",
            Keys: ["Host","Port", "Database", "Username","Password"]            
            ),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50002"),
            Name: "RabbitMq",
            InjectedComponent: "?TODO - RABBITMQ?",
            Type: "Message queue"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50003"),
            Name: "IMAP",
            Type: "Email",
            InjectedComponent: "?TODO - IMAP?",
            Keys: ["ServerAddress","Port", "SecurityType", "AuthenticationMethod","Username", "Password"]            
            ),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50004"),
            Name: "POP3",
            Type: "Email",
            InjectedComponent: "?TODO - POP3?",
            Keys: ["ServerAddress","Port", "SecurityType", "AuthenticationMethod","Username", "Password"]            
        ),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50005"),
            Name: "SMTP",
            Type: "Email",
            InjectedComponent: "?TODO - SMTP?",
            Keys: ["ServerAddress","Port", "SecurityType", "AuthenticationMethod","Username", "Password"]            
        ),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50006"),
            Name: "REST client",
            InjectedComponent: "?TODO - REST API?",
            Type: "API")*/
    };
    
    //TODO: Feature that returns AllServiceTypes, and ServiceTypesAvailableInSystem (filter out any wo configuration)
}