﻿//sbyte     -128 to 127                                             Signed 8 - bit integer System.SByte
//byte      0 to 255                                                Unsigned 8-bit integer  System.Byte
//short     -32,768 to 32,767	                                    Signed 16-bit integer   System.Int16
//ushort    0 to 65,535     	                                    Unsigned 16-bit integer System.UInt16
//int       -2,147,483,648 to 2,147,483,647	                        Signed 32-bit integer   System.Int32
//uint      0 to 4,294,967,295	                                    Unsigned 32-bit integer System.UInt32
//long      -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807	Signed 64-bit integer   System.Int64
//ulong     0 to 18,446,744,073,709,551,615	                        Unsigned 64-bit integer System.UInt64
//nint      Depends on platform (computed at runtime)               Signed 32 - bit or 64 - bit integer System.IntPtr
//nuint     Depends on platform (computed at runtime)               Unsigned 32 - bit or 64 - bit integer
//#define WORD_SIZE_8
//#define WORD_SIZE_16
#define WORD_SIZE_32
//#define WORD_SIZE_64
//#define TARGET_IS_LITTLE_ENDIAN
//#define TARGET_IS_BIG_ENDIAN
#define PAD_END_OF_STRUCT 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    public static class Binary
    {
        public enum e_endianess { Little = 1, Big = 2 }
        public static e_endianess Endianess;


        static Binary()
        {
            Endianess = e_endianess.Little;
            if (!BitConverter.IsLittleEndian)
                Endianess = e_endianess.Big;

        }
        /*
         * To support more data types that need conversions, add them in the c_conv class
         * a get/set handler func will be needed. Just follow the examples.
         */

        public class c_conv
        {

            static Dictionary<string, c_conv> type_lkup = null;
            public static c_conv _size_of_type(Type obj_type)
            {
                if (type_lkup == null)
                {
                    type_lkup = new Dictionary<string, c_conv>();
                    type_lkup.Add(typeof(UInt32).ToString(), new c_conv(typeof(UInt32), typeof(UInt32), 4, "uint32_t", c_conv.get_bytes_uint32, c_conv.set_bytes_uint32));
                    type_lkup.Add(typeof(UInt16).ToString(), new c_conv(typeof(UInt16), typeof(UInt16), 2, "uint16_t", c_conv.get_bytes_uint16, c_conv.set_bytes_uint16));
                    //type_lkup.Add(typeof(ushort).ToString(), new c_conv(typeof(ushort), typeof(UInt16), 2, "uint16_t"));
                    type_lkup.Add(typeof(byte).ToString(), new c_conv(typeof(byte), typeof(byte), 1, "uint8_t", c_conv.get_bytes_uint8, c_conv.set_bytes_uint8));

                    type_lkup.Add(typeof(Int32).ToString(), new c_conv(typeof(Int32), typeof(Int32), 4, "int32_t", c_conv.get_bytes_uint32, c_conv.set_bytes_int16));
                    type_lkup.Add(typeof(Int16).ToString(), new c_conv(typeof(Int16), typeof(Int16), 2, "int16_t", c_conv.get_bytes_int16, c_conv.set_bytes_int16));
                    //type_lkup.Add(typeof(short).ToString(), new c_conv(typeof(short), typeof(Int16), 2, "int16_t"));
                    type_lkup.Add(typeof(sbyte).ToString(), new c_conv(typeof(sbyte), typeof(sbyte), 1, "int8_t", c_conv.get_bytes_int8, c_conv.set_bytes_int8));
                }
                c_conv new_value;
                type_lkup.TryGetValue(obj_type.ToString(), out new_value);
                return new_value;
            }

            public c_conv(Type c_s_type, Type c_type, int size, string c_type_string
                , Func<object, byte[]> get_handler, Func<byte[], int, object> set_handler)
            {
                this.c_sharp_type = c_s_type;
                this.c_type = c_type;
                this.c_type_string = c_type_string;
                this.size = size;
                this.get_handler = get_handler;
                this.set_handler = set_handler;
            }

            public Type c_sharp_type;
            public string c_type_string;
            public Type c_type;
            public int size = 0;
            public Func<object, byte[]> get_handler = null;
            public Func<byte[], int, object> set_handler = null;

            public static byte[] get_bytes_int32(object value)
            {
                byte[] output = BitConverter.GetBytes((Int32)value);
                return _fix_endian(output);
            }
            public static byte[] get_bytes_uint32(object value)
            {
                byte[] output = BitConverter.GetBytes((UInt32)value);
                return _fix_endian(output);
            }
            public static byte[] get_bytes_int16(object value)
            {
                byte[] output = BitConverter.GetBytes((Int16)value);
                return _fix_endian(output);
            }
            public static byte[] get_bytes_uint16(object value)
            {
                byte[] output = BitConverter.GetBytes((UInt16)value);
                return _fix_endian(output);
            }
            public static byte[] get_bytes_int8(object value)
            {
                return BitConverter.GetBytes((sbyte)value);
            }
            public static byte[] get_bytes_uint8(object value)
            {
                return BitConverter.GetBytes((byte)value);
            }

            public static object set_bytes_int32(byte[] value, int pos)
            {
                return (UInt16)BitConverter.ToInt32(_fix_endian(value, pos, 4), 0);
            }
            public static object set_bytes_uint32(byte[] value, int pos)
            {
                return (UInt32)BitConverter.ToUInt32(_fix_endian(value, pos, 4), 0);
            }
            public static object set_bytes_int16(byte[] value, int pos)
            {
                return (Int16)BitConverter.ToInt16(_fix_endian(value, pos, 2), 0);
            }
            public static object set_bytes_uint16(byte[] value, int pos)
            {
                return (UInt16)BitConverter.ToUInt16(_fix_endian(value, pos, 2), 0);
            }
            public static object set_bytes_int8(byte[] value, int pos)
            {
                return (sbyte)value[pos];
            }
            public static object set_bytes_uint8(byte[] value, int pos)
            {
                return (byte)value[pos];
            }

            protected internal static byte[] _fix_endian(byte[] value)
            {
                if (Binary.Endianess != e_endianess.Little && BitConverter.IsLittleEndian)
                    Array.Reverse(value);
                return value;
            }
            protected internal static byte[] _fix_endian(byte[] value, int pos, int size)
            {

                byte[] output = new byte[4];
                Array.Copy(value, pos, output, 0, size);
                if (Binary.Endianess != e_endianess.Little && BitConverter.IsLittleEndian)
                    Array.Reverse(output);
                return output;
            }

        }

        private static System.Reflection.BindingFlags bind_flag =
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        public static void SetEndianes(e_endianess end)
        {
            Binary.Endianess = end;
        }

        private static bool _is_aligned(int address, int byte_width)
        {
            return (((address)) & (byte_width - 1)) != 0;
        }

        private static int _align(int address, int byte_width)
        {
            return ((address + (byte_width - 1)) & ~(byte_width - 1));
        }
                
        private static int _h_size_array(int begin_address, ref int widest_byte
            , FieldInfo array_object, object constructor, Dictionary<string, int> mem_address_list)
        {
            int byte_size = 0;
            int aligned_size = begin_address;

            if (array_object.FieldType.IsArray)
            {
                Type element_type = array_object.FieldType.GetElementType();

                Array arr_ref_source = array_object.GetValue(constructor) as Array;
                int arrayLength = arr_ref_source.GetLength(0);

                int idx = 0;
                foreach (var arr_ref_source_item in arr_ref_source)
                {
                    c_conv cnv = c_conv._size_of_type(arr_ref_source_item.GetType());
                    byte_size = cnv.size;
                    widest_byte = Math.Max(byte_size, widest_byte);
                    aligned_size = Binary._align(aligned_size, byte_size);
                    mem_address_list.Add(array_object.Name + "_" + idx++, aligned_size);
                    aligned_size += byte_size;
                }
#if PAD_END_OF_ARRAYS
                //make sure we end on a multiple of the widest byte address?? maybe?
                if (Binary._is_aligned(aligned_size, widest_byte))
                {
                    aligned_size = ((aligned_size + (widest_byte - 1)) & ~(widest_byte - 1));
                }
#endif
            }
            return aligned_size;
        }

        public static int _size_of<T>(T new_obj)
        {
            int byte_size = 0;
            int aligned_size = 0;
            int widest_byte = 0;

            FieldInfo xref_source = new_obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)xref_source.GetValue(new_obj);

            foreach (FieldInfo current_fld in new_obj.GetType().
                GetFields(bind_flag).OrderBy(field => field.MetadataToken))
            {
                if (current_fld.FieldType.IsArray)
                {
                    aligned_size = _h_size_array(aligned_size, ref widest_byte, current_fld, new_obj, mem_address_list);
                }
                else
                {
                    if (current_fld.Name != "mem_address" && current_fld.Name != "__size__")
                    {
                        c_conv cnv = c_conv._size_of_type(current_fld.FieldType);
                        byte_size = cnv.size;
                        widest_byte = Math.Max(byte_size, widest_byte);
                        aligned_size = Binary._align(aligned_size, byte_size);
                        //FieldInfo xref_source = new_obj.GetType().GetField("mem_address",bind_flag);
                        //List<int> mem_address_list =(List<int>) xref_source.GetValue(new_obj);
                        mem_address_list.Add(current_fld.Name, aligned_size);
                        aligned_size += byte_size;
                    }
                }
            }
#if PAD_END_OF_STRUCT
            //make sure we end on a multiple of the widest byte address?? maybe?
            if (Binary._is_aligned(aligned_size, widest_byte))
            {
                aligned_size = ((aligned_size + (widest_byte - 1)) & ~(widest_byte - 1));
            }
#endif

            return aligned_size;
        }

        public static string to_c_c_plusplus<T>(T new_obj)
        {
            //we only export structures that have a '__size__' field
            FieldInfo __struct_size_field__ = new_obj.GetType().GetField("__size__", bind_flag);
            if (__struct_size_field__ == null)
            {
                throw new Exception($"The specified type {new_obj.GetType()} does not bave a public static field named '__size__'");
            }
            int __struct_size__ = (int)__struct_size_field__.GetValue(null);

            byte[] stream = new byte[__struct_size__];
            string struct_info = "struct " + new_obj.GetType().Name + "\t //size=" + __struct_size__ + "\r\n{\r\n";

            FieldInfo xref_source = new_obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)xref_source.GetValue(new_obj);
            int addr_idx = 0;

            //fields HAVE to be returned in meta data order!!
            foreach (FieldInfo current_fld in new_obj.GetType().GetFields(bind_flag)
                .OrderBy(field => field.MetadataToken))
            {
                if (current_fld.FieldType.IsArray)
                {
                    struct_info += _h_write_array_to_c(current_fld, new_obj, current_fld.Name, mem_address_list);
                    //h_write_object(begin_address, ref widest_byte, current_fld.GetType(), new_obj, stream);
                }
                else
                {
                    if (current_fld.Name != "mem_address" && current_fld.Name != "__size__")
                    {
                        object reff = current_fld.GetValue(new_obj);
                        struct_info += _h_write_object_to_c(current_fld.FieldType, reff, current_fld.Name, mem_address_list);
                    }
                }
            }

            struct_info += "};";

            return struct_info;
        }

        private static string _h_write_array_to_c(FieldInfo current_fld, object new_obj
            , string field_name, Dictionary<string, int> mem_address_list)
        {
            string field_info = "";
            string array_info = "";
            if (current_fld.FieldType.IsArray)
            {

                Array arr_ref_source = current_fld.GetValue(new_obj) as Array;
                int arrayLength = arr_ref_source.GetLength(0);
                c_conv cnv = null;
                int idx = 0;
                foreach (var arr_ref_source_item in arr_ref_source)
                {
                    cnv = c_conv._size_of_type(arr_ref_source_item.GetType());
                    int address = mem_address_list[current_fld.Name + "_" + idx++];
                    int span = (address + cnv.size) - 1;
                    array_info += $"\t\t\t\t//memory position {address}-{span} (byte boundary)\r\n";

                }
                field_info = $"{cnv.c_type_string} {current_fld.Name}[{arrayLength}]\t\t//size {cnv.size}*{arrayLength}\r\n";
                field_info += array_info;
            }
            //field_info += "\r\n";
            return field_info;
        }

        private static int next_address = 0;
        private static string _h_write_object_to_c(Type current_fld, object new_obj, string field_name
            , Dictionary<string, int> mem_address_list)
        {

            // we only export fields that have a 'width' field
            Type source_type = current_fld;
            int address = mem_address_list[field_name];
            string field_info = "";
            c_conv cnv = c_conv._size_of_type(current_fld);
            int span = (address + cnv.size) - 1;

            next_address = address + cnv.size;

            field_info += $"{cnv.c_type_string} {field_name} \t\t//size {cnv.size}\tmemory position {address}-{span} (byte boundary)";

            field_info += "\r\n";



            return field_info;
        }

        public static T Read<T>(byte[] stream)
        {
            T new_obj = (T)Activator.CreateInstance(typeof(T));

            FieldInfo f_info = new_obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)f_info.GetValue(new_obj);

            foreach (FieldInfo current_fld in new_obj.GetType().
                GetFields(bind_flag).OrderBy(field => field.MetadataToken))
            {
                if (current_fld.FieldType.IsArray)
                {
                    _h_read_array(current_fld, new_obj, stream, mem_address_list);
                }
                else
                {
                    h_read_object(current_fld, new_obj, stream, mem_address_list);
                }
            }
            return new_obj;
        }
        public static byte[] Write<T>(T new_obj)
        {
            //we only export structures that have a '__size__' field
            FieldInfo __struct_size_field__ = new_obj.GetType().GetField("__size__", bind_flag);
            if (__struct_size_field__ == null)
            {
                throw new Exception($"The specified type {new_obj.GetType()} does not bave a public static field named '__size__'");
            }
            int __struct_size__ = (int)__struct_size_field__.GetValue(null);

            byte[] stream = new byte[__struct_size__];
            int addr_idx = 0;

            FieldInfo xref_source = new_obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)xref_source.GetValue(new_obj);


            //fields HAVE to be returned in meta data order!!
            foreach (FieldInfo current_fld in new_obj.GetType().GetFields(bind_flag)
                .OrderBy(field => field.MetadataToken))
            {
                if (current_fld.FieldType.IsArray)
                {
                    _h_write_array(current_fld, new_obj, stream, mem_address_list);
                    //h_write_object(begin_address, ref widest_byte, current_fld.GetType(), new_obj, stream);
                }
                else
                {
                    if (current_fld.Name != "mem_address" && current_fld.Name != "__size__")
                    {
                        object reff = current_fld.GetValue(new_obj);
                        _h_write_object(current_fld, reff, stream, mem_address_list);
                    }
                }
            }

            //uint16_t crc_test = (uint16_t)Kernel.Base.CRC.generate(stream, __struct_size__);
            //FieldInfo crc_field = new_obj.GetType().GetField("crc");
            //int crc_address = (int)_h_get_value(crc_field.GetValue(new_obj), "_address");
            //int crc_size = (int)_h_get_value(crc_field.GetValue(new_obj), "width");
            ////compute crc for array and store
            //Array.Copy(
            //    BitConverter.GetBytes(
            //        (uint16_t)Kernel.Base.CRC.generate(stream, __struct_size__))
            //    , 0, stream, crc_address, crc_size);

            return stream;
        }

        public static byte[] get_struct_crc_bytes<T>(T obj, byte[] stream)
        {
            FieldInfo __struct_size_field__ = obj.GetType().GetField("__size__", bind_flag);
            if (__struct_size_field__ == null)
            {
                throw new Exception($"The specified type {obj.GetType()} does not bave a public/private static field named '__size__'");
            }

            FieldInfo xref_source = obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)xref_source.GetValue(obj);

            FieldInfo crc_field = obj.GetType().GetField("crc");
            c_conv cnv = c_conv._size_of_type(crc_field.FieldType);
            int crc_address = mem_address_list["crc"];
            return cnv.get_handler(stream);
        }

        public static byte[] set_struct_crc_bytes<T>(T obj, byte[] stream)
        {
            FieldInfo __struct_size_field__ = obj.GetType().GetField("__size__", bind_flag);
            if (__struct_size_field__ == null)
            {
                throw new Exception($"The specified type {obj.GetType()} does not bave a public static/private field named '__size__'");
            }

            FieldInfo xref_source = obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)xref_source.GetValue(obj);

            UInt16 crc_test = (UInt16)Kernel.Base.CRC.generate(stream, stream.Length);
            FieldInfo crc_field = obj.GetType().GetField("crc");
            c_conv cnv = c_conv._size_of_type(crc_field.FieldType);
            int crc_address = mem_address_list["crc"];

            //compute crc for array and store
            Array.Copy(
                BitConverter.GetBytes(
                    (UInt16)crc_test)
                , 0, stream, crc_address, cnv.size);
            return stream;

        }

        public static byte[] clear_struct_crc_bytes<T>(T obj, byte[] stream)
        {
            FieldInfo __struct_size_field__ = obj.GetType().GetField("__size__", bind_flag);
            if (__struct_size_field__ == null)
            {
                throw new Exception($"The specified type {obj.GetType()} does not bave a public static field named '__size__'");
            }

            FieldInfo xref_source = obj.GetType().GetField("mem_address", bind_flag);
            Dictionary<string, int> mem_address_list = (Dictionary<string, int>)xref_source.GetValue(obj);

            UInt16 crc_test = 0;
            FieldInfo crc_field = obj.GetType().GetField("crc");
            c_conv cnv = c_conv._size_of_type(crc_field.FieldType);
            int crc_address = mem_address_list["crc"];

            Array.Copy(
                BitConverter.GetBytes(
                    (UInt16)crc_test)
                , 0, stream, crc_address, cnv.size);
            return stream;
        }

        private static void _h_write_array(FieldInfo current_fld, object new_obj
            , byte[] stream, Dictionary<string, int> mem_address_list)
        {

            if (current_fld.FieldType.IsArray)
            {
                Array arr_ref_source = current_fld.GetValue(new_obj) as Array;
                c_conv cnv = null;
                int idx = 0;
                foreach (var arr_ref_source_item in arr_ref_source)
                {
                    cnv = c_conv._size_of_type(arr_ref_source_item.GetType());
                    int address = mem_address_list[current_fld.Name + "_" + idx++];
                    byte[] val = cnv.get_handler(arr_ref_source_item);
                    Array.Copy(val, 0, stream, address, cnv.size);

                }
            }
        }

        private static void _h_write_object(FieldInfo current_fld, object new_obj, byte[] stream
            , Dictionary<string, int> mem_address_list)
        {
            if (current_fld.Name != "mem_address" && current_fld.Name != "__size__")
            {
                c_conv cnv = c_conv._size_of_type(current_fld.FieldType);
                int byte_size = cnv.size;
                int address = mem_address_list[current_fld.Name];
                byte[] val = cnv.get_handler(new_obj);
                Array.Copy(val, 0, stream, address, byte_size);

            }
        }

        private static void h_read_object(FieldInfo current_fld, object new_obj, byte[] stream,
            Dictionary<string, int> mem_address_list)
        {
            if (current_fld.Name != "mem_address" && current_fld.Name != "__size__")
            {
                c_conv cnv = c_conv._size_of_type(current_fld.FieldType);
                int byte_size = cnv.size;
                int address = mem_address_list[current_fld.Name];
                object val = cnv.set_handler(stream, address);
                current_fld.SetValue(new_obj, val);
            }
        }

        private static void _h_read_array(FieldInfo current_fld, object new_obj, byte[] stream, Dictionary<string, int> mem_address_list)
        {

            if (current_fld.FieldType.IsArray)
            {
                Array arr_ref_source = current_fld.GetValue(new_obj) as Array;

                c_conv cnv = null;
                int idx = 0;

                foreach (var arr_ref_source_item in arr_ref_source)
                {
                    cnv = c_conv._size_of_type(arr_ref_source_item.GetType());

                    int address = mem_address_list[current_fld.Name + "_" + idx];
                    object val = cnv.set_handler(stream, address);
                    arr_ref_source.SetValue(val, idx++);
                }
            }
        }

    }
}
