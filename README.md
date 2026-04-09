# Task Follow-Up Management System

A complete web-based **Task Follow-Up Management System** built with **ASP.NET Core MVC** and **SQL Server**. This system enables managers to assign tasks to employees, define deadlines, monitor progress, perform follow-ups, and review responses. Employees can view assigned tasks, update progress, respond to follow-ups, and mark tasks as completed.

## Features

### Authentication & User Management
- ASP.NET Core Identity with role-based access control
- Three roles: **Admin**, **Manager**, **Employee**
- Admin can create users and assign roles

### Task Management
- Managers: Create, edit, delete, and assign tasks to employees
- Employees: View assigned tasks, update status, add progress notes
- Filter tasks by employee, priority, status, and due date
- Pagination and search support

### Follow-Up Management
- Managers: Add follow-up notes for any task
- Employees: Respond to follow-up notes
- Full follow-up history tracking

### Notification System
- In-app notifications for task assignments, follow-ups, deadlines, and completions
- Notification badge with real-time count polling
- Mark as read / mark all as read

### Dashboards & Reports
- **Admin Dashboard**: User and task statistics
- **Manager Dashboard**: Task metrics, employee performance, recent follow-ups
- **Employee Dashboard**: Active tasks, pending follow-ups, upcoming deadlines
- **Reports Page**: Task distribution charts and employee performance metrics

### Task Status Workflow
`New` → `Assigned` → `InProgress` → `PendingReview` → `Completed` → `Closed`
- Automatic `Overdue` detection when due date passes

### Priority Levels
- Low, Medium, High, Critical (with color-coded badges)

## Tech Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Language**: C#
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **UI**: Bootstrap 5 + Bootstrap Icons
- **Architecture**: MVC with Services layer

## Project Structure

```
TaskFollowUpManagementSystem/
├── Controllers/          # MVC Controllers
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── EmployeeController.cs
│   ├── HomeController.cs
│   ├── ManagerController.cs
│   └── NotificationController.cs
├── Data/                 # Database context and seed data
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Models/               # Entity models and enums
│   ├── Enums/
│   ├── ApplicationUser.cs
│   ├── TaskItem.cs
│   ├── FollowUp.cs
│   └── Notification.cs
├── Services/             # Business logic services
│   ├── TaskService.cs
│   ├── FollowUpService.cs
│   └── NotificationService.cs
├── ViewModels/           # View-specific data models
├── Views/                # Razor views organized by controller
│   ├── Account/
│   ├── Admin/
│   ├── Employee/
│   ├── Manager/
│   ├── Notification/
│   └── Shared/
└── wwwroot/              # Static files (CSS, JS)
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/Abdul1996-7/TaskFollowUpManagementSystem.git
cd TaskFollowUpManagementSystem/TaskFollowUpManagementSystem
```

### 2. Configure Connection String

Update the connection string in `appsettings.json` to point to your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskFollowUpManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

For SQL Server Express:
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=TaskFollowUpManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### 3. Apply Migrations

```bash
dotnet ef database update
```

Or the application will automatically apply migrations on startup.

### 4. Run the Application

```bash
dotnet run
```

Navigate to `http://localhost:5000` in your browser.

### 5. Login with Seed Accounts

The application seeds the following demo accounts on first run:

| Role     | Email                        | Password      |
|----------|------------------------------|---------------|
| Admin    | admin@taskmanager.com        | Admin@123     |
| Manager  | manager@taskmanager.com      | Manager@123   |
| Employee | employee1@taskmanager.com    | Employee@123  |
| Employee | employee2@taskmanager.com    | Employee@123  |

## Database Design

### ApplicationUser
Extends ASP.NET Core Identity with: FullName, Department, CreatedAt

### TaskItem
TaskId, Title, Description, AssignedById, AssignedToId, StartDate, DueDate, ExpectedDuration, Priority, Status, ProgressNote, CreatedAt, UpdatedAt

### FollowUp
FollowUpId, TaskItemId, ManagerId, EmployeeId, FollowUpDate, Note, EmployeeResponse, ResponseDate, CreatedAt

### Notification
NotificationId, UserId, TaskItemId, Message, Type, IsRead, CreatedAt

## Business Rules

- Only Managers can create and assign tasks
- Only the assigned Employee can update task status
- Only Managers can add follow-ups
- Employees can only respond to follow-ups for their tasks
- Overdue status is automatically set when DueDate passes and task is not completed
- All actions are tracked with timestamps

## UI Features

- Responsive Bootstrap 5 layout
- Role-based navigation
- Color-coded priority and status badges
- Dashboard cards with metrics
- Search and filter on task lists
- Pagination for tables
- Real-time notification badge
- Professional admin panel styling

## License

This project is for educational and demonstration purposes.
