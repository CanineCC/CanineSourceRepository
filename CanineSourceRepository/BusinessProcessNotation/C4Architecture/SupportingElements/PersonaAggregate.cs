using System.ComponentModel.DataAnnotations;

namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public class PersonaAggregate(
    Guid id,
    string name,
    string description,
    Scope scope)
{//TODO: CreateFeature, DeleteFeature, Update (name, description,scope) Feature, AddConsumeComponentFeature (add to ConsumeComponents), RemoveConsumeComponentFeature
    //Update C4 diagram (maybe own diagram?, or other way to show on component level?)
    
    public record RelationToComponent(Guid ComponentId, string Description);
    //RelationFromSystem ?
    [Required] public Guid Id { get; internal set; } = id;
    [Required] public string Name { get; internal set; } = name;
    [Required] public string Description { get; internal set; } = description;
    [Required] public Scope Scope { get; internal set;} = scope;
    public List<RelationToComponent> ConsumeComponents { get; } = new List<RelationToComponent>();
    // public record RelationToSystem(Guid SystemId, string Description);// udledt ?
    //  public record RelationToContainer(Guid ContainerId, string Description);// udledt ?
}