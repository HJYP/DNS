using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DNS_Changer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dbFile = "db.txt";

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(dbFile))
            {
                var lines = File.ReadAllLines(dbFile);
                if (lines.Length >= 2)
                {
                    preferredDns.Text = lines[0];
                    alternateDns.Text = lines[1];
                }
            }
        }

        private void change_btn_Click(object sender, RoutedEventArgs e)
        {
            string dns1 = preferredDns.Text;
            string dns2 = alternateDns.Text;

            File.WriteAllLines(dbFile, new string[] { dns1, dns2 });

            try
            {
                string adapterName = "Ethernet";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set dns name=\"{adapterName}\" static {dns1}",
                    Verb = "runas",
                    UseShellExecute = true
                });

                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip add dns name=\"{adapterName}\" {dns2} index=2",
                    Verb = "runas",
                    UseShellExecute = true
                });

                MessageBox.Show("DNS تغییر یافت و ذخیره شد.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا: " + ex.Message);
            }
        }
    }
}
