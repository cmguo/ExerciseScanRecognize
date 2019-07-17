using Account.Service;
using Base.Mvvm;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using TalBase.Model;
using TalBase.Service;

namespace Account.Model
{
    public class AccountModel : ModelBase
    {

        private static AccountModel s_instance;
        public static AccountModel Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new AccountModel();
                }
                return s_instance;
            }
        }

        public string ServiceUri { get; private set; }

        public Dictionary<string, string> ServiceUris { get; private set; }

        private int _SelectedServiceUri;
        public int SelectedServiceUri {
            get => _SelectedServiceUri;
            set
            {
                _SelectedServiceUri = value;
                string uri = ServiceUris.ElementAt(value).Value;
                ServiceUri = uri;
                Configuration.ServiceUri = uri;
            }
        }

        public LoginData LoginData { get; private set; }

        public AccountData Account { get; private set; }

        private IAccount service;

        private DispatcherTimer timer;

        public AccountModel()
        {
            ServiceUris = new Dictionary<string, string>();
            ServiceUris.Add("开发环境", "http://homework.idev.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUris.Add("测试环境", "http://homework.itest.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUris.Add("线上环境", "http://homework.ipub.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUri = Configuration.ServiceUri;
            _SelectedServiceUri = ServiceUris.Values.ToList().IndexOf(Configuration.ServiceUri);
            //LoginData = new LoginData() { LoginName = "huanglaoshi3", Password = "2019@100tal",
            //    AuthenticationType = LoginData.LOGIN_BY_PASSWORD };
            LoginData = new LoginData() { LoginName = "xujinming", Password = "123@qwe",
                AuthenticationType = LoginData.LOGIN_BY_PASSWORD };
            Account = new Service.AccountData();
            service = Base.Service.Services.Get<IAccount>();
            timer = new DispatcherTimer() { Interval = TimeSpan.FromHours(6) };
            timer.Tick += Timer_Tick;
        }

        public async Task Login()
        {
            string password = LoginData.Password;
            if (LoginData.AuthenticationType == LoginData.LOGIN_BY_PASSWORD)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] output = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                LoginData.Password = BitConverter.ToString(output).Replace("-", "").ToLower();
            }
            try
            {
                Account = await service.Login(LoginData);
            }
            catch (Exception e)
            {
                LoginData.Password = password;
                throw e;
            }
            RaisePropertyChanged("Account");
            LoginData.Password = null;
            LoginData.AuthenticationType = LoginData.LOGIN_BY_TICKET;
            timer.Start();
            Base.Mvvm.Action.ActionException += RelayCommand_ActionException;
        }

        public async Task Logout()
        {
            LogoutData logout = new LogoutData() { Ticket = Account.Ticket };
            await service.Logout(logout);
            Clear();
        }

        public void Clear()
        {
            Account = new AccountData();
            LoginData.AuthenticationType = LoginData.LOGIN_BY_PASSWORD;
            RaisePropertyChanged("Account");
            timer.Stop();
            RelayCommand.ActionException -= RelayCommand_ActionException;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            BackgroudWork.Execute(() => Login());
        }

        private void RelayCommand_ActionException(object sender, RelayCommand.ActionExceptionEventArgs e)
        {
            if (e.Exception is ServiceException && ((e.Exception as ServiceException).Status >= LoginData.LOGIN_OUT_FIRST
                && (e.Exception as ServiceException).Status < LoginData.LOGIN_OUT_LAST))
            {
                Clear();
            }
        }

    }
}
