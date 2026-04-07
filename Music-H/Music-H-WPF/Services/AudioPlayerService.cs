using System.Windows.Media;

namespace Music_H_WPF.Services;

public sealed class AudioPlayerService : IAudioPlayerService
{
    private readonly MediaPlayer _mediaPlayer = new();

    public AudioPlayerService()
    {
        _mediaPlayer.MediaEnded += (_, _) => MediaEnded?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.MediaOpened += (_, _) => MediaOpened?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? MediaEnded;
    public event EventHandler? MediaOpened;

    public TimeSpan Position
    {
        get => _mediaPlayer.Position;
        set => _mediaPlayer.Position = value;
    }

    public TimeSpan Duration =>
        _mediaPlayer.NaturalDuration.HasTimeSpan
            ? _mediaPlayer.NaturalDuration.TimeSpan
            : TimeSpan.Zero;

    public double Volume
    {
        get => _mediaPlayer.Volume;
        set => _mediaPlayer.Volume = Math.Clamp(value, 0, 1);
    }

    public void Open(string filePath)
    {
        _mediaPlayer.Open(new Uri(filePath, UriKind.Absolute));
    }

    public void Play() => _mediaPlayer.Play();

    public void Pause() => _mediaPlayer.Pause();

    public void Stop() => _mediaPlayer.Stop();
}
