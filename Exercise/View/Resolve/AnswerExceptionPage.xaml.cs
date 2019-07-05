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

namespace Exercise.View.Resolve
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    internal class AnswerExceptionViewModel : ViewModelBase
    {
        private IList<Question> _Questions;
        public IList<Question> Questions
        {
            get => _Questions;
            set
            {
                _Questions = value;
                RaisePropertyChanged("Questions");
            }
        }

        private Question _SelectedQuestion;
        public Question SelectedQuestion
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

    }

    public partial class AnswerExceptionPage : Page
    {

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
                ? ex.Page.Answer.AnswerExceptions : ex.Page.Answer.CorrectionExceptions;
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

        private void FillAnswers(Question question)
        {
            PageData.Question qe = data.AreaInfo.SelectMany(a => a.QuestionInfo)
                .Where(q => q.QuestionId == question.QuestionId).First();
            viewModel.Answers = qe.ItemInfo[0].Value.Split(',').Concat(new string[] { null }).ToList();
        }

        private void FillScores()
        {
            List<string> answers = new List<string>();
            for (int i = 1; i < 10; ++i)
                answers.Add(i.ToString());
            answers.Add("0");
            answers.Add("<=删除");
            viewModel.Answers = answers;
        }

        private void Question_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedQuestion = (sender as FrameworkElement).DataContext as Question;
        }

        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            if (type == ExceptionType.AnswerException)
            {
                viewModel.SelectedAnswer = (sender as FrameworkElement).DataContext as string;
            }
            else
            {
                string n = (string)(sender as FrameworkElement).DataContext;
                if (n.Length == 1)
                {
                    viewModel.SelectedAnswer += n;
                }
                else
                {
                    if (viewModel.SelectedAnswer.Length > 0)
                        viewModel.SelectedAnswer = viewModel.SelectedAnswer.Substring(0, viewModel.SelectedAnswer.Length - 1);
                }
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedQuestion.ItemInfo.All(i =>
            {
                i.StatusOfItem = -1;
                i.AnalyzeResult = new List<Result>();
                if (viewModel.SelectedAnswer != null && viewModel.SelectedAnswer != "")
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
                string answer = String.Join(",", viewModel.SelectedQuestion.ItemInfo
                    .Where(i => i.AnalyzeResult != null)
                    .SelectMany(i => i.AnalyzeResult.Select(r => r.Value))
                    .Where(v => v != null));
                if (type == ExceptionType.AnswerException)
                {
                    FillAnswers(viewModel.SelectedQuestion);
                    if (answer.Length == 0)
                        answer = null;
                    else if (answer.Length == 1)
                        answer = viewModel.Answers.Where(a => answer.Equals(a)).First();
                }
                viewModel.SelectedAnswer = answer;
            }
        }

    }
}
