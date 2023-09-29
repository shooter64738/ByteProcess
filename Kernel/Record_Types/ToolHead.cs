
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

    public class _Header
    {
        public byte __rec_type__;
        public byte key1 = _Key.key1;
        public byte key2 = _Key.key2;
        public byte key3 = _Key.key3;
    }

    public class ToolHead : _Header, iRecord_Types
    {
        /*
         * why are these not properties? Because..
         * 1. properties were traditionaly slower to access, this is not the case with
         * new C# compilers, but properties are not needed in this design. We do not 
         * need to change values often. These values are set once and left alone.
         * 2. C++ doesnt support the same property sytntax as C#, and cant be copied
         * and pasted from one place to another. These values can simply be copied 
         * from this code and pasted directly into C++. This saves time, and ensures
         * that the field values from C# and C++ match up
         * 3. ONLY the types syntactically similar to C++ are processed in the ToArray
         * and ToStruct methods. bools, ints, longs, custom objects can all be placed
         * in the classes and used C# side, but they will NOT get handled in the binary
         * processing methods. They will be ignored.
         */
        protected internal Dictionary<string, int> mem_address = new Dictionary<string, int>();

        public static int __size__ = 0;

        public Int16 z0 = 32;
        public UInt32[] a = new UInt32[3];
        public UInt16[] b = new UInt16[2];
        public byte[] c = new byte[5];
        public UInt16 crc = 0;

        public static ToolHead SerialProcessor(List<byte> data)
        {

            if (data.Count >= ToolHead.__size__)
            {
                //we are copying to copy off starting at zero, the length we need
                //because the serial data may be much bigger than out struct size
                byte[] frag_array = new byte[__size__];
                Array.Copy(data.ToArray(), 0, frag_array, 0, __size__);

                ToolHead new_he = Binary.Read<ToolHead>(frag_array);
                //hold the crc value we were sent
                UInt16 serial_crc = new_he.crc;
                //before we can compute a new crc on the data, we must first zero out
                byte[] ar_data = Binary.clear_struct_crc_bytes(new_he, frag_array);
                UInt16 gen_crc = Kernel.Base.CRC.generate(ar_data, __size__);
                bool crc_fail = new_he.crc != gen_crc;
                return !crc_fail ? new_he : null;
            }
            return null;
        }

        public ToolHead()
        {
            __rec_type__ = (int)Record_Type.ToolHead;
            //only need to set size once, but it MUST be set to call reader/writer functions
            //if (__size__ == 0)
            //__size__ = Binary._size_of<ToolHead>(this);
            __size__ = Binary._size_of(this);
        }

        public List<byte> ToArray()
        {
            if (__size__ == 0)
                throw new Exception($"No size set in constructor of {this} for ToArray() method");
            return Binary.Write<ToolHead>(this).ToList();
        }
        public List<byte> ToArray(bool create_crc)
        {
            List<byte> stream = this.ToArray();
            int test = 0;
            stream = Binary.set_struct_crc_bytes<ToolHead>(this, stream.ToArray()).ToList();
            return stream;
        }

        public static ToolHead ToStruct(List<byte> bytes)
        {
            return Binary.Read<ToolHead>(bytes.ToArray());
        }
    }
}
