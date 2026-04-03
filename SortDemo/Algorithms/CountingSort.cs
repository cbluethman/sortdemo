using SortDemo.Models;

namespace SortDemo.Algorithms;

public class CountingSort : ISortAlgorithm
{
    public string Name => "Counting Sort";
    public string Description => "A non-comparison sort that counts the occurrences of each value, then uses those counts to place elements directly into their correct positions. Extremely fast when the range of values is small relative to the number of elements.";
    public string TimeComplexity => "O(n + k) where k = range of values";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n == 0) return;

        int max = array[0];
        for (int i = 1; i < n; i++)
            if (array[i] > max) max = array[i];

        int[] count = new int[max + 1];
        int[] output = new int[n];

        // Count occurrences
        for (int i = 0; i < n; i++)
        {
            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, i));
            count[array[i]]++;
        }

        // Cumulative count
        for (int i = 1; i <= max; i++)
            count[i] += count[i - 1];

        // Build output
        for (int i = n - 1; i >= 0; i--)
        {
            output[count[array[i]] - 1] = array[i];
            count[array[i]]--;
        }

        // Copy back (reversed if descending)
        for (int i = 0; i < n; i++)
        {
            ct.ThrowIfCancellationRequested();
            array[i] = ascending ? output[i] : output[n - 1 - i];
            await onStep(new StepInfo(StepKind.Write, i));
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }
}
