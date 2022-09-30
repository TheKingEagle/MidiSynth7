﻿using MidiSynth7.entities.controls;
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
    /// Interaction logic for SequenceEditor.xaml
    /// </summary>
    public partial class SequenceEditor : Page, IDialogView
    {
        public SequenceEditor(MainWindow win, Grid container)
        {
            _win = win;
            _container = container;
            InitializeComponent();
            
        }
        private MainWindow _win;
        private Grid _container;

        private MPTPattern ActivePattern;
        string _ttl = "Sequence Tracker v1.0";
        public string DialogTitle { get => _ttl; set => _ttl = value; }
        public bool HelpRequested { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool CanRequestHelp => false;

        public event EventHandler<DialogEventArgs> DialogClosed;

        public void InvokeHelpRequested(Control target)
        {
            throw new NotImplementedException();
        }

        public void LoadPattern(int index=0 )
        {
            string ttl = DialogTitle;
            loader.Visibility = Visibility.Visible;
            UpdateLayout();
            PatternContainer.Children.Clear();

            Console.WriteLine("Yup");
            ActivePattern = new MPTPattern(16,4,TrackerPattern.GetEmptyPattern(32,20), PatternContainer);
            ActivePattern.PatternSelectionChange += ActivePattern_PatternSelectionChange;
            PatternContainer.Children.Add(ActivePattern);
            loader.Visibility = Visibility.Collapsed;
            PatternContainer.Margin = new Thickness(0, (PatternScroller.ViewportHeight-21) / 2, 0, (PatternScroller.ViewportHeight-21) / 2);

        }

        private void ActivePattern_PatternSelectionChange(object sender, SelectionEventArgs e)
        {
            PatternScroller.ScrollToVerticalOffset(((PatternScroller.ViewportHeight - 21) / 2) + (e.SelectedIndex * 21) - ((PatternScroller.ViewportHeight - 21) / 2));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PatternContainer.Children.Clear();
            ActivePattern = null;

            DialogClosed?.Invoke(this, new DialogEventArgs(_win, _container));
        }

        private void PatternScroller_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if(e.Key == Key.Up)
            {
                ActivePattern.ActiveRowIndex--;
                if(ActivePattern.ActiveRowIndex < 0)
                {
                    //TODO: Load previous pattern
                    ActivePattern.ActiveRowIndex = ActivePattern.RowCount - 1;
                }
                ActivePattern.SetHotRow(ActivePattern.ActiveRowIndex);
            }
            
            if (e.Key == Key.Down)
            {
                ActivePattern.ActiveRowIndex++;
                if (ActivePattern.ActiveRowIndex > ActivePattern.RowCount - 1)
                {
                    //TODO: Load next pattern
                    ActivePattern.ActiveRowIndex = 0;
                }
                ActivePattern.SetHotRow(ActivePattern.ActiveRowIndex);
            }
            if(e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ActivePattern.SelectActiveChannel();
            }
            if (e.Key == Key.L && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                ActivePattern.SelectActiveChannelBit();
            }
            int bit = ActivePattern.selectedBit;
            int ch = ActivePattern.selectedChannel;
            if (e.Key == Key.Left)
            {
                bit--;
            }
            if (e.Key == Key.Right)
            {
                bit++;
            }
            if (bit > 4)
            {
                bit = 0;
                ch++;
                if(ch > ActivePattern.ChannelCount-1)
                {
                    ch = 0;
                }
            }
            if (bit < 0)
            {
                bit = 4;
                ch--;
                if (ch < 0)
                {
                    ch = ActivePattern.ChannelCount - 1;
                }
            }
            ActivePattern.MoveBitActiveRow(ch, bit);
        }
    }
}
