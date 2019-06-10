using Account.Service;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
            LoginData = new LoginData() { UserName = "guochunmao", Password = "guochunmao123" };
            Account = new Service.AccountData();
            Service = TalBase.Service.Services.Get<IAccount>();
        }

        public async Task Login()
        {
            Account = await Service.Login(LoginData);
        }
    }
}
