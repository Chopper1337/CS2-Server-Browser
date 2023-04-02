using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace CS2_Server_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List of servers
        List<Server> servers = new List<Server>();

        // Relative path to CS2 executable
        string gameExecutable = "bin\\win64\\cs2.exe";

        private bool ready = false;

        // List of priority levels
        List<string> priorityLevels = new List<string>
        {
            "High",
            "Normal",
            "Low"
        };

        // List of languages
        List<string> languages = new List<string>
        {
            "English",
            "French",
            "German",
            "Italian",
            "Spanish",
            "Polish",
            "Portuguese",
            "Russian",
            "Korean",
            "Japanese",
            "Chinese",
            "Thai",
            "Vietnamese",
            "Bulgarian",
            "Czech",
            "Danish",
            "Dutch",
            "Finnish",
            "Greek",
            "Hungarian",
            "Norwegian",
            "Romanian",
            "Swedish",
            "Turkish",
            "Ukrainian"
        };

        // List of valid monitor frequencies
        List<string> monitorFrequencies = new List<string>
        {
            "60",
            "75",
            "85",
            "100",
            "120",
            "144",
            "165",
            "240",
            "360"
        };

        public MainWindow()
        {
            InitializeComponent();

            StatusMessageTextBlock.Text = "Loading list headers...";
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Location", Binding = new Binding("location") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("name") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "IP", Binding = new Binding("ip") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Port", Binding = new Binding("port") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Gamemode", Binding = new Binding("gamemode") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("status") });

            PriorityComboBox.ItemsSource = priorityLevels;
            LanguageComboBox.ItemsSource = languages;
            FreqComboBox.ItemsSource = monitorFrequencies;
            ThreadsComboBox.ItemsSource = FindThreads();
            StatusMessageTextBlock.Text = "Loading combo boxes...";

            // Set saved values
            PriorityComboBox.SelectedItem = CS2_Server_Browser.Properties.Settings.Default.Priority;
            LanguageComboBox.SelectedItem = CS2_Server_Browser.Properties.Settings.Default.Language;
            FreqComboBox.SelectedItem = CS2_Server_Browser.Properties.Settings.Default.Frequency;
            ThreadsComboBox.SelectedItem = CS2_Server_Browser.Properties.Settings.Default.Threads;
            StatusMessageTextBlock.Text = "Loading saved settings...";

            MessageTextBlock.Text =
                DownloadText("https://raw.githubusercontent.com/Chopper1337/CS2-Server-Browser/master/message");

            ready = true;

        }

        // Find how many threads the CPU has and add them to the combobox
        private List<string> FindThreads()
        {
            int threadCount = Environment.ProcessorCount;
            List<string> threadList = new List<string>();
            for (int i = 1; i <= threadCount; i++)
            {
                threadList.Add(i.ToString());
            }
            return threadList;
        }

        // Verify executable file
        private bool VerifyGameExecutable()
        {
            StatusMessageTextBlock.Text = "Verifying CS2.exe exists...";
            if (!File.Exists(gameExecutable))
            {
                MessageBox.Show("CS2.exe not found!");
                return false;
            }

            return true;
        }   

        // Fill the "servers" list with servers from a file named "servers.txt"
        private void LoadServers(object sender, RoutedEventArgs e)
        {
            StatusMessageTextBlock.Text = "Loading servers.txt...";
            StreamReader reader = new StreamReader("servers.txt");
            // For each line in the file, create a new server and add it to the list
            foreach (var line in File.ReadLines("servers.txt"))
            {
                var serverData = line.Split(',');
                var server = new Server
                {
                    name = serverData[0],
                    ip = serverData[1],
                    port = serverData[2],
                    gamemode = (Gamemode)Enum.Parse(typeof(Gamemode), serverData[3]),
                    location = FindLocation(serverData[1]),
                    status = Ping(serverData[1])
                };

                servers.Add(server);
            }

            //Make the DataGrid show the servers
            foreach (Server server in servers)
            {
                ServerDataGrid.Items.Add(server);
            }
        }

        // Check if "servers.txt" exists, if it does, load the servers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("servers.txt"))
                // Download the file from Github
                using (var client = new WebClient())
                    client.DownloadFile("https://raw.githubusercontent.com/Chopper1337/CS2-Server-Browser/master/servers.txt", "servers.txt");

            if (new FileInfo("servers.txt").Length != 0)
                LoadServers(sender, e);
        }

        public void StartGame(string threads, string freq, string lang, string priority, string ip, string port)
        {
            string arguments = "-insecure -threads " + threads + " -freq " + freq + " -nojoy -belowaverage -fullscreen -limitvsconst -forcenovsync -softparticlesdefaultoff -console -language " + lang + " -novid -" + priority + $" +connect {ip}:{port}";
            Process.Start(gameExecutable, arguments);
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            if (!VerifyGameExecutable()) return;
            if(!(ServerDataGrid.SelectedItem is Server selectedServer)) return;

            string ip = selectedServer.ip;
            string port = selectedServer.port;
            string threads = ThreadsComboBox.SelectedValue.ToString().Split(':')[0];
            string freq = FreqComboBox.SelectedValue.ToString().Split(':')[0];
            string lang = LanguageComboBox.SelectedValue.ToString().Split(':')[0];
            string priority = PriorityComboBox.SelectedValue.ToString().Split(':')[0];
            StartGame(threads, freq, lang, priority, ip, port);
            
            StatusMessageTextBlock.Text = "Connecting to " + selectedServer.ip;
        }

        // Ping the selected server and return its status (Online/Offline)
        private Status Ping(string ip)
        {
            StatusMessageTextBlock.Text = $"Pinging {ip}...";
            var pingSender = new Ping();
            var reply = pingSender.Send(ip);
            return reply != null && reply.Status == IPStatus.Success ? Status.Online : Status.Offline;
        }


        // Pull text from a URI
        private string DownloadText(string uri)
        {
            StatusMessageTextBlock.Text = "Loading custom message from GitHub...";
            string message = "";
            try
            {
                var request =
                    WebRequest.Create(
                        //"https://raw.githubusercontent.com/Chopper1337/CS2-Server-Browser/master/message");
                        uri);
                var response = request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                message = reader.ReadToEnd();
                reader.Close();
                response.Close();
            }
            catch (Exception)
            {
                // ignored
            }
            return message;
        }

        // Find the location of the IP
        private string FindLocation(string ip)
        {
            StatusMessageTextBlock.Text = $"Locating {ip}...";
            string location = "Unknown";
            try
            {
                var request = WebRequest.Create("http://ip-api.com/json/" + ip);
                var response = request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                var json = JObject.Parse(responseFromServer);
                location = json["country"].ToString();
                reader.Close();
                response.Close();
            }
            catch (Exception)
            {
                // ignored
            }
            return location;
        }

        // Save settings when the window is closed
        private void SettingChanged(object sender, RoutedEventArgs e)
        {
            StatusMessageTextBlock.Text = $"Saving settings...";
            if (!ready) return;
            CS2_Server_Browser.Properties.Settings.Default.Priority = PriorityComboBox.SelectedValue.ToString();
            CS2_Server_Browser.Properties.Settings.Default.Language = LanguageComboBox.SelectedValue.ToString();
            CS2_Server_Browser.Properties.Settings.Default.Frequency = FreqComboBox.SelectedValue.ToString();
            CS2_Server_Browser.Properties.Settings.Default.Threads = ThreadsComboBox.SelectedValue.ToString();
            CS2_Server_Browser.Properties.Settings.Default.Save();
        }
    }
}
