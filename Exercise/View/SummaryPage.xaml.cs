﻿using Base.Misc;
using Base.Mvvm;
using Base.Mvvm.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Exercise.View
{
    internal class HasExceptionConverter : VisibilityConverter
    {
        internal HasExceptionConverter()
        {
            CollapsedValues = new object[] { 0 };
            //VisibleValues = new object[0];
        }
    }

    internal class NoExceptionConverter : VisibilityConverter
    {
        internal NoExceptionConverter()
        {
            VisibleValues = new object[] { 0 };
            //CollapsedValues = new object[0];
        }
    }

    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class SummaryPage : Page
    {

        public SummaryPage()
        {
            InitializeComponent();
        }

    }
}
