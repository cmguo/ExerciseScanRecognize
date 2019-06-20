using Base.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            MessageBox.Show(e.Exception.ToString(), "错误", MessageBoxButton.OKCancel);
            e.IsHandled = true;
        }
    }
}
