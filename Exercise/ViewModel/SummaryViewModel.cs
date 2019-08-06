using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using System.Threading.Tasks;

namespace Exercise.ViewModel
{
    class SummaryViewModel : ExerciseViewModel
    {
        public RelayCommand ResolveCommand { get; set; }
        public RelayCommand SubmitCommand { get; set; }

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SchoolModel schoolModel = SchoolModel.Instance;

        public SummaryViewModel() 
        {
            ResolveCommand = new RelayCommand((e) => Resolve(e));
            SubmitCommand = new RelayCommand((e) => Submit(e));
            CloseMessage = "本次扫描结果未上传，" + CloseMessage;
        }

        private void Resolve(object obj)
        {
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ResolvePage());
        }

        private async Task Submit(object obj)
        {
            await exerciseModel.SubmitResult();
            SubmitPage page = new SubmitPage();
            exerciseModel.Clear();
            (obj as System.Windows.Controls.Page).NavigationService.Navigate(page);
        }

        protected override bool Continue(object obj)
        {
            bool result = base.Continue(obj);
            if (result)
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new ScanningPage());
            return result;
        }

    }
}
