using System.Windows;
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
            Loaded += AnswerExceptionPage_Loaded;
            Unloaded += AnswerExceptionPage_Unloaded;
            answers.SelectionChanged += Answers_SelectionChanged;
        }

        private void AnswerExceptionPage_Loaded(object sender, System.EventArgs e)
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

        private void AnswerExceptionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            analyze.PropertyChanged -= Analyze_PropertyChanged;
        }

        private void Analyze_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedException")
            {
                answers.UnselectAll();
                if (analyze.SelectedException == null)
                    return;
                foreach (char c in analyze.SelectedException.SelectedAnswer)
                {
                    ListViewItem item = answers.ItemContainerGenerator.ContainerFromItem(c.ToString()) as ListViewItem;
                    if (item != null)
                        item.IsSelected = true;
                }
                if (answers.SelectedItems.Count == 0)
                {
                    ListViewItem item = answers.ItemContainerGenerator.ContainerFromItem(PageAnalyze.NULL_ANSWER) as ListViewItem;
                    if (item != null)
                        item.IsSelected = true;
                }
                ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
                rp.SetPaperFocusRect(PaperOverlayConverter.MakeRect(analyze.SelectedException.Location, 0));
            }
        }

        private void Answers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (answers.SelectedItems.Count == 0)
            {
                ListViewItem item = answers.ItemContainerGenerator.ContainerFromItem(PageAnalyze.NULL_ANSWER) as ListViewItem;
                if (item != null)
                    item.IsSelected = true;
            }
            else if (e.AddedItems.Contains(PageAnalyze.NULL_ANSWER))
            {
                foreach (string answer in answers.Items)
                {
                    if (answer == PageAnalyze.NULL_ANSWER)
                        continue;
                    ListViewItem item = answers.ItemContainerGenerator.ContainerFromItem(answer) as ListViewItem;
                    if (item != null)
                        item.IsSelected = false;
                }
            }
            else if (e.AddedItems.Count > 0)
            {
                ListViewItem item = answers.ItemContainerGenerator.ContainerFromItem(PageAnalyze.NULL_ANSWER) as ListViewItem;
                if (item != null)
                    item.IsSelected = false;
            }
        }

        private void FillScores()
        {
            List<object> scores = new List<object>();
            for (int i = 1; i < 10; ++i)
                scores.Add((char)('0' + i));
            scores.Add('0');
            scores.Add('.');
            scores.Add(false);
            numbers.ItemsSource = scores;
        }

        private void Score_Click(object sender, RoutedEventArgs e)
        {
            object n = (sender as FrameworkElement).DataContext;
            if (n is char)
            {
                score.SelectedText = n.ToString();
                ++score.SelectionStart;
                score.SelectionLength = 0;
            }
            else
            {
                if (score.SelectionStart > 0 && score.SelectionLength == 0)
                {
                    --score.SelectionStart;
                    ++score.SelectionLength;
                }
                score.SelectedText = "";
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (type == ExceptionType.AnswerException)
            {
                analyze.SelectedException.SelectedAnswer = String.Join("",
                    answers.SelectedItems.Cast<String>());
            }
            try
            {
                analyze.Confirm();
                if (!analyze.Next())
                {
                    ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
                    rp.Resolve();
                }
            }
            catch (System.Exception ex)
            {
                msg.Text = ex.Message;
            }
        }

    }
}
