using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * Liscense info.. This is free shit. Do with it what you wish. I make no warranty
 * for this code. Its useful to me, I hope you find it useful too.
 */

namespace kernel_test
{
    class Program
    {
        static void Main(string[] args)
        {
            intentional compiler error to make you read this. you can delete this line now
            /*I made this to simplify sending/receiving serial data from MCU boards such as STM
             * and Atmel. This is by no means an all encompassing Serializing project. It hadles
             * basic C/C++ data types. Other types could be added, I just do not have a need
             * to do so at this time. Feel free to add your own types, all I ask is you push it back to me on 
             * GitHub.
             * 
             * You can specify the target endianess in the binary class by using 
             * Binary.SetEndianes(). At start up the endianess is set to the same type as the system
             * this is running on. If you are running on a machine thats little endian and you need
             * to output to a system using big endian, set teh endianess before to use the ToArray
             * or ToStruct methods.
             * 
             * Program flow:
             * ToolHead is an example class. Each class you create should follow ToolHeads example
             * Rule #1. Each class MUST contain a dictionary field called 'mem_address'. C# reflection will
             * look for this field.
             * Rule #2. Each class MUST contain a field called __struct_size__. Again reflection will look for it.
             * (I used an interface to enforce some of the requirements, but not every requirement can
             * be enforced with an interface)
             * 
             * On the mCU I have structs that contain data. I send those structs via serial (usually) to
             * a pc. On mCU side a type C++ struct might look like this
             * struct s_toolhead
             * {
             * int16_t z0 = 0;
             * int32_t a[3]{0};
             * int16_t b[2]{0};
             * uint8_t c[5]{0};
             * uint16_t crc = 0;
             * };
             *              
             * s_toolhead th{0};
             * th.a[0] = 75; th.a[3] = 4;  th.c[2] = 99;
             * 
             * To send that to the pc side, I call something like this
             * serial.write((char*)th,sizeof(th));
             * There is no special processing or handling. No byte packing, or aligning to single bytes
             * The data now is sent over the wire to the pc, and I get it in the serial buffer
             * 
             * PC Side
             * Step 1. When the class is instantiated the Kernel.Binary._size_of method gets called in the
             * classes constructor
             * 
             * Step 2. The '_size_of' method will use reflection to process the class instance, which determines
             * the '__struct_size__' value, and stores field width, and memory addresses in the mem_address lookup
             * dictionary.
             * If '_size_of' is never called for a class, none of this will work. Memory address table will remain
             * empty, there will be no assigned sizes, getters, setters, nothing..
             * 
             * Step 3. After the class is instantiated, you can call a number of methods for the class. I have provided
             * some examples below.
             * 
             * The 'Binary' class has a number of methods in it that you could use on other class types, such as
             * '_size_of'. I have done my best to ensure the bye padding and byte boundaries are enforced. If you
             * find a mistake or an error let me know please.
             */

            /*Create an instance of ToolHead
             * This will call _size_of, set address information, and field names
             */
            Kernel.Record_Types.ToolHead th = new Kernel.Record_Types.ToolHead();

            /*If you want a C/C++ version of your class as a struct that is copy and paste ready, call this
             * method and copy the returned string result.
             */
            string str = Kernel.Binary.to_c_c_plusplus(th);

            //set some values in the class
            th.a[0] = 75; th.a[2] = 4; th.c[2] = 99;

            /*Convert this class to a stream of bytes that can be sent over serial
             * This method is overlaoded.
             * ToArray() will return a stream of bytes with NO CRC
             * ToArray(true) will return a stream of bytes WITH a computed 16 bit crc value
             */
            List<byte> byte_data = th.ToArray(true);

            /*Simulated serial data
             * If we take our List<byte> data from above and send that bye array into the
             * static mathod for our class, we will get back a converted stream of bytes
             * that represents that class.
             * 
             * How do we know which class to use? Well for me I check the first 4 bytes of
             * the byte array to determine which class should handle the data. See 
             * _assign_reader(List<byte> data) in Comm.cs for details. If that wont work
             * for your situation, feel free to roll your own.
             * 
             * NOTE: I HAVE NOT FINSIHED MY COMM CLASS. I have written jsut enough code in
             * it to do serial testing.
             */
            Kernel.Record_Types.ToolHead th1 = Kernel.Record_Types.ToolHead.SerialProcessor(byte_data);

            /*This serves no real purpose to already have a class created, convert to
             * binary, and then convert back. Im just showing an example of how you would 
             * accomplish that.
             * We call ToStruct, pass in our byte array and get a new object back.
             * The serial processor calls this internally. 
            */
            Kernel.Record_Types.ToolHead th2 = Kernel.Record_Types.ToolHead.ToStruct(byte_data);

            /*
             * Because I dont want to create an entire new solution just to demo functionality
             * you are going to get some of my serial processing code too. Use it, I dont care
             * but its written specifically to rx/tx data to a motion control system that gets
             * a refresh from the mCU at 50hz. 
             */
            Kernel.Comm n_com = new Kernel.Comm("COM4", 230400, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One, 500, 500);

            int last_count = 0;

            //Com will fill the objectes tabel with record data.. process those and spit them out on the screen            
            while (true)
            {
                if (n_com.Objects.Count != last_count)
                {
                    //Console.CursorLeft = 0;
                    //Console.CursorTop = 0;
                    //Console.Write(last_count.ToString("0000 "));
                    //Console.CursorLeft = 0;
                    //Console.CursorTop = 1;
                    //Kernel.Record_Types.ToolHead th = (Kernel.Record_Types.ToolHead)n_com.Objects[last_count];
                    //Console.Write($"  Cooling Fan {th.Cooling_Fan}  Nozzle Fan {th.Nozzle_Fan}  ");
                    //Console.CursorLeft = 0;
                    //Console.CursorTop = 2;
                    //Console.Write($" Current Temp {th.Current_Temp} Target Temp {th.Target_Temp}  ");
                    //show spindle torque if tool head is spindle.
                    //laser output is related to the S word, it has not 'load' factor.

                    last_count++;
                }
            }

        }
    }
}
