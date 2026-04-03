# SortDemo

A Windows desktop application that visualizes sorting algorithms in real time. Watch 14 different algorithms sort data represented as horizontal bars, complete with color-coded states, sound effects, and live performance statistics. Compare any two algorithms head-to-head in Race Mode.

![Screenshot](screenshot.png)

## Features

### 14 Sorting Algorithms

Bubble, Selection, Insertion, Merge, Quick, Heap, Shell, Radix (LSD), Counting, Cocktail Shaker, Comb, Tim, Gnome, and Bitonic. Each algorithm includes a plain-English description and best/average/worst time complexity displayed in the UI.

### Animated Visualization

Bars are drawn in real time using a custom WPF `FrameworkElement` with direct `OnRender` drawing. Bar colors indicate the current operation:

| Color  | Meaning              |
|--------|----------------------|
| Blue   | Normal (unsorted)    |
| Orange | Being compared       |
| Red    | Being swapped/written|
| Green  | Sorted (final position)|
| Gold   | Pivot element        |

### Sound Effects

Each step plays a short sine-wave tone mapped to the bar's value -- low bars produce low-pitched tones, high bars produce high-pitched ones. Audio is generated at 44.1 kHz through the Win32 `waveOut` API with a pre-allocated rotating buffer pool. A mute toggle and volume slider are provided.

### Race Mode

Pick two algorithms and race them side by side on identical shuffled data. Both run concurrently with independent visualizers and stat counters. The first to finish is announced as the winner.

### Controls

- **Speed slider** (1--100) -- exponential delay mapping from ~500 ms down to ~1 ms per step, with frame skipping at high speeds
- **Bar count slider** (10--500) -- gaps between bars are removed automatically above 200 bars
- **Ascending / Descending toggle** -- sort in either direction
- **Live stats** -- comparisons, swaps/writes, and elapsed time updated during the sort

### Dark Theme

The entire UI uses a dark color scheme (`#1E1E2E` background, `#2D2D3D` surfaces) with custom-styled buttons, combo boxes, and sliders.

## Requirements

- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build and Run

```
dotnet build SortDemo.sln
dotnet run --project SortDemo
```

Or open `SortDemo.sln` in Visual Studio 2022+ and press F5.

## Project Structure

```
SortDemo/
  Algorithms/
    ISortAlgorithm.cs       # Interface: Name, Description, TimeComplexity, SortAsync
    BubbleSort.cs            # 14 algorithm implementations
    SelectionSort.cs
    InsertionSort.cs
    MergeSort.cs
    QuickSort.cs
    HeapSort.cs
    ShellSort.cs
    RadixSort.cs
    CountingSort.cs
    CocktailShakerSort.cs
    CombSort.cs
    TimSort.cs
    GnomeSort.cs
    BitonicSort.cs
  Models/
    SortableItem.cs          # BarState enum, StepKind enum, StepInfo record
  Sound/
    ToneGenerator.cs         # waveOut P/Invoke sine-wave generator
  Themes/
    Colors.xaml              # Dark theme resource dictionary
  ViewModels/
    MainViewModel.cs         # Single-sort mode logic and shared settings
    RaceViewModel.cs         # Race mode orchestration and winner detection
    RaceLaneViewModel.cs     # Per-lane sort execution with independent stats
    RelayCommand.cs          # ICommand implementation
  Visualization/
    SortVisualizer.cs        # Custom FrameworkElement, draws bars via OnRender
  MainWindow.xaml            # UI layout (single mode and race mode panels)
  MainWindow.xaml.cs         # Code-behind (minimal -- just disposes the view model)
  App.xaml / App.xaml.cs     # Application entry point
  SortDemo.csproj            # .NET 8 WPF project, no external NuGet packages
```

## Architecture Notes

**Algorithm contract.** Every algorithm implements `ISortAlgorithm.SortAsync`, which operates on an `int[]` and calls a `Func<StepInfo, Task>` callback on each compare, swap, write, pivot-set, or mark-sorted event. This callback is where the UI applies highlights, plays tones, and yields via `Task.Delay` to control animation speed.

**Rendering.** `SortVisualizer` is a lightweight `FrameworkElement` that overrides `OnRender` to draw rectangles directly to a `DrawingContext`. A bound `RenderVersion` property is incremented to trigger repaints without allocating new visual objects.

**Sound.** `ToneGenerator` pre-allocates four `waveOut` buffers on startup and rotates through them to avoid per-tone allocation. If a buffer is still in the driver queue, the tone is silently skipped rather than blocking.

**Race Mode.** `RaceViewModel` clones the shuffled array for each lane, then runs both `RaceLaneViewModel.RunAsync` tasks concurrently via `Task.WhenAll`. The first lane to complete calls `DeclareWinner`, which uses `Interlocked.CompareExchange` to ensure only one winner is recorded.

## License

This project is provided as-is for educational purposes.
