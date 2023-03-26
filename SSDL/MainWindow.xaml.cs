using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using LibreHardwareMonitor.Hardware;
using System.Linq;
using System.Timers;
using Microsoft.Win32;
using System.IO;

namespace SSDL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public PerformanceCounter vramCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", "pid_4_luid_0x00000000_0x000127AC_phys_0_eng_0_engtype_VideoEncode"); 
        public bool isActive = false;
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                textBox_path.Text = Environment.GetEnvironmentVariable("SSDL_PATH", EnvironmentVariableTarget.Machine);
            }
            catch
            {
                MessageBox.Show("Installation Path not set.", "SSDL");
            }

           

            Computer computer = new Computer();
            computer.Open();
            computer.IsGpuEnabled = true;

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += (s, e) =>
            {
                var gpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia) ?? computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd);
                if (gpu != null)
                {
                    gpu.Update();
                    foreach (var sensor in gpu.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                        {
                            string text = $"GPU Usage: {sensor.Value}%";
                            label3.Dispatcher.Invoke(() => label3.Content = text);
                            break;
                        }
                    }
                }
            };
            timer.Start();
        }

        private void Browse_clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Batch script (*.bat)|*.bat|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string fileName = dialog.FileName;
                textBox_path.Text = dialog.FileName;
                // Use the selected file name here
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            // Set the name of the environment variable
            string variableName = "SSDL_PATH";

            // Store the string in the environment variable
            string stringToStore = textBox_path.Text;
            Environment.SetEnvironmentVariable(variableName, stringToStore, EnvironmentVariableTarget.Machine);

            // Read the string from the environment variable
            //string storedString = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);

            label2.Content = "Done!";


        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo ProcessInfo;
            if (isActive)
            {
                foreach (var process in Process.GetProcessesByName("cmd"))
                {
                    process.Kill();
                }
                foreach (var process in Process.GetProcessesByName("Python"))
                {
                    process.Kill();
                }
                button1.Content = "START";
                isActive = false;
            }
            else
            {
                Process process = new Process();
                process.StartInfo.FileName = Environment.GetEnvironmentVariable("SSDL_PATH", EnvironmentVariableTarget.Machine);
                //process.StartInfo.Arguments = "-noSplash -noFilePatching -showScriptErrors \"-name=Meta\" \"-mod=I:/Steam/steamapps/common/Arma 2;expansion;expansion/beta;expansion/beta/expansion;servermods/@HC_DAYZ;servermods/@HC_WEAPONS;servermods/@HC_EXTRAS;servermods/@HC_ACE\"";
                process.StartInfo.WorkingDirectory = Environment.GetEnvironmentVariable("SSDL_PATH", EnvironmentVariableTarget.Machine).Replace("webui-user.bat", "");
                //MessageBox.Show(Environment.GetEnvironmentVariable("SSDL_PATH", EnvironmentVariableTarget.Machine).Replace("webui-user.bat", ""));
                process.Start();
                button1.Content = "STOP";
                isActive = true;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string[] arrLine = File.ReadAllLines(Environment.GetEnvironmentVariable("SSDL_PATH", EnvironmentVariableTarget.Machine));
            arrLine[6 - 1] = "set COMMANDLINE_ARGS =" + textBox_args.Text;
            File.WriteAllLines(Environment.GetEnvironmentVariable("SSDL_PATH", EnvironmentVariableTarget.Machine), arrLine);
            label2_Copy.Content = "Done!";
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string fileName = dialog.FileName;
                string fileContents = File.ReadAllText(fileName);
                textBox.Text = fileContents;
            }
        }
    }
}
