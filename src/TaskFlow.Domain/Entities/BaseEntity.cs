
namespace TaskFlow.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.Now;
}
