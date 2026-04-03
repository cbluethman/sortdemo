using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.ViewModels;

public class RaceLaneViewModel : INotifyPropertyChanged
{
    private CancellationTokenSource? _cts;
    private Stopwatch _stopwatch = new();
    private DispatcherTimer? _timer;

    public ISortAlgorithm Algorithm { get; }

    private SortableItem[] _items = Array.Empty<SortableItem>();
    public SortableItem[] Items
    {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }

    private int _renderVersion;
    public int RenderVersion
    {
        get => _renderVersion;
        set { _renderVersion = value; OnPropertyChanged(); }
    }

    private int _comparisons;
    public int Comparisons
    {
        get => _comparisons;
        set { _comparisons = value; OnPropertyChanged(); }
    }

    private int _swaps;
    public int Swaps
    {
        get => _swaps;
        set { _swaps = value; OnPropertyChanged(); }
    }

    private string _elapsedTime = "0.0s";
    public string ElapsedTime
    {
        get => _elapsedTime;
        set { _elapsedTime = value; OnPropertyChanged(); }
    }

    private string _status = "Ready";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private bool _isFinished;
    public bool IsFinished
    {
        get => _isFinished;
        set { _isFinished = value; OnPropertyChanged(); }
    }

    public RaceLaneViewModel(ISortAlgorithm algorithm)
    {
        Algorithm = algorithm;
    }

    public void SetItems(int[] values)
    {
        var items = new SortableItem[values.Length];
        for (int i = 0; i < values.Length; i++)
            items[i] = new SortableItem(values[i]);
        Items = items;
        Comparisons = 0;
        Swaps = 0;
        ElapsedTime = "0.0s";
        Status = "Ready";
        IsFinished = false;
        RenderVersion++;
    }

    public async Task RunAsync(bool ascending, Func<int> getDelayMs, Func<int> getFrameSkip, Action<string>? onFinished, Sound.ToneGenerator? toneGenerator = null)
    {
        Comparisons = 0;
        Swaps = 0;
        Status = "Sorting...";
        IsFinished = false;

        for (int i = 0; i < Items.Length; i++)
            Items[i].State = BarState.Normal;

        _cts = new CancellationTokenSource();
        _stopwatch = Stopwatch.StartNew();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += (_, _) => ElapsedTime = $"{_stopwatch.Elapsed.TotalSeconds:F1}s";
        _timer.Start();

        int[] array = new int[Items.Length];
        for (int i = 0; i < Items.Length; i++)
            array[i] = Items[i].Value;

        int stepCounter = 0;
        var prevHighlighted = new List<int>();

        try
        {
            await Algorithm.SortAsync(array, async step =>
            {
                foreach (int pi in prevHighlighted)
                {
                    if (pi >= 0 && pi < Items.Length && Items[pi].State != BarState.Sorted)
                        Items[pi].State = BarState.Normal;
                }
                prevHighlighted.Clear();

                foreach (int idx in step.Indices)
                    if (idx >= 0 && idx < Items.Length)
                        Items[idx].Value = array[idx];

                switch (step.Kind)
                {
                    case StepKind.Compare:
                        Comparisons++;
                        foreach (int idx in step.Indices)
                            if (idx >= 0 && idx < Items.Length)
                            { Items[idx].State = BarState.Comparing; prevHighlighted.Add(idx); }
                        break;
                    case StepKind.Swap:
                    case StepKind.Write:
                        Swaps++;
                        foreach (int idx in step.Indices)
                            if (idx >= 0 && idx < Items.Length)
                            { Items[idx].State = BarState.Swapping; prevHighlighted.Add(idx); }
                        break;
                    case StepKind.MarkSorted:
                        foreach (int idx in step.Indices)
                            if (idx >= 0 && idx < Items.Length)
                                Items[idx].State = BarState.Sorted;
                        break;
                    case StepKind.SetPivot:
                        foreach (int idx in step.Indices)
                            if (idx >= 0 && idx < Items.Length)
                            { Items[idx].State = BarState.Pivot; prevHighlighted.Add(idx); }
                        break;
                }

                if (toneGenerator != null && step.Indices.Length > 0)
                {
                    int idx = step.Indices[0];
                    if (idx >= 0 && idx < Items.Length)
                        toneGenerator.PlayTone(Items[idx].Value, Items.Length);
                }

                stepCounter++;
                int skip = getFrameSkip();
                if (stepCounter % (skip + 1) == 0)
                {
                    RenderVersion++;
                    await Task.Delay(getDelayMs(), _cts!.Token);
                }
            }, _cts.Token, ascending);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.WriteLine($"Race lane sort failed: {ex}");
        }
        finally
        {
            _stopwatch.Stop();
            _timer.Stop();
            ElapsedTime = $"{_stopwatch.Elapsed.TotalSeconds:F1}s";

            if (_cts != null && !_cts.IsCancellationRequested)
            {
                for (int i = 0; i < Items.Length; i++)
                    Items[i].State = BarState.Sorted;
                Status = "Finished!";
                IsFinished = true;
                onFinished?.Invoke(Algorithm.Name);
            }
            else
            {
                Status = "Stopped";
            }

            _cts?.Dispose();
            _cts = null;
            RenderVersion++;
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
