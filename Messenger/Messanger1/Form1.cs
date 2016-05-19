using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.SqlServer.Server;
using Mono.Nat;
namespace Messanger1
{
    public partial class Form1 : Form
    {


        private string globalIP;
        private Thread t;
        private Thread t2;
        private TcpClient client;
        private TcpClient client2;
        private TcpListener server;
        private StreamReader srout;
        private StreamWriter swout;
        private StreamWriter swin;
        private StreamReader srin;
        private Socket listenerSocket;
        private TcpListener tcpLsn;
        private Int32 port;
        private INatDevice device;
        public Form1()
        {
            InitializeComponent();
        }
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();
            // t = new Thread(GettingIP);
            // t.Start();
            label1.Text = "[Getting your external IP...]";
            t2 = new Thread(Snooper);
            port = Convert.ToInt32(GetOpenPort());
            richTextBox1.AppendText(">Opened port: " + port + "\n");
            t2.Start();
        }


        public void GettingIP()
        {

            globalIP = GetPublicIP();
            label1.Invoke(new Action(() => label1.Text = String.Format("Your IP:\n{0}", globalIP)));
            t.Abort();
        }
        public static string GetPublicIP()
        {
            try
            {
                string url = "http://checkip.dyndns.org";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];
                return a4;
            }
            catch
            {
                return " ";
            }

        }
        private string GetOpenPort()
        {
            int PortStartIndex = 1000;
            int PortEndIndex = 2000;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

            List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
            int unusedPort = 0;

            for (int port = PortStartIndex; port < PortEndIndex; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    unusedPort = port;
                    break;
                }
            }
            return unusedPort.ToString();
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            device = args.Device;
            richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(">NAT device found! \n")));
            device.CreatePortMap(new Mapping(Protocol.Tcp, port, port));
            globalIP = device.GetExternalIP().ToString();
            label1.Invoke(new Action(() => label1.Text = ""));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Yellow));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionBackColor = Color.Green));
            richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(String.Format(">Server using IP:{0}:{1}\n",globalIP, port))));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Black));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionBackColor = Color.LightBlue));
        }

        private void DeviceLost(object sender, DeviceEventArgs args)
        {
            device = args.Device;
        }


        public void Snooper()
        {

            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
            }
            catch
            {
                MessageBox.Show("Bad PORT");
                return;
            }
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Yellow));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionBackColor = Color.Green));
            richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(String.Format(">Server started. Currently using port:{0}\n",  port))));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Black));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionBackColor = Color.LightBlue));
           
            //MessageBox.Show("Server Started!"); 
            client2 = server.AcceptTcpClient();
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Green));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionBackColor = Color.Yellow));
            richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(">Connection with client established\n")));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Black));
            richTextBox1.Invoke(new Action(() => richTextBox1.SelectionBackColor = Color.LightBlue));
            while (true)
            {
                try
                {
                    srin = new StreamReader(client2.GetStream());
                    string message = srin.ReadLine();
                    // StreamWriter sw = new StreamWriter(client2.GetStream());
                    richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Blue));
                    richTextBox1.Invoke(new Action(() => richTextBox1.AppendText(message + "\n")));
                    richTextBox1.Invoke(new Action(() => richTextBox1.SelectionColor = Color.Black));
                }
                catch
                {
                    MessageBox.Show("Client lost!");
                    break;
                }

            }

        }
        private void newConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPRequester ipr = new IPRequester();
            ipr.ShowDialog();
            try
            {
                TcpClient client = new TcpClient(IPRequester.targetip, IPRequester.targetport);
                swout = new StreamWriter(client.GetStream());
                richTextBox1.AppendText("___________________________________________");
                swout.WriteLine("Hello, i'm client:{0}, my port is:{1}\n", globalIP, port);
                swout.Flush();
            }
            catch
            {

            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                swout.WriteLine(textBox1.Text);
                richTextBox1.SelectionAlignment = HorizontalAlignment.Right;
                richTextBox1.SelectionColor = Color.Green;
                richTextBox1.AppendText(textBox1.Text + "\n");
                richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                richTextBox1.SelectionColor = Color.Black;
                swout.Flush();
            }
            catch (Exception ex)
            {
                richTextBox1.SelectionColor = Color.DodgerBlue;
                richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Italic);
                richTextBox1.SelectionAlignment=HorizontalAlignment.Right;
                richTextBox1.AppendText("[Message error]\n");
                richTextBox1.SelectionAlignment = HorizontalAlignment.Left;
                richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Regular);
                richTextBox1.SelectionColor = Color.Black;
            }
            textBox1.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            device.DeletePortMap(new Mapping(Protocol.Tcp, port, port));
            try
            {
                //t.Join();
                t.Abort();
               // t2.Join();
                t2.Abort();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Some generic problems occured. Don't worry just noticing.",
                    "Generic error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            Environment.Exit(0);
            //Application.Exit();

        }



    }
}

