using System;
using System.Windows.Threading;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Base class for all application timers with common functionality
    /// </summary>
    public abstract class BaseTimer : IDisposable
    {
        protected DispatcherTimer timer;
        protected TimeSpan interval;
        protected DateTime nextTriggerTime;
        protected bool isDisposed;

        public string Name { get; protected set; }
        public bool IsActive => timer?.IsEnabled == true;
        public bool IsPaused { get; protected set; }
        public DateTime NextTriggerTime => nextTriggerTime;
        public TimeSpan Interval => interval;

        // Events
        public event EventHandler<TimerEventArgs> TimerTriggered;
        public event EventHandler<TimerStateChangedEventArgs> StateChanged;

        protected BaseTimer(string name, TimeSpan interval)
        {
            Name = name;
            this.interval = interval;
            InitializeTimer();
        }

        protected virtual void InitializeTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = interval
            };
            timer.Tick += OnTimerTick;
        }

        protected virtual void OnTimerTick(object sender, EventArgs e)
        {
            if (IsPaused) return;

            // Update next trigger time
            CalculateNextTriggerTime();

            // Fire the event
            var eventArgs = new TimerEventArgs(Name, DateTime.Now);
            TimerTriggered?.Invoke(this, eventArgs);

            // Let derived classes handle their specific logic
            OnTimerTriggered(eventArgs);
        }

        protected abstract void OnTimerTriggered(TimerEventArgs e);

        public virtual void Activate()
        {
            if (IsActive) return;

            CalculateNextTriggerTime();
            timer.Start();
            IsPaused = false;

            OnStateChanged(TimerState.Active);
            System.Diagnostics.Debug.WriteLine($"Timer '{Name}' activated. Next trigger: {nextTriggerTime}");
        }

        public virtual void Deactivate()
        {
            if (!IsActive) return;

            timer.Stop();
            IsPaused = false;

            OnStateChanged(TimerState.Inactive);
            System.Diagnostics.Debug.WriteLine($"Timer '{Name}' deactivated.");
        }

        public virtual void Pause()
        {
            if (!IsActive || IsPaused) return;

            IsPaused = true;
            OnStateChanged(TimerState.Paused);
            System.Diagnostics.Debug.WriteLine($"Timer '{Name}' paused.");
        }

        public virtual void Resume()
        {
            if (!IsActive || !IsPaused) return;

            IsPaused = false;
            OnStateChanged(TimerState.Active);
            System.Diagnostics.Debug.WriteLine($"Timer '{Name}' resumed.");
        }

        public virtual void Reset()
        {
            bool wasActive = IsActive;

            if (wasActive)
            {
                timer.Stop();
            }

            CalculateNextTriggerTime();

            if (wasActive)
            {
                timer.Start();
            }

            OnStateChanged(TimerState.Reset);
            System.Diagnostics.Debug.WriteLine($"Timer '{Name}' reset. Next trigger: {nextTriggerTime}");
        }

        public virtual void UpdateInterval(TimeSpan newInterval)
        {
            if (newInterval <= TimeSpan.Zero)
                throw new ArgumentException("Interval must be positive", nameof(newInterval));

            bool wasActive = IsActive;

            if (wasActive)
            {
                Deactivate();
            }

            interval = newInterval;
            timer.Interval = newInterval;

            if (wasActive)
            {
                Activate();
            }

            System.Diagnostics.Debug.WriteLine($"Timer '{Name}' interval updated to {newInterval}");
        }

        protected virtual void CalculateNextTriggerTime()
        {
            nextTriggerTime = DateTime.Now.Add(interval);
        }

        protected virtual void OnStateChanged(TimerState newState)
        {
            StateChanged?.Invoke(this, new TimerStateChangedEventArgs(Name, newState, nextTriggerTime));
        }

        public virtual void Dispose()
        {
            if (!isDisposed)
            {
                Deactivate();
                timer = null;
                isDisposed = true;
            }
        }
    }

    /// <summary>
    /// Timer for regular question intervals
    /// </summary>
    public class QuestionTimer : BaseTimer
    {
        public QuestionTimer(TimeSpan interval) : base("Question Timer", interval) { }

        protected override void OnTimerTriggered(TimerEventArgs e)
        {
            // Question-specific logic handled by event subscribers
        }
    }

    /// <summary>
    /// Timer for drink reminders
    /// </summary>
    public class DrinkReminderTimer : BaseTimer
    {
        public DrinkReminderTimer(TimeSpan interval) : base("Drink Reminder", interval) { }

        protected override void OnTimerTriggered(TimerEventArgs e)
        {
            // Drink reminder logic handled by event subscribers
        }
    }

    /// <summary>
    /// One-shot timer for pre-question alerts (not repeating)
    /// </summary>
    public class PreQuestionAlertTimer : BaseTimer
    {
        private TimeSpan alertOffset;
        private TimeSpan questionInterval;

        public PreQuestionAlertTimer(TimeSpan questionInterval, TimeSpan alertOffset)
            : base("Pre-Question Alert", questionInterval - alertOffset)
        {
            this.alertOffset = alertOffset;
            this.questionInterval = questionInterval;
        }

        protected override void OnTimerTriggered(TimerEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Pre-question alert timer TRIGGERED at {DateTime.Now:HH:mm:ss.fff}");

            // Pre-question alert is one-shot, so deactivate after triggering
            Deactivate();
        }

        public void UpdateForNewQuestionInterval(TimeSpan newQuestionInterval)
        {
            this.questionInterval = newQuestionInterval;
            var newInterval = questionInterval - alertOffset;

            if (newInterval > TimeSpan.Zero)
            {
                bool wasActive = IsActive;
                if (wasActive)
                {
                    Deactivate();
                }

                UpdateInterval(newInterval);

                if (wasActive)
                {
                    Activate();
                }

                System.Diagnostics.Debug.WriteLine($"Pre-question alert updated for new question interval ({newQuestionInterval}). Alert will fire {alertOffset} before each question.");
            }
            else
            {
                // Alert time is longer than question interval, so disable
                Deactivate();
                System.Diagnostics.Debug.WriteLine($"Pre-question alert disabled - alert time ({alertOffset}) >= question interval ({newQuestionInterval})");
            }
        }

        public void UpdateAlertOffset(TimeSpan newAlertOffset)
        {
            this.alertOffset = newAlertOffset;
            var newInterval = questionInterval - alertOffset;

            if (newInterval > TimeSpan.Zero)
            {
                bool wasActive = IsActive;
                if (wasActive)
                {
                    Deactivate();
                }

                UpdateInterval(newInterval);

                if (wasActive)
                {
                    Activate();
                }

                System.Diagnostics.Debug.WriteLine($"Pre-question alert offset updated to {newAlertOffset}. New interval: {newInterval}");
            }
            else
            {
                Deactivate();
                System.Diagnostics.Debug.WriteLine($"Pre-question alert disabled - alert time ({newAlertOffset}) >= question interval ({questionInterval})");
            }
        }

        /// <summary>
        /// Update the alert timer based on when the next question is due
        /// </summary>
        /// <param name="timeUntilNextQuestion">How much time is left until the next question</param>
        /// <summary>
        /// Update the alert timer based on when the next question is due
        /// </summary>
        /// <param name="timeUntilNextQuestion">How much time is left until the next question</param>
        public void UpdateForRemainingQuestionTime(TimeSpan timeUntilNextQuestion)
        {
            var timeUntilAlert = timeUntilNextQuestion - alertOffset;

            System.Diagnostics.Debug.WriteLine($"UpdateForRemainingQuestionTime: timeUntilNextQuestion={timeUntilNextQuestion.TotalMinutes:F2}min, alertOffset={alertOffset.TotalMinutes:F2}min, timeUntilAlert={timeUntilAlert.TotalMinutes:F2}min");

            if (timeUntilAlert > TimeSpan.Zero)
            {
                bool wasActive = IsActive;

                // Stop current timer if running
                if (wasActive)
                {
                    timer.Stop();
                }

                // Update the interval property and timer with the calculated time
                interval = timeUntilAlert;
                timer.Interval = timeUntilAlert;
                nextTriggerTime = DateTime.Now.Add(timeUntilAlert);

                // Restart if it was active
                if (wasActive)
                {
                    timer.Start();
                    IsPaused = false;
                    OnStateChanged(TimerState.Active);
                    System.Diagnostics.Debug.WriteLine($"Pre-question alert restarted to fire in {timeUntilAlert.TotalSeconds:F1} seconds ({timeUntilAlert.TotalMinutes:F2} minutes)");
                }

                System.Diagnostics.Debug.WriteLine($"Pre-question alert configured: will fire at {nextTriggerTime:HH:mm:ss.fff} (in {timeUntilAlert.TotalSeconds:F1}s)");
            }
            else
            {
                // Not enough time left for an alert
                if (IsActive)
                {
                    Deactivate();
                }
                System.Diagnostics.Debug.WriteLine($"Pre-question alert skipped - not enough time remaining ({timeUntilNextQuestion.TotalMinutes:F1}min) for alert offset ({alertOffset.TotalMinutes:F1}min)");
            }
        }

        public bool IsValidConfiguration()
        {
            return alertOffset < questionInterval && alertOffset > TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Timer manager to coordinate all timers
    /// </summary>
    public class TimerManager : IDisposable
    {
        private QuestionTimer questionTimer;
        private DrinkReminderTimer drinkTimer;
        private PreQuestionAlertTimer preQuestionTimer;

        public QuestionTimer QuestionTimer => questionTimer;
        public DrinkReminderTimer DrinkTimer => drinkTimer;
        public PreQuestionAlertTimer PreQuestionTimer => preQuestionTimer;

        public bool AnyTimerActive =>
            (questionTimer?.IsActive == true) ||
            (drinkTimer?.IsActive == true) ||
            (preQuestionTimer?.IsActive == true);

        public event EventHandler<TimerEventArgs> QuestionTimerTriggered;
        public event EventHandler<TimerEventArgs> DrinkReminderTriggered;
        public event EventHandler<TimerEventArgs> PreQuestionAlertTriggered;

        public void InitializeQuestionTimer(TimeSpan interval)
        {
            questionTimer?.Dispose();
            questionTimer = new QuestionTimer(interval);
            questionTimer.TimerTriggered += (s, e) => QuestionTimerTriggered?.Invoke(s, e);
        }

        public void InitializeDrinkTimer(TimeSpan interval)
        {
            drinkTimer?.Dispose();
            drinkTimer = new DrinkReminderTimer(interval);
            drinkTimer.TimerTriggered += (s, e) => DrinkReminderTriggered?.Invoke(s, e);
        }

        public void InitializePreQuestionTimer(TimeSpan questionInterval, TimeSpan alertOffset)
        {
            preQuestionTimer?.Dispose();
            preQuestionTimer = new PreQuestionAlertTimer(questionInterval, alertOffset);
            preQuestionTimer.TimerTriggered += (s, e) => PreQuestionAlertTriggered?.Invoke(s, e);
        }

        public void StartAll()
        {
            questionTimer?.Activate();
            drinkTimer?.Activate();
            preQuestionTimer?.Activate();
        }

        public void StopAll()
        {
            questionTimer?.Deactivate();
            drinkTimer?.Deactivate();
            preQuestionTimer?.Deactivate();
        }

        public void PauseAll()
        {
            questionTimer?.Pause();
            drinkTimer?.Pause();
            preQuestionTimer?.Pause();
        }

        public void ResumeAll()
        {
            questionTimer?.Resume();
            drinkTimer?.Resume();
            preQuestionTimer?.Resume();
        }

        public void ResetAll()
        {
            questionTimer?.Reset();
            drinkTimer?.Reset();
            preQuestionTimer?.Reset();
        }

        public string GetStatusSummary()
        {
            var status = new System.Text.StringBuilder();

            if (questionTimer != null)
            {
                status.AppendLine($"Question Timer: {GetTimerStatus(questionTimer)}");
            }

            if (drinkTimer != null)
            {
                status.AppendLine($"Drink Timer: {GetTimerStatus(drinkTimer)}");
            }

            if (preQuestionTimer != null)
            {
                status.AppendLine($"Pre-Question Alert: {GetTimerStatus(preQuestionTimer)}");
            }

            return status.ToString();
        }

        private string GetTimerStatus(BaseTimer timer)
        {
            if (timer.IsPaused)
                return "Paused";
            if (timer.IsActive)
                return $"Active (next: {timer.NextTriggerTime:HH:mm:ss})";
            return "Inactive";
        }

        public void Dispose()
        {
            questionTimer?.Dispose();
            drinkTimer?.Dispose();
            preQuestionTimer?.Dispose();
        }
    }

    // Event arguments and enums
    public class TimerEventArgs : EventArgs
    {
        public string TimerName { get; }
        public DateTime TriggerTime { get; }

        public TimerEventArgs(string timerName, DateTime triggerTime)
        {
            TimerName = timerName;
            TriggerTime = triggerTime;
        }
    }

    public class TimerStateChangedEventArgs : EventArgs
    {
        public string TimerName { get; }
        public TimerState NewState { get; }
        public DateTime NextTriggerTime { get; }

        public TimerStateChangedEventArgs(string timerName, TimerState newState, DateTime nextTriggerTime)
        {
            TimerName = timerName;
            NewState = newState;
            NextTriggerTime = nextTriggerTime;
        }
    }

    public enum TimerState
    {
        Inactive,
        Active,
        Paused,
        Reset
    }
}