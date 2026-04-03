using SortDemo.Models;

namespace SortDemo.Algorithms;

public class QuickSort : ISortAlgorithm
{
    public string Name => "Quick Sort";
    public string Description => "Picks a pivot element and partitions the array so that elements less than the pivot come before it and elements greater come after. Recursively sorts the partitions. Very fast in practice despite O(n\u00B2) worst case.";
    public string TimeComplexity => "Best: O(n log n) | Avg: O(n log n) | Worst: O(n\u00B2)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        if (array.Length <= 1) return;
        await QuickSortRecursive(array, 0, array.Length - 1, onStep, ct, ascending);

        for (int i = 0; i < array.Length; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }

    private async Task QuickSortRecursive(int[] array, int low, int high,
        Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending)
    {
        if (low >= high) return;

        int pivotIdx = await Partition(array, low, high, onStep, ct, ascending);
        await QuickSortRecursive(array, low, pivotIdx - 1, onStep, ct, ascending);
        await QuickSortRecursive(array, pivotIdx + 1, high, onStep, ct, ascending);
    }

    private async Task<int> Partition(int[] array, int low, int high,
        Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending)
    {
        int pivot = array[high];
        await onStep(new StepInfo(StepKind.SetPivot, high));
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, j, high));

            bool shouldMove = ascending ? array[j] < pivot : array[j] > pivot;
            if (shouldMove)
            {
                i++;
                (array[i], array[j]) = (array[j], array[i]);
                await onStep(new StepInfo(StepKind.Swap, i, j));
            }
        }

        (array[i + 1], array[high]) = (array[high], array[i + 1]);
        await onStep(new StepInfo(StepKind.Swap, i + 1, high));
        return i + 1;
    }
}
