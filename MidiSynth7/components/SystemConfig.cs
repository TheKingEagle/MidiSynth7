using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanford.Multimedia.Midi;

namespace MidiSynth7.components
{
    public class SystemConfig
    {
        public int ActiveOutputDeviceIndex { get; set; }

        public int ActiveInputDeviceIndex { get; set; }
        
        public int[] ChannelInstruments { get; set; }
        public int[] ChannelBanks { get; set; }

        /// <summary>
        /// This is configuration containing Octave, transpose, and octaveFX 3 transpose
        /// </summary>
        public int[] PitchOffsets { get; set; }

        public int[] ChannelVolumes { get; set; }

        public int[] ChannelPans { get; set; }

        public int[] ChannelReverbs { get; set; }

        public int[] ChannelChoruses { get; set; }

        public int[] ChannelModulations { get; set; }

        public List<(string name, (int controllerID, int value)[])> ChannelCustomControls { get; set; }

        public int SelectedRiff { get; set; }

        public bool EnableRiffs { get; set; }

        public string InstrumentDefinitionPath { get; set; }

        public DisplayModes DisplayMode { get; set; }

        public SystemConfig()
        {
            //default constructor
        }

        public SystemConfig(DisplayModes displayMode)
        {
            //Constructor for new instance
            DisplayMode = displayMode;

            ActiveInputDeviceIndex   = -1;
            ActiveOutputDeviceIndex  = 0;
            EnableRiffs              = false;
            ChannelInstruments       = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelBanks             = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //Offset layout:                     {global octave, global transpose, ofx 3 transpose 1, ofx3 transpose 2, ..etc}
            PitchOffsets           = new int[] { 3, 0, -12, -24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelVolumes           = new int[] { 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127 };
            ChannelPans              = new int[] { 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64 };
            ChannelReverbs           = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelChoruses          = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelModulations       = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelCustomControls    = new List<(string name, (int controllerID, int value)[])>();

            var controller1 = new (int controllerID, int value)[]
            {
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0),
                ((int)ControllerType.TremeloLevel, 0)
            };

            var controller2 = new (int controllerID, int value)[]
            {
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0),
                ((int)ControllerType.PhaserLevel, 0)
            };

            ChannelCustomControls.Add(("Tremolo",controller1));
            ChannelCustomControls.Add(("Phaser", controller2));

        }

        public bool CheckForMissingValues()
        {
            object[] items = new object[]
            {
                DisplayMode, EnableRiffs,PitchOffsets,ChannelInstruments,
                ChannelVolumes,ChannelCustomControls,ChannelChoruses,ChannelReverbs,
                ActiveInputDeviceIndex,ActiveOutputDeviceIndex, SelectedRiff,
                ChannelModulations,ChannelPans,ChannelBanks
            };
            return items.Any(v => v == null);
        }

    }

    public enum DisplayModes
    {
        Standard = 0,
        Studio = 1,
        Compact = 2
    }
}
