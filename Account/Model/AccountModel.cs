using Account.Service;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Account.Model
{
    public class AccountModel
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

        public LoginData LoginData { get; private set; }

        public Service.AccountData Account { get; private set; }

        private IAccount Service;

        public AccountModel()
        {
            LoginData = new LoginData() { LoginName = "18638217959", Password = "123@qwe",
                AuthenticationType = LoginData.LOGIN_BY_PASSWORD };
            Account = new Service.AccountData();
            Service = Base.Service.Services.Get<IAccount>();
        }

        public async Task Login()
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(Encoding.UTF8.GetBytes(LoginData.Password));
            LoginData.Password = BitConverter.ToString(output).Replace("-", "").ToLower();
            Account = await Service.Login(LoginData);
            LoginData.AuthenticationType = LoginData.LOGIN_BY_TICKET;
        }

        public async Task Logout()
        {
            LogoutData logout = new LogoutData() { Ticket = Account.Ticket };
            await Service.Logout(logout);
            Account = new AccountData();
        }
    }
}
