namespace BigFileSort.Domain;

public static class ParseContextExtensions
{
    public static void AddLineToIndex(this ParseContext parseContext, string text, int number)
    {
        if (parseContext.TargetIndex.TryGetValue(text, out var numbers))
        {
            numbers.Add(number);
        }
        else
        {
            parseContext.TargetIndex.Add(text, new List<int>(4) { number });
        }
    }
}