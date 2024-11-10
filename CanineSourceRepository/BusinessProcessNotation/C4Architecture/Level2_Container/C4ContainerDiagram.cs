using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level2_Container;

public class C4ContainerDiagram : ContainerDiagram
{
    private readonly BpnSystemProjection.BpnSystem _system;
    private readonly BpnSystemProjection.ContextDetails[] _containers;

    public C4ContainerDiagram(BpnSystemProjection.BpnSystem system, BpnSystemProjection.ContextDetails[] containers)
    {
        _system = system;
        _containers = containers;
    }
    protected override string Title => _system.Name;

    protected override IEnumerable<Structure> Structures
    {
        get
        {
            List<Structure> structures = new List<Structure>();
            List<Container> containers = new List<Container>();

            structures.Add(Person.None | Boundary.External | ("User", "TODO-user", "Todo-user description."));
            //TODO: External systems
            foreach (var container in _containers)
            {
                containers.Add(Container.None | (ContainerType.WebApplication, container.Name.ToPascalCase(), container.Name, "C#, WebApi", container.Description));
            }
            structures.Add(Bound("c1", _system.Name, containers.ToArray()));
            //TODO:: INTERNALSERVICES:      Container.None | (ContainerType.Database, "SqlDatabase", "SqlDatabase", "SQL Database", "Stores user registration information, hashed auth credentials, access logs, etc."),
            //TODO:: INTERNALSERVICES:      Container.None | (ContainerType.Queue, "RabbitMQ", "RabbitMQ", "RabbitMQ", "Stores user registration information, hashed auth credentials, access logs, etc."),
            
            return structures;
        }
    } 
    //TODO: Service calls to internal systems:   SoftwareSystem.None | ("BankingSystem", "Internet Banking System", "Allows customers to view information about their bank accounts, and make payments."),
    //TODO: Service calls to external systems:   SoftwareSystem.None | Boundary.External | ("MailSystem", "E-mail system", "The internal Microsoft Exchange e-mail system."),

    protected override IEnumerable<Relationship> Relationships
    {
        get
        {//TODO: relations between containers and services
            //TODO: relations between containers (using events?)
            return [];
            /*{
        this["Customer"] > this["WebApp"] | ("Uses", "HTTPS"),
        this["Customer"] > this["Spa"] | ("Uses", "HTTPS"),
        this["Customer"] > this["MobileApp"] | "Uses",

        this["WebApp"] > this["Spa"] | "Delivers" | Position.Neighbor,
        this["Spa"] > this["BackendApi"] | ("Uses", "async, JSON/HTTPS"),
        this["MobileApp"] > this["BackendApi"] | ("Uses", "async, JSON/HTTPS"),
        this["SqlDatabase"] < this["BackendApi"] | ("Uses", "async, JSON/HTTPS") | Position.Neighbor,
        this["RabbitMQ"] < this["BackendApi"] | ("Uses", "async, JSON"),

        this["Customer"] < this["MailSystem"] | "Sends e-mails to",
        this["MailSystem"] < this["BackendApi"] | ("Sends e-mails using", "sync, SMTP"),
        this["BackendApi"] > this["BankingSystem"] | ("Uses", "sync/async, XML/HTTPS") | Position.Neighbor
      }*/
        }
    }
}