using System.Net;
using Newtonsoft.Json;
using System;
using System.Text;
using System.IO;
using NativeWifi;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace mifi_status
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _oldBatteryPercentage = 100;
        private int _lowBatteryPercentage = 25;
        private WlanClient _wlan;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            string ssid0;
            try
            {
                if (_wlan == null)
                {
                    _wlan = new WlanClient();
                }
                Collection<string> connectedSsids = new Collection<string>();

                foreach (var wlanInterface in _wlan.Interfaces)
                {
                    var ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                    connectedSsids.Add(new string(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength)));
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
                    var request = (HttpWebRequest)WebRequest.Create("http://192.168.0.1/cgi-bin/qcmap_web_cgi");
                    request.ServicePoint.Expect100Continue = false;
                    request.Method = "POST";
                    byte[] data = Encoding.UTF8.GetBytes("{\"module\":\"status\",\"action\":0}");
                    request.ContentLength = data.Length;
                    var stream = request.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                    var response = (HttpWebResponse)request.GetResponse();
                    stream = response.GetResponseStream();

                    if (stream == null) return;

                    var reader = new StreamReader(stream);
                    var serializedJson = reader.ReadToEnd();

                    dynamic json = JsonConvert.DeserializeObject(serializedJson);

                    string[] connections = { "disable", "disconnected", "connecting", "disconnecting", "connected" };
                    string[] networks = { "no service", "GSM", "3G", "LTE", "TD-SCDMA", "CDMA 1x", "CDMA EVDO" };
                    string[] sims = { "invalid", "no SIM", "error", "ready", "PIN requested", "PIN verified", "PUK requested", "permanently locked" };

                    var voltage = Convert.ToInt32(json.battery.voltage);
                    StatusConn.Content = connections[json.wan.connectStatus];
                    StatusBatt.Content = json.battery.voltage + "%";
                    StatusNet.Content = networks[json.wan.networkType] + " (" + json.wan.signalStrength + "/4)";
                    StatusSim.Content = sims[json.wan.simStatus];
                    StatusClients.Content = json.connectedDevices.number;
                    StatusMessages.Content = json.message.unreadMessages;
                    var ds = Convert.ToDouble(json.wan.dailyStatistics);
                    var ts = Convert.ToDouble(json.wan.totalStatistics);
                    StatusUse.Content = GetFilesizeHuman(ds, 2) + " / " + GetFilesizeHuman(ts, 2);
                    var rxs = Convert.ToDouble(json.wan.rxSpeed);
                    var txs = Convert.ToDouble(json.wan.txSpeed);
                    StatusSpeed.Content = GetFilesizeHuman(txs, 1) + "/s / " + GetFilesizeHuman(rxs, 1) + "/s";

                    LowBatteryNotification(voltage);
                }
                catch (Exception)
                {
                    SetAllToEmptyString();
                }
            }
            else
            {
                SetAllToEmptyString();
            }

        }

        private void SetAllToEmptyString()
        {
            const string empty = "–";
            StatusConn.Content = empty;
            StatusBatt.Content = empty;
            StatusNet.Content = empty;
            StatusSim.Content = empty;
            StatusUse.Content = empty;
            StatusSpeed.Content = empty;
            StatusClients.Content = empty;
        }

        private static string GetFilesizeHuman(double len, int decimalPlaces = 0)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            var order = 0;
            while (len >= 1024 && ++order < sizes.Length)
            {
                len = len / 1024;
            }

            var decimalPlacesString = "";

            while (decimalPlaces > 0)
            {
                decimalPlacesString += "0";
                decimalPlaces--;
            }
            
            // ReSharper disable FormatStringProblem
            return string.Format("{0:0." + decimalPlacesString + "} {1}", len, sizes[order]);
        }

        private void LowBatteryNotification(int percentage)
        {
            if (_oldBatteryPercentage == percentage) return;

            _oldBatteryPercentage = percentage;

            var brush = percentage <= _lowBatteryPercentage ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.Transparent);

            LabelBatt.Background = brush;
            StatusBatt.Background = brush;

        }

    }
}