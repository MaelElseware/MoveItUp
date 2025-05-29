# Move It Up! 🧠💪

**A fun WPF desktop application that combines trivia questions with physical exercises to keep you mentally sharp and physically active during work or study sessions.**

## 🎯 What It Does

Move It Up! periodically interrupts your computer time with trivia questions from various categories. Based on whether you answer correctly or incorrectly, you'll be prompted to do different physical exercises - turning learning into an active, engaging experience!

### Key Features

- 🎲 **Smart Question System**: Questions from 9 categories (Biology, Gaming, History, Geography, Physics, Cinema, Music, Math, General Culture)
- 🏃‍♂️ **Exercise Integration**: Get correct answers? Reward exercises! Wrong answers? Redemption exercises!
- 📈 **Progress Tracking**: Level up in different categories, earn points, and unlock difficulty tiers
- ⏰ **Customizable Timers**: Set question intervals (default: 50 minutes) and optional drink reminders
- 🎮 **Discord Rich Presence**: Show off your trivia prowess to friends
- 🔊 **Sound Effects**: Audio cues for questions, alerts, and achievements
- 💾 **Persistent Progress**: Your achievements and category progress are saved locally
- 🚀 **System Tray Support**: Runs minimized in the background
- 🧮 **Dynamic Math Questions**: Procedurally generated math problems based on your skill level

## 🏆 Gamification Features

- **Level System**: Progress from Level 1 to Level 10+ based on total score
- **Category Titles**: Earn specialized titles like "Gene Genius" (Biology) or "Gaming Legend" (Gaming)
- **Difficulty Progression**: Start with easy questions and advance to medium/hard as you improve
- **Quick Answer Bonus**: Double points for answering within the first 15 seconds
- **Anti-Spam Protection**: 30-minute cooldown between progress updates to prevent gaming the system

## 🎮 How It Works

1. **Set Your Timer**: Choose how often you want trivia breaks (1-1440 minutes)
2. **Answer Questions**: When prompted, answer multiple-choice questions from various categories
3. **Do Exercises**: Based on your performance, complete physical activities ranging from jumping jacks to planks
4. **Track Progress**: Watch your scores grow and difficulty levels increase in each category
5. **Stay Active**: Optional drink reminders and pre-question alerts keep you engaged

## 🛠️ Exercise Difficulty Modes

- **Match Question**: Exercise difficulty matches the question difficulty
- **Easy/Medium/Hard**: Fixed difficulty regardless of question
- **Mixed**: Random exercise difficulty for variety

## 🗂️ Data Files

The app uses JSON files for easy customization:
- `Questions_[Category].json`: Trivia questions organized by category
- `exercises.json`: Physical exercises for correct/incorrect answers
- Sample files are automatically created on first run

## 🔧 Technical Features

- Built with **WPF** and **.NET Framework 4.7.2**
- **System tray integration** with custom icon
- **Windows startup registration** (optional)
- **Local data persistence** in AppData folder
- **Discord Rich Presence** integration
- **Sound system** with custom WAV file support

## 🎨 Categories & Icons

- 🧬 **Biology** - From cells to evolution
- 🎮 **Gaming** - Video game trivia
- 📜 **History** - Historical events and figures  
- 🌍 **Geography** - Countries, capitals, and landmarks
- ⚛️ **Physics** - Scientific principles and discoveries
- 🎬 **Cinema** - Movies and film industry
- 🎵 **Music** - Musical knowledge and artists
- 🔢 **Mathematics** - Dynamically generated math problems
- 🧠 **General Culture** - Broad knowledge questions

## 📋 System Requirements

- Windows 7/8/10/11
- .NET Framework 4.7.2 or higher
- Discord (optional, for Rich Presence)
- Audio device (optional, for sound effects)

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

**Stay sharp, stay active, and Move It Up!** 🚀
