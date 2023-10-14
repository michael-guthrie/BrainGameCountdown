using System.Diagnostics.CodeAnalysis;

public struct WorkingNumber
{
    public int Number { get; set; }
    public string? Source { get; set; }

    public static bool operator ==(WorkingNumber left, WorkingNumber right) => left.Number == right.Number && left.Source == right.Source;
    public static bool operator !=(WorkingNumber left, WorkingNumber right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (!(obj is WorkingNumber))
        {
            return false;
        }

        WorkingNumber other = (WorkingNumber)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode() ^ (Source?.GetHashCode() ?? 0);
    }
}