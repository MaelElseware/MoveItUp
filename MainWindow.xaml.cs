using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using TriviaExercise.Models;
using System.Drawing;
using System.Collections.Generic;
using TriviaExercise.Helpers;
using System.Diagnostics;

namespace TriviaExercise
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer questionTimer;
        private DispatcherTimer drinkTimer;
        private DispatcherTimer displayUpdateTimer;
        private DispatcherTimer preQuestionAlertTimer;

        private TriviaData triviaData;
        private NotifyIcon notifyIcon;
        private Random random = new Random();
        private DateTime nextQuestionTime;
        private DateTime nextDrinkTime;
        private PlayerProgress playerProgress;

        private DiscordRichPresence discordRPC;
        private bool isQuestionActive = false;
        private bool isExerciseActive = false;

        private AppSettings.Settings appSettings;
        private bool isLoadingSettings = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemTray();

            discordRPC = new DiscordRichPresence();

            // Load settings before setting defaults
            LoadApplicationSettings();

            // Set default data folder if not already set in settings
            if (string.IsNullOrEmpty(appSettings.DataFolderPath))
            {
                SetDefaultDataFolder();
            }
            else
            {
                DataFolderPathTextBox.Text = appSettings.DataFolderPath;
            }

            InitializeDisplayUpdateTimer();
            LoadPlayerProgress();

            // Auto-start the timer when the app launches
            Loaded += MainWindow_Loaded;
        }

        // Method to load settings and apply them to UI
        private void LoadApplicationSettings()
        {
            isLoadingSettings = true;
            try
            {
                appSettings = AppSettings.LoadSettings();
                ApplySettingsToUI();

                StatusTextBox.Text += $"Settings loaded: {AppSettings.GetSettingsSummary(appSettings)}\n";
            }
            finally
            {
                isLoadingSettings = false;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Check command line arguments for minimized startup
            string[] args = Environment.GetCommandLineArgs();
            bool shouldStartMinimized = args.Length > 1 && args[1] == "--minimized";

            // Override with settings if command line says to start minimized
            if (shouldStartMinimized)
            {
                StartMinimizedCheckBox.IsChecked = true;
                appSettings.StartMinimized = true;
            }

            // Update startup checkbox state
            RefreshStartupStatus();

            // Load data files info
            UpdateLoadedFilesDisplay();

            // Update progress display
            UpdatePlayerProgressDisplay();

            // Auto-enable Discord Rich Presence if setting is enabled
            if (appSettings.DiscordRichPresenceEnabled)
            {
                DiscordRichPresenceCheckBox_CheckedChanged(null, null);
            }

            // Auto-start the timer when the app launches
            StartButton_Click(null, null);
            InitializeSoundSystem();

            // Start minimized if setting is enabled or launched from startup
            if (appSettings.StartMinimized || shouldStartMinimized)
            {
                MinimizeToTray();
            }
        }

        private void InitializeSoundSystem()
        {
            // Initialize sound system and display status
            string soundStatus = SoundHelper.InitializeSounds();
            StatusTextBox.Text += $"\n{soundStatus}";
        }

        private void ApplySettingsToUI()
        {
            // Apply question interval
            IntervalTextBox.Text = appSettings.QuestionIntervalMinutes.ToString();

            // Apply drink reminder settings
            DrinkReminderCheckBox.IsChecked = appSettings.DrinkReminderEnabled;
            DrinkIntervalTextBox.Text = appSettings.DrinkIntervalMinutes.ToString();

            // Apply start minimized setting
            StartMinimizedCheckBox.IsChecked = appSettings.StartMinimized;

            // Apply exercise difficulty
            SetExerciseDifficultyFromSettings();

            // Apply Discord Rich Presence setting
            DiscordRichPresenceCheckBox.IsChecked = appSettings.DiscordRichPresenceEnabled;

            // Apply sound settings
            SoundsEnabledCheckBox.IsChecked = appSettings.SoundsEnabled;
            PreQuestionAlertTextBox.Text = appSettings.PreQuestionAlertMinutes.ToString();

            // Data folder path is applied in constructor
        }

        private void LoadPlayerProgress()
        {
            playerProgress = PlayerProgressSystem.LoadProgress();
        }

        private void UpdatePlayerProgressDisplay()
        {
            if (playerProgress == null) return;

            // Update main status (no cooldown display here anymore)
            int totalScore = PlayerProgressSystem.GetTotalScore(playerProgress);
            int level = PlayerProgressSystem.GetPlayerLevel(playerProgress);
            var (bestCategory, title) = PlayerProgressSystem.GetPlayerTitle(playerProgress);

            PlayerTitleTextBlock.Text = $"Level {level} | {title} | {totalScore} pts";

            // Update category details
            var categoryDetails = new List<string>();

            foreach (var category in Enum.GetValues(typeof(QuestionCategory)).Cast<QuestionCategory>())
            {
                if (playerProgress.CategoryProgress.ContainsKey(category))
                {
                    var progress = playerProgress.CategoryProgress[category];
                    string icon = CategoryHelper.GetCategoryIcon(category);
                    string name = CategoryHelper.GetCategoryDisplayName(category);
                    string difficulty = progress.CurrentDifficulty.ToString();
                    string categoryTitle = PlayerProgressSystem.GetCategoryTitle(category, progress.Score);

                    categoryDetails.Add($"{icon} {name}: {progress.Score}pts [{difficulty}] \"{categoryTitle}\"");
                }
            }

            CategoryProgressTextBlock.Text = string.Join("\n", categoryDetails);

            // Update Discord Rich Presence
            discordRPC?.UpdatePlayerProgress(playerProgress);
        }

        private string FormatCooldownTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1)
            {
                return $"{(int)Math.Ceiling(timeSpan.TotalSeconds)}s";
            }
            else if (timeSpan.TotalHours < 1)
            {
                int minutes = (int)Math.Ceiling(timeSpan.TotalMinutes);
                return $"{minutes}min";
            }
            else
            {
                int hours = (int)timeSpan.TotalHours;
                int minutes = timeSpan.Minutes;
                return minutes > 0 ? $"{hours}h {minutes}min" : $"{hours}h";
            }
        }

        private void InitializeDisplayUpdateTimer()
        {
            displayUpdateTimer = new DispatcherTimer();
            displayUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            displayUpdateTimer.Tick += DisplayUpdateTimer_Tick;
            displayUpdateTimer.Start();
        }

        private void DisplayUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            if (questionTimer?.IsEnabled == true)
            {
                var timeUntilNext = nextQuestionTime - DateTime.Now;
                if (timeUntilNext.TotalSeconds > 0)
                {
                    NextQuestionTextBlock.Text = $"Next question in: {FormatTimeSpan(timeUntilNext)}";
                }
                else
                {
                    NextQuestionTextBlock.Text = "Question due now!";
                }
            }
            else
            {
                NextQuestionTextBlock.Text = "Timer not running";
            }

            if (drinkTimer?.IsEnabled == true)
            {
                var timeUntilDrink = nextDrinkTime - DateTime.Now;
                if (timeUntilDrink.TotalSeconds > 0)
                {
                    NextDrinkTextBlock.Text = $"Next drink reminder in: {FormatTimeSpan(timeUntilDrink)}";
                    NextDrinkTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    NextDrinkTextBlock.Text = "Drink reminder due now!";
                    NextDrinkTextBlock.Visibility = Visibility.Visible;
                }
            }
            else
            {
                NextDrinkTextBlock.Visibility = Visibility.Collapsed;
            }

            // Update progress cooldown display
            if (playerProgress != null)
            {
                var (canUpdate, timeUntilNext) = PlayerProgressSystem.CanUpdateProgress(playerProgress);
                if (!canUpdate)
                {
                    string timeDisplay = FormatCooldownTime(timeUntilNext);
                    ProgressCooldownTextBlock.Text = $"Progress locked for: {timeDisplay}";
                    ProgressCooldownTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    ProgressCooldownTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ProgressCooldownTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{timeSpan.Seconds}s";
            }
        }

        private void IntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the question timer if it's running
            if (questionTimer?.IsEnabled == true)
            {
                UpdateQuestionTimer();
                // Restart pre-question alert timer with new interval
                if (appSettings.SoundsEnabled && appSettings.PreQuestionAlertMinutes > 0)
                {
                    preQuestionAlertTimer?.Stop();
                    if (int.TryParse(IntervalTextBox.Text, out int interval) && interval > 0)
                    {
                        StartPreQuestionAlertTimer(interval);
                    }
                }
            }
            SaveApplicationSettings();
        }

        private void DrinkIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the drink timer if it's running
            if (drinkTimer?.IsEnabled == true)
            {
                UpdateDrinkTimer();
            }
            SaveApplicationSettings();
        }

        private void DrinkReminderCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (DrinkReminderCheckBox.IsChecked == true)
            {
                if (questionTimer?.IsEnabled == true) // Only start if main timer is running
                {
                    StartDrinkTimer();
                }
            }
            else
            {
                StopDrinkTimer();
            }
            SaveApplicationSettings();
        }

        private void ExerciseDifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveApplicationSettings();
        }

        private void SoundsEnabledCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SaveApplicationSettings();
        }

        private void PreQuestionAlertTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveApplicationSettings();
        }

        private void UpdateQuestionTimer()
        {
            if (!int.TryParse(IntervalTextBox.Text, out int interval) || interval <= 0)
            {
                return;
            }

            questionTimer.Stop();
            questionTimer.Interval = TimeSpan.FromMinutes(interval);
            nextQuestionTime = DateTime.Now.Add(questionTimer.Interval);
            questionTimer.Start();
        }

        private void UpdateDrinkTimer()
        {
            if (!int.TryParse(DrinkIntervalTextBox.Text, out int interval) || interval <= 0)
            {
                return;
            }

            if (drinkTimer != null)
            {
                drinkTimer.Stop();
                drinkTimer.Interval = TimeSpan.FromMinutes(interval);
                nextDrinkTime = DateTime.Now.Add(drinkTimer.Interval);
                drinkTimer.Start();
            }
        }

        private void StartDrinkTimer()
        {
            if (!int.TryParse(DrinkIntervalTextBox.Text, out int interval) || interval <= 0)
            {
                return;
            }

            drinkTimer = new DispatcherTimer();
            drinkTimer.Interval = TimeSpan.FromMinutes(interval);
            drinkTimer.Tick += DrinkTimer_Tick;
            nextDrinkTime = DateTime.Now.Add(drinkTimer.Interval);
            drinkTimer.Start();
        }

        private void StopDrinkTimer()
        {
            drinkTimer?.Stop();
            drinkTimer = null;
        }

        private void DrinkTimer_Tick(object sender, EventArgs e)
        {
            ShowDrinkReminder();
            nextDrinkTime = DateTime.Now.Add(drinkTimer.Interval);
        }

        private void ShowDrinkReminder()
        {
            if (appSettings.SoundsEnabled)
            {
                SoundHelper.PlayDrinkReminder();
            }

            System.Windows.MessageBox.Show("💧 Time to drink some water! Stay hydrated! 💧",
                "Drink Reminder", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InitializeSystemTray()
        {
            notifyIcon = new NotifyIcon();

            // Try to load custom icon, fallback to default if not found
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MoveItUp.ico");
                if (File.Exists(iconPath))
                {
                    notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    notifyIcon.Icon = SystemIcons.Application;
                }
            }
            catch (Exception ex)
            {
                // Fallback to system icon if loading fails
                notifyIcon.Icon = SystemIcons.Application;
                StatusTextBox.Text += $"Warning: Could not load custom icon for system tray: {ex.Message}\n";
            }

            notifyIcon.Text = "Move It Up !";
            notifyIcon.Visible = false;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => ShowWindow());
            contextMenu.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            notifyIcon.Visible = false;
        }

        private void SetDefaultDataFolder()
        {
            if (string.IsNullOrEmpty(appSettings.DataFolderPath))
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                DataFolderPathTextBox.Text = baseDirectory;
            }
            else
            {
                DataFolderPathTextBox.Text = appSettings.DataFolderPath;
            }

            string sourceDirectory = DataFolderPathTextBox.Text;
            string questionsPath = Path.Combine(sourceDirectory, "Questions_GeneralCulture.json");
            string exercisesPath = Path.Combine(sourceDirectory, "exercises.json");
            StatusTextBox.Text += JsonHelper.CreateSampleJsonFilesIfNotExist(questionsPath, exercisesPath);
        }

        private void UpdateLoadedFilesDisplay()
        {
            if (!Directory.Exists(DataFolderPathTextBox.Text))
            {
                LoadedFilesTextBlock.Text = "❌ Folder not found";
                return;
            }

            var questionFiles = Directory.GetFiles(DataFolderPathTextBox.Text, "Questions*.json");
            var exerciseFile = Path.Combine(DataFolderPathTextBox.Text, "exercises.json");
            string ExerciseFileStatus;

            // Clear the file settings display - show minimal info
            LoadedFilesTextBlock.Text = $"✅ Ready ({questionFiles.Length} question files)";
            if (File.Exists(exerciseFile))
            {
                ExerciseFileStatus = "✅ exercises.json found";
            }
            else
            {
                ExerciseFileStatus = "❌ exercises.json not found";
            }
            LoadedFilesTextBlock.Text += $" - {ExerciseFileStatus}";

            // Build detailed info for status log
            string detailedInfo = $"Found {questionFiles.Length} question file(s)";
            if (questionFiles.Length > 0)
            {
                detailedInfo += ":\n";
                foreach (var file in questionFiles)
                {
                    var category = CategoryHelper.GetCategoryFromFileName(file);
                    var icon = CategoryHelper.GetCategoryIcon(category);
                    var displayName = CategoryHelper.GetCategoryDisplayName(category);
                    detailedInfo += $"  {icon} {Path.GetFileName(file)} ({displayName})\n";
                }
            }

            detailedInfo += ExerciseFileStatus;


            // Add the detailed info to status log
            StatusTextBox.Text += $"\n{detailedInfo}\n";
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Data Folder";
                folderDialog.SelectedPath = DataFolderPathTextBox.Text;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DataFolderPathTextBox.Text = folderDialog.SelectedPath;
                    UpdateLoadedFilesDisplay();
                    SaveApplicationSettings();
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoadTriviaData())
                return;

            if (!int.TryParse(IntervalTextBox.Text, out int interval) || interval <= 0)
            {
                StatusTextBox.Text += "\nPlease enter a valid interval in minutes.";
                return;
            }

            // Start question timer
            questionTimer = new DispatcherTimer();
            questionTimer.Interval = TimeSpan.FromMinutes(interval);
            questionTimer.Tick += QuestionTimer_Tick;
            nextQuestionTime = DateTime.Now.Add(questionTimer.Interval);
            questionTimer.Start();

            // Start pre-question alert timer if sounds are enabled
            if (appSettings.SoundsEnabled && appSettings.PreQuestionAlertMinutes > 0)
            {
                StartPreQuestionAlertTimer(interval);
            }

            // Start drink timer if enabled
            if (DrinkReminderCheckBox.IsChecked == true)
            {
                StartDrinkTimer();
            }

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            var selectedExerciseDifficulty = GetSelectedExerciseDifficulty();

            string drinkStatus = DrinkReminderCheckBox.IsChecked == true ?
                $"Drink reminders every {DrinkIntervalTextBox.Text} minute(s)." :
                "No drink reminders.";

            StatusTextBox.Text += $"\nTimer started! Questions will appear every {interval} minute(s).\n" +
                               $"Exercise Difficulty: {GetExerciseDifficultyDisplayName(selectedExerciseDifficulty)}\n" +
                               drinkStatus;

            if (StartMinimizedCheckBox.IsChecked == true)
            {
                MinimizeToTray();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            questionTimer?.Stop();
            preQuestionAlertTimer?.Stop();
            StopDrinkTimer();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusTextBox.Text = "Timer stopped.";
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoadTriviaData())
            {
                ShowRandomQuestion();
            }
        }

        private ExerciseDifficultyMode GetSelectedExerciseDifficulty()
        {
            var selectedItem = ExerciseDifficultyComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string tag = selectedItem.Tag.ToString();
                switch (tag)
                {
                    case "Easy":
                        return ExerciseDifficultyMode.Easy;
                    case "Medium":
                        return ExerciseDifficultyMode.Medium;
                    case "Hard":
                        return ExerciseDifficultyMode.Hard;
                    case "Mixed":
                        return ExerciseDifficultyMode.Mixed;
                    case "MatchQuestion":
                    default:
                        return ExerciseDifficultyMode.MatchQuestion;
                }
            }
            return ExerciseDifficultyMode.MatchQuestion; // Default fallback
        }

        private string GetExerciseDifficultyDisplayName(ExerciseDifficultyMode mode)
        {
            switch (mode)
            {
                case ExerciseDifficultyMode.Easy:
                    return "Easy";
                case ExerciseDifficultyMode.Medium:
                    return "Medium";
                case ExerciseDifficultyMode.Hard:
                    return "Hard";
                case ExerciseDifficultyMode.Mixed:
                    return "Mixed (Random)";
                case ExerciseDifficultyMode.MatchQuestion:
                default:
                    return "Match Question";
            }
        }

        private bool LoadTriviaData()
        {
            try
            {
                string dataFolder = DataFolderPathTextBox.Text;

                if (!Directory.Exists(dataFolder))
                {
                    StatusTextBox.Text = "Data folder not found. Please select a valid folder.";
                    return false;
                }

                // Load all question files that start with "Questions"
                var questionFiles = Directory.GetFiles(dataFolder, "Questions*.json");
                var allQuestions = new List<Question>();

                foreach (var file in questionFiles)
                {
                    try
                    {
                        string questionsJson = File.ReadAllText(file);
                        var questionsData = JsonConvert.DeserializeObject<QuestionsData>(questionsJson);

                        if (questionsData?.Questions != null && questionsData.Questions.Count > 0)
                        {
                            // Determine category from filename
                            var category = CategoryHelper.GetCategoryFromFileName(file);

                            // Set category for all questions in this file
                            foreach (var question in questionsData.Questions)
                            {
                                question.Category = category;
                            }

                            allQuestions.AddRange(questionsData.Questions);
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusTextBox.Text += $"Error loading {Path.GetFileName(file)}: {ex.Message}\n";
                    }
                }

                if (allQuestions.Count == 0)
                {
                    StatusTextBox.Text = "No questions found in any Questions*.json files.";
                    return false;
                }

                // Load exercises
                string exercisesPath = Path.Combine(dataFolder, "exercises.json");
                if (!File.Exists(exercisesPath))
                {
                    StatusTextBox.Text = "exercises.json file not found in the data folder.";
                    return false;
                }

                string exercisesJson = File.ReadAllText(exercisesPath);
                var exercisesData = JsonConvert.DeserializeObject<ExercisesData>(exercisesJson);

                if (exercisesData?.CorrectAnswerExercises == null && exercisesData?.WrongAnswerExercises == null)
                {
                    StatusTextBox.Text = "No exercises found in the exercises.json file.";
                    return false;
                }

                // Combine data
                triviaData = new TriviaData
                {
                    Questions = allQuestions,
                    CorrectAnswerExercises = exercisesData.CorrectAnswerExercises ?? new List<Exercise>(),
                    WrongAnswerExercises = exercisesData.WrongAnswerExercises ?? new List<Exercise>()
                };

                int correctExercises = triviaData.CorrectAnswerExercises.Count;
                int wrongExercises = triviaData.WrongAnswerExercises.Count;

                // Show summary by category
                var categorySummary = allQuestions
                    .GroupBy(q => q.Category)
                    .Select(g => $"{CategoryHelper.GetCategoryIcon(g.Key)} {CategoryHelper.GetCategoryDisplayName(g.Key)}: {g.Count()}")
                    .ToList();

                StatusTextBox.Text = $"Loaded {allQuestions.Count} questions total:\n" +
                                   string.Join("\n", categorySummary) + "\n" +
                                   $"{correctExercises} correct exercises, {wrongExercises} wrong exercises.";
                return true;
            }
            catch (Exception ex)
            {
                StatusTextBox.Text = $"Error loading data files: {ex.Message}";
                return false;
            }
        }

        private void QuestionTimer_Tick(object sender, EventArgs e)
        {
            // Don't show question if one is already active or exercise is running
            if (isQuestionActive || isExerciseActive)
            {
                StatusTextBox.Text += "\n⏸️ Question skipped - another window is active";
                nextQuestionTime = DateTime.Now.Add(questionTimer.Interval);
                return;
            }

            ShowRandomQuestion();
            nextQuestionTime = DateTime.Now.Add(questionTimer.Interval);
            if (appSettings.SoundsEnabled && appSettings.PreQuestionAlertMinutes > 0)
            {
                preQuestionAlertTimer?.Stop();
                if (int.TryParse(IntervalTextBox.Text, out int interval) && interval > 0)
                {
                    StartPreQuestionAlertTimer(interval);
                }
            }
        }

        private void ShowRandomQuestion()
        {
            if (triviaData?.Questions?.Count > 0)
            {
                // Check if question or exercise is already active
                if (isQuestionActive || isExerciseActive)
                {
                    StatusTextBox.Text += "\n⏸️ Question blocked - window already active";
                    return;
                }
                if (appSettings.SoundsEnabled)
                {
                    SoundHelper.PlayNewQuestionSound();
                }
                var selectedExerciseDifficulty = GetSelectedExerciseDifficulty();
                Question question = null;

                // 10% chance to generate a math question instead of using file questions
                bool generateMathQuestion = random.Next(100) < 10;

                if (generateMathQuestion)
                {
                    // Generate a math question based on current math category difficulty
                    DifficultyLevel mathDifficulty = PlayerProgressSystem.GetCategoryDifficulty(playerProgress, QuestionCategory.Math);
                    question = MathQuestionGenerator.CreateMathQuestion(mathDifficulty);
                    StatusTextBox.Text += $"\n🔢 Generated math question (Difficulty: {mathDifficulty})";
                }
                else
                {
                    // Select a random category from available questions
                    var availableCategories = triviaData.Questions
                        .GroupBy(q => q.Category)
                        .ToList();

                    if (availableCategories.Count > 0)
                    {
                        var selectedCategoryGroup = availableCategories[random.Next(availableCategories.Count)];
                        var selectedCategory = selectedCategoryGroup.Key;

                        // Get current difficulty for this category
                        var categoryDifficulty = PlayerProgressSystem.GetCategoryDifficulty(playerProgress, selectedCategory);

                        // Filter questions by category and difficulty
                        var filteredQuestions = selectedCategoryGroup
                            .Where(q => q.Difficulty == categoryDifficulty)
                            .ToList();

                        if (filteredQuestions.Count > 0)
                        {
                            question = filteredQuestions[random.Next(filteredQuestions.Count)];
                        }
                        else
                        {
                            // Fallback to any question from this category if no matching difficulty
                            question = selectedCategoryGroup.ElementAt(random.Next(selectedCategoryGroup.Count()));
                            StatusTextBox.Text += $"\nNo {categoryDifficulty} questions found for {selectedCategory}, showing random from category.";
                        }
                    }
                }

                if (question != null)
                {
                    // Set question as active and update Discord
                    isQuestionActive = true;
                    discordRPC?.SetActivity("In Question", $"Answering {CategoryHelper.GetCategoryDisplayName(question.Category)} question");

                    var questionWindow = new QuestionWindow(question, triviaData, selectedExerciseDifficulty, playerProgress);
                    questionWindow.QuestionAnswered += OnQuestionAnswered;
                    questionWindow.ExerciseRequested += OnExerciseRequested;

                    // Handle window closing to reset activity flag
                    questionWindow.Closed += (s, e) =>
                    {
                        isQuestionActive = false;
                        discordRPC?.SetActivity("Idle");
                    };

                    questionWindow.Show();
                }
            }
        }

        private void OnQuestionAnswered(object sender, QuestionAnsweredEventArgs e)
        {
            // Play appropriate sound
            if (appSettings.SoundsEnabled)
            {
                if (e.IsCorrect)
                {
                    SoundHelper.PlayQuestionSuccess();
                }
                else
                {
                    SoundHelper.PlayQuestionFailure();
                }
            }

            // Process the answer and update progress
            var result = PlayerProgressSystem.ProcessAnswer(playerProgress, e.Category, e.QuestionDifficulty, e.IsCorrect, e.WasQuick);

            // Update the display
            UpdatePlayerProgressDisplay();

            // Log the result with appropriate feedback
            string resultText = e.IsCorrect ? "✅ Correct" : "❌ Wrong";
            string speed = e.WasQuick ? " (Quick!)" : "";
            string categoryIcon = CategoryHelper.GetCategoryIcon(e.Category);

            if (result.ProgressUpdated)
            {
                string pointsText = "";
                if (result.PointsAwarded > 0)
                {
                    pointsText = $" +{result.PointsAwarded}pts";
                }
                else if (result.PointsAwarded < 0)
                {
                    pointsText = $" {result.PointsAwarded}pt"; // Already includes the minus sign
                }

                string difficultyText = result.DifficultyChanged ? $" → {result.NewDifficulty}" : "";
                StatusTextBox.Text += $"\n{categoryIcon} {resultText}{speed}{pointsText}{difficultyText}";
            }
            else
            {
                // Show cooldown message with better formatting
                string timeDisplay = FormatCooldownTime(result.TimeUntilNextUpdate);
                StatusTextBox.Text += $"\n{categoryIcon} {resultText}{speed} (Progress in {timeDisplay} - anti-spam)";
            }
        }

        private void OnExerciseRequested(object sender, ExerciseRequestedEventArgs e)
        {
            if (isExerciseActive)
            {
                return;
            }

            if (appSettings.SoundsEnabled)
            {
                SoundHelper.PlayNewExerciseSound();
            }

            isExerciseActive = true;
            discordRPC?.SetActivity("Exercising", e.Exercise.Description.Length > 50 ?
                e.Exercise.Description.Substring(0, 47) + "..." : e.Exercise.Description);

            var exerciseWindow = new ExerciseWindow(e.IsCorrect, e.Exercise);
            exerciseWindow.Closed += (s, e2) =>
            {
                isExerciseActive = false;
                discordRPC?.SetActivity("Idle");
            };
            exerciseWindow.Show();
        }

        private void MinimizeToTray()
        {
            Hide();
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000, "Move It Up !",
                "App minimized to system tray. Double-click to restore.", ToolTipIcon.Info);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && questionTimer?.IsEnabled == true)
            {
                MinimizeToTray();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Save settings before closing
            SaveApplicationSettings();

            displayUpdateTimer?.Stop();
            questionTimer?.Stop();
            preQuestionAlertTimer?.Stop();
            drinkTimer?.Stop();
            notifyIcon?.Dispose();
            discordRPC?.Dispose();
            base.OnClosing(e);
        }

        private void StartMinimizedCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            SaveApplicationSettings();
        }
        private void StartupCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool isChecked = StartupCheckBox.IsChecked == true;

            if (isChecked)
            {
                bool success = StartupHelper.RegisterStartup(true); // Start minimized
                if (success)
                {
                    StatusTextBox.Text += "\nRegistered for Windows startup.\n";
                }
                else
                {
                    StatusTextBox.Text += "\nFailed to register for Windows startup.\n";
                    // Uncheck the checkbox if registration failed
                    StartupCheckBox.IsChecked = false;
                }
            }
            else
            {
                bool success = StartupHelper.UnregisterStartup();
                if (success)
                {
                    StatusTextBox.Text += "\nUnregistered from Windows startup.\n";
                }
                else
                {
                    StatusTextBox.Text += "\nFailed to unregister from Windows startup.\n";
                    // Re-check the checkbox if unregistration failed
                    StartupCheckBox.IsChecked = true;
                }
            }

            // Automatically refresh status after any change
            RefreshStartupStatus();
        }

        private void RefreshStartupStatus()
        {
            bool isRegistered = StartupHelper.IsRegisteredForStartup();

            // Temporarily remove event handlers to prevent recursion
            StartupCheckBox.Checked -= StartupCheckBox_CheckedChanged;
            StartupCheckBox.Unchecked -= StartupCheckBox_CheckedChanged;

            StartupCheckBox.IsChecked = isRegistered;

            // Re-add event handlers
            StartupCheckBox.Checked += StartupCheckBox_CheckedChanged;
            StartupCheckBox.Unchecked += StartupCheckBox_CheckedChanged;

            string command = StartupHelper.GetStartupCommand();
            if (!string.IsNullOrEmpty(command))
            {
                StatusTextBox.Text += $"\nStartup status: Registered\nCommand: {command}\n";
            }
            else
            {
                StatusTextBox.Text += "\nStartup status: Not registered\n";
            }
        }

        // ========================== PREQUESTION STUFF ==========================
        private void StartPreQuestionAlertTimer(int questionIntervalMinutes)
        {
            if (!double.TryParse(PreQuestionAlertTextBox.Text, out double alertMinutes) || alertMinutes <= 0)
            {
                return;
            }

            // Don't set alert if it's longer than or equal to the question interval
            if (alertMinutes >= questionIntervalMinutes)
            {
                StatusTextBox.Text += $"\n⚠️ Pre-question alert ({alertMinutes}min) disabled - longer than question interval ({questionIntervalMinutes}min)";
                return;
            }

            double alertIntervalMinutes = questionIntervalMinutes - alertMinutes;

            preQuestionAlertTimer = new DispatcherTimer();
            preQuestionAlertTimer.Interval = TimeSpan.FromMinutes(alertIntervalMinutes);
            preQuestionAlertTimer.Tick += PreQuestionAlertTimer_Tick;
            preQuestionAlertTimer.Start();
        }

        private void PreQuestionAlertTimer_Tick(object sender, EventArgs e)
        {
            if (appSettings.SoundsEnabled)
            {
                SoundHelper.PlayPreQuestionAlert();
                StatusTextBox.Text += "\n🔔 Pre-question alert played\n";
            }
        }

        // ========================== SAVING STUFF ==========================

        // method to set exercise difficulty combo box from settings
        private void SetExerciseDifficultyFromSettings()
        {
            foreach (ComboBoxItem item in ExerciseDifficultyComboBox.Items)
            {
                if (item.Tag != null && Enum.TryParse<ExerciseDifficultyMode>(item.Tag.ToString(), out var mode))
                {
                    if (mode == appSettings.ExerciseDifficulty)
                    {
                        ExerciseDifficultyComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        // method to save current settings
        private void SaveApplicationSettings()
        {
            // Don't save during loading phase
            if (isLoadingSettings) return;

            try
            {
                // Update settings from UI controls
                if (int.TryParse(IntervalTextBox.Text, out int questionInterval))
                {
                    appSettings.QuestionIntervalMinutes = questionInterval;
                }

                if (int.TryParse(DrinkIntervalTextBox.Text, out int drinkInterval))
                {
                    appSettings.DrinkIntervalMinutes = drinkInterval;
                }

                if (double.TryParse(PreQuestionAlertTextBox.Text, out double preQuestionAlert))
                {
                    appSettings.PreQuestionAlertMinutes = preQuestionAlert;
                }

                appSettings.DrinkReminderEnabled = DrinkReminderCheckBox.IsChecked == true;
                appSettings.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
                appSettings.DiscordRichPresenceEnabled = DiscordRichPresenceCheckBox.IsChecked == true;
                appSettings.SoundsEnabled = SoundsEnabledCheckBox.IsChecked == true;
                appSettings.ExerciseDifficulty = GetSelectedExerciseDifficulty();

                // Save data folder if it's different from default
                string defaultFolder = AppDomain.CurrentDomain.BaseDirectory;
                appSettings.DataFolderPath = DataFolderPathTextBox.Text.Equals(defaultFolder, StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : DataFolderPathTextBox.Text;

                // Save to disk
                bool success = AppSettings.SaveSettings(appSettings);
                if (success)
                {
                    StatusTextBox.Text += "\n✅ Settings saved successfully.\n";
                }
                else
                {
                    StatusTextBox.Text += "\n❌ Failed to save settings.\n";
                }
            }
            catch (Exception ex)
            {
                StatusTextBox.Text += $"\n❌ Error saving settings: {ex.Message}\n";
            }
        }

        // ========================== DISCORD STUFF ==========================

        private void DiscordRichPresenceCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (isLoadingSettings) return;

            bool isEnabled = DiscordRichPresenceCheckBox.IsChecked == true;
            
            if (isEnabled)
            {
                bool success = discordRPC.Initialize(true);
                if (success)
                {
                    // Update with current progress immediately
                    discordRPC.UpdatePlayerProgress(playerProgress);
                    discordRPC.SetActivity("Idle");
                    StatusTextBox.Text += "\n✅ Discord Rich Presence enabled successfully.";
                }
                else
                {
                    StatusTextBox.Text += "\n❌ Failed to enable Discord Rich Presence. Is Discord running?";
                    // Uncheck the checkbox if initialization failed
                    DiscordRichPresenceCheckBox.IsChecked = false;
                }
            }
            else
            {
                discordRPC.Disable();
                StatusTextBox.Text += "\n🔌 Discord Rich Presence disabled.";
            }
            SaveApplicationSettings();
        }

        // handle exercise window tracking:
        private void ShowExerciseWindow(bool isCorrect, Exercise exercise)
        {
            if (isExerciseActive)
            {
                return; // Don't show multiple exercise windows
            }

            isExerciseActive = true;
            discordRPC?.SetActivity("Exercising", exercise.Description.Length > 50 ?
                exercise.Description.Substring(0, 47) + "..." : exercise.Description);

            var exerciseWindow = new ExerciseWindow(isCorrect, exercise);
            exerciseWindow.Closed += (s, e) =>
            {
                isExerciseActive = false;
                discordRPC?.SetActivity("Idle");
            };
            exerciseWindow.Show();
        }
    }

    // New enum for exercise difficulty modes
    public enum ExerciseDifficultyMode
    {
        Easy,
        Medium,
        Hard,
        Mixed,
        MatchQuestion
    }

    // Event args for question answered
    public class QuestionAnsweredEventArgs : EventArgs
    {
        public QuestionCategory Category { get; set; }
        public DifficultyLevel QuestionDifficulty { get; set; }
        public bool IsCorrect { get; set; }
        public bool WasQuick { get; set; }
    }
}