using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components
{
    public class NumberedEntry
    {
        public int Index { get; private set; }
        public string EntryName { get; private set; }

        public NumberedEntry(int entryID, string entryName)
        {
            Index = entryID;
            EntryName = entryName;
        }
        public override string ToString()
        {
            return EntryName;
        }
    }
}
