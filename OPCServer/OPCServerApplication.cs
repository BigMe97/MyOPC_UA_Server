using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Opc.Ua;
using Opc.UaFx;
using Opc.UaFx.Client;
using Opc.UaFx.Server;
using static System.Net.Mime.MediaTypeNames;


namespace OPCServer
{
    public partial class OPCServerApplication : Form
    {
        OpcServer server;
        
        string configFile = String.Format("{0}\\Configuration.xml", Directory.GetParent(Environment.CurrentDirectory).Parent.FullName);
        string address;

        public OPCServerApplication()
        {
            InitializeComponent();

            GetConfiguration(configFile);
            server.Start();
        }

        public void GetConfiguration(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlReader reader = XmlReader.Create(filePath);
            XmlNode node = doc.SelectSingleNode("CONFIG/address");
            address = Convert.ToString(node.InnerText);
            txtURL.Text = address;

            lbVariables.Items.Clear();
            List<OpcDataVariableNode> nodes = new List<OpcDataVariableNode>();
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "item"))
                {
                    if (reader.HasAttributes)
                    {
                        string name = reader.GetAttribute("name");
                        string typ = reader.GetAttribute("type");
                        lbVariables.Items.Add(name);
                        txtValidate.Text = typ;
                        if (typ == "double")
                        {
                            nodes.Add(new OpcDataVariableNode<double>(name));
                        }
                        else if (typ == "int")
                        {
                            nodes.Add(new OpcDataVariableNode<int>(name));
                        }
                        else if (typ == "string")
                        {
                            nodes.Add(new OpcDataVariableNode<string>(name));
                        }
                        else
                        {
                            nodes.Add(new OpcDataVariableNode(name));

                        }
                    }
                }
            }
            XmlNodeList items = doc.SelectNodes("CONFIG/item");
            
            foreach (XmlNode item in items)
            {
                
            }
            
            server = new OpcServer(address, nodes);

        }

       

        private void timer_Tick(object sender, EventArgs e)
        {
            if (btnStartStop.Text == "Start")
            {
                lbValues.Items.Clear();
            }
            else
            {
                var client = new OpcClient(address);
                client.Connect();
                lbValues.Items.Clear();
                foreach (var item in lbVariables.Items)
                {
                    lbValues.Items.Add(client.ReadNode(String.Format("ns=2;s={0}", item.ToString())));
                }
                client.Disconnect();
            }
        }

        private void OPCServerApplication_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Stop();
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (btnStartStop.Text == "Start")
            {
                server.Stop();
                btnStartStop.Text = "Stop";
                txtURL.Enabled = false;
                GetConfiguration(configFile);
                server.Start();
                btnStartStop.BackColor = Color.Red;

            }
            else
            {
                btnStartStop.Text = "Start";
                btnStartStop.BackColor = Color.Green;
                txtURL.Enabled = true;
                server.Stop();
            }
        }
    }
}
