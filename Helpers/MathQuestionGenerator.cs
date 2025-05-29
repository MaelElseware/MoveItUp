using System;
using System.Collections.Generic;
using System.Linq;
using TriviaExercise.Models;

namespace TriviaExercise.Helpers
{
    public static class MathQuestionGenerator
    {
        private static Random random = new Random();

        /// <summary>
        /// Creates a random math question based on difficulty level
        /// </summary>
        /// <param name="difficulty">The difficulty level for the math question</param>
        /// <returns>A generated math question</returns>
        public static Question CreateMathQuestion(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return CreateEasyMathQuestion();
                case DifficultyLevel.Medium:
                    return CreateMediumMathQuestion();
                case DifficultyLevel.Hard:
                    return CreateHardMathQuestion();
                default:
                    // For Mixed difficulty, randomly choose between Easy, Medium, Hard
                    var difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard };
                    var randomDifficulty = difficulties[random.Next(difficulties.Length)];
                    return CreateMathQuestion(randomDifficulty);
            }
        }

        /// <summary>
        /// Creates an easy math question (addition or subtraction with whole numbers)
        /// </summary>
        /// <returns>An easy math question</returns>
        private static Question CreateEasyMathQuestion()
        {
            bool isAddition = random.Next(2) == 0;
            int num1, num2, correctAnswer;
            string questionText;

            if (isAddition)
            {
                // Addition: numbers between 10-500
                num1 = random.Next(10, 501);
                num2 = random.Next(10, 501);
                correctAnswer = num1 + num2;
                questionText = $"What is {num1} + {num2}?";
            }
            else
            {
                // Subtraction: ensure positive result
                num1 = random.Next(50, 1000);
                num2 = random.Next(10, num1);
                correctAnswer = num1 - num2;
                questionText = $"What is {num1} - {num2}?";
            }

            var answers = GenerateIntegerAnswers(correctAnswer);

            return new Question
            {
                Text = questionText,
                Answers = answers.Select(a => a.ToString()).ToList(),
                CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                Difficulty = DifficultyLevel.Easy,
                Category = QuestionCategory.Math
            };
        }

        /// <summary>
        /// Creates a medium math question (multiplication, division, or operations with decimals)
        /// </summary>
        /// <returns>A medium math question</returns>
        private static Question CreateMediumMathQuestion()
        {
            int operationType = random.Next(4); // 0: multiplication, 1: division, 2: addition with decimals, 3: subtraction with decimals

            switch (operationType)
            {
                case 0: // Multiplication
                    return CreateMultiplicationQuestion();
                case 1: // Division
                    return CreateDivisionQuestion();
                case 2: // Addition with decimals
                    return CreateDecimalAdditionQuestion();
                case 3: // Subtraction with decimals
                default:
                    return CreateDecimalSubtractionQuestion();
            }
        }

        /// <summary>
        /// Creates a hard math question (multiple operations or percentage calculations)
        /// </summary>
        /// <returns>A hard math question</returns>
        private static Question CreateHardMathQuestion()
        {
            bool isPercentage = random.Next(2) == 0;

            if (isPercentage)
            {
                return CreatePercentageQuestion();
            }
            else
            {
                return CreateMultipleOperationsQuestion();
            }
        }

        private static Question CreateMultiplicationQuestion()
        {
            int num1 = random.Next(12, 50);
            int num2 = random.Next(12, 50);
            int correctAnswer = num1 * num2;
            string questionText = $"What is {num1} × {num2}?";

            var answers = GenerateIntegerAnswers(correctAnswer);

            return new Question
            {
                Text = questionText,
                Answers = answers.Select(a => a.ToString()).ToList(),
                CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                Difficulty = DifficultyLevel.Medium,
                Category = QuestionCategory.Math
            };
        }

        private static Question CreateDivisionQuestion()
        {
            // Generate division that results in whole numbers
            int correctAnswer = random.Next(5, 100);
            int divisor = random.Next(2, 20);
            int dividend = correctAnswer * divisor;

            string questionText = $"What is {dividend} ÷ {divisor}?";

            var answers = GenerateIntegerAnswers(correctAnswer);

            return new Question
            {
                Text = questionText,
                Answers = answers.Select(a => a.ToString()).ToList(),
                CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                Difficulty = DifficultyLevel.Medium,
                Category = QuestionCategory.Math
            };
        }

        private static Question CreateDecimalAdditionQuestion()
        {
            double num1 = Math.Round(random.NextDouble() * 100 + 10, 1);
            double num2 = Math.Round(random.NextDouble() * 100 + 10, 1);
            double correctAnswer = Math.Round(num1 + num2, 1);

            string questionText = $"What is {num1} + {num2}?";

            var answers = GenerateDecimalAnswers(correctAnswer);

            return new Question
            {
                Text = questionText,
                Answers = answers.Select(a => a.ToString("F1")).ToList(),
                CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                Difficulty = DifficultyLevel.Medium,
                Category = QuestionCategory.Math
            };
        }

        private static Question CreateDecimalSubtractionQuestion()
        {
            double num1 = Math.Round(random.NextDouble() * 100 + 50, 1);
            double num2 = Math.Round(random.NextDouble() * num1, 1);
            double correctAnswer = Math.Round(num1 - num2, 1);

            string questionText = $"What is {num1} - {num2}?";

            var answers = GenerateDecimalAnswers(correctAnswer);

            return new Question
            {
                Text = questionText,
                Answers = answers.Select(a => a.ToString("F1")).ToList(),
                CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                Difficulty = DifficultyLevel.Medium,
                Category = QuestionCategory.Math
            };
        }

        private static Question CreatePercentageQuestion()
        {
            int percentage = random.Next(5, 50);
            int baseNumber = random.Next(100, 1000);
            double correctAnswer = Math.Round((double)(percentage * baseNumber) / 100, 1);

            string questionText = $"What is {percentage}% of {baseNumber}?";

            var answers = GenerateDecimalAnswers(correctAnswer);

            return new Question
            {
                Text = questionText,
                Answers = answers.Select(a => a.ToString("F1")).ToList(),
                CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                Difficulty = DifficultyLevel.Hard,
                Category = QuestionCategory.Math
            };
        }

        private static Question CreateMultipleOperationsQuestion()
        {
            int operationType = random.Next(3); // 0: (a + b) / c, 1: (a - b) * c, 2: (a * b) - c

            switch (operationType)
            {
                case 0: // (a + b) / c
                    {
                        double a = Math.Round(random.NextDouble() * 50 + 10, 1);
                        double b = Math.Round(random.NextDouble() * 50 + 10, 1);
                        int c = random.Next(2, 10);
                        double correctAnswer = Math.Round((a + b) / c, 1);

                        string questionText = $"What is ({a} + {b}) ÷ {c}?";
                        var answers = GenerateDecimalAnswers(correctAnswer);

                        return new Question
                        {
                            Text = questionText,
                            Answers = answers.Select(ans => ans.ToString("F1")).ToList(),
                            CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                            Difficulty = DifficultyLevel.Hard,
                            Category = QuestionCategory.Math
                        };
                    }
                case 1: // (a - b) * c
                    {
                        double a = Math.Round(random.NextDouble() * 100 + 50, 1);
                        double b = Math.Round(random.NextDouble() * 40 + 5, 1);
                        int c = random.Next(2, 8);
                        double correctAnswer = Math.Round((a - b) * c, 1);

                        string questionText = $"What is ({a} - {b}) × {c}?";
                        var answers = GenerateDecimalAnswers(correctAnswer);

                        return new Question
                        {
                            Text = questionText,
                            Answers = answers.Select(ans => ans.ToString("F1")).ToList(),
                            CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                            Difficulty = DifficultyLevel.Hard,
                            Category = QuestionCategory.Math
                        };
                    }
                case 2: // (a * b) - c
                default:
                    {
                        int a = random.Next(10, 30);
                        int b = random.Next(3, 10);
                        int c = random.Next(20, 100);
                        int correctAnswer = (a * b) - c;

                        string questionText = $"What is ({a} × {b}) - {c}?";
                        var answers = GenerateIntegerAnswers(correctAnswer);

                        return new Question
                        {
                            Text = questionText,
                            Answers = answers.Select(ans => ans.ToString()).ToList(),
                            CorrectAnswerIndex = answers.ToList().IndexOf(correctAnswer),
                            Difficulty = DifficultyLevel.Hard,
                            Category = QuestionCategory.Math
                        };
                    }
            }
        }

        /// <summary>
        /// Generates 4 integer answer choices with one correct answer
        /// </summary>
        /// <param name="correctAnswer">The correct answer</param>
        /// <returns>Array of 4 answer choices</returns>
        private static int[] GenerateIntegerAnswers(int correctAnswer)
        {
            var answers = new HashSet<int> { correctAnswer };

            // Generate 3 incorrect answers
            while (answers.Count < 4)
            {
                int variation = random.Next(10, Math.Max(11, correctAnswer / 3));
                int wrongAnswer = random.Next(2) == 0 ?
                    correctAnswer + variation :
                    Math.Max(1, correctAnswer - variation);

                answers.Add(wrongAnswer);
            }

            return answers.OrderBy(x => random.Next()).ToArray();
        }

        /// <summary>
        /// Generates 4 decimal answer choices with one correct answer
        /// </summary>
        /// <param name="correctAnswer">The correct answer</param>
        /// <returns>Array of 4 answer choices</returns>
        private static double[] GenerateDecimalAnswers(double correctAnswer)
        {
            var answers = new HashSet<double> { correctAnswer };

            // Generate 3 incorrect answers
            while (answers.Count < 4)
            {
                double variation = Math.Round(random.NextDouble() * Math.Max(5.0, correctAnswer * 0.3) + 0.5, 1);
                double wrongAnswer = random.Next(2) == 0 ?
                    Math.Round(correctAnswer + variation, 1) :
                    Math.Round(Math.Max(0.1, correctAnswer - variation), 1);

                answers.Add(wrongAnswer);
            }

            return answers.OrderBy(x => random.Next()).ToArray();
        }
    }
}