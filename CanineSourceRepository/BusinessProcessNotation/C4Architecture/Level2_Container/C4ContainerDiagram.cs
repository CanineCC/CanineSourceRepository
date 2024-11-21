using C4Sharp.Diagrams.Builders;
using C4Sharp.Elements;
using C4Sharp.Elements.Containers;
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

                foreach (var task in container.TasksWithService)
                {
                    var service = ServiceType.ServiceTypes.First(p => p.Id == task.ServiceDependencyId);
                    containers.Add(Container.None | (ContainerType.Database, task.NamedConfigurationName.ToPascalCase(), task.NamedConfigurationName, service.InjectedComponent.Name));
                }
            }
            structures.Add(Bound(_system.Name.ToPascalCase(), _system.Name, containers.ToArray()));
            
            return structures;
        }
    } 
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
                foreach (var task in container.TasksWithService)
                {
                    relationships.Add(this[task.NamedConfigurationName.ToPascalCase()] < this[container.Name.ToPascalCase()] | "uses");
                }
            }
            return relationships.ToArray();
        }
    }
}