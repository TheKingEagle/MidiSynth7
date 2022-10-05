﻿using MidiSynth7.components;
using MidiSynth7.components.dialog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        public event EventHandler<EventArgs> DialogShown;
        public bool shown = false;
        bool _helpRequest = false;
        static Grid ModalOverlay = new Grid();
        private void BNHelpRequested_Click(object sender, RoutedEventArgs e)
        {
            _helpRequest = !_helpRequest;
            Cursor = _helpRequest ? Cursors.Help : Cursors.Arrow;
            
        }

        public void ShowDialog(IDialogView View, MainWindow window, Grid container, bool OverlayModal=false, byte overlayOpacity = 128)
        {
           
            //fading out during view switching does not work well.
            activeDialog = View;
            Dlg_TitleBlock.Text = activeDialog.DialogTitle;
            activeDialog.DialogClosed += ActiveDialog_DialogClosed;
            BNHelpRequested.Visibility = activeDialog.CanRequestHelp ? Visibility.Visible : Visibility.Collapsed;
            BNHelpRequested.IsEnabled = activeDialog.CanRequestHelp;
            FR_dialogView.Content = activeDialog;
            if (!shown)
            {
                if (OverlayModal)
                {
                    if (container.Children.Contains(ModalOverlay)) return;
                    ModalOverlay.Background = new SolidColorBrush(Color.FromArgb(overlayOpacity, 0, 0, 0));
                    container.Children.Add(ModalOverlay);
                }
                container.Children.Add(this);
                if (container.Visibility != Visibility.Visible) { 
                    window.FadeUI(0, 1, container);
                }
                window.ScaleUI(0.8, 1, this);
                shown = true;
            }
            
        }
        private void ActiveDialog_DialogClosed(object sender, DialogEventArgs e)
        {
            if(e.Container.Children.IndexOf(ModalOverlay) > -1)
            {
                e.Container.Children.Remove(ModalOverlay);
            }
            shown = false;
            e.Window.ScaleUI(1.0, 0.8, this);
            if (e.Container.Children.Count > 1) return;//don't fade the container if there's more than one dialog inside.
            e.Window.FadeUI(1.0, 0, e.Container);

        }
        public void OnDialogAnimationComplete(Grid container)
        {
            if (shown == false)
            {
                container.Children.Remove(this);
            }
            if (shown)
            {
                DialogShown?.Invoke(this.activeDialog, new EventArgs());
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

        public static void Message(MainWindow win, Grid container, string text,string caption, Icons icon, byte overlayOpacity = 0)
        {
            if (container.Children.Contains(ModalOverlay)) return;
            ModalOverlay.Background = new SolidColorBrush(Color.FromArgb(overlayOpacity, 0, 0, 0));
            container.Children.Add(ModalOverlay);
            Dialog d = new Dialog();
            
            Message v = new Message(win, container, caption, text, icon);
            v.DialogClosed += V_DialogClosed;
            d.ShowDialog(v, win, container);
            
            
            
        }

        private static void V_DialogClosed(object sender, DialogEventArgs e)
        {
            e.Container.Children.Remove(ModalOverlay);
        }
    }
}
