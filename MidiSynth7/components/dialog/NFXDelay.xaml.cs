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

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for NFXDelay.xaml
    /// </summary>
    public partial class NFXDelay : Page, IDialogView
    {
        public NFXDelay( MainWindow win,  Grid grid)
        {
            InitializeComponent();
            ActiveWindow = win;
            Container = grid;
            NFXProfiles = ActiveWindow.NFXProfiles;
            PopulateSavedNFXProfiles();
            LB_SavedProfiles.SelectedIndex = 0;
        }

        private MainWindow ActiveWindow;

        private Grid Container;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public List<NFXDelayProfile> NFXProfiles = new List<NFXDelayProfile>();

        private NFXDelayProfile _backupProfile; //TODO: Implementation backup profile

        public string DialogTitle { get => "Customize NoteFX Delay"; set { return; } }

        public bool HelpRequested { get; set; }

        public bool CanRequestHelp => true;

        public void InvokeHelpRequested(Control sender)
        {
            if(sender.ToolTip != null)
            {
                MessageBox.Show(sender.ToolTip.ToString(), "Context" );
            }
        }

        #region NFX Logic
        private void bn_NFXProfSave_Click(object sender, RoutedEventArgs e)
        {
            //Invoke event
            ActiveWindow.SaveNFXProfiles();
            ActiveWindow.currentView.HandleEvent(this, new EventArgs(), "RefNFXDelay");
            DialogClosed?.Invoke(this, new DialogEventArgs(ActiveWindow, Container));
        }


        private void bn_NFXProfAdd_Click(object sender, RoutedEventArgs e)
        {
            //TODO: new profile
            NFXProfiles.Add(new NFXDelayProfile() { Delay = 280, OffsetMap = new List<(int pitch, int decay)> { (0, 0), (0, 50) }, ProfileName = "New Profile" });
            PopulateSavedNFXProfiles();
        }

        private void bn_NFXProfDel_Click(object sender, RoutedEventArgs e)
        {
            if(LB_SavedProfiles.SelectedIndex == 0)
            {
                Dialog.Message(ActiveWindow, Container, "You may not delete this profile.", "Invalid Operation", Icons.Critical, 128);
                return;
            }

            NFXProfiles.Remove((NFXDelayProfile)LB_SavedProfiles.SelectedItem);
            PopulateSavedNFXProfiles();
        }

        private void LB_SavedProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXProfileSetEditor((NFXDelayProfile)LB_SavedProfiles.SelectedItem);
        }

        private void Lv_steps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lv_steps.SelectedItem == null)
            {
                GB_StepSettings.Header = "Step Setting";
                return;
            }
            var f = new { Step = 0, Pitch = 12, Decay = 20 };
            var item = Cast(f, lv_steps.SelectedItem);
            Dial_NFX_Decay.SetValueSuppressed(item.Decay);
            Dial_NFX_Pitch.SetValueSuppressed(item.Pitch);
            GB_StepSettings.Header = "Step Setting (Editing step #" + item.Step + ")";
        }

        //Compiler trickery at its worst
        private static T Cast<T>(T _, object x) => (T)x;

        private void Dial_NFX_ValChanged(object sender, EventArgs e)
        {
            //profile
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXDelayProfile prof = (NFXDelayProfile)LB_SavedProfiles.SelectedItem;
            prof.Delay = Dial_NFX_Interval.Value;
            List<(int pitch, int decay)> newSteps = new List<(int pitch, int decay)>();
            for (int i = 0; i < Dial_NFX_StepCount.Value; i++)
            {
                if (prof.OffsetMap.Count > i)
                {
                    newSteps.Add(prof.OffsetMap[i]);
                }
                else
                {
                    newSteps.Add((0, 0));

                }
            }
            prof.OffsetMap = newSteps;
            NFXPopulateSteps(prof);
        }

        private void Dial_NFX_STEP_ValChanged(object sender, EventArgs e)
        {
            if (lv_steps.SelectedIndex < 0) return;
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXDelayProfile prof = (NFXDelayProfile)LB_SavedProfiles.SelectedItem;

            var offset = prof.OffsetMap[lv_steps.SelectedIndex];
            prof.OffsetMap.Remove(offset);
            offset.decay = Dial_NFX_Decay.Value;
            offset.pitch = Dial_NFX_Pitch.Value;
            prof.OffsetMap.Insert(lv_steps.SelectedIndex, offset);
            //pray
            int index = lv_steps.SelectedIndex;
            lv_steps.Items[index] = new { Step = lv_steps.SelectedIndex + 1, Pitch = offset.pitch, Decay = offset.decay };
            lv_steps.SelectedIndex = index;//pain!
        }

        internal void PopulateSavedNFXProfiles()
        {
            LB_SavedProfiles.Items.Clear();

            foreach (NFXDelayProfile item in NFXProfiles)
            {
                LB_SavedProfiles.Items.Add(item);
            }
            LB_SavedProfiles.SelectedIndex = LB_SavedProfiles.Items.Count - 1;
        }

        private void NFXProfileSetEditor(NFXDelayProfile profile)
        {
            
            gb_NFXProfEditor.IsEnabled = true;
            _backupProfile = profile;

            TB_NFX_profile_name.Text = profile.ProfileName;
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
        #endregion

        private void TB_NFX_profile_name_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (LB_SavedProfiles.SelectedItem == null) return;
                NFXDelayProfile prof = (NFXDelayProfile)LB_SavedProfiles.SelectedItem;
                prof.ProfileName = TB_NFX_profile_name.Text;
                PopulateSavedNFXProfiles();
            }
        }
    }

}
