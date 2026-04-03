using SortDemo.Models;

namespace SortDemo.Algorithms;

public class CombSort : ISortAlgorithm
{
    public string Name => "Comb Sort";
    public string Description => "An improvement over bubble sort that uses a shrinking gap (divided by 1.3 each pass) to compare and swap elements far apart. This eliminates small values at the end (\"turtles\") early, then finishes with a standard bubble sort pass.";
    public string TimeComplexity => "Best: O(n log n) | Avg: O(n\u00B2/2\u1D56) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        int gap = n;
        double shrink = 1.3;
        bool sorted = false;

        while (!sorted)
        {
            gap = (int)(gap / shrink);
            if (gap <= 1)
            {
                gap = 1;
                sorted = true;
            }

            for (int i = 0; i + gap < n; i++)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, i, i + gap));

                if (SortHelper.ShouldSwap(array[i], array[i + gap], ascending))
                {
                    (array[i], array[i + gap]) = (array[i + gap], array[i]);
                    await onStep(new StepInfo(StepKind.Swap, i, i + gap));
                    sorted = false;
                }
            }
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }
}
