using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace WelfareMonitorApp.Converters
{
    public class ScoreToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float score)
            {
                score = Math.Clamp(score, 0f, 1f);
                int red = (int)((1 - score) * 255);
                int green = (int)(score * 255);
                return Color.FromRgb(red, green, 0);
            }
            return Color.FromRgb(255, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
