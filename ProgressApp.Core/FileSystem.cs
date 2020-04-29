using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressApp.Core {

public static class 
FileSytem {
    public static string
    DeleteFileIfExist(this string file) {
        if (File.Exists(file))
            File.Delete(file);
        return file;
    }

    public static bool
    FileExists(this string file) => File.Exists(file);

    public static long
    GetFileSize(this string file) => new FileInfo(file).Length;

    public static long
    GetFileSizeOrZero(this string file) => FileExists(file) ? GetFileSize(file) : 0;

    public static FileStream
    OpenFileForWrite(this string file) => File.OpenWrite(file);

    public static FileStream
    OpenFileForWrite(this string file, SeekOrigin origin) {
        var filestream = OpenFileForWrite(file);
        filestream.Seek(0, origin);
        return filestream;
    }
}
}
