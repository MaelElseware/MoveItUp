using System.Collections.Generic;

namespace TriviaExercise.Models
{
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Mixed // For random difficulty selection
    }

    public enum QuestionCategory
    {
        Biology,
        Gaming,
        History,
        Geography,
        Physics,
        GeneralCulture, // Default
        Cinema,
        Musique,
        Math // Procedural generation
    }

    public class Question
    {
        public string Text { get; set; }
        public List<string> Answers { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
        public QuestionCategory Category { get; set; } = QuestionCategory.GeneralCulture;
    }

    public class Exercise
    {
        public string Description { get; set; }
        public int? DurationSeconds { get; set; }
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
        public string Category { get; set; } // Optional: e.g., "Stretching", "Cardio", "Mental Break"
    }

    // For questions.json files
    public class QuestionsData
    {
        public List<Question> Questions { get; set; }
    }

    // For exercises.json
    public class ExercisesData
    {
        public List<Exercise> CorrectAnswerExercises { get; set; }
        public List<Exercise> WrongAnswerExercises { get; set; }
    }

    // Combined data for internal use
    public class TriviaData
    {
        public List<Question> Questions { get; set; }
        public List<Exercise> CorrectAnswerExercises { get; set; }
        public List<Exercise> WrongAnswerExercises { get; set; }
    }
}