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

            // Create sample exercises.json with image support
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
                            Category = "Cardio",
                            ImageFileName = "JumpingJacks.png" // Example image
                        },
                        new Exercise
                        {
                            Description = "Excellent! Take 10 deep breaths and stretch your arms above your head.",
                            DurationSeconds = 30,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Stretching",
                        },
                        new Exercise
                        {
                            Description = "Well done! Do 10 push-ups or wall push-ups if needed.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Strength",
                        },
                        new Exercise
                        {
                            Description = "Perfect! Hold a plank position for as long as you can.",
                            DurationSeconds = 60,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Strength",
                            ImageFileName = "Plank.png" // Example image
                        },
                        new Exercise
                        {
                            Description = "Outstanding! Do 20 burpees - you've earned this challenge!",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Cardio"
                            // No image for this one - demonstrates optional nature
                        },
                        new Exercise
                        {
                            Description = "Amazing! Run in place for 2 minutes at high intensity.",
                            DurationSeconds = 120,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Cardio",
                        }
                    },
                    WrongAnswerExercises = new List<Exercise>
                    {
                        new Exercise
                        {
                            Description = "Oops! Walk around your room 3 times to get those brain cells moving.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Mental Break",
                        },
                        new Exercise
                        {
                            Description = "Not quite! Do some gentle neck rolls and shoulder shrugs.",
                            DurationSeconds = 45,
                            Difficulty = DifficultyLevel.Easy,
                            Category = "Stretching",
                        },
                        new Exercise
                        {
                            Description = "Close, but not quite! Do 15 squats to boost your energy.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Strength",
                            ImageFileName = "Squats.png" // Example image
                        },
                        new Exercise
                        {
                            Description = "Try again next time! March in place for 90 seconds.",
                            DurationSeconds = 90,
                            Difficulty = DifficultyLevel.Medium,
                            Category = "Cardio"
                            // No image for this one
                        },
                        new Exercise
                        {
                            Description = "Tough question! Do 20 high knee jacks to reset your focus.",
                            DurationSeconds = null,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Cardio",
                            ImageFileName = "HighKneeJack.png" // Example image
                        },
                        new Exercise
                        {
                            Description = "No worries! Take a 2-minute mindful breathing break.",
                            DurationSeconds = 120,
                            Difficulty = DifficultyLevel.Hard,
                            Category = "Mental Break",
                        }
                    }
                };

                try
                {
                    string json = JsonConvert.SerializeObject(sampleExercises, Formatting.Indented);
                    File.WriteAllText(exercisesPath, json);
                    ResultString += $"Created sample exercises file at: {exercisesPath}\n";

                    // Also create the Illustrations folder and add a note about images
                    CreateIllustrationsFolder();
                }
                catch (Exception ex)
                {
                    ResultString += $"Error creating exercises file: {ex.Message}\n";
                }
            }
            return ResultString;
        }

        /// <summary>
        /// Create the Illustrations folder and add a readme file
        /// </summary>
        private static void CreateIllustrationsFolder()
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string illustrationsPath = Path.Combine(baseDirectory, "Illustrations");

                if (!Directory.Exists(illustrationsPath))
                {
                    Directory.CreateDirectory(illustrationsPath);

                    // Create a readme file explaining the image requirements
                    string readmePath = Path.Combine(illustrationsPath, "README.txt");
                    string readmeContent = @"Exercise Illustrations Folder
=============================

This folder should contain PNG images for exercises.

Image Requirements:
- Format: PNG files only
- Recommended size: 1200x1000 pixels (width x height)
- File naming: Use descriptive names matching the ImageFileName in exercises.json

Example images referenced in the sample exercises.json:
- Plank.png
- HighKneeJack.png
- Squats.png
- JumpingJacks.png

Images will be displayed at a maximum size of 240x200 pixels in the exercise window,
maintaining aspect ratio.

If an image file is not found, an error message will be displayed instead.
";

                    File.WriteAllText(readmePath, readmeContent);
                    System.Diagnostics.Debug.WriteLine($"Created Illustrations folder and README at: {illustrationsPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Illustrations folder: {ex.Message}");
            }
        }
    }
}