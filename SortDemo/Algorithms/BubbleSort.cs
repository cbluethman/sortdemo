using SortDemo.Models;

namespace SortDemo.Algorithms;

public class BubbleSort : ISortAlgorithm
{
    public string Name => "Bubble Sort";
    public string Description => "Repeatedly steps through the list, compares adjacent elements, and swaps them if they are in the wrong order. The pass through the list is repeated until the list is sorted. Simple but inefficient for large datasets.";
    public string TimeComplexity => "Best: O(n) | Avg: O(n\u00B2) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, j, j + 1));

                if (SortHelper.ShouldSwap(array[j], array[j + 1], ascending))
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    await onStep(new StepInfo(StepKind.Swap, j, j + 1));
                    swapped = true;
                }
            }
            await onStep(new StepInfo(StepKind.MarkSorted, n - 1 - i));
            if (!swapped) break;
        }
        await onStep(new StepInfo(StepKind.MarkSorted, 0));
    }
}
