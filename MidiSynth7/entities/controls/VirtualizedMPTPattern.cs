using MidiSynth7.components;
using System.Windows;
using System.Windows.Media;

namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Eventually, this is probably going to replace the currently implemented pattern editor...
    /// ...when I get tired of the craptastic performance
    /// </summary>
    public class VirtualizedMPTPattern : FrameworkElement
    {
        public TrackerPattern PatternData { get; private set; }

        private SolidColorBrush bg_subdivision1 = new SolidColorBrush(Color.FromArgb(255, 24, 30, 40));
        private SolidColorBrush bg_subdivision2 = new SolidColorBrush(Color.FromArgb(255, 20, 26, 34));
        private SolidColorBrush bg_subdivisionN = new SolidColorBrush(Color.FromArgb(255, 12, 16, 20));

        public VirtualizedMPTPattern(TrackerPattern src)
        {
            PatternData = src;
            Width = 120 * PatternData.ChannelCount;
            Height = 22 * PatternData.RowCount;
        }
        public VirtualizedMPTPattern()
        {
            TrackerSequence seq = new TrackerSequence()
            {
                Instruments = new System.Collections.Generic.List<TrackerInstrument>(),
                Patterns = new System.Collections.Generic.List<TrackerPattern>(),
                SelectedInstrument = -1,
                SelectedOctave = 3,
                SequenceName = "Test sequence",
            };
            seq.Patterns.Add(TrackerPattern.GetEmptyPattern(seq, 32, 20));
            PatternData = seq.Patterns[0];
            Width = 120 * PatternData.ChannelCount;
            Height = 22 * PatternData.RowCount;
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (PatternData == null) return;
            for (int i = 0; i < PatternData.RowCount; i++)
            {
                Brush bg = bg_subdivisionN;
                if (i % PatternData.RowsPerBeat == 0)
                {
                    bg = bg_subdivision2;
                }
                if (i % PatternData.RowsPerMeasure == 0)
                {
                    bg = bg_subdivision1;
                }
                dc.DrawRectangle(bg, null, new Rect(0, i * 22, 126 * PatternData.ChannelCount, 22));
                foreach (SeqData item in PatternData.Rows[i].Notes)
                {
                    item.Render(dc);
                }
            }
        }
    }
}
