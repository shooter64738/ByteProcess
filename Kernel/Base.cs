#define PAD_END_OF_STRUCT
//#define PAD_END_OF_ARRAYS
//#define INIT_NULL_ARRAY_OBJECTS_ON_WRITE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    public class Base
    {
        const string KERNEL_BASE_FIRMWARE_VERSION = "0.0.1a";
		public string get_version()
		{
			return KERNEL_BASE_FIRMWARE_VERSION;
		}
		public class CRC
		{
            public static UInt16 generate(List<byte> data_p, int length)
            {
                byte x;
                byte _8 = 8;
                byte _12 = 12;
                byte _5 = 5;
                UInt16 crc = 0xFFFF;
                int idx = 0;
                while (idx < length)
                {
                    x = (byte)(crc >> 8 ^ data_p[idx++]);
                    x ^= (byte)(x >> 4);
                    UInt16 c_8 = (UInt16)(crc << _8);
                    UInt16 x_12 = ((UInt16)(x << _12));
                    UInt16 x_5 = ((UInt16)(x << _5));
                    UInt16 x_0 = ((UInt16)x);
                    crc = (UInt16)(c_8 ^ x_12 ^ x_5 ^ x_0);
                }
                return (UInt16)crc;

            }
            public static UInt16 generate(byte[] data_p, int length)
            {
                byte x;
                byte _8 = 8;
                byte _12 = 12;
                byte _5 = 5;
                UInt16 crc = 0xFFFF;
                int idx = 0;
                while (idx < length)
                {
                    x = (byte)(crc >> 8 ^ data_p[idx++]);
                    x ^= (byte)(x >> 4);
                    UInt16 c_8 = (UInt16)(crc << _8);
                    UInt16 x_12 = ((UInt16)(x << _12));
                    UInt16 x_5 = ((UInt16)(x << _5));
                    UInt16 x_0 = ((UInt16)x);
                    crc = (UInt16)(c_8 ^ x_12 ^ x_5 ^ x_0);
                }
                return (UInt16)crc;

            }

            public static bool check(List<byte> byte_list, ref int read_crc, ref int data_crc, ref int pos)
            {
                int crc_pos = pos;
                read_crc = CRC.Get_Uint16_t(byte_list);

                byte_list[crc_pos] = 0;
                byte_list[crc_pos + 1] = 0;
                data_crc = CRC.generate(byte_list.ToArray(), byte_list.Count);
                return read_crc == data_crc;
            }

            public static UInt16 Get_Uint16_t(List<byte> _raw_bytes)
            {
                byte[] d = _raw_bytes.GetRange(_raw_bytes.Count-2, 2).ToArray();
                if (d != null)
                    return BitConverter.ToUInt16(d, 0);
                return 0;

            }

        };
		
	}
}
