# Pattern Selection Checklist

- What is the change? (extending behavior, new provider, new workflow)
- Are we avoiding breaking public API?
- Do we need runtime switching? → Strategy
- Do we need object creation isolation? → Factory
- Do we need behavior extension w/o modification? → Decorator
- Do we need separate read/write models? → CQRS
- What tests will validate the behavior?
