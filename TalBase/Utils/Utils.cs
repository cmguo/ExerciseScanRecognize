namespace TalBase.Utils
{
    public class NetWorkManager
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool IsNetWorkAvailable
        {
            get
            {
                int desc;
                return InternetGetConnectedState(out desc, 0);
            }
        }

        public static void CheckNetWorkAvailable()
        {
            if (!IsNetWorkAvailable)
                throw new System.Exception("当前无网络连接");
        }

        public static void CheckNetWorkAvailable(string msg)
        {
            if (!IsNetWorkAvailable)
                throw new System.Exception(msg);
        }
    }
   
}
