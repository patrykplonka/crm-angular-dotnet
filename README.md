# 📚 Tutoring Management System

A full-stack **Tutoring Management System** built with **Angular**, **ASP.NET Core 9 Web API**, and **Microsoft SQL Server**.  
This application allows students to book lessons, chat in real-time with tutors, authenticate users securely, and rate tutors.  
It also includes an admin panel for managing users, lessons, and platform settings.

---

## ✨ Features

- 📅 Lesson booking system (students ↔ tutors)  
- 💬 Real-time chat between users using SignalR  
- 🔐 User authentication and authorization (JWT)  
- ⭐ Tutor rating and reviews  
- 🛠️ Admin panel for platform management  
- 📊 Dashboard with key statistics and logs  

---

## 🧱 Tech Stack

### 🔹 Frontend (Angular)

- Angular 15+
- RxJS
- Angular Material
- JWT Auth Interceptors
- ngx-toastr, ngx-spinner

### 🔹 Backend (ASP.NET Core 9)

- ASP.NET Core 9 Web API
- Entity Framework Core
- Microsoft SQL Server
- SignalR for real-time communication
- AutoMapper & FluentValidation
- ASP.NET Identity (JWT-based)

---

## 🚀 Getting Started

### ✅ Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Node.js + npm](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)

---

### 📥 Clone the Repository

```bash
git clone https://github.com/patrykplonka/crm-angular-dotnet
cd tutoring-management-system
```

🔧 Backend Setup
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```
🎨 Frontend Setup
```bash
cd frontend
npm install
ng serve
```
