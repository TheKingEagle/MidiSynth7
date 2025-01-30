using Sanford.Multimedia.Midi;
using System.Collections.Generic;
using System.Linq;

namespace MidiSynth7.components.sequencer
{
    public class SequenceProcessor
    {
        public static List<ChannelMessage> GetAllChannelMessages(Sequence sequence)
        {
            var messages = new List<ChannelMessage>();

            if (sequence?.Patterns == null)
                return messages;

            foreach (var pattern in sequence.Patterns)
            {
                if (pattern?.Steps == null)
                    continue;

                foreach (var step in pattern.Steps)
                {
                    if (step?.MidiMessages == null)
                        continue;

                    messages.AddRange(step.MidiMessages);
                }
            }

            return messages;
        }

        public static int? GetRootKey(Sequence sequence)
        {
            var messages = GetAllChannelMessages(sequence);

            // Find the first Note On message
            var noteOnMessage = messages
                .FirstOrDefault(m => m.Command == ChannelCommand.NoteOn && m.Data2 > 0); // Data2 > 0 ensures it's not a Note Off

            return noteOnMessage?.Data1; // Data1 represents the note number (MIDI key)
        }
    }
}
