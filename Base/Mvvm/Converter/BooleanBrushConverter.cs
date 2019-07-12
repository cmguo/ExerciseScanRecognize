﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Base.Mvvm.Converter
{
    [ValueConversion(typeof(object), typeof(Brush))]
    public class BooleanBrushConverter : BooleanConverter<Brush>
    {
    }

}
