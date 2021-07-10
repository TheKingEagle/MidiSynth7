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
using System.Windows.Threading;
using MidiSynth7.entities;
using MidiSynth7.entities.controls;
namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for PianoControl.xaml
    /// </summary>
    public partial class PianoControlFullRange : UserControl
    {
        List<BlackKey> blackKeys = new List<BlackKey>();
        List<WhiteKey> whiteKeys = new List<WhiteKey>();
        public PianoControlFullRange()
        {
            InitializeComponent();

            for (int i = 0; i < kTypeTable.Length; i++)
            {

                foreach (UIElement item in contentGrid.Children)
                {
                    if (item.GetType() == typeof(BlackKey))
                    {
                        if (((BlackKey)item).KeyID == i + 1)
                        {
                            ((BlackKey)item).VKeyDown += PianoControl_vKeyDown;
                            ((BlackKey)item).VKeyUp += PianoControl_vKeyUp;
                            blackKeys.Add((BlackKey)item);
                        }
                    }
                    if (item.GetType() == typeof(WhiteKey))
                    {
                        if (((WhiteKey)item).KeyID == i + 1)
                        {
                            ((WhiteKey)item).VKeyDown += PianoControl_vKeyDown;
                            ((WhiteKey)item).VKeyUp += PianoControl_vKeyUp;
                            whiteKeys.Add((WhiteKey)item);
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

        public Key[] KeysTable =
        {
           Key.Z, Key.S,Key.X,Key.D,Key.C,Key.V,Key.G,Key.B,Key.H,Key.N,Key.J,Key.M,KeyInterop.KeyFromVirtualKey(188),Key.L,KeyInterop.KeyFromVirtualKey(190),KeyInterop.KeyFromVirtualKey(186),KeyInterop.KeyFromVirtualKey(191),
           Key.Q,Key.D2,Key.W,Key.D3,Key.E,Key.D4,Key.R,Key.T,Key.D6,Key.Y,Key.D7,Key.U,Key.I,Key.D9,Key.O,Key.D0,Key.P,KeyInterop.KeyFromVirtualKey(189),
           KeyInterop.KeyFromVirtualKey(219),KeyInterop.KeyFromVirtualKey(221),KeyInterop.KeyFromVirtualKey(8),KeyInterop.KeyFromVirtualKey(220)//This will only control the first 39....

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
        string[] Letters =
        {
            "A","A#","B","C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//15
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//27
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//39
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//51
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//63
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//75
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//87
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//99
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//111
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//123... Overkill, I know... I allow for transpose. for some reason...
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//123... Overkill, I know... I allow for transpose. for some reason...
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//123... Overkill, I know... I allow for transpose. for some reason...
            "C","C#","D","D#","E","F","F#","G","G#","A","A#","B",//123... Overkill, I know... I allow for transpose. for some reason...



        };
        public void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            //for (int i = 0; i < KeysTable.Length; i++)
            //{
            //    if (KeysTable[i] == (Key)e.Key)
            //    {
            //        foreach (object item in contentGrid.Children)
            //        {
            //            if (item.GetType() == typeof(BlackKey))
            //            {
            //                if (((BlackKey)item).KeyID == i+22)
            //                {
            //                    ((BlackKey)item).SendOn(true);
            //                }
            //            }
            //            if (item.GetType() == typeof(WhiteKey))
            //            {
            //                if (((WhiteKey)item).KeyID == i+22)
            //                {
            //                    ((WhiteKey)item).SendOn(true);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            //for (int i = 0; i < KeysTable.Length; i++)
            //{

            //    if (KeysTable[i] == (Key)e.Key)
            //    {
            //        foreach (object item in contentGrid.Children)
            //        {
            //            if (item.GetType() == typeof(BlackKey))
            //            {
            //                if (((BlackKey)item).KeyID == i + 1)
            //                {
            //                    ((BlackKey)item).SendOff(true);
            //                }
            //            }
            //            if (item.GetType() == typeof(WhiteKey))
            //            {
            //                if (((WhiteKey)item).KeyID == i + 1)
            //                {
            //                    ((WhiteKey)item).SendOff(true);
            //                }
            //            }
            //        }
            //    }
            //}
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
                        if (item.GetType() == typeof(BlackKey))
                        {
                            if (((BlackKey)item).KeyID == i + 21)
                            {
                                ((BlackKey)item).SetLetter(Letters[i + 12 + Transpose]);
                            }
                        }
                        if (item.GetType() == typeof(WhiteKey))
                        {
                            if (((WhiteKey)item).KeyID == i + 21)
                            {
                                ((WhiteKey)item).SetLetter(Letters[i + 12 + Transpose]);
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
            //duh.
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            WhiteKey WK_item = whiteKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            if (BK_item != null)
            {
                BK_item.FSendOn();
            }
            if (WK_item != null)
            {
                WK_item.FSendOn();
            }
        }
        public void ALTLightKey(int zbkeyid)
        {
            //duh.
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            WhiteKey WK_item = whiteKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            if (BK_item != null)
            {
                BK_item.FSendOnA();
            }
            if (WK_item != null)
            {
                WK_item.FSendOnA();
            }
        }
        public void UnLightKey(int zbkeyid)
        {
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            WhiteKey WK_item = whiteKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            if (BK_item != null)
            {
                BK_item.FSendOff();
            }
            if (WK_item != null)
            {
                WK_item.FSendOff();
            }
        }
        #endregion

        public event EventHandler<PKeyEventArgs> pKeyUp;
        public event EventHandler<PKeyEventArgs> pKeyDown;

    }

}
