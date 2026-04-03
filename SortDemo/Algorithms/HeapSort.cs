using SortDemo.Models;

namespace SortDemo.Algorithms;

public class HeapSort : ISortAlgorithm
{
    public string Name => "Heap Sort";
    public string Description => "Builds a max-heap from the array, then repeatedly extracts the maximum element and places it at the end. Uses the heap data structure to efficiently find the next largest element. In-place with guaranteed O(n log n).";
    public string TimeComplexity => "Best: O(n log n) | Avg: O(n log n) | Worst: O(n log n)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;

        // Build heap
        for (int i = n / 2 - 1; i >= 0; i--)
            await Heapify(array, n, i, onStep, ct, ascending);

        // Extract elements
        for (int i = n - 1; i > 0; i--)
        {
            ct.ThrowIfCancellationRequested();
            (array[0], array[i]) = (array[i], array[0]);
            await onStep(new StepInfo(StepKind.Swap, 0, i));
            await onStep(new StepInfo(StepKind.MarkSorted, i));
            await Heapify(array, i, 0, onStep, ct, ascending);
        }
        await onStep(new StepInfo(StepKind.MarkSorted, 0));
    }

    private async Task Heapify(int[] array, int n, int i,
        Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending)
    {
        int extreme = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        if (left < n)
        {
            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, extreme, left));
            if (SortHelper.ShouldSwap(array[extreme], array[left], !ascending))
                extreme = left;
        }

        if (right < n)
        {
            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, extreme, right));
            if (SortHelper.ShouldSwap(array[extreme], array[right], !ascending))
                extreme = right;
        }

        if (extreme != i)
        {
            (array[i], array[extreme]) = (array[extreme], array[i]);
            await onStep(new StepInfo(StepKind.Swap, i, extreme));
            await Heapify(array, n, extreme, onStep, ct, ascending);
        }
    }
}
