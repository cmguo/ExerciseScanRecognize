using System.Windows;
using System.Windows.Controls;
using System.Linq;
using TalBase.ViewModel;
using static Exercise.Algorithm.AnswerData;
using static Exercise.Model.ExerciseModel;
using System.Collections.Generic;
using Base.Misc;
using Exercise.Algorithm;
using System;
using Exception = Exercise.Model.ExerciseModel.Exception;
using TalBase.View;
using static Exercise.Model.Page;

namespace Exercise.View.Resolve
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    internal class AnswerExceptionViewModel : ViewModelBase
    {
        private IList<QuestionException> _Questions;
        public IList<QuestionException> Questions
        {
            get => _Questions;
            set
            {
                _Questions = value;
                RaisePropertyChanged("Questions");
            }
        }

        private QuestionException _SelectedQuestion;
        public QuestionException SelectedQuestion
        {
            get => _SelectedQuestion;
            set
            {
                _SelectedQuestion = value;
                RaisePropertyChanged("SelectedQuestion");
            }
        }

        private IList<string> _Answers;
        public IList<string> Answers
        {
            get => _Answers;
            set
            {
                _Answers = value;
                RaisePropertyChanged("Answers");
            }
        }

        private string _SelectedAnswer;
        public string SelectedAnswer
        {
            get => _SelectedAnswer;
            set
            {
                _SelectedAnswer = value;
                RaisePropertyChanged("SelectedAnswer");
            }
        }

        private IList<object>_Scores;
        public IList<object> Scores
        {
            get => _Scores;
            set
            {
                _Scores = value;
                RaisePropertyChanged("Scores");
            }
        }

    }

    public partial class AnswerExceptionPage : Page
    {
        private const string NULL_ANSWER = "未作答";

        private AnswerExceptionViewModel viewModel;
        private ExceptionType type;
        private PageData data;

        public AnswerExceptionPage()
        {
            InitializeComponent();
            viewModel = FindResource("ViewModel") as AnswerExceptionViewModel;
            this.Loaded += AnswerExceptionPage_Initialized;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void AnswerExceptionPage_Initialized(object sender, System.EventArgs e)
        {
            DataTemplate qi = FindResource("QuestionItem") as DataTemplate;
            Exception ex = DataContext as Exception;
            type = ex.Type;
            data = ex.Page.MetaData;
            viewModel.Questions = type == ExceptionType.AnswerException 
                ? ex.Page.AnswerExceptions : ex.Page.CorrectionExceptions;
            viewModel.SelectedQuestion = viewModel.Questions[0];
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

        private void FillAnswers(QuestionException question)
        {
            viewModel.Answers = question.Question.ItemInfo[0].Value.Split(',').Concat(new string[] { NULL_ANSWER }).ToList();
        }

        private void FillScores(QuestionException question)
        {
            int total = (int) float.Parse(question.Question.ItemInfo[0].TotalScore);
            viewModel.Answers = Enumerable.Range(0, total + 1).Select(v => v.ToString()).ToList();
        }

        private void FillScores()
        {
            List<object> scores = new List<object>();
            for (int i = 1; i < 10; ++i)
                scores.Add(i);
            scores.Add(0);
            scores.Add(false);
            viewModel.Scores = scores;
        }

        private void Question_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedQuestion = (sender as FrameworkElement).DataContext as QuestionException;
        }

        private void Score_Click(object sender, RoutedEventArgs e)
        {
            object n = (sender as FrameworkElement).DataContext;
            if (n is Int32)
            {
                viewModel.SelectedAnswer += n.ToString();
            }
            else
            {
                if (viewModel.SelectedAnswer.Length > 0)
                    viewModel.SelectedAnswer = viewModel.SelectedAnswer.Substring(0, viewModel.SelectedAnswer.Length - 1);
            }
        }

        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedAnswer = (sender as FrameworkElement).DataContext as string;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!viewModel.Answers.Contains(viewModel.SelectedAnswer))
            {
                PopupDialog.Show(this, "TODO", "输入值不在有效范围中", 0, "确定");
                return;
            }
            viewModel.SelectedQuestion.Answer.ItemInfo.All(i =>
            {
                i.StatusOfItem = -1;
                i.AnalyzeResult = new List<Result>();
                if (viewModel.SelectedAnswer != NULL_ANSWER && viewModel.SelectedAnswer != "")
                {
                    i.AnalyzeResult.Add(new Result() { Value = viewModel.SelectedAnswer });
                }
                return true;
            });
            viewModel.SelectedQuestion.HasException = false;
            int n = viewModel.Questions.IndexOf(viewModel.SelectedQuestion);
            if (++n < viewModel.Questions.Count)
            {
                viewModel.SelectedQuestion = viewModel.Questions[n];
            }
            else
            {
                ResolvePage rp = UITreeHelper.GetParentOfType<ResolvePage>(this);
                rp.Resolve();
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedQuestion")
            {
                string answer = String.Join(",", viewModel.SelectedQuestion.Answer.ItemInfo
                    .Where(i => i.AnalyzeResult != null)
                    .SelectMany(i => i.AnalyzeResult.Select(r => r.Value))
                    .Where(v => v != null));
                if (type == ExceptionType.AnswerException)
                {
                    FillAnswers(viewModel.SelectedQuestion);
                    if (answer.Length == 0)
                        answer = NULL_ANSWER;
                    else if (answer.Length == 1)
                        answer = viewModel.Answers.Where(a => answer.Equals(a)).First();
                }
                else
                {
                    FillScores(viewModel.SelectedQuestion);
                }
                viewModel.SelectedAnswer = answer;
            }
        }

        private void TalRatio_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
