using System;
using System.Globalization;
using Safester.Models;
using Safester.Utils;
using Xamarin.Forms;

namespace Safester.Services.Converters
{
    public class StringCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = System.Convert.ToString(parameter) ?? "u";

            switch (param.ToUpper())
            {
                case "U":
                    return ((string)value).ToUpper();
                case "L":
                    return ((string)value).ToLower();
                default:
                    return ((string)value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class IntToTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)value)
            {
                case 1:
                    return "Type1";
                case 2:
                    return "Type2";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            try
            {
                long tick = (long)value;
                dt = dt.AddMilliseconds(tick).ToLocalTime();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Date Converter Exception - {0}", ex);
            }

            bool isShortDate = false;
            if (parameter != null && parameter is string && ((string)parameter).Equals("shortDate"))
                isShortDate = true;

            if (string.IsNullOrEmpty(App.CurrentLanguage) == false && App.CurrentLanguage.Equals("fr", StringComparison.OrdinalIgnoreCase))
            {
                return dt.ToString(isShortDate ? "dd/MM/yy HH:mm" : "dd/MM/yyyy HH:mm");
            }
            
            return dt.ToString(isShortDate ? "M/d/yy h:mm tt" : "M/d/yyyy h:mm tt");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double size = (long)value;
                if (size < 1024)
                    return "1" + AppResources.KB;

                size /= 1024;
                if (size < 1024) // 1MB
                    return (int)size + AppResources.KB;

                size /= 1024;
                if (size < 1024) // 1GB
                    return string.Format("{0:f2}", size) + AppResources.MB;

                size /= 1024;
                if (size < 1024) // 1TB
                    return string.Format("{0:f2}", size) + AppResources.GB;

                size /= 1024;
                return string.Format("{0:f2}", size) + AppResources.TB;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Size Converter Exception - {0}", ex);
            }

            return "0B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class FileNameSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var data = (Attachment)value;
                return String.Format("{0} ({1})", data.filename, Utils.Utils.GetSizeString(data.size));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Size Converter Exception - {0}", ex);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class FileNameExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var data = (string)value;
                return Utils.Utils.GetFileImageName(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Size Converter Exception - {0}", ex);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BoolToFontAttrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRead = (bool)value;
            if (!isRead)
                return FontAttributes.Bold;

            return FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class StarBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class UnStarNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ThemeHelper.CurrentTheme == ThemeStyle.DARK_THEME ? "star_white.png" : "star_black.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class TextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return ThemeHelper.GetThemeTextColor();

            return ThemeHelper.GetMailListDateSizeColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
