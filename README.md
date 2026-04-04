# SortDemo

A Windows desktop application that visualizes sorting algorithms in real time. Watch 14 different algorithms sort data represented as colored bars, complete with sound effects and live performance statistics. Compare any two algorithms head-to-head in Race Mode.

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

Pick two algorithms and race them side by side on identical shuffled data. Both run concurrently with independent visualizers and stat counters. The first to finish is announced as the winner. Race Mode is integrated directly into the main window -- switch between single-sort and race mode without opening a separate window.

### Controls

- **Speed slider** (1--100) -- exponential delay mapping from ~500 ms down to ~1 ms per step, with frame skipping at high speeds
- **Bar count slider** (10--500) -- gaps between bars are removed automatically above 200 bars
- **Ascending / Descending toggle** -- sort in either direction
- **Mute / Volume** -- toggle sound on or off and adjust volume
- **Live stats** -- comparisons, swaps/writes, and elapsed time updated during the sort

### Dark Theme

The entire UI uses a dark color scheme (`#1E1E2E` background, `#2D2D3D` surfaces) with custom-styled buttons, combo boxes, and sliders.

## Requirements

- Windows 10 or later (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for building from source)

## Building

### Debug Build

```
dotnet build SortDemo.sln
dotnet run --project SortDemo
```

Or open `SortDemo.sln` in Visual Studio 2022+ and press F5.

### Release Build

```
dotnet build SortDemo.sln -c Release
```

### Self-Contained Publish

Produces a single-file executable that does not require .NET to be installed:

```
dotnet publish SortDemo/SortDemo.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

The output is `publish/SortDemo.exe`.

### Creating a Release

Push a version tag to trigger the GitHub Actions workflow:

```
git tag v1.0.0
git push origin v1.0.0
```

The workflow (`.github/workflows/release.yml`) builds a self-contained single-file exe, zips it, and creates a GitHub Release with both the standalone exe and the zip archive attached.

## Testing

The `SortDemo.Tests` project uses xUnit. Run all tests with:

```
dotnet test SortDemo.sln
```

The test suite covers:

- **All 14 sorting algorithms** -- correctness for random, pre-sorted, reverse-sorted, single-element, empty, and duplicate-heavy arrays, in both ascending and descending order
- **Step callbacks** -- verifies that algorithms emit compare, swap, write, and mark-sorted events
- **Cancellation** -- confirms algorithms respect `CancellationToken` and throw `OperationCanceledException`
- **Edge cases** -- algorithms for non-comparison sorts (Radix, Counting, Bitonic) have dedicated test classes
- **Models** -- `SortableItem` and `StepInfo` behavior
- **Sound** -- `ToneGenerator` construction and disposal

## Project Structure

```
SortDemo/
  Algorithms/
    ISortAlgorithm.cs           # Interface: Name, Description, TimeComplexity, SortAsync
    BubbleSort.cs               # 14 algorithm implementations
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
    SortableItem.cs             # BarState enum, StepKind enum, StepInfo record
  Sound/
    ToneGenerator.cs            # waveOut P/Invoke sine-wave generator
  Themes/
    Colors.xaml                 # Dark theme resource dictionary
  ViewModels/
    MainViewModel.cs            # Single-sort mode logic and shared settings
    RaceViewModel.cs            # Race mode orchestration and winner detection
    RaceLaneViewModel.cs        # Per-lane sort execution with independent stats
    RelayCommand.cs             # ICommand implementation
  Visualization/
    SortVisualizer.cs           # Custom FrameworkElement, draws bars via OnRender
  MainWindow.xaml               # UI layout (single mode and race mode panels)
  MainWindow.xaml.cs            # Code-behind (minimal -- disposes the view model)
  App.xaml / App.xaml.cs        # Application entry point
  SortDemo.csproj               # .NET 8 WPF project, no external NuGet packages

SortDemo.Tests/
  Algorithms/
    ComparisonSortTests.cs      # Parameterized tests for all comparison-based sorts
    BitonicSortTests.cs         # Bitonic sort-specific tests (power-of-two padding)
    CountingSortTests.cs        # Counting sort-specific tests
    RadixSortTests.cs           # Radix sort-specific tests
    AlgorithmStepBehaviorTests.cs  # Step callback verification
    SortHelperTests.cs          # Shared helper method tests
  Helpers/
    SortTestHelper.cs           # Test data generators and assertion helpers
    Usings.cs                   # Global usings for test project
  Models/
    SortableItemTests.cs        # Model unit tests
  Sound/
    ToneGeneratorTests.cs       # Audio subsystem tests
  SortDemo.Tests.csproj         # xUnit + coverlet

SortDemo.Package/
  Package.appxmanifest          # MSIX manifest for Microsoft Store
  Images/                       # Store logos and splash screen
  SortDemo.Package.wapproj      # Windows Application Packaging project

.github/
  workflows/
    release.yml                 # CI: build + GitHub Release on version tags

SortDemo.sln                    # Solution file (all three projects)
```

## Architecture Notes

**Algorithm contract.** Every algorithm implements `ISortAlgorithm.SortAsync`, which operates on an `int[]` and calls a `Func<StepInfo, Task>` callback on each compare, swap, write, pivot-set, or mark-sorted event. This callback is where the UI applies highlights, plays tones, and yields via `Task.Delay` to control animation speed.

**Rendering.** `SortVisualizer` is a lightweight `FrameworkElement` that overrides `OnRender` to draw rectangles directly to a `DrawingContext`. A bound `RenderVersion` property is incremented to trigger repaints without allocating new visual objects.

**Sound.** `ToneGenerator` pre-allocates four `waveOut` buffers on startup and rotates through them to avoid per-tone allocation. If a buffer is still in the driver queue, the tone is silently skipped rather than blocking.

**Race Mode.** `RaceViewModel` clones the shuffled array for each lane, then runs both `RaceLaneViewModel.RunAsync` tasks concurrently via `Task.WhenAll`. The first lane to complete calls `DeclareWinner`, which uses `Interlocked.CompareExchange` to ensure only one winner is recorded.

**Unsafe code.** The project enables `AllowUnsafeBlocks` for the `waveOut` P/Invoke interop in `ToneGenerator`.

## License

This project is provided as-is for educational purposes.
