using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MidiSynth7.components
{
    public class SystemComponent
    {
        public static Key[] KeysTable =
        {
           Key.Z, Key.S,Key.X,Key.C,Key.F,Key.V,Key.G,Key.B,Key.N,Key.J,Key.M,Key.K,KeyInterop.KeyFromVirtualKey(188),Key.L,KeyInterop.KeyFromVirtualKey(190),KeyInterop.KeyFromVirtualKey(191),KeyInterop.KeyFromVirtualKey(222),Key.RightShift,KeyInterop.KeyFromVirtualKey(13),
           Key.Q,Key.W,Key.D3,Key.E,Key.D4,Key.R,Key.D5,Key.T,Key.Y,Key.D7,Key.U,Key.D8,Key.I,Key.O,Key.D0,Key.P,KeyInterop.KeyFromVirtualKey(189),
           KeyInterop.KeyFromVirtualKey(219),KeyInterop.KeyFromVirtualKey(187),KeyInterop.KeyFromVirtualKey(221)
        };
    }
}
