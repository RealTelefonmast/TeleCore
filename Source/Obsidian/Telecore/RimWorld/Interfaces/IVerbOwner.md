---
Namespace: Verse
tags:
  - Interface
---

IVerbOwner is an interface that declares a common set of properties and methods for handling game verbs.

#### Properties  

```cs
	VerbTracker VerbTracker { get; }

	List<VerbProperties> VerbProperties { get; }

	List<Tool> Tools { get; }

	ImplementOwnerTypeDef ImplementOwnerTypeDef { get; }

	Thing ConstantCaster { get; }
```

## Methods

```cs
	string UniqueVerbOwnerID();
	bool VerbsStillUsableBy(Pawn p);
```

## References

- [[Tool]]
- [[VerbTracker]]
- [[VerbProperties]]
- [[ImplementOwnerTypeDef]]
- [[Thing]]
