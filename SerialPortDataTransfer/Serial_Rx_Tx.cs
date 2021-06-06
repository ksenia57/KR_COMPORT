// Serial COM Port receive message event handler
// 12/16/2017, Dale Gambill
// When a line of text arrives from the COM port terminated by a \n character, this module will pass the message to
// the function specified by the application.   The application can also send a line of text.
//
// IMPORTANT: The dot net function below, comPort.ReadLine(), will not throw an error if there is no data, but might throw 'System.TimeoutException', if the data
// is not lines of text terminated by /n.  This would be because ReadLine() cannot find a line terminator in the wrong type of data.
// This code is intended for use with lines of text only.  It is not intended for use with any other type of data.
//
using System;
using System.IO.Ports;
using System.Diagnostics;
using System.Text;

namespace SERIAL_RX_TX
{
    public class SerialComPort
    {
        private SerialPort comPort;

        // constructor
        public SerialComPort()
        {
            comPort = new SerialPort();
        }

        ~SerialComPort()
        {
            Close();
        }


        // User must register function to call when a line of text terminated by \n has been received
        public delegate void ReceiveCallback(string receivedMessage);
        public event ReceiveCallback onMessageReceived = null;
        public void RegisterReceiveCallback(ReceiveCallback FunctionToCall)
        {
            onMessageReceived += FunctionToCall;
        }
        public void DeRegisterReceiveCallback(ReceiveCallback FunctionToCall)
        {
            onMessageReceived -= FunctionToCall;
        }

        public void SendLine(string aString)
        {
            try
            {
                if (comPort.IsOpen)
                {
                    comPort.Write(aString);
                }
            }
            catch (Exception exp)
            {
                Debug.Print(exp.Message);
            }
        }

        public string Open(string portName, string baudRate, string dataBits, string parity, string stopBits)
        {
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();

            try
            {
                comPort.WriteBufferSize = 4096;
                comPort.ReadBufferSize = 4096;
                comPort.WriteTimeout = 500;
                comPort.ReadTimeout = 500;
                comPort.DtrEnable = true;
                comPort.Handshake = Handshake.None;
                comPort.PortName = portName.TrimEnd();
                comPort.BaudRate = Convert.ToInt32(baudRate);
                comPort.DataBits = Convert.ToInt32(dataBits);

                switch (parity)
                {
                    case "None":
                        comPort.Parity = Parity.None;
                        break;
                    case "Even":
                        comPort.Parity = Parity.Even;
                        break;
                    case "Odd":
                        comPort.Parity = Parity.Odd;
                        break;
                }
                switch (stopBits)
                {
                    case "One":
                        comPort.StopBits = StopBits.One;
                        break;
                    case "Two":
                        comPort.StopBits = StopBits.Two;
                        break;
                }
                comPort.Open();
                comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
            catch (Exception error)
            {
                return error.Message + "\r\n";
            }
            if (comPort.IsOpen)
            {
                return string.Format(" [" + dtn + "] " + "{0} Открыт \r\n", comPort.PortName);
            }
            else
            {
                return string.Format(" [" + dtn + "] " + "{0} Произошла ошибка \r\n", comPort.PortName);
            }
        }

        public string Close()
        {
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();

            try
            {
                comPort.Close();
            }
            catch (Exception error)
            {
                return error.Message + "\r\n";
            }
            return string.Format(" [" + dtn + "] " + "{0} Закрыт\r\n", comPort.PortName);
        }

        public bool IsOpen()
        {
            return comPort.IsOpen;
        }

        
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (!comPort.IsOpen)
            {
                return;
            }
            string indata = string.Empty;

            try
            {
                indata = comPort.ReadLine();

                StringBuilder sb = new System.Text.StringBuilder();
                string[] binaryArr1 = new string[sb.Length];
                string[] binaryArr2 = new string[sb.Length];
                string[] residueArr = new string[binaryArr1.Length];
                foreach (byte b in System.Text.Encoding.UTF8.GetBytes(indata))
                    for (int k = 0; k < sb.Length; k++)
                    {
                        sb.Append(Convert.ToString(b, 2).PadLeft(11, '0').PadRight(15, '0'))/*.Append(' ')*/;
                        binaryArr1[k] = sb.ToString();
                        sb.Append(Convert.ToString(b, 2).PadLeft(11, '0'))/*.Append(' ')*/;
                        binaryArr2[k] = sb.ToString();
                        //string binaryStr = sb.ToString();
                    }
                for (int i = 0; i < binaryArr1.Length; i++)
                {
                    int binaryInt = Convert.ToInt32(binaryArr1[i], 2);
                    int residue = binaryInt % 11/*1011*/;
                    residueArr[i] = Convert.ToString(residue, 2);
                    binaryArr1[i] = binaryArr2[i] + residueArr[i] + ' ';
                }
                if (onMessageReceived != null)
                {
                    onMessageReceived(indata);

                }
            }

            catch (Exception error)
            {
                Debug.Print(error.Message);
            }

        }
    }
}
