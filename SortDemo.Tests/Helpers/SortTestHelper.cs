using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.Tests.Helpers;

/// <summary>
/// Shared test infrastructure for all sorting algorithm tests.
/// Provides standard test arrays, a no-op step callback, and assertion helpers.
/// </summary>
public static class SortTestHelper
{
    /// <summary>A step callback that does nothing, used when we only care about the final result.</summary>
    public static readonly Func<StepInfo, Task> NoOpStep = _ => Task.CompletedTask;

    /// <summary>A step callback that records all steps for later inspection.</summary>
    public static Func<StepInfo, Task> RecordingStep(List<StepInfo> steps)
        => step => { steps.Add(step); return Task.CompletedTask; };

    /// <summary>Runs a sort algorithm and asserts the array is correctly sorted.</summary>
    public static async Task AssertSorts(ISortAlgorithm algorithm, int[] input, bool ascending = true)
    {
        int[] expected = (int[])input.Clone();
        Array.Sort(expected);
        if (!ascending)
            Array.Reverse(expected);

        await algorithm.SortAsync(input, NoOpStep, CancellationToken.None, ascending);

        Assert.Equal(expected, input);
    }

    /// <summary>Standard test arrays covering common edge cases.</summary>
    public static IEnumerable<object[]> StandardTestArrays()
    {
        yield return new object[] { Array.Empty<int>(), "empty" };
        yield return new object[] { new[] { 42 }, "single element" };
        yield return new object[] { new[] { 1, 2, 3, 4, 5 }, "already sorted" };
        yield return new object[] { new[] { 5, 4, 3, 2, 1 }, "reverse sorted" };
        yield return new object[] { new[] { 3, 3, 3, 3, 3 }, "all duplicates" };
        yield return new object[] { new[] { 5, 1, 4, 2, 8 }, "random small" };
        yield return new object[] { new[] { 10, 7, 8, 9, 1, 5 }, "random medium" };
        yield return new object[] { new[] { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 }, "with duplicates" };
        yield return new object[] { new[] { 1, 2 }, "two elements sorted" };
        yield return new object[] { new[] { 2, 1 }, "two elements unsorted" };
    }

    /// <summary>
    /// Test arrays that only contain non-negative values.
    /// Required for CountingSort and RadixSort which assume non-negative integers.
    /// </summary>
    public static IEnumerable<object[]> NonNegativeTestArrays()
    {
        yield return new object[] { Array.Empty<int>(), "empty" };
        yield return new object[] { new[] { 42 }, "single element" };
        yield return new object[] { new[] { 1, 2, 3, 4, 5 }, "already sorted" };
        yield return new object[] { new[] { 5, 4, 3, 2, 1 }, "reverse sorted" };
        yield return new object[] { new[] { 3, 3, 3, 3, 3 }, "all duplicates" };
        yield return new object[] { new[] { 5, 1, 4, 2, 8 }, "random small" };
        yield return new object[] { new[] { 10, 7, 8, 9, 1, 5 }, "random medium" };
        yield return new object[] { new[] { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 }, "with duplicates" };
        yield return new object[] { new[] { 0, 0, 0, 1, 0 }, "with zeros" };
        yield return new object[] { new[] { 100, 50, 200, 150 }, "larger values" };
    }
}
