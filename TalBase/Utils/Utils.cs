
using Microsoft.VisualBasic.Devices;
using Panuon.UI;

namespace TalBase.Utils
{
    public class NetWorkManager
    {
        public static bool CheckNetWorkAvailable()
        {
            Network network = new Network();
            if (network.IsAvailable)
            {
                return true;
            }
            PUMessageBox.ShowDialog("当前电脑无网络连接，请检查后再开始扫描", "", Buttons.OK, false);
            return false;
            
        }
    }
   
}
