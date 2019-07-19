using Base.Mvvm;
using Exercise.Model;
using Exercise.Service;
using Exercise.View;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TalBase.View;
using static Exercise.Service.HistoryData;

namespace Exercise.ViewModel
{
    public class ExerciseViewModel : ScanViewModel
    {
        public class ClassDetail : HistoryData.ClassDetail
        {
            public int StudentCount { get; set; }
            public IList<StudentDetail> Result => SubmitStudentList;
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
            CloseMessage = "本次扫描结果未上传，退出后，扫描结果将作废，确认退出软件吗？";
            Update();
        }

        protected void Update()
        {
            StudentCount = exerciseModel.PageStudents.Where(s => s.AnswerPages.Any(p => p != null && p.PagePath != null)).Count();
            ClassDetails = schoolModel.Classes.Select(c => new ClassDetail()
            {
                Name = c.ClassName,
                StudentCount = c.Students.Count(),
                SubmitStudentList = c.Students.Where(s => s.AnswerPages != null)
                    .Select(s => new StudentDetail() { Name = s.Name, StudentNo = s.StudentNo }).ToList(),
                LostStudentList = c.Students.Where(s => s.AnswerPages == null)
                    .Select(s => new StudentDetail() { Name = s.Name, StudentNo = s.StudentNo }).ToList(),
            }).ToList();
            RaisePropertyChanged("StudentCount");
            RaisePropertyChanged("ClassDetails");
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
            int result = PopupDialog.Show("退出软件", CloseMessage, 1, "退出", "取消");
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
            int result = PopupDialog.Show(obj as UIElement, "放弃扫描任务", "放弃后，本次扫描结果将作废，确认放弃么？", 0, "放弃本次扫描", "取消");
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
