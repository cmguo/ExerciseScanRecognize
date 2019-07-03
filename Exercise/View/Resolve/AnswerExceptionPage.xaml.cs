using System.Windows;
using System.Windows.Controls;
using System.Linq;
using TalBase.ViewModel;
using static Exercise.Algorithm.AnswerData;
using static Exercise.Model.ExerciseModel;
using System.Collections.Generic;
using Base.Misc;
using Exercise.Algorithm;

namespace Exercise.View.Resolve
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    internal class AnswerExceptionViewModel : ViewModelBase
    {
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
        private IList<Question> answerExceptions;

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
            answerExceptions = type == ExceptionType.AnswerException 
                ? ex.Page.Answer.AnswerExceptions : ex.Page.Answer.CorrectionExceptions;
            foreach (var ae in answerExceptions)
            {
                Button bt = qi.LoadContent() as Button;
                bt.DataContext = ae;
                questions.Children.Add(bt);
            }
            viewModel.SelectedQuestion = answerExceptions[0];
            if (type == ExceptionType.AnswerException)
                correct.Visibility = Visibility.Collapsed;
            else
                answers.Visibility = Visibility.Collapsed;
            FillScores();
        }

        private void FillAnswers(Question question)
        {
            DataTemplate ai = FindResource("AnswerItem") as DataTemplate;
            PageData.Question qe = data.AreaInfo.SelectMany(a => a.QuestionInfo)
                .Where(q => q.QuestionId == question.QuestionId).First();
            answers.Children.Clear();
            foreach (string ae in qe.ItemInfo[0].Value.Split(','))
            {
                Button bt = ai.LoadContent() as Button;
                bt.DataContext = ae;
                answers.Children.Add(bt);
            }
            Button bt2 = ai.LoadContent() as Button;
            bt2.DataContext = null;
            answers.Children.Add(bt2);
        }

        private void FillScores()
        {
            DataTemplate ai = FindResource("AnswerItem") as DataTemplate;
            for (int i = 1; i < 10; ++i)
            {
                Button bt = ai.LoadContent() as Button;
                bt.DataContext = i.ToString();
                numbers.Children.Add(bt);
            }
            Button bt1 = ai.LoadContent() as Button;
            bt1.DataContext = "0";
            numbers.Children.Add(bt1);
            Button bt2 = ai.LoadContent() as Button;
            bt2.DataContext = "<-删除";
            numbers.Children.Add(bt2);
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
                int n = numbers.Children.IndexOf(sender as UIElement);
                if (n < 10)
                {
                    viewModel.SelectedAnswer += (string) (sender as FrameworkElement).DataContext;
                }
                else if (n == 10)
                {
                    viewModel.SelectedAnswer = viewModel.SelectedAnswer.Substring(0, viewModel.SelectedAnswer.Length - 1);
                }
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedQuestion.ItemInfo.All(i =>
            {
                i.StatusOfItem = -1;
                i.AnalyzeResult = null;
                if (viewModel.SelectedAnswer != null && viewModel.SelectedAnswer != "")
                {
                    i.AnalyzeResult = new List<Result>();
                    i.AnalyzeResult.Add(new Result() { Value = viewModel.SelectedAnswer });
                }
                return true;
            });
            viewModel.SelectedQuestion.HasException = false;
            int n = answerExceptions.IndexOf(viewModel.SelectedQuestion);
            if (++n < answerExceptions.Count)
            {
                viewModel.SelectedQuestion = answerExceptions[n];
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
                if (type == ExceptionType.AnswerException)
                    FillAnswers(viewModel.SelectedQuestion);
                else
                    viewModel.SelectedAnswer = "";
            }
        }

    }
}
