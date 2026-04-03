using System.Windows;
using System.Windows.Media;
using SortDemo.Models;

namespace SortDemo.Visualization;

public class SortVisualizer : FrameworkElement
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(SortableItem[]), typeof(SortVisualizer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RenderVersionProperty =
        DependencyProperty.Register(nameof(RenderVersion), typeof(int), typeof(SortVisualizer),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public SortableItem[]? Items
    {
        get => (SortableItem[]?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public int RenderVersion
    {
        get => (int)GetValue(RenderVersionProperty);
        set => SetValue(RenderVersionProperty, value);
    }

    private static readonly Brush NormalBrush = new SolidColorBrush(Color.FromRgb(0x46, 0x82, 0xB4));
    private static readonly Brush ComparingBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00));
    private static readonly Brush SwappingBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x14, 0x3C));
    private static readonly Brush SortedBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57));
    private static readonly Brush PivotBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
    private static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));

    static SortVisualizer()
    {
        NormalBrush.Freeze();
        ComparingBrush.Freeze();
        SwappingBrush.Freeze();
        SortedBrush.Freeze();
        PivotBrush.Freeze();
        BackgroundBrush.Freeze();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Consume all available space offered by the parent
        return new Size(
            double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 300 : availableSize.Height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

        var items = Items;
        if (items == null || items.Length == 0) return;

        int count = items.Length;
        double barWidth = ActualWidth / count;
        double gap = count > 200 ? 0 : 1;
        int maxValue = 0;
        for (int i = 0; i < count; i++)
        {
            if (items[i].Value > maxValue) maxValue = items[i].Value;
        }
        if (maxValue == 0) return;

        for (int i = 0; i < count; i++)
        {
            double barHeight = (items[i].Value / (double)maxValue) * (ActualHeight - 4);
            double x = i * barWidth;
            double y = ActualHeight - barHeight;

            Brush brush = items[i].State switch
            {
                BarState.Comparing => ComparingBrush,
                BarState.Swapping => SwappingBrush,
                BarState.Sorted => SortedBrush,
                BarState.Pivot => PivotBrush,
                _ => NormalBrush
            };

            dc.DrawRectangle(brush, null, new Rect(x + gap / 2, y, Math.Max(barWidth - gap, 1), barHeight));
        }
    }
}
