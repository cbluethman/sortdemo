using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.Tests.Algorithms;

/// <summary>
/// BitonicSort pads to power-of-2 internally, so it works with any array size
/// but has unique internal behavior worth testing separately.
/// </summary>
public class BitonicSortTests
{
    private readonly BitonicSort _algorithm = new();

    public static IEnumerable<object[]> TestArrays() => SortTestHelper.StandardTestArrays();

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
    public async Task SortAsync_PowerOfTwoSize_SortsCorrectly()
    {
        int[] array = { 8, 3, 7, 1, 5, 2, 6, 4 }; // exactly 8 elements
        await SortTestHelper.AssertSorts(_algorithm, array, ascending: true);
    }

    [Fact]
    public async Task SortAsync_NonPowerOfTwoSize_SortsCorrectly()
    {
        int[] array = { 5, 3, 7, 1, 6 }; // 5 elements, needs padding to 8
        await SortTestHelper.AssertSorts(_algorithm, array, ascending: true);
    }

    [Fact]
    public async Task SortAsync_EmitsWriteSteps_WhenPaddingIsNeeded()
    {
        int[] array = { 5, 3, 7 }; // 3 elements, padded to 4
        var steps = new List<StepInfo>();

        await _algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        // When array size is not a power of 2, results are copied back with Write steps
        Assert.Contains(steps, s => s.Kind == StepKind.Write);
    }

    [Fact]
    public async Task SortAsync_PowerOfTwo_NoWriteStepsForCopyBack()
    {
        // When size IS a power of 2, the padded array IS the original array,
        // so no copy-back Write steps are emitted (only Compare/Swap/MarkSorted)
        int[] array = { 4, 2, 3, 1 }; // exactly 4 elements
        var steps = new List<StepInfo>();

        await _algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        // Verify sorted
        Assert.Equal(new[] { 1, 2, 3, 4 }, array);
        // No Write steps for copy-back when no padding was needed
        Assert.DoesNotContain(steps, s => s.Kind == StepKind.Write);
    }

    [Fact]
    public async Task SortAsync_InvokesOnStepCallbacks()
    {
        int[] array = { 5, 3, 1, 4, 2 };
        var steps = new List<StepInfo>();

        await _algorithm.SortAsync(array, SortTestHelper.RecordingStep(steps), CancellationToken.None);

        Assert.NotEmpty(steps);
        Assert.Contains(steps, s => s.Kind == StepKind.Compare);
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
    public async Task SortAsync_LargeRandomArray_SortsCorrectly()
    {
        var rng = new Random(42);
        int[] array = Enumerable.Range(0, 100).Select(_ => rng.Next(0, 1000)).ToArray();

        await SortTestHelper.AssertSorts(_algorithm, array, ascending: true);
    }

    [Fact]
    public void Metadata_IsPopulated()
    {
        Assert.False(string.IsNullOrWhiteSpace(_algorithm.Name));
        Assert.False(string.IsNullOrWhiteSpace(_algorithm.Description));
        Assert.False(string.IsNullOrWhiteSpace(_algorithm.TimeComplexity));
    }
}
