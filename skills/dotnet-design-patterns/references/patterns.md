# Pattern Quick Reference (C#/.NET)

## Factory / Abstract Factory
Use when: create objects without coupling to concrete types.
Typical files: Factories/, Interfaces/, Implementations/.

## Strategy
Use when: multiple algorithms interchangeable at runtime.
Typical files: Strategies/, Interfaces/, DI registration.

## Decorator
Use when: extend behavior without modifying core class.
Typical files: Decorators/, Base interface, registration order.

## Adapter
Use when: wrap incompatible interfaces.

## Repository
Use when: abstract data access from domain logic.

## CQRS
Use when: reads and writes have different models/constraints.
Files: Commands/, Queries/, Handlers/.

## Mediator
Use when: decouple senders/receivers; often via MediatR.

## Unit of Work
Use when: coordinate multiple repositories in a single transaction.

## Observer / Pub-Sub
Use when: event-driven updates.

## Builder
Use when: complex object construction.

## Singleton (with care)
Use when: single shared instance is required.

## State
Use when: object behavior changes based on internal state.

## Chain of Responsibility
Use when: multiple handlers can process a request.
