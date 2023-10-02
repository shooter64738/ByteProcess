using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;


namespace Kernel
{
    public class Comm
    {
        private static Thread sendThread = new Thread(_send);
        private static System.Timers.Timer aTimer;
        private static SerialPort _serialPort = null;
        private int bytesToRead = 0;
        private bool _alive = false;
        public bool Offline = true;
        public List<byte> IncomingData = new List<byte>();
        public List<object> Objects = new List<object>();
        private static List<byte> OutgoingData = new List<byte>();
        private static bool _sending = true;

        public Comm(string portName
            , int baudRate
            , Parity parity
            , int dataBits
            , StopBits stopBits
            , int readTime
            , int writeTime)
        {
            Initialize(portName, baudRate, parity, dataBits, stopBits, readTime, writeTime);
        }

        public bool Initialize(string portName
        , int baudRate
        , Parity parity
        , int dataBits
        , StopBits stopBits
        , int readTime
        , int writeTime)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = readTime;
            _serialPort.WriteTimeout = writeTime;
            _serialPort.WriteBufferSize = 64;
            _serialPort.DataReceived += _serialPort_DataReceived;

            //aTimer = new System.Timers.Timer(2000);
            //// Hook up the Elapsed event for the timer. 
            //aTimer.Elapsed += ATimer_Elapsed;
            //aTimer.AutoReset = true;
            //aTimer.Enabled = true;

            bool success = false;

            try
            {
                _serialPort.Open();
                //start a thread to send the data
                //once it completes the thread will terminate. 
                sendThread.Start();
                success = true;
            }
            catch (Exception ex)
            {
                //add a message to the kernel errors
                success = false;
            }
            return success;
        }

        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_alive)
            {
                Offline = true;
            }
            _alive = false;
        }

        public List<string> GetPorts()
        {
            return SerialPort.GetPortNames().ToList();
        }

        public void Close()
        {
            _shutdown();
        }

        public void Send(List<byte> data)
        {
            OutgoingData.AddRange(data);
        }

        private Func<List<byte>, string, object> Serial_Read;
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //try
            {
                if (_serialPort.IsOpen)
                {
                    bytesToRead = _serialPort.BytesToRead;
                    byte[] newData = new byte[bytesToRead];
                    _serialPort.Read(newData, 0, bytesToRead);
                    _alive = true;
                    Offline = false;
                    IncomingData.AddRange(newData);
                    newData = null;
                    bytesToRead = 0;
                    //need at least 4 bytes of data to be useful (rec type,keys(1,2,3))
                    while (IncomingData.Count > 4)
                    {
                        if (Serial_Read == null)
                        {
                            Serial_Read = _assign_reader(IncomingData);
                        }

                        if (Serial_Read != null) //if not null we had enough data for the start
                        {
                            object result = Serial_Read(IncomingData, "crc");
                            if (result != null)
                            {
                                Objects.Add(result);
                                Serial_Read = null;
                            }
                            else//we dont have all the data
                            {
                                Serial_Read = null;
                                break;
                            }
                        }
                        else
                            break;
                    }
                }
            }
            //catch (TimeoutException)
            //{
            //    //add a message to the kernel errors, but continue
            //}
            //catch (Exception ex)
            //{
            //    //add a message to the kernel errors, and stop
            //    _shutdown();
            //}
        }

        private Func<List<byte>, string, object> _assign_reader(List<byte> data)
        {
            if (data.Count < 4)
                return null;
            /*must have atleast 4 bytes
             * 1. type
             * 2. always 07
             * 3. always 27
             * 4. always 99
             */

            for (int read_byte = 0; read_byte < data.Count - 4; read_byte++)
            {
                bool header_pass = false;
                if (Record_Types._Key.key1 == data[read_byte + 1])
                {
                    header_pass = Record_Types._Key.key2 == data[read_byte + 2]
                        && Record_Types._Key.key3 == data[read_byte + 3];
                }
                if (!header_pass)
                    continue;

                if (read_byte > 0)
                    data.RemoveRange(0, read_byte);

                read_byte = 0;

                Record_Types.Record_Type rec_type = (Record_Types.Record_Type)data[read_byte];

                switch (rec_type)
                {
                    case Record_Types.Record_Type.ToolHead:
                        if (data.Count < Record_Types.ToolHead.__size__)
                            return null;
                        return Record_Types.ToolHead.SerialProcessor;
                    //break;
                    case Record_Types.Record_Type.Fan:
                        break;
                    default:
                        break;
                }
            }
            return null;

        }

        private void _shutdown()
        {
            _serialPort.DataReceived -= _serialPort_DataReceived;
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.Close();
        }

        private static void _send()
        {
            while (_sending)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        if (OutgoingData.Count > 0)
                        {
                            byte[] bytes = OutgoingData.ToArray();
                            int size = bytes.Length;

                            OutgoingData.RemoveRange(0, size);
                            _serialPort.Write(bytes, 0, size);
                            bytes = null;
                            size = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _sending = false;
                    //add a message to the kernel errors
                }
            }
        }
    }
}
