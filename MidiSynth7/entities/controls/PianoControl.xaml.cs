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
namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for PianoControl.xaml
    /// </summary>
    public partial class PianoControl : UserControl
    {
        List<BlackKey> blackKeys = new List<BlackKey>();
        List<WhiteKey> whiteKeys = new List<WhiteKey>();
        public PianoControl()
        {
            InitializeComponent();
            
            for (int i = 0; i < kTypeTable.Length+1; i++)
            {
                
                foreach (UIElement item in contentGrid.Children)
                {
                    if (item.GetType() == typeof(BlackKey))
                    {
                        if (((BlackKey)item).KeyID == i)
                        {
                            ((BlackKey)item).VKeyDown += PianoControl_vKeyDown; 
                            ((BlackKey)item).VKeyUp += PianoControl_vKeyUp;
                            blackKeys.Add((BlackKey)item);
                        }
                    }
                    if (item.GetType() == typeof(WhiteKey))
                    {
                        if (((WhiteKey)item).KeyID == i)
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
            for (int i = 0; i < KeysTable.Length; i++)
            {
                if (KeysTable[i] == (Key)e.Key)
                {
                    foreach (object item in contentGrid.Children)
                    {
                        if (item.GetType() == typeof(BlackKey))
                        {
                            if (((BlackKey)item).KeyID == i + 21)
                            {
                                ((BlackKey)item).SendOn(true);
                            }
                        }
                        if (item.GetType() == typeof(WhiteKey))
                        {
                            if (((WhiteKey)item).KeyID == i + 21)
                            {
                                ((WhiteKey)item).SendOn(true);
                            }
                        }
                    }
                }
            }
        }

        public void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < KeysTable.Length; i++)
            {
                if (KeysTable[i] == (Key)e.Key)
                {
                    foreach (object item in contentGrid.Children)
                    {
                        if (item.GetType() == typeof(BlackKey))
                        {
                            if (((BlackKey)item).KeyID == i + 21)
                            {
                                ((BlackKey)item).SendOff(true);
                            }
                        }
                        if (item.GetType() == typeof(WhiteKey))
                        {
                            if (((WhiteKey)item).KeyID == i + 21)
                            {
                                ((WhiteKey)item).SendOff(true);
                            }
                        }
                    }
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
                        if (item.GetType() == typeof(BlackKey))
                        {
                            if (((BlackKey)item).KeyID == i+21)
                            {
                                ((BlackKey)item).SetLetter(Letters[i + 12 + Transpose]);
                            }
                        }
                        if (item.GetType() == typeof(WhiteKey))
                        {
                            if (((WhiteKey)item).KeyID == i+21)
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
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid );
            WhiteKey WK_item =  whiteKeys.FirstOrDefault(o => o.KeyID == zbkeyid );
            if(BK_item != null)
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
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid );
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

        public void CustomLightKey(int zbkeyid,LinearGradientBrush background)
        {
            //duh.
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            WhiteKey WK_item = whiteKeys.FirstOrDefault(o => o.KeyID == zbkeyid);
            if (BK_item != null)
            {
                BK_item.FSendOnC(background);
            }
            if (WK_item != null)
            {
                WK_item.FSendOnC(background);
            }
        }
        public void UnLightKey(int zbkeyid)
        {
            BlackKey BK_item = blackKeys.FirstOrDefault(o => o.KeyID == zbkeyid );
            WhiteKey WK_item = whiteKeys.FirstOrDefault(o => o.KeyID == zbkeyid );
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
    public class PKeyEventArgs : EventArgs
    {
        public int KeyID { get; private set; }
        public int Velocity { get; private set; }
        public PKeyEventArgs(int keyID)
        {
            KeyID = keyID;
        }
        public PKeyEventArgs(int keyID,int Velocity)
        {
            KeyID = keyID;
            this.Velocity = Velocity;
        }
    }
}
