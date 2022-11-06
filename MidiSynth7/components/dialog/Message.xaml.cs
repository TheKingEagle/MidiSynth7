using System;
using System.Windows;
using System.Windows.Controls;

namespace MidiSynth7.components.dialog
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    public partial class Message : Page, IDialogView
    {
        private string _title;
        private MainWindow _appContext;
        private Grid _container;
        public Message(MainWindow AppContext, Grid Container, string Title,string Message,Icons Icon, bool canCancel=false)
        {
            InitializeComponent();
            _appContext = AppContext;
            _title = Title;
            _container = Container;
            PM_Icon_Warning.Visibility = Icon == Icons.Warning ? Visibility.Visible : Visibility.Collapsed;
            PM_Icon_Critical.Visibility = Icon == Icons.Critical ? Visibility.Visible : Visibility.Collapsed;
            PM_Icon_Info.Visibility = Icon == Icons.Info ? Visibility.Visible : Visibility.Collapsed;
            TX_Message.Text = Message;
            bn_Cancel.Visibility = canCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        public string DialogTitle { get => _title; set => throw new NotImplementedException(); }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        private void Bn_OK_Click(object sender, RoutedEventArgs e) => DialogClosed?.Invoke(this, new DialogEventArgs(_appContext, _container));

        private void bn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogClosed?.Invoke(this, new DialogEventArgs(_appContext, _container,false));
        }
    }
    public enum Icons
    {
        None,
        Info,
        Warning,
        Critical
    }
}
