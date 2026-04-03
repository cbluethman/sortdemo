namespace SortDemo.Models;

public enum BarState
{
    Normal,
    Comparing,
    Swapping,
    Sorted,
    Pivot
}

public enum StepKind
{
    Compare,
    Swap,
    MarkSorted,
    SetPivot,
    Write
}

public record StepInfo(StepKind Kind, params int[] Indices);

public class SortableItem
{
    public int Value { get; set; }
    public BarState State { get; set; }

    public SortableItem(int value)
    {
        Value = value;
        State = BarState.Normal;
    }
}
