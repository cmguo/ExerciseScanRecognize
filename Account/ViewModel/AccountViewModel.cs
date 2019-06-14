using Account.Model;
using Account.Service;
using Base.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using TalBase.ViewModel;

namespace Account.ViewModel
{
    public class AccountViewModel : ViewModelBase
    {
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        public AccountViewModel()
        {
            CurrentUser = AccountModel.Instance.LoginData;
            LoginCommand = new RelayCommand(DoLogin);
            LogoutCommand = new RelayCommand(DoLogout, CanDoAuthenticated);
        }

        private bool _IsAuthenticated;
        public bool IsAuthenticated
        {
            get { return _IsAuthenticated; }
            set
            {
                if (value != _IsAuthenticated)
                {
                    _IsAuthenticated = value;
                    RaisePropertyChanged("IsAuthenticated");
                    RaisePropertyChanged("IsNotAuthenticated");
                }
            }
        }

        public bool IsNotAuthenticated
        {
            get
            {
                return !IsAuthenticated;
            }
        }

        public bool CanDoAuthenticated(object ignore)
        {
            return IsAuthenticated;
        }

        public LoginData CurrentUser { get; private set; }

        private async Task DoLogin(object obj)
        {
            await AccountModel.Instance.Login();
            (obj as NavigationWindow).Navigate(new Uri(Configuration.StartupPage));
        }

        private bool CanDoLogout(object obj)
        {
            return IsAuthenticated;
        }

        private async Task DoLogout(object obj)
        {
            await AccountModel.Instance.Logout();
        }
    }

}
