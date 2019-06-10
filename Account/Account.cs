using Account.Service;
using System.Windows;

namespace Account
{
    public class Module
    {
        public static void Init(Application app)
        {
            TalBase.Service.Services.MessageHandler = new AccountHandler();
        }
    }
}
