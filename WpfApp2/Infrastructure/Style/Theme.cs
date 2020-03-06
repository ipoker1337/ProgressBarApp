using System.Windows.Media;

namespace WpfApp2.Infrastructure.Style
{
    public class Theme : ITheme
    {
        public static ITheme Light { get; } = new LightTheme();
        public static ITheme Dark { get; } = new DarkTheme();

        public Theme(   Color windowForeground,
                        Color windowBackground,                        
                        Color validationError,
                        Color progressBarHighlight,
                        Color progressBarBackground,
                        Color progressBarBorder,
                        Color progressBarText)
        {
            WindowBackground = windowBackground;
            WindowForeground = windowForeground;
            ValidationError = validationError;
            ProgressBarHighlight = progressBarHighlight;
            ProgressBarBackground = progressBarBackground;
            ProgressBarBorder = progressBarBorder;
            ProgressBarText = progressBarText;
        }

        public Color WindowBackground { get; }
        public Color WindowForeground { get; }
        public Color ValidationError { get; }
        public Color ProgressBarHighlight { get; }
        public Color ProgressBarBackground { get; }
        public Color ProgressBarBorder { get; }
        public Color ProgressBarText { get; }
    }
}
