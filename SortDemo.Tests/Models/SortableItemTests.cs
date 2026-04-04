using SortDemo.Models;

namespace SortDemo.Tests.Models;

public class SortableItemTests
{
    [Fact]
    public void Constructor_SetsValueAndDefaultState()
    {
        var item = new SortableItem(42);

        Assert.Equal(42, item.Value);
        Assert.Equal(BarState.Normal, item.State);
    }

    [Fact]
    public void Value_CanBeModified()
    {
        var item = new SortableItem(10);
        item.Value = 20;

        Assert.Equal(20, item.Value);
    }

    [Fact]
    public void State_CanBeModified()
    {
        var item = new SortableItem(10);
        item.State = BarState.Comparing;

        Assert.Equal(BarState.Comparing, item.State);
    }

    [Theory]
    [InlineData(BarState.Normal)]
    [InlineData(BarState.Comparing)]
    [InlineData(BarState.Swapping)]
    [InlineData(BarState.Sorted)]
    [InlineData(BarState.Pivot)]
    public void State_SupportsAllBarStates(BarState state)
    {
        var item = new SortableItem(0);
        item.State = state;

        Assert.Equal(state, item.State);
    }
}

public class StepInfoTests
{
    [Fact]
    public void Constructor_SetsKindAndIndices()
    {
        var step = new StepInfo(StepKind.Compare, 0, 1);

        Assert.Equal(StepKind.Compare, step.Kind);
        Assert.Equal(new[] { 0, 1 }, step.Indices);
    }

    [Fact]
    public void Constructor_WithSingleIndex()
    {
        var step = new StepInfo(StepKind.MarkSorted, 5);

        Assert.Equal(StepKind.MarkSorted, step.Kind);
        Assert.Equal(new[] { 5 }, step.Indices);
    }

    [Fact]
    public void Constructor_WithNoIndices()
    {
        var step = new StepInfo(StepKind.Compare);

        Assert.Equal(StepKind.Compare, step.Kind);
        Assert.Empty(step.Indices);
    }

    [Fact]
    public void Record_SupportsValueEquality()
    {
        var step1 = new StepInfo(StepKind.Swap, 0, 1);
        var step2 = new StepInfo(StepKind.Swap, 0, 1);

        Assert.Equal(step1, step2);
    }

    [Theory]
    [InlineData(StepKind.Compare)]
    [InlineData(StepKind.Swap)]
    [InlineData(StepKind.MarkSorted)]
    [InlineData(StepKind.SetPivot)]
    [InlineData(StepKind.Write)]
    public void StepKind_AllValuesAreDefined(StepKind kind)
    {
        var step = new StepInfo(kind, 0);
        Assert.Equal(kind, step.Kind);
    }
}
