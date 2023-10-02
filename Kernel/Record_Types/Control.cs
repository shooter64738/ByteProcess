using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Record_Types
{
    public class Control : iRecord_Types
    {
        protected internal Dictionary<string, int> mem_address = new Dictionary<string, int>();
        public static int __size__ = 0;
        public static int __widest__ = 0;

        public Record_Type __rec_type__ = Record_Type.Control;
        public byte key1 = _Key.key1;
        public byte key2 = _Key.key2;
        public byte key3 = _Key.key3;
        public UInt16 control = 0;
        public UInt16 crc = 0;

        public Control()
        {
            __rec_type__ = Record_Type.ToolHead;
            Binary._size_of(this);

        }
        public static Control SerialProcessor(List<byte> data, string crc_field)
        {
            if (data.Count >= Control.__size__)
            {
                //we are copying to copy off starting at zero, the length we need
                //because the serial data may be much bigger than out struct size
                byte[] frag_array = new byte[__size__];
                Array.Copy(data.ToArray(), 0, frag_array, 0, __size__);
                data.RemoveRange(0, __size__);
                Control new_he = (Control)Binary.Read(frag_array, new Control());
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
            return Binary.Write<Control>(this).ToList();
        }
        public List<byte> ToArray(bool create_crc, string crc_field)
        {
            List<byte> stream = this.ToArray(crc_field);
            stream = Binary.set_struct_crc_bytes<Control>(this, stream.ToArray(), crc_field).ToList();
            return stream;
        }
        public static Control ToStruct(List<byte> bytes)
        {
            return (Control)Binary.Read(bytes.ToArray(), new Control());
        }
    }
}
