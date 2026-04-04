using SortDemo.Sound;

namespace SortDemo.Tests.Sound;

/// <summary>
/// Tests for ToneGenerator that verify it handles edge cases without crashing.
/// We cannot test actual audio output in unit tests, but we can verify that
/// the public API does not throw on boundary inputs.
/// </summary>
public class ToneGeneratorTests
{
    [Fact]
    public void Constructor_DoesNotThrow()
    {
        using var generator = new ToneGenerator();
        // Simply constructing should succeed
    }

    [Fact]
    public void IsMuted_DefaultsToFalse()
    {
        using var generator = new ToneGenerator();
        Assert.False(generator.IsMuted);
    }

    [Fact]
    public void IsMuted_CanBeSet()
    {
        using var generator = new ToneGenerator();
        generator.IsMuted = true;
        Assert.True(generator.IsMuted);
    }

    [Fact]
    public void Volume_DefaultsToPointThree()
    {
        using var generator = new ToneGenerator();
        Assert.Equal(0.3, generator.Volume);
    }

    [Fact]
    public void Volume_CanBeSet()
    {
        using var generator = new ToneGenerator();
        generator.Volume = 0.8;
        Assert.Equal(0.8, generator.Volume);
    }

    [Fact]
    public void PlayTone_BeforeOpen_DoesNotThrow()
    {
        using var generator = new ToneGenerator();
        // PlayTone should return early because _isOpen is false
        generator.PlayTone(50, 100);
    }

    [Fact]
    public void PlayTone_WhenMuted_DoesNotThrow()
    {
        using var generator = new ToneGenerator();
        generator.IsMuted = true;
        generator.PlayTone(50, 100);
    }

    [Theory]
    [InlineData(0, 100)]      // value = 0
    [InlineData(100, 100)]    // value = max
    [InlineData(50, 100)]     // normal case
    [InlineData(0, 0)]        // maxValue = 0, early return
    [InlineData(-1, 100)]     // negative value
    [InlineData(200, 100)]    // value > maxValue
    [InlineData(0, -1)]       // negative maxValue, early return
    public void PlayTone_WithVariousInputs_DoesNotThrow(int value, int maxValue)
    {
        using var generator = new ToneGenerator();
        // Not opened, so all these should return early without crashing
        generator.PlayTone(value, maxValue);
    }

    [Fact]
    public void PlayTone_WithMaxInt_DoesNotThrow()
    {
        using var generator = new ToneGenerator();
        generator.PlayTone(int.MaxValue, int.MaxValue);
    }

    [Fact]
    public void Stop_BeforeOpen_DoesNotThrow()
    {
        using var generator = new ToneGenerator();
        generator.Stop();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var generator = new ToneGenerator();
        generator.Dispose();
        generator.Dispose(); // second call should be safe
    }

    [Fact]
    public void Dispose_AfterPlayTone_DoesNotThrow()
    {
        var generator = new ToneGenerator();
        generator.PlayTone(50, 100);
        generator.Dispose();
    }

    [Fact]
    public void PlayTone_AfterDispose_DoesNotThrow()
    {
        var generator = new ToneGenerator();
        generator.Dispose();
        // PlayTone checks _disposed and returns early
        generator.PlayTone(50, 100);
    }
}
