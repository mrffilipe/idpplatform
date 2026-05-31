namespace Kyvo.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void SetCreatedAt()
    {
        var utcNow = DateTime.UtcNow;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
