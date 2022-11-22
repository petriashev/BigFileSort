namespace BigFileSort.Domain;

public record Line(string? Source, int Number, string Text)
{
    public override string ToString() => Source ?? string.Empty;

    public static bool operator < (Line line1, Line line2) => string.Compare(line1.Text, line2.Text, StringComparison.OrdinalIgnoreCase) < 0;

    public static bool operator > (Line line1, Line line2) => string.Compare(line1.Text, line2.Text, StringComparison.OrdinalIgnoreCase) > 0;

    public ReadOnlySpan<char> AsSpan() => Source;
}