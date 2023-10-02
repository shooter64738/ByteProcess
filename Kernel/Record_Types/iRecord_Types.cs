using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Record_Types
{
    interface iRecord_Types
    {
        List<byte> ToArray(string crc_field = "crc");
        List<byte> ToArray(bool create_crc, string crc_field = "crc");
    }
}
