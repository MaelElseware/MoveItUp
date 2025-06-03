using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TriviaExercise.Models;

namespace TriviaExercise
{
    public partial class ExerciseWindow : Window
    {
        private Exercise exercise;
        private DispatcherTimer countdownTimer;
        private int remainingSeconds;

        public ExerciseWindow(bool isCorrect, Exercise exercise)
        {
            InitializeComponent();
            this.exercise = exercise;

            ResultTextBlock.Text = isCorrect ? "Correct! 🎉" : "Wrong answer 😅";
            ResultTextBlock.Foreground = isCorrect ?
                new SolidColorBrush(Colors.Green) :
                new SolidColorBrush(Colors.Red);

            ExerciseTextBlock.Text = exercise.Description;

            // Load exercise image if available
            LoadExerciseImage();

            if (exercise.DurationSeconds.HasValue)
            {
                StartCountdown();
            }
        }

        /// <summary>
        /// Load and display the exercise image if specified
        /// </summary>
        private void LoadExerciseImage()
        {
            if (string.IsNullOrEmpty(exercise.ImageFileName))
            {
                // No image specified, keep image area hidden
                ImageBorder.Visibility = Visibility.Collapsed;
                return;
            }

            // Validate filename
            if (!Helpers.ImageHelper.IsValidImageFileName(exercise.ImageFileName))
            {
                ShowImageError($"Invalid image filename: {exercise.ImageFileName}");
                System.Diagnostics.Debug.WriteLine($"Invalid exercise image filename: '{exercise.ImageFileName}'");
                return;
            }

            // Try to load the image using ImageHelper
            var bitmap = Helpers.ImageHelper.LoadExerciseImage(exercise.ImageFileName);

            if (bitmap != null)
            {
                // Successfully loaded image
                ExerciseImage.Source = bitmap;
                ImageBorder.Visibility = Visibility.Visible;
                ImageErrorText.Visibility = Visibility.Collapsed;

                System.Diagnostics.Debug.WriteLine($"Successfully loaded exercise image: {exercise.ImageFileName}");
            }
            else
            {
                // Failed to load image
                string imagePath = Helpers.ImageHelper.GetImagePath(exercise.ImageFileName);
                bool fileExists = Helpers.ImageHelper.ExerciseImageExists(exercise.ImageFileName);

                if (!fileExists)
                {
                    ShowImageError($"Image not found: {exercise.ImageFileName}");
                    System.Diagnostics.Debug.WriteLine($"Exercise image not found: {imagePath}");
                }
                else
                {
                    ShowImageError("Failed to load image");
                    System.Diagnostics.Debug.WriteLine($"Failed to load exercise image: {imagePath}");
                }
            }
        }

        /// <summary>
        /// Show an error message when image loading fails
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void ShowImageError(string errorMessage)
        {
            ImageErrorText.Text = errorMessage;
            ImageErrorText.Visibility = Visibility.Visible;
            ExerciseImage.Source = null;
            ImageBorder.Visibility = Visibility.Visible; // Still show the border with error text
        }

        private void StartCountdown()
        {
            remainingSeconds = exercise.DurationSeconds.Value;
            TimerTextBlock.Visibility = Visibility.Visible;
            UpdateTimerDisplay();

            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            UpdateTimerDisplay();

            if (remainingSeconds <= 0)
            {
                countdownTimer.Stop();
                TimerTextBlock.Text = "Time's up!";
                TimerTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        private void UpdateTimerDisplay()
        {
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            TimerTextBlock.Text = $"{minutes:D2}:{seconds:D2}";
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            countdownTimer?.Stop();
            base.OnClosing(e);
        }
    }
}