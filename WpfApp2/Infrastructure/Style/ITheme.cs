using System.Windows.Media;

namespace WpfApp2.Infrastructure.Style
{
    public interface ITheme
    {
        Color WindowBackground { get; }
        Color WindowForeground { get;  }
        Color ValidationError { get; }   
        Color ProgressBarHighlight { get; }
        Color ProgressBarBackground { get;  }
        Color ProgressBarBorder { get;  }        
        Color ProgressBarText { get; }        
    }
}
