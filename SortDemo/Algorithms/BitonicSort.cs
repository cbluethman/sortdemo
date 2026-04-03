using SortDemo.Models;

namespace SortDemo.Algorithms;

public class BitonicSort : ISortAlgorithm
{
    public string Name => "Bitonic Sort";
    public string Description => "A parallel-friendly sorting network that first creates bitonic sequences (sequences that first increase then decrease), then merges them. Designed for parallel hardware \u2014 all comparisons at each stage are independent.";
    public string TimeComplexity => "O(n log\u00B2 n) comparisons";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n <= 1) return;
        int size = 1;
        while (size < n) size <<= 1;

        // Pad to power of 2 with sentinel values
        int sentinel = ascending ? int.MaxValue : int.MinValue;
        int[] padded;
        if (size != n)
        {
            padded = new int[size];
            Array.Copy(array, padded, n);
            for (int i = n; i < size; i++)
                padded[i] = sentinel;
        }
        else
        {
            padded = array;
        }

        // Bitonic sort network on padded array
        for (int k = 2; k <= size; k *= 2)
        {
            for (int j = k / 2; j > 0; j /= 2)
            {
                for (int i = 0; i < size; i++)
                {
                    int l = i ^ j;
                    if (l > i)
                    {
                        ct.ThrowIfCancellationRequested();

                        // Only visualize indices within original array bounds
                        if (i < n && l < n)
                            await onStep(new StepInfo(StepKind.Compare, i, l));

                        bool asc = ((i & k) == 0) == ascending;
                        if ((asc && padded[i] > padded[l]) || (!asc && padded[i] < padded[l]))
                        {
                            (padded[i], padded[l]) = (padded[l], padded[i]);

                            if (i < n && l < n)
                                await onStep(new StepInfo(StepKind.Swap, i, l));
                        }
                    }
                }
            }
        }

        // Copy results back if we padded
        if (padded != array)
        {
            for (int i = 0; i < n; i++)
            {
                array[i] = padded[i];
                await onStep(new StepInfo(StepKind.Write, i));
            }
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }
}
