# Move It Up! 🧠💪

**A fun desktop application that combines trivia questions with physical exercises to keep you physically active during long work sessions.**

## 🤔 Why This App Exists

I can never seem to remember it... when I'm deeply focused on coding, I often forget to take breaks and move around. Hours can pass without me getting up from my chair... This sedentary behavior isn't just uncomfortable : **prolonged sitting has been linked to increased risks of cardiovascular disease, diabetes, and musculoskeletal problems**. Research shows that **sitting for extended periods can reduce blood circulation, weaken core muscles, and negatively impact posture and mental clarity**.

Rather than relying on willpower alone (which I clearly lack of), I created this gamified system to automatically remind me to move (itup) while keeping my brain engaged with fun questions.
We all tend to ignore notifications so asking a general culture question seemed like an interesting lever in order to force you to consider what's happening on screen.
But don't worry, the app will tell you X minutes (configurable) before a new exercise happens and if you are in a meeting you can using the window tray context menu to reset the timer.
The app also features an inactivity checker (mouse and keyboard input) which will stop the timer if you are gone for Y minutes (configurable).

![AppScreenshot](https://github.com/user-attachments/assets/640cabd4-0082-4119-9fa1-a52bd394180c)

*The main control panel showing timer settings, progress tracking, and category statistics*



### Key Features

- 📝 **Fully Customizable Content**: Easily modify questions and exercises through JSON files - **1000+ pre-made questions included in the release!**
- 🎲 **Smart Question System**: Questions from 9 categories (Biology, Gaming, History, Geography, Physics, Cinema, Music, Calculus (procedurally generated), General Culture)
- 🏃‍♂️ **Exercise Integration**: Get correct answers? Reward exercises! Wrong answers? Redemption exercises!
- 📈 **Progress Tracking/Progression**: Level up in different categories, earn points, and unlock difficulty tiers
- ⏰ **Customizable Timers**: Set question intervals (default: 50 minutes) and optional drink reminders
- 🎮 **Discord Rich Presence**: Show off your trivia prowess to friends
- 🔊 **Sound Effects**: Audio cues for questions, alerts, and achievements
- 💾 **Persistent Progress**: Your achievements and category progress are saved
- 🚀 **System Tray Support**: Runs minimized in the background, use the context menu to handle the app behavior without opening it
- 🧮 **Dynamic Math Questions**: Procedurally generated math calculus based on your skill level

## 🏆 Gamification Features

- **Level System**: Progress from Level 1 to Level 10+ based on total score
- **Category Titles**: Earn specialized titles like "Gene Genius" (Biology) or "Gaming Legend" (Gaming) and share with your friends
- **Difficulty Progression**: Start with easy questions and advance to medium/hard as you improve
- **Quick Answer Bonus**: Double points for answering within the first 15 seconds
- **Anti-Spam Protection**: 30-minute cooldown between progress updates to prevent gaming the system

## 🎮 How It Works

1. **Set Your Timers**: Choose how often you want trivia breaks (1-1440 minutes) and inactivity threshold.
2. **Answer Questions**: When prompted, answer multiple-choice questions from one of various categories
3. **Do Exercises**: Based on your performance, complete physical activities ranging from jumping jacks to planks
4. **Track Progress**: Watch your scores grow and difficulty levels increase in each category
5. **Stay Active**: Optional drink reminders and pre-question alerts keep you engaged

## 🛠️ Exercise Difficulty Modes

- **Match Question**: Exercise difficulty matches the question difficulty
- **Easy/Medium/Hard**: Fixed difficulty regardless of question
- **Mixed**: Random exercise difficulty for variety

## 🗂️ Customizable Content System

**✨ 1000+ Questions Included Out of the Box!** The released version comes with over 1000 trivia questions across all categories.

### Easy JSON Customization
The app uses simple JSON files that **anyone** can edit:
- 📚 **`Questions_[Category].json`**: Add your own trivia questions organized by category
- 🏃‍♂️ **`exercises.json`**: Customize physical exercises for correct/incorrect answers
- 🎯 **Multiple Difficulty Levels**: Each question and exercise can have Easy/Medium/Hard difficulty
- 📁 **Auto-Generated Samples**: Sample files are automatically created on first run for reference if none were downloaded

### Content Categories:
- 🧬 **Biology** (100+ questions) - From cells to evolution
- 🎮 **Gaming** (150+ questions) - Video game trivia and industry knowledge
- 📜 **History** (120+ questions) - Historical events, figures, and timelines
- 🌍 **Geography** (110+ questions) - Countries, capitals, landmarks, and cultures
- ⚛️ **Physics** (90+ questions) - Scientific principles and discoveries
- 🎬 **Cinema** (130+ questions) - Movies, actors, directors, and film industry
- 🎵 **Music** (100+ questions) - Musical knowledge, artists, and genres
- 🔢 **Mathematics** (Infinite) - Dynamically generated math mental calculus
- 🧠 **General Culture** (200+ questions) - Broad knowledge and trivia

**Want more content?** Simply add your own questions to the JSON files or create new category files!

## 🔧 Technical Features

- Built with **WPF** and **.NET Framework 4.7.2**
- **System tray integration** with custom icon
- **Windows startup registration** (optional)
- **Discord Rich Presence** integration

## 📋 System Requirements

- Windows 7/8/10/11
- .NET Framework 4.7.2 or higher
- Discord (optional, for Rich Presence)
- Audio device (optional, for sound effects)

---

## 🔧 Creating a Complete Release

To create a fully functioning release of Move It Up!, you'll need to include several additional files alongside the compiled executable:
All minimal files are included with each rebuild (sounds and icon). And a base json for questions or exercise are also generated apon first launch if none are available.

### Required Files Structure
```
MoveItUp-Release/
├── TriviaExercise.exe
├── TriviaExercise.exe.config
├── (other .dll files from build)
├── MoveItUp.ico                    # Application icon
├── Sounds/                         # Sound effects folder
│   ├── pre_question.wav
│   ├── drink_reminder.wav
│   ├── question_success.wav
│   ├── question_failure.wav
│   ├── new_question.wav
│   └── exercise_start.wav
└── Questions_GeneralCulture.json   # Sample questions (auto-generated)
```

### 🎵 Sound Files Setup
Makes sur a `Sounds` folder is in the same directory as your executable or add these WAV files: (provided within latest release)
- **`pre_question.wav`** - Plays before questions (30-second warning)
- **`drink_reminder.wav`** - Water break reminder sound
- **`question_success.wav`** - Correct answer celebration
- **`question_failure.wav`** - Wrong answer sound
- **`new_question.wav`** - New question alert
- **`exercise_start.wav`** - Exercise time notification

### 🚀 Release Checklist
- [ ] Compile in Release mode
- [ ] Check that MoveItUp.ico is in executable directory
- [ ] Check Sounds folder with its 6 WAV files
- [ ] Verify system tray icon displays correctly
- [ ] Test question loading and exercise prompts
- [ ] (optional) Ensure Discord Rich Presence works (if Discord is installed)
---

## 📄 License

This project is licensed under the **Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License**.

### What this means:
- ✅ You can use, modify, and share this code for personal, educational, or non-commercial purposes
- ✅ You must give appropriate credit to the original author
- ✅ If you remix, transform, or build upon the material, you must distribute your contributions under the same license
- ❌ You cannot use this code for commercial purposes without explicit permission

For commercial licensing inquiries, please contact the repository owner.

---

**Stay active, stay alive, and Move It Up!** 🚀
