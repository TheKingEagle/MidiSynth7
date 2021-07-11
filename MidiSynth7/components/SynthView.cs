using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MidiSynth7.components
{
    public interface ISynthView
    {
        void HandleNoteOnEvent(object sender, NoteEventArgs e);
        void HandleNoteOffEvent(object sender, NoteEventArgs e);
        void HandleEvent(object sender, EventArgs e, string id="generic");

        void HandleKeyDown(object sender, KeyEventArgs e);
        void HandleKeyUp(object sender, KeyEventArgs e);
    }
}
