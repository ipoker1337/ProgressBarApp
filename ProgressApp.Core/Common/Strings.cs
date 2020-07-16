using System;

namespace ProgressApp.Core.Common {
public static class 
Strings {
    public static string
    VerifyNonEmpty(this string value) {
        if (String.IsNullOrEmpty(value))
            throw new ArgumentException("Cannot by empty");
        return value;
    }

}
}
