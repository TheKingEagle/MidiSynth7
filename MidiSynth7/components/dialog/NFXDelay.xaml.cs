using MidiSynth7.entities.controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for NFXDelay.xaml
    /// </summary>
    public partial class NFXDelay : Page, IDialogView
    {
        public NFXDelay( MainWindow win,  Grid grid, NFXDelayProfile defaultProf = null)
        {
            InitializeComponent();
            ActiveWindow = win;
            Container = grid;
            NFXProfiles = ActiveWindow.NFXProfiles;
            PopulateSavedNFXProfiles();
            LB_SavedProfiles.SelectedIndex = 0;
            if(defaultProf != null)
            {
                LB_SavedProfiles.SelectedIndex = LB_SavedProfiles.Items.IndexOf(defaultProf);
            }
        }

        private MainWindow ActiveWindow;

        private Grid Container;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public List<NFXDelayProfile> NFXProfiles = new List<NFXDelayProfile>();

        private NFXDelayProfile _backupProfile; //TODO: Implementation backup profile

        public string DialogTitle { get => "Customize NoteFX Chain"; set { return; } }

        public bool HelpRequested { get; set; }

        public bool CanRequestHelp => true;

        public void InvokeHelpRequested(Control sender)
        {
            
            string HelperTitle = "";
            if(sender as DialControl != null)
            {
                HelperTitle = ": " + ((DialControl)sender).Text;
            }
            if(sender.ToolTip != null)
            {
                Dialog.Message(ActiveWindow,Container,sender.ToolTip.ToString(), "Display Help"+HelperTitle,Icons.Info,128,false);
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

        private async void bn_NFXProfDel_Click(object sender, RoutedEventArgs e)
        {
            if(LB_SavedProfiles.SelectedIndex == 0)
            {
                await Dialog.Message(ActiveWindow, Container, "You may not delete this profile.", "Invalid Operation", Icons.Critical, 128);
                return;
            }
            bool? v = await Dialog.Message(ActiveWindow, Container, "Are you sure you want to delete this profile? This cannot be undone.", $"Delete {((NFXDelayProfile)LB_SavedProfiles.SelectedItem).ProfileName}", Icons.Warning, 128,true);
            if (v.HasValue && v==false)
            {
                return;
            }
            NFXProfiles.Remove((NFXDelayProfile)LB_SavedProfiles.SelectedItem);
            PopulateSavedNFXProfiles();
        }

        private void LB_SavedProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedProfiles.SelectedItem == null) return;
            NFXProfileSetEditor((NFXDelayProfile)LB_SavedProfiles.SelectedItem);
            NFXDelayProfile f = (NFXDelayProfile)LB_SavedProfiles.SelectedItem;
            _backupProfile = new NFXDelayProfile()
            {
                Delay = f.Delay,
                ProfileName = f.ProfileName
            };
            _backupProfile.OffsetMap = new List<(int pitch, int decay)>();
            _backupProfile.OffsetMap.AddRange(f.OffsetMap.ToArray());
            ToggleElement(BN_NFX_ResetValues, false);

        }

        private void Lv_steps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lv_steps.SelectedItem == null)
            {
                GB_StepSettings.Header = "Key Setting";
                return;
            }
            var f = new { Step = 0, Pitch = 12, Decay = 20 };
            var item = Cast(f, lv_steps.SelectedItem);
            Dial_NFX_Decay.SetValueSuppressed(item.Decay);
            Dial_NFX_Pitch.SetValueSuppressed(item.Pitch);
            GB_StepSettings.Header = "Key Setting (Editing key #" + item.Step + ")";
        }

        //Compiler trickery at its worst
        private static T Cast<T>(T _, object x) => (T)x;

        private void Dial_NFX_ValChanged(object sender, EventArgs e)
        {
            //profile
            ToggleElement(BN_NFX_ResetValues, true);

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
            ToggleElement(BN_NFX_ResetValues, true);

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

        internal int PopulateSavedNFXProfiles()
        {
            ToggleElement(BN_NFX_ResetValues, false);

            int last = LB_SavedProfiles.SelectedIndex;
            LB_SavedProfiles.Items.Clear();

            foreach (NFXDelayProfile item in NFXProfiles)
            {
                LB_SavedProfiles.Items.Add(item);
            }
            return last;
        }

        private void NFXProfileSetEditor(NFXDelayProfile profile)
        {
            
            gb_NFXProfEditor.IsEnabled = true;
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
            ToggleElement(BN_NFX_ResetValues,true);
            if (e.Key == Key.Enter)
            {
                if (LB_SavedProfiles.SelectedItem == null) return;
                NFXDelayProfile prof = (NFXDelayProfile)LB_SavedProfiles.SelectedItem;
                if (string.IsNullOrWhiteSpace(TB_NFX_profile_name.Text))
                {
                    Dialog.Message(ActiveWindow, Container, "Please enter a meaningful name for this profile.", "Invalid Name", Icons.Warning, 128);
                    TB_NFX_profile_name.Text = prof.ProfileName;
                    return;
                }
                prof.ProfileName = TB_NFX_profile_name.Text;
                PopulateSavedNFXProfiles();
            }
        }

        private void ToggleElement(Control ele,bool enabled)
        {
            ele.IsEnabled = enabled;
            ele.Opacity = enabled ? 1:0.55;
            ele.BorderBrush = enabled ? (SolidColorBrush)TryFindResource("ButtonDefaultBorderBrush") : Brushes.DarkGray;
            //ele.Effect = enabled ? null : new BlurEffect() { Radius = 4 };
        }

        private void BN_NFX_Duplicate_Click(object sender, RoutedEventArgs e)
        {
            if(LB_SavedProfiles.SelectedItem == null)
            {
                return;
            }
            NFXDelayProfile prof = (NFXDelayProfile)LB_SavedProfiles.SelectedItem;
            NFXDelayProfile c = prof.Clone();
            ActiveWindow.NFXProfiles.Add(c);
            ActiveWindow.SaveNFXProfiles();
            PopulateSavedNFXProfiles();
            LB_SavedProfiles.SelectedItem = c;
        }

        private async void BN_NFX_ResetValues_Click(object sender, RoutedEventArgs e)
        {
            bool? md = await Dialog.Message(ActiveWindow, Container, "Revert all unsaved changes to this preset?", "Confirm Revert", Icons.Warning, 128, true);

            if(md.HasValue && md == true)
            {
                Dial_NFX_Interval.SetValueSuppressed(_backupProfile.Delay);
                //Profile
                NFXPopulateSteps(_backupProfile);
                Dial_NFX_StepCount.SetValueSuppressed(_backupProfile.OffsetMap.Count);
                TB_NFX_profile_name.Text = _backupProfile.ProfileName;
                ToggleElement(BN_NFX_ResetValues, false);
            }
        }
    }

}
