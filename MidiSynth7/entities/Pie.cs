using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
namespace MidiSynth7.entities
{
    public class Pie:Shape
    {
        public Pie()
        {
            Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF000000"));
            Stroke = Brushes.Black;
            StrokeThickness = 2;
            
        }
        protected override Geometry DefiningGeometry
        {
            get
            {
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    DrawGeometry(context);
                }

                geometry.Freeze();
                return geometry;
            }
        }

        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Radius { get; set; }
        public double Angle { get { return (double)GetValue(AngleProperty); } set { if (value > 127 && value <= 358) { SetValue(AngleProperty, value); } } }
        //FIXME: it fills backwards and IDK why.
        public double Rotation { get { return (double)GetValue(RotationProperty); } set { if (value < -127 && value >= -358) { SetValue(RotationProperty, -value); value = -value; } } }

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(Pie), 
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));


        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(double), typeof(Pie),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
       
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
        } 

        private void DrawGeometry(StreamGeometryContext context)
        {
            Point startPoint = new Point(CenterX, CenterY);

            Point outerArcStartPoint = ComputeCoords(-Rotation, Radius);
            outerArcStartPoint.Offset(CenterX, CenterY);

            Point outerArcEndPoint = ComputeCoords(-Rotation + Angle, Radius);
            outerArcEndPoint.Offset(CenterX, CenterY);
            bool largeArc = Angle > 180;
            Size outerArcSize = new Size(Radius, Radius);

            context.BeginFigure(startPoint, true, true);
            context.LineTo(outerArcStartPoint, true, true);
            context.ArcTo(outerArcEndPoint, outerArcSize, 0, largeArc, SweepDirection.Clockwise, true, true);
            
        }

        private Point ComputeCoords(double angle, double radius)
        {
            double anglerad = (Math.PI / 180) * (angle - 90);
            double x = radius * Math.Cos(anglerad);
            double y = radius * Math.Sin(anglerad);
            return new Point(x,y);
        }
    }
}
