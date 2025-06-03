using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TriviaExercise
{
    public partial class DrinkReminderWindow : Window
    {
        private DispatcherTimer autoCloseTimer;
        private int remainingSeconds = 10; // Auto-close after 10 seconds

        public DrinkReminderWindow()
        {
            InitializeComponent();
            PositionWindowBottomRight();
            StartAutoCloseTimer();
        }

        /// <summary>
        /// Position the window in the bottom right corner of the screen
        /// </summary>
        private void PositionWindowBottomRight()
        {
            // Get the working area (screen minus taskbar)
            var workingArea = SystemParameters.WorkArea;

            // Position in bottom right corner with some margin
            this.Left = workingArea.Right - this.Width - 20;
            this.Top = workingArea.Bottom - this.Height - 20;
        }

        /// <summary>
        /// Start the auto-close countdown timer
        /// </summary>
        private void StartAutoCloseTimer()
        {
            autoCloseTimer = new DispatcherTimer();
            autoCloseTimer.Interval = TimeSpan.FromSeconds(1);
            autoCloseTimer.Tick += AutoCloseTimer_Tick;
            autoCloseTimer.Start();

            UpdateCountdownDisplay();
        }

        /// <summary>
        /// Handle auto-close timer tick
        /// </summary>
        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;

            if (remainingSeconds <= 0)
            {
                // Time's up - close the window
                autoCloseTimer.Stop();
                Close();
            }
            else
            {
                UpdateCountdownDisplay();
            }
        }

        /// <summary>
        /// Update the countdown display
        /// </summary>
        private void UpdateCountdownDisplay()
        {
            CountdownTextBlock.Text = $"Auto-closing in {remainingSeconds} second{(remainingSeconds != 1 ? "s" : "")}...";
        }

        /// <summary>
        /// Handle "Got it!" button click
        /// </summary>
        private void GotItButton_Click(object sender, RoutedEventArgs e)
        {
            autoCloseTimer?.Stop();
            Close();
        }

        /// <summary>
        /// Handle close button click
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            autoCloseTimer?.Stop();
            Close();
        }

        /// <summary>
        /// Allow dragging the window by clicking anywhere on it
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Stop the timer when the window is closing
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            autoCloseTimer?.Stop();
            base.OnClosing(e);
        }

        /// <summary>
        /// Handle window loaded event to ensure proper positioning
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Reposition after the window is fully initialized
            // This ensures correct positioning even if the window size changed during initialization
            PositionWindowBottomRight();
        }
    }
}