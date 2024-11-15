namespace CanineSourceRepository.BusinessProcessNotation.C4Architecture.SupportingElements;

public record ServiceType(Guid Id, string Name, string Type)
{
    public Guid Id { get; } = Id;
    public string Name { get; } = Name;
    public string Type { get; } = Type;

    public static ServiceType[] ServiceTypes => new[]
    {
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50001"),
            Name: "Marten",
            Type: "EventSourcing"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50002"),
            Name: "Postgres",
            Type: "Database"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50002"),
            Name: "RabbitMq",
            Type: "Message queue"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50003"),
            Name: "IMAP",
            Type: "Email"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50004"),
            Name: "POP3",
            Type: "Email"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50005"),
            Name: "SMTP",
            Type: "Email"),
        new ServiceType(
            Id: Guid.Parse("152ff325-fa01-4882-85ca-6af9a8e50006"),
            Name: "REST client",
            Type: "API")
    };
    
    //TODO: Feature that returns AllServiceTypes, and ServiceTypesAvailableInSystem (filter out any wo configuration)
}