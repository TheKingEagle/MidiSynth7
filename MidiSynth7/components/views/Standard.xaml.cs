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
    /// Interaction logic for Standard.xaml
    /// </summary>
    public partial class Standard : Page
    {
        public Standard()
        {
            InitializeComponent();
            
        }

        private void cb_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MIO_bn_stop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void toggleChecked(object sender, RoutedEventArgs e)
        {

        }

        private void MIO_bn_SetSF2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void riffcenter_toggleCheck(object sender, RoutedEventArgs e)
        {

        }

        private void pianomain_pKeyUp(object sender, entities.controls.PKeyEventArgs e)
        {

        }

        private void cp_bnPlay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cp_bnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cp_bnBrowse_Click(object sender, RoutedEventArgs e)
        {

        }

        private void criffenablecheck(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #region RiffCenter Checks
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

        #endregion

        private void cb_mPatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cb_mBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cb_sPatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cb_sBank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CTRL_ValueChanged(object sender, EventArgs e)
        {

        }

        private void BnRiff_Define_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OFX_bn_Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pianomain_pKeyDown(object sender, entities.controls.PKeyEventArgs e)
        {

        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            pianomain.SetNoteText(0);
        }
    }
}
