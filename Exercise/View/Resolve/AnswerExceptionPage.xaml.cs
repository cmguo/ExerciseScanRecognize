﻿using System.Windows;
using static Exercise.Model.ExerciseModel;
using System.Collections.Generic;
using Base.Misc;
using Exception = Exercise.Model.ExerciseModel.Exception;
using TalBase.View;
using Exercise.Model;
using System.Windows.Controls;
using System;
using System.Linq;

namespace Exercise.View.Resolve
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>

    public partial class AnswerExceptionPage : System.Windows.Controls.Page
    {
        private ExceptionType type;
        private PageAnalyze analyze;

        public AnswerExceptionPage()
        {
            InitializeComponent();
            Loaded += AnswerExceptionPage_Initialized;
            Unloaded += AnswerExceptionPage_Unloaded;
        }

        private void AnswerExceptionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            analyze.PropertyChanged -= Analyze_PropertyChanged;
        }

        private void AnswerExceptionPage_Initialized(object sender, System.EventArgs e)
        {
            Exception ex = DataContext as Exception;
            type = ex.Type;
            analyze = ex.Page.Analyze;
            analyze.PropertyChanged += Analyze_PropertyChanged;
            analyze.Switch(type);
            if (type == ExceptionType.AnswerException)
            {
                correct.Visibility = Visibility.Collapsed;
            }
            else
            {
                answers.Visibility = Visibility.Collapsed;
                FillScores();
            }
        }

        private void Analyze_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedException")
            {
                //answers.UnselectAll();
                //if (analyze.SelectedException == null)
                //    return;
                //foreach (char c in analyze.SelectedException.SelectedAnswer)
                //{
                //    ListViewItem item = answers.ItemContainerGenerator.ContainerFromItem(c.ToString()) as ListViewItem;
                //    if (item != null)
                //        item.IsSelected = true;
                //}
                ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
                rp.SetPaperFocusRect(PaperOverlayConverter.MakeRect(analyze.SelectedException.Location, 0));
            }
        }

        private void FillScores()
        {
            List<object> scores = new List<object>();
            for (int i = 1; i < 10; ++i)
                scores.Add(i);
            scores.Add(0);
            scores.Add(false);
            numbers.ItemsSource = scores;
        }

        private void Score_Click(object sender, RoutedEventArgs e)
        {
            object n = (sender as FrameworkElement).DataContext;
            analyze.SelectedException.InputNumber((n is int) ? (int)n : (int?)null);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (type == ExceptionType.AnswerException)
            {
                analyze.SelectedException.SelectedAnswer = String.Join("",
                    answers.SelectedItems.Cast<String>());
            }
            if (!analyze.Confirm())
            {
                PopupDialog.Show(this, "确认", "输入值不在有效范围中", 0, "确定");
                return;
            }
            if (!analyze.Next())
            {
                ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
                rp.Resolve();
            }
        }

    }
}
