using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using Music_H_WPF.Commands;
using Music_H_WPF.Models;
using Music_H_WPF.Services;

namespace Music_H_WPF.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IAudioPlayerService _audioPlayerService;
    private readonly DispatcherTimer _progressTimer;
    private readonly RelayCommand _playCommand;
    private readonly RelayCommand _pauseCommand;
    private readonly RelayCommand _stopCommand;
    private readonly RelayCommand _nextCommand;
    private readonly RelayCommand _previousCommand;

    private Track? _selectedTrack;
    private int _currentTrackIndex = -1;
    private bool _isPlaying;
    private TimeSpan _currentPosition = TimeSpan.Zero;
    private TimeSpan _totalDuration = TimeSpan.Zero;
    private double _seekValue;
    private bool _isSeeking;
    private double _volume = 0.6;

    public MainViewModel()
    {
        _audioPlayerService = new AudioPlayerService();
        _audioPlayerService.MediaEnded += (_, _) => PlayNext();
        _audioPlayerService.MediaOpened += (_, _) =>
        {
            TotalDuration = _audioPlayerService.Duration;
            RaiseCanExecuteChanged();
        };
        _audioPlayerService.Volume = _volume;

        _progressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _progressTimer.Tick += (_, _) => UpdateProgress();
        _progressTimer.Start();

        ImportCommand = new RelayCommand(ImportTracks);
        _playCommand = new RelayCommand(Play, () => SelectedTrack is not null);
        _pauseCommand = new RelayCommand(Pause, () => IsPlaying);
        _stopCommand = new RelayCommand(Stop, () => SelectedTrack is not null);
        _nextCommand = new RelayCommand(PlayNext, () => Playlist.Count > 1);
        _previousCommand = new RelayCommand(PlayPrevious, () => Playlist.Count > 1);

        RefreshPlaylistFromDisk();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Track> Playlist { get; } = [];

    public ICommand ImportCommand { get; }
    public ICommand PlayCommand => _playCommand;
    public ICommand PauseCommand => _pauseCommand;
    public ICommand StopCommand => _stopCommand;
    public ICommand NextCommand => _nextCommand;
    public ICommand PreviousCommand => _previousCommand;

    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set
        {
            if (SetField(ref _selectedTrack, value))
            {
                _currentTrackIndex = value is null ? -1 : Playlist.IndexOf(value);
                RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            if (SetField(ref _isPlaying, value))
            {
                RaiseCanExecuteChanged();
            }
        }
    }

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        private set
        {
            if (SetField(ref _currentPosition, value))
            {
                OnPropertyChanged(nameof(CurrentPositionText));
            }
        }
    }

    public TimeSpan TotalDuration
    {
        get => _totalDuration;
        private set
        {
            if (SetField(ref _totalDuration, value))
            {
                OnPropertyChanged(nameof(TotalDurationText));
                OnPropertyChanged(nameof(SeekMaximum));
            }
        }
    }

    public string CurrentPositionText => FormatTime(CurrentPosition);
    public string TotalDurationText => FormatTime(TotalDuration);

    public double SeekValue
    {
        get => _seekValue;
        set
        {
            if (SetField(ref _seekValue, value) && _isSeeking)
            {
                _audioPlayerService.Position = TimeSpan.FromSeconds(value);
            }
        }
    }

    public double SeekMaximum => Math.Max(1, TotalDuration.TotalSeconds);

    public double Volume
    {
        get => _volume;
        set
        {
            if (SetField(ref _volume, value))
            {
                _audioPlayerService.Volume = value;
            }
        }
    }

    public void BeginSeek() => _isSeeking = true;

    public void EndSeek()
    {
        _audioPlayerService.Position = TimeSpan.FromSeconds(SeekValue);
        _isSeeking = false;
    }

    private void ImportTracks()
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Audio Files|*.mp3;*.wav;*.flac;*.aac;*.m4a;*.wma|All Files|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        foreach (string file in dialog.FileNames)
        {
            MusicLibraryService.CopyIntoMusicLibrary(file);
        }

        RefreshPlaylistFromDisk();
    }

    /// <summary>
    /// 从程序目录下 music 文件夹重新扫描并填充播放列表（启动时与导入后调用）。
    /// </summary>
    private void RefreshPlaylistFromDisk()
    {
        string? previousPath = SelectedTrack?.FilePath;

        Playlist.Clear();
        MusicLibraryService.EnsureMusicDirectoryExists();
        foreach (Track track in MusicLibraryService.ScanTracks())
        {
            Playlist.Add(track);
        }

        if (Playlist.Count == 0)
        {
            SelectedTrack = null;
            _currentTrackIndex = -1;
        }
        else if (previousPath is not null)
        {
            SelectedTrack = Playlist.FirstOrDefault(t =>
                string.Equals(t.FilePath, previousPath, StringComparison.OrdinalIgnoreCase))
                ?? Playlist[0];
        }
        else
        {
            SelectedTrack = Playlist[0];
        }

        RaiseCanExecuteChanged();
    }

    private void Play()
    {
        if (SelectedTrack is null)
        {
            return;
        }

        int selectedIndex = Playlist.IndexOf(SelectedTrack);
        if (selectedIndex >= 0 && selectedIndex != _currentTrackIndex)
        {
            _currentTrackIndex = selectedIndex;
            _audioPlayerService.Open(SelectedTrack.FilePath);
        }

        _audioPlayerService.Play();
        IsPlaying = true;
    }

    private void Pause()
    {
        _audioPlayerService.Pause();
        IsPlaying = false;
    }

    private void Stop()
    {
        _audioPlayerService.Stop();
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
        SeekValue = 0;
    }

    private void PlayNext()
    {
        if (Playlist.Count == 0)
        {
            return;
        }

        _currentTrackIndex = (_currentTrackIndex + 1 + Playlist.Count) % Playlist.Count;
        SelectedTrack = Playlist[_currentTrackIndex];
        _audioPlayerService.Open(SelectedTrack.FilePath);
        _audioPlayerService.Play();
        IsPlaying = true;
    }

    private void PlayPrevious()
    {
        if (Playlist.Count == 0)
        {
            return;
        }

        _currentTrackIndex = (_currentTrackIndex - 1 + Playlist.Count) % Playlist.Count;
        SelectedTrack = Playlist[_currentTrackIndex];
        _audioPlayerService.Open(SelectedTrack.FilePath);
        _audioPlayerService.Play();
        IsPlaying = true;
    }

    private void UpdateProgress()
    {
        if (_isSeeking || !IsPlaying)
        {
            return;
        }

        CurrentPosition = _audioPlayerService.Position;
        TotalDuration = _audioPlayerService.Duration;
        SeekValue = CurrentPosition.TotalSeconds;
    }

    private void RaiseCanExecuteChanged()
    {
        _playCommand.RaiseCanExecuteChanged();
        _pauseCommand.RaiseCanExecuteChanged();
        _stopCommand.RaiseCanExecuteChanged();
        _nextCommand.RaiseCanExecuteChanged();
        _previousCommand.RaiseCanExecuteChanged();
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalHours >= 1
            ? time.ToString(@"hh\:mm\:ss")
            : time.ToString(@"mm\:ss");
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
