using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using System.Windows;

namespace DNS_Changer
{
    public partial class MainWindow : Window
    {
        private string dbFile = "db.txt";

        public MainWindow()
        {
            InitializeComponent();

            // بارگذاری DNS ذخیره شده
            if (File.Exists(dbFile))
            {
                var lines = File.ReadAllLines(dbFile);
                if (lines.Length >= 2)
                {
                    preferredDns.Text = lines[0];
                    alternateDns.Text = lines[1];
                }
            }

            // نمایش اسم آداپتر فعال
            string adapterName = GetActiveAdapterName();
            adapterLabel.Content = $"Adapter: {adapterName ?? "Not Found"}";
        }

        private string GetActiveAdapterName()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProps = ni.GetIPProperties();
                    if (ipProps.GatewayAddresses.Count > 0)
                    {
                        return ni.Name;
                    }
                }
            }
            return null;
        }

        private bool IsValidDns(string dns)
        {
            return IPAddress.TryParse(dns, out var ip) &&
                   ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        private void change_btn_Click(object sender, RoutedEventArgs e)
        {
            string dns1 = preferredDns.Text;
            string dns2 = alternateDns.Text;

            if (!IsValidDns(dns1) || !IsValidDns(dns2))
            {
                MessageBox.Show("لطفاً DNS معتبر وارد کنید (مثلاً 8.8.8.8).");
                return;
            }

            File.WriteAllLines(dbFile, new string[] { dns1, dns2 });

            try
            {
                string adapterName = GetActiveAdapterName();
                if (adapterName == null)
                {
                    MessageBox.Show("هیچ آداپتر فعالی پیدا نشد.");
                    return;
                }

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

        private void reset_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string adapterName = GetActiveAdapterName();
                if (adapterName == null)
                {
                    MessageBox.Show("هیچ آداپتر فعالی پیدا نشد.");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set dns name=\"{adapterName}\" dhcp",
                    Verb = "runas",
                    UseShellExecute = true
                });

                MessageBox.Show("DNS به حالت پیش‌فرض (DHCP) ریست شد.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا: " + ex.Message);
            }
        }
    }
}
