namespace BigFileSort.Model;

public readonly struct LineValue
{
    public readonly int Number;
    public readonly string Text;
    
    public LineValue(int number, string text)
    {
        Number = number;
        Text = text;
    }
    
    /// <inheritdoc />
    public override string ToString() => $"{Number}. {Text}";
    
    public static bool operator < (LineValue line1, LineValue line2) => string.Compare(line1.Text, line2.Text, StringComparison.OrdinalIgnoreCase) < 0;

    public static bool operator > (LineValue line1, LineValue line2) => string.Compare(line1.Text, line2.Text, StringComparison.OrdinalIgnoreCase) > 0;
}