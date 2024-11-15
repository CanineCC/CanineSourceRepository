namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public class PersonaAggregate
{//TODO: CreateFeature, DeleteFeature, Update (name, description,scope) Feature, AddConsumeComponentFeature (add to ConsumeComponents), RemoveConsumeComponentFeature
    //Update C4 diagram (maybe own diagram?, or other way to show on component level?)
    
    
    public void Apply(
        PersonaAggregate aggregate,
        AddPersonaFeature.PersonaCreated @event
    )
    {
        aggregate.Id = @event.PersonaId;
        aggregate.Name = @event.Name;
        aggregate.Description = @event.Description;
        aggregate.Scope = @event.Scope;
    }
    
    public record RelationToComponent(Guid ComponentId, string Description);
    [Required] public Guid Id { get; internal set; } 
    [Required] public string Name { get; internal set; } 
    [Required] public string Description { get; internal set; } 
    [Required] public Scope Scope { get; internal set;} 
    public List<RelationToComponent> ConsumeComponents { get; } = new List<RelationToComponent>();
    // public record RelationToSystem(Guid SystemId, string Description);// udledt ?
    //  public record RelationToContainer(Guid ContainerId, string Description);// udledt ?
}