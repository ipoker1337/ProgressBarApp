using System.Windows.Media;

namespace WpfApp2.Infrastructure.Style
{
    public class LightTheme : ITheme
    {
        public Color WindowForeground { get; } = Colors.Black;
        public Color WindowBackground { get; } = (Color)ColorConverter.ConvertFromString("#FFfafafa");
        public Color ValidationError { get; set; } = (Color)ColorConverter.ConvertFromString("#F44336");


        public Color ProgressBarHighlight { get; set; } = (Color)ColorConverter.ConvertFromString("#FF086F9E");
        public Color ProgressBarBackground { get; set; } = (Color)ColorConverter.ConvertFromString("#FFB9B9B9");
        public Color ProgressBarBorder { get; set; } = (Color)ColorConverter.ConvertFromString("#FFCCCCCC");
        public Color ProgressBarText { get; } = Colors.Gray;
    }
}