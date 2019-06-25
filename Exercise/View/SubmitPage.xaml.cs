using Base.Mvvm.Converter;
using System.Windows.Controls;
using static Exercise.Model.SubmitModel;

namespace Exercise.View
{

    internal class RetryConverter : VisibilityConverter
    {
        internal RetryConverter()
        {
            VisibleValues = new object[] { TaskStatus.Failed };
        }
    }

    internal class ReturnConverter : VisibilityConverter
    {
        internal ReturnConverter()
        {
            VisibleValues = new object[] { TaskStatus.Failed, TaskStatus.Completed };
        }
    }

    internal class SubmittingConverter : VisibilityConverter
    {
        internal SubmittingConverter()
        {
            VisibleValues = new object[] { TaskStatus.Submiting };
        }
    }

    internal class CompletedConverter : VisibilityConverter
    {
        internal CompletedConverter()
        {
            VisibleValues = new object[] { TaskStatus.Completed };
        }
    }


    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class SubmitPage : Page
    {

        public SubmitPage()
        {
            InitializeComponent();
        }

    }
}
