---
name: dotnet-design-patterns
version: 0.1.0
description: Assist with selecting and applying C#/.NET design patterns with practical, testable implementations.
---

# When to use
Use this skill when:
- Designing new features or refactoring existing code in C#/.NET
- You need to select a design pattern (Factory/Strategy/Decorator/CQRS, etc.)
- You want a consistent, testable pattern implementation

# Inputs
- Feature description or problem statement
- Current code location (project + folder)
- Constraints (performance, testability, existing architecture)

# Outputs
- Recommended pattern + rationale
- File/class changes (where to place each piece)
- Example skeletons with TODOs
- Test strategy for the change

# Workflow
1) Identify the core problem and constraints.
2) Map to pattern candidates (use references/patterns.md).
3) Choose 1 pattern and justify.
4) Sketch C# class layout and DI registrations.
5) Add tests or suggest minimal tests.

# Examples
- "We need to support multiple payment providers" → Strategy + Factory
- "We need to add behavior without modifying existing class" → Decorator
- "We need to separate writes and reads" → CQRS

# Guardrails
- Do not over-engineer: prefer simplest pattern that solves the problem.
- Keep public APIs backward compatible unless instructed.
- Ensure naming aligns with solution conventions.
