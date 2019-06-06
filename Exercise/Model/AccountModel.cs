using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Model
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

        public Login LoginData { get; private set; }

        public Account Account { get; private set; }

        private IAccount Service;

        public AccountModel()
        {
            LoginData = new Login() { UserName = "guochunmao", Password = "guochunmao123" };
            Account = new Account();
            Service = Services.Get<IAccount>();
        }

        public async Task Login()
        {
            Account account = await Service.Login(LoginData);
            Account = account;
        }
    }
}
