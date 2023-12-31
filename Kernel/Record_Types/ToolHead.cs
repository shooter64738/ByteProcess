﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Record_Types
{
    public static class _Key
    {
        public const byte key1 = 7;
        public const byte key2 = 27;
        public const byte key3 = 99;
    }



    public class ToolHead : iRecord_Types
    {
        protected internal Dictionary<string, int> mem_address = new Dictionary<string, int>();
        public static int __size__ = 0;
        public static int __widest__ = 0;

        public Record_Type __rec_type__ = Record_Type.ToolHead;
        public byte key1 = _Key.key1;
        public byte key2 = _Key.key2;
        public byte key3 = _Key.key3;

        public UInt16 temperature = 0;
        public byte pressure = 0;
        public UInt16 output = 0;
        public byte extruder_fan = 0;
        public byte part_fan = 0;
        public byte air_assist = 0;
        public byte coolant = 0;
        public UInt16 arc_gap = 0;
        public UInt16 crc = 0;

        public ToolHead()
        {
            __rec_type__ = Record_Type.ToolHead;
            Binary._size_of(this);

        }
        public static ToolHead SerialProcessor(List<byte> data, string crc_field)
        {
            if (data.Count >= ToolHead.__size__)
            {
                //we are copying to copy off starting at zero, the length we need
                //because the serial data may be much bigger than out struct size
                byte[] frag_array = new byte[__size__];
                Array.Copy(data.ToArray(), 0, frag_array, 0, __size__);
                data.RemoveRange(0, __size__);
                ToolHead new_he = (ToolHead)Binary.Read(frag_array, new ToolHead());
                //hold the crc value we were sent
                UInt16 serial_crc = (UInt16)Binary.get_struct_crc_uint(new_he, frag_array, crc_field);
                //before we can compute a new crc on the data, we must first zero out
                byte[] ar_data = Binary.clear_struct_crc_bytes(new_he, frag_array, crc_field);
                UInt16 gen_crc = Kernel.Base.CRC.generate(ar_data, __size__);
                bool crc_fail = serial_crc != gen_crc;
                return !crc_fail ? new_he : null;
            }
            return null;
        }
        public List<byte> ToArray(string crc_field)
        {
            if (__size__ == 0)
                throw new Exception($"No size set in constructor of {this} for ToArray() method");
            return Binary.Write<ToolHead>(this).ToList();
        }
        public List<byte> ToArray(bool create_crc, string crc_field)
        {
            List<byte> stream = this.ToArray(crc_field);
            stream = Binary.set_struct_crc_bytes<ToolHead>(this, stream.ToArray(), crc_field).ToList();
            return stream;
        }
        public static ToolHead ToStruct(List<byte> bytes)
        {
            return (ToolHead)Binary.Read(bytes.ToArray(), new ToolHead());
        }
    }


}
