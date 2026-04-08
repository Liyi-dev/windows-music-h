using System.IO;
using Music_H_WPF.Models;

namespace Music_H_WPF.Services;

public sealed class MusicLibraryService
{
    private static readonly HashSet<string> AudioExtensions =
    [
        ".mp3", ".wav", ".flac", ".aac", ".m4a", ".wma", ".ogg"
    ];

    /// <summary>
    /// 程序根目录（通常为 exe 所在目录，如 bin/Debug/net8.0-windows）下的 music 文件夹。
    /// </summary>
    public static string GetMusicDirectoryPath() =>
        Path.Combine(AppContext.BaseDirectory, "music");

    public static void EnsureMusicDirectoryExists()
    {
        string path = GetMusicDirectoryPath();
        Directory.CreateDirectory(path);
    }

    public static IReadOnlyList<Track> ScanTracks()
    {
        string root = GetMusicDirectoryPath();
        if (!Directory.Exists(root))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(root)
            .Where(f => AudioExtensions.Contains(Path.GetExtension(f)))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Select(f => new Track
            {
                Title = Path.GetFileNameWithoutExtension(f),
                FilePath = Path.GetFullPath(f)
            })
            .ToList();
    }

    /// <summary>
    /// 将外部文件复制到 music 目录（同名则覆盖），返回目标路径。
    /// </summary>
    public static string CopyIntoMusicLibrary(string sourceFilePath)
    {
        EnsureMusicDirectoryExists();
        string fileName = Path.GetFileName(sourceFilePath);
        string dest = Path.Combine(GetMusicDirectoryPath(), fileName);
        File.Copy(sourceFilePath, dest, overwrite: true);
        return Path.GetFullPath(dest);
    }
}
