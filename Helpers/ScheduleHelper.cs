using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Manages schedule-based timer control (work hours and weekdays)
    /// </summary>
    public class ScheduleHelper
    {
        private DispatcherTimer scheduleCheckTimer;
        private bool isInitialized = false;
        private bool wasWithinSchedulePreviously = true;

        // Schedule settings - UPDATED TO SUPPORT DECIMAL HOURS
        public bool OnlyBetweenHoursEnabled { get; set; } = false;
        public double StartHour { get; set; } = 9.0;   // 9:00 AM (can be 9.5 for 9:30)
        public double EndHour { get; set; } = 17.0;    // 5:00 PM (can be 17.5 for 5:30)
        public bool OnlyWeekdaysEnabled { get; set; } = false;

        // Events
        public event Action ScheduleBecameActive;
        public event Action ScheduleBecameInactive;

        // Properties
        public bool IsWithinSchedule => CheckIfCurrentTimeIsWithinSchedule();
        public bool IsScheduleEnabled => OnlyBetweenHoursEnabled || OnlyWeekdaysEnabled;

        public ScheduleHelper()
        {
            // Check schedule every 10 seconds for reasonably responsive detection
            scheduleCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            scheduleCheckTimer.Tick += ScheduleCheckTimer_Tick;
        }

        public void StartMonitoring()
        {
            if (!IsScheduleEnabled)
            {
                Debug.WriteLine("Schedule monitoring not started - no schedule restrictions enabled");
                return;
            }

            if (scheduleCheckTimer.IsEnabled)
            {
                Debug.WriteLine("Schedule monitoring already running");
                return;
            }

            wasWithinSchedulePreviously = IsWithinSchedule; // Initialize with current state
            scheduleCheckTimer.Start();
            isInitialized = true;

            Debug.WriteLine($"Schedule monitoring started - Hours: {(OnlyBetweenHoursEnabled ? $"{AppSettings.DecimalHourToTimeString(StartHour)}-{AppSettings.DecimalHourToTimeString(EndHour)}" : "Any")}, " +
                          $"Days: {(OnlyWeekdaysEnabled ? "Weekdays only" : "All days")}");
        }

        public void StopMonitoring()
        {
            scheduleCheckTimer?.Stop();
            isInitialized = false;
            Debug.WriteLine("Schedule monitoring stopped");
        }

        public void UpdateSettings(bool onlyBetweenHoursEnabled, double startHour, double endHour, bool onlyWeekdaysEnabled)
        {
            bool wasWithinSchedule = IsWithinSchedule;
            bool wasEnabled = IsScheduleEnabled;

            OnlyBetweenHoursEnabled = onlyBetweenHoursEnabled;
            StartHour = Math.Max(0, Math.Min(23.99, startHour));
            EndHour = Math.Max(0, Math.Min(23.99, endHour));
            OnlyWeekdaysEnabled = onlyWeekdaysEnabled;

            // Ensure start hour is before end hour (with small tolerance for decimals)
            if (Math.Abs(StartHour - EndHour) < 0.01)
            {
                EndHour = (StartHour + 1) % 24;
            }

            bool nowEnabled = IsScheduleEnabled;
            bool nowWithinSchedule = IsWithinSchedule;

            Debug.WriteLine($"Schedule settings updated - Hours: {(OnlyBetweenHoursEnabled ? $"{AppSettings.DecimalHourToTimeString(StartHour)}-{AppSettings.DecimalHourToTimeString(EndHour)}" : "Any")}, " +
                          $"Days: {(OnlyWeekdaysEnabled ? "Weekdays only" : "All days")}");

            // Handle monitoring state changes
            if (!wasEnabled && nowEnabled)
            {
                // Schedule restrictions were just enabled
                StartMonitoring();
                if (!nowWithinSchedule)
                {
                    ScheduleBecameInactive?.Invoke();
                }
            }
            else if (wasEnabled && !nowEnabled)
            {
                // Schedule restrictions were disabled
                StopMonitoring();
                if (!wasWithinSchedule)
                {
                    ScheduleBecameActive?.Invoke();
                }
            }
            else if (nowEnabled && isInitialized)
            {
                // Settings changed while enabled - check for state change
                if (wasWithinSchedule != nowWithinSchedule)
                {
                    if (nowWithinSchedule)
                    {
                        ScheduleBecameActive?.Invoke();
                    }
                    else
                    {
                        ScheduleBecameInactive?.Invoke();
                    }
                }
            }
        }

        private void ScheduleCheckTimer_Tick(object sender, EventArgs e)
        {
            if (!IsScheduleEnabled) return;

            bool isCurrentlyWithinSchedule = CheckIfCurrentTimeIsWithinSchedule();

            if (wasWithinSchedulePreviously != isCurrentlyWithinSchedule)
            {
                if (isCurrentlyWithinSchedule)
                {
                    Debug.WriteLine("🟢 Schedule became ACTIVE");
                    ScheduleBecameActive?.Invoke();
                }
                else
                {
                    Debug.WriteLine("🔴 Schedule became INACTIVE");
                    ScheduleBecameInactive?.Invoke();
                }

                wasWithinSchedulePreviously = isCurrentlyWithinSchedule; // Update the field
            }
        }

        private bool CheckIfCurrentTimeIsWithinSchedule()
        {
            DateTime now = DateTime.Now;

            // Check weekday restriction
            if (OnlyWeekdaysEnabled)
            {
                if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                {
                    return false;
                }
            }

            // Check hour restriction
            if (OnlyBetweenHoursEnabled)
            {
                double currentHour = now.Hour + (now.Minute / 60.0);

                // Handle normal case (e.g., 9.0-17.5)
                if (StartHour < EndHour)
                {
                    if (currentHour < StartHour || currentHour >= EndHour)
                    {
                        return false;
                    }
                }
                // Handle overnight case (e.g., 22.5-6.0)
                else if (StartHour > EndHour)
                {
                    if (currentHour < StartHour && currentHour >= EndHour)
                    {
                        return false;
                    }
                }
                // If StartHour == EndHour, it's effectively disabled
            }

            return true;
        }

        public string GetScheduleStatus()
        {
            if (!IsScheduleEnabled)
            {
                return "No schedule restrictions";
            }

            DateTime now = DateTime.Now;
            string status = "";

            if (OnlyWeekdaysEnabled)
            {
                status += "Weekdays only";
                if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
                {
                    status += " (Currently weekend - INACTIVE)";
                }
            }

            if (OnlyBetweenHoursEnabled)
            {
                if (!string.IsNullOrEmpty(status)) status += ", ";
                status += $"Hours: {AppSettings.DecimalHourToTimeString(StartHour)}-{AppSettings.DecimalHourToTimeString(EndHour)}";

                if (!IsTimeWithinHours(now.Hour + (now.Minute / 60.0)))
                {
                    status += " (Currently outside hours - INACTIVE)";
                }
            }

            return status;
        }

        private bool IsTimeWithinHours(double currentHour)
        {
            if (!OnlyBetweenHoursEnabled) return true;

            if (StartHour < EndHour)
            {
                return currentHour >= StartHour && currentHour < EndHour;
            }
            else if (StartHour > EndHour)
            {
                return currentHour >= StartHour || currentHour < EndHour;
            }

            return true; // If StartHour == EndHour
        }

        public TimeSpan? GetTimeUntilNextActiveSchedule()
        {
            if (!IsScheduleEnabled || IsWithinSchedule)
            {
                return null; // Either no restrictions or currently active
            }

            DateTime now = DateTime.Now;
            DateTime nextActiveTime = CalculateNextActiveTime(now);

            return nextActiveTime - now;
        }

        private DateTime CalculateNextActiveTime(DateTime now)
        {
            DateTime candidate = now.Date; // Start with today at midnight

            // Look ahead up to 7 days to find next valid time
            for (int daysAhead = 0; daysAhead <= 7; daysAhead++)
            {
                DateTime checkDate = candidate.AddDays(daysAhead);

                // Skip weekends if weekday-only is enabled
                if (OnlyWeekdaysEnabled &&
                    (checkDate.DayOfWeek == DayOfWeek.Saturday || checkDate.DayOfWeek == DayOfWeek.Sunday))
                {
                    continue;
                }

                // If we're checking today and it's past the end hour, try tomorrow
                if (daysAhead == 0 && OnlyBetweenHoursEnabled && (now.Hour + now.Minute / 60.0) >= EndHour)
                {
                    continue;
                }

                // Set the start hour for this date
                DateTime candidateTime = OnlyBetweenHoursEnabled ?
                    GetDateTimeFromDecimalHour(checkDate, StartHour) :
                    checkDate; // If no hour restriction, any time is fine

                // If this is today and we're before the start hour, use the start hour
                if (daysAhead == 0 && OnlyBetweenHoursEnabled && (now.Hour + now.Minute / 60.0) < StartHour)
                {
                    return candidateTime;
                }

                // If this is a future day, use the start hour (or beginning of day)
                if (daysAhead > 0)
                {
                    return candidateTime;
                }
            }

            // Fallback - next Monday at start hour
            DateTime nextMonday = now.Date.AddDays(((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7);
            if (nextMonday == now.Date && (int)now.DayOfWeek == (int)DayOfWeek.Monday)
            {
                nextMonday = nextMonday.AddDays(7); // If today is Monday, get next Monday
            }

            return OnlyBetweenHoursEnabled ? GetDateTimeFromDecimalHour(nextMonday, StartHour) : nextMonday;
        }

        /// <summary>
        /// Convert decimal hour to DateTime on a specific date
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="decimalHour">Hour in decimal format (e.g., 9.5 for 9:30)</param>
        /// <returns>DateTime with the specified time</returns>
        private DateTime GetDateTimeFromDecimalHour(DateTime date, double decimalHour)
        {
            int hours = (int)Math.Floor(decimalHour);
            int minutes = (int)Math.Round((decimalHour - hours) * 60);

            // Handle edge case where rounding gives us 60 minutes
            if (minutes >= 60)
            {
                hours++;
                minutes = 0;
            }

            return date.Date.AddHours(hours).AddMinutes(minutes);
        }

        public void Dispose()
        {
            StopMonitoring();
            scheduleCheckTimer = null;
        }
    }
}