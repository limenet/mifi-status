using System.Windows;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Text;
using System.IO;
using NativeWifi;
using System.Collections.ObjectModel;
using Windows.UI.Notifications;
using System.Diagnostics;

namespace mifi_status
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ToastNotifier toastManager;
        int oldBatteryPercentage = 100;
        private WlanClient wlan;
        public MainWindow()
        {
            this.toastManager = ToastNotificationManager.CreateToastNotifier("mifi-battery-low");
            InitializeComponent();
        }
        public void refreshData()
        {
            string ssid0;
            try
            {
                if (wlan == null)
                {
                    this.wlan = new WlanClient();
                }
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
                    string[] networks = { "no service", "GSM", "3G", "LTE", "TD-SCDMA", "CDMA 1x", "CDMA EVDO" };
                    string[] sims = { "invalid", "no SIM", "error", "ready", "PIN requested", "PIN verified", "PUK requested", "permanently locked" };

                    int voltage = Convert.ToInt32(json.battery.voltage);
                    this.statusConn.Content = connections[json.wan.connectStatus];
                    this.statusBatt.Content = json.battery.voltage + "%";
                    this.statusNet.Content = networks[json.wan.networkType] + " (" + json.wan.signalStrength + "/4)";
                    this.statusSIM.Content = sims[json.wan.simStatus];
                    this.statusClients.Content = json.connectedDevices.number;
                    double ds = Convert.ToDouble(json.wan.dailyStatistics);
                    double ts = Convert.ToDouble(json.wan.totalStatistics);
                    this.statusUse.Content = getFilesizeHuman(ds, 2) + " / " + getFilesizeHuman(ts, 2);
                    double rxs = Convert.ToDouble(json.wan.rxSpeed);
                    double txs = Convert.ToDouble(json.wan.txSpeed);
                    this.statusSpeed.Content = getFilesizeHuman(txs, 1) + "/s / " + getFilesizeHuman(rxs, 1) + "/s";

                    sendBatteryNotification(voltage);
                }
                catch (Exception)
                {
                    setAllToEmptyString();
                }
            }
            else
            {
                setAllToEmptyString();
            }

        }

        private void setAllToEmptyString()
        {
            String empty = "–";
            this.statusConn.Content = empty;
            this.statusBatt.Content = empty;
            this.statusNet.Content = empty;
            this.statusSIM.Content = empty;
            this.statusUse.Content = empty;
            this.statusSpeed.Content = empty;
            this.statusClients.Content = empty;
        }

        public string getFilesizeHuman(double len, int decimalPlaces = 0)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1024;
            }

            String decimalPlacesString = "";

            while (decimalPlaces > 0)
            {
                decimalPlacesString += "0";
                decimalPlaces--;
            }

            return String.Format("{0:0." + decimalPlacesString + "} {1}", len, sizes[order]);
        }

        public void sendBatteryNotification(int percentage)
        {
            if (this.oldBatteryPercentage != percentage)
            {
                this.oldBatteryPercentage = percentage;
                if (percentage <= 20)
                {
                    var xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                    var text = xml.GetElementsByTagName("text");
                    text[0].AppendChild(xml.CreateTextNode("mifi-status"));
                    text[1].AppendChild(xml.CreateTextNode("Battery level: " + percentage + "%"));
                    var toast = new ToastNotification(xml);
                    this.toastManager.Show(toast);
                }
            }
        }

    }
}