using Music_H_WPF.Models;
using System.IO;

namespace Music_H_WPF.Services
{
    public interface IMusicDecryptorService
    {
        Task<MusicDecryptOutput> DecryptAsync(Stream input, string fileName, CancellationToken cancellationToken);
    }
}
