using System.Runtime.InteropServices;

namespace SortDemo.Sound;

/// <summary>
/// Generates short sine-wave tones using waveOut with a rotating buffer pool.
/// Each buffer is allocated once and reused, avoiding alloc/free during playback.
/// </summary>
public class ToneGenerator : IDisposable
{
    private const int SampleRate = 44100;
    private const int Channels = 1;
    private const int BitsPerSample = 16;
    private const int ToneDurationMs = 40;
    private const double MinFrequency = 200.0;
    private const double MaxFrequency = 1200.0;
    private const int BufferCount = 4;

    private IntPtr _hWaveOut;
    private bool _isOpen;
    private bool _disposed;
    private readonly object _lock = new();

    // Pre-allocated buffer pool
    private readonly IntPtr[] _headers = new IntPtr[BufferCount];
    private readonly IntPtr[] _dataBuffers = new IntPtr[BufferCount];
    private readonly int _dataSize;
    private readonly int _headerSize;
    private int _currentBuffer;

    public bool IsMuted { get; set; }
    public double Volume { get; set; } = 0.3;

    public ToneGenerator()
    {
        _dataSize = SampleRate * ToneDurationMs / 1000 * Channels * BitsPerSample / 8;
        _headerSize = Marshal.SizeOf<WaveHeader>();
    }

    public void Open()
    {
        if (_isOpen) return;

        var format = new WaveFormatEx
        {
            wFormatTag = 1,
            nChannels = (short)Channels,
            nSamplesPerSec = SampleRate,
            nAvgBytesPerSec = SampleRate * Channels * BitsPerSample / 8,
            nBlockAlign = (short)(Channels * BitsPerSample / 8),
            wBitsPerSample = (short)BitsPerSample,
            cbSize = 0
        };

        if (waveOutOpen(out _hWaveOut, 0xFFFFFFFF, ref format, IntPtr.Zero, IntPtr.Zero, 0) != 0)
            return;

        _isOpen = true;

        // Pre-allocate all buffers
        for (int i = 0; i < BufferCount; i++)
        {
            _dataBuffers[i] = Marshal.AllocHGlobal(_dataSize);
            _headers[i] = Marshal.AllocHGlobal(_headerSize);

            var header = new WaveHeader
            {
                lpData = _dataBuffers[i],
                dwBufferLength = (uint)_dataSize
            };
            Marshal.StructureToPtr(header, _headers[i], false);
        }
    }

    public void PlayTone(int value, int maxValue)
    {
        if (IsMuted || !_isOpen || _disposed || maxValue <= 0) return;

        try
        {
            lock (_lock)
            {
            if (_disposed) return;
            double ratio = Math.Clamp((double)value / maxValue, 0.0, 1.0);
            double frequency = MinFrequency + ratio * (MaxFrequency - MinFrequency);

            // Pick next buffer in rotation
            int buf = _currentBuffer;
            _currentBuffer = (_currentBuffer + 1) % BufferCount;

            IntPtr hdrPtr = _headers[buf];
            IntPtr dataPtr = _dataBuffers[buf];

            // Check buffer state before reusing
            var hdr = Marshal.PtrToStructure<WaveHeader>(hdrPtr);
            if ((hdr.dwFlags & 0x10) != 0) // WHDR_INQUEUE — driver still owns this buffer
                return; // skip tone rather than corrupt memory

            if ((hdr.dwFlags & 0x01) != 0) // WHDR_PREPARED
                waveOutUnprepareHeader(_hWaveOut, hdrPtr, _headerSize);

            // Fill audio data
            FillSineWave(dataPtr, frequency);

            // Reset header fields
            hdr.lpData = dataPtr;
            hdr.dwBufferLength = (uint)_dataSize;
            hdr.dwFlags = 0;
            hdr.dwBytesRecorded = 0;
            hdr.dwLoops = 0;
            Marshal.StructureToPtr(hdr, hdrPtr, false);

            waveOutPrepareHeader(_hWaveOut, hdrPtr, _headerSize);
            waveOutWrite(_hWaveOut, hdrPtr, _headerSize);
            }
        }
        catch
        {
            // Never crash the app over audio
        }
    }

    private unsafe void FillSineWave(IntPtr buffer, double frequency)
    {
        int sampleCount = _dataSize / 2; // 16-bit samples
        short* samples = (short*)buffer;
        int fadeSamples = SampleRate * 3 / 1000; // 3ms fade

        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / SampleRate;
            double sample = Math.Sin(2.0 * Math.PI * frequency * t) * Volume;

            if (i < fadeSamples)
                sample *= (double)i / fadeSamples;
            else if (i > sampleCount - fadeSamples)
                sample *= (double)(sampleCount - i) / fadeSamples;

            samples[i] = (short)(sample * short.MaxValue);
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_isOpen) waveOutReset(_hWaveOut);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (!_isOpen || _disposed) return;
            _disposed = true;

            waveOutReset(_hWaveOut);

            for (int i = 0; i < BufferCount; i++)
            {
                if (_headers[i] != IntPtr.Zero)
                {
                    waveOutUnprepareHeader(_hWaveOut, _headers[i], _headerSize);
                    Marshal.FreeHGlobal(_headers[i]);
                    _headers[i] = IntPtr.Zero;
                }
                if (_dataBuffers[i] != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_dataBuffers[i]);
                    _dataBuffers[i] = IntPtr.Zero;
                }
            }

            waveOutClose(_hWaveOut);
            _isOpen = false;
        }
        GC.SuppressFinalize(this);
    }

    ~ToneGenerator() => Dispose();

    // P/Invoke
    [DllImport("winmm.dll")]
    private static extern int waveOutOpen(out IntPtr hWaveOut, uint deviceId, ref WaveFormatEx lpFormat,
        IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);
    [DllImport("winmm.dll")]
    private static extern int waveOutClose(IntPtr hWaveOut);
    [DllImport("winmm.dll")]
    private static extern int waveOutReset(IntPtr hWaveOut);
    [DllImport("winmm.dll")]
    private static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);
    [DllImport("winmm.dll")]
    private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);
    [DllImport("winmm.dll")]
    private static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, int uSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveFormatEx
    {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHeader
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }
}
