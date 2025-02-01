using System;
using System.Windows;
using System.Windows.Controls;

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class StringPrompt : Page, IDialogView
    {
        private string _title;
        private MainWindow _appContext;
        private Grid _container;
        public StringPrompt(MainWindow AppContext, Grid Container, string Title,string Message,string DefaultText="", bool CanCancel=false)
        {
            InitializeComponent();
            _appContext = AppContext;
            _title = Title;
            _container = Container;
            TX_Message.Text = Message;
            Tb_StringPrompt.Text = DefaultText;
            bn_Cancel.Visibility = CanCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        public string DialogTitle { get => _title; set => throw new NotImplementedException(); }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }


        public string PromptResponse { get; private set; }
        private void Bn_OK_Click(object sender, RoutedEventArgs e) {
            PromptResponse = Tb_StringPrompt.Text;
            DialogClosed?.Invoke(this, new DialogEventArgs(_appContext, _container));
        }

        private void bn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogClosed?.Invoke(this, new DialogEventArgs(_appContext, _container,false));
        }
    }
}
