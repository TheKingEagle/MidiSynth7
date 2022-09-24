using System;
using System.Collections.Generic;
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
    /// Interaction logic for LightCell.xaml
    /// </summary>
    public partial class LightCell : UserControl
    {
        int _rows = 1;
        int _cols = 4;
        int _marker = 4;
        public int Rows { get { return _rows; } set { _rows = value; PopulateLights(); } }
        public int Columns { get { return _cols; } set { _cols = value; PopulateLights(); } }
        public int Marker { get { return _marker; } set { if (value > _cols || value < 1) { throw  new ArgumentException("Marker out of range of column count"); } _marker = value; PopulateLights(); } }

        public string Header { get { return GB_LCBox.Header.ToString(); } set { GB_LCBox.Header = value; } }
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

        public void SetLight(int index)
        {
            foreach (Ellipse item in WP_LightsContainer.Children)
            {
                item.Fill = FindResource("CH_IND_OFF") as Brush;
            }
            if (index > WP_LightsContainer.Children.Count || index <0 )
            {
                return;
            }
            ((Ellipse)WP_LightsContainer.Children[index]).Fill = FindResource("CH_IND_ON") as Brush;
        }
    }
}
