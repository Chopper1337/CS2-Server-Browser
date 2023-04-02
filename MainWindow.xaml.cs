using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace CS2_Server_Browser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List of servers
        List<Server> servers = new List<Server>();

        public MainWindow()
        {
            InitializeComponent();

            //Make the DataGrid show the servers
            foreach (Server server in servers)
            {
                ServerDataGrid.Items.Add(server);
            }

            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Location", Binding = new Binding("location") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("name") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "IP", Binding = new Binding("ip") });
            ServerDataGrid.Columns.Add(new DataGridTextColumn { Header = "Gamemode", Binding = new Binding("gamemode") });

        }

        // Fill the "servers" list with servers from a file named "servers.txt"
        private void LoadServers(object sender, RoutedEventArgs e)
        {
            StreamReader reader = new StreamReader("servers.txt");
            // For each line in the file, create a new server and add it to the list
            foreach (string line in File.ReadLines("servers.txt"))
            {
                string[] serverData = line.Split(',');
                Server server = new Server();
                server.name = serverData[0];
                server.ip = serverData[1];
                server.gamemode = (Gamemode)Enum.Parse(typeof(Gamemode), serverData[2]);
                server.location = serverData[3];
                servers.Add(server);
            }
        }

        // Check if "servers.txt" exists, if it does, load the servers
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("servers.txt")) return;

            if (new FileInfo("servers.txt").Length != 0)
                LoadServers(sender, e);
        }

        public void StartGame(string threads, string freq, string lang, string priority, string ip)
        {
            string arguments = "-insecure -threads " + threads + " -freq " + freq +
                        " -nojoy -belowaverage -fullscreen -limitvsconst -forcenovsync -softparticlesdefaultoff -console -language " +
                        lang + " -novid -" + priority + " +connect " + ip;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            if(!(ServerDataGrid.SelectedItem is Server selectedServer)) return;
            string ip = selectedServer.ip;
            string threads = ThreadsComboBox.SelectedValue.ToString().Split(':')[0];
            string freq = FreqComboBox.SelectedValue.ToString().Split(':')[0];
            string lang = LanguageComboBox.SelectedValue.ToString().Split(':')[0];
            string priority = PriorityComboBox.SelectedValue.ToString().Split(':')[0];
            StartGame(threads, freq, lang, priority, ip);
            
            MessageBox.Show("Connecting to " + selectedServer.ip);
        }
    }
}
