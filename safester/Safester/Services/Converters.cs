using System;
using System.Globalization;
using Safester.Models;
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

            return dt.ToString("g");
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
                    return "1KB";

                size /= 1024;
                if (size < 1024) // 1MB
                    return (int)size + "KB";

                size /= 1024;
                if (size < 1024) // 1GB
                    return string.Format("{0:f2}MB", size);

                size /= 1024;
                if (size < 1024) // 1TB
                    return string.Format("{0:f2}GB", size);

                size /= 1024;
                return string.Format("{0:f2}TB", size);
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
}
