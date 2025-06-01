using System;
using System.IO;
using Newtonsoft.Json;
using TriviaExercise.Models;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Manages application settings persistence
    /// </summary>
    public class AppSettings
    {
        private static readonly string SETTINGS_FILE_NAME = "app_settings.json";
        private static readonly string SETTINGS_FILE_PATH;

        static AppSettings()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(localAppData, "TriviaExercise");
            Directory.CreateDirectory(appFolder);
            SETTINGS_FILE_PATH = Path.Combine(appFolder, SETTINGS_FILE_NAME);
        }

        /// <summary>
        /// Application settings data structure
        /// </summary>
        public class Settings
        {
            public int QuestionIntervalMinutes { get; set; } = 50;
            public int DrinkIntervalMinutes { get; set; } = 90;
            public bool DrinkReminderEnabled { get; set; } = false;
            public bool StartMinimized { get; set; } = false;
            public ExerciseDifficultyMode ExerciseDifficulty { get; set; } = ExerciseDifficultyMode.MatchQuestion;
            public bool DiscordRichPresenceEnabled { get; set; } = false;
            public string DataFolderPath { get; set; } = string.Empty; // Empty means use default
            public DateTime LastSaved { get; set; } = DateTime.Now;

            // Sound settings
            public bool SoundsEnabled { get; set; } = true;
            public double PreQuestionAlertMinutes { get; set; } = 1; // 1 minute default

            // Activity monitoring settings
            public ActivityBehavior ActivityMonitoringBehavior { get; set; } = ActivityBehavior.PauseOnly;
            public uint InactivityThresholdMinutes { get; set; } = 5; // Reset timer after 5 minutes of inactivity

            // Schedule settings - UPDATED TO SUPPORT DECIMAL HOURS
            public bool OnlyBetweenHoursEnabled { get; set; } = true;
            public double ScheduleStartHour { get; set; } = 9.0;  // 9:00 AM (can be 9.5 for 9:30)
            public double ScheduleEndHour { get; set; } = 17.0;   // 5:00 PM (can be 17.5 for 5:30)
            public bool OnlyWeekdaysEnabled { get; set; } = true;
        }

        /// <summary>
        /// Load settings from disk
        /// </summary>
        /// <returns>Loaded settings or default settings if file doesn't exist</returns>
        public static Settings LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE_PATH))
                {
                    string json = File.ReadAllText(SETTINGS_FILE_PATH);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);

                    // Validate loaded settings
                    if (settings != null)
                    {
                        ValidateSettings(settings);
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default settings if file doesn't exist or error occurred
            return new Settings();
        }

        /// <summary>
        /// Save settings to disk
        /// </summary>
        /// <param name="settings">Settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SaveSettings(Settings settings)
        {
            try
            {
                ValidateSettings(settings);
                settings.LastSaved = DateTime.Now;

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SETTINGS_FILE_PATH, json);

                System.Diagnostics.Debug.WriteLine($"Settings saved to: {SETTINGS_FILE_PATH}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate and correct settings values
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        private static void ValidateSettings(Settings settings)
        {
            // Ensure intervals are within reasonable bounds
            if (settings.QuestionIntervalMinutes < 1)
                settings.QuestionIntervalMinutes = 1;
            else if (settings.QuestionIntervalMinutes > 1440) // Max 24 hours
                settings.QuestionIntervalMinutes = 1440;

            if (settings.DrinkIntervalMinutes < 1)
                settings.DrinkIntervalMinutes = 1;
            else if (settings.DrinkIntervalMinutes > 1440) // Max 24 hours
                settings.DrinkIntervalMinutes = 1440;

            // Validate sound settings
            if (settings.PreQuestionAlertMinutes < 0)
                settings.PreQuestionAlertMinutes = 0;
            else if (settings.PreQuestionAlertMinutes > 60) // Max 1 hour
                settings.PreQuestionAlertMinutes = 60;

            // Validate activity monitoring settings
            if (settings.InactivityThresholdMinutes < 1)
                settings.InactivityThresholdMinutes = 1;
            else if (settings.InactivityThresholdMinutes > 1440) // Max 24 hours
                settings.InactivityThresholdMinutes = 1440;

            // Validate schedule settings - UPDATED FOR DECIMAL HOURS
            if (settings.ScheduleStartHour < 0)
                settings.ScheduleStartHour = 0;
            else if (settings.ScheduleStartHour > 23.99) // Max 23:59 (23.98333...)
                settings.ScheduleStartHour = 23.99;

            if (settings.ScheduleEndHour < 0)
                settings.ScheduleEndHour = 0;
            else if (settings.ScheduleEndHour > 23.99) // Max 23:59
                settings.ScheduleEndHour = 23.99;

            // Ensure start hour is different from end hour (with small tolerance for decimals)
            if (Math.Abs(settings.ScheduleStartHour - settings.ScheduleEndHour) < 0.01)
            {
                settings.ScheduleEndHour = (settings.ScheduleStartHour + 1) % 24;
            }

            // Round to minute precision (avoid excessive decimal places)
            // This preserves user input like 13.4 while limiting precision to practical levels
            settings.ScheduleStartHour = Math.Round(settings.ScheduleStartHour, 2);
            settings.ScheduleEndHour = Math.Round(settings.ScheduleEndHour, 2);

            // Validate data folder path if set
            if (!string.IsNullOrEmpty(settings.DataFolderPath))
            {
                try
                {
                    // Check if path is valid and accessible
                    if (!Directory.Exists(settings.DataFolderPath))
                    {
                        // Try to create the directory if it's a Data subfolder
                        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string dataDirectory = Path.Combine(baseDirectory, "Data");

                        if (settings.DataFolderPath.Equals(dataDirectory, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                Directory.CreateDirectory(dataDirectory);
                                // Successfully created, keep the setting
                            }
                            catch
                            {
                                // Reset to default if Data folder can't be created
                                settings.DataFolderPath = string.Empty;
                            }
                        }
                        else
                        {
                            // Reset to default if custom folder doesn't exist
                            settings.DataFolderPath = string.Empty;
                        }
                    }
                }
                catch
                {
                    // Reset to default if path is invalid
                    settings.DataFolderPath = string.Empty;
                }
            }
        }

        /// <summary>
        /// Convert decimal hour to time string (e.g., 9.5 -> "9:30", 17.25 -> "17:15", 13.4 -> "13:24")
        /// </summary>
        /// <param name="decimalHour">Hour in decimal format</param>
        /// <returns>Time string in HH:MM format</returns>
        public static string DecimalHourToTimeString(double decimalHour)
        {
            int hours = (int)Math.Floor(decimalHour);
            double fractionalHour = decimalHour - hours;
            int minutes = (int)Math.Round(fractionalHour * 60);

            // Handle edge case where rounding gives us 60 minutes
            if (minutes >= 60)
            {
                hours++;
                minutes = 0;
            }

            // Ensure hours don't exceed 23
            if (hours >= 24)
            {
                hours = 23;
                minutes = 59;
            }

            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// Convert time string to decimal hour (e.g., "9:30" -> 9.5, "17:15" -> 17.25)
        /// </summary>
        /// <param name="timeString">Time string in HH:MM format or decimal format</param>
        /// <returns>Hour in decimal format</returns>
        public static double TimeStringToDecimalHour(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return 0;

            // Normalize decimal separator - replace comma with period for consistent parsing
            string normalizedString = timeString.Replace(',', '.');

            // Try to parse as decimal with invariant culture (always uses period as decimal separator)
            if (double.TryParse(normalizedString, System.Globalization.NumberStyles.Float,
                              System.Globalization.CultureInfo.InvariantCulture, out double directDecimal))
            {
                return Math.Max(0, Math.Min(23.99, directDecimal));
            }

            // Try to parse as HH:MM format
            if (timeString.Contains(":"))
            {
                var parts = timeString.Split(':');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int hours) &&
                    int.TryParse(parts[1], out int minutes))
                {
                    double result = hours + (minutes / 60.0);
                    return Math.Max(0, Math.Min(23.99, result));
                }
            }

            // Try one more time with current culture as fallback
            if (double.TryParse(timeString, out double culturalDecimal))
            {
                return Math.Max(0, Math.Min(23.99, culturalDecimal));
            }

            return 0; // Fallback for invalid input
        }

        /// <summary>
        /// Get the full path to the settings file (for debugging)
        /// </summary>
        /// <returns>Full path to settings file</returns>
        public static string GetSettingsFilePath()
        {
            return SETTINGS_FILE_PATH;
        }

        /// <summary>
        /// Check if settings file exists
        /// </summary>
        /// <returns>True if settings file exists</returns>
        public static bool SettingsFileExists()
        {
            return File.Exists(SETTINGS_FILE_PATH);
        }

        /// <summary>
        /// Delete the settings file (for reset functionality)
        /// </summary>
        /// <returns>True if successful or file didn't exist</returns>
        public static bool DeleteSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE_PATH))
                {
                    File.Delete(SETTINGS_FILE_PATH);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get settings summary for display
        /// </summary>
        /// <param name="settings">Settings to summarize</param>
        /// <returns>Human-readable settings summary</returns>
        public static string GetSettingsSummary(Settings settings)
        {
            string scheduleInfo = "";
            if (settings.OnlyBetweenHoursEnabled || settings.OnlyWeekdaysEnabled)
            {
                scheduleInfo = "Schedule: ";
                if (settings.OnlyWeekdaysEnabled)
                    scheduleInfo += "Weekdays";
                if (settings.OnlyBetweenHoursEnabled)
                {
                    if (scheduleInfo != "Schedule: ") scheduleInfo += ", ";
                    scheduleInfo += $"{DecimalHourToTimeString(settings.ScheduleStartHour)}-{DecimalHourToTimeString(settings.ScheduleEndHour)}";
                }
                scheduleInfo += " | ";
            }

            return $"Question Interval: {settings.QuestionIntervalMinutes}min | " +
                   $"Drink Reminder: {(settings.DrinkReminderEnabled ? $"{settings.DrinkIntervalMinutes}min" : "Off")} | " +
                   $"Exercise: {settings.ExerciseDifficulty} | " +
                   $"Discord: {(settings.DiscordRichPresenceEnabled ? "On" : "Off")} | " +
                   $"Sounds: {(settings.SoundsEnabled ? "On" : "Off")} | " +
                   $"Activity Monitor: {settings.ActivityMonitoringBehavior} | " +
                   scheduleInfo +
                   $"Start Minimized: {(settings.StartMinimized ? "Yes" : "No")}";
        }
    }
}