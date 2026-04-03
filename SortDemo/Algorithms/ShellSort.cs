using SortDemo.Models;

namespace SortDemo.Algorithms;

public class ShellSort : ISortAlgorithm
{
    public string Name => "Shell Sort";
    public string Description => "An optimization of insertion sort that compares elements separated by a gap, progressively reducing the gap until it becomes 1. Allows elements to move long distances quickly in early passes, making the final insertion sort pass very efficient.";
    public string TimeComplexity => "Best: O(n log n) | Avg: O(n\u00B3\u02F2) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;

        for (int gap = n / 2; gap > 0; gap /= 2)
        {
            for (int i = gap; i < n; i++)
            {
                int temp = array[i];
                int j = i;

                while (j >= gap)
                {
                    ct.ThrowIfCancellationRequested();
                    await onStep(new StepInfo(StepKind.Compare, j - gap, j));

                    if (SortHelper.ShouldSwap(array[j - gap], temp, ascending))
                    {
                        array[j] = array[j - gap];
                        await onStep(new StepInfo(StepKind.Swap, j, j - gap));
                        j -= gap;
                    }
                    else
                    {
                        break;
                    }
                }
                array[j] = temp;
            }
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }
}
