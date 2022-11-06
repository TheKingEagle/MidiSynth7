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
using System.Collections.ObjectModel;
using MidiSynth7.components.dialog;

namespace MidiSynth7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declarations

        private bool _Closing = false;
        private bool PatternLoaded = false;
        private bool _Minimized = false;
        private int appinfo_projectRevision = 0;
        private int _width, _height = 0;
        private double scale = 1;
        private bool _loadingView = false;
        private UIElement _elementFromanim = null;
        internal DisplayModes _switchto = DisplayModes.Standard;
        internal ISynthView currentView;
        internal DisplayModes CurrentViewDM;
        

        public bool SynHelpRequested { get; private set; }
        public InstrumentDefinition ActiveInstrumentDefinition { get; set; }
        public NFXDelayProfile ActiveNFXProfile { get; set; }
        public List<InstrumentDefinition> Definitions { get; private set; }
        public SystemConfig AppConfig;
        public MidiEngine MidiEngine;
        public List<NumberedEntry> OutputDevices = new List<NumberedEntry>();
        public List<NumberedEntry> InputDevices = new List<NumberedEntry>();
        public List<Ellipse> channelElipses = new List<Ellipse>();
        public List<ChInvk> channelIndicators = new List<ChInvk>();
        public List<NFXDelayProfile> NFXProfiles = new List<NFXDelayProfile>();

        public List<TrackerSequence> Tracks = new List<TrackerSequence>();

        public TrackerSequence ActiveSequence { get; set; }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(App.APP_DATA_DIR + "sequences\\"))
            {
                Directory.CreateDirectory(App.APP_DATA_DIR + "sequences\\");
            }

            PopulateSequences();

            appinfo_projectRevision = Assembly.GetExecutingAssembly().GetName().Version.Revision;
            AppConfig = LoadConfig();

            if (!string.IsNullOrWhiteSpace(AppConfig.InstrumentDefinitionPath))
            {
                if (!File.Exists(AppConfig.InstrumentDefinitionPath))
                {
                    Definitions = new List<InstrumentDefinition>();
                    Definitions.Add(InstrumentDefinition.GetDefaultDefinition());
                    ActiveInstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();//set active

                    Dialog.Message(this, GR_OverlayContent,
                "The instrument Definition file failed to load. It was not found. Using the default one instead",
                "Instrument Definition Not Found", Icons.Warning);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(AppConfig.InstrumentDefinitionPath))
                    {
                        Definitions = JsonConvert.DeserializeObject<List<InstrumentDefinition>>(sr.ReadToEnd());
                        Console.WriteLine("InsDef: File loaded.");
                        if (Definitions.Count == 0)
                        {
                            Definitions = new List<InstrumentDefinition>();
                            Definitions.Add(InstrumentDefinition.GetDefaultDefinition());
                            ActiveInstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();//set active
                            Dialog.Message(this, GR_OverlayContent, "The instrument Definition file is invalid. Using the default one instead", "No Instruments", Icons.Warning);
                        }

                    }
                }
            }
            else
            {
                Console.WriteLine("InsDef: Not configured.");
                Definitions = new List<InstrumentDefinition>();
                Definitions.Add(InstrumentDefinition.GetDefaultDefinition());
                ActiveInstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();//set active
            }

            if (!File.Exists(App.APP_DATA_DIR + "profiles.nfx"))
            {
                NFXProfiles = new List<NFXDelayProfile>();
                NFXDelayProfile df = new NFXDelayProfile()
                {
                    Delay = 280,
                    ProfileName = "Default",
                    OffsetMap = new List<(int pitch, int decay)>() { (0, 0), (0, 0) }
                };
                NFXProfiles.Add(df);
            }
            else
            {
                using (StreamReader sr = new StreamReader(App.APP_DATA_DIR + "profiles.nfx"))
                {
                    NFXProfiles = JsonConvert.DeserializeObject<List<NFXDelayProfile>>(sr.ReadToEnd());
                    Console.WriteLine("NFXProfiles: File loaded.");
                    if (NFXProfiles.Count == 0)
                    {
                        NFXDelayProfile df = new NFXDelayProfile()
                        {
                            Delay = 280,
                            ProfileName = "Default",
                            OffsetMap = { (0, 0), (0, 0) }
                        };
                        NFXProfiles.Add(df);
                        Dialog.Message(this, GR_OverlayContent,
                "NFX Delay file invalid. Using default profile instead",
                "NFX Delay Profile", Icons.Warning);
                    }

                }
            }

            GR_OverlayContent.Visibility = Visibility.Collapsed;
            GR_OverlayContent.Opacity = 0;
            Loadview(AppConfig.DisplayMode);
        }

        internal void PopulateSequences()
        {
            if(Tracks != null)
            {
                Tracks.Clear();
            } 
            else
            {
                Tracks = new List<TrackerSequence>();
            }
            foreach (string item in Directory.GetFiles(App.APP_DATA_DIR + "sequences\\", "*.mton"))
            {
                using (StreamReader sr = new StreamReader(item))
                {
                    TrackerSequence ts = JsonConvert.DeserializeObject<TrackerSequence>(sr.ReadToEnd());
                    if (ts != null)
                    {
                        Tracks.Add(ts);
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse file: " + item);
                    }
                }
            }
            currentView?.HandleEvent(this, new EventArgs(), "TrSeqUpdate");
        }

        ~MainWindow()
        {
            Console.WriteLine("Saving config.");
            SaveConfig();
        }

        #region Window Interaction
        private void Gr_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowHelper.SendMessage(new WindowInteropHelper(this).Handle, WindowHelper.WM_CLBUTTONDOWN, WindowHelper.HT_CAPTION, 0);
            e.Handled = false;
        }

        private void Bn_Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Closing = true;
                FadeUI(1, 0, this);
                ScaleUI(1, 0.8,this);
                

            }
            catch (Exception ex)
            {
                Dialog.Message(this, GR_OverlayContent,
                ex.Message,
                "RMSoftware MIDISynth 7.0", Icons.Critical);
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
            catch (Exception ex)//wait why is there an exception catch here?
            {
                Dialog.Message(this, GR_OverlayContent,
                ex.Message,
                "RMSoftware MIDISynth 7.0", Icons.Critical);
            }
        }

        private void Bn_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (GR_OverlayContent.Visibility == Visibility.Visible) return;
            Dialog g = new Dialog();
            g.SnapsToDevicePixels = true;
            g.ShowDialog(new Settings(AppConfig,this,g), this, GR_OverlayContent);
        }

        private void Bn_Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Bn_about_Click(object sender, RoutedEventArgs e)
        {
            Dialog.Message(this, GR_OverlayContent,
                "RMSoftware's MIDISynth 7.0 © 2012 - 2022 RMSoftware Development\r\n\r\nThis will be replaced with a more suitable dialog at some point.",
                "RMSoftware MIDI Synthesizer 7.0", Icons.Info);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(GR_OverlayContent.Visibility == Visibility.Visible)
            {
                return;
            }
            currentView.HandleKeyDown(sender, e);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (GR_OverlayContent.Visibility == Visibility.Visible)
            {
                return;
            }
            currentView.HandleKeyUp(sender, e);
        }

        #endregion

        #region Config-View Interaction

        private void CfgHelpRequested_Click(object sender, RoutedEventArgs e)
        {
            

            SynHelpRequested = !SynHelpRequested;
            Cursor = SynHelpRequested ? Cursors.Help : Cursors.Arrow;
        }


        private void Gr_OverlayContent_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;

            //if (SynHelpRequested)
            //{
            //    this.Cursor = Cursors.Arrow;
            //    SynHelpRequested = false;
            //}
        }

        #endregion

       

        #region Window Logic & Animation
        private void Window_Initialized(object sender, EventArgs e)
        {
            int midiInIndex = 0;
            FadeUI(1, 0, this);
            foreach (string item in MidiEngine.GetInputDevices())
            {
                InputDevices.Add(new NumberedEntry(midiInIndex, item));
                midiInIndex++;
            }
            
            int midiOutIndex = 0;
            foreach (string item in MidiEngine.GetOutputDevices())
            {
                OutputDevices.Add(new NumberedEntry(midiOutIndex, item));
                midiOutIndex++;
            }
            
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if(window.WindowState == WindowState.Normal || window.WindowState == WindowState.Maximized)
            {
                _Minimized = false;
                FadeUI(0, 1, this);
                ScaleUI(0.8, 1,this);
            }

            MainWinBdr.Margin = WindowState == WindowState.Maximized ? new Thickness(0) : new Thickness(12);

            canvasMAX.Visibility = WindowState != WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
            canvasRest.Visibility = WindowState == WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScaleUI(0.8, 1,this);
            FadeUI(0, 1, this);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if(MidiEngine != null)
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
            }
            

            // close all active threads
            Environment.Exit(0);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                scale = Math.Min(this.ActualWidth / _width, this.ActualHeight / _height);
                ScaleUI(0.8, 1, FR_SynthView);
            }
            else
            {
                scale = 1;

                ScaleUI(0.8, 1, FR_SynthView);
            }
        }

        private void Loadview(DisplayModes mode)
        {
            CurrentViewDM = mode;
            switch (mode)
            {
                case DisplayModes.Standard:
                    this.Width = 1106;
                    this.Height = 596;
                    _width = 1106;
                    _height = 596;
                    Title = $"RMSoftware MIDI Synthesizer v7.0 • Standard Edition • {(Environment.Is64BitProcess ? "x64" : "x86")} rev. {appinfo_projectRevision}";
                    currentView = null;
                    currentView = new components.views.StandardView(this, ref AppConfig, ref MidiEngine);
                    FR_SynthView.Content = currentView;

                    break;
                case DisplayModes.Studio:
                    this.Width = 1524;
                    this.Height = 658;
                    _width = 1524;
                    _height = 658;
                    Title = $"RMSoftware MIDI Synthesizer v7.0 • Studio Edition • {(Environment.Is64BitProcess ? "x64" : "x86")} rev. {appinfo_projectRevision}";
                    currentView = null;
                    currentView = new components.views.StudioView(this, ref AppConfig, ref MidiEngine);
                    FR_SynthView.Content = currentView;

                    break;
                case DisplayModes.Compact:
                    //todo: Compact mode
                    break;
                default:
                    break;
            }

            WindowHelper.PostitionWindowOnScreen(this);
        }

        internal void SwitchView(DisplayModes mode)
        {
            //hide the window for a minute

            _loadingView = true;
            _Minimized = false;
            _switchto = mode;
            FadeUI(1, 0, this);
            ScaleUI(1, 0.8, this);
        }

        public void FadeUI(double from, double to, UIElement uielm)
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
            Timeline.SetDesiredFrameRate(WindowStoryboard, 60);
            WindowStoryboard.Children.Add(WindowDoubleAnimation);
            WindowStoryboard.Completed += WindowStoryboard_Completed;
            WindowDoubleAnimation.Freeze();
            WindowStoryboard.Begin((FrameworkElement)uielm, HandoffBehavior.Compose);
        }

        public void ScaleUI(double from, double to, UIElement uielm, double originX = 0.5d, double originY = 0.5d)
        {
            //ensure element is visible.
            
            if(from <= to)
            {
                if (uielm != this)
                {
                uielm.Visibility = Visibility.Visible;
                }
            }
            double _scale = (uielm != this && uielm.GetType() != typeof(Dialog)) ? scale : 1; //exclude scaling to some elements (thanks to WPF blurring :))
            ScaleTransform trans = new ScaleTransform();
            uielm.RenderTransform = trans;
            uielm.RenderTransformOrigin = new Point(originX,originY);
            
            if(from != to)
            {
                DoubleAnimation scaler = new DoubleAnimation(from * _scale, to * _scale, TimeSpan.FromMilliseconds(120),FillBehavior.HoldEnd);
                scaler.Completed += (object s, EventArgs e)=> {
                    if (from > to)
                    {
                        if (uielm == this) return;
                        uielm.Visibility = Visibility.Collapsed;
                        
                    }
                    if (uielm.GetType() == typeof(Dialog))
                    {
                        ((Dialog)uielm).OnDialogAnimationComplete(GR_OverlayContent);
                    }
                };
                scaler.AutoReverse = false;
                CubicEase ease = new CubicEase();
                ease.EasingMode = EasingMode.EaseInOut;
                scaler.EasingFunction = ease;
                Timeline.SetDesiredFrameRate(scaler, 60);
                scaler.Freeze();
                trans.BeginAnimation(ScaleTransform.ScaleXProperty, scaler);
                trans.BeginAnimation(ScaleTransform.ScaleYProperty, scaler);
            }
            
        }

        private void WindowStoryboard_Completed(object sender, EventArgs e)
        {
            if(_elementFromanim.Opacity <= 0.2 && _elementFromanim != this)
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

        #endregion

        #region Application Configuration Logic
        public void SaveConfig()
        {
            if (!Directory.Exists(App.APP_DATA_DIR)) Directory.CreateDirectory(App.APP_DATA_DIR);
            if(AppConfig == null)
            {
                Dialog.Message(this,GR_OverlayContent,
                    "The application configuration could not be saved because it's null.",
                    "Config Error",Icons.Critical);
                return;
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
                Dialog.Message(this, GR_OverlayContent,
                    "The application configuration could not be saved because it's null.",
                    "Config Error", Icons.Critical);
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

        public string SaveNFXProfiles()
        {
            string path = App.APP_DATA_DIR + "profiles.nfx";
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(NFXProfiles, Formatting.Indented));
                    Console.WriteLine("Profiles.nfx saved");
                }
            }
            catch (Exception ex)
            {
                Dialog.Message(this, GR_OverlayContent,
                    ex.Message,
                    "Write Failure", Icons.Critical);
            }
            return path;
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
                        Dialog.Message(this, GR_OverlayContent,
                    "Notice: Your settings are incompatible with this version of MidiSynth. \r\n\r\n" +
                            $"Previous config saved as '{App.APP_DATA_DIR + filename}'\r\n\r\n A new one will be created.",
                    "Incompatible Configuration", Icons.Warning);

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

        

        #endregion

        #region MIDI Engine Logic
        public void GenerateMIDIEngine(ISynthView view, int deviceId=0)
        {
            try
            {
                if (MidiEngine != null)
                {
                    MidiEngine.MidiEngine_Close();
                }
                MidiEngine = new MidiEngine(deviceId);
                
                MidiEngine.NotePlayed += MidiEngine_NotePlayed;
                MidiEngine.NoteStopped += MidiEngine_NoteStoped;
                MidiEngine.FileLoadComplete += MidiEngine_FileLoadComplete;
                MidiEngine.SequenceBuilder_Completed += MidiEngine_SequenceBuilder_Completed;

                
                //tell view we updated shit
                view.HandleEvent(this, new EventArgs(), "RefMainWin");
                view.HandleEvent(this, new EventArgs(), "MTaskWorker");
                if (MidiEngine.inDevice != null)
                {
                    MidiEngine.inDevice.StopRecording();
                    MidiEngine.inDevice.Close();
                }
                if (AppConfig.ActiveInputDeviceIndex > -1)
                {
                    MidiEngine.inDevice = new Sanford.Multimedia.Midi.InputDevice(AppConfig.ActiveInputDeviceIndex);
                    MidiEngine.inDevice.PostDriverCallbackToDelegateQueue = false;
                    MidiEngine.inDevice.PostEventsOnCreationContext = false;
                    MidiEngine.inDevice.ChannelMessageReceived += InDevice_ChannelMessageReceived;
                    MidiEngine.inDevice.StartRecording(); 
                }

                if (MidiEngine.inDevice2 != null)
                {
                    MidiEngine.inDevice2.StopRecording();
                    MidiEngine.inDevice2.Close();
                }
                if (AppConfig.ActiveInputDevice2Index > -1)
                {
                    MidiEngine.inDevice2 = new Sanford.Multimedia.Midi.InputDevice(AppConfig.ActiveInputDevice2Index);
                    MidiEngine.inDevice2.PostDriverCallbackToDelegateQueue = false;
                    MidiEngine.inDevice2.PostEventsOnCreationContext = false;
                    MidiEngine.inDevice2.ChannelMessageReceived += InDevice_ChannelMessageReceived;
                    MidiEngine.inDevice2.StartRecording();
                }
            }
            catch (Exception ex)
            {
                Dialog.Message(this, GR_OverlayContent, "Failed to create a MIDI Engine:\r\n" + ex.Message, "MIDI Engine", Icons.Critical);
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
            Dispatcher.InvokeAsync(() => currentView.HandleNoteOffEvent(sender, e));
        }

        private void MidiEngine_NotePlayed(object sender, NoteEventArgs e)
        {
            Dispatcher.InvokeAsync(() => currentView.HandleNoteOnEvent(sender, e));
        }

        private void InDevice_ChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            if (e.Message.Command == ChannelCommand.Controller)
            {
                if (e.Message.Data1 == (int)ControllerType.HoldPedal1)
                {
                    if((sender as Sanford.Multimedia.Midi.InputDevice) == MidiEngine.inDevice && !AppConfig.Input1RelayMode)
                    {
                        Dispatcher.InvokeAsync(()=>currentView.HandleEvent(this, e, e.Message.Data2 >= 63 ? "SynthSustainCTRL_ON" : "SynthSustainCTRL_OFF"));
                        for (int i = 0; i < 16; i++)
                        {
                            //This is probably slow :D
                            MidiEngine.MidiNote_SetControl(ControllerType.HoldPedal1, i, e.Message.Data2);

                        }
                        return;
                    }

                    if ((sender as Sanford.Multimedia.Midi.InputDevice) == MidiEngine.inDevice2 && !AppConfig.Input2RelayMode)
                    {
                        Dispatcher.InvokeAsync(() => currentView.HandleEvent(this, e, e.Message.Data2 >= 63 ? "SynthSustainCTRL_ON" : "SynthSustainCTRL_OFF"));
                        for (int i = 0; i < 16; i++)
                        {
                            //This is probably slow :D
                            MidiEngine.MidiNote_SetControl(ControllerType.HoldPedal1, i, e.Message.Data2);

                        }
                        return;
                    }

                }
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);

            }
            if (e.Message.Command == ChannelCommand.PitchWheel)
            {
                //MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                //TODO: Add settings to toggle some controls off
            }
            if (e.Message.Command == ChannelCommand.PolyPressure)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == ChannelCommand.ChannelPressure)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == ChannelCommand.ProgramChange)
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
            }
            if (e.Message.Command == ChannelCommand.NoteOn)
            {
                //MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                //Dispatcher.InvokeAsync(()=>currentView.HandleNoteOnEvent(this, new NoteEventArgs(e.Message)));
                if(sender as Sanford.Multimedia.Midi.InputDevice == MidiEngine.inDevice && !AppConfig.Input1RelayMode)
                {
                    //TODO: FIXME -- Non-relay mode still bound to ui thread.
                    Dispatcher.InvokeAsync(() => currentView.HandleNoteOn_VS_Event(this, new PKeyEventArgs(e.Message.Data1), e.Message.Data2));
                    return;
                }
                if (sender as Sanford.Multimedia.Midi.InputDevice == MidiEngine.inDevice2 && !AppConfig.Input2RelayMode)
                {
                    //TODO: FIXME -- Non-relay mode still bound to ui thread.
                    Dispatcher.InvokeAsync(() => currentView.HandleNoteOn_VS_Event(this, new PKeyEventArgs(e.Message.Data1), e.Message.Data2));
                    return;
                }
                else
                {
                    MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                    Dispatcher.InvokeAsync(()=>currentView.HandleNoteOnEvent(this, new NoteEventArgs(e.Message)));
                    return;
                }
            }
            if (e.Message.Command == ChannelCommand.NoteOff || (e.Message.Command == ChannelCommand.NoteOn && e.Message.Data2 == 0))
            {
                MidiEngine.MidiEngine_SendRawChannelMessage(e.Message);
                Dispatcher.InvokeAsync(()=>currentView.HandleNoteOffEvent(this,new NoteEventArgs(e.Message)));
            }
        }



        #endregion

        public void ShowNFX()
        {
            if (GR_OverlayContent.Visibility == Visibility.Visible)
            {
                return;
            }
            Dialog g = new Dialog();
            
            g.SnapsToDevicePixels = true;
            g.ShowDialog(new NFXDelay(this, GR_OverlayContent), this, GR_OverlayContent);
        }

        

        public async void ShowMPT()
        {
            PatternLoaded = false;
            if (GR_OverlayContent.Visibility == Visibility.Visible)
            {
                return;
            }
            SequenceEditor editor = new SequenceEditor(this,GR_OverlayContent);
            Dialog g = new Dialog();
            g.DialogShown += G_Shown;
            g.SnapsToDevicePixels = true;
            await g.ShowDialog(editor, this, GR_OverlayContent);
        }
        
        private void G_Shown(object sender, EventArgs e)
        {
            if (PatternLoaded) return;
            var editor = sender as SequenceEditor;
            if (ActiveSequence == null)
            {
                TrackerSequence ts = new TrackerSequence();

                ts.Patterns = new List<TrackerPattern>()
                {
                    TrackerPattern.GetEmptyPattern(32, 20),
                    TrackerPattern.GetEmptyPattern(32, 20),
                    TrackerPattern.GetEmptyPattern(32, 20),
                    TrackerPattern.GetEmptyPattern(32, 20),
                };
                ts.SelectedOctave = 3;
                ts.Instruments = new List<TrackerInstrument>()
                {
                    new TrackerInstrument(0,-1,0,0,"Acoustic Grand"),
                    new TrackerInstrument(1,-1,0,4,"Rhodes E. Piano"),
                    new TrackerInstrument(2,-1,0,18,"Rock Organ"),
                    new TrackerInstrument(3,-1,0,32,"Acoustic Bass"),
                };
                ts.SelectedInstrument = 1;
                ts.SequenceName = "Untitled Sequence" + (Tracks.Count + 1);
                ActiveSequence = ts;
                editor.LoadSequence(ts);
                editor.LoadPattern(0);

            }
            else
            {
                editor.LoadSequence(ActiveSequence);
                editor.LoadPattern(0);
            }
            PatternLoaded = true;
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
                            index.Fill = (Brush)wndw.FindResource("CH_IND_OFF");
                        }
                    };
                    wndw.Dispatcher.InvokeAsync(invoker);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("gracefully continue... but: "+ex.Message);
                }

            }
        }

    }
}
