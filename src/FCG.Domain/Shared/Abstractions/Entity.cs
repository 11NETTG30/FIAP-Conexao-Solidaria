namespace FCG.Domain.Shared.Abstractions;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    public override bool Equals(object? obj)
    {
        Entity? compareTo = obj as Entity;

        if (ReferenceEquals(this, compareTo))
            return true;
    
        if (ReferenceEquals(null, compareTo))
            return false;
    
        return Id.Equals(compareTo.Id);
    }

    public static bool operator ==(Entity a, Entity b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            return true;
    
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            return false;
    
        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}