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
    
    public static void AddLineToIndex(this ParseContext parseContext, VirtualString text, int number)
    {
        var targetIndex = parseContext.VirtualTargetIndex;
   
        if (targetIndex.TryGetValue(text, out var numbers))
        {
            numbers.Add(number);
        }
        else
        {
            targetIndex.Add(text, new List<int>(64) { number });
        }
    }
}