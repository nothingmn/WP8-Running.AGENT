using System.Threading.Tasks;
using GpsSimulator;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Shell;
using NExtra.Geo;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WP8Runner
{
    public partial class MainPage : PhoneApplicationPage
    {
        //private IGeoPositionWatcher<GeoCoordinate> _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
        private IGeoPositionWatcher<GeoCoordinate> _watcher = new GeoCoordinateSimulator();
        private MapPolyline _line;
        private DispatcherTimer _timer = new DispatcherTimer();
        private long _startTime;

        public MainPage()
        {
            InitializeComponent();
            _started = false;
            // create a line which illustrates the run
            _line = new MapPolyline();
            _line.StrokeColor = Colors.Red;
            _line.StrokeThickness = 5;
            Map.MapElements.Add(_line);


            _watcher.PositionChanged += Watcher_PositionChanged;
            _watcher.Start();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan runTime = TimeSpan.FromMilliseconds(System.Environment.TickCount - _startTime);
            _time = runTime.ToString(@"hh\:mm\:ss");
            timeLabel.Text = _time;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            if (_timer.IsEnabled)
            {
                _started = false;
                _timer.Stop();
                StartButton.Content = "Start";
            }
            else
            {
                _started = true;
                if (_line.Path.Count > 0)
                {
                    _line.Path.Clear();
                    Map.UpdateLayout();
                }

                _timer.Start();
                _startTime = System.Environment.TickCount;
                StartButton.Content = "Stop";
            }
        }

        private bool _started = false;
        //ID_CAP_LOCATION
        private double _kilometres;
        private long _previousPositionChangeTick;

        private StreamSocket _socket = null;
        private DataWriter _dataWriter = null;

        private async Task<bool> SetupDeviceConn()
        {
            //Connect to your paired host PC using BT + StreamSocket (over RFCOMM)
            PeerFinder.AlternateIdentities["Bluetooth:PAIRED"] = "";

            var devices = await PeerFinder.FindAllPeersAsync();

            if (devices.Count == 0)
            {
                MessageBox.Show("No paired device");
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            var peerInfo = devices.FirstOrDefault(c => c.DisplayName.Contains("ROB17R"));
            if (peerInfo == null)
            {
                MessageBox.Show("No paired device");
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:"));
                return false;
            }

            _socket = new StreamSocket();

            //"{00001101-0000-1000-8000-00805f9b34fb}" - is the GUID for the serial port service.
            await _socket.ConnectAsync(peerInfo.HostName, "{00001101-0000-1000-8000-00805f9b34fb}");

            _dataWriter = new DataWriter(_socket.OutputStream);

            
            return true;
        }

        private void Watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var coord = new GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude);

            if (_started)
            {
                var prev = Map.Center;
                if (_line.Path.Count > 0) prev = _line.Path.Last();
                var previousPoint = prev;
                var distance = coord.GetDistanceTo(previousPoint);
                if (distance > 0)
                {
                    var millisPerKilometer = (1000.0/distance)*
                                             (System.Environment.TickCount - _previousPositionChangeTick);

                    _kilometres += distance/1000.0;

                    _pace = TimeSpan.FromMilliseconds(millisPerKilometer).ToString(@"mm\:ss");
                    paceLabel.Text = _pace;
                    caloriesLabel.Text = string.Format("{0:f0}", _kilometres * 65);
                    _distance = string.Format("{0:f2} km", _kilometres);
                    distanceLabel.Text = _distance;
                    _calories = string.Format("{0:f0}", _kilometres * 65);
                    caloriesLabel.Text = _calories; 
                    

                    PositionHandler handler = new PositionHandler();
                    var heading = handler.CalculateBearing(new Position(previousPoint), new Position(coord));
                    Map.SetView(coord, Map.ZoomLevel, heading, MapAnimationKind.Parabolic);
                    var title = "WP8 Runner";
                    if (_connected) title += " - Connected to AGENT";
                    ShellTile.ActiveTiles.First().Update(new IconicTileData()
                        {

                            Title = title,
                            WideContent1 = string.Format("{0:f2} km", _kilometres),
                            WideContent2 = string.Format("{0:f0} calories", _kilometres*65),
                        });
                    _line.Path.Add(coord);
                    _previousPositionChangeTick = System.Environment.TickCount;
                }
            }
            else
            {
                Map.Center = coord;
            }
            SendPosition(coord);
        }

        private async void SendPosition(GeoCoordinate coord)
        {
            if (_connected && _started)
            {
                string payload = string.Format("Data,{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", coord.Altitude,
                                               coord.Course, coord.HorizontalAccuracy,
                                               coord.Latitude, coord.Longitude, coord.Speed, coord.VerticalAccuracy,
                                               _pace, _distance, _calories, _time);
                Debug.WriteLine("SENDING PAYLOAD" + payload);
                _dataWriter.WriteString( payload + '\0');
                
                await _dataWriter.StoreAsync();
            }
        }

        private async void SendHello()
        {
            if (_connected)
            {
                string payload = string.Format("Hello");
                Debug.WriteLine("SENDING PAYLOAD" + payload);
                _dataWriter.WriteString(payload + '\0');

                await _dataWriter.StoreAsync();
            }
        }

        private string _pace = "00:00";
        private string _distance = "0 km";
        private string _calories = "0";
        private string _time = "00:00";
        private bool _connected = false;
        private async void ConnectToAgent()
        {
            if (await SetupDeviceConn())
            {
                ConnectButton.IsEnabled = false;
                _connected=true;
                SendHello();

            }
        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToAgent();
        }
    }
}