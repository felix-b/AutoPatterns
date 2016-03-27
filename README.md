# MetaPatterns
A Roslyn-based meta-programming library, which generates implementations of design patterns and abstractions through pipelines of reusable conventions

## Highlights: why another meta-programming library
- The code is generated at run-time, not at compile-time. This allows aspect-oriented customization of applications by sharing binaries, rather than by sharing code. Which in turn eliminates the need to manage multiple branches of codebase - just manage multiple deployments. 
- Maintainability of code generators. All references to types and members by code generators are type-safe (minimize the chances to generate invalid programs), and refactor-safe (e.g. when a member or a type is renamed, those reference are subject to rename refactoring in Visual Studio).  
