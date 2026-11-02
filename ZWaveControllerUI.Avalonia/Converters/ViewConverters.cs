// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Material.Icons;

namespace ZWaveControllerUI.Converters
{
    // Small converters standing in for WPF DataTriggers (which set a property when a converted value matched).

    /// <summary>
    /// Icon name (as the view models keep it, e.g. "FileCode") to a Material icon.
    /// </summary>
    public class IconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MaterialIconKind)
                return value;
            if (value is string name && Enum.TryParse(name, out MaterialIconKind kind))
                return kind;
            return MaterialIconKind.Tag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Bold for the node the controller itself is (NodeSubjectConverter).
    /// </summary>
    public class NodeSubjectFontWeightConverter : IValueConverter
    {
        private readonly NodeSubjectConverter _isSubject = new NodeSubjectConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _isSubject.Convert(value, targetType, parameter, culture) is true ? FontWeight.Bold : FontWeight.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Red for failed nodes (NodeFailedConverter).
    /// </summary>
    public class NodeFailedForegroundConverter : IValueConverter
    {
        private readonly NodeFailedConverter _isFailed = new NodeFailedConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _isFailed.Convert(value, targetType, parameter, culture) is true ? Brushes.Red : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Security scheme bar of the node list: shown (opacity 1) when the node has the scheme in the parameter, keeping its slot otherwise.
    /// </summary>
    public class NodeSecuritySchemeOpacityConverter : IValueConverter
    {
        private readonly NodeSecuritySchemesConverter _hasScheme = new NodeSecuritySchemesConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _hasScheme.Convert(value, targetType, parameter, culture) is true ? 1.0 : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// True for a node itself, false for one of its end points.
    /// </summary>
    public class IsNotEndPointConverter : IValueConverter
    {
        private readonly NodeEndPointConverter _isEndPoint = new NodeEndPointConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(_isEndPoint.Convert(value, targetType, parameter, culture) is true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// The forced listening flag can be set for nodes that are neither listening nor FLiRS.
    /// </summary>
    public class CanForceListeningConverter : IValueConverter
    {
        private readonly NodeEndPointConverter _isEndPoint = new NodeEndPointConverter();
        private readonly NodeListeningConverter _isListening = new NodeListeningConverter();
        private readonly NodeFlirsConverter _isFlirs = new NodeFlirsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(_isEndPoint.Convert(value, targetType, parameter, culture) is true)
                && !(_isListening.Convert(value, targetType, parameter, culture) is true)
                && !(_isFlirs.Convert(value, targetType, parameter, culture) is true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// string.Format(parameter, values), or empty while any value is missing. Stands in for WPF MultiBindings
    /// whose parts fall back to null and render nothing (Avalonia formats missing parts as "(unset)").
    /// </summary>
    public class FormatOrEmptyConverter : Avalonia.Data.Converters.IMultiValueConverter
    {
        public object Convert(System.Collections.Generic.IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || !(parameter is string format))
                return string.Empty;
            foreach (var value in values)
            {
                if (value == null || value == Avalonia.AvaloniaProperty.UnsetValue || value is Avalonia.Data.BindingNotification)
                    return string.Empty;
            }
            var args = new object[values.Count];
            values.CopyTo(args, 0);
            return string.Format(culture, format, args);
        }
    }
}

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// False only when all values are true (a MultiDataTrigger that collapses an element when all conditions hold).
    /// </summary>
    public class NandConverter : Avalonia.Data.Converters.IMultiValueConverter
    {
        public object Convert(System.Collections.Generic.IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0)
                return true;
            foreach (var value in values)
            {
                if (!(value is bool b) || !b)
                    return true;
            }
            return false;
        }
    }
}

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// True when the value equals ConverterParameter, compared as invariant text (a DataTrigger Value="5" on a number).
    /// </summary>
    public class ValueEqualsConverter : Avalonia.Data.Converters.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(System.Convert.ToString(value, CultureInfo.InvariantCulture), System.Convert.ToString(parameter, CultureInfo.InvariantCulture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// Flags of a sent command's SecureType: "Secured" is true for Secure, false for NonSecure and null otherwise;
    /// "Broadcast" is true for Broadcast only.
    /// </summary>
    public class SecureTypeFlagConverter : Avalonia.Data.Converters.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ZWaveController.Enums.SecureType secureType))
                return parameter as string == "Broadcast" ? (bool?)false : null;
            if (parameter as string == "Broadcast")
                return secureType == ZWaveController.Enums.SecureType.Broadcast;
            if (secureType == ZWaveController.Enums.SecureType.Secure)
                return true;
            if (secureType == ZWaveController.Enums.SecureType.NonSecure)
                return false;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// Named colors (properties of Colors) offered for the topology map legend.
    /// </summary>
    public static class NamedColors
    {
        public static System.Reflection.PropertyInfo[] Properties { get; } = typeof(Avalonia.Media.Colors).GetProperties();
    }

    /// <summary>
    /// Brush of a named color property (a NamedColors item).
    /// </summary>
    public class ColorPropertyBrushConverter : Avalonia.Data.Converters.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Reflection.PropertyInfo property && property.GetValue(null) is Avalonia.Media.Color color)
                return new Avalonia.Media.SolidColorBrush(color);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
