using System.Collections.ObjectModel;

namespace MidiSynth7.components
{
    public class Bank
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public ObservableCollection<NumberedEntry> Instruments { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public Bank() { }
        public Bank(int index, string name)
        {
            //this is only temporary
            Index = index;
            Name = name;
            Instruments = new ObservableCollection<NumberedEntry>();
        }
    }
    /// <summary>
    /// Scaffolding for a JSON based instrument definition file
    /// </summary>
    public class InstrumentDefinition
    {
        public string Name { get; set; }
        public ObservableCollection<Bank> Banks { get; set; }

        public int AssociatedDeviceIndex { get; set; }

        public static ObservableCollection<Bank> DefaultBanks()
        {
            Bank defaultBank = new Bank
            {
                Index = 0,
                Name = "General MIDI",
                Instruments = new ObservableCollection<NumberedEntry>(new NumberedEntry[]
                {
                    new NumberedEntry(0,"Acoustic Grand Piano"),
                    new NumberedEntry(1,"Bright Acoustic Piano"),
                    new NumberedEntry(2,"Electric Grand Piano"),
                    new NumberedEntry(3,"Honky-tonk Piano"),
                    new NumberedEntry(4,"Rhodes Piano"),
                    new NumberedEntry(5,"Chorused Piano"),
                    new NumberedEntry(6,"Harpsichord"),
                    new NumberedEntry(7,"Clavinet"),
                    new NumberedEntry(8,"Celesta"),
                    new NumberedEntry(9,"Glockenspiel"),
                    new NumberedEntry(10,"Music Box"),
                    new NumberedEntry(11,"Vibraphone"),
                    new NumberedEntry(12,"Marimba"),
                    new NumberedEntry(13,"Xylophone"),
                    new NumberedEntry(14,"Tubular Bells"),
                    new NumberedEntry(15,"Dulcimer"),
                    new NumberedEntry(16,"Hammond Organ"),
                    new NumberedEntry(17,"Percussive Organ"),
                    new NumberedEntry(18,"Rock Organ"),
                    new NumberedEntry(19,"Church Organ"),
                    new NumberedEntry(20,"Reed Organ"),
                    new NumberedEntry(21,"Accordion"),
                    new NumberedEntry(22,"Harmonica"),
                    new NumberedEntry(23,"Tango Accordion"),
                    new NumberedEntry(24,"Acoustic Guitar (nylon)"),
                    new NumberedEntry(25,"Acoustic Guitar (steel)"),
                    new NumberedEntry(26,"Electric Guitar (jazz)"),
                    new NumberedEntry(27,"Electric Guitar (clean)"),
                    new NumberedEntry(28,"Electric Guitar (muted)"),
                    new NumberedEntry(29,"Overdriven Guitar"),
                    new NumberedEntry(30,"Distortion Guitar"),
                    new NumberedEntry(31,"Guitar Harmonics"),
                    new NumberedEntry(32,"Acoustic Bass"),
                    new NumberedEntry(33,"Electric Bass (finger)"),
                    new NumberedEntry(34,"Electric Bass (pick)"),
                    new NumberedEntry(35,"Fretless Bass"),
                    new NumberedEntry(36,"Slap Bass 1"),
                    new NumberedEntry(37,"Slap Bass 2"),
                    new NumberedEntry(38,"Synth Bass 1"),
                    new NumberedEntry(39,"Synth Bass 2"),
                    new NumberedEntry(40,"Violin"),
                    new NumberedEntry(41,"Viola"),
                    new NumberedEntry(42,"Cello"),
                    new NumberedEntry(43,"Contrabass"),
                    new NumberedEntry(44,"Tremolo Strings"),
                    new NumberedEntry(45,"Pizzicato Strings"),
                    new NumberedEntry(46,"Orchestral Harp"),
                    new NumberedEntry(47,"Timpani"),
                    new NumberedEntry(48,"String Ensemble 1"),
                    new NumberedEntry(49,"String Ensemble 2"),
                    new NumberedEntry(50,"SynthStrings 1"),
                    new NumberedEntry(51,"SynthStrings 2"),
                    new NumberedEntry(52,"Choir Aahs"),
                    new NumberedEntry(53,"Voice Oohs"),
                    new NumberedEntry(54,"Synth Voice"),
                    new NumberedEntry(55,"Orchestra Hit"),
                    new NumberedEntry(56,"Trumpet"),
                    new NumberedEntry(57,"Trombone"),
                    new NumberedEntry(58,"Tuba"),
                    new NumberedEntry(59,"Muted Trumpet"),
                    new NumberedEntry(60,"French Horn"),
                    new NumberedEntry(61,"Brass Section"),
                    new NumberedEntry(62,"Synth Brass 1"),
                    new NumberedEntry(63,"Synth Brass 2"),
                    new NumberedEntry(64,"Soprano Sax"),
                    new NumberedEntry(65,"Alto Sax"),
                    new NumberedEntry(66,"Tenor Sax"),
                    new NumberedEntry(67,"Baritone Sax"),
                    new NumberedEntry(68,"Oboe"),
                    new NumberedEntry(69,"English Horn"),
                    new NumberedEntry(70,"Bassoon"),
                    new NumberedEntry(71,"Clarinet"),
                    new NumberedEntry(72,"Piccolo"),
                    new NumberedEntry(73,"Flute"),
                    new NumberedEntry(74,"Recorder"),
                    new NumberedEntry(75,"Pan Flute"),
                    new NumberedEntry(76,"Bottle Blow"),
                    new NumberedEntry(77,"Shakuhachi"),
                    new NumberedEntry(78,"Whistle"),
                    new NumberedEntry(79,"Ocarina"),
                    new NumberedEntry(80,"Lead 1 (square)"),
                    new NumberedEntry(81,"Lead 2 (sawtooth)"),
                    new NumberedEntry(82,"Lead 3 (calliope lead)"),
                    new NumberedEntry(83,"Lead 4 (chiff lead)"),
                    new NumberedEntry(84,"Lead 5 (charang)"),
                    new NumberedEntry(85,"Lead 6 (voice)"),
                    new NumberedEntry(86,"Lead 7 (fifths)"),
                    new NumberedEntry(87,"Lead 8 (bass + lead)"),
                    new NumberedEntry(88,"Pad 1 (new age)"),
                    new NumberedEntry(89,"Pad 2 (warm)"),
                    new NumberedEntry(90,"Pad 3 (polysynth)"),
                    new NumberedEntry(91,"Pad 4 (choir)"),
                    new NumberedEntry(92,"Pad 5 (bowed)"),
                    new NumberedEntry(93,"Pad 6 (metallic)"),
                    new NumberedEntry(94,"Pad 7 (halo)"),
                    new NumberedEntry(95,"Pad 8 (sweep)"),
                    new NumberedEntry(96,"FX 1 (rain)"),
                    new NumberedEntry(97,"FX 2 (soundtrack)"),
                    new NumberedEntry(98,"FX 3 (crystal)"),
                    new NumberedEntry(99,"FX 4 (atmosphere)"),
                    new NumberedEntry(100,"FX 5 (brightness)"),
                    new NumberedEntry(101,"FX 6 (goblins)"),
                    new NumberedEntry(102,"FX 7 (echoes)"),
                    new NumberedEntry(103,"FX 8 (sci-fi)"),
                    new NumberedEntry(104,"Sitar"),
                    new NumberedEntry(105,"Banjo"),
                    new NumberedEntry(106,"Shamisen"),
                    new NumberedEntry(107,"Koto"),
                    new NumberedEntry(108,"Kalimba"),
                    new NumberedEntry(109,"Bagpipe"),
                    new NumberedEntry(110,"Fiddle"),
                    new NumberedEntry(111,"Shanai"),
                    new NumberedEntry(112,"Tinkle Bell"),
                    new NumberedEntry(113,"Agogo"),
                    new NumberedEntry(114,"Steel Drums"),
                    new NumberedEntry(115,"Woodblock"),
                    new NumberedEntry(116,"Taiko Drum"),
                    new NumberedEntry(117,"Melodic Tom"),
                    new NumberedEntry(118,"Synth Drum"),
                    new NumberedEntry(119,"Reverse Cymbal"),
                    new NumberedEntry(120,"Guitar Fret Noise"),
                    new NumberedEntry(121,"Breath Noise"),
                    new NumberedEntry(122,"Seashore"),
                    new NumberedEntry(123,"Bird Tweet"),
                    new NumberedEntry(124,"Telephone Ring"),
                    new NumberedEntry(125,"Helicopter"),
                    new NumberedEntry(126,"Applause"),
                    new NumberedEntry(127,"Gunshot"),
                }),
            };

            Bank percussion = new Bank()
            {
                Index = 127,
                Name = "General MIDI Drums",
                Instruments = new ObservableCollection<NumberedEntry>(new NumberedEntry[]
                {
                    new NumberedEntry(0, "Standard"),
                    new NumberedEntry(8, "Room"),
                    new NumberedEntry(16, "Power"),
                    new NumberedEntry(24, "Electronic"),
                    new NumberedEntry(25, "TR-808"),
                    new NumberedEntry(32, "Jazz"),
                    new NumberedEntry(40, "Brush"),
                    new NumberedEntry(48, "Orchestra"),
                    new NumberedEntry(56, "SFX"),
                })
            };

            //TODO: Maybe add more later?
            Bank[] banks = new Bank[]
            {
                defaultBank,
                percussion
            };
            return new ObservableCollection<Bank>(banks);
        }

        public static InstrumentDefinition GetDefaultDefinition()
        {
            InstrumentDefinition def = new InstrumentDefinition
            {
                Banks = DefaultBanks(),
                Name = "Default",
                AssociatedDeviceIndex = -1
            };
            return def;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
