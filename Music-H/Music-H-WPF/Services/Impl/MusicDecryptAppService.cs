using Music_H_WPF.Extensions;
using Music_H_WPF.Models;
using System.IO;

namespace Music_H_WPF.Services.Impl
{
    public class MusicDecryptAppService : IMusicDecryptAppService
    {
        /// <summary>
        /// 解密器服务
        /// </summary>
        private readonly IMusicDecryptorService _decryptor;

        public MusicDecryptAppService()
        {
            _decryptor = new MusicDecryptorService();
        }

        public async Task<MusicDecryptResultDto> DecryptAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (stream.CanSeek && stream.Length == 0) throw new ArgumentException("输入流不能为空", nameof(stream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("文件名不能为空", nameof(fileName));

            var result = await _decryptor.DecryptAsync(stream, fileName, cancellationToken);
            return new MusicDecryptResultDto(
                FileStream: result.Data,
                FileName: result.FileName,
                ContentType: AudioContentTypeProvider.Get(result.Extension)
            );
        }

        public async Task<MusicDecryptResultDto> DecryptFromFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("文件路径不能为空", nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException("文件不存在", filePath);

            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, FileOptions.Asynchronous);
            return await DecryptAsync(stream, Path.GetFileName(filePath), cancellationToken);
        }

        public async Task<List<MusicDecryptResultDto>> BatchDecryptAsync(IReadOnlyList<MusicDecryptInput> files, int maxConcurrency = 4, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(files);
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = files.Select(async input =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try { return await DecryptAsync(input.Stream, input.FileName, cancellationToken); }
                finally { semaphore.Release(); }
            });
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}
