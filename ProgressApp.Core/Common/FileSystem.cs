using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProgressApp.Core.Common {

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
}
}
