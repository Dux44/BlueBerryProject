using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BlueBerryProject.FormConstruct
{
    internal class DoubleSlider : Canvas
    {
        private readonly Rectangle track;
        
        private readonly Thumb lowerThumb;
        private readonly Thumb upperThumb;

        private double lowerValue = 0;
        private double upperValue = 0;

       // public double MinThumbDistance { get; set; } = 0;
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 0;
        public double LowerValue
        {
            get { return lowerValue; }
            set 
            { 
                lowerValue = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public double UpperValue
        {
            get { return upperValue; }
            set
            {
                upperValue = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public double TrackWidth => Width-10;

        public event EventHandler ValueChanged; //подія valueChanged 
        public DoubleSlider()
        {
            Height = 50;
            Background = Brushes.Transparent;

            //лінія треку
            track = new Rectangle
            {
                Height = 4,
                Fill = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Children.Add(track);

            //свторюю лівий slider
            lowerThumb = CreateThumb();
            lowerThumb.DragDelta += LowerThumb_DragDelta;
            Children.Add(lowerThumb);

            //створюю правий slider
            upperThumb = CreateThumb();
            upperThumb.DragDelta += UpperThumb_DragDelta;
            Children.Add(upperThumb);

            this.Loaded += (s, e) => UpdateUI();
        }

        private void UpperThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newX = Canvas.GetLeft(upperThumb) + e.HorizontalChange;
            newX = Math.Max(Canvas.GetLeft(lowerThumb) + upperThumb.Width, Math.Min(newX, TrackWidth));

            UpperValue = MinValue + (newX / TrackWidth) * (MaxValue - MinValue);
            Canvas.SetLeft(upperThumb, newX);

            UpdateUI();
        }

        private void LowerThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newX = Canvas.GetLeft(lowerThumb) + e.HorizontalChange;
            newX = Math.Max(0, Math.Min(newX, Canvas.GetLeft(upperThumb) - lowerThumb.Width));

            LowerValue = MinValue + (newX / TrackWidth) * (MaxValue - MinValue);
            Canvas.SetLeft(lowerThumb, newX);

            UpdateUI();
        }
        private Thumb CreateThumb()
        {
            return new Thumb
            {
                Width = 10,
                Height = 20,
                Background = Brushes.DarkGray,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        public void UpdateUI()
        {
            if (double.IsNaN(ActualWidth) || ActualWidth == 0) return; // Чекаємо, поки UI буде готовий

            
            double leftPos = (LowerValue - MinValue) / (MaxValue - MinValue) * TrackWidth;
            double rightPos = (UpperValue - MinValue) / (MaxValue - MinValue) * TrackWidth;

            Canvas.SetLeft(track, 10);
            track.Width = TrackWidth;

            Canvas.SetLeft(lowerThumb, leftPos);
            Canvas.SetTop(lowerThumb, (Height - lowerThumb.Height) / 2 - track.Height / 2 - 19); //розміщення lowerthumb на track

            Canvas.SetLeft(upperThumb, rightPos);
            Canvas.SetTop(upperThumb, (Height - upperThumb.Height) / 2 - track.Height / 2 - 19); //розміщення upperthumb на track

        }
    }
}
