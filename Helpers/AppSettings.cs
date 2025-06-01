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

            // Schedule settings - NEW PROPERTIES
            public bool OnlyBetweenHoursEnabled { get; set; } = true;
            public int ScheduleStartHour { get; set; } = 9;  // 9 AM
            public int ScheduleEndHour { get; set; } = 17;   // 5 PM
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

            // Validate schedule settings
            if (settings.ScheduleStartHour < 0)
                settings.ScheduleStartHour = 0;
            else if (settings.ScheduleStartHour > 23)
                settings.ScheduleStartHour = 23;

            if (settings.ScheduleEndHour < 0)
                settings.ScheduleEndHour = 0;
            else if (settings.ScheduleEndHour > 23)
                settings.ScheduleEndHour = 23;

            // Ensure start hour is different from end hour
            if (settings.ScheduleStartHour == settings.ScheduleEndHour)
            {
                settings.ScheduleEndHour = (settings.ScheduleStartHour + 1) % 24;
            }

            // Validate data folder path if set
            if (!string.IsNullOrEmpty(settings.DataFolderPath))
            {
                try
                {
                    // Check if path is valid and accessible
                    if (!Directory.Exists(settings.DataFolderPath))
                    {
                        // Reset to default if folder doesn't exist
                        settings.DataFolderPath = string.Empty;
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
                    scheduleInfo += $"{settings.ScheduleStartHour:D2}:00-{settings.ScheduleEndHour:D2}:00";
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