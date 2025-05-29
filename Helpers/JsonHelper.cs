using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TriviaExercise.Models;

namespace TriviaExercise.Helpers
{
    internal class JsonHelper
    {

        static public string CreateSampleJsonFilesIfNotExist(string questionsPath, string exercisesPath)
        {
            string ResultString = null;
            // Create sample questions.json
            if (!File.Exists(questionsPath))
            {
                var sampleQuestions = new QuestionsData
                {
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "What is the capital of France?",
                            Answers = new List<string> { "London", "Berlin", "Paris", "Madrid" },
                            CorrectAnswerIndex = 2,
                            Difficulty = DifficultyLevel.Easy,
                            Category = QuestionCategory.GeneralCulture
                        },
                        new Question
                        {
                            Text = "Which planet is known as the Red Planet?",
                            Answers = new List<string> { "Venus", "Mars", "Jupiter", "Saturn" },
                            CorrectAnswerIndex = 1,
                            Difficulty = DifficultyLevel.Easy,
                            Category = QuestionCategory.GeneralCulture
                        },
                        new Question
                        {
                            Text = "What is the chemical symbol for gold?",
                            Answers = new List<string> { "Go", "Gd", "Au", "Ag" },
                            CorrectAnswerIndex = 2,
                            Difficulty = DifficultyLevel.Medium,
                            Category = QuestionCategory.GeneralCulture
                        },
                        new Question
                        {
                            Text = "In which year did World War II end?",
                            Answers = new List<string> { "1944", "1945", "1946", "1947" },
                            CorrectAnswerIndex = 1,
                            Difficulty = DifficultyLevel.Medium,
                            Category = QuestionCategory.GeneralCulture
                        },
                        new Question
                        {
                            Text = "What is the derivative of x² with respect to x?",
                            Answers = new List<string> { "x", "2x", "x²", "2x²" },
                            CorrectAnswerIndex = 1,
                            Difficulty = DifficultyLevel.Hard,
                            Category = QuestionCategory.GeneralCulture
                        },
                        new Question
                        {
                            Text = "What is the square root of 144?",
                            Answers = new List<string> { "11", "12", "13", "14" },
                            CorrectAnswerIndex = 1,
                            Difficulty = DifficultyLevel.Hard,
                            Category = QuestionCategory.GeneralCulture
                        }
                    }
                };

                try
                {
                    string json = JsonConvert.SerializeObject(sampleQuestions, Formatting.Indented);
                    File.WriteAllText(questionsPath, json);
                    ResultString = $"Created sample questions file at: {questionsPath}\n";
                }
                catch (Exception ex)
                {
                    ResultString = $"Error creating questions file: {ex.Message}\n";
                }
            }

            // Create sample exercises.json (unchanged)
            if (!File.Exists(exercisesPath))
            {
                var sampleExercises = new ExercisesData
                {
                    CorrectAnswerExercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Description = "Great job! Stand up and do 5 jumping jacks!",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Cardio"
                        },
                        new Exercise
                        {
                            Description = "Excellent! Take 10 deep breaths and stretch your arms above your head.",
                            DurationSeconds = 30,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Stretching"
                        },
                        new Exercise
                        {
                            Description = "Well done! Do 10 push-ups or wall push-ups if needed.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Strength"
                        },
                        new Exercise
                        {
                            Description = "Perfect! Hold a plank position for as long as you can.",
                            DurationSeconds = 60,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Strength"
                        },
                        new Exercise
                        {
                            Description = "Outstanding! Do 20 burpees - you've earned this challenge!",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Cardio"
                        },
                        new Exercise
                        {
                            Description = "Amazing! Run in place for 2 minutes at high intensity.",
                            DurationSeconds = 120,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Cardio"
                        }
                    },
                    WrongAnswerExercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Description = "Oops! Walk around your room 3 times to get those brain cells moving.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Mental Break"
                        },
                        new Exercise
                        {
                            Description = "Not quite! Do some gentle neck rolls and shoulder shrugs.",
                            DurationSeconds = 45,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Stretching"
                        },
                        new Exercise
                        {
                            Description = "Close, but not quite! Do 15 squats to boost your energy.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Strength"
                        },
                        new Exercise
                        {
                            Description = "Try again next time! March in place for 90 seconds.",
                            DurationSeconds = 90,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Cardio"
                        },
                        new Exercise
                        {
                            Description = "Tough question! Do 30 mountain climbers to reset your focus.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Cardio"
                        },
                        new Exercise
                        {
                            Description = "No worries! Take a 2-minute mindful breathing break.",
                            DurationSeconds = 120,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Mental Break"
                        }
                    }
                };

                try
                {
                    string json = JsonConvert.SerializeObject(sampleExercises, Formatting.Indented);
                    File.WriteAllText(exercisesPath, json);
                    ResultString += $"Created sample exercises file at: {exercisesPath}\n";
                }
                catch (Exception ex)
                {
                    ResultString += $"Error creating exercises file: {ex.Message}\n";
                }
            }
            return ResultString;
        }
    }
}
