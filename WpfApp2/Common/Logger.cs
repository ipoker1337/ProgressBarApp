namespace WpfApp2.Common {

public interface
ILogger {
   void Debug(string message);
}

public class
DefaultLogger : ILogger {
    public static readonly ILogger Null = new NullLogger();

    public void 
    Debug(string message) => System.Diagnostics.Debug.WriteLine(message);

    private class
    NullLogger : ILogger {
        public void 
        Debug(string message) { }
    }
}
}
