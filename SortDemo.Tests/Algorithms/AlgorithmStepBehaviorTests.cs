using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.Tests.Algorithms;

/// <summary>
/// Tests that verify each algorithm emits the expected types of step callbacks.
/// Different algorithms use different step patterns (some emit SetPivot, some use Write, etc.).
/// </summary>
public class AlgorithmStepBehaviorTests
{
    private async Task<List<StepInfo>> RunAndCollectSteps(ISortAlgorithm algorithm, int[] array)
    {
        var steps = new List<StepInfo>();
        await algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);
        return steps;
    }

    [Fact]
    public async Task BubbleSort_EmitsCompareAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new BubbleSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
        Assert.Contains(steps, s => s.Kind == StepKind.MarkSorted);
        // BubbleSort does not use SetPivot or Write
        Assert.DoesNotContain(steps, s => s.Kind == StepKind.SetPivot);
        Assert.DoesNotContain(steps, s => s.Kind == StepKind.Write);
    }

    [Fact]
    public async Task BubbleSort_AlreadySorted_NoSwapSteps()
    {
        var steps = await RunAndCollectSteps(new BubbleSort(), new[] { 1, 2, 3 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.DoesNotContain(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public async Task SelectionSort_EmitsSetPivotSteps()
    {
        var steps = await RunAndCollectSteps(new SelectionSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.SetPivot);
        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
    }

    [Fact]
    public async Task InsertionSort_EmitsSetPivotSteps()
    {
        var steps = await RunAndCollectSteps(new InsertionSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.SetPivot);
    }

    [Fact]
    public async Task QuickSort_EmitsSetPivotAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new QuickSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.SetPivot);
        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public async Task MergeSort_EmitsWriteSteps()
    {
        var steps = await RunAndCollectSteps(new MergeSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Write);
    }

    [Fact]
    public async Task HeapSort_EmitsCompareAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new HeapSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public async Task TimSort_EmitsSetPivotAndWriteSteps()
    {
        // Use an array larger than the Run size (32) to trigger merge behavior
        var rng = new Random(42);
        int[] array = Enumerable.Range(0, 64).Select(_ => rng.Next(0, 100)).ToArray();
        var steps = await RunAndCollectSteps(new TimSort(), array);

        Assert.Contains(steps, s => s.Kind == StepKind.SetPivot);
        Assert.Contains(steps, s => s.Kind == StepKind.Write);
    }

    [Fact]
    public async Task CocktailShakerSort_EmitsCompareAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new CocktailShakerSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public async Task GnomeSort_EmitsCompareAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new GnomeSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public async Task CombSort_EmitsCompareAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new CombSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public async Task ShellSort_EmitsCompareAndSwapSteps()
    {
        var steps = await RunAndCollectSteps(new ShellSort(), new[] { 3, 1, 2 });

        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    // ---------------------------------------------------------------
    // Verify step indices are within array bounds
    // ---------------------------------------------------------------

    [Theory]
    [InlineData(typeof(BubbleSort))]
    [InlineData(typeof(SelectionSort))]
    [InlineData(typeof(InsertionSort))]
    [InlineData(typeof(QuickSort))]
    [InlineData(typeof(MergeSort))]
    [InlineData(typeof(HeapSort))]
    [InlineData(typeof(ShellSort))]
    [InlineData(typeof(CocktailShakerSort))]
    [InlineData(typeof(CombSort))]
    [InlineData(typeof(GnomeSort))]
    [InlineData(typeof(TimSort))]
    [InlineData(typeof(BitonicSort))]
    [InlineData(typeof(CountingSort))]
    [InlineData(typeof(RadixSort))]
    public async Task AllAlgorithms_StepIndicesAreWithinBounds(Type algorithmType)
    {
        var algorithm = (ISortAlgorithm)Activator.CreateInstance(algorithmType)!;
        int[] array = { 5, 3, 8, 1, 9, 2, 7, 4, 6 };
        int n = array.Length;
        var steps = new List<StepInfo>();

        await algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        foreach (var step in steps)
        {
            foreach (int index in step.Indices)
            {
                Assert.InRange(index, 0, n - 1);
            }
        }
    }
}
