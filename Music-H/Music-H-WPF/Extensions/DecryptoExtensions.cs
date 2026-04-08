using MusicDecrypto.Library;
using System.IO;
using System.Reflection;

namespace Music_H_WPF.Extensions
{
    /// <summary>
    /// 反射扩展
    /// </summary>
    public static class DecryptoExtensions
    {
        private static readonly FieldInfo BufferField = typeof(DecryptoBase).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("找不到 _buffer 字段");

        public static Stream GetAudioStream(this DecryptoBase decrypto)
        {
            var stream = BufferField.GetValue(decrypto) as Stream;
            if (stream == null) throw new Exception("获取解密流失败");
            if (stream.CanSeek) stream.Position = 0;
            return stream;
        }
    }

    /// <summary>
    /// ContentType 工具
    /// </summary>
    public static class AudioContentTypeProvider
    {
        public static string Get(string ext) => ext.ToLower() switch
        {
            ".mp3" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            _ => "application/octet-stream"
        };
    }
}
