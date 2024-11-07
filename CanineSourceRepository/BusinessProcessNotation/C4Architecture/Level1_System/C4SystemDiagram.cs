using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;
using Position = C4Sharp.Elements.Relationships.Position;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

public class ContainerDiagramSample : ContextDiagram
{
    protected override string Title => "Container diagram for Internet Banking System";

    protected override IEnumerable<Structure> Structures => new Structure[]
    {
        Person.None | Boundary.External 
                    | ("Customer", "Personal Banking Customer", "A customer of the bank, with personal bank accounts."),
        /*
        SoftwareSystem.None | ("BankingSystem", "Internet Banking System", 
            "Allows customers to view information about their bank accounts, and make payments."),
        
        SoftwareSystem.None | Boundary.External 
                            | ("MailSystem", "E-mail system", "The internal Microsoft Exchange e-mail system."),
        */
        Bound("c1", "Internet Banking",
            Container.None | (ContainerType.WebApplication, "Usersystem".ToPascalCase(), "User system", "C#, WebApi", 
                "Delivers the static content and the Internet banking SPA") /*
            Container.None | (ContainerType.Spa, "Spa", "Spa", "JavaScript, Angular",
                "Delivers the static content and the Internet banking SPA"),

            Container.None | (ContainerType.Mobile, "MobileApp", "Mobile App", "C#, Xamarin",
                "Provides a mobile banking experience"),

            Container.None | (ContainerType.Database, "SqlDatabase", "SqlDatabase", "SQL Database",
                "Stores user registration information, hashed auth credentials, access logs, etc."),

            Container.None | (ContainerType.Queue, "RabbitMQ", "RabbitMQ", "RabbitMQ",
                "Stores user registration information, hashed auth credentials, access logs, etc."),

            Container.None | (ContainerType.Api, "BackendApi", "BackendApi", "Dotnet, Docker Container",
                "Provides Internet banking functionality via API.")*/
        )
    };

    protected override IEnumerable<Relationship> Relationships => new[]
    {
        this["Customer"] > this["Usersystem"] | ("Uses", "HTTPS"),
       /* this["Customer"] > this["Spa"] | ("Uses", "HTTPS"),
        this["Customer"] > this["MobileApp"] | "Uses",
        
        this["WebApp"] > this["Spa"] | "Delivers" | Position.Neighbor,
        this["Spa"] > this["BackendApi"] | ("Uses", "async, JSON/HTTPS"),
        this["MobileApp"] > this["BackendApi"] | ("Uses", "async, JSON/HTTPS"),
        this["SqlDatabase"] < this["BackendApi"] | ("Uses", "async, JSON/HTTPS") | Position.Neighbor,
        this["RabbitMQ"] < this["BackendApi"] | ("Uses", "async, JSON"),
        
        this["Customer"] < this["MailSystem"] | "Sends e-mails to",
        this["MailSystem"] < this["BackendApi"] | ("Sends e-mails using", "sync, SMTP"),
        this["BackendApi"] > this["BankingSystem"] | ("Uses", "sync/async, XML/HTTPS") | Position.Neighbor*/
    };
}
public class C4SystemDiagram : ContainerDiagram
{
    //list of systems... and an external entity interacting with them
    private readonly BpnSystemProjection.BpnSystem[] _systems;
    private readonly string _solutionName;

    public C4SystemDiagram(string solutionName, BpnSystemProjection.BpnSystem[] systems)
    {
        _systems = systems;
        _solutionName = solutionName;
    }
    protected override string Title => _solutionName;

    protected override IEnumerable<Structure> Structures {
        get
        {
            List<Structure> structures = new List<Structure>();

            structures.Add(Person.None | Boundary.External | ("User", "TODO - dunno - TODO", "A xxx (user) of the yyyy (system/entity), with zzzz (resource?)."));
            foreach (var system in _systems)
            {
                structures.Add(SoftwareSystem.None | (system.Name.ToPascalCase(), system.Name,system.Description));
                //TODO: HOW.... SoftwareSystem.None | Boundary.External | ("MailSystem", "E-mail system", "The internal Microsoft Exchange e-mail system."),
            }
            return structures;
        }
    }

    protected override IEnumerable<Relationship> Relationships
    {
        get
        {
            
            //todo find relations between internal and external systems (calls / events)
            return [
                    this["User"] < this[_systems.First().Name.ToPascalCase()] | "uses",
                ];/* new[]
      {
        this["Customer"] < this["MailSystem"] | "Sends e-mails to",
        this["MailSystem"] < this["BackendApi"] | ("Sends e-mails using", "sync, SMTP"),
        this["BackendApi"] > this["BankingSystem"] | ("Uses", "sync/async, XML/HTTPS") | Position.Neighbor
      };*/
        }
    }
}