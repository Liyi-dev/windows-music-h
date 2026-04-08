using System.IO;

namespace Music_H_WPF.Models
{
    /// <summary>
    /// 待解密输入。解密过程中会读取 <see cref="Stream"/>，调用方需保持流可用，并在完成后自行释放。
    /// </summary>
    public record MusicDecryptInput(Stream Stream, string FileName);

    /// <summary>
    /// 音乐解密结果
    /// </summary>
    /// <param name="FileStream">文件流</param>
    /// <param name="FileName">文件名</param>
    /// <param name="ContentType">类型</param>
    public record MusicDecryptResultDto(Stream FileStream, string FileName, string ContentType);

    /// <summary>
    /// 解密结果模型
    /// </summary>
    public class MusicDecryptOutput
    {
        /// <summary>
        /// 数据流
        /// </summary>
        public Stream Data { get; init; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 扩展
        /// </summary>
        public string Extension { get; init; }
    }
}
