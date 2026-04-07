namespace Music_H_WPF.Services;

public interface IAudioPlayerService
{
    event EventHandler? MediaEnded;
    event EventHandler? MediaOpened;

    TimeSpan Position { get; set; }
    TimeSpan Duration { get; }
    double Volume { get; set; }

    void Open(string filePath);
    void Play();
    void Pause();
    void Stop();
}
