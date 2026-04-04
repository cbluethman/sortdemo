using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.Tests.Algorithms;

/// <summary>
/// RadixSort (LSD) requires non-negative integer values, so it gets its own test class.
/// </summary>
public class RadixSortTests
{
    private readonly RadixSort _algorithm = new();

    public static IEnumerable<object[]> TestArrays() => SortTestHelper.NonNegativeTestArrays();

    [Theory]
    [MemberData(nameof(TestArrays))]
    public async Task SortAsync_Ascending_ProducesCorrectResult(int[] input, string label)
    {
        await SortTestHelper.AssertSorts(_algorithm, input, ascending: true);
    }

    [Theory]
    [MemberData(nameof(TestArrays))]
    public async Task SortAsync_Descending_ProducesCorrectResult(int[] input, string label)
    {
        await SortTestHelper.AssertSorts(_algorithm, input, ascending: false);
    }

    [Fact]
    public async Task SortAsync_EmptyArray_CompletesSuccessfully()
    {
        int[] array = Array.Empty<int>();
        await _algorithm.SortAsync(array, SortTestHelper.NoOpStep, CancellationToken.None);
        Assert.Empty(array);
    }

    [Fact]
    public async Task SortAsync_SingleElement_RemainsUnchanged()
    {
        int[] array = { 7 };
        await _algorithm.SortAsync(array, SortTestHelper.NoOpStep, CancellationToken.None);
        Assert.Equal(new[] { 7 }, array);
    }

    [Fact]
    public async Task SortAsync_InvokesOnStepCallbacks()
    {
        int[] array = { 5, 3, 1, 4, 2 };
        var steps = new List<StepInfo>();

        await _algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        Assert.NotEmpty(steps);
        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
        Assert.Contains(steps, s => s.Kind == StepKind.Write);
        Assert.Contains(steps, s => s.Kind == StepKind.MarkSorted);
    }

    [Fact]
    public async Task SortAsync_ThrowsWhenCancelled()
    {
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
            () => _algorithm.SortAsync(array, cancelAfterFewSteps, cts.Token));
    }

    [Fact]
    public async Task SortAsync_MultiDigitNumbers_SortsCorrectly()
    {
        int[] array = { 170, 45, 75, 90, 802, 24, 2, 66 };
        await SortTestHelper.AssertSorts(_algorithm, array, ascending: true);
    }

    [Fact]
    public async Task SortAsync_Descending_ReversesResult()
    {
        int[] array = { 170, 45, 75, 90, 802, 24, 2, 66 };
        await SortTestHelper.AssertSorts(_algorithm, array, ascending: false);
    }

    [Fact]
    public async Task SortAsync_DescendingEmitsSwapSteps()
    {
        int[] array = { 3, 1, 2 };
        var steps = new List<StepInfo>();

        await _algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None, ascending: false);

        // Descending mode reverses the array at the end, which emits Swap steps
        Assert.Contains(steps, s => s.Kind == StepKind.Swap);
    }

    [Fact]
    public void Metadata_IsPopulated()
    {
        Assert.False(string.IsNullOrWhiteSpace(_algorithm.Name));
        Assert.False(string.IsNullOrWhiteSpace(_algorithm.Description));
        Assert.False(string.IsNullOrWhiteSpace(_algorithm.TimeComplexity));
    }
}
