using Account.Service;
using Base.Misc;
using Base.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TalBase.Model;
using TalBase.Service;

namespace Account.Model
{
    public class AccountModel : ModelBase
    {

        private static readonly Logger Log = Logger.GetLogger<AccountModel>();

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
            ServiceUris.Add("开发环境", "http://zx.idev.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUris.Add("联调环境", "http://zx.iunion.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUris.Add("测试环境", "http://zx.itest.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUris.Add("线上环境", "http://zx.ipub.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUris.Add("演示环境", "http://zx.ishow.talcloud.com/homework/api/v1/answerCardApp");
            ServiceUri = Configuration.ServiceUri;
            _SelectedServiceUri = ServiceUris.Values.ToList().IndexOf(Configuration.ServiceUri);
            //LoginData = new LoginData() { LoginName = "huanglaoshi3", Password = "2019@100tal",
            //    AuthenticationType = LoginData.LOGIN_BY_PASSWORD };
            LoginData = new LoginData() { LoginName = Configuration.AccountName,
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
            finally
            {
                LoginData.Password = password;
            }
            RaisePropertyChanged("Account");
            LoginData.Password = null;
            LoginData.Ticket = Account.Ticket;
            LoginData.AuthenticationType = LoginData.LOGIN_BY_TICKET;
            Configuration.AccountName = LoginData.LoginName;
            timer.Start();
            Base.Mvvm.Action.ActionException += Action_ActionException;
        }

        public async Task Logout()
        {
            LogoutData logout = new LogoutData() { Ticket = Account.Ticket };
            try
            {
                await service.Logout(logout);
            }
            catch (Exception e)
            {
                Log.w("Logout", e);
            }
            Clear();
        }

        public void Clear()
        {
            Account = new AccountData();
            LoginData.AuthenticationType = LoginData.LOGIN_BY_PASSWORD;
            LoginData.Ticket = null;
            timer.Stop();
            RelayCommand.ActionException -= Action_ActionException;
            RaisePropertyChanged("Account");
        }

        private async Task ReLogin()
        {
            try
            {
                await Login();
            }
            catch (Exception e)
            {
                Log.w("ReLogin", e);
                Clear();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            BackgroundWork.Execute(() => ReLogin());
        }

        private void Action_ActionException(object sender, RelayCommand.ActionExceptionEventArgs e)
        {
            if (e.Exception is ServiceException && ((e.Exception as ServiceException).Status >= LoginData.LOGIN_OUT_FIRST
                && (e.Exception as ServiceException).Status < LoginData.LOGIN_OUT_LAST))
            {
                Clear();
                e.IsHandled = true;
            }
        }

    }
}
