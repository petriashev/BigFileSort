using BigFileSort.System;

namespace BigFileSort.Parsing;

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
            parseContext.TargetIndex.Add(text, new List<int>(64) { number });
        }
    }
    
    public static void AddLineToIndex(this ParseContext parseContext, VirtualString text, int number)
    {
        var targetIndex = parseContext.VirtualTargetIndex;

        if (!parseContext.Command.Configuration.Sort.UseMultithreading)
        {
            if (targetIndex.TryGetValue(text, out var numbers))
            {
                numbers.Add(number);
                parseContext.Metrics.SetNumbersCount(numbers.Count);
            }
            else
            {
                targetIndex.TryAdd(text, new List<int>(parseContext.Metrics.MaxNumbersPerString) { number });
            }
        }
        else
        {
            if (targetIndex.TryGetValue(text, out var numbers))
            {
                lock (numbers)
                {
                    numbers.Add(number);
                }
            
                parseContext.Metrics.SetNumbersCount(numbers.Count);
            }
            else
            {
                var isAdded = targetIndex.TryAdd(text, new List<int>(parseContext.Metrics.MaxNumbersPerString) { number });
                if (!isAdded)
                {
                    AddLineToIndex(parseContext, text, number);
                }
            }
        }
    }
}