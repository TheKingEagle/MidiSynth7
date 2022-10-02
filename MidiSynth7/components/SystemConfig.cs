using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MidiSynth7.components
{
    public class SystemConfig
    {
        public int ActiveOutputDeviceIndex { get; set; }

        public int ActiveInputDeviceIndex { get; set; }

        public int ActiveInputDevice2Index { get; set; }

        public bool Input1RelayMode { get; set; }

        public bool Input2RelayMode { get; set; }

        public bool EnforceInstruments { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelInstruments { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelBanks { get; set; }

        /// <summary>
        /// Expects 16 items (Three are used for now)
        /// </summary>
        public int[] PitchOffsets { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelVolumes { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelPans { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelReverbs { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelChoruses { get; set; }

        /// <summary>
        /// Expects 16 items
        /// </summary>
        public int[] ChannelModulations { get; set; }

        public List<(string name, (int controllerID, int value)[])> ChannelCustomControls { get; set; }
        
        /// <summary>
        /// Configures which events the input devices may send, are processed [true] or ignored [false] by the synth.
        /// Expects 9 items: {pitch wheel, velocity, instrument change, volume, pan/balance, Reverb, chorus, phaser, modulation}
        /// </summary>
        public bool[] InDeviceAllowedParams { get; set; }

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
            ActiveInputDevice2Index   = -1;
            ActiveOutputDeviceIndex  = 0;
            EnableRiffs              = false;
            ChannelInstruments       = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelBanks             = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //Offset layout:                     {global octave, global transpose, ofx 3 transpose 1, ofx3 transpose 2, ..etc}
            PitchOffsets             = new int[] { 3, 0, -12, -24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelVolumes           = new int[] { 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127 };
            ChannelPans              = new int[] { 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64 };
            ChannelReverbs           = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelChoruses          = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelModulations       = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            ChannelCustomControls    = new List<(string name, (int controllerID, int value)[])>();

            InDeviceAllowedParams    = new bool[] { true, true, true, true, true, true, true, true, true };

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
                DisplayMode,PitchOffsets,ChannelInstruments,
                ChannelVolumes,ChannelCustomControls,ChannelChoruses,ChannelReverbs,
                ChannelModulations,ChannelPans,ChannelBanks, InDeviceAllowedParams
            };
            return items.Any(v => v == null);
        }

        public bool CheckForInvalidCounts()
        {
            object[] items = new object[]
            {
                PitchOffsets,ChannelInstruments,
                ChannelVolumes,ChannelChoruses,ChannelReverbs,
                ChannelModulations,ChannelPans,ChannelBanks
            };
            object[] items2 = new object[]
            {
                InDeviceAllowedParams
            };
            return items.Any(v=> ((Array)v).Length < 16) & items2.Any(v2 => ((Array)v2).Length < 9);
        }

    }

    public enum DisplayModes
    {
        Standard = 0,
        Studio = 1,
        Compact = 2
    }
}
