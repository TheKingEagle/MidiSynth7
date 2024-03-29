﻿using MidiSynth7.entities.controls;
using System;
using System.Windows.Input;

namespace MidiSynth7.components
{
    public interface ISynthView
    {
        void HandleNoteOnEvent(object sender, NoteEventArgs e);
        void HandleNoteOn_VS_Event(object sender, PKeyEventArgs e, int data2, int channel=0);
        void HandleNoteOffEvent(object sender, NoteEventArgs e);
        void HandleEvent(object sender, EventArgs e, string id="generic");

        void HandleKeyDown(object sender, KeyEventArgs e);
        void HandleKeyUp(object sender, KeyEventArgs e);
    }
}
