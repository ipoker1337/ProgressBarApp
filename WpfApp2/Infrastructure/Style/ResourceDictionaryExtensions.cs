using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfApp2.Infrastructure.Style
{
    public static class ResourceDictionaryExtensions
    {
        private static Guid CurrentThemeKey { get; } = Guid.NewGuid();
        private static Guid ThemeManagerKey { get; } = Guid.NewGuid();

        public static void SetTheme(this ResourceDictionary resourceDictionary, ITheme theme)
        {
            SetSolidColorBrush(resourceDictionary, "WindowForegroundBrush", theme.WindowForeground);
            SetSolidColorBrush(resourceDictionary, "WindowBackgroundBrush", theme.WindowBackground);

            SetSolidColorBrush(resourceDictionary, "ValidationErrorBrush", theme.ValidationError);
            resourceDictionary["ValidationErrorColor"] = theme.ValidationError;

            SetSolidColorBrush(resourceDictionary, "ProgressBarHighligthBrush", theme.ProgressBarHighlight);
            SetSolidColorBrush(resourceDictionary, "ProgressBarBackgroundBrush", theme.ProgressBarBackground);
            SetSolidColorBrush(resourceDictionary, "ProgressBarBorderBrush", theme.ProgressBarBorder);
            SetSolidColorBrush(resourceDictionary, "ProgressBarTextBrush", theme.ProgressBarText);

/*            if (!(resourceDictionary.GetThemeManager() is ThemeManager themeManager))
            {
                resourceDictionary[ThemeManagerKey] = themeManager = new ThemeManager(resourceDictionary);
            }
            ITheme oldTheme = resourceDictionary.GetTheme();
            resourceDictionary[CurrentThemeKey] = theme;*/
        }

        public static ITheme GetTheme(this ResourceDictionary resourceDictionary)
        {
            if (resourceDictionary[CurrentThemeKey] is ITheme theme)
            {
                return theme;
            }
            //Attempt to simply look up the appropriate resources
            return new Theme(GetColor("WindowForegroundBrush"),
                                GetColor("WindowBackgroundBrush"),
                                GetColor("ValidationErrorBrush"),
                                GetColor("ProgressBarHighligthBrush"),
                                GetColor("ProgressBarBackgroundBrush"),
                                GetColor("ProgressBarBorderBrush"),
                                GetColor("ProgressBarTextBrush"));       


            Color GetColor(params string[] keys)
            {
                foreach (string key in keys)
                {
                    if (TryGetColor(key, out Color color))
                    {
                        return color;
                    }
                }
                throw new InvalidOperationException($"Could not locate required resource with key(s) '{string.Join(", ", keys)}'");
            }

            bool TryGetColor(string key, out Color color)
            {
                if (resourceDictionary[key] is SolidColorBrush brush)
                {
                    color = brush.Color;
                    return true;
                }
                color = default;
                return false;
            }
        }

/*        public static IThemeManager GetThemeManager(this ResourceDictionary resourceDictionary)
        {
            if (resourceDictionary == null) throw new ArgumentNullException(nameof(resourceDictionary));
            return resourceDictionary[ThemeManagerKey] as IThemeManager;
        }*/

        internal static void SetSolidColorBrush(this ResourceDictionary sourceDictionary, string name, Color value)
        {
            if (sourceDictionary == null) throw new ArgumentNullException(nameof(sourceDictionary));
            if (name == null) throw new ArgumentNullException(nameof(name));

            sourceDictionary[name + "Color"] = value;

            if (sourceDictionary[name] is SolidColorBrush brush)
            {
                if (brush.Color == value) return;

                if (!brush.IsFrozen)
                {
                    var animation = new ColorAnimation
                    {
                        From = brush.Color,
                        To = value,
                        Duration = new Duration(TimeSpan.FromMilliseconds(300))
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                    return;
                }
            }

            var newBrush = new SolidColorBrush(value);
            newBrush.Freeze();
            sourceDictionary[name] = newBrush;
        }
        public class ThemeChangedEventArgs : EventArgs
        {
            public ThemeChangedEventArgs(ResourceDictionary resourceDictionary, ITheme oldTheme, ITheme newTheme)
            {
                ResourceDictionary = resourceDictionary;
                OldTheme = oldTheme;
                NewTheme = newTheme;
            }

            public ResourceDictionary ResourceDictionary { get; }
            public ITheme NewTheme { get; }
            public ITheme OldTheme { get; }
        }

        public interface IThemeManager
        {
            event EventHandler<ThemeChangedEventArgs> ThemeChanged;
        }

        private class ThemeManager : IThemeManager
        {
            private ResourceDictionary _ResourceDictionary;

            public ThemeManager(ResourceDictionary resourceDictionary)
            {
                _ResourceDictionary = resourceDictionary ?? throw new ArgumentNullException(nameof(resourceDictionary));
            }

            public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

            public void OnThemeChange(ITheme oldTheme, ITheme newTheme)
            {
                ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(_ResourceDictionary, oldTheme, newTheme));
            }
        }
    }
}
