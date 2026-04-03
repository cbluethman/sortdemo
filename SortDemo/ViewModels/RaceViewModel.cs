using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SortDemo.Algorithms;
using SortDemo.Models;

namespace SortDemo.ViewModels;

public class RaceViewModel : INotifyPropertyChanged
{
    private readonly Random _random = new();
    private int _winnerDeclared; // 0 = no, 1 = yes (atomic via Interlocked)

    /// <summary>Set by MainViewModel to share its ToneGenerator with race lanes.</summary>
    public Sound.ToneGenerator? ToneGenerator { get; set; }

    public RaceViewModel()
    {
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

        _selectedLeft = Algorithms[4];  // Quick Sort
        _selectedRight = Algorithms[0]; // Bubble Sort

        LeftLane = new RaceLaneViewModel(_selectedLeft);
        RightLane = new RaceLaneViewModel(_selectedRight);

        StartCommand = new RelayCommand(_ => StartRace(), _ => !IsRunning);
        StopCommand = new RelayCommand(_ => StopRace(), _ => IsRunning);
        ResetCommand = new RelayCommand(_ => GenerateItems(), _ => !IsRunning);
        ToggleDirectionCommand = new RelayCommand(_ => SortAscending = !SortAscending, _ => !IsRunning);

        GenerateItems();
    }

    public List<ISortAlgorithm> Algorithms { get; }

    private ISortAlgorithm _selectedLeft = null!;
    public ISortAlgorithm SelectedLeft
    {
        get => _selectedLeft;
        set
        {
            _selectedLeft = value;
            OnPropertyChanged();
            if (!IsRunning && value != null)
                GenerateItems();
        }
    }

    private ISortAlgorithm _selectedRight = null!;
    public ISortAlgorithm SelectedRight
    {
        get => _selectedRight;
        set
        {
            _selectedRight = value;
            OnPropertyChanged();
            if (!IsRunning && value != null)
                GenerateItems();
        }
    }

    public RaceLaneViewModel LeftLane { get; private set; }
    public RaceLaneViewModel RightLane { get; private set; }

    private int _barCount = 100;
    public int BarCount
    {
        get => _barCount;
        set { _barCount = value; OnPropertyChanged(); if (!IsRunning) GenerateItems(); }
    }

    private int _speedValue = 50;
    public int SpeedValue
    {
        get => _speedValue;
        set { _speedValue = value; OnPropertyChanged(); }
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
        set { _isRunning = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    private string _winnerText = "";
    public string WinnerText
    {
        get => _winnerText;
        set { _winnerText = value; OnPropertyChanged(); }
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ToggleDirectionCommand { get; }

    private int[]? _sharedData;

    public void GenerateItems()
    {
        _sharedData = new int[_barCount];
        for (int i = 0; i < _barCount; i++)
            _sharedData[i] = _random.Next(1, _barCount + 1);

        LeftLane = new RaceLaneViewModel(SelectedLeft);
        RightLane = new RaceLaneViewModel(SelectedRight);
        OnPropertyChanged(nameof(LeftLane));
        OnPropertyChanged(nameof(RightLane));

        LeftLane.SetItems((int[])_sharedData.Clone());
        RightLane.SetItems((int[])_sharedData.Clone());
        WinnerText = "";
    }

    private async void StartRace()
    {
        if (IsRunning) return;
        IsRunning = true;
        Interlocked.Exchange(ref _winnerDeclared, 0);
        WinnerText = "";

        if (_sharedData == null) GenerateItems();

        // Ensure lanes use current algorithms with fresh data
        LeftLane = new RaceLaneViewModel(SelectedLeft);
        RightLane = new RaceLaneViewModel(SelectedRight);
        OnPropertyChanged(nameof(LeftLane));
        OnPropertyChanged(nameof(RightLane));

        LeftLane.SetItems((int[])_sharedData!.Clone());
        RightLane.SetItems((int[])_sharedData!.Clone());

        var leftTask = LeftLane.RunAsync(SortAscending, GetDelayMs, GetFrameSkip, DeclareWinner, ToneGenerator);
        var rightTask = RightLane.RunAsync(SortAscending, GetDelayMs, GetFrameSkip, DeclareWinner, ToneGenerator);

        try
        {
            await Task.WhenAll(leftTask, rightTask);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Race failed: {ex}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void DeclareWinner(string algorithmName)
    {
        if (Interlocked.CompareExchange(ref _winnerDeclared, 1, 0) != 0) return;
        WinnerText = $"\U0001F3C6 {algorithmName} wins!";
    }

    public void StopRace()
    {
        LeftLane.Stop();
        RightLane.Stop();
    }

    private int GetDelayMs()
    {
        return Math.Max(1, (int)(500.0 * Math.Pow(0.95, SpeedValue)));
    }

    private int GetFrameSkip()
    {
        if (SpeedValue > 90) return (SpeedValue - 90) * 3;
        if (SpeedValue > 75 && BarCount > 200) return 2;
        return 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
