using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Relationships;
using Position = C4Sharp.Elements.Relationships.Position;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.Level1_System;

public class C4SystemDiagram : ContainerDiagram
{
    //list of systems... and an external entity interacting with them
    private readonly SolutionProjection.SystemDetails[] _systems;
    private readonly string _solutionName;

    public C4SystemDiagram(string solutionName, SolutionProjection.SystemDetails[] systems)
    {
        _systems = systems;
        _solutionName = solutionName;
    }
    protected override string Title => _solutionName;

    protected override IEnumerable<Structure> Structures {
        get
        {
            List<Structure> structures = new List<Structure>();

            foreach (var system in _systems)
            {
                foreach (var persona in system.Personas)
                {
                    structures.Add(Person.None | Boundary.Internal | (persona.Name.ToPascalCase(), persona.Name, persona.Description));
                }
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
            var relationships = new List<Relationship>();
            //todo find relations between internal and external systems (calls / events)
            
            foreach (var system in _systems)
            {
                foreach (var persona in system.Personas)
                {
                    relationships.Add(this[persona.Name.ToPascalCase()] > this[system.Name.ToPascalCase()] | "uses");
                }
            }
            return relationships.ToArray();
      /*{
        this["Customer"] < this["MailSystem"] | "Sends e-mails to",
        this["MailSystem"] < this["BackendApi"] | ("Sends e-mails using", "sync, SMTP"),
        this["BackendApi"] > this["BankingSystem"] | ("Uses", "sync/async, XML/HTTPS") | Position.Neighbor
      };*/
        }
    }
}