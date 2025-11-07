# TableFlo - Quick Start Guide

Get up and running with TableFlo in 5 minutes!

---

## 🚀 Quick Start

### 1. Open the Project

**Option A: Visual Studio**
```
1. Open Visual Studio 2022
2. Click "Open a project or solution"
3. Navigate to: /Users/coderay/Desktop/Casino/TableFlo.sln
4. Click Open
```

**Option B: Command Line**
```bash
cd /Users/coderay/Desktop/Casino
dotnet build
dotnet run --project src/TableFlo.UI/TableFlo.UI.csproj
```

### 2. First Run

On first launch, TableFlo will:
- ✅ Create the SQLite database (`tableflo.db`)
- ✅ Seed sample data (11 dealers, 10 tables, 3 shifts)
- ✅ Show the login window

### 3. Login

Use the demo credentials:
```
Employee Number: ADMIN001
Password: admin123
```

### 4. Explore

You'll see:
- **Header** - TableFlo logo, navigation, user info
- **Section 1** - Current dealers on tables
- **Section 2** - Next scheduled dealers
- **Section 3** - Dealers on break

---

## 🎮 What You Can Do (Current Build)

✅ **Working Features:**
- Login with employee authentication
- View the main dashboard layout
- See the modern dark theme with gold accents
- Explore the 3-section rotation view
- Logout (returns to login screen)

⏳ **Coming Soon:**
- Data displayed in grids
- Push button functionality
- AI schedule generation
- Break management
- Analytics dashboard

---

## 🗂️ Sample Data

### Dealers (11 Total)
- George Soros
- Bob Schmidt  
- Jimmy John
- Chris Scoff
- Sally Jones
- Amy Smith
- Wilson Chen
- Garcia Lopez
- Keller Brown
- Davis Taylor
- Nguyen Tran

All have random certifications in Blackjack, Roulette, Craps, Pai Gow, and Baccarat.

### Tables (10 Total)
- **BJ1, BJ2** - Blackjack ($25-$1000)
- **BJ302** - Blackjack High Limit ($50-$2000)
- **R12, R301** - Roulette
- **CR4** - Craps (requires 3 dealers)
- **PG101** - Pai Gow
- **TX311** - Texas Hold'em
- **SD101** - Spanish 21
- **BJ102** - Closed table

### Accounts
- **ADMIN001** / admin123 (Admin/Supervisor)
- **D1001** / dealer123 (George Soros)
- **D1002** / dealer123 (Bob Schmidt)
- _(All dealers: password is "dealer123")_

---

## 📁 Important Files

| File | Purpose |
|------|---------|
| `README.md` | Full documentation |
| `PROJECT_STATUS.md` | Development status & roadmap |
| `QUICKSTART.md` | This file |
| `TableFlo.sln` | Visual Studio solution |
| `tableflo.db` | SQLite database (auto-created) |

---

## 🐛 Troubleshooting

### "Database not found"
- **Fix:** Delete `tableflo.db` and restart the app - it will recreate

### "Cannot find TableFlo.sln"
- **Fix:** Make sure you're in `/Users/coderay/Desktop/Casino/` directory

### "Build errors"
- **Fix:** Restore NuGet packages: `dotnet restore`

### "Login fails"
- **Fix:** Use exactly: `ADMIN001` / `admin123` (case sensitive)

---

## 📚 Next Steps

1. **Read the full README** - `/Users/coderay/Desktop/Casino/README.md`
2. **Check project status** - `/Users/coderay/Desktop/Casino/PROJECT_STATUS.md`
3. **Explore the code** - Start with `src/TableFlo.Core/Models/`
4. **Review the AI engine** - `src/TableFlo.Services/SchedulingService.cs`

---

## 💬 Key Concepts

### MVVM Pattern
- **Model** - Data classes in `TableFlo.Core/Models/`
- **View** - XAML files in `TableFlo.UI/Views/`
- **ViewModel** - Logic classes in `TableFlo.UI/ViewModels/`

### AI Scheduling
The `SchedulingService` uses a multi-factor scoring algorithm:
- Skill matching (highest priority)
- Fairness scoring (rotation diversity)
- Seniority consideration
- Preferred pit bonuses
- Break timing penalties
- Movement efficiency

### Data Flow
```
UI (WPF) 
  ↓
ViewModels (MVVM)
  ↓
Services (Business Logic)
  ↓
Repositories (Data Access)
  ↓
Database (SQLite)
```

---

## 🎯 Current Focus

The foundation is complete. Current development priorities:

1. **Data Binding** - Connect UI to actual data
2. **Button Actions** - Wire up push/break functionality
3. **Management Screens** - Add dealer/table CRUD operations

---

**Ready to build something amazing!** 🚀

For detailed information, see `README.md` and `PROJECT_STATUS.md`.

