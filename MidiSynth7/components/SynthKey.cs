using MidiSynth7.entities.controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MidiSynth7.components
{
    public interface ISynthKey
    {
        int KeyID { get; set; }
        string NoteText { get; set; }

        event EventHandler<PKeyEventArgs> VKeyUp;
        
        event EventHandler<PKeyEventArgs> VKeyDown;

        void SendOn();

        void SendOff();

        void FSendOn();

        void FSendOff();

        void FSendOnA();

        void FSendOnC(Brush background);

        void SetLetter(string NoteID);

    }
}
