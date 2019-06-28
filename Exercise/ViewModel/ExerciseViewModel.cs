using Base.Misc;
using Base.Mvvm;
using Exercise.Model;
using Exercise.View;
using MyToolkit.Collections;
using Panuon.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TalBase.View;
using static Exercise.Model.ExerciseModel;

namespace Exercise.ViewModel
{
    public class ExerciseViewModel : ScanViewModel
    {
        public class ClassDetail
        {
            public string ClassName { get; set; }
            public int StudentCount { get; set; }
            public int ResultCount { get; set; }
        }

        #region Properties

        public int StudentCount { get; private set; }
        public List<ClassDetail> ClassDetails { get; private set; }

        #endregion

        #region Commands

        public RelayCommand DiscardCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }

        protected string CloseMessage { get; set; }

        #endregion

        private ScanModel scanModel = ScanModel.Instance;
        private ExerciseModel exerciseModel = ExerciseModel.Instance;
        private SchoolModel schoolModel = SchoolModel.Instance;

        public ExerciseViewModel()
        {
            DiscardCommand = new RelayCommand((e) => Discard(e));
            CloseCommand = new RelayCommand((e) => Close(e));
            exerciseModel.PropertyChanged += ExerciseModel_PropertyChanged;
            CloseMessage = "退出后本次扫描结果将放弃，确认退出吗？";
            Update();
        }

        protected void Update()
        {
            StudentCount = exerciseModel.PageStudents.Where(s => s.AnswerPages.All(p => p != null && p.PagePath != null)).Count();
            ClassDetails = schoolModel.Classes.Select(c => new ClassDetail()
            {
                ClassName = c.ClassName,
                StudentCount = c.Students.Count(),
                ResultCount = c.Students.Where(
                    s => s.AnswerPages != null && s.AnswerPages.All(p => p != null && p.PagePath != null)).Count(),
            }).ToList();
        }

        public override void Release()
        {
            base.Release();
            ClassDetails = null;
            exerciseModel.PropertyChanged -= ExerciseModel_PropertyChanged;
        }

        protected virtual async Task Close(object obj)
        {
            bool paused = await scanModel.PauseScan();
            int result = PopupDialog.Show(CloseMessage, 1, "退出", "取消");
            if (result == 0)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                Window.GetWindow((obj as ExecutedRoutedEventArgs).OriginalSource as UIElement).Close();
            }
            else if (paused)
            {
                scanModel.ResumeScan();
                //(obj as Route)
            }
        }

        protected async Task Discard(object obj)
        {
            bool paused = await scanModel.PauseScan();
            int result = PopupDialog.Show("放弃后，本次扫描结果将作废，确认放弃吗？", 0, "放弃本次扫描", "取消");
            if (result == 0)
            {
                await scanModel.CancelScan();
                exerciseModel.Discard();
                (obj as System.Windows.Controls.Page).NavigationService.Navigate(new HomePage());
            }
            else if (paused)
            {
                scanModel.ResumeScan();
            }
        }

        private void ExerciseModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExerciseData")
            {
                if (exerciseModel.ExerciseData == null)
                    Error = 2;
            }
        }
    }
}
