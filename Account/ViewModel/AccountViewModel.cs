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
using System.Collections.Generic;

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

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        private AccountModel accountModel = AccountModel.Instance;

        public AccountViewModel()
        {
            LoginData = AccountModel.Instance.LoginData;
            LoginCommand = new RelayCommand(DoLogin);
            LogoutCommand = new RelayCommand(DoLogout);
            accountModel.PropertyChanged += AccountModel_PropertyChanged;
        }

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
            (obj as Page).NavigationService.Navigate(new AccountPage());
        }

        private void AccountModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

    }

}
