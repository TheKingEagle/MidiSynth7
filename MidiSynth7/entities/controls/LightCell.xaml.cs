using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MidiSynth7.entities.controls
{
    /// <summary>
    /// Interaction logic for LightCell.xaml
    /// </summary>
    public partial class LightCell : UserControl
    {
        int _rows = 1;
        int _cols = 4;
        int _marker = 4;
        public int Rows { get { return _rows; } set { _rows = value; PopulateLights(); } }
        public int Columns { get { return _cols; } set { _cols = value; PopulateLights(); } }
        public int Marker { get { return _marker; } set { if (value > _cols || value < 1) { throw new ArgumentException("Marker out of range of column count"); } _marker = value; PopulateLights(); } }

        public string Header { get { return GB_LCBox.Header.ToString(); } set { GB_LCBox.Header = value; } }

        public bool EnableClick { get; set; } = false;

        public event EventHandler<LightCellEventArgs> LightIndexChanged;
        public LightCell()
        {
            InitializeComponent();
            PopulateLights();
        }

        private void PopulateLights()
        {
            WP_LightsContainer.Width = Columns * 18;
            WP_LightsContainer.Height = Rows * 18;
            WP_LightsContainer.Children.Clear();
            //populate lights
            for (int i = 0; i < (Rows * Columns) + 1; i++)
            {
                Ellipse light = new Ellipse();
                light.Fill = FindResource("CH_IND_OFF") as Brush;
                light.Stroke = FindResource("CH_IND_STROKE") as Brush;
                light.MouseUp += (object s, MouseButtonEventArgs e) => {
                    if(EnableClick) SetLight(WP_LightsContainer.Children.IndexOf((Ellipse)s),false);
                };
                if (i % _marker == 0)
                {
                    light.Stroke = FindResource("CH_IND_MARKER") as Brush;
                }

                light.StrokeThickness = 1.4;
                light.Width = 14;
                light.Height = 14;
                light.Margin = new Thickness(2, 2, 2, 2);
                WP_LightsContainer.Children.Add(light);
            }

        
    }

        public void SetLight(int index, bool SupressEvent = true)
        {
            foreach (Ellipse item in WP_LightsContainer.Children)
            {
                item.Fill = FindResource("CH_IND_OFF") as Brush;
            }
            if (index > WP_LightsContainer.Children.Count || index < 0)
            {
                return;
            }
                ((Ellipse)WP_LightsContainer.Children[index]).Fill = FindResource("CH_IND_ON") as Brush;

            if (!SupressEvent)
            {
                LightIndexChanged?.Invoke(this, new LightCellEventArgs(index));
            }
        }
    }

    public class LightCellEventArgs : EventArgs
    { 
        public int LightIndex { get; private set; }

        public LightCellEventArgs(int index)
        {
            LightIndex = index;
        }
    }
}
