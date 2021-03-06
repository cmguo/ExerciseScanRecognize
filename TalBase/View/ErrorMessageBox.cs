using Base.Mvvm;
using Base.Service;
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

        private static bool isShowing = false;

        private static void Action_ActionException(object sender, RelayCommand.ActionExceptionEventArgs e)
        {
            if (!isShowing)
            {
                isShowing = true;
                if (e.Parameter is UIElement)
                {
                    PopupDialog.Show(e.Parameter as UIElement, "出现异常", GetMessage(e.Exception), 0, "确定");
                }
                else
                {
                    PopupDialog.Show(Application.Current.MainWindow, "出现异常", GetMessage(e.Exception), 0, "确定");
                }
                isShowing = false;
            }
            e.IsHandled = true;
        }

        private static string GetMessage(Exception e)
        {
            if (e is HttpRequestException)
                return "网络异常，" + e.Message;
            else if (e is HttpResponseException)
                return "服务异常，" + e.Message;
            return e.Message;
        }
    }
}
