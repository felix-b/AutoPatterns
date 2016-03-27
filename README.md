# MetaPatterns
A Roslyn-based meta-programming library, which generates implementations of design patterns and abstractions through pipelines of reusable conventions

## Highlights: why another meta-programming library
- *Simple to learn and fun to use*. Code generators (or more precisely, reusable conventions) are basically templates of generated code written in T4-like concept. And yet (as opposed to T4 templates), they are just regular classes written in C# and compiled as part of your project, so that you can use coding tools you normally do, e.g. code navigation, IntelliSense, refactorigns.
- *Enables a whole new set of modularity and customization scenarios*. Code generation takes place at run-time, not at design- or compile-time. Which means, the code being generated depends on a set of modules configured to load in a specific deployment. This allows aspect-oriented composition of applications by sharing binaries, rather than by sharing code. Which for example, eliminates the need to manage multiple branches of customized codebases - just manage multiple customized deployments.  
- *Maintainable*. References to types and members by code generators are type-safe (minimize the chances to generate invalid programs), and refactor-safe (e.g. when a member or a type is renamed, those reference are subject to rename refactoring in Visual Studio). The goal here is to prevent compilation of code generators that produce invalid programs, as much as possible.
- *Powerful*. 

