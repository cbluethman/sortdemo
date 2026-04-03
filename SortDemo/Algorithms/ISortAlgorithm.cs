using SortDemo.Models;

namespace SortDemo.Algorithms;

public interface ISortAlgorithm
{
    string Name { get; }
    string Description { get; }
    string TimeComplexity { get; }
    Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true);
}

public static class SortHelper
{
    /// <summary>Returns true if a should come before b given the sort direction.</summary>
    public static bool ShouldSwap(int a, int b, bool ascending)
        => ascending ? a > b : a < b;
}
