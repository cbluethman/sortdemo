using SortDemo.Models;

namespace SortDemo.Algorithms;

public class TimSort : ISortAlgorithm
{
    public string Name => "Tim Sort";
    public string Description => "A hybrid algorithm combining insertion sort and merge sort. Divides the array into small runs, sorts each with insertion sort, then merges runs together. Designed for real-world data and used as the default sort in Python and Java.";
    public string TimeComplexity => "Best: O(n) | Avg: O(n log n) | Worst: O(n log n)";
    private const int Run = 32;

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;

        for (int i = 0; i < n; i += Run)
        {
            await InsertionSortRun(array, i, Math.Min(i + Run - 1, n - 1), onStep, ct, ascending);
        }

        for (int size = Run; size < n; size *= 2)
        {
            for (int left = 0; left < n; left += 2 * size)
            {
                int mid = Math.Min(left + size - 1, n - 1);
                int right = Math.Min(left + 2 * size - 1, n - 1);

                if (mid < right)
                    await Merge(array, left, mid, right, onStep, ct, ascending);
            }
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }

    private async Task InsertionSortRun(int[] array, int left, int right,
        Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending)
    {
        for (int i = left + 1; i <= right; i++)
        {
            int key = array[i];
            int j = i - 1;

            await onStep(new StepInfo(StepKind.SetPivot, i));

            while (j >= left)
            {
                ct.ThrowIfCancellationRequested();
                await onStep(new StepInfo(StepKind.Compare, j, j + 1));

                if (SortHelper.ShouldSwap(array[j], key, ascending))
                {
                    array[j + 1] = array[j];
                    await onStep(new StepInfo(StepKind.Swap, j, j + 1));
                    j--;
                }
                else break;
            }
            array[j + 1] = key;
        }
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
