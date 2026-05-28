# NoCrastinator

NoCrastinator is a lightweight goal‑tracking application designed specifically for demonstrating real‑world QA processes.  
It includes intentional, controlled defects to help testers practice exploratory testing, boundary analysis, defect reporting, and API validation in a realistic environment.

---

## 🚀 Purpose

This project exists to support hands‑on QA learning by providing:

- A simple, easy‑to-understand domain (goal tracking)
- A real API surface to explore and test
- Predictable behaviours mixed with seeded bugs
- A safe environment for practicing defect discovery and reporting
- A consistent target for automation exercises

It is ideal for:
- Junior QA onboarding  
- QA bootcamps


## 🚀 Local Setup (Phase 0)

## Clone repo

```bash
git clone [https://github.com/your-org/nocrastinator](https://github.com/maxdatasolqa/maxdata.gap2it.nocrastinator.github)
cd nocrastinator
run script /domain/schema.sql
modify connection string in appsettings.Development.json
example:
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=NoCrastinatorDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
dotnet run --project NoCrastinator.Api
login with:
email: abc
password: aBC123.
