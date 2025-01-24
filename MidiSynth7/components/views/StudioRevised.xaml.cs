﻿using Microsoft.Win32;
using MidiSynth7.components.dialog;
using MidiSynth7.entities.controls;
using NAudio.SoundFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MidiSynth7.components.views
{
    /// <summary>
    /// Interaction logic for StudioView.xaml
    /// </summary>
    public partial class StudioRevised : Page, ISynthView
    {
        SystemConfig Config;
        MidiEngine MidiEngine;
        MainWindow AppContext;
        private int Transpose;
        private bool mfile_playing = false;
        private List<Ellipse> channelElipses = new List<Ellipse>();
        private List<MainWindow.ChInvk> channelIndicators = new List<MainWindow.ChInvk>();
        int pattern = 0;
        int step = 0;
        int activeCh = -1;
        public bool HaltKeyboardInput { get; private set; }

        public StudioRevised(MainWindow context, ref SystemConfig config, ref MidiEngine engine)
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

            UpdateInstrumentSelection(config);
            #endregion

            NFXProfileUpdate();

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


            //Cb_SequencerProfile.ItemsSource = AppContext.Tracks;
            //Cb_SequencerProfile.SelectedIndex = 0;
        }

        private void UpdateInstrumentSelection(SystemConfig config)
        {
            cb_mBank.Items.Clear();
            cb_dkitlist.Items.Clear();
            foreach (Bank item in AppContext.ActiveInstrumentDefinition.Banks.Where(xb => xb.Index != 127))
            {
                cb_mBank.Items.Add(item);
                //OFX_I1BankSel.Items.Add(item);
                //OFX_I2BankSel.Items.Add(item);
            }
            var dkits = AppContext.ActiveInstrumentDefinition.Banks.FirstOrDefault(xb => xb.Index == 127);
            if (dkits != null)
            {
                foreach (NumberedEntry item in dkits?.Instruments)
                {
                    cb_dkitlist.Items.Add(item);
                }
            }
            else
            {
                dkits = AppContext.ActiveInstrumentDefinition.Banks.FirstOrDefault(xb => xb.Index == 128);
                if (dkits == null)
                {
                    Dialog.Message(AppContext, AppContext.GR_OverlayContent, "The drumkit bank could not be found. Ensure its 127 or 128.", "Where is your drumkit?", Icons.Warning);
                }
                else
                {
                    foreach (NumberedEntry item in dkits?.Instruments)
                    {
                        cb_dkitlist.Items.Add(item);
                    }
                }

            }
            //OFX_I1BankSel.SelectedIndex = 0;
            //OFX_I2BankSel.SelectedIndex = 0;
            cb_mBank.SelectedIndex = 0;
            cb_dkitlist.SelectedIndex = 0;
            config.ChannelBanks[0] = (cb_mBank.Items.Count >= config.ChannelBanks[0]) ? config.ChannelBanks[0] : 0;
            config.ChannelInstruments[0] = (cb_mPatch.Items.Count >= config.ChannelInstruments[0]) ? config.ChannelInstruments[0] : 0;
            
            config.ChannelInstruments[9] = (cb_dkitlist.Items.Count >= config.ChannelInstruments[9]) ? config.ChannelInstruments[9] : 0;
            //OFX_I1BankSel.SelectedIndex = mainCG.IT_Octave3BankIndex1;
            //OFX_I2BankSel.SelectedIndex = mainCG.IT_Octave3BankIndex2;
            cb_mBank.SelectedIndex = config.ChannelBanks[0];

            cb_mPatch.SelectedIndex = config.ChannelInstruments[0];

            cb_dkitlist.SelectedIndex = config.ChannelInstruments[9];
        }

        async Task FlashChannelActivity(int index)
        {
            Action invoker = delegate ()
            {
                AppContext.channelElipses[index].Fill = (Brush)FindResource("CH_IND_ON");
                AppContext.channelIndicators[index].CounterReset();
            };
            await Dispatcher.BeginInvoke(invoker);
        }

        private void UpdateMIDIControls(int channel = -1)//TODO: Be Channel specific
        {

            if (MidiEngine != null)
            {
                if (channel < 0)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        FlashChannelActivity(i);
                        MidiEngine.MidiNote_SetReverb(i, CTRL_Reverb.Value);
                        MidiEngine.MidiNote_SetChorus(i, CTRL_Chorus.Value);
                        MidiEngine.MidiNote_SetModulation(i, CTRL_Modulation.Value);
                        MidiEngine.MidiNote_SetPan(i, CTRL_Balance.Value);
                        MidiEngine.MidiNote_SetControl(Sanford.Multimedia.Midi.ControllerType.Volume, i, CTRL_Volume.Value);
                    }
                }
                else
                {
                    if (channel > 15)
                    {
                        throw new Exception("Invalid channel specified.");
                    }
                    FlashChannelActivity(channel);
                    MidiEngine.MidiNote_SetReverb(channel, CTRL_Reverb.Value);
                    MidiEngine.MidiNote_SetChorus(channel, CTRL_Chorus.Value);
                    MidiEngine.MidiNote_SetModulation(channel, CTRL_Modulation.Value);
                    MidiEngine.MidiNote_SetPan(channel, CTRL_Balance.Value);
                    MidiEngine.MidiNote_SetControl(Sanford.Multimedia.Midi.ControllerType.Volume, channel, CTRL_Volume.Value);
                }


            }

        }

        private void Cb_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cb_internalsf2.IsChecked.Value)
            {
                Config.ActiveOutputDeviceIndex = cb_Devices.SelectedIndex;
                AppContext.GenerateMIDIEngine(this, ((NumberedEntry)cb_Devices.SelectedItem).Index);
                //Set definition
                AppContext.ActiveInstrumentDefinition = AppContext.Definitions.FirstOrDefault(x => x.AssociatedDeviceIndex == (((NumberedEntry)cb_Devices.SelectedItem)?.Index ?? -1)) ?? AppContext.Definitions[0];//associated or default
                UpdateInstrumentSelection(AppContext.AppConfig);
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
            gb_ds.IsEnabled = cb_DS_Enable.IsChecked.Value;
            if (Config != null) Config.EnforceInstruments = CB_EnforceInstruments?.IsChecked ?? true;
        }

        private void MIO_bn_SetSF2_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Composer

        private void cp_bnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!mfile_playing)
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
            if (of.ShowDialog().Value)
            {
                if (MidiEngine != null)
                {
                    MidiEngine.MidiFile_Add(of.FileName);

                }
            }
        }

        #endregion

        #region Riff Center


        private void BnRiff_Define_Click(object sender, RoutedEventArgs e)
        {
            CB_Sequencer_Check.IsChecked = false;
            

        }


        // Class-level field for CancellationTokenSource
        private CancellationTokenSource cancellationTokenSource;

        private async void Riffcenter_toggleCheck(object sender, RoutedEventArgs e)
        {
            gb_riff.IsEnabled = true;
            bool check = CB_Sequencer_Check.IsChecked ?? false;
            

            int ticksPerDot = 6; // TODO: Ensure this value is set by the sequence
            int DotDuration = (int)((float)(2500 / (float)(Dial_RiffTempo.Value * 1000)) * (ticksPerDot * 1000));
            Console.WriteLine("DotDuration:" + DotDuration);

            // Cancel any previous task if it was running
            cancellationTokenSource?.Cancel();

            // Create a new CancellationTokenSource
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            // Play the selected sequence
            if (check)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {

                            if (!check || token.IsCancellationRequested) { break; }
                            Dispatcher.InvokeAsync(() => LC_PatternNumber.SetLight(pattern));
                            for (step = 0; step < LC_PatternStep.Rows * LC_PatternStep.Columns; step++)
                            {
                                Dispatcher.InvokeAsync(() => check = CB_Sequencer_Check.IsChecked.Value);
                                if (!check || token.IsCancellationRequested) return;
                                Dispatcher.InvokeAsync(() => LC_PatternStep.SetLight(step));

                                
                                    MetronomeTick(step); // oh hey
                                
                                // TODO: Further process the sequence parameters within it.
                                Dispatcher.InvokeAsync(() => DotDuration = (int)((float)(2500 / (float)(Dial_RiffTempo.Value * 1000)) * (ticksPerDot * 1000)));
                                System.Threading.Thread.Sleep(DotDuration); // Still not ideal, but better
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Handle cancellation if necessary
                    }
                }, token);
            }

            if (!check)
            {
                // Cancel the running task
                cancellationTokenSource?.Cancel();

                LC_PatternNumber.SetLight(-1);
                LC_PatternStep.SetLight(-1);
                MIO_bn_stop_Click(this, new RoutedEventArgs());
            }
        }


        private void MetronomeTick(int step)
        {
            if (step > 0 && step != LC_PatternStep.Rows * LC_PatternStep.Columns)
            {
                if (step % LC_PatternStep.Marker == 0)//TODO replace with value of beatsPerRow in pattern editor
                {
                    AppContext.MidiEngine.MidiNote_Play(9, 42, 44, false);
                }
                if (step % LC_PatternStep.Marker == 1)//TODO replace with value of beatsPerRow in pattern editor
                {
                    AppContext.MidiEngine.MidiNote_Stop(9, 42, false);
                }
            }
            if (step == 0 || step % LC_PatternStep.Columns == 0)
            {
                if (step % LC_PatternStep.Marker == 0)//TODO replace with value of beatsPerRow in pattern editor
                {
                    AppContext.MidiEngine.MidiNote_Play(9, 46, 44, false);
                }

            }
            if (step == 1 || step == (LC_PatternStep.Rows * LC_PatternStep.Columns) + 1)//TODO replace with value of beatsPerRow in pattern editor
            {
                AppContext.MidiEngine.MidiNote_Stop(9, 46, false);
            }
        }
        #endregion

        #region Bank/Instrument selection UI events

        private void Cb_mPatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_mBank.SelectedItem == null) { return; }
            if (cb_mPatch.SelectedItem == null) { return; }
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
            cb_mPatch.SelectedIndex = 0;
        }

        

        private void Cb_dkit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion

        private void CTRL_ValueChanged(object sender, EventArgs e)
        {
            UpdateMIDIControls(activeCh);
            if (Config != null)
            {

                Config.ChannelReverbs[activeCh < 0 ? 16 : activeCh] = CTRL_Reverb.Value;
                Config.ChannelVolumes[activeCh < 0 ? 16 : activeCh] = CTRL_Volume.Value;
                Config.ChannelModulations[activeCh < 0 ? 16 : activeCh] = CTRL_Modulation.Value;
                Config.ChannelChoruses[activeCh < 0 ? 16 : activeCh] = CTRL_Chorus.Value;
                Config.ChannelPans[activeCh < 0 ? 16 : activeCh] = CTRL_Balance.Value;
                Config.PitchOffsets[0] = CTRL_Octave.Value;//global octave

            }
        }

        private void OFX_bn_Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Piano keys

        private void Pianomain_pKeyDown(object sender, PKeyEventArgs e)
        {

            if (MidiEngine != null)
            {
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
                if (CB_Sequencer_Check.IsChecked.Value)
                {
                    //return;
                }
                if (cb_DS_Enable.IsChecked.Value)
                {
                    NumberedEntry dkit = (NumberedEntry)cb_dkitlist.SelectedItem;
                    MidiEngine.MidiNote_SetProgram(127, dkit.Index, 9);
                    MidiEngine.MidiNote_Play(9, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);
                    return;
                }
                
                
                if (Config.EnforceInstruments)
                {
                    MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 0);
                }
                //MidiEngine.MidiNote_SetPan(0, CTRL_Balance.Value);
                MidiEngine.MidiNote_Play(0, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, CTRL_Volume.Value);

            }
        }

        private void Pianomain_pKeyDown_VelocitySense(object sender, PKeyEventArgs e, int velocity, int channel = 0)
        {

            if (MidiEngine != null)
            {
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
                if (CB_Sequencer_Check.IsChecked.Value)
                {
                    //return;
                }
                if (cb_DS_Enable.IsChecked.Value)
                {
                    NumberedEntry dkit = (NumberedEntry)cb_dkitlist.SelectedItem;
                    MidiEngine.MidiNote_SetProgram(127, dkit.Index, 9);
                    MidiEngine.MidiNote_Play(9, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);
                    return;
                }
               
               
                if (channel == 0)
                {
                    if (Config.EnforceInstruments)
                    {
                        MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, 0);
                    }
                    //MidiEngine.MidiNote_SetPan(0, CTRL_Balance.Value);
                }
                MidiEngine.MidiNote_Play(channel, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value, velocity);

            }
        }

        private void Pianomain_pKeyUp(object sender, PKeyEventArgs e)
        {
            if (MidiEngine != null)
            {
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
                if (CB_Sequencer_Check.IsChecked.Value)
                {
                    //return;
                }
                if (cb_DS_Enable.IsChecked.Value)
                {

                    MidiEngine.MidiNote_Stop(9, Transpose + e.KeyID + 12 + 12 * CTRL_Octave.Value);
                    return;
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
            await Dispatcher.InvokeAsync(() => pianomain.UnLightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value));

            if (e.ChannelMssge.Data2 > 0)
            {
                if (e.ChannelMssge.MidiChannel == 0)
                {
                    pianomain.LightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value);
                }
                if (e.ChannelMssge.MidiChannel != 0 && e.ChannelMssge.MidiChannel != 9)
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
            if (cb_NFX_Enable.IsChecked.Value)
            {
#pragma warning disable CS4014 // need to continue regardless of state. because I said so 🙃
                Dispatcher.InvokeAsync(() => PlayDelayedNFX((Bank)cb_mBank.SelectedItem, (NumberedEntry)cb_mPatch.SelectedItem, e.ChannelMssge.MidiChannel, e.ChannelMssge.Data1, e.ChannelMssge.Data2, AppContext.ActiveNFXProfile.Delay, AppContext.ActiveNFXProfile.OffsetMap.Count));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async void HandleNoteOffEvent(object sender, NoteEventArgs e)
        {
            await FlashChannelActivity(e.ChannelMssge.MidiChannel);
            pianomain.UnLightKey(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value);

            if (cb_NFX_Enable.IsChecked.Value)
            {
#pragma warning disable CS4014 // 😏
                Dispatcher.InvokeAsync(() => StopDelayedNFX(e.ChannelMssge.MidiChannel, e.ChannelMssge.Data1, AppContext.ActiveNFXProfile.Delay, AppContext.ActiveNFXProfile.OffsetMap.Count));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            }
            //Pianomain_pKeyUp(sender, new PKeyEventArgs(e.ChannelMssge.Data1 - 12 - Transpose - 12 * CTRL_Octave.Value));
        }

        public void HandleEvent(object sender, EventArgs e, string id = "generic")
        {
            void InsDefUpdate()
            {
                AppContext.ActiveInstrumentDefinition = AppContext.Definitions.FirstOrDefault(x => x.AssociatedDeviceIndex == ((NumberedEntry)cb_Devices.SelectedItem).Index) ?? AppContext.Definitions[0];//associated or default
                UpdateInstrumentSelection(AppContext.AppConfig);
            }
            Console.WriteLine("EventID: {0}", id);
            switch (id)
            {
                case "RefMIDIEngine": AppContext.GenerateMIDIEngine(this, ((NumberedEntry)cb_Devices.SelectedItem).Index); break;
                case "RefMainWin": AppContext = (MainWindow)sender; break;
                case "MTaskWorker": MidiEngine = AppContext.MidiEngine; break;
                case "RefAppConfig": Config = AppContext.AppConfig; break;
                case "SynthSustainCTRL_ON": Mio_SustainPdl.Fill = (Brush)FindResource("CH_IND_ON"); break;
                case "SynthSustainCTRL_OFF": Mio_SustainPdl.Fill = (Brush)FindResource("CH_IND_OFF"); break;
                case "MidiEngine_FileLoadComplete": cp_Info.Text = MidiEngine.Copyright; break;
                case "MidiEngine_SequenceBuilder_Completed": break;
                case "InsDEF_Changed": InsDefUpdate(); break;
                case "RefNFXDelay": NFXProfileUpdate(); break;
                case "TrSeqUpdate":
                    //Cb_SequencerProfile.ItemsSource = AppContext.Tracks;
                    //Cb_SequencerProfile.SelectedIndex = 0;
                    ; break;
                default:
                    Console.WriteLine("Unrecognized event string: {0}... lol", id);
                    break;
            }
        }

        private void NFXProfileUpdate()
        {
            cb_NFX_Dropdown.Items.Clear();
            foreach (NFXDelayProfile item in AppContext.NFXProfiles)
            {
                cb_NFX_Dropdown.Items.Add(item);
            }
            cb_NFX_Dropdown.SelectedIndex = 0;
        }

        public void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (HaltKeyboardInput) return;
            pianomain.UserControl_KeyDown(sender, e);
        }

        public void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (HaltKeyboardInput) return;
            pianomain.UserControl_KeyUp(sender, e);

            #region Studio keyboard shortcuts
            
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

        public async void HandleNoteOn_VS_Event(object sender, PKeyEventArgs e, int velocity, int channel = 0) => await Dispatcher.InvokeAsync(() => Pianomain_pKeyDown_VelocitySense(sender, e, velocity, channel));

        #endregion

        #region NoteFX Functions

        private async Task PlayDelayedNFX(Bank bank, NumberedEntry patch, int ch, int note, int velocity, int delay, int count)
        {
            /*
                    The only possible channels that I can play from on the keyboard is 0, and 1.

                    ch1. Default instrument
                    ch2. Split instrument

                    If I want 6 possible steps, my map will have to look like this.

                    0. {02,04,06,08,10,12}
                    1. {03,05,07,11,13,15}
                       Channel 0 is reserved for default note, 
                       Channel 1 is reserved for split note
                       Channel 9 is reserved for percussion
             
             */
            if (MidiEngine == null) return;
            if (count > 6 || count < 1)
            {
                throw new ArgumentException("Count must be no more than 6, no less than 1.");
            }

            async Task t()
            {
                // int[CHANNEL INDEX, STEP INDEX];
                int[,] channelMapper = new int[15, 6] {
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 9, 9, 9, 9, 9, 9 },
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                };


                for (int i = 0; i < count; i++)
                {
                    await Task.Delay(delay);
                    if (Config.EnforceInstruments)
                    {
                        MidiEngine.MidiNote_SetProgram(bank.Index, patch.Index, channelMapper[ch, i]);
                    }
                    int velo = velocity - (int)(velocity * (float)((float)AppContext.ActiveNFXProfile.OffsetMap[i].decay / 100));

                    MidiEngine.MidiNote_Play(channelMapper[ch, i], note + AppContext.ActiveNFXProfile.OffsetMap[i].pitch, velo, false);
                    Dispatcher.Invoke(() =>
                    {
                        int zkb = note + AppContext.ActiveNFXProfile.OffsetMap[i].pitch - 12 - Transpose - (12 * CTRL_Octave.Value);
                        pianomain.ALTLightKey(zkb);
                    });
                    
                    await Dispatcher.InvokeAsync(() => FlashChannelActivity(channelMapper[ch, i]));

                }
            }
            await Task.Run(() => t());
        }

        private async Task StopDelayedNFX(int ch, int note, int delay, int count)
        {
            if (MidiEngine == null) return;
            if (count > 6 || count < 1)
            {
                throw new ArgumentException("Count must be no more than 6, no less than 1.");
            }
            async Task t()
            {

                int[,] channelMapper = new int[15, 6] {
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 9, 9, 9, 9, 9, 9 },
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                    { 1, 2, 3, 4, 5, 6},
                };

                for (int i = 0; i < count; i++)
                {
                    await Task.Delay(delay);
                    MidiEngine.MidiNote_Stop(channelMapper[ch, i], note + AppContext.ActiveNFXProfile.OffsetMap[i].pitch, false);

                    Dispatcher.Invoke(() =>
                    {
                        int zkb = note + AppContext.ActiveNFXProfile.OffsetMap[i].pitch - 12 - Transpose - (12 * CTRL_Octave.Value);
                        pianomain.UnLightKey(zkb);
                    });
                    await Dispatcher.InvokeAsync(() => FlashChannelActivity(channelMapper[ch, i]));
                }
            }
            await Task.Run(() => t());
        }

        #endregion

        private void BN_CustomizeDelay_Click(object sender, RoutedEventArgs e)
        {
            //use active profile!
            AppContext.ShowNFX();
        }

        private void Dials_TextPromptStateChanged(object sender, EventArgs e)
        {
            HaltKeyboardInput = ((DialControl)sender).InputCaptured;
        }

        private void Cb_nfx_ProfileSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppContext == null) return;
            if (cb_NFX_Dropdown.SelectedItem == null) return;
            var item = cb_NFX_Dropdown.SelectedItem;

            AppContext.ActiveNFXProfile = (NFXDelayProfile)item;
            if (AppContext.ActiveNFXProfile == null)
            {
                AppContext.ActiveNFXProfile = AppContext.NFXProfiles[0];
            }

            //update all onscreen datas

            Dial_NFX_Interval.SetValueSuppressed(AppContext.ActiveNFXProfile.Delay);
            Dial_NFX_StepCount.SetValueSuppressed(AppContext.ActiveNFXProfile.OffsetMap.Count);

            lv_steps.Items.Clear();
            for (int i = 0; i < AppContext.ActiveNFXProfile.OffsetMap.Count; i++)
            {
                lv_steps.Items.Add(new { Step = i + 1, Pitch = AppContext.ActiveNFXProfile.OffsetMap[i].pitch, Decay = AppContext.ActiveNFXProfile.OffsetMap[i].decay });
            }
        }

        private void Cb_SequencerProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppContext != null)
            {
                //AppContext.ActiveSequence = Cb_SequencerProfile.SelectedItem as TrackerSequence;
            }
        }

        private void LC_PatternStep_LightIndexChanged(object sender, LightCellEventArgs e)
        {
            step = e.LightIndex;
        }

        private void ChInd_MouseUp(object sender, MouseButtonEventArgs e)
        {

            Ellipse invoked = sender as Ellipse;
            if (invoked != null)
            {
                if (invoked.Tag != null && int.TryParse(invoked.Tag.ToString(), out int channelId))
                {
                    if (activeCh == channelId)
                    {
                        activeCh = -1;
                    }
                    else
                    {
                        activeCh = channelId;
                    }
                }
            }

            SetCHIndMarker();
            CTRL_Balance.SetValueSuppressed(Config.ChannelPans[activeCh < 0 ? 16 : activeCh]);
            CTRL_Chorus.SetValueSuppressed(Config.ChannelChoruses[activeCh < 0 ? 16 : activeCh]);
            CTRL_Volume.SetValueSuppressed(Config.ChannelVolumes[activeCh < 0 ? 16 : activeCh]);
            CTRL_Reverb.SetValueSuppressed(Config.ChannelReverbs[activeCh < 0 ? 16 : activeCh]);
        }

        private void SetCHIndMarker()
        {
            foreach (var ch in AppContext.channelElipses)
            {
                if (int.TryParse(ch.Tag.ToString(), out int channelID))
                {
                    if (channelID == activeCh)
                    {
                        ch.Stroke = (Brush)FindResource("CH_IND_MARKER");
                        GB_Controllers.Header = $"Controllers [Channel: {activeCh + 1}]";
                    }
                    else
                    {
                        ch.Stroke = (Brush)FindResource("CH_IND_STROKE");
                    }
                    if (activeCh == -1)
                    {
                        GB_Controllers.Header = $"Controllers [Channel: All]";
                    }
                }

            }
        }

        private void LC_PatternNumber_LIC(object sender, LightCellEventArgs e)
        {
            pattern = e.LightIndex;
            step = -1;
        }

        private void Dial_MarkerValueChange(object sender, EventArgs e)
        {
            LC_PatternStep.Columns = Dial_Marker_NotesPerMeasure.Value;
            LC_PatternStep.Rows = Dial_Marker_Measures.Value;
            LC_PatternStep.Marker = Dial_Marker_Subdivision.Value;
        }
    }
}
