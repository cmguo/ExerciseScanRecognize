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
            HiddenValues = new object[0];
        }
    }

    internal class ReturnConverter : VisibilityConverter
    {
        internal ReturnConverter()
        {
            VisibleValues = new object[] { TaskStatus.Failed, TaskStatus.Completed };
            HiddenValues = new object[0];
        }
    }

    internal class SubmittingConverter : VisibilityConverter
    {
        internal SubmittingConverter()
        {
            VisibleValues = new object[] { TaskStatus.Submiting };
            HiddenValues = new object[0];
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
