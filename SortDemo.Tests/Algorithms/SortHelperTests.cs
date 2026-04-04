using SortDemo.Algorithms;

namespace SortDemo.Tests.Algorithms;

public class SortHelperTests
{
    [Theory]
    [InlineData(5, 3, true, true)]    // 5 > 3 ascending -> should swap
    [InlineData(3, 5, true, false)]   // 3 < 5 ascending -> no swap
    [InlineData(3, 3, true, false)]   // equal ascending -> no swap
    [InlineData(3, 5, false, true)]   // 3 < 5 descending -> should swap
    [InlineData(5, 3, false, false)]  // 5 > 3 descending -> no swap
    [InlineData(3, 3, false, false)]  // equal descending -> no swap
    public void ShouldSwap_ReturnsCorrectResult(int a, int b, bool ascending, bool expected)
    {
        Assert.Equal(expected, SortHelper.ShouldSwap(a, b, ascending));
    }

    [Fact]
    public void ShouldSwap_WithNegativeValues_Ascending()
    {
        Assert.True(SortHelper.ShouldSwap(0, -1, true));
        Assert.False(SortHelper.ShouldSwap(-1, 0, true));
    }

    [Fact]
    public void ShouldSwap_WithNegativeValues_Descending()
    {
        Assert.True(SortHelper.ShouldSwap(-1, 0, false));
        Assert.False(SortHelper.ShouldSwap(0, -1, false));
    }

    [Fact]
    public void ShouldSwap_WithExtremeValues()
    {
        Assert.True(SortHelper.ShouldSwap(int.MaxValue, int.MinValue, true));
        Assert.False(SortHelper.ShouldSwap(int.MinValue, int.MaxValue, true));
        Assert.True(SortHelper.ShouldSwap(int.MinValue, int.MaxValue, false));
        Assert.False(SortHelper.ShouldSwap(int.MaxValue, int.MinValue, false));
    }
}
