using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        public int Marker { get { return _marker; } set { if (value > _cols || value < 1) { _marker = _cols; } _marker = value; PopulateLights(); } }

        public string Header { get { return GB_LCBox.Header.ToString(); } set { GB_LCBox.Header = value; } }

        public bool EnableClick { get; set; } = false;

        public int LightIndex { get; private set; }

        public bool FlashMode { get; set; } = false;

        public Brush LitBrush { get; set; }

        public Brush OffBrush { get; set; }

        public event EventHandler<LightCellEventArgs> LightIndexChanged;
        public LightCell()
        {
            LitBrush = TryFindResource("CH_IND_ON") as Brush;
            OffBrush = TryFindResource("CH_IND_OFF") as Brush;
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
                light.Fill = OffBrush as Brush;
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
            if (FlashMode)
            {
                FlashLight(index, SupressEvent);
                return;
            }
            foreach (Ellipse item in WP_LightsContainer.Children)
            {
                // Stop any running animations
                item.BeginAnimation(Ellipse.FillProperty, null);
                item.Fill = OffBrush as Brush;
            }
            if (index > WP_LightsContainer.Children.Count || index < 0)
            {
                return;
            }
                ((Ellipse)WP_LightsContainer.Children[index]).Fill = LitBrush as Brush;
            LightIndex = index;
            if (!SupressEvent)
            {
                LightIndexChanged?.Invoke(this, new LightCellEventArgs(index));
            }
        }

        public void FlashLight(int index, bool SuppressEvent = true)
        {
            // Stop animations and reset all lights to "off"
            foreach (Ellipse item in WP_LightsContainer.Children)
            {
                // Stop any running animations
                item.BeginAnimation(Ellipse.FillProperty, null);

                // Reset the Fill to "off"
                item.Fill = OffBrush as Brush;
            }

            // Ensure the index is within bounds
            if (index >= WP_LightsContainer.Children.Count || index < 0)
            {
                return;
            }

            // Get the target light
            var targetLight = (Ellipse)WP_LightsContainer.Children[index];

            // Create a storyboard for animating the Fill property
            var storyboard = new Storyboard();

            // Define the animation to switch between brushes
            var brushAnimation = new ObjectAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromSeconds(0.5), // Full cycle: "on" to "off"
                RepeatBehavior = RepeatBehavior.Forever // Keep repeating
            };

            // KeyFrame 1: Set to "on" brush
            brushAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                Value = LitBrush
            });

            // KeyFrame 2: Set to "off" brush
            brushAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.25)), // Halfway mark
                Value = OffBrush
            });

            // Apply the animation to the Fill property
            Storyboard.SetTarget(brushAnimation, targetLight);
            Storyboard.SetTargetProperty(brushAnimation, new PropertyPath(Ellipse.FillProperty));

            storyboard.Children.Add(brushAnimation);

            // Start the animation
            storyboard.Begin();

            // Update LightIndex
            LightIndex = index;

            // Trigger the LightIndexChanged event if not suppressed
            if (!SuppressEvent)
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
