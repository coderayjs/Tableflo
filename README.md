# TableFlo

**The Casino Workforce Automation Engine**

AI-powered dealer rotations, break scheduling, skill tracking, and real-time compliance — built specifically for casinos.

---

## 🎰 Overview

TableFlo revolutionizes casino dealer floor operations through innovative automation solutions. Our AI-powered system eliminates the headaches of manual scheduling, ensures fairness, maintains compliance, and optimizes dealer rotations in real-time.

### Key Features

✅ **AI-Powered Scheduling** - Automatic dealer rotation generation based on skills, fairness, and compliance  
✅ **Real-Time Dashboard** - Live view of all tables, dealers, and break status  
✅ **Fair Rotation Engine** - Ensures equal distribution of games and prevents favoritism  
✅ **Break Compliance** - Automatic tracking and alerts for labor law compliance  
✅ **Call-In Handling** - AI instantly reassigns tables when dealers call in sick  
✅ **Skill Matching** - Certifications and proficiency levels tracked per game type  
✅ **Audit Trail** - Complete logging of all actions with employee attribution  
✅ **Analytics Dashboard** - Performance metrics and rotation efficiency insights  

---

## 🏗️ Architecture

TableFlo follows professional enterprise patterns:

- **WPF** - Modern Windows Presentation Foundation UI
- **MVVM** - Model-View-ViewModel pattern for clean separation
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Dependency Injection** - Loose coupling and testability
- **Entity Framework Core** - SQLite database (easily adaptable to SQL Server)

### Project Structure

```
TableFlo/
├── src/
│   ├── TableFlo.Core/          # Models, Enums, Interfaces
│   ├── TableFlo.Data/          # EF Core, Repositories, Database
│   ├── TableFlo.Services/      # Business Logic, AI Engine
│   └── TableFlo.UI/            # WPF Views, ViewModels, Styles
└── TableFlo.sln                # Visual Studio Solution
```

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10/11**
- **.NET 8.0 SDK** or later
- **Visual Studio 2022** (Community, Professional, or Enterprise)

### Installation

1. **Clone or open the repository:**
   ```bash
   cd /Users/coderay/Desktop/Casino
   ```

2. **Open the solution:**
   - Open `TableFlo.sln` in Visual Studio

3. **Restore NuGet packages:**
   - Visual Studio will automatically restore packages
   - Or run: `dotnet restore`

4. **Build the solution:**
   - Press `Ctrl+Shift+B` in Visual Studio
   - Or run: `dotnet build`

5. **Run the application:**
   - Press `F5` in Visual Studio
   - Or run: `dotnet run --project src/TableFlo.UI/TableFlo.UI.csproj`

### Demo Credentials

```
Employee Number: ADMIN001
Password: admin123
```

---

## 🎯 Core Functionality

### AI Scheduling Engine

The heart of TableFlo is its AI optimization engine that considers:

1. **Skill Matching** - Matches dealer certifications to game types
2. **Proficiency Levels** - Assigns experienced dealers to high-limit tables
3. **Fairness Scoring** - Tracks rotation diversity to prevent same-game assignments
4. **Workload Balance** - Ensures equal time distribution among dealers
5. **Break Compliance** - Guarantees timely breaks and meal periods
6. **Movement Efficiency** - Minimizes cross-casino walking by pit optimization
7. **Seniority Consideration** - Respects hierarchy where applicable

### Real-Time Operations

- **Push Management** - Swap current and next dealers with one click
- **Break Tracking** - Send dealers to break and auto-manage return
- **Live Status** - See who's dealing, who's pushing, who's on break
- **Coverage Alerts** - Instant notification of unstaffed tables
- **Countdown Timers** - Visual alerts for upcoming rotations

### Compliance & Audit

- **Activity Logging** - Every action tracked with timestamp and user
- **Labor Law Compliance** - Automatic break/meal deadline enforcement
- **Historical Reports** - Export logs for compliance audits
- **Session Management** - Track who was "the pencil" for each shift

---

## 📊 Data Models

### Core Entities

- **Employee** - All staff (dealers, supervisors, managers)
- **Dealer** - Extended profile with certifications and availability
- **Table** - Game tables with requirements and status
- **Assignment** - Dealer-to-table assignments (current and next)
- **BreakRecord** - Break and meal tracking
- **AuditLog** - Complete activity trail
- **DealerCertification** - Game skills and proficiency levels

### Game Types Supported

- Blackjack
- Roulette
- Craps (with role-specific assignments)
- Pai Gow
- Baccarat
- Three Card Poker
- Texas Hold'em
- Ultimate Texas Hold'em
- Mississippi Stud
- Spanish 21

---

## 🎨 UI Design

TableFlo features a modern, professional dark theme inspired by premium casino applications:

- **Dark Background** - Reduces eye strain during long shifts
- **Gold Accents** - Professional casino-style branding
- **3-Section Layout** - Clear separation of current, next, and break status
- **Responsive Design** - Works on various screen sizes
- **Intuitive Controls** - Minimal training required

---

## 🔧 Configuration

### Database

By default, TableFlo uses **SQLite** for portability. To use SQL Server:

1. Update `App.xaml.cs`:
   ```csharp
   services.AddDbContext<TableFloDbContext>(options =>
       options.UseSqlServer("YourConnectionString"));
   ```

2. Add the SQL Server NuGet package:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```

### Customization

- **Push Intervals** - Modify `PushIntervalMinutes` per table type
- **Break Rules** - Configure `BreakIntervalMinutes` and `MealDeadlineHours` in shifts
- **Fairness Weights** - Adjust scoring algorithm in `SchedulingService.cs`

---

## 📈 Future Enhancements

Planned features for future releases:

- [ ] Multi-casino support with network sync
- [ ] Mobile companion app for floor supervisors
- [ ] Predictive analytics for staffing needs
- [ ] Integration with casino management systems (CMS)
- [ ] Advanced reporting with charts and graphs
- [ ] Dealer performance tracking and reviews
- [ ] Tip pooling and distribution management
- [ ] Real-time chat for floor communication
- [ ] Automated shift scheduling (beyond daily rotations)

---

## 💼 Business Value

### For Casinos

- **Time Savings** - Hours of manual scheduling reduced to seconds
- **Compliance** - Eliminate labor law violations
- **Fairness** - Remove bias and favoritism accusations
- **Efficiency** - Optimize dealer flow for maximum coverage
- **Insights** - Data-driven staffing decisions

### For Dealers

- **Fair Treatment** - Equal distribution of desirable tables
- **Predictability** - Know your rotation schedule
- **Compliance** - Guaranteed breaks and meals
- **Transparency** - Clear view of assignments

### For Supervisors

- **Automation** - AI handles the heavy lifting
- **Flexibility** - Manual override always available
- **Accountability** - Complete audit trail
- **Intelligence** - AI recommendations for optimal decisions

---

## 📄 License

This is a proprietary application developed for the casino industry.

---

## 👥 Contact

**TableFlo**  
Stateline, Nevada

For inquiries about licensing or customization, please contact your TableFlo representative.

---

## 🙏 Acknowledgments

Built with love for the casino industry. Special thanks to the floor supervisors and dealers who provided invaluable feedback during development.

---

**TableFlo** - Elevate Your Casino Operations™

