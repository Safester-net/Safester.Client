using System;
using Safester.Services;
using Safester.Views;
using Xamarin.Forms;

namespace Safester.Utils
{
    public enum ThemeStyle
    {
        STANDARD_THEME,
        DARK_THEME
    }

    public static class ThemeHelper
    {
        public static ThemeStyle CurrentTheme { get; set; }

        public static void ChangeTheme(ThemeStyle theme)
        {
            CurrentTheme = theme;

            var settingsService = DependencyService.Get<SettingsService>();
            settingsService.ChangeTheme(theme);

            MainPage.MainMasterPage?.ChangeMenuTheme();
            ChangeNavigationColor();
        }

        public static Color GetLoginBGColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return (Color)App.Current.Resources["Primary"];
                case ThemeStyle.DARK_THEME:
                    return Color.Black;
                default:
                    return (Color)App.Current.Resources["Primary"];
            }
        }

        public static string GetLoginLogoName()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return  "logo.png";
                case ThemeStyle.DARK_THEME:
                    return "logo_black.png";
                default:
                    return "logo.png";
            }
        }

        public static void ChangeNavigationColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    App.Current.Resources["NavigationPrimary"] = Color.FromHex("#2196F3");
                    break;
                case ThemeStyle.DARK_THEME:
                    App.Current.Resources["NavigationPrimary"] = Color.FromHex("#0356b3");
                    break;
                default:
                    App.Current.Resources["NavigationPrimary"] = Color.FromHex("#2196F3");
                    break;
            }
        }

        public static Color GetListPageBGColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return Color.White;
                case ThemeStyle.DARK_THEME:
                    return Color.Black;
                default:
                    return Color.White;
            }
        }

        public static Color GetMenuBGColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return (Color)App.Current.Resources["Primary"];
                case ThemeStyle.DARK_THEME:
                    return Color.Black;
                default:
                    return (Color)App.Current.Resources["Primary"];
            }
        }

        public static Color GetMenuSelectionColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return (Color)App.Current.Resources["StdSelectionColor"];
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["LightAccentColor"];
                default:
                    return (Color)App.Current.Resources["StdSelectionColor"];
            }
        }

        public static Color GetReadMailBGColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return Color.White;
                case ThemeStyle.DARK_THEME:
                    return Color.Black;
                default:
                    return Color.White;
            }
        }

        public static Color GetReadMailLabelColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return (Color)App.Current.Resources["LightGrayColor"];
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["DarkGrayColor"];
                default:
                    return (Color)App.Current.Resources["LightGrayColor"];
            }
        }

        public static Color GetThemeTextColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return Color.Black;
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["LightBackgroundColor"];
                default:
                    return Color.Black;
            }
        }

        public static Color GetMailListDateSizeColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return Color.Blue;
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["ListDateHourColor"];
                default:
                    return Color.Blue;
            }
        }

        public static Color GetSettingsLabelColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return (Color)App.Current.Resources["Primary"];
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["LightBackgroundColor"];
                default:
                    return (Color)App.Current.Resources["Primary"];
            }
        }

        public static Color GetSettingsEntryColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return Color.Black;
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["LightBackgroundColor"];
                default:
                    return Color.Black;
            }
        }

        public static Color GetSearchEntryBorderColor()
        {
            switch (CurrentTheme)
            {
                case ThemeStyle.STANDARD_THEME:
                    return (Color)App.Current.Resources["DarkGrayColor"];
                case ThemeStyle.DARK_THEME:
                    return (Color)App.Current.Resources["LightBackgroundColor"];
                default:
                    return (Color)App.Current.Resources["DarkGrayColor"];
            }
        }
    }
}
