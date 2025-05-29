using System;
using System.IO;
using System.Media;
using System.Diagnostics;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Handles sound playback for the trivia application
    /// </summary>
    public static class SoundHelper
    {
        private static readonly string SOUNDS_FOLDER = "Sounds";
        private static readonly string SOUNDS_PATH;

        // Default sound file names
        private const string PRE_QUESTION_SOUND = "pre_question.wav";
        private const string DRINK_REMINDER_SOUND = "drink_reminder.wav";
        private const string QUESTION_SUCCESS_SOUND = "question_success.wav";
        private const string QUESTION_FAILURE_SOUND = "question_failure.wav";
        private const string NEW_QUESTION_SOUND = "new_question.wav";
        private const string EXERCISE_SOUND = "exercise_start.wav";

        static SoundHelper()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            SOUNDS_PATH = Path.Combine(baseDirectory, SOUNDS_FOLDER);

            // Create sounds directory if it doesn't exist
            try
            {
                if (!Directory.Exists(SOUNDS_PATH))
                {
                    Directory.CreateDirectory(SOUNDS_PATH);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not create sounds directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Play the pre-question alert sound
        /// </summary>
        public static void PlayPreQuestionAlert()
        {
            PlaySound(PRE_QUESTION_SOUND, "Pre-question alert");
        }

        /// <summary>
        /// Play the new question sound
        /// </summary>
        public static void PlayNewQuestionSound()
        {
            PlaySound(NEW_QUESTION_SOUND, "New Question Sound");
        }

        /// <summary>
        /// Play the new exercise sound
        /// </summary>
        public static void PlayNewExerciseSound()
        {
            PlaySound(EXERCISE_SOUND, "New Exercise Sound");
        }

        /// <summary>
        /// Play the drink reminder sound
        /// </summary>
        public static void PlayDrinkReminder()
        {
            PlaySound(DRINK_REMINDER_SOUND, "Drink reminder");
        }

        /// <summary>
        /// Play the question success sound
        /// </summary>
        public static void PlayQuestionSuccess()
        {
            PlaySound(QUESTION_SUCCESS_SOUND, "Question success");
        }

        /// <summary>
        /// Play the question failure sound
        /// </summary>
        public static void PlayQuestionFailure()
        {
            PlaySound(QUESTION_FAILURE_SOUND, "Question failure");
        }

        /// <summary>
        /// Play a sound file from the sounds directory
        /// </summary>
        /// <param name="fileName">Name of the sound file</param>
        /// <param name="description">Description for logging</param>
        private static void PlaySound(string fileName, string description)
        {
            try
            {
                string soundPath = Path.Combine(SOUNDS_PATH, fileName);

                if (File.Exists(soundPath))
                {
                    // Use SoundPlayer for WAV files
                    using (var player = new SoundPlayer(soundPath))
                    {
                        player.Play(); // Non-blocking play
                    }
                    Debug.WriteLine($"Played sound: {description} ({fileName})");
                }
                else
                {
                    // Fallback to system beep
                    PlaySystemBeep();
                    Debug.WriteLine($"Sound file not found: {soundPath}, using system beep for {description}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing sound {fileName}: {ex.Message}");
                // Fallback to system beep on error
                PlaySystemBeep();
            }
        }

        /// <summary>
        /// Play a system beep as fallback
        /// </summary>
        private static void PlaySystemBeep()
        {
            try
            {
                Console.Beep();
            }
            catch
            {
                // Some systems don't support Console.Beep, so we fail silently
            }
        }

        /// <summary>
        /// Check if sound files exist and create sample WAV files if needed
        /// </summary>
        /// <returns>Status message about sound files</returns>
        public static string InitializeSounds()
        {
            var missingFiles = new System.Collections.Generic.List<string>();
            var existingFiles = new System.Collections.Generic.List<string>();

            var soundFiles = new[]
            {
                PRE_QUESTION_SOUND,
                DRINK_REMINDER_SOUND,
                QUESTION_SUCCESS_SOUND,
                QUESTION_FAILURE_SOUND,
                EXERCISE_SOUND,
                NEW_QUESTION_SOUND
            };

            foreach (var soundFile in soundFiles)
            {
                string fullPath = Path.Combine(SOUNDS_PATH, soundFile);
                if (File.Exists(fullPath))
                {
                    existingFiles.Add(soundFile);
                }
                else
                {
                    missingFiles.Add(soundFile);
                }
            }

            string status = "";
            if (existingFiles.Count > 0)
            {
                status += $"✅ Found {existingFiles.Count} sound files\n";
            }

            if (missingFiles.Count > 0)
            {
                status += $"⚠️ Missing {missingFiles.Count} sound files (will use system beep):\n";
                foreach (var file in missingFiles)
                {
                    status += $"  - {file}\n";
                }
                status += $"Place WAV files in: {SOUNDS_PATH}\n";
            }

            return status;
        }

        /// <summary>
        /// Get the sounds directory path for user reference
        /// </summary>
        /// <returns>Full path to sounds directory</returns>
        public static string GetSoundsDirectory()
        {
            return SOUNDS_PATH;
        }

        /// <summary>
        /// Test all sound effects
        /// </summary>
        public static void TestAllSounds()
        {
            Debug.WriteLine("Testing all sounds...");

            PlayPreQuestionAlert();
            System.Threading.Thread.Sleep(500);

            PlayDrinkReminder();
            System.Threading.Thread.Sleep(500);

            PlayQuestionSuccess();
            System.Threading.Thread.Sleep(500);

            PlayQuestionFailure();

            Debug.WriteLine("Sound test completed");
        }
    }
}