using System.IO;
using Music_H_WPF.Models;

namespace Music_H_WPF.Services
{
    public interface IMusicDecryptAppService
    {
        /// <summary>
        /// 解密（例如 <c>File.OpenRead(path)</c> 或内存流）。
        /// </summary>
        Task<MusicDecryptResultDto> DecryptAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// 从本地文件路径解密（适合 WPF 选文件对话框返回的路径）。
        /// </summary>
        Task<MusicDecryptResultDto> DecryptFromFileAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量解密
        /// </summary>
        Task<List<MusicDecryptResultDto>> BatchDecryptAsync(IReadOnlyList<MusicDecryptInput> files, int maxConcurrency = 4, CancellationToken cancellationToken = default);
    }
}
