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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page, IDialogView
    {
        Dialog _parent;
        public Settings(SystemConfig appConfig, MainWindow appWindow, Dialog parent)
        {
            InitializeComponent();
            AppConfig = appConfig;
            AppContext = appWindow;
            _parent = parent;
            foreach (NumberedEntry item in AppContext.InputDevices)
            {
                cm_InputDevices.Items.Add(item);
                cm_InputDevices2.Items.Add(item);
            }
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

            //set in device
            NumberedEntry e1 = AppContext.InputDevices.FirstOrDefault(x => x.Index == AppConfig.ActiveInputDeviceIndex);
            NumberedEntry e2 = AppContext.InputDevices.FirstOrDefault(x => x.Index == AppConfig.ActiveInputDevice2Index);
            if (e1 != null)
            {
                cm_InputDevices.SelectedItem = e1;
            }
            if (e2 != null)
            {
                cm_InputDevices2.SelectedItem = e2;
            }

        }
        public SystemConfig AppConfig;
        public MainWindow AppContext;

        private List<(string name, bool value)> checkstates = new List<(string name, bool value)>();

        public string DialogTitle { get => "Configuration"; set { return; } }
        public bool HelpRequested { get => false; set { return; } }
        public bool CanRequestHelp => false;


        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        #region Setting logic

        private void Bn_cfgSave_Click(object sender, RoutedEventArgs e)
        {
            AppConfig.ActiveInputDeviceIndex = ((NumberedEntry)cm_InputDevices.SelectedItem).Index;
            AppConfig.ActiveInputDevice2Index = ((NumberedEntry)cm_InputDevices2.SelectedItem).Index;
            AppContext.currentView.HandleEvent(AppContext, new EventArgs(), "RefMIDIEngine");
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
            if (AppConfig.DisplayMode == AppContext.CurrentViewDM)
            {
                AppContext.currentView.HandleEvent(AppContext, new EventArgs(), "RefMainWin");
                AppContext.currentView.HandleEvent(sender, new EventArgs(), "RefAppConfig");
            }
            if (AppConfig.DisplayMode != AppContext.CurrentViewDM)
            {
                AppContext.SwitchView(AppConfig.DisplayMode);
            }
            AppContext.SaveConfig();
            DialogClosed?.Invoke(this, new DialogEventArgs(AppContext, AppContext.GR_OverlayContent));

        }
        #endregion

        private void Bn_cfgLaunchInsdef_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Improve consistency later when I'm not time constrained.
            _parent.ShowDialog(new InstrumentDefinitions(AppContext, AppContext.GR_OverlayContent, _parent), AppContext, AppContext.GR_OverlayContent);
        }
    }
}
