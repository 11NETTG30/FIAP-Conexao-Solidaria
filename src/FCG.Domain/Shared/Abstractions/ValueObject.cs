namespace FCG.Domain.Shared.Abstractions;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> ObterComponentesDeIgualdade();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        ValueObject outroObjeto = (ValueObject)obj;

        return ObterComponentesDeIgualdade()
            .SequenceEqual(outroObjeto.ObterComponentesDeIgualdade());
    }

    public override int GetHashCode()
    {
        return ObterComponentesDeIgualdade()
            .Aggregate(0, (hash, component) =>
            {
                return HashCode.Combine(hash, component);
            });
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}