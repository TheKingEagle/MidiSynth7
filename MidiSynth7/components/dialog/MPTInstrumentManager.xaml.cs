using MidiSynth7.entities.controls;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
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
using System.Xaml;

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for MPTInstrumentManager.xaml
    /// </summary>
    public partial class MPTInstrumentManager : Page, IDialogView
    {
        public MPTInstrumentManager()
        {
            InitializeComponent();
        }

        MainWindow AppContext;
        Grid Overlay;
        TrackerSequence Sequence;
        InstrumentDefinition actdef;

        public MPTInstrumentManager(TrackerSequence ts, MainWindow _AppContext, Grid _Container)
        {
            Sequence = ts;
            AppContext = _AppContext;
            Overlay = _Container;
            InitializeComponent();
            cb_ChannelSelection.Items.Clear();
            LB_InstrumentMap.Items.Clear();
            cb_Devices.Items.Clear();
            cb_sBank.Items.Clear();
            cb_sPatch.Items.Clear();
            Tb_insName.IsEnabled = false;
            cb_ChannelSelection.IsEnabled = false;
            cb_Devices.IsEnabled = false;
            cb_sPatch.IsEnabled = false;
            cb_sBank.IsEnabled = false;
            cb_Devices.Items.Add(new NumberedEntry(-1,"<Active Output Device>"));
            foreach (NumberedEntry odev in _AppContext.OutputDevices)
            {
                cb_Devices.Items.Add(odev);
            }
            foreach  (TrackerInstrument ins in ts.Instruments)
            {
                LB_InstrumentMap.Items.Add(ins);
            }
            for (int i = -1; i < 16; i++)
            {
                cb_ChannelSelection.Items.Add(new NumberedEntry(i, i <= -1 ? "<Mapped>" : $"Channel {i+1}"));
            }

        }

        public string DialogTitle { get => "Allocate Instruments"; set => throw new NotImplementedException(); }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        private void bn_SaveInsMap_Click(object sender, RoutedEventArgs e)
        {
            TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
            Sequence.Instruments.Remove(ti);
            Sequence.Instruments.Add(ti);
            Sequence.SaveSequence();

            AppContext.MidiEngine.OpenOutputDevice(ti.DeviceIndex, ti.DeviceIndex);
            DialogClosed?.Invoke(this, new DialogEventArgs(AppContext, Overlay));
        }

        private void Tb_insName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
            ti.DisplayName = Tb_insName.Text;
        }

        private void cb_ChannelSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
            ti.ChannelIndex = (cb_ChannelSelection.SelectedItem as NumberedEntry).Index;
        }

        private void cb_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
            UpdateInstrumentLists(ti);
            if(cb_Devices.SelectedItem != null)
            {
                int index = (cb_Devices.SelectedItem as NumberedEntry).Index;
                ti.DeviceIndex = index;
                
                
            }
        }

        private void cb_sBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Bank b = cb_sBank.SelectedItem as Bank;
            if (b != null)
            {
                cb_sPatch.Items.Clear();
                foreach (NumberedEntry item in b.Instruments)
                {
                    cb_sPatch.Items.Add(item);
                }
                TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
                ti.Bank = b.Index;
            }
            
        }

        private void cb_sPatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cb_sPatch.SelectedItem != null)
            {
                TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
                ti.Instrument = (cb_sPatch.SelectedItem as NumberedEntry).Index;
            }
            
        }

        private void LB_InstrumentMap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(LB_InstrumentMap.SelectedItems.Count == 0)
            {
                Tb_insName.IsEnabled = false;
                cb_ChannelSelection.IsEnabled = false;
                cb_Devices.IsEnabled = false;
                cb_sPatch.IsEnabled = false;
                cb_sBank.IsEnabled = false;
            } else
            {
                Tb_insName.IsEnabled = true;
                cb_ChannelSelection.IsEnabled = true;
                cb_Devices.IsEnabled = true;
                cb_sPatch.IsEnabled = true;
                cb_sBank.IsEnabled = true;
                TrackerInstrument ti = LB_InstrumentMap.SelectedItem as TrackerInstrument;
                if (ti != null)
                {
                    Tb_insName.Text = ti.DisplayName;
                    cb_ChannelSelection.SelectedItem = cb_ChannelSelection.Items.OfType<NumberedEntry>()
                        .FirstOrDefault(x => x.Index == ti.ChannelIndex);
                    
                    if(ti.DeviceIndex == -1)
                    {
                        cb_Devices.SelectedIndex = 0;
                    } else
                    {
                        
                        cb_Devices.SelectedItem = cb_Devices.Items.OfType<NumberedEntry>()
                            .FirstOrDefault(x => x.Index == ti.DeviceIndex);
                    }
                    
                }
                UpdateInstrumentLists(ti);

            }
        }

        private void UpdateInstrumentLists(TrackerInstrument ti)
        {
            
            if (ti != null)
            {
                cb_sBank.Items.Clear();
                cb_sPatch.Items.Clear();
                //populate the banks based on selected CB_DEVICES
                actdef = AppContext.Definitions.FirstOrDefault(x => x.AssociatedDeviceIndex == (((NumberedEntry)cb_Devices.SelectedItem)?.Index ?? -1)) ?? AppContext.Definitions[0];

                if (actdef != null)
                {
                    foreach (var item in actdef.Banks)
                    {
                        cb_sBank.Items.Add(item);
                    }
                }
                cb_sBank.SelectedItem = cb_sBank.Items.OfType<Bank>()
                    .FirstOrDefault(x => x.Index == ti.Bank);

                cb_sPatch.SelectedItem = cb_sPatch.Items.OfType<NumberedEntry>()
                    .FirstOrDefault(x => x.Index == ti.Instrument);
            }
        }

        private void bn_AddInstrument_Click(object sender, RoutedEventArgs e)
        {
            if(LB_InstrumentMap.Items.Count >= 255)
            {
                Dialog.Message(AppContext, Gr_Content, "Maximum number of instruments reached", "ERROR", Icons.Critical);
                return;
            }
            TrackerInstrument ins = new TrackerInstrument((byte)LB_InstrumentMap.Items.Count, -1, -1, 0, 0, "New Instrument");
            LB_InstrumentMap.Items.Add(ins);
            Sequence.Instruments.Add(ins);
        }
    }
}
