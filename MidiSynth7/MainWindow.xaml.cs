using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MidiSynth7.components;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.ComponentModel;
using Sanford.Multimedia.Midi;
using MidiSynth7.entities.controls;

namespace MidiSynth7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declarations

        private bool _Closing = false;
        private bool _Minimized = false;
        private int appinfo_projectRevision = 0;
        private bool _loadingView = false;
        DisplayModes _switchto = DisplayModes.Standard;
        UIElement _elementFromanim = null;
        private ISynthView currentView;
        public List<NumberedEntry> OutputDevices = new List<NumberedEntry>();
        public List<NumberedEntry> InputDevices = new List<NumberedEntry>();
        public List<Ellipse> channelElipses = new List<Ellipse>();
        public List<ChInvk> channelIndicators = new List<ChInvk>();
        public InstrumentDefinition InstrumentDefinition { get; private set; }
        public SystemConfig AppConfig;

        public MidiEngine MidiEngine;
        private BackgroundWorker midiTaskWorker;
        private bool mthdinit;//todo: remove?

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            #region Generate presets
            if (!Directory.Exists(App.PRESET_DIR))
            {
                Directory.CreateDirectory(App.PRESET_DIR);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset1.mid"))
            {
                fs.Write(Properties.Resources.preset1, 0, Properties.Resources.preset1.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset2.mid"))
            {
                fs.Write(Properties.Resources.preset2, 0, Properties.Resources.preset2.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset3.mid"))
            {
                fs.Write(Properties.Resources.preset3, 0, Properties.Resources.preset3.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset4.mid"))
            {
                fs.Write(Properties.Resources.preset4, 0, Properties.Resources.preset4.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset5.mid"))
            {
                fs.Write(Properties.Resources.preset5, 0, Properties.Resources.preset5.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset6.mid"))
            {
                fs.Write(Properties.Resources.preset6, 0, Properties.Resources.preset6.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset7.mid"))
            {
                fs.Write(Properties.Resources.preset7, 0, Properties.Resources.preset7.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset8.mid"))
            {
                fs.Write(Properties.Resources.preset8, 0, Properties.Resources.preset8.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset9.mid"))
            {
                fs.Write(Properties.Resources.preset9, 0, Properties.Resources.preset9.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset10.mid"))
            {
                fs.Write(Properties.Resources.preset10, 0, Properties.Resources.preset10.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset11.mid"))
            {
                fs.Write(Properties.Resources.preset11, 0, Properties.Resources.preset11.Length);
            }
            using (FileStream fs = File.Create(App.PRESET_DIR + "\\preset12.mid"))
            {
                fs.Write(Properties.Resources.preset12, 0, Properties.Resources.preset12.Length);
            }
            #endregion

            appinfo_projectRevision = Assembly.GetExecutingAssembly().GetName().Version.Revision;
            AppConfig = LoadConfig();
            for (int i = 0; i < 9; i++)
            {
                CheckBox cbparam = WP_AllowedParams.Children[i] as CheckBox;
                cbparam.IsChecked = AppConfig.InDeviceAllowedParams[i];
            }
            if (!string.IsNullOrWhiteSpace(AppConfig.InstrumentDefinitionPath))
            {
                if (!File.Exists(AppConfig.InstrumentDefinitionPath))
                {
                    InstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();
                    MessageBox.Show("The specified file could not be found. The default definition will be used instead.", "Missing Instrument Definition", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(AppConfig.InstrumentDefinitionPath))
                    {
                        InstrumentDefinition = JsonConvert.DeserializeObject<InstrumentDefinition>(sr.ReadToEnd());
                        Console.WriteLine("InsDef: File loaded.");
                        if (InstrumentDefinition?.Banks.Count == 0)
                        {
                            InstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();
                            MessageBox.Show("The selected file has no instrument list. The default definition will be used instead.", "No Instruments!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                    }
                }
            }
            else
            {
                Console.WriteLine("InsDef: Not configured.");
                InstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();
            }

            Loadview(AppConfig.DisplayMode);
            
        }

        private void Loadview(DisplayModes mode)
        {
            switch (mode)
            {
                case DisplayModes.Standard:
                    this.Width = 1106;
                    this.Height = 590;
                    Title = $"RMSoftware MIDI Synthesizer v7.0 • Standard Edition • {(Environment.Is64BitProcess ? "x64" : "x86")} rev. {appinfo_projectRevision}";
                    currentView = new components.views.StandardView(this, ref AppConfig, ref MidiEngine);
                    FR_SynthView.Content = currentView;
                    break;
                case DisplayModes.Studio:
                    this.Width = 1524;
                    this.Height = 652;
                    Title = $"RMSoftware MIDI Synthesizer v7.0 • Studio Edition • {(Environment.Is64BitProcess ? "x64" : "x86")} rev. {appinfo_projectRevision}";
                    
                    FR_SynthView.Content = "//TODO: Studio view";//TODO: Pass parameters and other important system info

                    break;
                case DisplayModes.Compact:
                    //todo: Compact mode
                    break;
                default:
                    break;
            }

            MainWindow.PostitionWindowOnScreen(this);
        }

        private void SwitchView (DisplayModes mode)
        {
            //hide the window for a minute

            _loadingView = true;
            _Minimized = false;
            _switchto = mode;
            FadeUI(1, 0, this);
            ScaleUI(1, 0.8,this);

            
        }

        ~MainWindow()
        {
            Console.WriteLine("Saving config.");
            SaveConfig();
        }

        #region External API
        public const int WM_CLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hw, int message, int wp, int lp);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public static void PostitionWindowOnScreen(Window window, double horizontalShift = 0, double verticalShift = 0)
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
            window.Left = screen.WorkingArea.X + ((screen.WorkingArea.Width - window.ActualWidth) / 2) + horizontalShift;
            window.Top = screen.WorkingArea.Y + ((screen.WorkingArea.Height - window.ActualHeight) / 2) + verticalShift;
        }
        #endregion

        private void GR_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SendMessage(new WindowInteropHelper(this).Handle, WM_CLBUTTONDOWN, HT_CAPTION, 0);
            e.Handled = false;
        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Closing = true;
                FadeUI(1, 0, this);
                ScaleUI(1, 0.8,this);
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Bn_minimize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //WindowState = System.Windows.WindowState.Minimized;
                _Closing = false;
                _Minimized = true;
                FadeUI(1, 0, this);
                ScaleUI(1, 0.8,this);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Bn_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (GR_OverlayContent.Visibility == Visibility.Visible) return;
            FadeUI(0, 1, GR_OverlayContent);
            ScaleUI(0.8, 1, BDR_SettingsFrame);
        }

        private void Bn_Maximize_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIO_bn_about_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Initialized(object sender, EventArgs e)
        {

            int midiInIndex = 0;
            FadeUI(1, 0, this);
            foreach (string item in MidiEngine.GetInputDevices())
            {
                InputDevices.Add(new NumberedEntry(midiInIndex, item));
                midiInIndex++;
            }
            foreach (NumberedEntry item in InputDevices)
            {
                cm_InputDevices.Items.Add(item);
                cm_InputDevices2.Items.Add(item);
            }

            
            

        }

        private void FadeUI(double from, double to, UIElement uielm)
        {
            _elementFromanim = uielm;
            if(uielm.Visibility != Visibility.Visible)
            {
                uielm.Opacity = 0;
                uielm.Visibility = Visibility.Visible;
            }
            Storyboard WindowStoryboard = new Storyboard();
            DoubleAnimation WindowDoubleAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(120)), FillBehavior.HoldEnd);
            WindowDoubleAnimation.AutoReverse = false;
            QuadraticEase qe = new QuadraticEase();
            qe.EasingMode = EasingMode.EaseInOut;
            WindowDoubleAnimation.EasingFunction = qe;
            Storyboard.SetTargetProperty(WindowStoryboard, new PropertyPath(OpacityProperty));
            Storyboard.SetTarget(WindowDoubleAnimation, uielm);
            WindowStoryboard.Children.Add(WindowDoubleAnimation);
            WindowStoryboard.Completed += WindowStoryboard_Completed;
            WindowStoryboard.Begin((FrameworkElement)uielm, HandoffBehavior.Compose);
        }

        private void WindowStoryboard_Completed(object sender, EventArgs e)
        {
            if(_elementFromanim.Opacity <= 0.9 && _elementFromanim != this)
            {
                _elementFromanim.Visibility = Visibility.Collapsed;
            }
            if (_Closing)
            {
                this.Close();
            }
            if (_Minimized)
            {
                _Minimized = false;
                this.WindowState = System.Windows.WindowState.Minimized;
                ScaleUI(1, 1,this);
                this.Opacity = 1;

            }
            if(_loadingView)
            {
                
                _Minimized = false;
                _loadingView = false;
                Loadview(_switchto);
                FadeUI(0, 1, this);
                ScaleUI(0.8, 1,this);

            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            ScaleUI(0.8, 1,this);
            FadeUI(0, 1, this);
        }

        private void ScaleUI(double from, double to, UIElement uielm)
        {
            ScaleTransform trans = new ScaleTransform();
            uielm.RenderTransform = trans;
            uielm.RenderTransformOrigin = new Point(0.5d, 0.5d);
            
            if(from != to)
            {
                DoubleAnimation scaler = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(120),FillBehavior.HoldEnd);
                scaler.AutoReverse = false;

                CubicEase ease = new CubicEase();
                ease.EasingMode = EasingMode.EaseInOut;
                scaler.EasingFunction = ease;
                trans.BeginAnimation(ScaleTransform.ScaleXProperty, scaler);
                trans.BeginAnimation(ScaleTransform.ScaleYProperty, scaler);
            }
            
        }


        private void window_StateChanged(object sender, EventArgs e)
        {

            if(window.WindowState != WindowState.Minimized)
            {
                _Minimized = false;
                FadeUI(0, 1, this);
                ScaleUI(0.8, 1,this);
            }
        }

        public void SaveConfig()
        {
            if (!Directory.Exists(App.APP_DATA_DIR)) Directory.CreateDirectory(App.APP_DATA_DIR);
            if(AppConfig == null)
            {
                MessageBox.Show("Configuration error: null");
                return;
            }
            if(File.Exists(App.APP_DATA_DIR + "synth7.config"))
            {
                File.Delete(App.APP_DATA_DIR + "synth7.config");
            }
            
            using (StreamWriter sw = new StreamWriter(App.APP_DATA_DIR + "synth7.config"))
            {
                sw.WriteLine(JsonConvert.SerializeObject(AppConfig,Formatting.Indented));
                sw.Flush();
            }
        }

        public void SaveConfigAs(SystemConfig cfg, string filename)
        {
            if (!Directory.Exists(App.APP_DATA_DIR)) Directory.CreateDirectory(App.APP_DATA_DIR);
            if (cfg == null)
            {
                MessageBox.Show("Configuration error: null");
                return;
            }
            if (File.Exists(App.APP_DATA_DIR + filename))
            {
                File.Delete(App.APP_DATA_DIR + filename);
            }

            using (StreamWriter sw = new StreamWriter(App.APP_DATA_DIR + filename))
            {
                sw.WriteLine(JsonConvert.SerializeObject(cfg, Formatting.Indented));
                sw.Flush();
            }
        }

        public class ChInvk
        {
            Ellipse index;
            MainWindow wndw;
            public int counterval { get; set; }
            public ChInvk(Ellipse indx, MainWindow wnd)
            {
                index = indx;
                System.Timers.Timer t = new System.Timers.Timer(10);
                t.Elapsed += t_Elapsed;
                counterval = 0;
                wndw = wnd;
                t.AutoReset = true;
                t.Start();
            }
            public void CounterReset()
            {
                counterval = 0;
            }
            private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    Action invoker = delegate ()
                    {
                        counterval += 15;
                        if (counterval > 160)
                        {
                            counterval = 0;
                            index.Fill = (Brush)wndw.FindResource("CH_Ind_off");
                        }
                    };
                    wndw.Dispatcher.Invoke(invoker);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("gracefully continue... but: "+ex.Message);
                }
                
            }
        }

        public void GenerateMIDIEngine(int deviceId=0)
        {
            midiTaskWorker = new BackgroundWorker();
            midiTaskWorker.WorkerSupportsCancellation = false;
            midiTaskWorker.DoWork += MidiTaskWorker_DoWork;
            midiTaskWorker.RunWorkerCompleted += MidiTaskWorker_RunWorkerCompleted;
            midiTaskWorker.RunWorkerAsync(deviceId);
        }

        private void MidiTaskWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
            }
            //set in device
            if (AppConfig.ActiveInputDeviceIndex < cm_InputDevices.Items.Count)
            {
                cm_InputDevices.SelectedIndex = AppConfig.ActiveInputDeviceIndex;
            }
            if (AppConfig.ActiveInputDevice2Index < cm_InputDevices2.Items.Count)
            {
                cm_InputDevices2.SelectedIndex = AppConfig.ActiveInputDevice2Index;
            }
            //tell view we updated shit
            currentView.HandleEvent(this, new EventArgs(), "RefMainWin");
            currentView.HandleEvent(sender, new EventArgs(), "MTaskWorker");
        }

        private void MidiTaskWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (MidiEngine != null)
                {
                    MidiEngine.MidiEngine_Close();
                }
                int d = 0;
                void Invoker()
                {
                    d = (int)e.Argument;
                }
                Dispatcher.Invoke(Invoker);
                MidiEngine = new MidiEngine(d);
                MidiEngine.NotePlayed += MidiEngine_NotePlayed;
                MidiEngine.NoteStopped += MidiEngine_NoteStoped;
                MidiEngine.FileLoadComplete += MidiEngine_FileLoadComplete;
                MidiEngine.SequenceBuilder_Completed += MidiEngine_SequenceBuilder_Completed;

                mthdinit = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a MIDI Engine:\r\n" + ex.Message, "MIDI Engine", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void MidiEngine_SequenceBuilder_Completed(object sender, EventArgs e)
        {
            currentView.HandleEvent(sender,e,"MidiEngine_SequenceBuilder_Completed");
        }

        private void MidiEngine_FileLoadComplete(object sender, EventArgs e)
        {
            currentView.HandleEvent(sender, e, "MidiEngine_FileLoadComplete");

        }

        private void MidiEngine_NoteStoped(object sender, NoteEventArgs e)
        {
            currentView.HandleNoteOffEvent(sender, e);
        }

        private void MidiEngine_NotePlayed(object sender, NoteEventArgs e)
        {
            currentView.HandleNoteOnEvent(sender, e);
        }

        public SystemConfig LoadConfig()
        {
            if(!File.Exists(App.APP_DATA_DIR+"synth7.config"))
            {
                Console.WriteLine("CONFIG: config file didn't exist.");
                return new SystemConfig(DisplayModes.Standard);
            }
            else
            {
                string json = "";
                using (StreamReader sr = new StreamReader(App.APP_DATA_DIR + "synth7.config"))
                {
                    json = sr.ReadToEnd();
                }
                SystemConfig cfg = JsonConvert.DeserializeObject<SystemConfig>(json);
                if (cfg == null)
                {
                    Console.WriteLine("CONFIG: Json was invalid, null entity returned.");
                    return new SystemConfig(DisplayModes.Standard);

                }

                else
                {
                    if (cfg.CheckForMissingValues() || cfg.CheckForInvalidCounts())
                    {
                        string filename = "config.old";
                        Console.WriteLine("CONFIG: json produced incompatible data");
                        MessageBox.Show("Notice: Your settings are incompatible with this version of MidiSynth. \r\n\r\n" +
                            $"Previous config saved as '{App.APP_DATA_DIR + filename}'\r\n\r\n A new one will be created.", "Incompatible Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);

                        SaveConfigAs(cfg, filename);
                        SystemConfig newcfg = new SystemConfig(DisplayModes.Standard);

                        SaveConfigAs(newcfg, "synth7.config");
                        return newcfg;
                    }
                    Console.WriteLine("CONFIG: successfully loaded saved config.");
                    return cfg;
                }
            }
        }

        private void window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (MidiEngine.inDevice != null)
            {
                MidiEngine.inDevice.StopRecording();
                MidiEngine.inDevice.Close();
            }
            if (MidiEngine.inDevice2 != null)
            {
                MidiEngine.inDevice2.StopRecording();
                MidiEngine.inDevice2.Close();
            }
            MidiEngine.MidiEngine_Close();

            //It is actually required, apparently... It prevents a crash on exit due to TaskCancleException.
            // close all active threads
            Environment.Exit(0);
        }

        private void window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            currentView.HandleKeyDown(sender, e);
        }

        private void window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            currentView.HandleKeyUp(sender, e);


        }

        private void CM_InputDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //for program start
            if (MidiEngine.inDevice != null)
            {
                MidiEngine.inDevice.StopRecording();
                MidiEngine.inDevice.Close();
            }
            if (cm_InputDevices.SelectedIndex > -1)
            {
                MidiEngine.inDevice = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices.SelectedItem).Index);
                MidiEngine.inDevice.ChannelMessageReceived += inDevice_ChannelMessageReceived;
                MidiEngine.inDevice.StartRecording();
            }

            if (MidiEngine.inDevice2 != null)
            {
                MidiEngine.inDevice2.StopRecording();
                MidiEngine.inDevice2.Close();
            }
            if (cm_InputDevices2.SelectedIndex > -1)
            {
                MidiEngine.inDevice2 = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices2.SelectedItem).Index);
                MidiEngine.inDevice2.ChannelMessageReceived += inDevice_ChannelMessageReceived;
                MidiEngine.inDevice2.StartRecording();
            }
        }

        private void inDevice_ChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            if (e.Message.Command == Sanford.Multimedia.Midi.ChannelCommand.Controller)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == Sanford.Multimedia.Midi.ChannelCommand.PitchWheel)
            {
                //MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                //TODO: Add settings to toggle some controls off
            }
            if (e.Message.Command == Sanford.Multimedia.Midi.ChannelCommand.PolyPressure)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == Sanford.Multimedia.Midi.ChannelCommand.ChannelPressure)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == ChannelCommand.ProgramChange)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == ChannelCommand.NoteOn)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                currentView.HandleNoteOnEvent(this, new NoteEventArgs(e.Message));
               
                //currentView.HandleNoteOn_VS_Event(this, new PKeyEventArgs(e.Message.Data1), e.Message.Data2);
            }
            if (e.Message.Command == ChannelCommand.NoteOff || (e.Message.Command == ChannelCommand.NoteOn && e.Message.Data2 == 0))
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                currentView.HandleNoteOffEvent(this,new NoteEventArgs(e.Message));
            }
        }

        private void bn_cfgSave_Click(object sender, RoutedEventArgs e)
        {
            if (MidiEngine.inDevice != null)
            {
                MidiEngine.inDevice.StopRecording();
                MidiEngine.inDevice.Close();
            }
            if (cm_InputDevices.SelectedIndex > -1)
            {
                MidiEngine.inDevice = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices.SelectedItem).Index);
                MidiEngine.inDevice.ChannelMessageReceived += inDevice_ChannelMessageReceived;
                MidiEngine.inDevice.StartRecording();
            }

            if (MidiEngine.inDevice2 != null)
            {
                MidiEngine.inDevice2.StopRecording();
                MidiEngine.inDevice2.Close();
            }
            
            if (cm_InputDevices2.SelectedIndex > -1)
            {
                MidiEngine.inDevice2 = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices2.SelectedItem).Index);
                MidiEngine.inDevice2.ChannelMessageReceived += inDevice_ChannelMessageReceived;
                MidiEngine.inDevice2.StartRecording();
            }

            AppConfig.ActiveInputDeviceIndex = cm_InputDevices.SelectedIndex;
            AppConfig.ActiveInputDevice2Index = cm_InputDevices2.SelectedIndex;

            for (int i = 0; i < 9; i++)
            {
                CheckBox cbcfg = WP_AllowedParams.Children[i] as CheckBox;
                AppConfig.InDeviceAllowedParams[i] = cbcfg.IsChecked.Value;
            }
            SaveConfig();
            currentView.HandleEvent(this, new EventArgs(), "RefMainWin");
            currentView.HandleEvent(sender, new EventArgs(), "RefAppConfig");
            ScaleUI(1, 0.8, BDR_SettingsFrame);
            FadeUI(1, 0, GR_OverlayContent);

        }
    }
}
