namespace MidiSynth7.components
{
    public class NumberedEntry
    {

        public int Index { get; set; }
        public string EntryName { get; set; }

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
