using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DailyToDo.Controls
{
    public partial class ConfettiControl : UserControl
    {
        private readonly Random _random = new Random();
        private readonly List<Color> _confettiColors = new List<Color>
        {
            Color.FromRgb(255, 107, 107), // Red
            Color.FromRgb(255, 195, 0),   // Yellow
            Color.FromRgb(72, 219, 251),  // Cyan
            Color.FromRgb(255, 159, 243), // Pink
            Color.FromRgb(147, 51, 234),  // Purple
            Color.FromRgb(34, 197, 94),   // Green
            Color.FromRgb(251, 146, 60),  // Orange
        };

        public ConfettiControl()
        {
            InitializeComponent();
        }

        public void Burst()
        {
            // Clear any existing confetti
            ConfettiCanvas.Children.Clear();

            // Create 20-30 confetti particles
            int particleCount = _random.Next(20, 31);
            
            for (int i = 0; i < particleCount; i++)
            {
                CreateConfettiParticle();
            }
        }

        private void CreateConfettiParticle()
        {
            // Random shape: circle or rectangle
            Shape particle;
            if (_random.Next(2) == 0)
            {
                // Circle
                particle = new Ellipse
                {
                    Width = _random.Next(6, 12),
                    Height = _random.Next(6, 12)
                };
            }
            else
            {
                // Rectangle
                double size = _random.Next(6, 12);
                particle = new Rectangle
                {
                    Width = size,
                    Height = size * _random.NextDouble() * 0.5 + 0.5 // Random aspect ratio
                };
            }

            // Random color
            particle.Fill = new SolidColorBrush(_confettiColors[_random.Next(_confettiColors.Count)]);
            particle.Opacity = 0.9;

            // Starting position (center of the checkbox area)
            double startX = 20;
            double startY = ActualHeight / 2;

            Canvas.SetLeft(particle, startX);
            Canvas.SetTop(particle, startY);

            // Add rotation transform
            particle.RenderTransform = new RotateTransform(0);
            particle.RenderTransformOrigin = new Point(0.5, 0.5);

            ConfettiCanvas.Children.Add(particle);

            // Create animation
            AnimateParticle(particle, startX, startY);
        }

        private void AnimateParticle(Shape particle, double startX, double startY)
        {
            // Random direction and distance
            double angle = _random.NextDouble() * Math.PI * 2; // Random angle in radians
            double distance = _random.Next(60, 120); // Distance to travel
            
            double endX = startX + Math.Cos(angle) * distance;
            double endY = startY + Math.Sin(angle) * distance;

            // Duration
            double duration = _random.NextDouble() * 0.4 + 0.6; // 0.6-1.0 seconds

            // Create storyboard
            Storyboard storyboard = new Storyboard();

            // X position animation
            DoubleAnimation xAnimation = new DoubleAnimation
            {
                From = startX,
                To = endX,
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(xAnimation, particle);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(Canvas.Left)"));

            // Y position animation with gravity
            DoubleAnimation yAnimation = new DoubleAnimation
            {
                From = startY,
                To = endY + 40, // Add gravity effect
                Duration = TimeSpan.FromSeconds(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(yAnimation, particle);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(Canvas.Top)"));

            // Rotation animation
            DoubleAnimation rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = _random.Next(-360, 360),
                Duration = TimeSpan.FromSeconds(duration)
            };
            Storyboard.SetTarget(rotationAnimation, particle);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            // Opacity fade out
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0.9,
                To = 0,
                BeginTime = TimeSpan.FromSeconds(duration * 0.5),
                Duration = TimeSpan.FromSeconds(duration * 0.5)
            };
            Storyboard.SetTarget(opacityAnimation, particle);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(xAnimation);
            storyboard.Children.Add(yAnimation);
            storyboard.Children.Add(rotationAnimation);
            storyboard.Children.Add(opacityAnimation);

            // Remove particle after animation completes
            storyboard.Completed += (s, e) =>
            {
                ConfettiCanvas.Children.Remove(particle);
            };

            storyboard.Begin();
        }
    }
}
