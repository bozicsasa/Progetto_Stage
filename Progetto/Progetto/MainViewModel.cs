using Gpx;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;

namespace Progetto
{
    class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            ImportCommand = new RelayCommand(Import);
            GpxPointsCollection = new ObservableCollection<GpxPoint>();
        }

        private ObservableCollection<GpxPoint> gpxPointsCollection;
        public ObservableCollection<GpxPoint> GpxPointsCollection
        {
            get { return gpxPointsCollection; }
            set { gpxPointsCollection = value; OnPropertyChange(nameof(GpxPointsCollection)); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChange(string obj)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(obj));
        }

        private XDocument GetGpxDoc(string sFile)
        {
            XDocument gpxDoc = XDocument.Load(sFile);
            return gpxDoc;
        }

        private XNamespace GetGpxNameSpace()
        {
            XNamespace gpx = XNamespace.Get("http://www.topografix.com/GPX/1/1");
            return gpx;
        }

        private string LoadGPXWaypoints(string sFile)
        {
            XDocument gpxDoc = GetGpxDoc(sFile);
            XNamespace gpx = GetGpxNameSpace();

            var waypoints = from waypoint in gpxDoc.Descendants(gpx + "wpt")
                            select new
                            {
                                Latitude = waypoint.Attribute("lat").Value,
                                Longitude = waypoint.Attribute("lon").Value,
                                Dt = waypoint.Element(gpx + "time") != null ?
                                  waypoint.Element(gpx + "time").Value : null,
                            };


            StringBuilder sb = new StringBuilder();
            foreach (var wpt in waypoints)
            {
                // This is where we'd instantiate data
                // containers for the information retrieved.
                sb.Append(
                  string.Format("{0},{1},{2}\n",
                  wpt.Latitude, wpt.Longitude, wpt.Dt));
            }

            return sb.ToString();
        }

        private void FillList(string file)
        {
            using (StringReader reader = new StringReader(file))
            {
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    string[] splittedLine = line.Split(',');
                    string latitude = splittedLine[0].Replace("°", "");
                    string longitude = splittedLine[1].Replace("°", "");
                    string time = splittedLine[2];
                    time = time.Replace('T', ' ');
                    time = time.Remove(time.IndexOf('Z'));

                    //DateTime dateTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    //GpxPoint gpxPoint = new GpxPoint();
                    //gpxPoint.Latitude = Convert.ToDouble(latitude);
                    //gpxPoint.Longitude = Convert.ToDouble(longitude);
                    //gpxPoint.Time = dateTime;
                    //GpxPointsCollection.Add(gpxPoint);

                    GpxPointsCollection.Add(new GpxPoint
                    {
                        Latitude = Convert.ToDouble(latitude, CultureInfo.InvariantCulture),
                        Longitude = Convert.ToDouble(longitude, CultureInfo.InvariantCulture),
                        Time = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Data = $"Time {time} - Lat: {latitude} - Lon: {longitude}"
                    });
                }
            }
        }


        public ICommand ImportCommand { get; set; }
        private void Import(object obj)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "Xml files (*.xml)|*.xml",
                Title = "Importa file"
            };
            if ((bool)open.ShowDialog())
            {
                string gpxPoints = LoadGPXWaypoints(open.FileName);
                FillList(gpxPoints);
            }
        }
    }
}
