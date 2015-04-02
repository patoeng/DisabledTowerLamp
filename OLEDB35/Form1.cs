using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OLEDB35
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.TransparencyKey = Color.Turquoise;
            this.BackColor = Color.Turquoise;
        }
        private NotifyIcon _trayIcon;
        private ContextMenu _trayMenu;
        private byte _step;
        private int _testmode, _enabledDut;
        private string _database ;
        private int _scanningRate;
        private string _recieverIp;
        private int _recieverPort;
        private int _listenPort;
        private bool _com;
        private byte[] _getUdpDataBack = new byte[600];

        private static UdpClient _udp;
        private static bool _getData;

        private Thread _myTh;
        public  void MyThread()
        {
            AppendText(textBox1, "Thread Started!\r\n");
            _udp = new UdpClient(_listenPort);
            while (true)
            {
               
                var ipl = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _listenPort);
                var message = _udp.Receive(ref ipl);
                _getUdpDataBack = message;
               // AppendText(textBox1,Encoding.ASCII.GetString(message));
                _getData = true;
                _com = false;
            }
        }

        private delegate void AppendTextBoxDelegate(TextBox tb, string s);
        private void AppendText(TextBox tb, string s)
        {

            if (tb.InvokeRequired)
            {
                tb.BeginInvoke(new AppendTextBoxDelegate(AppendText),new object[]{tb,s});
                
            }
            else
            {
                tb.AppendText(s);
            }
        }
      
        private void button1_Click(object sender, EventArgs e)
        {//Microsoft.ACE.OLEDB.12.0 //Microsoft.JET.OLEDB.4.0
          
        }
        //(eventdetails like 'TESTMODE%') OR ()
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            //Debug.WriteLine("sdfsdfsa");
            var myDataTable = new DataTable();
            try
            {
               using (  var conection =new OleDbConnection("Provider=Microsoft.JET.OLEDB.4.0;" + "data source=" + _database + ";"))
               // using (var conection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;" + "data source="+_database+";"))
                {
                    float percent = 0;
                    conection.Open();

                    var query =
                        "Select top 1 [eventdetails] from [tracking] where ([eventdetails] like 'STATCS4T%')  order by ID DESC ";
                    if (_step == 1)
                    {
                        query =
                            "Select top 1 [eventdetails] from tracking a  where (a.[event]='0x030B')OR ([eventdetails] like 'STATCS4T%')  order by ID DESC ";
                    }

                    var command = new OleDbCommand(query, conection);
                    var reader = command.ExecuteReader();
                  //  textBox1.AppendText(_step + "\r\n"); 
                    if (reader != null && reader.Read())
                    {
                     
                        var i = reader[0].ToString();
                        //   textBox1.AppendText(i + "=>" + _step + "\r\n");
                        if (_step == 0)
                        {
                            _testmode = i.Split('1').Length - 1;
                            if (_testmode <= 0)
                            {
                                _testmode = _enabledDut;
                            }
                         
                            label2.Text = "Test Mode : "+ _testmode;

                        }
                        else
                        {
                            if (_testmode <= 0)
                            {
                                _testmode = i.Split('1').Length - 1 + i.Split('2').Length - 1;
                                label2.Text = "Test Mode : " + _testmode;
                            }
                            _enabledDut = i.Split('1').Length - 1;
                            //textBox1.AppendText(i + " enabled dut: " + _enabledDut + "\r\n");
                            label3.Text = "Enabled DUT : " + _enabledDut;
                            percent = (float) _enabledDut*100/_testmode;
                            label1.Text = percent + "%";



                            //
                            

                            //
                            var udpsend = new UdpClient(0);
                            var reciever = new IPEndPoint(IPAddress.Parse(@_recieverIp), _recieverPort);

                            //dataBytes[0] =
                            var dataBytes = new byte[6];
                            // = System.Text.Encoding.ASCII.GetBytes(percent.ToString("F0"));
                            var ips = _recieverIp.Split('.');
                            dataBytes[0] = Convert.ToByte(ips[2]);
                            dataBytes[1] = Convert.ToByte(ips[3]);
                            dataBytes[2] = 255;
                            dataBytes[3] = 255;
                            dataBytes[4] = (byte) 'E';
                            if (percent >= 100)
                            {
                                dataBytes[5] = 1;
                                label1.ForeColor = Color.Green;
                            }
                            else
                            {
                                if (percent >= 75)
                                {
                                    dataBytes[5] = 2;
                                    label1.ForeColor = Color.Blue;
                                }
                                else
                                {
                                    if (percent > 50)
                                    {
                                        dataBytes[5] = 4;
                                        label1.ForeColor = Color.Yellow;
                                    }
                                    else
                                    {
                                        dataBytes[5] = 8;
                                        label1.ForeColor = Color.Red;
                                    }
                                }
                            }
                            //dataBytes[5] = 1;
                            label4.Text = _com ? "NET: Failed" : "NET: OK";
                            udpsend.Send(dataBytes, dataBytes.Length, reciever);
                            _com = true;
                        }
                    }
                    if (_step == 1)
                    {
                        if ((_testmode <= 0) || (_enabledDut <= 0))
                        {
                            label2.Text = "Test Mode : " + _testmode;
                            label3.Text = "Enabled DUT : " + _enabledDut;
                             label1.Text = "0%";
                        }
                        _testmode = 0;
                        _enabledDut = 0;
                    }
                    _step = (byte) (_step == 0 ? 1 : 0);
                   if (reader!=null) reader.Close();
                   conection.Close();

                }
                
            }
            catch (Exception ex)
            {
              // MessageBox.Show(ex.Message);
                timer1.Enabled = true;
            }
            timer1.Enabled = true;
        }
      
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Location = new Point(0, 0);
                // Create a simple tray menu with only one item.
                _trayMenu = new ContextMenu();
                _trayMenu.MenuItems.Add("Show", OnShow);
                _trayMenu.MenuItems.Add("Hide", OnHide);
                _trayMenu.MenuItems.Add("-");
                _trayMenu.MenuItems.Add("Exit", OnExit);

                // Create a tray icon. In this example we use a
                // standard system icon for simplicity, but you
                // can of course use your own custom icon too.
                _trayIcon = new NotifyIcon
                    {
                        Text = "Disabled DUT Tower Light v0.8",
                        Icon = new Icon(Icon, 40, 40),
                        ContextMenu = _trayMenu,
                        Visible = true
                    };
                _trayIcon.Click += _trayIcon_Click;

                //trayIcon.Icon = new Icon(".\\hourly.ico");
                // Add menu to tray icon and show it.
                var iniFile = new ini(".\\setting.ini");
                _database = iniFile.IniReadValue("setting", "TrackingLocation");
                _scanningRate = Convert.ToInt32(iniFile.IniReadValue("setting", "ScanningRate"))*100;
                _scanningRate = _scanningRate <= 0 ? 1000 : _scanningRate;
                _recieverIp = iniFile.IniReadValue("Controller", "IP");
                _recieverPort = Convert.ToInt32(iniFile.IniReadValue("Controller", "PORT"));
                _listenPort = Convert.ToInt32(iniFile.IniReadValue("setting", "PORT"));

                while (!File.Exists(_database))
                {
                    var savefile = new OpenFileDialog
                        {
                            //InitialDirectory = Path.GetDirectoryName(_filePath),
                            //OverwritePrompt = true,
                            Title = "Rasco Tracking database location",
                            FileName = Path.GetFileName(_database),
                            Filter = "Log files (*.mdb)|*.mdb",
                        };

                    // set a default file name
                    // set filters - this can be done in properties as well

                    if (savefile.ShowDialog() == DialogResult.OK)
                    {
                        _database = savefile.FileName;
                        _database = Path.GetFullPath(_database);
                        iniFile.IniWriteValue("setting", "TrackingLocation", _database);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            timer1.Interval = _scanningRate;
            timer1.Enabled = true;
            _myTh=new Thread(new ThreadStart(MyThread));
            _myTh.Start();
            Thread.Sleep(1);
            
        }

        void _trayIcon_Click(object sender, EventArgs e)
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _myTh.Abort();
            _udp.Close();
        }
        ////
        protected override void OnLoad(EventArgs e)
        {
            Visible = true; //show form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void OnHide(object sender, EventArgs e)
        {
            Visible = false;
        }
        private void OnShow(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            Visible = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
       
    }
}
