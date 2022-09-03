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
        private int _width, _height = 0;
        private double scale = 1;
        private bool _loadingView = false;
        private bool _insDefNameDirty = false;
        private bool _NFXProfileNameDirty = false;

        private NFXDelayProfile _backupProfile { get; set; }

        private UIElement _elementFromanim = null;
        private DisplayModes _switchto = DisplayModes.Standard;
        private ISynthView currentView;
        private DisplayModes CurrentViewDM;
        private List<(string name,bool value)> checkstates = new List<(string name, bool value)>();
        

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
            CFGCB_SynthRelay1.IsChecked = AppConfig.Input1RelayMode;
            CFGCB_SynthRelay2.IsChecked = AppConfig.Input2RelayMode;

            switch (AppConfig.DisplayMode)
            {
                case DisplayModes.Standard:
                    rb_syncfg_Standard.IsChecked = true;
                    break;
                case DisplayModes.Studio:
                    rb_syncfg_Extended.IsChecked = true;

                    break;
                case DisplayModes.Compact:
                    rb_syncfg_Micro.IsChecked = true;

                    break;
                default:
                    rb_syncfg_Standard.IsChecked = true;
                    break;
            }
            for (int i = 0; i < 9; i++)
            {
                CheckBox cbparam = WP_AllowedParams.Children[i] as CheckBox;
                cbparam.IsChecked = AppConfig.InDeviceAllowedParams[i];
            }
            if (!string.IsNullOrWhiteSpace(AppConfig.InstrumentDefinitionPath))
            {
                if (!File.Exists(AppConfig.InstrumentDefinitionPath))
                {
                    Definitions = new List<InstrumentDefinition>();
                    Definitions.Add(InstrumentDefinition.GetDefaultDefinition());
                    ActiveInstrumentDefinition = InstrumentDefinition.GetDefaultDefinition();//set active
                    MessageBox.Show("The definition file was not found. The default definition will be used instead.", "Missing Instrument Definition", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                            MessageBox.Show("The definition file was invalid. The default definition will be used instead.", "No Instruments!", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        MessageBox.Show("The NFX file was invalid. The default one will be used instead.", "NFX Profile corrupt!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }
            }

            GR_OverlayContent.Visibility = Visibility.Collapsed;
            GR_OverlayContent.Opacity = 0;
            BDR_SettingsFrame.Visibility = Visibility.Collapsed;
            BDR_NFXDelayCustomizationFrame.Visibility = Visibility.Collapsed;
            BDR_InstrumentDefinitionsFrame.Visibility = Visibility.Collapsed;
            Loadview(AppConfig.DisplayMode);
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
            if (GR_OverlayContent.Opacity == 1 && GR_OverlayContent.Visibility == Visibility.Visible) return;
            
            FadeUI(0, 1, GR_OverlayContent);
            //hide the other bdr windows
            GR_OverlayContent.Children.OfType<Border>().ToList().ForEach(x => x.Visibility = Visibility.Collapsed);
            ScaleUI(0.8, 1, BDR_SettingsFrame);
        }

        private void Bn_Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Bn_about_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The ultimate music machine... This is gonna be replaced with a real about screen soon", Assembly.GetExecutingAssembly().GetName().Name);
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
        private void Cm_InputDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////for program start
            //if (MidiEngine.inDevice != null)
            //{
            //    MidiEngine.inDevice.StopRecording();
            //    MidiEngine.inDevice.Close();
            //}
            //if (cm_InputDevices.SelectedIndex > -1)
            //{
            //    MidiEngine.inDevice = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices.SelectedItem).Index);
            //    MidiEngine.inDevice.ChannelMessageReceived += InDevice_ChannelMessageReceived;
            //    MidiEngine.inDevice.StartRecording();
            //}

            //if (MidiEngine.inDevice2 != null)
            //{
            //    MidiEngine.inDevice2.StopRecording();
            //    MidiEngine.inDevice2.Close();
            //}
            //if (cm_InputDevices2.SelectedIndex > -1)
            //{
            //    MidiEngine.inDevice2 = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices2.SelectedItem).Index);
            //    MidiEngine.inDevice2.ChannelMessageReceived += InDevice_ChannelMessageReceived;
            //    MidiEngine.inDevice2.StartRecording();
            //}
        }

        private void Bn_cfgSave_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.ActiveInputDeviceIndex = cm_InputDevices.SelectedIndex;
            AppConfig.ActiveInputDevice2Index = cm_InputDevices2.SelectedIndex;

            for (int i = 0; i < 9; i++)
            {
                CheckBox cbcfg = WP_AllowedParams.Children[i] as CheckBox;
                AppConfig.InDeviceAllowedParams[i] = cbcfg.IsChecked.Value;
            }

            AppConfig.DisplayMode = rb_syncfg_Extended.IsChecked.Value 
                ? DisplayModes.Studio : rb_syncfg_Micro.IsChecked.Value 
                ? DisplayModes.Compact : DisplayModes.Standard;
            AppConfig.Input1RelayMode = CFGCB_SynthRelay1.IsChecked.Value;
            AppConfig.Input2RelayMode = CFGCB_SynthRelay2.IsChecked.Value;
            if (AppConfig.DisplayMode == CurrentViewDM)
            {
                currentView.HandleEvent(this, new EventArgs(), "RefMainWin");
                currentView.HandleEvent(sender, new EventArgs(), "RefAppConfig");
            }
            if (AppConfig.DisplayMode != CurrentViewDM)
            {
                SwitchView(AppConfig.DisplayMode);
            }
            SaveConfig();

            ScaleUI(1, 0.8, BDR_SettingsFrame);
            FadeUI(1, 0, GR_OverlayContent);

        }

        private void CfgHelpRequested_Click(object sender, RoutedEventArgs e)
        {
            checkstates.Clear();
            foreach (CheckBox item in WindowHelper.FindVisualChildren<CheckBox>(BDR_SettingsFrame))
            {
                checkstates.Add((item.Name, item.IsChecked.Value));
            }

            SynHelpRequested = !SynHelpRequested;
            Cursor = SynHelpRequested ? Cursors.Help : Cursors.Arrow;
        }

        private void RelayMode_Click(object sender, RoutedEventArgs e)
        {
            if(SynHelpRequested)
            {
                CfgHelpRequest_RestoreCheckStates();
                Cursor = Cursors.Arrow;
                SynHelpRequested = false;
                MessageBox.Show("If checked, MIDI messages sent by the device will be processed as they are received. " +
                    "If unchecked, the message will be modified to the parameters set by the synth (Transpose, octave, instruments, etc.)\r\n\r\n" +
                    "Note: Certain parameters such as instrument selection, and control changes will affect midi output " +
                    "regardless of the setting. The difference is, when checked, control changes made within the synth will be " +
                    "overridden by the device when the device sends an event making the change.\r\n\r\n" +
                    "It is recommended to leave unchecked if the selected device is an external keyboard starting on the A0 key.","MIDI Device Relay Mode");
            }
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

        private void GB_AllowedParams_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            CfgHelpRequest_RestoreCheckStates();
            e.Handled = false;
            if (SynHelpRequested)
            {
                this.Cursor = Cursors.Arrow;
                SynHelpRequested = false;
                MessageBox.Show("Filters MIDI events sent by the device. When the device is in relay mode, these settings are ignored, meaning all events received from the device are processed by the synth.","Allowed Device Control Parameters");
            }
        }

        private void Bn_cfgLaunchInsdef_Click(object sender, RoutedEventArgs e)
        {
            PopulateSavedDefinitions();
            ScaleUI(1, 0.8, BDR_SettingsFrame);
            ScaleUI(0.8, 1, BDR_InstrumentDefinitionsFrame);
        }

        #endregion

        #region InsDef-View Interaction

        private void Bn_InsDefDel_Click(object sender, RoutedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null)
            {
                return;
            }
            if ((string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content == "Default")
            {
                MessageBox.Show("You may not delete the default definition.", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            Definitions.Remove(def);
            LB_SavedDefs.Items.Remove(LB_SavedDefs.SelectedItem);
        }

        private void LB_SavedDefs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            InsDefSetEditor((ListBoxItem)LB_SavedDefs.SelectedItem, Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content));
        }

        private void Bn_InsDefAdd_Click(object sender, RoutedEventArgs e)
        {
            InstrumentDefinition def = new InstrumentDefinition()
            {
                Name = "Definition " + (LB_SavedDefs.Items.Count + 1),
                Banks = new ObservableCollection<Bank>(),
                AssociatedDeviceIndex = -1
            };
            Definitions.Add(def);
            LB_SavedDefs.Items.Add(new ListBoxItem() { Content = def.Name });
            LB_SavedDefs.SelectedIndex = LB_SavedDefs.Items.Count - 1;
            InsDefSetEditor((ListBoxItem)LB_SavedDefs.SelectedItem, Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content));
        }

        private void Lv_banks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            if (lv_banks.SelectedItems.Count != 1)
            {
                lv_patches.ItemsSource = null;
                return;
            }
            if ((lv_banks.SelectedItem as Bank) == null) { return; }
            InsDefPopulatePatches(def, ((Bank)lv_banks.SelectedItem).Index);

        }

        private void Tb_defName_TextChanged(object sender, TextChangedEventArgs e)
        {
            _insDefNameDirty = true;
        }

        //TODO: Add/delete banks, patches, and save
        private void Bn_InsDefAddBank_Click(object sender, RoutedEventArgs e)
        {
            InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            def.Banks.Add(new Bank(def.Banks.Count, "Bank " + (def.Banks.Count+1)));
        }

        private void Bn_InsDefDelBank_Click(object sender, RoutedEventArgs e)
        {
            if (lv_banks.SelectedItems.Count <= 0) return;
            InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
           
            while (lv_banks.SelectedItems.Count > 0)
            {
                def.Banks.RemoveAt(lv_banks.SelectedIndex);
            }
        }

        private void Bn_InsDefAddPatch_Click(object sender, RoutedEventArgs e)
        {
            if (lv_banks.SelectedItem == null) return;
            Bank b = (Bank)lv_banks.SelectedItem;
            b.Instruments.Add(new NumberedEntry(b.Instruments.Count, "Instrument " + (b.Instruments.Count+1)));

        }

        private void Bn_InsDefDelPatch_Click(object sender, RoutedEventArgs e)
        {
            if (lv_banks.SelectedItem == null) return;
            if (lv_patches.SelectedItems.Count < 0) return;
            Bank b = (Bank)lv_banks.SelectedItem;

            while (lv_patches.SelectedItems.Count > 0)
            {
                b.Instruments.RemoveAt(lv_patches.SelectedIndex);
            }
        }

        private void Bn_InsDefRename_Click(object sender, RoutedEventArgs e)
        {
            InstrumentDefinition d = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            InstrumentDefinition n = Definitions.FirstOrDefault(x => x.Name == tb_defName.Text);
            if (n != null && _insDefNameDirty)
            {
                MessageBox.Show("A definition with this name already exists. Please try again.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                tb_defName.Text = (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content;
                _insDefNameDirty = false;
                return;
            }
            if (d != null)
            {
                d.Name = tb_defName.Text;
                int inx = LB_SavedDefs.SelectedIndex;
                PopulateSavedDefinitions();
                LB_SavedDefs.SelectedIndex = inx;
                _insDefNameDirty = false;
                return;
            }
        }

        private void Cm_InsDefDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NumberedEntry prv = null;
            if(e.RemovedItems.Count > 0)
            {
                prv = (NumberedEntry)e.RemovedItems[0];
            }
            if (Definitions == null || LB_SavedDefs.SelectedItem == null || cm_InsDefDevices.SelectedItem == null) return;
            InstrumentDefinition currentDef = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);

            int ProposedIndex = ((NumberedEntry)cm_InsDefDevices.SelectedItem).Index;

            InstrumentDefinition CheckDupe = Definitions.FirstOrDefault(x => x.AssociatedDeviceIndex == ProposedIndex);

            
            if (currentDef == null) return;
            if (CheckDupe != null )
            {
                if (CheckDupe != currentDef && ProposedIndex != -1)
                {
                    var name = ((NumberedEntry)cm_InsDefDevices.SelectedItem).EntryName;
                    var confirm = MessageBox.Show($"{name} already has definition '{CheckDupe.Name}' assigned. Are you sure you want reassign definitions?", "Definition Association", MessageBoxButton.YesNo,MessageBoxImage.Warning);
                    if (confirm != MessageBoxResult.Yes)
                    {
                        cm_InsDefDevices.SelectedItem = prv;
                        return;
                    }
                    CheckDupe.AssociatedDeviceIndex = -1;
                }
            }

            currentDef.AssociatedDeviceIndex = ((NumberedEntry)cm_InsDefDevices.SelectedItem).Index;
        }

        #endregion

        #region Instrument Definition Logic

        private void PopulateSavedDefinitions()
        {
            LB_SavedDefs.Items.Clear();

            foreach (InstrumentDefinition item in Definitions)
            {
                LB_SavedDefs.Items.Add(new ListBoxItem() { Content = item.Name });
            }
            //LB_SavedDefs.SelectedIndex = LB_SavedDefs.SelectedIndex = LB_SavedDefs.Items.Count - 1;//wtf?
            LB_SavedDefs.SelectedIndex = LB_SavedDefs.Items.Count - 1;
        }

        internal void PopulateSavedNFXProfiles()
        {
            LB_SavedProfiles.Items.Clear();

            foreach (NFXDelayProfile item in NFXProfiles)
            {
                LB_SavedProfiles.Items.Add(new ListBoxItem() { Content = item.ProfileName });
            }
            LB_SavedProfiles.SelectedIndex = LB_SavedProfiles.Items.Count - 1;
        }
        private void NFXProfileSetEditor(ListBoxItem lvItem, NFXDelayProfile profile)
        {
            if (lvItem == null)
            {
                gb_NFXProfEditor.IsEnabled = false;
                return;
            }
            gb_NFXProfEditor.IsEnabled = true;
            _backupProfile = profile;
            
            TB_NFX_profile_name.Text = (string)lvItem.Content;
            _NFXProfileNameDirty = false;
            Dial_NFX_Interval.SetValueSuppressed(profile.Delay);
            Dial_NFX_StepCount.SetValueSuppressed(profile.OffsetMap.Count);
            NFXPopulateSteps(profile);
        }
        private void NFXPopulateSteps(NFXDelayProfile selected)
        {
            lv_steps.Items.Clear();
            for (int i = 0; i < selected.OffsetMap.Count; i++)
            {
                lv_steps.Items.Add(new { Step = i + 1, Pitch = selected.OffsetMap[i].pitch, Decay = selected.OffsetMap[i].decay });
            }
            lv_steps.SelectedIndex = 0;
        }

        private void InsDefSetEditor(ListBoxItem lvItem, InstrumentDefinition insdf)
        {
            if (lvItem == null)
            {
                gb_InsDefEditor.IsEnabled = false;
                return;
            }
            gb_InsDefEditor.IsEnabled = true;
            bool EnableEdit = (string)lvItem.Content != "Default";

            tb_defName.IsEnabled = EnableEdit;
            tb_defName.Text = (string)lvItem.Content;
            _insDefNameDirty = false;
            bn_InsDefAddBank.IsEnabled = EnableEdit;
            bn_InsDefDelBank.IsEnabled = EnableEdit;
            bn_InsDefAddPatch.IsEnabled = EnableEdit;
            bn_InsDefDelPatch.IsEnabled = EnableEdit;
            bn_InsDefRename.IsEnabled = EnableEdit;
            bn_InsDefSetActiveDevice.IsEnabled = EnableEdit;
            cm_InsDefDevices.IsEnabled = EnableEdit;
            cm_InsDefDevices.SelectedItem = cm_InsDefDevices.Items.Cast<NumberedEntry>().FirstOrDefault(xx => xx.Index == insdf.AssociatedDeviceIndex);
            lv_banks.IsReadOnly = !EnableEdit;
            lv_patches.IsReadOnly = !EnableEdit;

            InsDefPopulateBanks(insdf);
        }

        private void InsDefPopulatePatches(InstrumentDefinition def, int bank)
        {
            if(def == null) return;//oops?
            lv_patches.ItemsSource = def.Banks.FirstOrDefault(x => x.Index == bank)?.Instruments;
        }

        private void InsDefPopulateBanks(InstrumentDefinition def)
        {
            lv_banks.ItemsSource = def.Banks;
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
            foreach (NumberedEntry item in InputDevices)
            {
                cm_InputDevices.Items.Add(item);
                cm_InputDevices2.Items.Add(item);
            }
            int midiOutIndex = 0;
            foreach (string item in MidiEngine.GetOutputDevices())
            {
                OutputDevices.Add(new NumberedEntry(midiOutIndex, item));
                midiOutIndex++;
            }
            foreach (NumberedEntry item in OutputDevices)
            {
                cm_InsDefDevices.Items.Add(item);
            }
            //add unused
            cm_InsDefDevices.Items.Insert(0, new NumberedEntry(-1, "Unassigned"));
            cm_InsDefDevices.SelectedIndex = 0;
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

        private void SwitchView(DisplayModes mode)
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
            double _scale = (uielm != this &&
                             uielm != BDR_InstrumentDefinitionsFrame &&
                             uielm != BDR_SettingsFrame) ? scale : 1; //exclude scaling to some elements (thanks to WPF blurring :))
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
                MessageBox.Show("Configuration error: null");
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

        public string SaveInsDef(string path)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(Definitions,Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Write Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return path;
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
                MessageBox.Show(ex.Message, "Write Failed", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CfgHelpRequest_RestoreCheckStates()
        {
            foreach (var item in checkstates)
            {
                CheckBox cb = this.FindName(item.name) as CheckBox;
                cb.IsChecked = item.value;
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
                view.HandleEvent(this, new EventArgs(), "RefMainWin");
                view.HandleEvent(this, new EventArgs(), "MTaskWorker");
                if (MidiEngine.inDevice != null)
                {
                    MidiEngine.inDevice.StopRecording();
                    MidiEngine.inDevice.Close();
                }
                if (cm_InputDevices.SelectedIndex > -1)
                {
                    MidiEngine.inDevice = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices.SelectedItem).Index);
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
                if (cm_InputDevices2.SelectedIndex > -1)
                {
                    MidiEngine.inDevice2 = new Sanford.Multimedia.Midi.InputDevice(((NumberedEntry)cm_InputDevices2.SelectedItem).Index);
                    MidiEngine.inDevice2.PostDriverCallbackToDelegateQueue = false;
                    MidiEngine.inDevice2.PostEventsOnCreationContext = false;
                    MidiEngine.inDevice2.ChannelMessageReceived += InDevice_ChannelMessageReceived;
                    MidiEngine.inDevice2.StartRecording();
                }
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

        private void Lv_banks_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if((string)e.Column.Header == "Index")//Is there a more solid way?
            {
                InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
                TextBox t = e.EditingElement as TextBox;
                Bank DataContext = (Bank)t.DataContext;
                
                bn_InsDefSave.IsEnabled = false;
               
                if (!int.TryParse(t.Text, out int triedval))
                {
                    MessageBox.Show("Value must be integer.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                if (triedval < 0)
                {
                    MessageBox.Show("Value must be positive integer", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                Bank f = def.Banks.FirstOrDefault(xx => xx.Index == triedval);

                if (f != null && DataContext != f)
                {
                    MessageBox.Show("Cannot have multiple items with the same index.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                bn_InsDefSave.IsEnabled = true;
            }
        }

        private void lv_banks_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {

        }

        private void Lv_patches_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if ((string)e.Column.Header == "Index")//Is there a more solid way?
            {
                Bank dfbank = (Bank)lv_banks.SelectedItem;
                TextBox t = e.EditingElement as TextBox;
                NumberedEntry DataContext = (NumberedEntry)t.DataContext;

                bn_InsDefSave.IsEnabled = false;

                if (!int.TryParse(t.Text, out int triedval))
                {
                    MessageBox.Show("Value must be integer.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                if (triedval < 0)
                {
                    MessageBox.Show("Value must be positive integer", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                NumberedEntry f = dfbank.Instruments.FirstOrDefault(xx => xx.Index == triedval);

                if (f != null && DataContext != f)
                {
                    MessageBox.Show("Cannot have multiple items with the same index.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                bn_InsDefSave.IsEnabled = true;
            }
        }

        private void Mi_PopGenMIDI_Click(object sender, RoutedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            if (lv_banks.SelectedItem == null) return;
            InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            Bank b = (Bank)lv_banks.SelectedItem;

            b.Instruments = InstrumentDefinition.DefaultBanks().FirstOrDefault(x => x.Index == 0).Instruments;
            InsDefPopulatePatches(def, b.Index);
        }

        private void Mi_PopGenDRUM_Click(object sender, RoutedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            if (lv_banks.SelectedItem == null) return;
            InstrumentDefinition def = Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            Bank b = (Bank)lv_banks.SelectedItem;

            b.Instruments = InstrumentDefinition.DefaultBanks().FirstOrDefault(x => x.Index == 127).Instruments;
            InsDefPopulatePatches(def, b.Index);
        }

        private void Bn_InsDefSave_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.InstrumentDefinitionPath = SaveInsDef(App.APP_DATA_DIR + "Instruments.def");
            SaveConfig();

            currentView.HandleEvent(this, new EventArgs(), "InsDEF_Changed");
            ScaleUI(1, 0.8, BDR_InstrumentDefinitionsFrame);
            ScaleUI(0.8, 1, BDR_SettingsFrame);
            //FadeUI(1, 0, GR_OverlayContent);
        }

        private void Bn_InsDefSetActiveDevice_Click(object sender, RoutedEventArgs e)
        {

            cm_InsDefDevices.SelectedItem = cm_InsDefDevices.Items.OfType<NumberedEntry>().FirstOrDefault(x=>x.Index == AppConfig.ActiveOutputDeviceIndex);

        }

        private void bn_NFXProfSave_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Save config
            SaveNFXProfiles();
            currentView.HandleEvent(this, new EventArgs(), "NFX_DelayUpdated");
            ScaleUI(1, 0.8, BDR_NFXDelayCustomizationFrame);
            FadeUI(1, 0, GR_OverlayContent);
        }

        private void GroupBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SynHelpRequested)
            {
                Cursor = Cursors.Arrow;
                SynHelpRequested = false;
                if (sender == GB_DelaySettings)
                {
                    MessageBox.Show("  • STEP COUNT: How many 'echoed' notes you will hear\r\n\r\n" +
                        "  • DELAY INTERVAL: How long between each echo, in milliseconds", ((GroupBox)sender).Header.ToString());
                }
                if (sender == GB_StepSettings)
                {
                    MessageBox.Show("  • RELATIVE PITCH: A range between -36 and 36 half-steps (semi-tones) from the original note. Use 0 If you don't want your echoed notes to change pitch.\r\n\r\n" +
                        "  • DECAY: A range between 0 (original volume/velocity) and 100 (silent). Use 0 If you don't want your echoed notes be quieter than the original.", ((GroupBox)sender).Header.ToString());
                }
                
            }
            
        }

        private void bn_NFXProfAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void bn_NFXProfDel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LB_SavedProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXProfileSetEditor((ListBoxItem)LB_SavedProfiles.SelectedItem, NFXProfiles.FirstOrDefault(x => x.ProfileName == (string)((ListBoxItem)LB_SavedProfiles.SelectedItem).Content));
        }

        private void Lv_steps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lv_steps.SelectedItem == null)
            {
                GB_StepSettings.Header = "Step Setting";
                return;
            }
            var f = new {Step = 0, Pitch=12, Decay=20 };
            var item = Cast(f,lv_steps.SelectedItem);
            Dial_NFX_Decay.SetValueUnsuppressed(item.Decay);
            Dial_NFX_Pitch.SetValueUnsuppressed(item.Pitch);
            GB_StepSettings.Header = "Step Setting (Editing step #" + item.Step + ")";
        }

        //Compiler trickery at its worst
        private static T Cast<T>(T _, object x) => (T)x;

        private void Dial_NFX_ValChanged(object sender, EventArgs e)
        {
            //profile
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXDelayProfile prof = NFXProfiles.FirstOrDefault(x => x.ProfileName == (string)((ListBoxItem)LB_SavedProfiles.SelectedItem).Content);
            prof.Delay = Dial_NFX_Interval.Value;
            List<(int pitch, int decay)> newSteps = new List<(int pitch, int decay)>();
            for (int i = 0; i < Dial_NFX_StepCount.Value; i++)
            {
                if(prof.OffsetMap.Count > i)
                {
                    newSteps.Add(prof.OffsetMap[i]);
                }
                else
                {
                    newSteps.Add((0,0));

                }
            }
            prof.OffsetMap = newSteps;
            NFXPopulateSteps(prof);
        }

        private void Dial_NFX_STEP_ValChanged(object sender, EventArgs e)
        {
            if (lv_steps.SelectedIndex < 0) return;
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXDelayProfile prof = NFXProfiles.FirstOrDefault(x => x.ProfileName == (string)((ListBoxItem)LB_SavedProfiles.SelectedItem).Content);

            var offset = prof.OffsetMap[lv_steps.SelectedIndex];
            prof.OffsetMap.Remove(offset);
            offset.decay = Dial_NFX_Decay.Value;
            offset.pitch = Dial_NFX_Pitch.Value;
            prof.OffsetMap.Insert(lv_steps.SelectedIndex, offset);


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
