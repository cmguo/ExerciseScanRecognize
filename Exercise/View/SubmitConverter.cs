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
using static Exercise.Model.SubmitModel;
using Page = Exercise.Model.Page;

namespace Exercise.View
{

    [ValueConversion(typeof(ExerciseModel.Exception), typeof(string))]
    public class SubmitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SubmitModel.TaskStatus? status = (SubmitModel.TaskStatus?)value;
            if (status == null)
                return null;
            if ((string)parameter == "Icon")
            {
                switch (status)
                {
                    case SubmitModel.TaskStatus.Failed:
                        return "/Icons/Error/24.png";
                    case SubmitModel.TaskStatus.Completed:
                        return "/Icons/Finish/24.png";
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Title")
            {
                switch (status)
                {
                    case SubmitModel.TaskStatus.Submiting:
                        return "扫描结果上传中，请勿关闭客户端...";
                    case SubmitModel.TaskStatus.Failed:
                        return "上传失败，网络存在异常，请检查后重新上传";
                    case SubmitModel.TaskStatus.Completed:
                        return "扫描结果上传成功";
                    default:
                        return null;
                }
            }
            else if ((string)parameter == "Message")
            {
                switch (status)
                {
                    case SubmitModel.TaskStatus.Failed:
                        return "本次扫描结果已经保存，后续可在“扫描记录管理”中重新上传";
                    case SubmitModel.TaskStatus.Completed:
                        return "稍后您可在网页端测试记录页面查看本次扫描结果";
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
