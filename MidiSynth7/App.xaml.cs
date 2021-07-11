using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
    }
}
