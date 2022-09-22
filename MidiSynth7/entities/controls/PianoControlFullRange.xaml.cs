﻿using System;
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
using System.Windows.Threading;
using MidiSynth7.components;
using MidiSynth7.entities;
using MidiSynth7.entities.controls;
namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for PianoControl.xaml
    /// </summary>
    public partial class PianoControlFullRange : UserControl
    {
        List<(int keyID, ISynthKey keyItem)> Keys = new List<(int keyID, ISynthKey keyItem)>();
        public int KeyCount { get { return contentGrid.Children.Count; } }

        public PianoControlFullRange()
        {
            InitializeComponent();

            for (int i = 0; i < kTypeTable.Length + 1; i++)
            {
                foreach (UIElement item in contentGrid.Children)
                {
                    ISynthKey key = item as ISynthKey;
                    if (key != null)
                    {
                        if (key.KeyID == i)
                        {
                            key.VKeyDown += PianoControl_vKeyDown;
                            key.VKeyUp += PianoControl_vKeyUp;
                            Keys.Add((key.KeyID, key));
                        }
                    }
                }
            }
        }

        void PianoControl_vKeyUp(object sender, PKeyEventArgs e)
        {
            EventHandler<PKeyEventArgs> temp = pKeyUp;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        void PianoControl_vKeyDown(object sender, PKeyEventArgs e)
        {
            EventHandler<PKeyEventArgs> temp = pKeyDown;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        public System.Windows.Input.Key[] KeysTable =
        {
           Key.Z, Key.S,Key.X,Key.C,Key.F,Key.V,Key.G,Key.B,Key.N,Key.J,Key.M,Key.K,(System.Windows.Input.KeyInterop.KeyFromVirtualKey(188)),Key.L,(System.Windows.Input.KeyInterop.KeyFromVirtualKey(190)),(System.Windows.Input.KeyInterop.KeyFromVirtualKey(191)),(System.Windows.Input.KeyInterop.KeyFromVirtualKey(222)),Key.RightShift,(System.Windows.Input.KeyInterop.KeyFromVirtualKey(13)),
           Key.Q,Key.W,Key.D3,Key.E,Key.D4,Key.R,Key.D5,Key.T,Key.Y,Key.D7,Key.U,Key.D8,Key.I,Key.O,Key.D0,Key.P,(System.Windows.Input.KeyInterop.KeyFromVirtualKey(189)),
           (System.Windows.Input.KeyInterop.KeyFromVirtualKey(219)),(System.Windows.Input.KeyInterop.KeyFromVirtualKey(187)),(System.Windows.Input.KeyInterop.KeyFromVirtualKey(221))
        };

        KeyTypes[] kTypeTable =
            {
                KeyTypes.White,KeyTypes.Black, KeyTypes.White, KeyTypes.White, KeyTypes.Black,KeyTypes.White, KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//15
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//63
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//75
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//87
                
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//87
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//87
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//87
                KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,KeyTypes.Black,KeyTypes.White,//87
                KeyTypes.White,
            };
        
        public void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                return;
            }
            int indx = Array.IndexOf(KeysTable, e.Key);
            if (indx > -1)
            {
                var key = Keys.FirstOrDefault(k => k.keyID == indx + 21).keyItem;
                if (key != null)
                {
                    key.SendOn();
                }
            }
        }

        public void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                return;
            }
            int indx = Array.IndexOf(KeysTable, e.Key);
            if (indx > -1)
            {
                var key = Keys.FirstOrDefault(k => k.keyID == indx + 21).keyItem;
                if (key != null)
                {
                    key.SendOff();
                }
            }
        }

        public void SetNoteText(int Transpose)
        {
            Notify.Show(Transpose);
            try
            {
                for (int i = 0; i < kTypeTable.Length; i++)
                {

                    foreach (UIElement item in contentGrid.Children)
                    {
                        ISynthKey key = item as ISynthKey;
                        if (key != null)
                        {
                            if (key.KeyID == i + 21)
                            {
                                key.SetLetter(MidiEngine.GetNote(i+21 + 12 + Transpose).noteLabel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        #region Fake lighting.

        public void LightKey(int zbkeyid)
        {
            var key = Keys.FirstOrDefault(k => k.keyID == zbkeyid);
            if (key.keyItem != null)
            {
                key.keyItem.FSendOn();
            }
        }
        public void ALTLightKey(int zbkeyid)
        {
            var key = Keys.FirstOrDefault(k => k.keyID == zbkeyid);
            if (key.keyItem != null)
            {
                key.keyItem.FSendOnA();
            }
        }

        public void CustomLightKey(int zbkeyid, LinearGradientBrush background)
        {
            var key = Keys.FirstOrDefault(k => k.keyID == zbkeyid);
            if (key.keyItem != null)
            {
                key.keyItem.FSendOnC(background);
            }
        }
        public void UnLightKey(int zbkeyid)
        {
            var key = Keys.FirstOrDefault(k => k.keyID == zbkeyid);
            if (key.keyItem != null)
            {
                key.keyItem.FSendOff();
                
            }
        }
        #endregion

        public event EventHandler<PKeyEventArgs> pKeyUp;
        public event EventHandler<PKeyEventArgs> pKeyDown;

    }

}
