# 💼 RemoteJobBoard - Full-Stack Remote Job Platform

RemoteJobBoard is a premium, end-to-end full-stack web application designed to connect employers with remote job seekers. Built with a decoupled architecture utilizing **ASP.NET Core 10 Web API** on the backend and **Next.js (v15+ App Router)** on the frontend, featuring full **Role-Based Access Control (RBAC)** and **CRUD** functionalities.

---

## 🚀 Live Demo & Links
- **Frontend Live Website:** [https://remote-job-board-pi.vercel.app/](https://your-frontend.vercel.app) 
- **Backend API Endpoint:** [https://your-backend.onrender.com](https://your-backend.onrender.com) 

---

## 🛠️ Tech Stack & Architecture

### Frontend (Client-Side)
- **Framework:** Next.js 15+ (App Router, TypeScript)
- **Styling:** Tailwind CSS (With Dark Mode support)
- **State & Fetching:** Axios, React Hooks (`useState`, `useEffect`)
- **Optimization:** Dynamic Imports (`next/dynamic` to resolve SSR Hydration mismatches)

### Backend (Server-Side)
- **Framework:** ASP.NET Core 10 Web API
- **Database:** PostgreSQL (Relational Database)
- **ORM:** Entity Framework Core (EF Core)
- **Security:** JSON Web Tokens (JWT) Authentication & Claims-Based Authorization

---

## 🌟 Key Features

### 👤 Role-Based Authentication
- **Secure Register & Login** with hashed passwords and JWT Token generation.
- Dynamic navigation bar and separate dashboards automatically loaded based on user claims (`Employer` vs `JobSeeker`).

### 🚀 Employer Dashboard (Full CRUD)
- **Post Jobs:** Create new remote positions with Titles, Category, Salary Range, and Requirements.
- **Manage Openings:** Real-time **Edit** (Update) and **Delete** control over posted jobs.
- **Applicant Tracking System (ATS):** View candidates who applied, inspect their resumes via secure URLs, and change application status (`Pending`, `Interviewing`, `Hired`, `Rejected`).

### 💼 Job Seeker Dashboard
- **Job Feed & Filtering:** Live search by keywords and filter by categories (Remote, Hybrid, Full-Time).
- **One-Click Application:** Apply with Google Drive/Dropbox Resume URLs and a custom Cover Letter.
- **Application Tracking:** A dedicated dashboard containing a live data table tracking all submitted applications and their real-time evaluation status.
