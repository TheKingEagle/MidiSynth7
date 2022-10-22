using MidiSynth7.components;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        private FrameworkElement _parent;
        private SolidColorBrush bg_subdivision1 = new SolidColorBrush(Color.FromArgb(255, 24, 30, 40));
        private SolidColorBrush bg_subdivision2 = new SolidColorBrush(Color.FromArgb(255, 20, 26, 34));
        private SolidColorBrush bg_subdivisionN = new SolidColorBrush(Color.FromArgb(255, 12, 16, 20));
        private SolidColorBrush bg_subdivisionH = new SolidColorBrush(Color.FromArgb(255, 0, 67, 159));
        private SolidColorBrush bg_subdivisionP = new SolidColorBrush(Color.FromArgb(255, 57, 57, 57));
        
        public event EventHandler<ActiveRowEventArgs> ActiveRowChanged;

        private Point SelPoint1 = new Point(0, 0);
        private Point SelPoint2 = new Point(0, 0);
        public Rect GetSelectedBounds()
        {
            return new Rect(SelPoint1, SelPoint2);
        }
        private bool mouseDowned = false;

        public VirtualizedMPTPattern(TrackerPattern src, FrameworkElement parent)
        {
            Focusable = true;
            Focus();
            PatternData = src;
            _parent = parent;
            Width = SeqData.Width * PatternData.ChannelCount;
            Height = SeqData.Height * PatternData.RowCount;
            for (int i = 0; i < PatternData.RowCount; i++)
            {
                UpdateRow(i, i == ActiveRow);
            }
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

        public void RaiseKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case Key.Up:
                    UpdateRow(ActiveRow, false);
                    ActiveRow--;
                    if (ActiveRow < 0)
                    {
                        ActiveRow = RowCount - 1;
                    }
                    UpdateRow(ActiveRow, true);
                    break;
                case Key.Down:
                    UpdateRow(ActiveRow, false);
                    ActiveRow++;
                    if (ActiveRow > RowCount - 1)
                    {
                        //TODO: Load next pattern
                        ActiveRow = 0;
                    }
                    UpdateRow(ActiveRow, true);
                    break;
                case Key.Left:
                    break;
                case Key.Right:
                    break;
                case Key.L:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        //ActivePattern.SelectActiveChannel();
                        return;
                    }
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        //ActivePattern.SelectActiveChannelBit();
                        return;

                    }
                    break;
                case Key.Oem5:
                    break;
                default:
                    break;
            }
        }


        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (!mouseDowned)
            {
                SelPoint1 = e.GetPosition(this);
                SelPoint2 = e.GetPosition(this);
            }
            mouseDowned = true;
            CaptureMouse();
            
            GetSelection(GetSelectedBounds());

        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            ReleaseMouseCapture();
            mouseDowned = false;
            //SelPoint1 = e.GetPosition(Frame);
            //SelPoint2 = e.GetPosition(Frame);
            
            bool f = GetSelection(GetSelectedBounds());
            if (!f)
            {
                //set active row.
                UpdateRow(ActiveRow);
                ActiveRow = PatternData.Rows.Where(x => x.rowbounds.IntersectsWith(GetSelectedBounds())).First().Notes[0].Row;
                UpdateRow(ActiveRow,true);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDowned)
            {
                SelPoint2 = e.GetPosition(this);
                GetSelection(GetSelectedBounds());
                Rect f = new Rect(e.GetPosition(this), new Size(SeqData.Width, SeqData.Height));
                _parent.BringIntoView(f);
            }
        }

        public void UpdateRow(int i, bool hot = false, bool ignoreBits = false)
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
                ActiveRowChanged?.Invoke(this, new ActiveRowEventArgs(i));
            }
            if (ignoreBits && hot)
            {
                bg = bg_subdivisionP;
            }
            if (i > PatternData.RowCount - 1) return;
            PatternData.Rows[i].UpdateRow(i,bg,hot,ignoreBits);
        }

        public void UpdateBit(int col, int row, bool hot = false)
        {
            PatternData.Rows[row].Notes[col].UpdateBit(hot);
        }
        /// <summary>
        /// Get selection from bounds
        /// </summary>
        /// <param name="bounds">the selection bounds</param>
        /// <returns>true if multiple bits are selected</returns>
        public bool GetSelection(Rect bounds)
        {
            
            var sel = PatternData.Rows.Where(x => x.rowbounds.IntersectsWith(bounds));
            if(bounds.Height < 2)
            {
                 sel = PatternData.Rows.Where(x => x.rowbounds.IntersectsWith(bounds)).Take(1);
            }
            var ssel = PatternData.Rows.Where(zz=>zz.Notes.Where(pn=>pn.SelectedBits.Where(ff=>ff).Count()>0).Count()>0);
            foreach (var item in ssel)
            {
                var notes = item.Notes.Where(nn=> nn.SelectedBits.Where(xx=> xx).Count() > 0 && !nn.SqDatBit_Bounds.IntersectsWith(bounds));
                foreach (var note in notes)
                {
                    note.ClearSelection(ActiveRow);
                }
            }
            foreach (var item in sel)
            {
                item.DetectSelection(bounds,ActiveRow);
            }
            return bounds.Height > 2 || bounds.Width > 2;

        }
    }

    public class ActiveRowEventArgs : EventArgs
    {
        public int selectedIndex { get; private set; }

        public ActiveRowEventArgs(int sel)
        {
            selectedIndex = sel;
        }
    }
}
