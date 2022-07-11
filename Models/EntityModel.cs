namespace InscricaoChll.Api.Models;

public abstract class EntityModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class EntityIntIdModel : EntityModel
{
    public new long Id { get; set; }
}