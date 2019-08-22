using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Boot
{
    public interface IProduct
    {
        string ProductCode { get; }
        string LogPath { get; }
    }
}
