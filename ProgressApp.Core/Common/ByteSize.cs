using System;

namespace ProgressApp.Core.Common {
public readonly struct 
ByteSize {
    private const long BytesInKilobyte = 1024;
    private const long BytesInMegabyte = 1048576;
    private const long BytesInGigabyte = 1073741824;

    private const string ByteSymbol = "B";
    private const string KilobyteSymbol = "KB";
    private const string MegabyteSymbol = "MB";
    private const string GigabyteSymbol = "GB";

    public enum Unit {
        Bytes = 0,
        Kilobytes = 1,
        Megabytes = 2,
        Gigabytes = 3,
    }

    public ByteSize(double bytes) => ToBytes = bytes.VerifyNonNegative();

    public static ByteSize 
    FromBytes(long value) => new ByteSize(value);

    public static ByteSize
    FromMegaBytes(long value) => new ByteSize(value * BytesInMegabyte);

    public double ToBytes { get; }
    public double ToKilobytes => ToBytes / BytesInKilobyte;
    public double ToMegabytes => ToBytes / BytesInMegabyte;
    public double ToGigabytes => ToBytes / BytesInGigabyte;

    public string 
    ToReadable(Unit unit) {
        switch (unit) {
            case Unit.Gigabytes:
                return $"{ToGigabytes:N1} {GigabyteSymbol}";
            case Unit.Megabytes:
                return $"{ToMegabytes:N1} {MegabyteSymbol}";
            case Unit.Kilobytes:
                return $"{ToKilobytes:N1} {KilobyteSymbol}";
            default:
                return $"{ToBytes:N1} {ByteSymbol}";
        }
    }

    public string 
    ToReadable() => ToReadable(LargestUnit);

    public Unit LargestUnit {
        get {
            if (Math.Abs(ToGigabytes) >= 1) 
                return Unit.Gigabytes;
            if (Math.Abs(ToMegabytes) >= 1)
                return Unit.Megabytes;
            if (Math.Abs(ToKilobytes) >= 1)
                return Unit.Kilobytes;
            return Unit.Bytes;
        }
    }
}
}
