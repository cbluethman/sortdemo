using SortDemo.Models;

namespace SortDemo.Algorithms;

public class SelectionSort : ISortAlgorithm
{
    public string Name => "Selection Sort";
    public string Description => "Divides the array into sorted and unsorted regions. Repeatedly finds the minimum (or maximum) element from the unsorted region and moves it to the end of the sorted region. Simple but always O(n\u00B2) comparisons.";
    public string TimeComplexity => "Best: O(n\u00B2) | Avg: O(n\u00B2) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        for (int i = 0; i < n - 1; i++)
        {
            int bestIdx = i;
            await onStep(new StepInfo(StepKind.SetPivot, i));

            for (int j = i + 1; j < n; j++)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, bestIdx, j));

                if (SortHelper.ShouldSwap(array[bestIdx], array[j], ascending))
                    bestIdx = j;
            }

            if (bestIdx != i)
            {
                (array[i], array[bestIdx]) = (array[bestIdx], array[i]);
                await onStep(new StepInfo(StepKind.Swap, i, bestIdx));
            }

            await onStep(new StepInfo(StepKind.MarkSorted, i));
        }
        await onStep(new StepInfo(StepKind.MarkSorted, n - 1));
    }
}
