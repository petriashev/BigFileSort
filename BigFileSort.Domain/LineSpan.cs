namespace BigFileSort.Domain;

public readonly ref struct LineSpan
{
    public SpanString Number { get; }
    
    public SpanString Text { get; }

    public LineSpan(ReadOnlySpan<char> number, ReadOnlySpan<char> text)
    {
        Number = new SpanString(number);
        Text = new SpanString(text);
    }
    
    public static bool operator < (LineSpan line1, LineSpan line2) => SpanComparer.Compare(line1.Text.Span, line2.Text.Span) < 0;

    public static bool operator > (LineSpan line1, LineSpan line2) => SpanComparer.Compare(line1.Text.Span, line2.Text.Span) > 0;
    
    public static bool operator == (LineSpan line1, LineSpan line2) => SpanComparer.Compare(line1.Text.Span, line2.Text.Span) == 0;

    public static bool operator !=(LineSpan line1, LineSpan line2) => !(line1 == line2);

    public ReadOnlySpan<char> AsSpan()
    {
        return Number;
    }
}

public readonly ref struct SpanString
{
    public ReadOnlySpan<char> Span { get; }

    public SpanString(ReadOnlySpan<char> span) => Span = span;
    
    public static implicit operator ReadOnlySpan<char>(SpanString value) => value.Span;
    
    public static bool operator < (SpanString line1, SpanString line2) => SpanComparer.Compare(line1.Span, line2.Span) < 0;

    public static bool operator > (SpanString line1, SpanString line2) => SpanComparer.Compare(line1.Span, line2.Span) > 0;
    
    public static bool operator == (SpanString line1, SpanString line2) => SpanComparer.Compare(line1.Span, line2.Span) == 0;

    public static bool operator !=(SpanString line1, SpanString line2) => !(line1 == line2);
}

public static class SpanComparer
{
    public static int Compare(in ReadOnlySpan<char> span1, in ReadOnlySpan<char> span2)
    {
        if (span1.Length == 0 && span2.Length == 0) return 0;
        int max = Math.Min(span1.Length, span2.Length);
        for (int i = 0; i < max; i++)
        {
            if (span1[i] < span2[i])
                return -1;
            if (span1[i] > span2[i])
                return 1;
        }
        return span1.Length.CompareTo(span2.Length);
    }
}