using SortDemo.Models;

namespace SortDemo.Algorithms;

public class MergeSort : ISortAlgorithm
{
    public string Name => "Merge Sort";
    public string Description => "A divide-and-conquer algorithm that splits the array in half, recursively sorts each half, then merges the sorted halves back together. Guarantees O(n log n) time but requires additional memory for the merge step.";
    public string TimeComplexity => "Best: O(n log n) | Avg: O(n log n) | Worst: O(n log n)";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        if (array.Length <= 1) return;
        await MergeSortRecursive(array, 0, array.Length - 1, onStep, ct, ascending);

        for (int i = 0; i < array.Length; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }

    private async Task MergeSortRecursive(int[] array, int left, int right,
        Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending)
    {
        if (left >= right) return;

        int mid = left + (right - left) / 2;
        await MergeSortRecursive(array, left, mid, onStep, ct, ascending);
        await MergeSortRecursive(array, mid + 1, right, onStep, ct, ascending);
        await Merge(array, left, mid, right, onStep, ct, ascending);
    }

    private async Task Merge(int[] array, int left, int mid, int right,
        Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending)
    {
        int[] temp = new int[right - left + 1];
        int i = left, j = mid + 1, k = 0;

        while (i <= mid && j <= right)
        {
            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, i, j));

            if (!SortHelper.ShouldSwap(array[i], array[j], ascending))
                temp[k++] = array[i++];
            else
                temp[k++] = array[j++];
        }

        while (i <= mid) temp[k++] = array[i++];
        while (j <= right) temp[k++] = array[j++];

        for (int idx = 0; idx < temp.Length; idx++)
        {
            ct.ThrowIfCancellationRequested();
            array[left + idx] = temp[idx];
            await onStep(new StepInfo(StepKind.Write, left + idx));
        }
    }
}
