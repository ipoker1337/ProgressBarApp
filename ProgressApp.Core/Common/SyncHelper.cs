using System.Threading;

namespace ProgressApp.Core.Common {
public static class 
 SynchronizationContextHelper {
     public static SynchronizationContext CurrentOrDefault =>
        SynchronizationContext.Current ?? new SynchronizationContext();
}
}
