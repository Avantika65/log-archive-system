# Log Archive System

## Features
- Background service monitoring multiple folders
- Real-time file detection using FileSystemWatcher
- Duplicate prevention using SHA256 hashing
- SQLite persistence
- ASP.NET Core API
- Frontend log viewer
- Supports logs, PDFs, images, text files, etc.

## Tech Stack
- C#
- .NET
- ASP.NET Core
- SQLite
- HTML/CSS/JS

## Architecture
Worker Service → Archive → API → Frontend
- myCSharpApp/
    Background worker service
- LogViewerApi/
    ASP.NET Core Web API
- frontend/
    HTML/CSS/JS frontend viewer

## How to Run
### 1. Clone Repository
git clone https://github.com/Avantika65/log-archive-system.git

### 2. Start Background Worker
- cd myCSharpApp
- dotnet run

### 3. Start API
- Open another terminal:
- cd LogViewerApi
- dotnet run
- API runs on: http://localhost:5262

### 4. Start Frontend
- Open another terminal:
- cd frontend
- python3 -m http.server 5500
- Frontend runs on: http://localhost:5500

### 5. Test File Monitoring
- Add files manually to monitored source folders or 
- run **echo "test log" > source1/test.log**

The worker will automatically:
- detect file
- validate it
- archive it
- expose it through API/UI
