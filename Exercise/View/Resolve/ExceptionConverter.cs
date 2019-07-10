using Exercise.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using static Exercise.Model.ExerciseModel;
using Page = Exercise.Model.Page;

namespace Exercise.View.Resolve
{

    [ValueConversion(typeof(ExerciseModel.Exception), typeof(string))]
    public class ExceptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ExerciseModel.Exception ex = value as Model.ExerciseModel.Exception;
            if (ex == null)
                return null;
            Page page = ex.Page;
            if ((string)parameter == "Tree")
            {
                switch (ex.Type)
                {
                    case ExceptionType.NoPageCode:
                    case ExceptionType.PageCodeMissMatch:
                        return "未识别试卷" + ex.Index;
                    case ExceptionType.NoStudentCode:
                        return String.Format("试卷{2} （{0}-{1}页）", page.PageIndex + 1, page.PageIndex + 2, ex.Index);
                    case ExceptionType.AnalyzeException:
                        if (page.Student != null)
                            return String.Format("{0} {1} （第{2}页）", page.Student.TalNo, page.Student.Name, page.PageIndex + 1);
                        else
                            return String.Format("试卷{2} （{0}-{1}页）", page.PageIndex + 1, page.PageIndex + 2, ex.Index);
                    case ExceptionType.AnswerException:
                    case ExceptionType.CorrectionException:
                        return String.Format("{0} {1} （第{2}页）", page.Student.TalNo, page.Student.Name, page.PageIndex + 1);
                    case ExceptionType.PageLost:
                        return String.Format("{0} {1} （{2}-{3}页）", page.Student.TalNo, page.Student.Name, page.PageIndex + 1, page.PageIndex + 2);
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Title")
            {
                switch (ex.Type)
                {
                    case ExceptionType.NoPageCode:
                    case ExceptionType.PageCodeMissMatch:
                        return "此份试卷无法识别，存在以下异常，请放入此试卷重新扫描。（请勿放入他人试卷）";
                    case ExceptionType.NoStudentCode:
                        return "该试卷所属的学生是？";
                    case ExceptionType.AnalyzeException:
                        return String.Format("{0}{1}试卷无法识别，存在以下异常，请放入此试卷重新扫描。（请勿放入他人试卷）",
                            page.Student.TalNo, page.Student.Name);
                    case ExceptionType.AnswerException:
                        return String.Format("{0}{1} （第{2}页），以下题号存在作答识别异常，请确认识别结果。",
                            page.Student.TalNo, page.Student.Name, page.PageIndex + 1);
                    case ExceptionType.CorrectionException:
                        return String.Format("{0}{1} （第{2}页），以下题号存在批改识别异常，请确认得分。",
                            page.Student.TalNo, page.Student.Name, page.PageIndex + 1);
                    case ExceptionType.PageLost:
                        return String.Format("{0} {1} （{2}-{3}页）缺失，请放入其试卷重新扫描。（请勿放入他人试卷）",
                            page.Student.TalNo, page.Student.Name, page.PageIndex + 1, page.PageIndex + 2);
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Message")
            {
                if (page.Exception != null)
                    return page.Exception.Message;
                else if (ex.Type == ExceptionType.PageCodeMissMatch)
                    return "该试卷非本校试卷";
                else
                    return null;
            }
            else if ((string)parameter == "Rescan")
            {
                switch (ex.Type)
                {
                    case ExceptionType.NoPageCode:
                    case ExceptionType.PageCodeMissMatch:
                        return "扫描此份试卷";
                    case ExceptionType.AnalyzeException:
                    case ExceptionType.PageLost:
                        return String.Format("扫描{0}的试卷", page.Student.Name);
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "RemovePage")
            {
                switch (ex.Type)
                {
                    case ExceptionType.NoPageCode:
                    case ExceptionType.PageCodeMissMatch:
                    case ExceptionType.AnalyzeException:
                    case ExceptionType.NoStudentCode:
                        return "忽略此异常";
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Ignore")
            {
                switch (ex.Type)
                {
                    case ExceptionType.AnswerException:
                    case ExceptionType.CorrectionException:
                        return "将本页异常题目均设为0分";
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
