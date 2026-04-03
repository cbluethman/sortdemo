using SortDemo.Models;

namespace SortDemo.Algorithms;

public class CocktailShakerSort : ISortAlgorithm
{
    public string Name => "Cocktail Shaker Sort";
    public string Description => "A bidirectional variation of bubble sort that alternates between forward and backward passes. This helps move elements in both directions, reducing the \"turtle\" problem where small elements at the end move slowly.";
    public string TimeComplexity => "Best: O(n) | Avg: O(n\u00B2) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        bool swapped = true;
        int start = 0;
        int end = n - 1;

        while (swapped)
        {
            swapped = false;

            // Forward pass
            for (int i = start; i < end; i++)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, i, i + 1));

                if (SortHelper.ShouldSwap(array[i], array[i + 1], ascending))
                {
                    (array[i], array[i + 1]) = (array[i + 1], array[i]);
                    await onStep(new StepInfo(StepKind.Swap, i, i + 1));
                    swapped = true;
                }
            }
            await onStep(new StepInfo(StepKind.MarkSorted, end));
            end--;

            if (!swapped) break;
            swapped = false;

            // Backward pass
            for (int i = end; i > start; i--)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, i, i - 1));

                if (SortHelper.ShouldSwap(array[i - 1], array[i], ascending))
                {
                    (array[i], array[i - 1]) = (array[i - 1], array[i]);
                    await onStep(new StepInfo(StepKind.Swap, i, i - 1));
                    swapped = true;
                }
            }
            await onStep(new StepInfo(StepKind.MarkSorted, start));
            start++;
        }

        for (int i = start; i <= end; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }
}
