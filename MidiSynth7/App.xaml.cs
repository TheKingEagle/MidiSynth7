using MidiSynth7.components;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace MidiSynth7
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string APP_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MidiSynth7\\";
        public static string PRESET_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MidiSynth7\\RiffPreset";

        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "MIDISynth7_08282021800";

            _mutex = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application
                Process pthis = Process.GetCurrentProcess();
                Process[] prun = Process.GetProcessesByName(pthis.ProcessName);
                var other = prun.FirstOrDefault(pp => pp.Id != pthis.Id);
                WindowHelper.BringProcessToFront(other);
                Console.WriteLine("instance already running!!");
                Environment.Exit(0x00000480);
            }

            base.OnStartup(e);
        }

        
    }
}
