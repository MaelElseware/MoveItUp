using System;
using System.Diagnostics;
using TriviaExercise.Models;
using TriviaExercise.Helpers;
using DiscordRPC;
using DiscordRPC.Logging;

namespace TriviaExercise.Helpers
{
    /// <summary>
    /// Handles Discord Rich Presence integration for the trivia app
    /// </summary>
    public class DiscordRichPresence : IDisposable
    {
        private DiscordRpcClient client;
        private bool isEnabled;
        private bool isDisposed;

        // Discord Application ID - You'll need to create a Discord app at https://discord.com/developers/applications
        private const string DISCORD_APP_ID = "1377554667118268509";

        // Current state tracking
        private PlayerProgress currentProgress;
        private string currentActivity;
        private DateTime? activityStartTime;

        public bool IsEnabled
        {
            get => isEnabled;
            private set => isEnabled = value;
        }

        public bool IsConnected => client?.IsInitialized == true && !client.IsDisposed;

        /// <summary>
        /// Initialize Discord Rich Presence
        /// </summary>
        /// <param name="enabled">Whether to enable Discord integration</param>
        public bool Initialize(bool enabled)
        {
            if (!enabled)
            {
                Disable();
                return true;
            }

            try
            {
                // Check if Discord is running
                if (!IsDiscordRunning())
                {
                    Debug.WriteLine("Discord is not running - Rich Presence disabled");
                    return false;
                }

                // Initialize Discord RPC client
                client = new DiscordRpcClient(DISCORD_APP_ID);

                // Set up logging (optional)
                client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

                // Initialize the connection
                if (client.Initialize())
                {
                    IsEnabled = true;
                    activityStartTime = DateTime.UtcNow;

                    Debug.WriteLine("Discord Rich Presence initialized successfully");
                    return true;
                }
                else
                {
                    Debug.WriteLine("Failed to initialize Discord Rich Presence");
                    client?.Dispose();
                    client = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing Discord Rich Presence: {ex.Message}");
                client?.Dispose();
                client = null;
                return false;
            }
        }

        /// <summary>
        /// Update the player's progress information
        /// </summary>
        public void UpdatePlayerProgress(PlayerProgress progress)
        {
            currentProgress = progress;
            if (IsEnabled && IsConnected)
            {
                UpdatePresence();
            }
        }

        /// <summary>
        /// Set the current activity (Idle, In Question, Exercising)
        /// </summary>
        public void SetActivity(string activity, string details = null)
        {
            currentActivity = activity;

            if (activity != "Idle" && activityStartTime == null)
            {
                activityStartTime = DateTime.UtcNow;
            }
            else if (activity == "Idle")
            {
                activityStartTime = DateTime.UtcNow;
            }

            if (IsEnabled && IsConnected)
            {
                UpdatePresence(details);
            }
        }

        /// <summary>
        /// Update Discord presence with current information
        /// </summary>
        private void UpdatePresence(string customDetails = null)
        {
            if (!IsEnabled || !IsConnected || currentProgress == null)
                return;

            try
            {
                // Get player info
                int totalScore = PlayerProgressSystem.GetTotalScore(currentProgress);
                int level = PlayerProgressSystem.GetPlayerLevel(currentProgress);
                var (bestCategory, title) = PlayerProgressSystem.GetPlayerTitle(currentProgress);

                // Build presence
                var presence = new RichPresence()
                {
                    Details = customDetails ?? GetActivityDetails(),
                    State = $"Level {level} • {title}",
                    Assets = new Assets()
                    {
                        LargeImageKey = "trivia_logo", // You'll need to upload this to your Discord app
                        LargeImageText = "Move It Up! - Trivia & Exercise",
                        SmallImageKey = GetCategoryImageKey(bestCategory),
                        SmallImageText = $"Best at {CategoryHelper.GetCategoryDisplayName(bestCategory)}"
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = activityStartTime
                    }
                };

                // Add party info if in active session
                if (currentActivity != "Idle")
                {
                    presence.Party = new Party()
                    {
                        ID = "trivia_session",
                        Size = 1,
                        Max = 1
                    };
                }

                client.SetPresence(presence);
                Debug.WriteLine($"Discord presence updated: {presence.Details} | {presence.State}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating Discord presence: {ex.Message}");
            }
        }

        /// <summary>
        /// Get activity details based on current state
        /// </summary>
        private string GetActivityDetails()
        {
            if (currentProgress == null)
                return "Getting started...";

            int totalScore = PlayerProgressSystem.GetTotalScore(currentProgress);

            switch (currentActivity)
            {
                case "In Question":
                    return "Answering trivia question";
                case "Exercising":
                    return "Doing exercise routine";
                case "Idle":
                    return $"Ready to learn • {totalScore} points";
                default:
                    return $"Learning and moving • {totalScore} points";
            }
        }

        /// <summary>
        /// Get Discord image key for category
        /// </summary>
        private string GetCategoryImageKey(QuestionCategory category)
        {
            // These image keys need to be uploaded to your Discord application
            switch (category)
            {
                case QuestionCategory.Biology:
                    return "biology_icon";
                case QuestionCategory.Gaming:
                    return "gaming_icon";
                case QuestionCategory.History:
                    return "history_icon";
                case QuestionCategory.Geography:
                    return "geography_icon";
                case QuestionCategory.Physics:
                    return "physics_icon";
                case QuestionCategory.Cinema:
                    return "cinema_icon";
                case QuestionCategory.Musique:
                    return "music_icon";
                case QuestionCategory.Math:
                    return "math_icon";
                case QuestionCategory.GeneralCulture:
                    return "generalknowledge_icon";
                default:
                    return "generalknowledge_icon";
            }
        }

        /// <summary>
        /// Check if Discord is currently running
        /// </summary>
        private bool IsDiscordRunning()
        {
            try
            {
                var discordProcesses = Process.GetProcessesByName("Discord");
                return discordProcesses.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disable Discord Rich Presence
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;

            if (client != null)
            {
                try
                {
                    if (client.IsInitialized && !client.IsDisposed)
                    {
                        client.ClearPresence();
                        // Add a small delay to ensure the clear command is processed
                        System.Threading.Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error clearing Discord presence: {ex.Message}");
                }

                try
                {
                    if (!client.IsDisposed)
                    {
                        client.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing Discord client: {ex.Message}");
                }
                finally
                {
                    client = null;
                }
            }

            Debug.WriteLine("Discord Rich Presence disabled");
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                Disable();
                isDisposed = true;
            }
        }
    }
}