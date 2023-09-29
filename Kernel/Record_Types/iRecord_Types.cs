using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Record_Types
{
    interface iRecord_Types
    {
        List<byte> ToArray();
        List<byte> ToArray(bool create_crc);
    }
}
