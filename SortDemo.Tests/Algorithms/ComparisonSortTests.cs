using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.Tests.Algorithms;

/// <summary>
/// Tests all comparison-based sorting algorithms with identical test cases.
/// Each algorithm is tested for correctness, edge cases, step callbacks, and cancellation.
/// </summary>
public class ComparisonSortTests
{
    /// <summary>All comparison-based sort algorithm instances.</summary>
    public static IEnumerable<object[]> AllAlgorithms()
    {
        yield return new object[] { new BubbleSort() };
        yield return new object[] { new SelectionSort() };
        yield return new object[] { new InsertionSort() };
        yield return new object[] { new QuickSort() };
        yield return new object[] { new MergeSort() };
        yield return new object[] { new HeapSort() };
        yield return new object[] { new ShellSort() };
        yield return new object[] { new CocktailShakerSort() };
        yield return new object[] { new CombSort() };
        yield return new object[] { new GnomeSort() };
        yield return new object[] { new TimSort() };
    }

    /// <summary>Cross-product of algorithms x test arrays.</summary>
    public static IEnumerable<object[]> AlgorithmsWithArrays()
    {
        foreach (var algo in AllAlgorithms())
        foreach (var arr in SortTestHelper.StandardTestArrays())
            yield return new object[] { algo[0], (int[])((int[])arr[0]).Clone(), (string)arr[1] };
    }

    // ---------------------------------------------------------------
    // Correctness: ascending
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AlgorithmsWithArrays))]
    public async Task SortAsync_Ascending_ProducesCorrectResult(
        ISortAlgorithm algorithm, int[] input, string label)
    {
        await SortTestHelper.AssertSorts(algorithm, input, ascending: true);
    }

    // ---------------------------------------------------------------
    // Correctness: descending
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AlgorithmsWithArrays))]
    public async Task SortAsync_Descending_ProducesCorrectResult(
        ISortAlgorithm algorithm, int[] input, string label)
    {
        await SortTestHelper.AssertSorts(algorithm, input, ascending: false);
    }

    // ---------------------------------------------------------------
    // Step callbacks are invoked
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_InvokesOnStepCallbacks(ISortAlgorithm algorithm)
    {
        int[] array = { 5, 3, 1, 4, 2 };
        var steps = new List<StepInfo>();

        await algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        Assert.NotEmpty(steps);
        // Every algorithm should emit at least one Compare or Write step
        Assert.Contains(steps, s => s.Kind == StepKind.Compare || s.Kind == StepKind.Write);
    }

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_EmitsMarkSorted_ForNonTrivialArrays(ISortAlgorithm algorithm)
    {
        int[] array = { 3, 1, 2 };
        var steps = new List<StepInfo>();

        await algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        Assert.Contains(steps, s => s.Kind == StepKind.MarkSorted);
    }

    // ---------------------------------------------------------------
    // Cancellation
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_ThrowsWhenCancelled(ISortAlgorithm algorithm)
    {
        // Use a large enough array that cancellation will fire mid-sort
        int[] array = Enumerable.Range(0, 200).Reverse().ToArray();
        using var cts = new CancellationTokenSource();

        int stepCount = 0;
        Func<StepInfo, Task> cancelAfterFewSteps = _ =>
        {
            if (++stepCount >= 5)
                cts.Cancel();
            return Task.CompletedTask;
        };

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => algorithm.SortAsync(array, cancelAfterFewSteps, cts.Token));
    }

    // ---------------------------------------------------------------
    // Empty and single-element arrays complete without error
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_EmptyArray_CompletesSuccessfully(ISortAlgorithm algorithm)
    {
        int[] array = Array.Empty<int>();
        await algorithm.SortAsync(array, SortTestHelper.NoOpStep, CancellationToken.None);
        Assert.Empty(array);
    }

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_SingleElement_RemainsUnchanged(ISortAlgorithm algorithm)
    {
        int[] array = { 7 };
        await algorithm.SortAsync(array, SortTestHelper.NoOpStep, CancellationToken.None);
        Assert.Equal(new[] { 7 }, array);
    }

    // ---------------------------------------------------------------
    // Metadata properties are populated
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public void Algorithm_HasNonEmptyMetadata(ISortAlgorithm algorithm)
    {
        Assert.False(string.IsNullOrWhiteSpace(algorithm.Name));
        Assert.False(string.IsNullOrWhiteSpace(algorithm.Description));
        Assert.False(string.IsNullOrWhiteSpace(algorithm.TimeComplexity));
    }

    // ---------------------------------------------------------------
    // Large random array stress test
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_LargeRandomArray_SortsCorrectly(ISortAlgorithm algorithm)
    {
        var rng = new Random(42); // deterministic seed
        int[] array = Enumerable.Range(0, 100).Select(_ => rng.Next(0, 1000)).ToArray();

        await SortTestHelper.AssertSorts(algorithm, array, ascending: true);
    }

    // ---------------------------------------------------------------
    // Already-sorted array (ascending) is handled efficiently
    // ---------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_AlreadySortedAscending_RemainsCorrect(ISortAlgorithm algorithm)
    {
        int[] array = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        await SortTestHelper.AssertSorts(algorithm, array, ascending: true);
    }

    [Theory]
    [MemberData(nameof(AllAlgorithms))]
    public async Task SortAsync_AlreadySortedDescending_RemainsCorrect(ISortAlgorithm algorithm)
    {
        int[] array = { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        await SortTestHelper.AssertSorts(algorithm, array, ascending: false);
    }
}
