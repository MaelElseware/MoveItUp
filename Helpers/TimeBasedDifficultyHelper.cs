using System;
using TriviaExercise.Models;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Helper class to calculate exercise difficulty based on time of day
    /// </summary>
    public static class TimeBasedDifficultyHelper
    {
        /// <summary>
        /// Calculate the exercise difficulty based on the current time and schedule settings
        /// </summary>
        /// <param name="mode">The exercise difficulty mode</param>
        /// <param name="questionDifficulty">The difficulty of the current question (for MatchQuestion mode)</param>
        /// <param name="scheduleStartHour">Start hour of the schedule (0-23.99)</param>
        /// <param name="scheduleEndHour">End hour of the schedule (0-23.99)</param>
        /// <param name="isScheduleEnabled">Whether schedule restrictions are enabled</param>
        /// <returns>The calculated difficulty level</returns>
        public static DifficultyLevel GetExerciseDifficulty(
            ExerciseDifficultyMode mode,
            DifficultyLevel questionDifficulty,
            double scheduleStartHour = 0.0,
            double scheduleEndHour = 23.99,
            bool isScheduleEnabled = false)
        {
            switch (mode)
            {
                case ExerciseDifficultyMode.Easy:
                    return DifficultyLevel.Easy;
                case ExerciseDifficultyMode.Medium:
                    return DifficultyLevel.Medium;
                case ExerciseDifficultyMode.Hard:
                    return DifficultyLevel.Hard;
                case ExerciseDifficultyMode.Mixed:
                    return GetRandomDifficulty();
                case ExerciseDifficultyMode.MatchQuestion:
                    return questionDifficulty;
                case ExerciseDifficultyMode.Increasing:
                    return GetTimeBasedDifficulty(true, scheduleStartHour, scheduleEndHour, isScheduleEnabled);
                case ExerciseDifficultyMode.Decreasing:
                    return GetTimeBasedDifficulty(false, scheduleStartHour, scheduleEndHour, isScheduleEnabled);
                default:
                    return questionDifficulty;
            }
        }

        /// <summary>
        /// Get a random difficulty level for Mixed mode
        /// </summary>
        private static DifficultyLevel GetRandomDifficulty()
        {
            var random = new Random();
            var difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard };
            return difficulties[random.Next(difficulties.Length)];
        }

        /// <summary>
        /// Calculate difficulty based on time progression through the day
        /// </summary>
        /// <param name="isIncreasing">True for increasing difficulty, false for decreasing</param>
        /// <param name="scheduleStartHour">Start hour of the active period</param>
        /// <param name="scheduleEndHour">End hour of the active period</param>
        /// <param name="isScheduleEnabled">Whether to use schedule hours or full day</param>
        /// <returns>The calculated difficulty based on time position</returns>
        private static DifficultyLevel GetTimeBasedDifficulty(
            bool isIncreasing,
            double scheduleStartHour,
            double scheduleEndHour,
            bool isScheduleEnabled)
        {
            DateTime now = DateTime.Now;
            double currentHour = now.Hour + (now.Minute / 60.0);

            if (isScheduleEnabled)
            {
                // Use scheduled hours - divide into equal thirds
                double totalActiveHours;
                double currentPosition;

                if (scheduleStartHour < scheduleEndHour)
                {
                    // Normal schedule (e.g., 9:00 to 17:00)
                    totalActiveHours = scheduleEndHour - scheduleStartHour;

                    // Clamp current hour to the active range
                    if (currentHour < scheduleStartHour)
                    {
                        currentPosition = 0.0; // Before schedule starts
                    }
                    else if (currentHour > scheduleEndHour)
                    {
                        currentPosition = 1.0; // After schedule ends
                    }
                    else
                    {
                        currentPosition = (currentHour - scheduleStartHour) / totalActiveHours;
                    }
                }
                else
                {
                    // Overnight schedule (e.g., 22:00 to 06:00)
                    totalActiveHours = (24.0 - scheduleStartHour) + scheduleEndHour;

                    if (currentHour >= scheduleStartHour)
                    {
                        // Evening portion (e.g., 22:00 to 23:59)
                        currentPosition = (currentHour - scheduleStartHour) / totalActiveHours;
                    }
                    else if (currentHour <= scheduleEndHour)
                    {
                        // Morning portion (e.g., 00:00 to 06:00)
                        currentPosition = ((24.0 - scheduleStartHour) + currentHour) / totalActiveHours;
                    }
                    else
                    {
                        // Outside the overnight schedule
                        currentPosition = 0.5; // Default to middle difficulty
                    }
                }

                // Ensure position is between 0 and 1
                currentPosition = Math.Max(0.0, Math.Min(1.0, currentPosition));

                // If decreasing, invert the position
                if (!isIncreasing)
                {
                    currentPosition = 1.0 - currentPosition;
                }

                // Map position to difficulty levels (equal thirds)
                // 0.0 - 0.33: Easy
                // 0.33 - 0.66: Medium  
                // 0.66 - 1.0: Hard
                if (currentPosition < 0.33)
                {
                    return DifficultyLevel.Easy;
                }
                else if (currentPosition < 0.66)
                {
                    return DifficultyLevel.Medium;
                }
                else
                {
                    return DifficultyLevel.Hard;
                }
            }
            else
            {
                // Schedule disabled - use fixed time ranges for full day
                // 0:00-11:00: First difficulty level
                // 11:00-15:00: Second difficulty level  
                // 15:00-23:59: Third difficulty level

                DifficultyLevel baseDifficulty;

                if (currentHour < 11.0)
                {
                    baseDifficulty = DifficultyLevel.Easy;
                }
                else if (currentHour < 15.0)
                {
                    baseDifficulty = DifficultyLevel.Medium;
                }
                else
                {
                    baseDifficulty = DifficultyLevel.Hard;
                }

                // If decreasing mode, invert the difficulty
                if (!isIncreasing)
                {
                    switch (baseDifficulty)
                    {
                        case DifficultyLevel.Easy:
                            return DifficultyLevel.Hard;
                        case DifficultyLevel.Medium:
                            return DifficultyLevel.Medium; // Middle stays the same
                        case DifficultyLevel.Hard:
                            return DifficultyLevel.Easy;
                        default:
                            return baseDifficulty;
                    }
                }
                else
                {
                    return baseDifficulty;
                }
            }
        }

        /// <summary>
        /// Get a human-readable description of the current time-based difficulty
        /// </summary>
        /// <param name="mode">The exercise difficulty mode</param>
        /// <param name="scheduleStartHour">Start hour of the schedule</param>
        /// <param name="scheduleEndHour">End hour of the schedule</param>
        /// <param name="isScheduleEnabled">Whether schedule restrictions are enabled</param>
        /// <returns>A string describing the current difficulty and time position</returns>
        public static string GetTimeBasedDifficultyDescription(
            ExerciseDifficultyMode mode,
            double scheduleStartHour = 0.0,
            double scheduleEndHour = 23.99,
            bool isScheduleEnabled = false)
        {
            if (mode != ExerciseDifficultyMode.Increasing && mode != ExerciseDifficultyMode.Decreasing)
            {
                return string.Empty;
            }

            DateTime now = DateTime.Now;
            double currentHour = now.Hour + (now.Minute / 60.0);

            var difficulty = GetTimeBasedDifficulty(
                mode == ExerciseDifficultyMode.Increasing,
                scheduleStartHour,
                scheduleEndHour,
                isScheduleEnabled);

            string timeRange = isScheduleEnabled ?
                $"{AppSettings.DecimalHourToTimeString(scheduleStartHour)}-{AppSettings.DecimalHourToTimeString(scheduleEndHour)}" :
                "00:00-23:59";

            string currentTime = AppSettings.DecimalHourToTimeString(currentHour);
            string direction = mode == ExerciseDifficultyMode.Increasing ? "increasing" : "decreasing";

            return $"Currently {difficulty} ({direction} throughout {timeRange}, now {currentTime})";
        }
    }
}