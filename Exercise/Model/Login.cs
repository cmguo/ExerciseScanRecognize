﻿
using Newtonsoft.Json;

namespace Exercise.Model
{
    public class Login
    {
        public static readonly int LOGIN_BY_PASSWORD = 1;
        public static readonly int LOGIN_BY_TICKET = 2;

        [JsonProperty(PropertyName = "loginName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        public int authenticationType = LOGIN_BY_TICKET;

        public int nextAutoLogin = 1;

        public string clientType = "exercise";

        public string netType = "wifi";

        public string ipAddress = "127.0.0.1";

        public string deviceNumber = "0";

        public string osVersionNumber = "10";

        public string productVersionNumber = "1.0";
    }
}
