# AutoPatterns
A Roslyn-based meta-programming library, which automatically generates implementations of design patterns and abstractions through pipelines of reusable templates.

## Why another meta-programming library

### Powerful
**AutoPatterns** comes equipped with happy paths for the following use cases:
  - Implement a well-known abstraction (e.g. IComparable, IFormattable). Implement/override known members. 
  - Implement an abstraction which is not known in advance (e.g. ICustomerEntity, IInventoryService). Implement/override discovered members, be prepared for any types and signatures.
  - Declare and implement any member, not limited to those declared by interfaces or a base type.
  - Decorate existing virtual members, and members implemented earlier in the pipeline.
  - Apply code fragments per list of discovered members. Ability to conditionally select members and fragments.
  - Compose a class type out of multiple implemented abstractions
  - Compose a class type out of multiple classes -- known as **mix-ins**. Allow injection of mix-ins.
  - Provide delegates to factory methods for instantiation of generated types. Ability to have multiple constructors per type.

### Easy to learn and fun to use
With **AutoPatterns**, code templates are written in T4-like WYSIWYG concept. Yet, they differ from T4 templates, in that:
- They are regular classes written in C#, just like any other class in your project. You can use coding tools you normally do, e.g. code navigation, IntelliSense, refactorings.
- They don't have to be monolithic. Each template in a pipeline can (and should) take responsibility of a specific aspect, adding up to the complete implementation. Different templates can implement distinct sets of members, as well as decorate or transform implementation code accumulated by far. 
- While each pipeline is aimed to produce a specific kind of objects (e.g. data entities or configuration sections), the templates are the building blocks which can be reused among the different pipelines.

### Opens up modularity and customization scenarios

With **AutoPatterns**, code generation takes place at run-time, not at design- or compile-time. This opens up interesting possibilities:
- The code being generated may depend on -- for example, be extended by -- a set of modules configured to load in a specific deployment. 
- Pluggable aspect-oriented composition of application can be achieved by sharing binaries, rather than by sharing code. 
- The need to manage multiple branches of customized codebases can be eliminated. All extension/customization modules can reside in the main codebase, and only multiple deployment configurations must be managed.  

And also:
- Horizontal type composition through mix-ins enables subject-oriented programming in your designs.

### Allows creating safe and maintainable templates

- References from template code to types and members are type-safe and refactor-safe, in order to minimize chances of breaking the templates and producing invalid programs.
  - For example, when a member or a type is renamed, affected templates are updated by rename refactoring in Visual Studio, just like the rest of the code in your solution.
- The goal is, as much as possible, to prevent a template from successfully compiling, if it would produce invalid program.

