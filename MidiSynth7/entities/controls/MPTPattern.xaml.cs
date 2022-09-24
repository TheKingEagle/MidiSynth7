using MidiSynth7.components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for MPTPattern.xaml
    /// </summary>
    public partial class MPTPattern : ListBox
    {
        private TrackerSequence _data;
        private int _channelCount = 20;
        private int _rowcount = 32;
        private int _rowsPerBeat = 4;
        private int _rowsPerMeasure = 16;
        private SolidColorBrush bg_subdivision1 = new SolidColorBrush(Color.FromArgb(255, 24, 30, 40));
        private SolidColorBrush bg_subdivision2 = new SolidColorBrush(Color.FromArgb(255, 20, 26, 34));
        private SolidColorBrush bg_subdivisionN = new SolidColorBrush(Color.FromArgb(255, 12, 16, 20));

        private List<MPTRow> rows = new List<MPTRow>();
        public TrackerSequence Data
        {
            get => _data;
            set
            {
                _data = value;
                UpdateData();
            }
        }
        [Category("MPTPattern")]
        public int ChannelCount
        {
            get => _channelCount;
            set
            {
                _channelCount = value;
                Populate();
            }
        }
        [Category("MPTPattern")]

        public int RowsPerBeat
        {
            get => _rowsPerBeat;
            set
            {
                _rowsPerBeat = value;
                Populate();
            }
        }
        [Category("MPTPattern")]

        public int RowsPerMeasure
        {
            get => _rowsPerMeasure;
            set
            {
                _rowsPerMeasure = value;
                Populate();
            }
        }
        [Category("MPTPattern")]

        public int RowCount
        {
            get => _rowcount;
            set
            {
                _rowcount = value;
                Populate();
            }
        }
        public MPTPattern()
        {
            InitializeComponent();
            Populate();
        }
        public void Populate()
        {
            //Container.Children.Clear();
            rows.Clear();
            double h = 21;

            for (int i = 0; i < RowCount; i++)
            {
                MPTRow row = new MPTRow(i, ChannelCount);
                row.Background = bg_subdivisionN;
                if (i % RowsPerBeat == 0)
                {
                    row.Background = bg_subdivision2;
                }
                if (i % RowsPerMeasure == 0)
                {
                    row.Background = bg_subdivision1;
                }

                rows.Add(row);
            }
            ItemsSource = rows;
        }

        public void UpdateData()
        {

        }
    }


}
