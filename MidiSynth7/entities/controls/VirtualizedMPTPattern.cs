using MidiSynth7.components;
using System;
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
        public int ActiveRow { get; set; }

        public int RowCount { get => PatternData?.RowCount ?? 0; }
        public int ChannelCount { get => PatternData?.ChannelCount ?? 0; }
        public TrackerPattern PatternData { get; private set; }

        private SolidColorBrush bg_subdivision1 = new SolidColorBrush(Color.FromArgb(255, 24, 30, 40));
        private SolidColorBrush bg_subdivision2 = new SolidColorBrush(Color.FromArgb(255, 20, 26, 34));
        private SolidColorBrush bg_subdivisionN = new SolidColorBrush(Color.FromArgb(255, 12, 16, 20));
        private SolidColorBrush bg_subdivisionH = new SolidColorBrush(Color.FromArgb(255, 0, 67, 159));
        public event EventHandler<SelectionEventArgs> ActiveRowChanged;
        public VirtualizedMPTPattern(TrackerPattern src)
        {
            PatternData = src;
            Width = 120 * PatternData.ChannelCount;
            Height = 22 * PatternData.RowCount;
            for (int i = 0; i < PatternData.RowCount; i++)
            {
                UpdateRow(i, i == ActiveRow);
            }
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
                PatternData.Rows[i].Render(dc);
            }
        }

        public void UpdateRow(int i, bool hot = false)
        {
            SolidColorBrush bg = bg_subdivisionN;
            if (i % PatternData.RowsPerBeat == 0)
            {
                bg = bg_subdivision2;
            }
            if (i % PatternData.RowsPerMeasure == 0)
            {
                bg = bg_subdivision1;
            }
            if (hot)
            {
                bg = bg_subdivisionH;
                ActiveRowChanged?.Invoke(this, new SelectionEventArgs(i));
            }
            PatternData.Rows[i].UpdateRow(i,bg,hot);
        }
        public void UpdateBit(int col, int row, bool hot = false)
        {
            PatternData.Rows[row].Notes[col].UpdateBit(hot);
        }
    }
    public class SelectionEventArgs : EventArgs
    {
        public int selectedIndex { get; private set; }

        public SelectionEventArgs(int sel)
        {
            selectedIndex = sel;
        }
    }
}
