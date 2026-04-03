using SortDemo.Models;

namespace SortDemo.Algorithms;

public class InsertionSort : ISortAlgorithm
{
    public string Name => "Insertion Sort";
    public string Description => "Builds the sorted array one element at a time by picking each element and inserting it into its correct position among the already-sorted elements. Very efficient for small or nearly-sorted datasets.";
    public string TimeComplexity => "Best: O(n) | Avg: O(n\u00B2) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        for (int i = 1; i < n; i++)
        {
            int key = array[i];
            int j = i - 1;

            await onStep(new StepInfo(StepKind.SetPivot, i));

            while (j >= 0)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, j, j + 1));

                if (SortHelper.ShouldSwap(array[j], key, ascending))
                {
                    array[j + 1] = array[j];
                    await onStep(new StepInfo(StepKind.Swap, j, j + 1));
                    j--;
                }
                else
                {
                    break;
                }
            }
            array[j + 1] = key;
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }
}
