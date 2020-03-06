using System.Windows.Media;

namespace WpfApp2.Infrastructure.Style
{
    public class DarkTheme : ITheme
    {
        public Color WindowForeground { get; } = (Color)ColorConverter.ConvertFromString("#DDFFFFFF");
        public Color WindowBackground { get; } = (Color)ColorConverter.ConvertFromString("#FF303030");
        public Color ValidationError { get; set; } = (Color)ColorConverter.ConvertFromString("#F44336");


        public Color ProgressBarHighlight { get; set; } = (Color)ColorConverter.ConvertFromString("#0288d1");
        public Color ProgressBarBackground { get; set; } = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
        public Color ProgressBarBorder { get; set; } = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
        public Color ProgressBarText { get; } = (Color)ColorConverter.ConvertFromString("#89FFFFFF");
    }
}
