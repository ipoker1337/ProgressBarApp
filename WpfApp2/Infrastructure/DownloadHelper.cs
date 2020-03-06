using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2
{
    public class DownloadHelper
    {
        public static readonly string[] SizeSuffixes =
                  { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static (string, int) ConvertBytes(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return ("-" + ConvertBytes(-value), 0); }
            if (value == 0) { return (string.Format("{0:n" + decimalPlaces + "} bytes", 0),0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return (string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]), mag);
        }

        public static string ConvertBytes(Int64 value, int suffix, bool includeSuffix = true, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + ConvertBytes(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = suffix;

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            string result = string.Empty;

            if (includeSuffix)
                result = string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
            else result = string.Format("{0:n" + decimalPlaces + "}",
                adjustedSize);

            return result;
        }


        /*        
          * Converts a number of bytes to the appropriate unit that results in an
          * internationalized number that needs fewer than 4 digits.
          *
          * @param aBytes
          *        Number of bytes to convert
          * @return A pair: [new value with 3 sig. figs., its unit]
          *//*
                convertByteUnits: function DU_convertByteUnits(aBytes)
                {
                    let unitIndex = 0;

                    // Convert to next unit if it needs 4 digits (after rounding), but only if
                    // we know the name of the next unit
                    while (aBytes >= 999.5 && unitIndex < gStr.units.length - 1)
                    {
                        aBytes /= 1024;
                        unitIndex++;
                    }

                    // Get rid of insignificant bits by truncating to 1 or 0 decimal points
                    // 0 -> 0; 1.2 -> 1.2; 12.3 -> 12.3; 123.4 -> 123; 234.5 -> 235
                    // added in bug 462064: (unitIndex != 0) makes sure that no decimal digit for bytes appears when aBytes < 100
                    let fractionDigits = aBytes > 0 && aBytes < 100 && unitIndex != 0 ? 1 : 0;

                    // Don't try to format Infinity values using NumberFormat.
                    if (aBytes === Infinity)
                    {
                        aBytes = "Infinity";
                    }
                    else
                    {
                        aBytes = getLocaleNumberFormat(fractionDigits).format(aBytes);
                    }

                    return [aBytes, gBundle.GetStringFromName(gStr.units[unitIndex])];
                },*/
    }
}
