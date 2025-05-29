using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TriviaExercise.Models;

namespace TriviaExercise.Helpers
{
    public class PlayerProgressSystem
    {
        private static readonly string PROGRESS_FILE_NAME = "player_progress.json";
        private static readonly string PROGRESS_FILE_PATH;

        static PlayerProgressSystem()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(localAppData, "TriviaExercise");
            Directory.CreateDirectory(appFolder);
            PROGRESS_FILE_PATH = Path.Combine(appFolder, PROGRESS_FILE_NAME);
        }

        /// <summary>
        /// Load player progress from disk
        /// </summary>
        public static PlayerProgress LoadProgress()
        {
            try
            {
                if (File.Exists(PROGRESS_FILE_PATH))
                {
                    string json = File.ReadAllText(PROGRESS_FILE_PATH);
                    var progress = JsonConvert.DeserializeObject<PlayerProgress>(json);

                    // Ensure all categories are initialized
                    EnsureAllCategoriesInitialized(progress);
                    return progress;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading progress: {ex.Message}");
            }

            // Return new progress if file doesn't exist or error occurred
            return CreateNewProgress();
        }

        /// <summary>
        /// Save player progress to disk
        /// </summary>
        public static bool SaveProgress(PlayerProgress progress)
        {
            try
            {
                string json = JsonConvert.SerializeObject(progress, Formatting.Indented);
                File.WriteAllText(PROGRESS_FILE_PATH, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving progress: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a new player progress with all categories initialized
        /// </summary>
        private static PlayerProgress CreateNewProgress()
        {
            var progress = new PlayerProgress
            {
                CategoryProgress = new Dictionary<QuestionCategory, CategoryProgress>()
            };

            EnsureAllCategoriesInitialized(progress);
            return progress;
        }

        /// <summary>
        /// Ensures all question categories have progress entries
        /// </summary>
        private static void EnsureAllCategoriesInitialized(PlayerProgress progress)
        {
            if (progress.CategoryProgress == null)
                progress.CategoryProgress = new Dictionary<QuestionCategory, CategoryProgress>();

            foreach (QuestionCategory category in Enum.GetValues(typeof(QuestionCategory)))
            {
                if (!progress.CategoryProgress.ContainsKey(category))
                {
                    progress.CategoryProgress[category] = new CategoryProgress
                    {
                        Score = 0,
                        CurrentDifficulty = DifficultyLevel.Easy
                    };
                }
            }
        }

        /// <summary>
        /// Process a question answer and update progress
        /// </summary>
        public static ProgressUpdateResult ProcessAnswer(PlayerProgress progress, QuestionCategory category,
            DifficultyLevel questionDifficulty, bool isCorrect, bool wasQuick)
        {
            if (!progress.CategoryProgress.ContainsKey(category))
            {
                progress.CategoryProgress[category] = new CategoryProgress
                {
                    Score = 0,
                    CurrentDifficulty = DifficultyLevel.Easy
                };
            }

            var categoryProgress = progress.CategoryProgress[category];
            var result = new ProgressUpdateResult();

            // Check if enough time has passed since last progress update (30 minutes minimum)
            var timeSinceLastUpdate = DateTime.Now - progress.LastProgressUpdateTime;
            bool canUpdateProgress = timeSinceLastUpdate.TotalMinutes >= 30 || progress.LastProgressUpdateTime == DateTime.MinValue;

            // Always update question statistics
            categoryProgress.QuestionsAnswered++;

            if (isCorrect)
            {
                categoryProgress.CorrectAnswers++;

                if (canUpdateProgress)
                {
                    // Calculate points based on difficulty
                    int points = GetPointsForDifficulty(questionDifficulty);
                    if (wasQuick) points *= 2; // Double points for quick answers

                    categoryProgress.Score += points;
                    result.PointsAwarded = points;

                    // Advance difficulty if possible
                    var oldDifficulty = categoryProgress.CurrentDifficulty;
                    if (categoryProgress.CurrentDifficulty == DifficultyLevel.Easy)
                    {
                        categoryProgress.CurrentDifficulty = DifficultyLevel.Medium;
                    }
                    else if (categoryProgress.CurrentDifficulty == DifficultyLevel.Medium)
                    {
                        categoryProgress.CurrentDifficulty = DifficultyLevel.Hard;
                    }
                    // Hard stays at Hard

                    result.DifficultyChanged = oldDifficulty != categoryProgress.CurrentDifficulty;
                    result.NewDifficulty = categoryProgress.CurrentDifficulty;
                    result.ProgressUpdated = true;

                    // Update the global progress time
                    progress.LastProgressUpdateTime = DateTime.Now;
                }
                else
                {
                    result.ProgressUpdated = false;
                    result.TimeUntilNextUpdate = TimeSpan.FromMinutes(30) - timeSinceLastUpdate;
                }
            }
            else
            {
                if (canUpdateProgress)
                {
                    // Wrong answer - deduct 1 point (but don't go below 0)
                    int pointsDeducted = Math.Min(1, categoryProgress.Score);
                    categoryProgress.Score = Math.Max(0, categoryProgress.Score - 1);
                    result.PointsAwarded = -pointsDeducted; // Negative to indicate points lost

                    // Decrease difficulty if possible
                    var oldDifficulty = categoryProgress.CurrentDifficulty;
                    if (categoryProgress.CurrentDifficulty == DifficultyLevel.Hard)
                    {
                        categoryProgress.CurrentDifficulty = DifficultyLevel.Medium;
                    }
                    else if (categoryProgress.CurrentDifficulty == DifficultyLevel.Medium)
                    {
                        categoryProgress.CurrentDifficulty = DifficultyLevel.Easy;
                    }
                    // Easy stays at Easy

                    result.DifficultyChanged = oldDifficulty != categoryProgress.CurrentDifficulty;
                    result.NewDifficulty = categoryProgress.CurrentDifficulty;
                    result.ProgressUpdated = true;

                    // Update the global progress time
                    progress.LastProgressUpdateTime = DateTime.Now;
                }
                else
                {
                    result.ProgressUpdated = false;
                    result.TimeUntilNextUpdate = TimeSpan.FromMinutes(30) - timeSinceLastUpdate;
                }
            }

            result.IsCorrect = isCorrect;
            result.WasQuick = wasQuick;
            result.Category = category;

            // Save progress immediately
            SaveProgress(progress);

            return result;
        }

        /// <summary>
        /// Get base points for a difficulty level
        /// </summary>
        private static int GetPointsForDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return 1;
                case DifficultyLevel.Medium:
                    return 2;
                case DifficultyLevel.Hard:
                    return 4;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Get current difficulty for a specific category
        /// </summary>
        public static DifficultyLevel GetCategoryDifficulty(PlayerProgress progress, QuestionCategory category)
        {
            if (progress.CategoryProgress.ContainsKey(category))
            {
                return progress.CategoryProgress[category].CurrentDifficulty;
            }
            return DifficultyLevel.Easy; // Default for new categories
        }

        /// <summary>
        /// Get total score across all categories
        /// </summary>
        public static int GetTotalScore(PlayerProgress progress)
        {
            return progress.CategoryProgress.Values.Sum(cp => cp.Score);
        }

        /// <summary>
        /// Get current player level based on total score
        /// </summary>
        public static int GetPlayerLevel(PlayerProgress progress)
        {
            int totalScore = GetTotalScore(progress);

            if (totalScore <= 20) return 1;
            if (totalScore <= 40) return 2;
            if (totalScore <= 100) return 3;
            if (totalScore <= 250) return 4;
            if (totalScore <= 400) return 5;
            if (totalScore < 1000) return 6;

            // For scores 1000 and above: level = (first digit of thousands) + 6
            // Examples: 1234 → level 7, 2567 → level 8, 3890 → level 9, etc.
            int thousandsDigit = totalScore / 1000;
            return thousandsDigit + 6;
        }

        /// <summary>
        /// Get the best category (highest score) and associated title
        /// </summary>
        public static (QuestionCategory bestCategory, string title) GetPlayerTitle(PlayerProgress progress)
        {
            if (progress.CategoryProgress == null || !progress.CategoryProgress.Any())
            {
                return (QuestionCategory.GeneralCulture, GetCategoryTitle(QuestionCategory.GeneralCulture, 0));
            }

            var bestCategory = progress.CategoryProgress
                .OrderByDescending(kvp => kvp.Value.Score)
                .First();

            string title = GetCategoryTitle(bestCategory.Key, bestCategory.Value.Score);
            return (bestCategory.Key, title);
        }

        /// <summary>
        /// Get title for a specific category based on score
        /// </summary>
        public static string GetCategoryTitle(QuestionCategory category, int score)
        {
            var titles = GetCategoryTitles(category);

            // Find appropriate title based on score thresholds
            for (int i = titles.Length - 1; i >= 0; i--)
            {
                if (score >= titles[i].minScore)
                {
                    return titles[i].title;
                }
            }

            return titles[0].title; // Default to first title
        }

        /// <summary>
        /// Get all titles and thresholds for a category
        /// </summary>
        private static (int minScore, string title)[] GetCategoryTitles(QuestionCategory category)
        {
            switch (category)
            {
                case QuestionCategory.Biology:
                    return new[]
                    {
                        (0, "Cell Curious"),
                        (25, "Life Explorer"),
                        (100, "Gene Genius"),
                        (250, "Evolution Expert"),
                        (1000, "Bio Master")
                    };

                case QuestionCategory.Gaming:
                    return new[]
                    {
                        (0, "Button Masher"),
                        (25, "Casual Gamer"),
                        (100, "Level Grinder"),
                        (250, "Achievement Hunter"),
                        (1000, "Gaming Legend")
                    };

                case QuestionCategory.History:
                    return new[]
                    {
                        (0, "Time Traveler"),
                        (25, "History Buff"),
                        (100, "Chronicle Keeper"),
                        (250, "Past Master"),
                        (1000, "Historical Sage")
                    };

                case QuestionCategory.Geography:
                    return new[]
                    {
                        (0, "Map Reader"),
                        (25, "Globe Trotter"),
                        (100, "World Explorer"),
                        (250, "Continental Expert"),
                        (1000, "Geography Guru")
                    };

                case QuestionCategory.Physics:
                    return new[]
                    {
                        (0, "Force Finder"),
                        (25, "Wave Rider"),
                        (100, "Quantum Questioner"),
                        (250, "Physics Phenom"),
                        (1000, "Einstein's Heir")
                    };

                case QuestionCategory.Cinema:
                    return new[]
                    {
                        (0, "Popcorn Lover"),
                        (25, "Movie Buff"),
                        (100, "Film Critic"),
                        (250, "Cinema Scholar"),
                        (1000, "Hollywood Hero")
                    };

                case QuestionCategory.Musique:
                    return new[]
                    {
                        (0, "Note Newbie"),
                        (25, "Melody Maker"),
                        (100, "Rhythm Master"),
                        (250, "Music Maestro"),
                        (1000, "Symphony Sage")
                    };

                case QuestionCategory.Math:
                    return new[]
                    {
                        (0, "Number Novice"),
                        (25, "Calculator Kid"),
                        (100, "Mathemagician"),
                        (250, "Equation Expert"),
                        (1000, "Mathematical Mastermind")
                    };

                case QuestionCategory.GeneralCulture:
                default:
                    return new[]
                    {
                        (0, "Knowledge Seeker"),
                        (25, "Trivia Enthusiast"),
                        (100, "Culture Connoisseur"),
                        (250, "Wisdom Warrior"),
                        (1000, "Renaissance Mind")
                    };
            }
        }

        /// <summary>
        /// Check if progress can be updated (30 minute global cooldown)
        /// </summary>
        public static (bool canUpdate, TimeSpan timeUntilNext) CanUpdateProgress(PlayerProgress progress)
        {
            if (progress.LastProgressUpdateTime == DateTime.MinValue)
            {
                return (true, TimeSpan.Zero); // First time
            }

            var timeSinceLastUpdate = DateTime.Now - progress.LastProgressUpdateTime;
            bool canUpdate = timeSinceLastUpdate.TotalMinutes >= 30;

            if (canUpdate)
            {
                return (true, TimeSpan.Zero);
            }
            else
            {
                var timeUntilNext = TimeSpan.FromMinutes(30) - timeSinceLastUpdate;
                return (false, timeUntilNext);
            }
        }

        /// <summary>
        /// Get progress summary for display
        /// </summary>
        public static string GetProgressSummary(PlayerProgress progress)
        {
            int totalScore = GetTotalScore(progress);
            int level = GetPlayerLevel(progress);
            var (bestCategory, title) = GetPlayerTitle(progress);

            return $"Level {level} | {title} | {totalScore} pts";
        }
    }

    /// <summary>
    /// Represents overall player progress
    /// </summary>
    public class PlayerProgress
    {
        public Dictionary<QuestionCategory, CategoryProgress> CategoryProgress { get; set; }
        public DateTime LastPlayed { get; set; } = DateTime.Now;
        public DateTime LastProgressUpdateTime { get; set; } = DateTime.MinValue;
    }

    /// <summary>
    /// Represents progress in a specific category
    /// </summary>
    public class CategoryProgress
    {
        public int Score { get; set; }
        public DifficultyLevel CurrentDifficulty { get; set; }
        public int QuestionsAnswered { get; set; }
        public int CorrectAnswers { get; set; }
    }

    /// <summary>
    /// Result of processing a question answer
    /// </summary>
    public class ProgressUpdateResult
    {
        public bool ProgressUpdated { get; set; }
        public bool IsCorrect { get; set; }
        public bool WasQuick { get; set; }
        public QuestionCategory Category { get; set; }
        public int PointsAwarded { get; set; }
        public bool DifficultyChanged { get; set; }
        public DifficultyLevel NewDifficulty { get; set; }
        public TimeSpan TimeUntilNextUpdate { get; set; }
    }
}