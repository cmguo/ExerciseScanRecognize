﻿using System.Windows;
using static Exercise.Model.ExerciseModel;
using System.Collections.Generic;
using Base.Misc;
using Exception = Exercise.Model.ExerciseModel.Exception;
using TalBase.View;
using Exercise.Model;

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
            this.Loaded += AnswerExceptionPage_Initialized;
        }

        private void AnswerExceptionPage_Initialized(object sender, System.EventArgs e)
        {
            Exception ex = DataContext as Exception;
            type = ex.Type;
            analyze = ex.Page.Analyze;
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
