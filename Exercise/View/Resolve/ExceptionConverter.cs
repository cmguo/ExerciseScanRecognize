using Exercise.Model;
using System;
using System.Globalization;
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
                        return page.Student != null
                            ? String.Format("{0} {1}", page.Student.StudentNo, page.Student.Name)
                            : String.Format("未识别试卷{0}", ex.Index);
                    case ExceptionType.NoStudentCode:
                    case ExceptionType.StudentCodeMissMatch:
                        if (page.Another != null)
                            return String.Format("试卷{2} （{0}-{1}页）", page.PageIndex + 1, page.PageIndex + 2, ex.Index);
                        else
                            return String.Format("试卷{1} （第{0}页）", page.PageIndex + 1, ex.Index);
                    case ExceptionType.PageCodeMissMatch:
                    case ExceptionType.AnalyzeException:
                        string student = page.Student != null 
                            ? String.Format("{0} {1}", page.Student.StudentNo, page.Student.Name)
                            : String.Format("未识别试卷{0}", ex.Index);
                        if (page.Another != null)
                            return String.Format("{0} （{1}-{2}页）", student, page.PageIndex + 1, page.PageIndex + 2);
                        else
                            return String.Format("{0} （第{1}页）", student, page.PageIndex + 1);
                    case ExceptionType.AnswerException:
                    case ExceptionType.CorrectionException:
                        return String.Format("{0} {1} （第{2}页）", page.Student.StudentNo, page.Student.Name, page.PageIndex + 1);
                    case ExceptionType.PageLost:
                        if (page.Another != null)
                            return String.Format("{0} {1} （{2}-{3}页）", page.Student.StudentNo, page.Student.Name, page.PageIndex + 1, page.PageIndex + 2);
                        else
                            return String.Format("{0} {1} （第{2}页）", page.Student.StudentNo, page.Student.Name, page.PageIndex + 1);
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
                    case ExceptionType.AnalyzeException:
                        if (page.Student != null)
                            return String.Format("{0}{1}试卷无法识别，存在以下异常，请放入此试卷重新扫描。",
                                page.Student.StudentNo, page.Student.Name);
                        else
                            return String.Format("此份试卷无法识别，存在以下异常，请放入此试卷重新扫描。");
                    case ExceptionType.NoStudentCode:
                    case ExceptionType.StudentCodeMissMatch:
                        return "该试卷学生信息存在下列异常，请匹配学生信息。";
                    case ExceptionType.AnswerException:
                        return String.Format("{0}{1} （第{2}页），以下题号存在作答识别异常，请确认识别结果。",
                            page.Student.StudentNo, page.Student.Name, page.PageIndex + 1);
                    case ExceptionType.CorrectionException:
                        return String.Format("{0}{1} （第{2}页），以下题号存在批改识别异常，请确认得分。",
                            page.Student.StudentNo, page.Student.Name, page.PageIndex + 1);
                    case ExceptionType.PageLost:
                        return String.Format("{0} {1} （{2}-{3}页）缺失，请放入其试卷重新扫描。",
                            page.Student.StudentNo, page.Student.Name, page.PageIndex + 1, page.PageIndex + 2);
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Message")
            {
                if (page.Exception != null)
                    return page.Exception.Message;
                else if (page.Another != null && page.Another.Exception != null)
                    return page.Another.Exception.Message;
                else if (ex.Type == ExceptionType.PageCodeMissMatch)
                    return "该试卷二维码与本此考试不匹配";
                else if (ex.Type == ExceptionType.NoStudentCode)
                    return "学生二维码未识别";
                else if (ex.Type == ExceptionType.StudentCodeMissMatch)
                    return "该学生不在本校范围";
                else
                    return null;
            }
            else if ((string)parameter == "Message2")
            {
                if (ex.Type == ExceptionType.AnswerException)
                    return "本题的作答结果为：";
                else if (ex.Type == ExceptionType.CorrectionException)
                    return "本题的得分为：";
                else
                    return null;
            }
            else if ((string)parameter == "Rescan")
            {
                switch (ex.Type)
                {
                    case ExceptionType.NoPageCode:
                    case ExceptionType.PageCodeMissMatch:
                    case ExceptionType.AnalyzeException:
                        if (page.Student != null)
                            return String.Format("扫描{0}的试卷", page.Student.Name);
                        else
                            return "扫描此份试卷";
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
                    case ExceptionType.StudentCodeMissMatch:
                    case ExceptionType.PageLost:
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
