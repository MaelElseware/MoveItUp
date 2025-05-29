using System;
using System.IO;
using System.Text.RegularExpressions;
using TriviaExercise.Models;

namespace TriviaExercise.Helpers
{
    public static class CategoryHelper
    {
        /// <summary>
        /// Extracts the category from a filename based on patterns
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <returns>The detected category</returns>
        public static QuestionCategory GetCategoryFromFileName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

            // Remove "questions_" prefix if it exists
            if (fileName.StartsWith("questions_"))
            {
                fileName = fileName.Substring("questions_".Length);
            }

            // Check for specific patterns
            if (Regex.IsMatch(fileName, @"biolog"))
                return QuestionCategory.Biology;

            if (Regex.IsMatch(fileName, @"gam"))
                return QuestionCategory.Gaming;

            if (Regex.IsMatch(fileName, @"histor"))
                return QuestionCategory.History;

            if (Regex.IsMatch(fileName, @"geograph"))
                return QuestionCategory.Geography;

            if (Regex.IsMatch(fileName, @"physic"))
                return QuestionCategory.Physics;

            if (Regex.IsMatch(fileName, @"cinema|movie|film"))
                return QuestionCategory.Cinema;

            if (Regex.IsMatch(fileName, @"musiqu|music"))
                return QuestionCategory.Musique;

            if (Regex.IsMatch(fileName, @"math"))
                return QuestionCategory.Math;

            // Default category
            return QuestionCategory.GeneralCulture;
        }

        /// <summary>
        /// Gets a display-friendly name for a category
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns>Display name</returns>
        public static string GetCategoryDisplayName(QuestionCategory category)
        {
            switch (category)
            {
                case QuestionCategory.Biology:
                    return "Biology";
                case QuestionCategory.Gaming:
                    return "Gaming";
                case QuestionCategory.History:
                    return "History";
                case QuestionCategory.Geography:
                    return "Geography";
                case QuestionCategory.Physics:
                    return "Physics";
                case QuestionCategory.Cinema:
                    return "Cinema";
                case QuestionCategory.Musique:
                    return "Music";
                case QuestionCategory.Math:
                    return "Mathematics";
                case QuestionCategory.GeneralCulture:
                default:
                    return "General Culture";
            }
        }

        /// <summary>
        /// Gets an emoji icon for a category
        /// </summary>
        /// <param name="category">The category</param>
        /// <returns>Emoji string</returns>
        public static string GetCategoryIcon(QuestionCategory category)
        {
            switch (category)
            {
                case QuestionCategory.Biology:
                    return "🧬";
                case QuestionCategory.Gaming:
                    return "🎮";
                case QuestionCategory.History:
                    return "📜";
                case QuestionCategory.Geography:
                    return "🌍";
                case QuestionCategory.Physics:
                    return "⚛️";
                case QuestionCategory.Cinema:
                    return "🎬";
                case QuestionCategory.Musique:
                    return "🎵";
                case QuestionCategory.Math:
                    return "🔢";
                case QuestionCategory.GeneralCulture:
                default:
                    return "🧠";
            }
        }
    }
}