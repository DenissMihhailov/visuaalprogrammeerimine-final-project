using App.Domain.Entities.Sounds;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;


namespace App.Infrastructure.Audio;

public sealed class NaudioEngine : IAudioEngine, IDisposable
{
    private readonly ConcurrentDictionary<string, CachedSound> _cache = new(StringComparer.OrdinalIgnoreCase);

    private readonly MixingSampleProvider _mixer;
    private readonly VolumeSampleProvider _master;
    private readonly WaveOutEvent _output;

    public NaudioEngine()
    {
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
        {
            ReadFully = true
        };

        _master = new VolumeSampleProvider(_mixer) { Volume = 1.0f };

        _output = new WaveOutEvent
        {
            DesiredLatency = 60,
            NumberOfBuffers = 2
        };

        _output.Init(_master);
        _output.Play();
    }

    public void SetMasterVolume(float volume01)
    {
        if (volume01 < 0f) volume01 = 0f;
        if (volume01 > 1f) volume01 = 1f;
        _master.Volume = volume01;
    }

    public void Play(SoundBase sound)
    {
        try
        {
            var path = sound switch
            {
                BuiltInSound b => Path.Combine(AppContext.BaseDirectory, b.ResourcePath.Replace('/', Path.DirectorySeparatorChar)),
                UserSound u => u.FilePath,
                _ => throw new InvalidOperationException("Unknown sound")
            };

            if (!File.Exists(path))
            {
                Debug.WriteLine($"NOT FOUND: {path}");
                return;
            }

            var cached = _cache.GetOrAdd(path, p => new CachedSound(p));

            ISampleProvider input = new CachedSoundSampleProvider(cached);

            if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
                input = new MonoToStereoSampleProvider(input);

            if (input.WaveFormat.SampleRate != _mixer.WaveFormat.SampleRate)
                input = new WdlResamplingSampleProvider(input, _mixer.WaveFormat.SampleRate);

            if (!input.WaveFormat.Equals(_mixer.WaveFormat))
                throw new InvalidOperationException($"WaveFormat mismatch. Input={input.WaveFormat} Mixer={_mixer.WaveFormat}");

            _mixer.AddMixerInput(input);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    public void Dispose()
    {
        _output.Dispose();
    }
}

public sealed class CachedSound
{
    public float[] AudioData { get; }
    public WaveFormat WaveFormat { get; }

    public CachedSound(string audioFileName)
    {
        using var reader = new AudioFileReader(audioFileName);
        WaveFormat = reader.WaveFormat;

        var wholeFile = new List<float>((int)(reader.Length / 4));
        var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
        int samplesRead;
        while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            wholeFile.AddRange(buffer.Take(samplesRead));

        AudioData = wholeFile.ToArray();
    }
}

public sealed class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound _cachedSound;
    private long _position;

    public CachedSoundSampleProvider(CachedSound cachedSound) => _cachedSound = cachedSound;

    public WaveFormat WaveFormat => _cachedSound.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = _cachedSound.AudioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);

        Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
        _position += samplesToCopy;

        return (int)samplesToCopy;
    }
}
