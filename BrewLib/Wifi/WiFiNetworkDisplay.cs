﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BrewLib.WiFi
{
    public class WiFiNetworkDisplay : INotifyPropertyChanged
    {
        private readonly WiFiAdapter _adapter;
        public WiFiNetworkDisplay(WiFiAvailableNetwork availableNetwork, WiFiAdapter adapter)
        {
            AvailableNetwork = availableNetwork;
            this._adapter = adapter;
            UpdateWiFiImage();
            UpdateConnectivityLevel();
        }

        public bool IsOpenNetwork
        {
            get { return AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211; }
        }

        private async void UpdateWiFiImage()
        {
            string imageFileNamePrefix = "secure";
            if (AvailableNetwork.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.Open80211)
            {
                imageFileNamePrefix = "open";
            }

            string imageFileName = string.Format("ms-appx:/Assets/{0}_{1}bar.png", imageFileNamePrefix, AvailableNetwork.SignalBars);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                WiFiImage = new BitmapImage(new Uri(imageFileName));
                OnPropertyChanged("WiFiImage");
            });
        }

        public async void UpdateConnectivityLevel()
        {
            string connectivityLevel = "Not Connected";
            string connectedSsid = null;

            var connectedProfile = await _adapter.NetworkAdapter.GetConnectedProfileAsync();
            if (connectedProfile != null &&
                connectedProfile.IsWlanConnectionProfile &&
                connectedProfile.WlanConnectionProfileDetails != null)
            {
                connectedSsid = connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid();
            }

            if (!string.IsNullOrEmpty(connectedSsid))
            {
                if (connectedSsid.Equals(AvailableNetwork.Ssid))
                {
                    connectivityLevel = connectedProfile.GetNetworkConnectivityLevel().ToString();
                }
            }

            ConnectivityLevel = connectivityLevel;

            OnPropertyChanged("ConnectivityLevel");
            OnPropertyChanged("Background");
        }

        public string Ssid
        {
            get
            {
                return availableNetwork.Ssid;
            }
        }

        public string Bssid
        {
            get
            {
                return availableNetwork.Bssid;

            }
        }

        public string ChannelCenterFrequency
        {
            get
            {
                return string.Format("{0}kHz", availableNetwork.ChannelCenterFrequencyInKilohertz);
            }
        }

        public string Rssi
        {
            get
            {
                return string.Format("{0}dBm", availableNetwork.NetworkRssiInDecibelMilliwatts);
            }
        }

        public SolidColorBrush Background
        {
            get
            {
                return string.IsNullOrEmpty(ConnectivityLevel) ? new SolidColorBrush(Colors.Transparent) : (ConnectivityLevel.Equals("Not Connected")
                                                                                                            ? new SolidColorBrush(Colors.Black)
                                                                                                            : new SolidColorBrush(Colors.DarkGreen));
            }
        }

        public string SecuritySettings
        {
            get
            {
                return string.Format("Authentication: {0}; Encryption: {1}", availableNetwork.SecuritySettings.NetworkAuthenticationType, availableNetwork.SecuritySettings.NetworkEncryptionType);
            }
        }
        public string ConnectivityLevel
        {
            get;
            private set;
        }

        public BitmapImage WiFiImage
        {
            get;
            private set;
        }


        private WiFiAvailableNetwork availableNetwork;
        public WiFiAvailableNetwork AvailableNetwork
        {
            get
            {
                return availableNetwork;
            }

            private set
            {
                availableNetwork = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

}
