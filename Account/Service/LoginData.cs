
using Newtonsoft.Json;

namespace Account.Service
{
    public class LoginData
    {
        public static readonly int LOGIN_BY_PASSWORD = 1;
        public static readonly int LOGIN_BY_TICKET = 2;

        public static readonly int LOGIN_OUT_FIRST = 5000001;
        public static readonly int LOGIN_OUT_OF_TIME = 5000001;
        public static readonly int LOGIN_OUT_LAST = 5000010;

        public long AuthenticationType { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public long NextAutoLogin { get; set; }
        public string ClientType { get; set; }
        public string ProductVersionNumber { get; set; }
        public string OsVersionNumber { get; set; }
        public string DeviceNumber { get; set; }
        public string IpAddress { get; set; }
        public string NetType { get; set; }

        public LoginData()
        {
            NextAutoLogin = 1;
            ClientType = "ios";
            NetType = "wifi";
            IpAddress = "127.0.0.1";
            DeviceNumber = "0";
            OsVersionNumber = "10";
            ProductVersionNumber = "youchat.2.1.31.20190428.beta";
        }

    }
}
