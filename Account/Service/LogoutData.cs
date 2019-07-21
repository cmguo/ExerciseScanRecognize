namespace Account.Service
{
    public class LogoutData
    {

        public string IpAddress { get; set; }
        public string NetType { get; set; }
        public string DeviceNumber { get; set; }
        public string Ticket { get; set; }

        public LogoutData()
        {
            NetType = "wifi";
            IpAddress = "127.0.0.1";
            DeviceNumber = "0";
        }

    }
}
