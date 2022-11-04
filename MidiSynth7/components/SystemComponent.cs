using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MidiSynth7.components
{
    public static class SystemComponent
    {
        
        public static Key[] KeysTable =
        {
           Key.Z, Key.S,Key.X,Key.C,Key.F,Key.V,Key.G,Key.B,Key.N,Key.J,Key.M,Key.K,KeyInterop.KeyFromVirtualKey(188),Key.L,KeyInterop.KeyFromVirtualKey(190),KeyInterop.KeyFromVirtualKey(191),KeyInterop.KeyFromVirtualKey(222),Key.RightShift,KeyInterop.KeyFromVirtualKey(13),
           Key.Q,Key.W,Key.D3,Key.E,Key.D4,Key.R,Key.D5,Key.T,Key.Y,Key.D7,Key.U,Key.D8,Key.I,Key.O,Key.D0,Key.P,KeyInterop.KeyFromVirtualKey(189),
           KeyInterop.KeyFromVirtualKey(219),KeyInterop.KeyFromVirtualKey(187),KeyInterop.KeyFromVirtualKey(221)
        };
        public static Key[] MPTKeysTable = KeysTable.Where(x => x != KeyInterop.KeyFromVirtualKey(187)).ToArray();

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }

        public static Typeface MPTEditorFont = new Typeface(new FontFamily("Segoe UI Mono"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        
        public static GlyphTypeface MPTGlyphTypeFace;

    }
}
