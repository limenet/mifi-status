using System.Windows;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Text;
using System.IO;
using NativeWifi;
using System.Collections.ObjectModel;

namespace mifi_status
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private WlanClient wlan;
        public void refreshData()
        {
            if (wlan == null)
            {
                this.wlan = new WlanClient();
            }
            string ssid0;
            try
            {
                Collection<String> connectedSsids = new Collection<string>();

                foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces)
                {
                    Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                    connectedSsids.Add(new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength)));
                }

                ssid0 = connectedSsids[0];
            }
            catch (Exception)
            {
                ssid0 = "";
            }

            if (ssid0 == "Albatross")
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.0.1/cgi-bin/qcmap_web_cgi");
                    request.ServicePoint.Expect100Continue = false;
                    request.Method = "POST";
                    byte[] data = Encoding.UTF8.GetBytes("{\"module\":\"status\",\"action\":0}");
                    request.ContentLength = data.Length;
                    Stream stream = request.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    string serializedJson = reader.ReadToEnd();

                    dynamic json = JsonConvert.DeserializeObject(serializedJson);

                    string[] connections = { "disable", "disconnected", "connecting", "disconnecting", "connected" };
                    string[] networks = { "no service", "GSM", "WCDMA", "LTE", "TD-SCDMA", "CDMA 1x", "CDMA EVDO" };
                    string[] sims = { "invalid", "no SIM", "error", "ready", "PIN requested", "PIN verified", "PUK requested", "permanently locked" };

                    this.statusConn.Content = connections[json.wan.connectStatus];
                    this.statusBatt.Content = json.battery.voltage + "%";
                    this.statusNet.Content = networks[json.wan.networkType];
                    this.statusSIM.Content = sims[json.wan.simStatus];
                    this.statusUse.Content = getFilesizeHuman(Convert.ToDouble(json.wan.totalStatistics));
                    double rxs = Convert.ToDouble(json.wan.rxSpeed);
                    double txs = Convert.ToDouble(json.wan.txSpeed);
                    this.statusSpeed.Content = getFilesizeHuman(txs) + "/s / " + getFilesizeHuman(rxs) + "/s";
                }
                catch (Exception)
                {
                    setAllToNA();
                }
            }
            else
            {
                setAllToNA();
            }

        }

        private void setAllToNA()
        {
            this.statusConn.Content = "N/A";
            this.statusBatt.Content = "N/A";
            this.statusNet.Content = "N/A";
            this.statusSIM.Content = "N/A";
            this.statusUse.Content = "N/A";
            this.statusSpeed.Content = "N/A";
        }

        public string getFilesizeHuman(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }
}
