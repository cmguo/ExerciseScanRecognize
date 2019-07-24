using Account.Model;
using Account.Service;
using Base.Mvvm;
using System;
using System.Threading.Tasks;
using TalBase.ViewModel;
using TalBase.Utils;
using System.Collections.Generic;
using System.Windows;

namespace Account.ViewModel
{
    public class AccountViewModel : ViewModelBase
    {

        public ICollection<string> ServiceUris => accountModel.ServiceUris.Keys;

        public int SelectedServiceUri
        {
            get => accountModel.SelectedServiceUri;
            set => accountModel.SelectedServiceUri = value;
        }

        public LoginData LoginData { get; private set; }

        public AccountData Account => accountModel.Account;

        public Exception LoginException { get; set; }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        private AccountModel accountModel = AccountModel.Instance;

        static AccountViewModel()
        {
            AccountModel.Instance.PropertyChanged += AccountModel_PropertyChanged_Static;
        }

        public AccountViewModel()
        {
            LoginData = AccountModel.Instance.LoginData;
            LoginCommand = new RelayCommand(DoLogin);
            LogoutCommand = new RelayCommand(DoLogout);
            accountModel.PropertyChanged += AccountModel_PropertyChanged;
        }

        public override void Release()
        {
            base.Release();
            accountModel.PropertyChanged -= AccountModel_PropertyChanged;
        }

        private async Task DoLogin(object obj)
        {
            if (LoginData.Password == "" || LoginData.LoginName == "")
            {
                throw new Exception("请输入账号密码");

            }
            if (NetWorkManager.CheckNetWorkAvailable())
            {
                await AccountModel.Instance.Login();
                Window window = Application.Current.MainWindow;
                if (window != null)
                    window.Show();
                (obj as Window).Close();
            }
            else
            {
                throw new Exception("网络异常，请检查后重试");
            }
            
        }

        private async Task DoLogout(object obj)
        {
            await AccountModel.Instance.Logout();
            Window window = Application.Current.MainWindow;
            window.Hide();
        }

        private void AccountModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        private static void AccountModel_PropertyChanged_Static(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Account" && AccountModel.Instance.Account.Ticket == null)
            {
                new AccountWindow() { Owner = Application.Current.MainWindow }.Show();
            }
        }

    }

}
