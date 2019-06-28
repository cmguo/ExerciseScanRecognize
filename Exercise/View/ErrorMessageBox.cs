using Base.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TalBase.View;

namespace Exercise.View
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
            PopupDialog.Show(e.Exception.Message, 0, "确定");
            e.IsHandled = true;
        }
    }
}
