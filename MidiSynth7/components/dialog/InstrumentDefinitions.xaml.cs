using MidiSynth7.entities.controls;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for InstrumentDefinitions.xaml
    /// </summary>
    public partial class InstrumentDefinitions : Page, IDialogView
    {
        private bool _insDefNameDirty;

        private MainWindow _appContext;
        private Grid _container;
        private Dialog _parent;

        public InstrumentDefinitions(MainWindow AppContext, Grid Container, Dialog Parent)
        {
            InitializeComponent();
            _appContext = AppContext;
            _container = Container;
            _parent = Parent;

            foreach (NumberedEntry item in _appContext.OutputDevices)
            {
                cm_InsDefDevices.Items.Add(item);
            }
            //add unused
            cm_InsDefDevices.Items.Insert(0, new NumberedEntry(-1, "Unassigned"));
            cm_InsDefDevices.SelectedIndex = 0;
            PopulateSavedDefinitions();
        }

        #region Interface
        public string DialogTitle { get => "Edit Instrument Definitions"; set => throw new NotImplementedException(); }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
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
                Dialog.Message(_appContext,_container,"You may not delete the default definition.", "Invalid Operation",Icons.Critical,128);
                return;
            }
            InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            _appContext.Definitions.Remove(def);
            LB_SavedDefs.Items.Remove(LB_SavedDefs.SelectedItem);
        }

        private void LB_SavedDefs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            InsDefSetEditor((ListBoxItem)LB_SavedDefs.SelectedItem, _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content));
        }

        private void Bn_InsDefAdd_Click(object sender, RoutedEventArgs e)
        {
            InstrumentDefinition def = new InstrumentDefinition()
            {
                Name = "Definition " + (LB_SavedDefs.Items.Count + 1),
                Banks = new ObservableCollection<Bank>(),
                AssociatedDeviceIndex = -1
            };
            _appContext.Definitions.Add(def);
            LB_SavedDefs.Items.Add(new ListBoxItem() { Content = def.Name });
            LB_SavedDefs.SelectedIndex = LB_SavedDefs.Items.Count - 1;
            InsDefSetEditor((ListBoxItem)LB_SavedDefs.SelectedItem, _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content));
        }

        private void Lv_banks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
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

        private void Bn_InsDefAddBank_Click(object sender, RoutedEventArgs e)
        {
            InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            def.Banks.Add(new Bank(def.Banks.Count, "Bank " + (def.Banks.Count + 1)));
        }

        private void Bn_InsDefDelBank_Click(object sender, RoutedEventArgs e)
        {
            if (lv_banks.SelectedItems.Count <= 0) return;
            InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);

            while (lv_banks.SelectedItems.Count > 0)
            {
                def.Banks.RemoveAt(lv_banks.SelectedIndex);
            }
        }

        private void Bn_InsDefAddPatch_Click(object sender, RoutedEventArgs e)
        {
            if (lv_banks.SelectedItem == null) return;
            Bank b = (Bank)lv_banks.SelectedItem;
            b.Instruments.Add(new NumberedEntry(b.Instruments.Count, "Instrument " + (b.Instruments.Count + 1)));

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
            InstrumentDefinition d = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            InstrumentDefinition n = _appContext.Definitions.FirstOrDefault(x => x.Name == tb_defName.Text);
            if (n != null && _insDefNameDirty)
            {
                Dialog.Message(_appContext,_container,"A definition with this name already exists. Please try again.", "Duplicate Entry", Icons.Warning);
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
            if (e.RemovedItems.Count > 0)
            {
                prv = (NumberedEntry)e.RemovedItems[0];
            }
            if (_appContext.Definitions == null || LB_SavedDefs.SelectedItem == null || cm_InsDefDevices.SelectedItem == null) return;
            InstrumentDefinition currentDef = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);

            int ProposedIndex = ((NumberedEntry)cm_InsDefDevices.SelectedItem).Index;

            InstrumentDefinition CheckDupe = _appContext.Definitions.FirstOrDefault(x => x.AssociatedDeviceIndex == ProposedIndex);


            if (currentDef == null) return;
            if (CheckDupe != null)
            {
                if (CheckDupe != currentDef && ProposedIndex != -1)
                {
                    var name = ((NumberedEntry)cm_InsDefDevices.SelectedItem).EntryName;
                    //oh no... I need to make a confirmation one too!
                    var confirm = MessageBox.Show($"{name} already has definition '{CheckDupe.Name}' assigned. Are you sure you want reassign definitions?", "Definition Association", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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

        private void Lv_banks_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if ((string)e.Column.Header == "Index")//Is there a more solid way?
            {
                InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
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
            InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            Bank b = (Bank)lv_banks.SelectedItem;

            b.Instruments = InstrumentDefinition.DefaultBanks().FirstOrDefault(x => x.Index == 0).Instruments;
            InsDefPopulatePatches(def, b.Index);
        }

        private void Mi_PopGenDRUM_Click(object sender, RoutedEventArgs e)
        {
            if (LB_SavedDefs.SelectedItem == null) return;
            if (lv_banks.SelectedItem == null) return;
            InstrumentDefinition def = _appContext.Definitions.FirstOrDefault(x => x.Name == (string)((ListBoxItem)LB_SavedDefs.SelectedItem).Content);
            Bank b = (Bank)lv_banks.SelectedItem;

            b.Instruments = InstrumentDefinition.DefaultBanks().FirstOrDefault(x => x.Index == 127).Instruments;
            InsDefPopulatePatches(def, b.Index);
        }

        private void Bn_InsDefSave_Click(object sender, RoutedEventArgs e)
        {
            _appContext.AppConfig.InstrumentDefinitionPath = SaveInsDef(App.APP_DATA_DIR + "Instruments.def");
            _appContext.SaveConfig();

            _appContext.currentView.HandleEvent(this, new EventArgs(), "InsDEF_Changed");
            //DialogClosed?.Invoke(this, new DialogEventArgs(_appContext, _container)); no bueno
            _parent.ShowDialog(new Settings(_appContext.AppConfig, _appContext, _parent), _appContext, _container);
            
        }

        private void Bn_InsDefSetActiveDevice_Click(object sender, RoutedEventArgs e)
        {
            cm_InsDefDevices.SelectedItem = cm_InsDefDevices.Items.OfType<NumberedEntry>().FirstOrDefault(x => x.Index == _appContext.AppConfig.ActiveOutputDeviceIndex);
        }


        #endregion

        #region Instrument Definition Logic

        private string SaveInsDef(string path)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(_appContext.Definitions, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Write Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return path;
        }

        private void PopulateSavedDefinitions()
        {
            LB_SavedDefs.Items.Clear();

            foreach (InstrumentDefinition item in _appContext.Definitions)
            {
                LB_SavedDefs.Items.Add(new ListBoxItem() { Content = item.Name });
            }
            //LB_SavedDefs.SelectedIndex = LB_SavedDefs.SelectedIndex = LB_SavedDefs.Items.Count - 1;//wtf?
            LB_SavedDefs.SelectedIndex = LB_SavedDefs.Items.Count - 1;
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
            if (def == null) return;//oops?
            lv_patches.ItemsSource = def.Banks.FirstOrDefault(x => x.Index == bank)?.Instruments;
        }

        private void InsDefPopulateBanks(InstrumentDefinition def)
        {
            lv_banks.ItemsSource = def.Banks;
        }

        #endregion

    }
}
