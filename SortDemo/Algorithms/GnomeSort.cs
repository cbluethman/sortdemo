using SortDemo.Models;

namespace SortDemo.Algorithms;

public class GnomeSort : ISortAlgorithm
{
    public string Name => "Gnome Sort";
    public string Description => "Works like a garden gnome sorting flower pots: moves forward when the current element is in order, swaps and steps backward when it's not. Conceptually simple \u2014 similar to insertion sort but with no nested loops.";
    public string TimeComplexity => "Best: O(n) | Avg: O(n\u00B2) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        int i = 0;

        while (i < n)
        {
            if (i == 0) { i++; continue; }

            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, i, i - 1));

            if (!SortHelper.ShouldSwap(array[i - 1], array[i], ascending))
            {
                i++;
            }
            else
            {
                (array[i], array[i - 1]) = (array[i - 1], array[i]);
                await onStep(new StepInfo(StepKind.Swap, i, i - 1));
                i--;
            }
        }

        for (int j = 0; j < n; j++)
            await onStep(new StepInfo(StepKind.MarkSorted, j));
    }
}
