using SortDemo.Models;

namespace SortDemo.Algorithms;

public class RadixSort : ISortAlgorithm
{
    public string Name => "Radix Sort (LSD)";
    public string Description => "A non-comparison sort that distributes elements into buckets based on individual digits, from least significant to most significant. Processes one digit position per pass using counting sort as a stable subroutine.";
    public string TimeComplexity => "O(d \u00D7 n) where d = number of digits";

    public async Task SortAsync(int[] array, Func<StepInfo, Task> onStep, CancellationToken ct, bool ascending = true)
    {
        int n = array.Length;
        if (n == 0) return;

        int max = array[0];
        for (int i = 1; i < n; i++)
            if (array[i] > max) max = array[i];

        for (int exp = 1; max / exp > 0; exp *= 10)
        {
            await CountingSortByDigit(array, n, exp, onStep, ct);
        }

        // Reverse if descending
        if (!ascending)
        {
            for (int i = 0, j = n - 1; i < j; i++, j--)
            {
                ct.ThrowIfCancellationRequested();
                (array[i], array[j]) = (array[j], array[i]);
                await onStep(new StepInfo(StepKind.Swap, i, j));
            }
        }

        for (int i = 0; i < n; i++)
            await onStep(new StepInfo(StepKind.MarkSorted, i));
    }

    private async Task CountingSortByDigit(int[] array, int n, int exp,
        Func<StepInfo, Task> onStep, CancellationToken ct)
    {
        int[] output = new int[n];
        int[] count = new int[10];

        for (int i = 0; i < n; i++)
        {
            ct.ThrowIfCancellationRequested();
            await onStep(new StepInfo(StepKind.Compare, i));
            count[(array[i] / exp) % 10]++;
        }

        for (int i = 1; i < 10; i++)
            count[i] += count[i - 1];

        for (int i = n - 1; i >= 0; i--)
        {
            int digit = (array[i] / exp) % 10;
            output[count[digit] - 1] = array[i];
            count[digit]--;
        }

        for (int i = 0; i < n; i++)
        {
            ct.ThrowIfCancellationRequested();
            array[i] = output[i];
            await onStep(new StepInfo(StepKind.Write, i));
        }
    }
}
