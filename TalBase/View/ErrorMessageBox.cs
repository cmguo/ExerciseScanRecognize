using Base.Mvvm;
using System;
using System.Net.Http;
using System.Windows;

namespace TalBase.View
{
    public class ErrorMessageBox
    {
        public static void Init()
        {
            Base.Mvvm.Action.ActionException += Action_ActionException;
        }

        private static void Action_ActionException(object sender, RelayCommand.ActionExceptionEventArgs e)
        {
            if (e.Parameter is UIElement)
            {
                PopupDialog.Show(e.Parameter as UIElement, "出现异常", GetMessage(e.Exception), 0, "确定");
            }
            else
            {
                PopupDialog.Show(Application.Current.MainWindow, "出现异常", GetMessage(e.Exception), 0, "确定");
            }
            e.IsHandled = true;
        }

        private static string GetMessage(Exception e)
        {
            if (e is HttpRequestException)
                return "网络异常，" + e.Message;
            return e.Message;
        }
    }
}
