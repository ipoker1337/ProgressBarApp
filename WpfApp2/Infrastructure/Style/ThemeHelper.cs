using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WpfApp2.Infrastructure.Style
{
    public class ThemeHelper
    {
        public static ITheme GetTheme()
        {
            return Application.Current.Resources.GetTheme();
        }

        public static void SetTheme(ITheme theme)
        {
            Application.Current.Resources.SetTheme(theme);
        }

    }
}
