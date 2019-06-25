using Account.Model;
using Account.Service;
using Base.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using TalBase.ViewModel;
using Panuon.UI;
using System.Windows.Controls;
using TalBase.Utils;

namespace Account.ViewModel
{
    public class AccountViewModel : ViewModelBase
    {
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        private AccountModel accountModel = AccountModel.Instance;

        public AccountViewModel()
        {
            CurrentUser = AccountModel.Instance.LoginData;
            LoginCommand = new RelayCommand(DoLogin);
            LogoutCommand = new RelayCommand(DoLogout);
        }

        public LoginData CurrentUser { get; private set; }

        private async Task DoLogin(object obj)
        {
            if (NetWorkManager.CheckNetWorkAvailable())
            {
                await AccountModel.Instance.Login();
                (obj as Page).NavigationService.Navigate(new Uri(Configuration.StartupPage));
            }
        }

        private async Task DoLogout(object obj)
        {
            await AccountModel.Instance.Logout();
        }
    }

}
