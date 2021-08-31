using Microsoft.Win32;
using MidiSynth7.entities.controls;
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

namespace MidiSynth7.components.views
{
    /// <summary>
    /// Interaction logic for StudioView.xaml
    /// </summary>
    public partial class StudioView : Page, ISynthView
    {
        SystemConfig Config;
        MidiEngine MidiEngine;
        MainWindow AppContext;
        private int Transpose;
        private int RiffKey;
        private bool mfile_playing = false;
        private List<Ellipse> channelElipses = new List<Ellipse>();
        private List<MainWindow.ChInvk> channelIndicators = new List<MainWindow.ChInvk>();

        public StudioView(MainWindow context, ref SystemConfig config, ref MidiEngine engine)
        {
            InitializeComponent();
            Config = config;
            MidiEngine = engine;
            AppContext = context;
            cb_Devices.Items.Clear();


            if (Config.ActiveOutputDeviceIndex == -1)
            {
                Config.ActiveOutputDeviceIndex = 0;
            }

            int midiOutIndex = 0;
            AppContext.OutputDevices.Clear();
            foreach (string item in MidiEngine.GetOutputDevices())
            {
                AppContext.OutputDevices.Add(new NumberedEntry(midiOutIndex, item));
                midiOutIndex++;
            }
            foreach (NumberedEntry item in AppContext.OutputDevices)
            {
                cb_Devices.Items.Add(item);
            }

            if (Config.ActiveOutputDeviceIndex < cb_Devices.Items.Count)
            {
                cb_Devices.SelectedIndex = Config.ActiveOutputDeviceIndex;
            }

            Transpose = Config.PitchOffsets[1];
            CB_EnforceInstruments.IsChecked = config.EnforceInstruments;
            pianomain.SetNoteText(Transpose);

            #region Controls
            CTRL_Chorus.Value = Config.ChannelChoruses[0];
            CTRL_Modulation.Value = Config.ChannelModulations[0];
            CTRL_Balance.Value = Config.ChannelPans[0];
            CTRL_Octave.Value = Config.PitchOffsets[0];
            CTRL_Reverb.Value = Config.ChannelReverbs[0];
            CTRL_Volume.Value = Config.ChannelVolumes[0];
            UpdateMIDIControls();
            #endregion

            #region Patch & banks

            foreach (Bank item in AppContext.InstrumentDefinition.Banks)
            {
                cb_mBank.Items.Add(item);
                cb_sBank.Items.Add(item);
                //OFX_I1BankSel.Items.Add(item);
                //OFX_I2BankSel.Items.Add(item);
            }
            foreach (NumberedEntry item in AppContext.InstrumentDefinition.Drumkits)
            {
                cb_dkitlist.Items.Add(item);
            }
            //OFX_I1BankSel.SelectedIndex = 0;
            //OFX_I2BankSel.SelectedIndex = 0;
            cb_mBank.SelectedIndex = 0;
            cb_sBank.SelectedIndex = 0;

            config.ChannelBanks[0] = (cb_mBank.Items.Count >= config.ChannelBanks[0]) ? config.ChannelBanks[0] : 0;
            config.ChannelInstruments[0] = (cb_mPatch.Items.Count >= config.ChannelInstruments[0]) ? config.ChannelInstruments[0] : 0;
            config.ChannelBanks[4] = (cb_sBank.Items.Count >= config.ChannelBanks[4]) ? config.ChannelBanks[4] : 0;
            config.ChannelInstruments[4] = (cb_sPatch.Items.Count >= config.ChannelInstruments[4]) ? config.ChannelInstruments[4] : 0;

            //OFX_I1BankSel.SelectedIndex = mainCG.IT_Octave3BankIndex1;
            //OFX_I2BankSel.SelectedIndex = mainCG.IT_Octave3BankIndex2;
            cb_mBank.SelectedIndex = config.ChannelBanks[0];
            cb_sBank.SelectedIndex = config.ChannelBanks[4];

            cb_mPatch.SelectedIndex = config.ChannelInstruments[0];
            cb_sPatch.SelectedIndex = config.ChannelInstruments[4];
            #endregion

            #region ADD ellipses
            AppContext.channelIndicators.Clear();
            AppContext.channelElipses.Clear();
            AppContext.channelElipses.Add(ch1);
            AppContext.channelElipses.Add(ch2);
            AppContext.channelElipses.Add(ch3);
            AppContext.channelElipses.Add(ch4);
            AppContext.channelElipses.Add(ch5);
            AppContext.channelElipses.Add(ch6);
            AppContext.channelElipses.Add(ch7);
            AppContext.channelElipses.Add(ch8);
            AppContext.channelElipses.Add(ch9);
            AppContext.channelElipses.Add(ch10);
            AppContext.channelElipses.Add(ch11);
            AppContext.channelElipses.Add(ch12);
            AppContext.channelElipses.Add(ch13);
            AppContext.channelElipses.Add(ch14);
            AppContext.channelElipses.Add(ch15);
            AppContext.channelElipses.Add(ch16);
            foreach (Ellipse item in AppContext.channelElipses)
            {
                AppContext.channelIndicators.Add(new MainWindow.ChInvk(item, AppContext));
            }
            #endregion

        }

        async Task FlashChannelActivity(int index)
        {
            Action invoker = delegate ()
            {
                AppContext.channelElipses[index].Fill = (Brush)FindResource("CH_IND_On");
                AppContext.channelIndicators[index].CounterReset();
            };
            await Dispatcher.BeginInvoke(invoker);
        }

        private void UpdateMIDIControls()//TODO: Be Channel specific
        {

            if (MidiEngine != null)
            {
                for (int i = 0; i < 16; i++)
                {
                    MidiEngine.MidiNote_SetReverb(i, CTRL_Reverb.Value);
                    MidiEngine.MidiNote_SetChorus(i, CTRL_Chorus.Value);
                    MidiEngine.MidiNote_SetModulation(i, CTRL_Modulation.Value);
                    MidiEngine.MidiNote_SetPan(i, CTRL_Balance.Value);
                    MidiEngine.MidiNote_SetControl(Sanford.Multimedia.Midi.ControllerType.Volume, i, CTRL_Volume.Value);
                }

            }

        }

        private void Cb_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cb_internalsf2.IsChecked.Value)
            {
                Config.ActiveOutputDeviceIndex = cb_Devices.SelectedIndex;
                AppContext.GenerateMIDIEngine(this,((NumberedEntry)cb_Devices.SelectedItem).Index);
            }
        }

        private async void MIO_bn_stop_Click(object sender, RoutedEventArgs e)
        {
            MidiEngine?.MidiEngine_Panic();
            await invokeUnLightRange(0, pianomain.KeyCount + 21);
        }

        private void ToggleChecked(object sender, RoutedEventArgs e)
        {
            //TODO: Enable/disable groupboxes based on check state

           if(Config!= null) Config.EnforceInstruments = CB_EnforceInstruments?.IsChecked ?? true;
        }

        private void MIO_bn_SetSF2_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Composer

        private void cp_bnPlay_Click(object sender, RoutedEventArgs e)
        {
            if(!mfile_playing)
            {
                MidiEngine.MidiFile_Play();
                mfile_playing = true;
            }
        }

        private void cp_bnStop_Click(object sender, RoutedEventArgs e)
        {
            if (mfile_playing)
            {
                MidiEngine.MidiFile_Stop();
                mfile_playing = false;
            }
        }

        private void cp_bnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "MIDI Files (*.mid)|*.mid";
            of.Title = "Select MIDI File for Playback";
            if(of.ShowDialog().Value)
            {
                if(MidiEngine!=null)
                {
                    MidiEngine.MidiFile_Add(of.FileName);
                    
                }
            }
        }

        #endregion

        #region Riff Center
        private void rb_rcp12_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp11_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp10_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp9_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp8_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp7_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp6_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp5_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp4_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp3_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp2_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void rb_rcp1_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BnRiff_Define_Click(object sender, RoutedEventArgs e)
        {

        }

        private void criffenablecheck(object sender, RoutedEventArgs e)
        {

        }

        private void riffcenter_toggleCheck(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Bank/Instrument selection UI events

        private void Cb_mPatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MidiEngine != null)
            {
                Bank bank = (Bank)cb_mBank.SelectedItem;
                NumberedEntry patch = (NumberedEntry)cb_mPatch.SelectedItem;
                MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 0);
            }
            Config.ChannelInstruments[0] = cb_mPatch.SelectedIndex;
        }

        private void Cb_mBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_mPatch.Items.Clear();
            if (cb_mBank.SelectedItem != null)
            {
                Bank entry = (Bank)cb_mBank.SelectedItem;
                foreach (NumberedEntry item in entry.Instruments)
                {
                    cb_mPatch.Items.Add(item);
                }
            }
            cb_sPatch.SelectedIndex = 0;
        }

        private void Cb_sPatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MidiEngine != null)
            {
                Bank bank = (Bank)cb_sBank.SelectedItem;
                NumberedEntry patch = (NumberedEntry)cb_sPatch.SelectedItem;
                MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 4);
            }
            Config.ChannelInstruments[4] = cb_sPatch.SelectedIndex;

        }

        private void Cb_sBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_sPatch.Items.Clear();
            if (cb_sBank.SelectedItem != null)
            {
                Bank entry = (Bank)cb_sBank.SelectedItem;
                foreach (NumberedEntry item in entry.Instruments)
                {
                    cb_sPatch.Items.Add(item);
                }
            }
            cb_sPatch.SelectedIndex = 0;
        }

        private void Cb_dkit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion

        private void CTRL_ValueChanged(object sender, EventArgs e)
        {
            UpdateMIDIControls();
            if (Config != null)
            {
                Config.ChannelReverbs[0] = CTRL_Reverb.Value;
                Config.ChannelVolumes[0] = CTRL_Volume.Value;
                Config.ChannelModulations[0] = CTRL_Modulation.Value;
                Config.ChannelChoruses[0] = CTRL_Chorus.Value;
                Config.ChannelPans[0] = CTRL_Balance.Value;
                Config.PitchOffsets[0] = CTRL_Octave.Value;//global octave

            }
        }

        private void OFX_bn_Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Piano keys

        private void Pianomain_pKeyDown(object sender, PKeyEventArgs e)
        {
            RiffKey = e.KeyID;
            if (MidiEngine != null)
            {
                Bank sbank = (Bank)cb_sBank.SelectedItem;
                NumberedEntry spatch = (NumberedEntry)cb_sPatch.SelectedItem;
                Bank bank = (Bank)cb_mBank.SelectedItem;
                NumberedEntry patch = (NumberedEntry)cb_mPatch.SelectedItem;

                //NumberedEntry OFX_b1 = (NumberedEntry)OFX_I1BankSel.SelectedItem;
                //NumberedEntry OFX_b2 = (NumberedEntry)OFX_I2BankSel.SelectedItem;
                //NumberedEntry OFX_p1 = (NumberedEntry)OFX_I1PatchSel.SelectedItem;
                //NumberedEntry OFX_p2 = (NumberedEntry)OFX_I2PatchSel.SelectedItem;
                //TODO: REPLACE WITH AN ACTUAL SELECTION
                Bank OFX_b1 = new Bank(0, "b1");
                Bank OFX_b2 = new Bank(0, "b2");
                NumberedEntry OFX_p1 = new NumberedEntry(32, "p1"); ;
                NumberedEntry OFX_p2 = new NumberedEntry(48, "p2"); ;
                if (cb_RIFF_Enable.IsChecked.Value)
                {
                    return;
                }
                if (cb_DS_Enable.IsChecked.Value)
                {
                    NumberedEntry dkit = (NumberedEntry)cb_dkitlist.SelectedItem;
                    MidiEngine.MidiNote_SetProgram(127, dkit.Index, 9);
                    MidiEngine.MidiNote_Play(9, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                    return;
                }
                if (cb_OFX_Enble.IsChecked.Value)
                {
                    if (rb_ofx_Std.IsChecked.Value)
                    {
                        if(Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 1);
                            MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 2);
                        }
                        MidiEngine.MidiNote_SetPan(1, Config.ChannelPans[1]);
                        MidiEngine.MidiNote_SetPan(2, Config.ChannelPans[2]);
                        MidiEngine.MidiNote_Play(1, -12 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                        MidiEngine.MidiNote_Play(2, -24 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                    }
                    if (rb_ofx_Orchestral.IsChecked.Value)
                    {
                        if(Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(0, 46, 1);
                            MidiEngine.MidiNote_SetProgram(0, 48, 2);
                        }
                        MidiEngine.MidiNote_SetPan(1, 0);
                        MidiEngine.MidiNote_SetPan(2, 127);
                        MidiEngine.MidiNote_Play(1, -12 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                        MidiEngine.MidiNote_Play(2, -24 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                    }
                    if (rb_ofx_custom.IsChecked.Value)
                    {
                        if (Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(OFX_b1.Index, OFX_p1.Index, 1);
                            MidiEngine.MidiNote_SetProgram(OFX_b2.Index, OFX_p2.Index, 2);
                        }
                        MidiEngine.MidiNote_SetPan(1, 0);
                        MidiEngine.MidiNote_SetPan(2, 127);
                        //offsets {g, t, ofx3_1, ofx3_2}
                        int Offset1 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[2];
                        int Offset2 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[3];
                        MidiEngine.MidiNote_Play(1, Offset1 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                        MidiEngine.MidiNote_Play(2, Offset2 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                    }
                }
                if (cb__InsS_Enable.IsChecked.Value)
                {
                    if (e.KeyID <= 16)
                    {
                        if(Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(sbank.Index, spatch.Index, 3);
                        }
                        MidiEngine.MidiNote_SetPan(3, CTRL_Balance.Value);
                        MidiEngine.MidiNote_Play(3, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                        return;
                    }

                }
                if(Config.EnforceInstruments)
                {
                    MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 0);
                }
                MidiEngine.MidiNote_SetPan(0, CTRL_Balance.Value);
                MidiEngine.MidiNote_Play(0, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);

            }
        }

        private void Pianomain_pKeyDown_VelocitySense(object sender, PKeyEventArgs e, int velocity, int channel=0)
        {
            if (e.KeyID < 46)
            {
                RiffKey = e.KeyID;
            }
            if (MidiEngine != null)
            {
                Bank sbank = (Bank)cb_sBank.SelectedItem;
                NumberedEntry spatch = (NumberedEntry)cb_sPatch.SelectedItem;
                Bank bank = (Bank)cb_mBank.SelectedItem;
                NumberedEntry patch = (NumberedEntry)cb_mPatch.SelectedItem;

                //NumberedEntry OFX_b1 = (NumberedEntry)OFX_I1BankSel.SelectedItem;
                //NumberedEntry OFX_b2 = (NumberedEntry)OFX_I2BankSel.SelectedItem;
                //NumberedEntry OFX_p1 = (NumberedEntry)OFX_I1PatchSel.SelectedItem;
                //NumberedEntry OFX_p2 = (NumberedEntry)OFX_I2PatchSel.SelectedItem;
                //TODO: REPLACE WITH AN ACTUAL SELECTION
                Bank OFX_b1 = new Bank(0, "b1");
                Bank OFX_b2 = new Bank(0, "b2");
                NumberedEntry OFX_p1 = new NumberedEntry(32, "p1"); ;
                NumberedEntry OFX_p2 = new NumberedEntry(48, "p2"); ;
                if (cb_RIFF_Enable.IsChecked.Value)
                {
                    return;
                }
                if (cb_DS_Enable.IsChecked.Value)
                {
                    NumberedEntry dkit = (NumberedEntry)cb_dkitlist.SelectedItem;
                    MidiEngine.MidiNote_SetProgram(127, dkit.Index, 9);
                    MidiEngine.MidiNote_Play(9, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                    return;
                }
                if (cb_OFX_Enble.IsChecked.Value)
                {
                    if (rb_ofx_Std.IsChecked.Value)
                    {
                        if(Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 1);
                            MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 2);
                        }
                        MidiEngine.MidiNote_SetPan(1, 0);
                        MidiEngine.MidiNote_SetPan(2, 127);
                        MidiEngine.MidiNote_Play(1, -12 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                        MidiEngine.MidiNote_Play(2, -24 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                    }
                    if (rb_ofx_Orchestral.IsChecked.Value)
                    {
                        if(Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(0, 46, 1);
                            MidiEngine.MidiNote_SetProgram(0, 48, 2);
                        }
                        MidiEngine.MidiNote_SetPan(1, Config.ChannelPans[1]);
                        MidiEngine.MidiNote_SetPan(2, Config.ChannelPans[2]);
                        MidiEngine.MidiNote_Play(1, -12 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                        MidiEngine.MidiNote_Play(2, -24 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                    }
                    if (rb_ofx_custom.IsChecked.Value)
                    {
                        if (Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(OFX_b1.Index, OFX_p1.Index, 1);
                            MidiEngine.MidiNote_SetProgram(OFX_b2.Index, OFX_p2.Index, 2);
                        }
                        MidiEngine.MidiNote_SetPan(1, 0);
                        MidiEngine.MidiNote_SetPan(2, 127);
                        int Offset1 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[2];
                        int Offset2 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[3];
                        MidiEngine.MidiNote_Play(1, Offset1 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                        MidiEngine.MidiNote_Play(2, Offset2 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                    }
                }
                if (cb__InsS_Enable.IsChecked.Value)
                {
                    if (e.KeyID <= 16)
                    {
                        if(Config.EnforceInstruments)
                        {
                            MidiEngine.MidiNote_SetProgram(sbank.Index, spatch.Index, 3);
                        }
                        MidiEngine.MidiNote_SetPan(3, CTRL_Balance.Value);
                        MidiEngine.MidiNote_Play(3, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                        return;
                    }

                }
                if(channel == 0)
                {
                    if(Config.EnforceInstruments)
                    {
                        MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 0);
                    }
                    MidiEngine.MidiNote_SetPan(0, CTRL_Balance.Value);
                }
                
                MidiEngine.MidiNote_Play(channel, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);

            }
        }

        private void Pianomain_pKeyUp(object sender, entities.controls.PKeyEventArgs e)
        {
            if (MidiEngine != null)
            {
                Bank sbank = (Bank)cb_sBank.SelectedItem;
                NumberedEntry spatch = (NumberedEntry)cb_sPatch.SelectedItem;
                Bank bank = (Bank)cb_mBank.SelectedItem;
                NumberedEntry patch = (NumberedEntry)cb_mPatch.SelectedItem;

                //Bank OFX_b1 = new Bank (Bank)OFX_I1BankSel.SelectedItem;
                //Bank OFX_b2 = new Bank (Bank)OFX_I2BankSel.SelectedItem;
                //NumberedEntry OFX_p1 = (NumberedEntry)OFX_I1PatchSel.SelectedItem;
                //NumberedEntry OFX_p2 = (NumberedEntry)OFX_I2PatchSel.SelectedItem;
                //TODO: Implement custom octaveFX dialog
                Bank OFX_b1 = new Bank(0, "b1");
                Bank OFX_b2 = new Bank(0, "b2");
                NumberedEntry OFX_p1 = new NumberedEntry(32, "p1"); ;
                NumberedEntry OFX_p2 = new NumberedEntry(48, "p2"); ;
                if (cb_RIFF_Enable.IsChecked.Value)
                {
                    return;
                }
                if (cb_DS_Enable.IsChecked.Value)
                {

                    MidiEngine.MidiNote_Stop(9, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                    return;
                }
                if (cb_OFX_Enble.IsChecked.Value)
                {
                    if (rb_ofx_Std.IsChecked.Value || rb_ofx_Orchestral.IsChecked.Value)
                    {

                        MidiEngine.MidiNote_Stop(1, -12 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                        MidiEngine.MidiNote_Stop(2, -24 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                    }
                    if (rb_ofx_custom.IsChecked.Value)
                    {

                        int Offset1 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[2];
                        int Offset2 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[3];
                        MidiEngine.MidiNote_Stop(1, Offset1 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                        MidiEngine.MidiNote_Stop(2, Offset2 + Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                    }
                }
                if (cb_NFX_Enable.IsChecked.Value)
                {
                    if (rb_nfx_echo.IsChecked.Value && rb_ofx_custom.IsChecked.Value && cb_NFX_Echo_OFX.IsChecked.Value)
                    {
                        MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 10);
                        MidiEngine.MidiNote_SetProgram(OFX_b1.Index, OFX_p1.Index, 11);
                        MidiEngine.MidiNote_SetProgram(OFX_b2.Index, OFX_p2.Index, 12);
                        MidiEngine.MidiNote_PlayTimed(10, e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value, 110, 100);
                        //offsets {g, t, ofx3_1, ofx3_2}
                        int Offset1 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[2];
                        int Offset2 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[3];
                        MidiEngine.MidiNote_PlayTimed(11, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) + Offset1 - 12, 110, 100);
                        MidiEngine.MidiNote_PlayTimed(12, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) + Offset2 - 12, 110, 100);
                    }
                    if (rb_nfx_echo.IsChecked.Value && rb_ofx_custom.IsChecked.Value && !cb_NFX_Echo_OFX.IsChecked.Value)
                    {
                        MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 10);
                        MidiEngine.MidiNote_SetProgram(OFX_b1.Index, OFX_p1.Index, 11);
                        MidiEngine.MidiNote_SetProgram(OFX_b2.Index, OFX_p2.Index, 12);
                        MidiEngine.MidiNote_PlayTimed(10, e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value, 110, 100);
                        //offsets {g, t, ofx3_1, ofx3_2}
                        int Offset1 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[2];
                        int Offset2 = (!cb_OFX_AllowOffset.IsChecked.Value) ? 0 : Config.PitchOffsets[3];
                        MidiEngine.MidiNote_PlayTimed(11, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) + Offset1, 110, 100);
                        MidiEngine.MidiNote_PlayTimed(12, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) + Offset2, 110, 100);
                    }

                    if (rb_nfx_echo.IsChecked.Value && rb_ofx_Orchestral.IsChecked.Value && !cb_NFX_Echo_OFX.IsChecked.Value)
                    {
                        MidiEngine.MidiNote_SetProgram(0, patch.Index, 10);
                        MidiEngine.MidiNote_SetProgram(0, 46, 11);
                        MidiEngine.MidiNote_SetProgram(0, 48, 12);
                        MidiEngine.MidiNote_PlayTimed(10, e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value, 110, 100);
                        MidiEngine.MidiNote_PlayTimed(11, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) - 12, 110, 100);
                        MidiEngine.MidiNote_PlayTimed(12, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) - 24, 110, 100);
                    }
                    if (rb_nfx_echo.IsChecked.Value && rb_ofx_Orchestral.IsChecked.Value && cb_NFX_Echo_OFX.IsChecked.Value)
                    {
                        MidiEngine.MidiNote_SetProgram(0, patch.Index, 10);
                        MidiEngine.MidiNote_SetProgram(0, 46, 11);
                        MidiEngine.MidiNote_SetProgram(0, 48, 12);
                        MidiEngine.MidiNote_PlayTimed(10, e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value, 110, 100);
                        MidiEngine.MidiNote_PlayTimed(11, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) - 24, 110, 100);
                        MidiEngine.MidiNote_PlayTimed(12, (e.KeyID + Transpose + 12 + 12 * CTRL_Octave.Value) - 36, 110, 100);
                    }

                }
                if (cb__InsS_Enable.IsChecked.Value)
                {
                    if (e.KeyID <= 16)
                    {
                        MidiEngine.MidiNote_Stop(3, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                        return;
                    }

                }
                MidiEngine.MidiNote_Stop(0, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);

            }
        }

        private async Task invokeUnLightRange(int start = 0, int count = 40)
        {
            void lightaction()
            {
                for (int i = start; i < count; i++)
                {
                    pianomain.UnLightKey(i);
                }
            }
            await Task.Run(() => Dispatcher.Invoke(lightaction));
        }

        #endregion

        #region SynthView Interface
        public async void HandleNoteOnEvent(object sender, NoteEventArgs e)
        {
            await FlashChannelActivity(e.ChannelMssge.MidiChannel);


            if (e.ChannelMssge.Data2 > 0)
            {

                pianomain.LightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value);
                if (e.ChannelMssge.MidiChannel != 0)
                {
                    pianomain.ALTLightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value);
                }
                if (e.ChannelMssge.MidiChannel == 9)
                {
                    pianomain.CustomLightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value, (LinearGradientBrush)this.TryFindResource("PercussionKeyLightBrush"));
                }
            }

            else
            {
                pianomain.UnLightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value);
            }
        }

        public async void HandleNoteOffEvent(object sender, NoteEventArgs e)
        {
            await FlashChannelActivity(e.ChannelMssge.MidiChannel);
            pianomain.UnLightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value);
            //Pianomain_pKeyUp(sender, new PKeyEventArgs(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value));
        }

        public void HandleEvent(object sender, EventArgs e, string id = "generic")
        {
            //placeholder for now?
            if (id == "RefMainWin")
            {
                AppContext = (MainWindow)sender;
            }
            if (id == "MTaskWorker")
            {
                MidiEngine = AppContext.MidiEngine;
            }
            if (id == "RefAppConfig")
            {
                Config = AppContext.AppConfig;
            }

            if(id == "SynthSustainCTRL_ON")
            {
                Mio_SustainPdl.Fill = (Brush)FindResource("CH_IND_On");
            }
            if (id == "SynthSustainCTRL_OFF")
            {
                Mio_SustainPdl.Fill = (Brush)FindResource("CH_Ind_off");
            }
            if(id == "MidiEngine_FileLoadComplete")
            {
                cp_Info.Text = MidiEngine.Copyright;
            }
            if(id == "MidiEngine_SequenceBuilder_Completed")
            {

            }
        }

        public void HandleKeyDown(object sender, KeyEventArgs e) => pianomain.UserControl_KeyDown(sender, e);

        public void HandleKeyUp(object sender, KeyEventArgs e)
        {
            pianomain.UserControl_KeyUp(sender, e);

            #region Studio keyboard shortcuts
            if (e.Key == Key.O && Keyboard.Modifiers.HasFlag(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift))
            {
                cb_OFX_Enble.IsChecked = !cb_OFX_Enble.IsChecked.Value;
            }
            if (e.Key == Key.D && Keyboard.Modifiers.HasFlag(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift))
            {
                cb_DS_Enable.IsChecked = !cb_DS_Enable.IsChecked.Value;
            }
            #endregion

            if (e.Key == Key.Up && Config.PitchOffsets[1] < 12)
            {
                Config.PitchOffsets[1]++;
                pianomain.SetNoteText(Config.PitchOffsets[1]);
            }

            if (e.Key == Key.Down && Config.PitchOffsets[1] > -12)
            {
                Config.PitchOffsets[1]--;
                pianomain.SetNoteText(Config.PitchOffsets[1]);
            }

            if (e.Key == Key.Left)
            {
                Config.PitchOffsets[1] = 0;
                pianomain.SetNoteText(Config.PitchOffsets[1]);
            }

            Transpose = Config.PitchOffsets[1];

        }

        public async void HandleNoteOn_VS_Event(object sender, PKeyEventArgs e, int velocity,int channel=0) => await Dispatcher.InvokeAsync(() => Pianomain_pKeyDown_VelocitySense(sender, e, velocity,channel));

        #endregion

        private void Ch_Indicator10_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ch10_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
