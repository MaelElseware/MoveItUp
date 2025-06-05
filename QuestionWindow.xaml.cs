using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TriviaExercise.Models;
using TriviaExercise;
using System.Collections.Generic;
using TriviaExercise.Helpers;

namespace TriviaExercise
{
    public partial class QuestionWindow : Window
    {
        private Question question;
        private TriviaData triviaData;
        private ExerciseDifficultyMode exerciseDifficultyMode;
        private PlayerProgress playerProgress;
        private Random random = new Random();

        private ScheduleHelper scheduleHelper;

        private static bool ShowAnswerByDefault = false;
        // control default answer display
        private bool showAnswersDuringDelay = false;

        // Timer related fields
        private DispatcherTimer questionTimer;
        private int remainingSeconds = 60; // 1 minute
        private int totalSeconds = 60;
        private bool isAnswered = false;
        private bool answeredQuickly = false; // Answered within first 15 seconds
        private DateTime questionStartTime;

        // Event to notify when question is answered
        public event EventHandler<QuestionAnsweredEventArgs> QuestionAnswered;
        // and exercise
        public event EventHandler<ExerciseRequestedEventArgs> ExerciseRequested;

        public QuestionWindow(Question question, TriviaData triviaData, ExerciseDifficultyMode exerciseDifficultyMode, PlayerProgress playerProgress, ScheduleHelper scheduleHelper)
        {
            InitializeComponent();
            this.question = question;
            this.triviaData = triviaData;
            this.exerciseDifficultyMode = exerciseDifficultyMode;
            this.playerProgress = playerProgress;
            this.scheduleHelper = scheduleHelper;
            this.showAnswersDuringDelay = ShowAnswerByDefault || question.Category == QuestionCategory.Math;
            LoadQuestion();
            StartTimer();
        }

        // Backward compatibility constructor (for existing code that doesn't specify progress)
        public QuestionWindow(Question question, TriviaData triviaData, ExerciseDifficultyMode exerciseDifficultyMode, PlayerProgress playerProgress)
            : this(question, triviaData, exerciseDifficultyMode, playerProgress, null)
        {
        }

        public QuestionWindow(Question question, TriviaData triviaData, ExerciseDifficultyMode exerciseDifficultyMode)
            : this(question, triviaData, exerciseDifficultyMode, null, null)
        {
        }

        public QuestionWindow(Question question, TriviaData triviaData)
            : this(question, triviaData, ExerciseDifficultyMode.MatchQuestion, null, null)
        {
        }

        private void LoadQuestion()
        {
            // Set category display
            string categoryIcon = CategoryHelper.GetCategoryIcon(question.Category);
            string categoryName = CategoryHelper.GetCategoryDisplayName(question.Category);
            CategoryTextBlock.Text = $"{categoryIcon} {categoryName}";

            // Set question text
            QuestionTextBlock.Text = question.Text;

            // Clear and populate answer buttons
            AnswersPanel.Children.Clear();

            // Define the styles for each answer option
            string[] styleNames = { "AnswerAButton", "AnswerBButton", "AnswerCButton", "AnswerDButton" };
            char[] answerLetters = { 'A', 'B', 'C', 'D' };

            for (int i = 0; i < question.Answers.Count && i < 4; i++)
            {
                var button = new Button
                {
                    Content = $"{answerLetters[i]}. {question.Answers[i]}",
                    Height = 45,
                    Margin = new Thickness(0, 3, 0, 3),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Tag = i // Store the answer index
                };

                // Apply the appropriate style based on the answer index
                if (i < styleNames.Length)
                {
                    Style buttonStyle = (Style)FindResource(styleNames[i]);
                    button.Style = buttonStyle;
                }

                button.Click += AnswerButton_Click;
                AnswersPanel.Children.Add(button);
            }
        }

        private void StartTimer()
        {
            questionStartTime = DateTime.Now;
            remainingSeconds = 60;
            totalSeconds = 60;

            // Initialize progress bar
            UpdateTimerDisplay();
            UpdateProgressBar();

            questionTimer = new DispatcherTimer();
            questionTimer.Interval = TimeSpan.FromSeconds(1);
            questionTimer.Tick += QuestionTimer_Tick;
            questionTimer.Start();
        }

        private void QuestionTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            UpdateTimerDisplay();
            UpdateProgressBar();

            if (remainingSeconds <= 0)
            {
                // Time's up - treat as wrong answer
                questionTimer.Stop();
                TimerTimeUp();
            }
        }

        private void UpdateTimerDisplay()
        {
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            TimerTextBlock.Text = $"{minutes:D2}:{seconds:D2}";

            // Change color based on remaining time
            if (remainingSeconds <= 10)
            {
                // Last 10 seconds - red and urgent
                TimerTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                TimerStatusTextBlock.Text = "⚠️ URGENT";
                TimerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                TimerStatusTextBlock.Visibility = Visibility.Visible;
            }
            else if (remainingSeconds <= 30)
            {
                // Last 30 seconds - orange/yellow warning
                TimerTextBlock.Foreground = new SolidColorBrush(Colors.Orange);
                TimerStatusTextBlock.Text = "⚡ Hurry up";
                TimerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Orange);
                TimerStatusTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                // Normal time - white
                TimerTextBlock.Foreground = new SolidColorBrush(Colors.White);
                TimerStatusTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateProgressBar()
        {
            // Calculate progress percentage (starts at 100% and decreases)
            double progressPercentage = (double)remainingSeconds / totalSeconds;

            // Set the width of the progress bar
            ProgressFillBorder.Width = this.Width > 0 ?
                (this.Width - 70) * progressPercentage : // Subtract margins/padding
                430 * progressPercentage; // Fallback width

            // Change colors and text based on time remaining
            if (remainingSeconds > 45) // First 15 seconds (60-45) - Gold for quick answer zone
            {
                // Gold gradient for quick answer zone
                GradientStop1.Color = Color.FromRgb(255, 215, 0); // Gold
                GradientStop2.Color = Color.FromRgb(255, 165, 0); // Orange-Gold
                ProgressTextBlock.Text = "🚀 Quick Answer Zone!";
                ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (remainingSeconds > 30) // 45-30 seconds - Blue for normal time
            {
                // Blue gradient for normal time
                GradientStop1.Color = Color.FromRgb(52, 152, 219); // Blue
                GradientStop2.Color = Color.FromRgb(41, 128, 185); // Darker Blue
                ProgressTextBlock.Text = "Think it through...";
                ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (remainingSeconds > 10) // 30-10 seconds - Orange warning
            {
                // Orange gradient for warning
                GradientStop1.Color = Color.FromRgb(230, 126, 34); // Orange
                GradientStop2.Color = Color.FromRgb(211, 84, 0);   // Dark Orange
                ProgressTextBlock.Text = "⚡ Time running out...";
                ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);
            }
            else // Last 10 seconds - Red critical
            {
                // Red gradient for critical time
                GradientStop1.Color = Color.FromRgb(231, 76, 60);  // Red
                GradientStop2.Color = Color.FromRgb(192, 57, 43);  // Dark Red
                ProgressTextBlock.Text = "⚠️ CRITICAL!";
                ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);

                // Add pulsing effect for urgency
                if (remainingSeconds % 2 == 0)
                {
                    ProgressFillBorder.Opacity = 0.8;
                }
                else
                {
                    ProgressFillBorder.Opacity = 1.0;
                }
            }
        }

        private void TimerTimeUp()
        {
            if (isAnswered) return; // Already answered

            isAnswered = true;
            answeredQuickly = false; // Time ran out, so definitely not quick

            // Disable all answer buttons
            foreach (Button button in AnswersPanel.Children.OfType<Button>())
            {
                button.IsEnabled = false;
            }

            // Update timer display to show time's up
            TimerTextBlock.Text = "00:00";
            TimerTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            TimerStatusTextBlock.Text = "⌛ TIME'S UP!";
            TimerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            TimerStatusTextBlock.Visibility = Visibility.Visible;

            // Update progress bar to show empty/failed state
            ProgressFillBorder.Width = 0;
            GradientStop1.Color = Color.FromRgb(192, 57, 43); // Dark Red
            GradientStop2.Color = Color.FromRgb(192, 57, 43); // Dark Red
            ProgressTextBlock.Text = "⌛ TIME'S UP!";
            ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);

            // Notify that question was answered (incorrectly due to timeout)
            NotifyQuestionAnswered(false, false);

            // Show wrong answer exercise after a brief delay
            var delayTimer = new DispatcherTimer();
            delayTimer.Interval = TimeSpan.FromSeconds(2);
            delayTimer.Tick += (s, args) =>
            {
                delayTimer.Stop();
                ShowExercise(false); // Treat timeout as wrong answer
                Close();
            };
            delayTimer.Start();
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAnswered) return; // Prevent multiple answers

            var button = sender as Button;
            int selectedIndex = (int)button.Tag;
            bool isCorrect = selectedIndex == question.CorrectAnswerIndex;

            // Record the answer
            isAnswered = true;
            var answerTime = DateTime.Now;
            var timeTaken = answerTime - questionStartTime;
            answeredQuickly = timeTaken.TotalSeconds <= 15;

            // Stop the timer
            questionTimer.Stop();

            // Disable all buttons
            foreach (Button btn in AnswersPanel.Children.OfType<Button>())
            {
                btn.IsEnabled = false;
            }

            // Highlight the selected answer and correct answer
            if (showAnswersDuringDelay)
            {
                HighlightAnswers(selectedIndex, question.CorrectAnswerIndex);
            }

            // Update timer display
            if (isCorrect)
            {
                TimerTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                TimerStatusTextBlock.Text = answeredQuickly ? "🚀 QUICK!" : "✅ Correct";
                TimerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                TimerStatusTextBlock.Visibility = Visibility.Visible;

                // Update progress bar for correct answer
                if (answeredQuickly)
                {
                    GradientStop1.Color = Color.FromRgb(46, 204, 113); // Green
                    GradientStop2.Color = Color.FromRgb(39, 174, 96);  // Darker Green
                    ProgressTextBlock.Text = "🚀 LIGHTNING FAST!";
                }
                else
                {
                    GradientStop1.Color = Color.FromRgb(46, 204, 113); // Green
                    GradientStop2.Color = Color.FromRgb(39, 174, 96);  // Darker Green
                    ProgressTextBlock.Text = "✅ Well Done!";
                }
                ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                TimerTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                TimerStatusTextBlock.Text = "❌ Wrong";
                TimerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                TimerStatusTextBlock.Visibility = Visibility.Visible;

                // Update progress bar for wrong answer
                GradientStop1.Color = Color.FromRgb(231, 76, 60);  // Red
                GradientStop2.Color = Color.FromRgb(192, 57, 43);  // Dark Red
                ProgressTextBlock.Text = "❌ Try Again Next Time";
                ProgressTextBlock.Foreground = new SolidColorBrush(Colors.White);
            }

            // Log answer details (for future bonus feature)
            LogAnswerDetails(isCorrect, timeTaken, answeredQuickly);

            // Notify that question was answered
            NotifyQuestionAnswered(isCorrect, answeredQuickly);

            // Show exercise after a brief delay to let user see the result
            var delayTimer = new DispatcherTimer();
            delayTimer.Interval = TimeSpan.FromSeconds(2);
            delayTimer.Tick += (s, args) =>
            {
                delayTimer.Stop();
                ShowExercise(isCorrect);
                Close();
            };
            delayTimer.Start();
        }

        private void NotifyQuestionAnswered(bool isCorrect, bool wasQuick)
        {
            QuestionAnswered?.Invoke(this, new QuestionAnsweredEventArgs
            {
                Category = question.Category,
                QuestionDifficulty = question.Difficulty,
                IsCorrect = isCorrect,
                WasQuick = wasQuick
            });
        }

        private void HighlightAnswers(int selectedIndex, int correctIndex)
        {
            var buttons = AnswersPanel.Children.OfType<Button>().ToList();

            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == correctIndex)
                {
                    // Highlight correct answer in green
                    buttons[i].Background = new SolidColorBrush(Colors.LightGreen);
                    buttons[i].BorderBrush = new SolidColorBrush(Colors.Green);
                    buttons[i].BorderThickness = new Thickness(3);
                }
                else if (i == selectedIndex && selectedIndex != correctIndex)
                {
                    // Highlight wrong selected answer in red
                    buttons[i].Background = new SolidColorBrush(Colors.LightCoral);
                    buttons[i].BorderBrush = new SolidColorBrush(Colors.Red);
                    buttons[i].BorderThickness = new Thickness(3);
                }
                else
                {
                    // Dim other answers
                    buttons[i].Opacity = 0.6;
                }
            }
        }

        private void LogAnswerDetails(bool isCorrect, TimeSpan timeTaken, bool wasQuick)
        {
            // TODO: For future bonus feature - log answer statistics
            // This method can be used to track performance metrics:
            // - Response time
            // - Quick answer bonus eligibility
            // - Accuracy trends
            // - Category-specific performance

            string logMessage = $"[{DateTime.Now:HH:mm:ss}] Question answered: " +
                              $"Correct={isCorrect}, Time={timeTaken.TotalSeconds:F1}s, " +
                              $"Quick={wasQuick}, Category={question.Category}, " +
                              $"Difficulty={question.Difficulty}";

            System.Diagnostics.Debug.WriteLine(logMessage);

            // In the future, this could write to a file or database for analytics
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAnswered) return;

            isAnswered = true;
            answeredQuickly = false;
            questionTimer?.Stop();

            // Log skip action
            var timeTaken = DateTime.Now - questionStartTime;
            LogAnswerDetails(false, timeTaken, false);

            // Notify that question was answered (incorrectly due to skip)
            NotifyQuestionAnswered(false, false);

            Close();
        }

        private void ShowExercise(bool isCorrect)
        {
            Exercise exercise = GetRandomExercise(isCorrect, question.Difficulty);

            if (exercise != null)
            {
                // Instead of creating the exercise window directly, request it through event
                ExerciseRequested?.Invoke(this, new ExerciseRequestedEventArgs
                {
                    IsCorrect = isCorrect,
                    Exercise = exercise
                });
            }
        }

        private Exercise GetRandomExercise(bool isCorrect, DifficultyLevel questionDifficulty)
        {
            List<Exercise> availableExercises = null;

            if (isCorrect)
            {
                // For correct answers, use the correct exercise list
                if (triviaData.CorrectAnswerExercises?.Count > 0)
                {
                    availableExercises = triviaData.CorrectAnswerExercises;
                }
            }
            else
            {
                // For wrong answers, use the wrong exercise list
                if (triviaData.WrongAnswerExercises?.Count > 0)
                {
                    availableExercises = triviaData.WrongAnswerExercises;
                }
            }

            if (availableExercises == null || availableExercises.Count == 0)
            {
                return null;
            }

            // Determine target difficulty based on exercise difficulty mode
            DifficultyLevel targetDifficulty = GetTargetExerciseDifficulty(questionDifficulty);

            // If target difficulty is Mixed, return any random exercise
            if (targetDifficulty == DifficultyLevel.Mixed)
            {
                return availableExercises[random.Next(availableExercises.Count)];
            }

            // Try to find exercises matching the target difficulty first
            var matchingDifficultyExercises = availableExercises
                .Where(e => e.Difficulty == targetDifficulty)
                .ToList();

            if (matchingDifficultyExercises.Count > 0)
            {
                return matchingDifficultyExercises[random.Next(matchingDifficultyExercises.Count)];
            }

            // If no matching difficulty exercises, try nearby difficulties
            var nearbyDifficultyExercises = GetNearbyDifficultyExercises(availableExercises, targetDifficulty);

            if (nearbyDifficultyExercises.Count > 0)
            {
                return nearbyDifficultyExercises[random.Next(nearbyDifficultyExercises.Count)];
            }

            // If still no match, return any random exercise
            return availableExercises[random.Next(availableExercises.Count)];
        }

        private DifficultyLevel GetTargetExerciseDifficulty(DifficultyLevel questionDifficulty)
        {
            // Get schedule information
            bool isScheduleEnabled = scheduleHelper?.IsScheduleEnabled == true;
            double startHour = 0.0;
            double endHour = 23.99;

            if (scheduleHelper != null)
            {
                startHour = scheduleHelper.StartHour;
                endHour = scheduleHelper.EndHour;
            }

            // Use the TimeBasedDifficultyHelper to calculate the difficulty
            return TimeBasedDifficultyHelper.GetExerciseDifficulty(
                exerciseDifficultyMode,
                questionDifficulty,
                startHour,
                endHour,
                isScheduleEnabled);
        }

        private List<Exercise> GetNearbyDifficultyExercises(List<Exercise> exercises, DifficultyLevel targetDifficulty)
        {
            var result = new List<Exercise>();

            // Define difficulty proximity order
            var difficultyOrder = new Dictionary<DifficultyLevel, DifficultyLevel[]>
            {
                { DifficultyLevel.Easy, new[] { DifficultyLevel.Medium, DifficultyLevel.Hard } },
                { DifficultyLevel.Medium, new[] { DifficultyLevel.Easy, DifficultyLevel.Hard } },
                { DifficultyLevel.Hard, new[] { DifficultyLevel.Medium, DifficultyLevel.Easy } }
            };

            if (difficultyOrder.ContainsKey(targetDifficulty))
            {
                foreach (var nearbyDifficulty in difficultyOrder[targetDifficulty])
                {
                    var nearbyExercises = exercises.Where(e => e.Difficulty == nearbyDifficulty).ToList();
                    if (nearbyExercises.Count > 0)
                    {
                        result.AddRange(nearbyExercises);
                        break; // Take the closest difficulty level only
                    }
                }
            }

            return result;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            questionTimer?.Stop();
            base.OnClosing(e);
        }

        // Properties for future bonus feature access
        public bool WasAnsweredQuickly => answeredQuickly;
        public bool WasAnswered => isAnswered;
        public TimeSpan? AnswerTime => isAnswered ? DateTime.Now - questionStartTime : (TimeSpan?)null;

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
    public class ExerciseRequestedEventArgs : EventArgs
    {
        public bool IsCorrect { get; set; }
        public Exercise Exercise { get; set; }
    }
}