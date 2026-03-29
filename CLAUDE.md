# Project Rules
- Always read README first (currently a template; rely on solution/project files instead).
- Prefer small, testable changes. Ask before large refactors.
- Run tests (or explain if none) before final answer.
- If unsure about requirements or architecture, ask.

# Repository Overview
- Solution: Fsel.Src.sln
- Shared build config: Directory.Build.props, Directory.Packages.props, .editorconfig
- Services live under /Services/*
- Tests under /Tests (currently: Tests/Fsel.Course/Fsel.Course.Lms.Application.Test)
- Packages includes IdentityServer forks under /Packages/IdentityServer/*

# Common Commands
- dotnet --info
- dotnet restore Fsel.Src.sln
- dotnet build Fsel.Src.sln
- dotnet test Tests/Fsel.Course/Fsel.Course.Lms.Application.Test/Fsel.Course.Lms.Application.Test.csproj

# C# Conventions
- Use async/await for IO, avoid .Result/.Wait()
- Prefer records for DTOs, immutable where possible
- Keep DI registrations in Infrastructure or Api projects
- Avoid breaking public APIs without notice

# Workflow
1) Explore relevant files
2) Propose plan
3) Implement
4) Test
5) Report results + risks

# Local Skills
- skills/dotnet-design-patterns (use for pattern selection + structure)
