using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using SERIAL_RX_TX;



namespace COMPDT
{
    public partial class Form : System.Windows.Forms.Form
    {
        private SerialComPort serialcomport;
        private Timer receivedDataTimer;
        private Timer replayFileTimer;
        private string receivedData;
        private bool dataNastr = false;
        private bool dataReady = false;
        private StreamReader file;
        
        private void Form_Load(object sender, EventArgs e)
        { }   
        public Form()
        {
            InitializeComponent();
            Browse.Enabled = false;
            SendFile.Enabled = false;
            file = null;
            serialcomport = new SerialComPort();
            serialcomport.RegisterReceiveCallback(ReceiveDataHandler);
            receivedDataTimer = new Timer();
            receivedDataTimer.Interval = 25;   // 25 ms
            receivedDataTimer.Tick += new EventHandler(ReceivedDataTimerTick);
            receivedDataTimer.Start();
            replayFileTimer = new Timer();
            replayFileTimer.Interval = 1000;   // 1000 ms
            replayFileTimer.Tick += new EventHandler(ReplayFileTimerTick);
            replayFileTimer.Start();
            
        }
        private void ReceiveDataHandler(string data)
        {
            if (dataReady)
            {
                Debug.Print("Полученные данные были отброшены, потому что буфер строки не очищен");
            }
            else
            {
                dataReady = true;
                receivedData = data;
            }
        }
        private void ReceivedDataTimerTick(object sender, EventArgs e)
        {
            string path = textBoxPath.Text + "file.txt";
            if (dataReady)
                {
                Messages.Clear();
                dataReady = false;
                UpdateDataWindow("Данные приняты...");
                StellsBox.Clear();
                UpdateWindow(receivedData);
                using (FileStream file = new FileStream(path, FileMode.Append))
                using (StreamWriter sw = new StreamWriter(file))
                    sw.WriteLine(StellsBox.Text);
            }
        }
        private void ReplayFileTimerTick(object sender, EventArgs e)
        {
            if (file != null)
            {
                try
                {
                    string message = file.ReadLine();
                    if (!file.EndOfStream)
                    {
                        serialcomport.SendLine(message + "\n");
                    }
                    else
                    {
                        file.BaseStream.Seek(0, 0);  // start over reading the file
                    }
                }
                catch (Exception error)
                {
                    Debug.Print(error.Message);
                }
            }
        }
        private void UpdateDataWindow(string message)
        {
            Messages.Text += message;
            Messages.SelectionStart = Messages.TextLength;
            Messages.ScrollToCaret(); 
        }
        private void UpdateWindow(string message)
        {
            StellsBox.Text += message;
            StellsBox.SelectionStart = StellsBox.TextLength;
            StellsBox.ScrollToCaret(); 
        }

        public bool IsTextFile(string FilePath)
        {
            using (StreamReader reader = new StreamReader(FilePath))
            {
                int Character;
                while ((Character = reader.Read()) != -1)
                {
                    if ((Character > 0 && Character < 8) || (Character > 13 && Character < 26))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void SendFileButton(object sender, EventArgs e)
        {
            Messages.Clear();
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();
            
            if (!serialcomport.IsOpen())
            {
                UpdateDataWindow(" [" + dtn + "] " + "Откройте свой порт\r\n");
                return;
            }
            if (SendFile.Text == "Передать")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                DialogResult result = openFileDialog.ShowDialog();
                string rassh = openFileDialog.FileName;
                if ((result == DialogResult.OK) && (IsTextFile(rassh)))
                {
                    file = new System.IO.StreamReader(openFileDialog.FileName);
                    SendFile.Text = "Передать ";
                    UpdateDataWindow("Передача через COM порт: " + rassh + "\r\n");
                    
                }
                else UpdateDataWindow("Выберете текстовый файл\r\n");
            }
            else
            {
                if (file != null)
                {

                    file.Close();
                    file = null;
                    SendFile.Text = "Передать";
                    this.timer1.Dispose();
                }
            }
        }
        private void ConnectionButton(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();

            if (comboBoxPort.Text == "" || comboBoxBaudRate.Text == "")
            { Messages.Text = " [" + dtn + "] " + "Пожалуйста заполните настройки порта\n"; }
            else
            {
            // Handles the Open/Close button, which toggles its label, depending on previous state.
                string status;
                if (Connection.Text == "Подключиться")
                    {
                    status = serialcomport.Open(comboBoxPort.Text, comboBoxBaudRate.Text, comboBox1.Text, comboBox2.Text, comboBox3.Text);
                    if (status.Contains("Открыт"))
                    {
                        Connection.Text = "Отключиться";
                        Browse.Enabled = true;
                        SendFile.Enabled = true;
                        groupBox1.Enabled = false;
                        comboBoxPort.Enabled = false;

                    }
                }
                else
                {
                    status = serialcomport.Close();
                    Connection.Text = "Подключиться";
                    groupBox1.Enabled = true;
                    comboBoxPort.Enabled = true;
                    SendFile.Enabled = false;
                }
                UpdateDataWindow(status);
            }
        }
        private void BrowseButton(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Выберите путь, где хотите сохранить свои файлы:" })
            if (fbd.ShowDialog() == DialogResult.OK) 
            {
                FileInfo.Text = "Файлы будут сохранены по пути " + fbd.SelectedPath + "\n";
                textBoxPath.Text = fbd.SelectedPath + "\\";
            }
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        { }

        private void groupBox2_Enter(object sender, EventArgs e)
        { }

        private void comboBoxPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxBaudRate.Text = "9600";
            comboBox4.Text = "9600";
            comboBox1.Text = "8";
            comboBox2.Text = "None";
            comboBox3.Text = "1";
            string filesp = @"C:\KR\KR.txt";
            string filedr = @"C:\KR\KR2.txt";

            comboBoxBaudRate.Enabled = comboBoxPort.SelectedIndex <= 0; //disable if the first item is not selected
            comboBox4.Enabled = comboBoxPort.SelectedIndex <= 0;
            comboBox1.Enabled = comboBoxPort.SelectedIndex <= 0;
            comboBox2.Enabled = comboBoxPort.SelectedIndex <= 0;
            comboBox3.Enabled = comboBoxPort.SelectedIndex <= 0;

            if (comboBoxPort.SelectedIndex > 0)
            {
                if (File.Exists(filesp))
                {
                    string str = File.ReadAllText(filesp);
                    comboBox4.Text = str;
                }
                if (File.Exists(filedr))
                {
                    string str = File.ReadAllText(filedr);
                    comboBoxBaudRate.Text = str;
                }
            };

        }
        
        private void textBoxName_TextChanged(object sender, EventArgs e)
        { }

        private void comboBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filesp = @"C:\KR\KR.txt";
            if (comboBoxPort.SelectedIndex == 0)
            {
                string line = comboBoxBaudRate.Text;
                File.WriteAllText(filesp, line);
            }
        }

        private void comboBox4_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string filedr = @"C:\KR\KR2.txt";
            if (comboBoxPort.SelectedIndex == 0)
            {
                string line = comboBox4.Text;
                File.WriteAllText(filedr, line);
            }
        }
    }
}
