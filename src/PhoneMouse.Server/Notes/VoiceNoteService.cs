using System.Text;

namespace PhoneMouse.Server.Notes;

public sealed class VoiceNoteService
{
    private static readonly byte[]
        Utf8Bom =
        [
            0xEF,
            0xBB,
            0xBF
        ];


    private readonly object _gate =
        new();


    private readonly string
        _filePath;


    public VoiceNoteService()
    {
        string documents =
            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments);


        string directory =
            Path.Combine(
                documents,
                "PhoneMouse");


        Directory.CreateDirectory(
            directory);


        _filePath =
            Path.Combine(
                directory,
                "VoiceNotes.txt");


        EnsureUtf8Bom();
    }


    public string FilePath =>
        _filePath;


    public void Append(
        string text)
    {
        string normalized =
            text.Trim();


        if (
            string.IsNullOrWhiteSpace(
                normalized))
        {
            throw new ArgumentException(
                "文字内容不能为空。",
                nameof(text));
        }


        string block =
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}" +
            normalized +
            Environment.NewLine +
            Environment.NewLine;


        lock (_gate)
        {
            EnsureUtf8BomLocked();


            File.AppendAllText(
                _filePath,
                block,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier:
                        false));
        }
    }


    private void EnsureUtf8Bom()
    {
        lock (_gate)
        {
            EnsureUtf8BomLocked();
        }
    }


    private void EnsureUtf8BomLocked()
    {
        if (
            !File.Exists(
                _filePath))
        {
            File.WriteAllText(
                _filePath,
                string.Empty,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier:
                        true));

            return;
        }


        byte[] bytes =
            File.ReadAllBytes(
                _filePath);


        if (
            bytes.Length >= 3 &&
            bytes[0] ==
                Utf8Bom[0] &&
            bytes[1] ==
                Utf8Bom[1] &&
            bytes[2] ==
                Utf8Bom[2])
        {
            return;
        }


        // Alpha 0.7.1~0.7.1.3 写入的是“无 BOM UTF-8”。
        // 文件内容本身通常没有损坏，只是 Windows PowerShell 5.1
        // 的 Get-Content 会把无 BOM UTF-8 当成系统 ANSI 编码读取，
        // 因而显示成“浣犲ソ”之类的乱码。
        //
        // 这里把已有 UTF-8 内容按 UTF-8 解码，再重新写成 UTF-8 BOM，
        // 保留已有笔记，同时让记事本 / Windows PowerShell 5.1 更容易识别。
        string existingText =
            Encoding.UTF8.GetString(
                bytes);


        File.WriteAllText(
            _filePath,
            existingText,
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier:
                    true));
    }
}
