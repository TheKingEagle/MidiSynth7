using MidiSynth7.components;
using MidiSynth7.components.dialog;
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

namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : UserControl
    {
        public Dialog()
        {
            InitializeComponent();
        }

        ~Dialog()
        {
            if(activeDialog != null)
            {
                activeDialog = null;
            }
        }
        IDialogView activeDialog { get; set; }
       
        public bool shown = false;
        bool _helpRequest = false;
        private void BNHelpRequested_Click(object sender, RoutedEventArgs e)
        {
            _helpRequest = !_helpRequest;
            Cursor = _helpRequest ? Cursors.Help : Cursors.Arrow;
            
        }

        public void ShowDialog(IDialogView View, MainWindow window, Grid container)
        {
            if (shown)
            {
                //Fade out the dialog when switching from one view to the other if already shown.
                window.FadeUI(1, 0, container);
                window.ScaleUI(1, 0.8, this);
            }

            shown = true;
            activeDialog = View;
            Dlg_TitleBlock.Text = activeDialog.DialogTitle;
            activeDialog.DialogClosed += ActiveDialog_DialogClosed;
            BNHelpRequested.Visibility = activeDialog.CanRequestHelp ? Visibility.Visible : Visibility.Collapsed;
            BNHelpRequested.IsEnabled = activeDialog.CanRequestHelp;
            FR_dialogView.Content = View;
            container.Children.Add(this);
            window.FadeUI(0, 1, container);
            window.ScaleUI(0.8, 1, this);
            
        }

        private void ActiveDialog_DialogClosed(object sender, DialogEventArgs e)
        {
            shown = false;
            e.Window.ScaleUI(1.0, 0.8, this);
            e.Window.FadeUI(1.0, 0, e.Container);

        }
        public void OnDialogAnimationComplete(Grid container)
        {
            if (shown == false)
            {
                container.Children.Remove(this);
            }
        }

        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_helpRequest)
            {
                _helpRequest = false;
                Cursor = _helpRequest ? Cursors.Help : Cursors.Arrow;

                var f = sender;
                activeDialog?.InvokeHelpRequested(GetElementUnderMouse<Control>());
                e.Handled = true;
                
            }
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        public static T GetElementUnderMouse<T>() where T : UIElement
        {
            return FindVisualParent<T>(Mouse.DirectlyOver as UIElement);
        }

        public static void Message(MainWindow win, Grid container, string text,string caption, Icons icon)
        {
            if (container.Visibility == Visibility.Visible) return;
            Dialog d = new Dialog();
            d.ShowDialog(new Message(win, container, caption, text, icon), win, container);
        }
    }
}
