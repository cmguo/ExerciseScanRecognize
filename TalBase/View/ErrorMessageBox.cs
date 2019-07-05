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
            RelayCommand.ActionException += RelayCommand_ActionException;
            BackgroudWork.WorkException += RelayCommand_ActionException;
        }

        private static void RelayCommand_ActionException(object sender, RelayCommand.ActionExceptionEventArgs e)
        {
            if (e.Parameter is UIElement)
            {
                PopupDialog.Show(e.Parameter as UIElement, GetMessage(e.Exception), 0, "确定");
            }
            else
            {
                PopupDialog.Show(GetMessage(e.Exception), 0, "确定");
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
