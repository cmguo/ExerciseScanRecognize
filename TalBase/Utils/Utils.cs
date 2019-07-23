
using Microsoft.VisualBasic.Devices;
using Panuon.UI;
using TalBase.View;

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
            PopupDialog.Show("发现错误", "当前电脑无网络连接，请检查后再开始扫描。", 0, "确定");
            return false;
            
        }
    }
   
}
