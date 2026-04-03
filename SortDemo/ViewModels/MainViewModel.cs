using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using SortDemo.Algorithms;
using SortDemo.Models;
using SortDemo.Sound;

namespace SortDemo.ViewModels;

public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly Random _random = new();
    private readonly Dispatcher _dispatcher;
    private readonly ToneGenerator _toneGenerator = new();
    private CancellationTokenSource? _cts;
    private Stopwatch _stopwatch = new();
    private DispatcherTimer? _timer;

    public MainViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        _toneGenerator.Open();

        Algorithms = new List<ISortAlgorithm>
        {
            new BubbleSort(),
            new SelectionSort(),
            new InsertionSort(),
            new MergeSort(),
            new QuickSort(),
            new HeapSort(),
            new ShellSort(),
            new RadixSort(),
            new CountingSort(),
            new CocktailShakerSort(),
            new CombSort(),
            new TimSort(),
            new GnomeSort(),
            new BitonicSort()
        };

        SelectedAlgorithm = Algorithms[0];

        StartCommand = new RelayCommand(_ => StartSort(), _ => !IsRunning);
        StopCommand = new RelayCommand(_ => StopSort(), _ => IsRunning);
        ResetCommand = new RelayCommand(_ => GenerateItems(), _ => !IsRunning);
        ToggleSoundCommand = new RelayCommand(_ => IsMuted = !IsMuted);
        ToggleDirectionCommand = new RelayCommand(_ => SortAscending = !SortAscending, _ => !IsRunning);
        ToggleRaceModeCommand = new RelayCommand(_ => IsRaceMode = !IsRaceMode, _ => !IsRunning && !Race.IsRunning);

        Race = new RaceViewModel { ToneGenerator = _toneGenerator };
        Race.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Race.IsRunning))
            {
                OnPropertyChanged(nameof(CanChangeSettings));
                CommandManager.InvalidateRequerySuggested();
            }
        };
        GenerateItems();
    }

    // Race mode
    public RaceViewModel Race { get; }

    private bool _isRaceMode;
    public bool IsRaceMode
    {
        get => _isRaceMode;
        set { _isRaceMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(ModeLabel)); }
    }
    public string ModeLabel => IsRaceMode ? "Single Mode" : "Race Mode";

    // Properties
    public List<ISortAlgorithm> Algorithms { get; }
    public string AlgorithmDescription => SelectedAlgorithm?.Description ?? "";
    public string AlgorithmComplexity => SelectedAlgorithm?.TimeComplexity ?? "";

    private ISortAlgorithm _selectedAlgorithm = null!;
    public ISortAlgorithm SelectedAlgorithm
    {
        get => _selectedAlgorithm;
        set
        {
            _selectedAlgorithm = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AlgorithmDescription));
            OnPropertyChanged(nameof(AlgorithmComplexity));
        }
    }

    private int _barCount = 100;
    public int BarCount
    {
        get => _barCount;
        set
        {
            _barCount = value;
            OnPropertyChanged();
            Race.BarCount = value;
            if (!IsRunning) GenerateItems();
        }
    }

    private int _speedValue = 50;
    public int SpeedValue
    {
        get => _speedValue;
        set { _speedValue = value; OnPropertyChanged(); Race.SpeedValue = value; }
    }

    private SortableItem[] _items = Array.Empty<SortableItem>();
    public SortableItem[] Items
    {
        get => _items;
        set { _items = value; OnPropertyChanged(); }
    }

    private bool _sortAscending = true;
    public bool SortAscending
    {
        get => _sortAscending;
        set { _sortAscending = value; OnPropertyChanged(); OnPropertyChanged(nameof(SortDirectionLabel)); }
    }

    public string SortDirectionLabel => SortAscending ? "ASC" : "DESC";

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanChangeSettings));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool CanChangeSettings => !IsRunning && !Race.IsRunning;

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

    private int _renderVersion;
    public int RenderVersion
    {
        get => _renderVersion;
        set { _renderVersion = value; OnPropertyChanged(); }
    }

    private bool _isMuted;
    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            _toneGenerator.IsMuted = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SoundIcon));
        }
    }

    public string SoundIcon => IsMuted ? "\U0001F507" : "\U0001F50A";

    private double _volume = 0.3;
    public double Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            _toneGenerator.Volume = value;
            OnPropertyChanged();
        }
    }

    // Commands
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ToggleSoundCommand { get; }
    public ICommand ToggleDirectionCommand { get; }
    public ICommand ToggleRaceModeCommand { get; }

    // Methods
    public void GenerateItems()
    {
        var items = new SortableItem[_barCount];
        for (int i = 0; i < _barCount; i++)
        {
            items[i] = new SortableItem(_random.Next(1, _barCount + 1));
        }
        Items = items;
        Comparisons = 0;
        Swaps = 0;
        ElapsedTime = "0.0s";
    }

    private async void StartSort()
    {
        if (IsRunning) return;
        IsRunning = true;
        Comparisons = 0;
        Swaps = 0;

        // Reset all bar states
        for (int i = 0; i < Items.Length; i++)
            Items[i].State = BarState.Normal;

        _cts = new CancellationTokenSource();
        _stopwatch = Stopwatch.StartNew();

        // Timer to update elapsed time display
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += (_, _) => ElapsedTime = $"{_stopwatch.Elapsed.TotalSeconds:F1}s";
        _timer.Start();

        // Build the working array of ints
        int[] array = new int[Items.Length];
        for (int i = 0; i < Items.Length; i++)
            array[i] = Items[i].Value;

        int stepCounter = 0;
        var prevHighlighted = new List<int>();

        try
        {
            await SelectedAlgorithm.SortAsync(array, async step =>
            {
                // Reset only previously highlighted bars (not O(n))
                foreach (int pi in prevHighlighted)
                {
                    if (pi >= 0 && pi < Items.Length && Items[pi].State != BarState.Sorted)
                        Items[pi].State = BarState.Normal;
                }
                prevHighlighted.Clear();

                // Sync only affected indices from working array
                foreach (int idx in step.Indices)
                {
                    if (idx >= 0 && idx < Items.Length)
                        Items[idx].Value = array[idx];
                }

                // For swaps, also sync the partner indices
                if (step.Kind == StepKind.Swap && step.Indices.Length >= 2)
                {
                    foreach (int idx in step.Indices)
                        if (idx >= 0 && idx < Items.Length)
                            Items[idx].Value = array[idx];
                }

                // For write operations (merge/radix), sync a range around the index
                if (step.Kind == StepKind.Write && step.Indices.Length > 0)
                {
                    int idx = step.Indices[0];
                    if (idx >= 0 && idx < Items.Length)
                        Items[idx].Value = array[idx];
                }

                // Apply step highlights
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

                // Play tone mapped to the value of the first affected bar
                if (step.Indices.Length > 0)
                {
                    int idx = step.Indices[0];
                    if (idx >= 0 && idx < Items.Length)
                        _toneGenerator.PlayTone(Items[idx].Value, _barCount);
                }

                // Determine delay and whether to render
                stepCounter++;
                int skip = GetFrameSkip();
                if (stepCounter % (skip + 1) == 0)
                {
                    RenderVersion++;
                    await Task.Delay(GetDelayMs(), _cts!.Token);
                }

            }, _cts.Token, _sortAscending);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sort failed: {ex}");
        }
        finally
        {
            _stopwatch.Stop();
            _timer?.Stop();
            _toneGenerator.Stop();
            ElapsedTime = $"{_stopwatch.Elapsed.TotalSeconds:F1}s";

            // Mark all bars sorted on successful completion (not cancellation)
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                for (int i = 0; i < Items.Length; i++)
                    Items[i].State = BarState.Sorted;
            }

            _cts?.Dispose();
            _cts = null;
            IsRunning = false;
            RenderVersion++;
        }
    }

    private void StopSort()
    {
        _cts?.Cancel();
    }

    private int GetDelayMs()
    {
        // Exponential mapping: speed 1 = ~500ms, speed 100 = ~1ms
        return Math.Max(1, (int)(500.0 * Math.Pow(0.95, SpeedValue)));
    }

    private int GetFrameSkip()
    {
        if (SpeedValue > 90)
            return (SpeedValue - 90) * 3;
        if (SpeedValue > 75 && BarCount > 200)
            return 2;
        return 0;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _toneGenerator.Dispose();
        Race.StopRace();
    }

    // INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
