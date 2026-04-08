using Music_H_WPF.Extensions;
using Music_H_WPF.Models;
using MusicDecrypto.Library;
using System.IO;

namespace Music_H_WPF.Services.Impl
{
    public class MusicDecryptorService : IMusicDecryptorService
    {
        public async Task<MusicDecryptOutput> DecryptAsync(Stream input, string fileName, CancellationToken cancellationToken)
        {
            var marshal = new MarshalMemoryStream(input.Length);
            await input.CopyToAsync(marshal, cancellationToken);
            marshal.Position = 0;
            marshal.Name = fileName;
            var decrypto = DecryptoFactory.Create(marshal, fileName);
            if (decrypto == null) throw new NotSupportedException($"不支持的文件格式: {fileName}");
            var info = await decrypto.DecryptAsync();
            var stream = decrypto.GetAudioStream();
            return new MusicDecryptOutput
            {
                Data = stream,
                FileName = info.NewName,
                Extension = Path.GetExtension(info.NewName)
            };
        }
    }
}
