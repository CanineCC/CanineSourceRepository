using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;
using CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level3_Component;

public class C4ComponentDiagram : ComponentDiagram
{
    private readonly string _systemName;
    private readonly BpnBpnWebApiContainerProjection.BpnWebApiContainer _container;
    private readonly BpnBpnWebApiContainerProjection.FeatureDetails[] _components;
 //   private readonly BpnBpnWebApiContainerProjection.BpnWebApiContainer[] _containers;

    public C4ComponentDiagram(
        string systemName, 
        BpnBpnWebApiContainerProjection.BpnWebApiContainer container,
      //  BpnBpnWebApiContainerProjection.BpnWebApiContainer[] containers,
      BpnBpnWebApiContainerProjection.FeatureDetails[] components
        )
    {
        _systemName = systemName;
        _container = container;
        _components = components;
      //  _containers = containers;
    }
    protected override string Title => $"{_systemName} - {_container.Name}";

    protected override IEnumerable<Structure> Structures
    {//TODO: Also build in features/components
        get
        {//boundry around features in the "container"
         //include contains that communicates with features (via events? or direct calls?)
            List<Structure> structures = new List<Structure>();
            List<Component> components = new List<Component>();

            var c4container = Container.None | (ContainerType.WebApplication, _container.Name.ToPascalCase(), _container.Name,
                "C#, WebApi", _container.Description);
            structures.Add(c4container);
            
            foreach (var component in _components)
            {
                var newestVersion = component.Revisions.Last(); 
                components.Add(new (newestVersion.Name.ToPascalCase(), newestVersion.Name, "C#", newestVersion.Objective));
            }
            structures.Add(Bound("c1", _container.Name, components.ToArray()));
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