using System;
using System.Windows;
using System.Windows.Media;
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

            if (exercise.DurationSeconds.HasValue)
            {
                StartCountdown();
            }
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