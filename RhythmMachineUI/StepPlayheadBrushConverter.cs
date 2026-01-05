using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RhythmMachineUI;

public sealed class StepPlayheadBrushConverter : IMultiValueConverter
{
    public Brush Normal { get; set; } = Brushes.Transparent;
    public Brush Current { get; set; } = Brushes.DeepSkyBlue;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Normal;
        if (values[0] is not int stepIndex) return Normal;
        if (values[1] is not int currentStep) return Normal;

        return stepIndex == currentStep ? Current : Normal;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
