using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level2_Container;

public class C4ContainerDiagram : ContainerDiagram
{
    private readonly SystemProjection.BpnSystem _system;
    private readonly SystemProjection.ContainerDetails[] _containers;

    public C4ContainerDiagram(SystemProjection.BpnSystem system, SystemProjection.ContainerDetails[] containers)
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

            foreach (var container in _containers)
            {
                containers.Add(Container.None | (ContainerType.WebApplication, container.Name.ToPascalCase(), container.Name, "C#, WebApi", container.Description));
                foreach (var persona in container.Personas)
                {
                    structures.Add(Person.None | Boundary.Internal  | (persona.Name.ToPascalCase(), persona.Name, persona.Description));
                }
            }
            structures.Add(Bound(_system.Name.ToPascalCase(), _system.Name, containers.ToArray()));
            //TODO: External systems
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
            var relationships = new List<Relationship>();

            foreach (var container in _containers)
            {

                foreach (var persona in container.Personas)
                {
                    relationships.Add(this[persona.Name.ToPascalCase()] > this[container.Name.ToPascalCase()] |
                                      persona.RelationToContainer);
                }
            }



            //TODO: relations between containers (using events?)
            return relationships.ToArray();
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