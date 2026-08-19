# Architectural Document: Opt-In Write Eligibility for Get-Only Properties

**Status:** implemented (PR #7, pending review) · **Repo:** `Pooshit.Json` · **Base:** `master` @ `b1580b1` · **Author:** Sarah (architect) · **Date:** 2026-08-19
**Drivers:** DiVoid **#8522** (severity-1 regression) · DiVoid **#8444** (original request)
**Standards applied:** Design Contracts **#1136** (esp. §1 KISS/DRY/YAGNI, §5 Pre-Design Checklist — audited in §15 below) · Code Contracts **#114 §0**
**Concept nodes this changes:** **#3340** (serialize flow / write-eligibility) · **#3342** (model resolution & property discovery). **#3341** (read flow) is *unchanged* — see §9.

---

## 1. Problem Statement

`Pooshit.Json 0.5.0-preview.1` changed the writer's eligibility rule from *"readable **and** settable"* to *"readable"*. That correctly resolved #8444 (computed get-only properties never reached the wire), but it made the writer descend into **any** readable object graph — including `System.Exception`, `System.Type` and `System.Reflection.MethodBase`, which are almost entirely get-only and **self-referential** (`Type.UnderlyingSystemType` returns `this`). The walk does not terminate, producing a `StackOverflowException` — which .NET cannot catch and which **terminates the process**. It is reachable through `Pooshit.AspNetCore`'s `ErrorHandlerMiddleware`, so under 0.5.0-preview.1 *any error response crashes the service* instead of returning a 500.

The owner's resolution direction is to **withdraw** the approach rather than patch it.

> *"stay tuned - i send json for a redesign. undo last changes and introduce an attribute for properties to write is my best idea now."*
> — Toni, 2026-08-19 (#8522)

> *"The last change introduced a larger problem - writing get only on default writes out a lot of system data (exceptions and such) by default and we can not control that. […] Basic idea now is to undo our changes to reintroduce behavior like before and then introduce an attribute a consumer can use to mark properties which are to be written despite having only a getter."*
> — Toni, 2026-08-19 (task opening)

**Goal.** Replace *"emit every readable property by default"* with *"emit a property that has no public setter **only** when the consumer opts in via an attribute"*.

**Success criteria.**

| # | Criterion | Verifiable by |
|---|---|---|
| S1 | `Json.WriteString(new Exception("x"))` terminates and produces finite output. | Test on all three writer routines. |
| S2 | A get-only property carrying the new attribute is emitted on **all** writer paths, with and without `[ReflectType]`. | Test matrix, 6 cells. |
| S3 | A get-only property **without** the attribute is omitted, as before 0.5.0-preview.1. | Test matrix. |
| S4 | The reflective and source-generated model paths agree on every property shape in the matrix. | Test matrix run twice per shape. |
| S5 | `Json.Read<T>` does not throw on a `[ReflectType]` type carrying a get-only or non-public-set property. | Existing reader tests stay green (crash fix retained — §6.2). |

---

## 2. Scope & Non-Scope

**In scope**

- One new public attribute type (the assembly's first).
- The write-eligibility gate on all four `JsonWriter` / `JsonStreamWriter` object branches.
- Test-suite realignment for the shapes whose expected output inverts.
- Documentation reconciliation for the DiVoid concept nodes and the reopened trap ticket.

**Explicitly out of scope**

| Item | Why | Tracked as |
|---|---|---|
| Routing `JsonStreamWriter` through the model layer | Pre-existing structural defect, independent of this change. This design honours the attribute on that path *without* fixing the bypass — §7.3. | **#8466** |
| Cycle detection / depth cap in the writer | Dissolved by opt-in, not merely mitigated — §11. | — (open question §16.3) |
| `{ get; init; }` remaining wire-writable on the reflection path | Pre-existing, unchanged by this design. | **#8452** |
| `[ReflectType]` + `init` failing to compile (CS8852) | `Pooshit.Reflection` defect. | **#8453** |
| `ReflectionModel.GetProperty` operator-precedence half (indexer guard misses the `DataMember` branch) | Pre-existing, untouched. | **#3352 §2** |
| Dictionary-key naming divergence between the writers | Unrelated. | **#8467** |
| Making anonymous types serializable again | Accepted consequence — §10. Priced and rejected. | **#3348** (reopens) |
| Package version / delisting `0.5.0-preview.1` | Operator/packaging decision. | Open question §16.1 |

---

## 3. Assumptions & Constraints

| # | Assumption / constraint | Confidence | How established |
|---|---|---|---|
| A1 | Arbitrary consumer-defined attributes survive into `Pooshit.Reflection` source-generated metadata and are readable at runtime via `IPropertyInfo.Attributes`. | **Measured** | §4. This was the design's single blocking risk; it does **not** materialise. |
| A2 | The generator lists get-only and non-public-set properties (with a `null` setter delegate) rather than dropping them, so an opt-in gate can still see them. | **Measured** | §4, and #3342. |
| A3 | The generator does **not** carry an attribute declared on a base property down to an `override`. | **Measured** | §4. Closed by design decision D3. |
| A4 | No writer path walks *fields*; both walk properties only (`IModel.Properties`, `Type.GetProperties()`). | **Measured** | Source read of both writers. Determines D2. |
| A5 | `Pooshit.Reflection` 0.1.11-preview is a fixed external dependency; this design requires **no** change to it. | **Measured** | Consequence of A1. |
| A6 | The library targets `netstandard2.0` and `netstandard2.1`; the attribute must use only constructs valid on both. | Read from `Pooshit.Json.csproj` | A plain parameterless `Attribute` subclass qualifies trivially. |
| A7 | `0.5.0-preview.1` is published and in the wild but adopted by no consumer (mamgo-backend held its upgrade at 0.4.0-preview.1 per #8522). | High | #8522 "Upgrade held. PR #816 stays on 0.4.0-preview.1". |

---

## 4. Measurement Report — the cross-repo constraint, resolved

The brief flagged decision (b) as a potential blocking cross-repo dependency: *if the source generator does not surface consumer attributes, the attribute silently does nothing under `[ReflectType]`.* This was measured, not assumed, using a throwaway probe project referencing `Pooshit.Reflection 0.1.11-preview` (the exact version `Pooshit.Json` consumes) and inspecting both the emitted generator output and the runtime `IPropertyInfo.Attributes` values.

### Result 1 — consumer attributes DO flow. The blocker does not exist.

The generator emits attribute instances into the property metadata verbatim, for **custom** attribute types, including constructor arguments and named-property initialisers, on get-only and `private set` properties alike. Runtime readback through `IPropertyInfo.Attributes` returns them.

| Probe shape | Emitted into generated metadata | Visible at runtime |
|---|---|---|
| Custom parameterless attribute on a get-only property | yes | yes |
| Custom attribute with a constructor argument | yes | yes |
| Custom attribute with a named-property initialiser | yes | yes |
| Custom attribute on `{ get; private set; }` | yes | yes |
| Unattributed property | empty array | empty array |

**Consequence:** no `Pooshit.Reflection` change is required, and the attribute is honoured identically on both model paths. A1, A2, A5 confirmed.

### Result 2 — one genuine path divergence, on `override` only

| Case | Source-gen path | Reflection path (attribute `Inherited = true`) | Agree? |
|---|---|---|---|
| Attribute on the property itself | sees it | sees it | ✅ |
| Attribute on a **non-overridden** inherited base property | sees it | sees it | ✅ |
| Attribute declared on a base property, derived property `override`s it | **blind** | sees it | ❌ **split** |

This is the same *class* of defect as #8444's "the two paths disagree about the same type", and it would be inherited by any new attribute. It is **not** introduced by this design — `[DataMember]` and `[IgnoreDataMember]` already behave this way on overridden properties — but shipping a new attribute into it knowingly would be a defect.

### Result 3 — `Inherited = false` closes the split at zero cost

Re-measured with `[AttributeUsage(..., Inherited = false)]` on the probe attribute:

| Case | Source-gen | Reflection (`Inherited = false`) | Agree? |
|---|---|---|---|
| Attribute on the property itself | sees it | sees it | ✅ |
| Non-overridden inherited base property | sees it | sees it | ✅ |
| Base declaration + `override` | blind | **blind** | ✅ **agree** |

One declaration keyword makes the two model paths provably agree on every measured shape. This is design decision **D3**.

---

## 5. Architectural Overview

The change is confined to a **single predicate** — write eligibility — plus **one new public type**. There is no new layer, no new service, no new abstraction, and no new configuration.

```
                      ┌───────────────────────────────────────────┐
                      │  NEW:  Pooshit.Json.JsonWriteAttribute    │
                      │        (public, parameterless, Property,  │
                      │         Inherited = false)                │
                      └───────────────┬───────────────────────────┘
                                      │ read as property metadata
              ┌───────────────────────┴───────────────────────┐
              │                                               │
   ┌──────────▼───────────┐                        ┌──────────▼──────────┐
   │  Model layer         │                        │  raw System.        │
   │  IModel/IPropertyInfo│                        │  Reflection         │
   │  .Attributes         │                        │  PropertyInfo       │
   └──────────┬───────────┘                        └──────────┬──────────┘
              │                                               │
   ┌──────────▼───────────┐                        ┌──────────▼──────────┐
   │ JsonWriter  sync ─┐  │                        │ JsonStreamWriter    │
   │ JsonWriter  async─┼──┤  WRITE-ELIGIBILITY     │   sync  ─┐          │
   └───────────────────┘  │  PREDICATE (§6.1)      │   async ─┴──────────│
                          │  applied at 4 sites    │                     │
                          └────────────────────────┴─────────────────────┘
                                      │
                              ┌───────▼────────┐
                              │ JsonReader     │  UNCHANGED (§9)
                              └────────────────┘
```

**The one-sentence design:** restore the pre-0.5.0 requirement that a written property be settable, define "settable" as *publicly* settable so that both model paths agree, and let `[JsonWrite]` waive that requirement for a property the consumer explicitly nominates.

---

## 6. Components & Responsibilities

### 6.1 `JsonWriteAttribute` — the opt-in marker (NEW)

| Aspect | Decision | Rationale |
|---|---|---|
| **Type name** | `JsonWriteAttribute`, used as `[JsonWrite]` | See D1 below. |
| **Namespace** | `Pooshit.Json` | The library's root namespace, alongside `JsonOptions`, `ByteArrayBehavior`, `NamingStrategies`. A new `Pooshit.Json.Attributes` sub-namespace for one type is an unearned folder (#1136 §4 *can-it-be-merged*). |
| **File** | `Pooshit.Json/JsonWriteAttribute.cs` | One type per file (#114 §1), sibling of `JsonOptions.cs`. |
| **Targets** | `AttributeTargets.Property` **only** | See D2 below. |
| **Inherited** | `false` (explicit) | See D3 below — the measured path-agreement fix. |
| **AllowMultiple** | `false` (the default; do not state it) | Marking a property twice is meaningless. |
| **Parameters** | **none** — no constructor arguments, no properties | See D4 below. |
| **Owns** | The *declaration* that a property is nominated for output. | |
| **Does NOT own** | Naming (that is `[DataMember(Name=…)]` / the naming strategy), exclusion (that is `[IgnoreDataMember]`), read behaviour (§9), or anything on the reader path. | |

**D1 — why `[JsonWrite]` and not `[JsonInclude]`.**
`Write` is the library's own verb for serialization throughout its public surface — `Json.WriteString`, `Json.Write`, `JsonWriter`, `IJsonWriter`, `JsonStreamWriter`. Naming the attribute after that verb keeps one vocabulary (#1136 §1 DRY at the vocabulary level) and matches the owner's own phrasing (*"an attribute for properties to write"*). `[JsonInclude]` was considered and rejected: `System.Text.Json.Serialization.JsonIncludeAttribute` already exists with a *different* meaning (include non-public members), so a consumer file importing both namespaces gets either a compile ambiguity or — worse — a silent mental model mismatch. #8444's phrase *"a `[JsonInclude]`-equivalent"* was descriptive of the concept, not a naming instruction.

**D2 — why `AttributeTargets.Property` and not `Property | Field`.**
Measured (A4): **no writer path walks fields.** `JsonWriter` iterates `IModel.Properties`; `JsonStreamWriter` iterates `Type.GetProperties()`. Permitting `AttributeTargets.Field` would let a consumer write `[JsonWrite]` on a field, compile cleanly, and get *nothing* — a silent no-op, which is precisely the failure mode #8444 was filed about. The attribute must not accept a target it cannot honour. Note `AttributeTargets.Property` also covers **indexers** in C#; §7.4 handles that.

**D3 — why `Inherited = false`.**
Measured in §4, Results 2 and 3. Without it, an attribute declared on a base property and overridden in a derived class is honoured on the reflection path and ignored on the source-gen path — a behaviour split keyed on the presence of `[ReflectType]`, which is the exact defect shape #8444 exists to eliminate. With it, both paths ignore the base declaration and agree. The consumer-visible rule becomes simple and statable: **the attribute applies to the declaration it is written on.** A consumer who overrides a nominated property re-declares the attribute on the override. This is documented in §12, not left to discovery.

**D4 — why no parameters.**
YAGNI (#1136 §1, §3). There is no value the feature needs to carry: the attribute is a boolean nomination and its presence *is* the value. A `bool Write = true` property, a `Name` property (that is `[DataMember]`'s job), or a `Condition` enum would each be a knob with no named operator and no environment variance (#1136 §3). Secondary benefit, measured: a parameterless public constructor is the simplest shape for the source generator to re-emit (it emits `new global::Pooshit.Json.JsonWriteAttribute()`), removing any generation-time risk class entirely.

### 6.2 Write-eligibility predicate — the changed rule

**New rule, stated once, authoritative:**

> A property is emitted **iff** it is **readable**, **not** marked `[IgnoreDataMember]`, **not** an indexer, **and** (it has a **public setter** **or** it carries `[JsonWrite]`).

Compare:

| Version | Rule |
|---|---|
| 0.4.0-preview.1 (before) | readable *and* settable — where "settable" meant `CanWrite` on the reflection path and *has-a-setter-delegate* on the source-gen path (**these disagreed** — #8444) |
| 0.5.0-preview.1 (withdrawn) | readable — *the regression* |
| **This design** | readable **and** (publicly settable **or** `[JsonWrite]`) — the same meaning of "settable" on both paths |

Three clauses are load-bearing and must not be simplified away:

1. **`readable` stays an unconditional precondition.** `[JsonWrite]` waives the *setter* requirement only. It must **not** force emission of a set-only property: on the reflection path `PropertyInfo.GetValue` on a write-only property throws, and on the source-gen path the getter delegate is `null`. A `[JsonWrite]` set-only property is a consumer error and is silently skipped, exactly as an unattributed one is today.
2. **`[IgnoreDataMember]` beats `[JsonWrite]`.** A property carrying both is contradictory; the more restrictive wins, and this falls out naturally if the ignore check is evaluated first. State it in the XML doc; do not add a diagnostic (#1136 §6 *defensive code for impossible scenarios* — a contradictory pair is a consumer typo, not a system failure mode).
3. **"Publicly settable" is the *same* predicate the reader already uses** — `HasSetter`. See §6.3 and D6; this is what makes S4 achievable.

### 6.3 `ReflectionProperty.HasSetter` — kept as `SetMethod?.IsPublic == true` (**D6**)

The brief invited a re-argument, noting the shipped definition is a published breaking change whose justification *"weakens under opt-in"*. **I disagree with that framing, and recommend keeping it.** Three arguments, in ascending order of force:

1. **It was ruled on its own merits, not as a consequence of emit-by-default.** Per #8464, CF-2 was *"escalated to Toni as a genuine compatibility decision on a published library; he chose refuse non-public setters."* Reverting it would reverse an owner ruling that this redesign has no new information against.
2. **The tamperability argument does not actually weaken.** It never depended on the writer. `JsonReader` assigns whatever the *wire* names, regardless of whether the writer would have emitted it. Making the writer opt-in protects nothing on the read side. So the reason to refuse non-public setters on read — *a shape that appears immutable while being wire-tamperable is a trap* (#8444) — is exactly as strong under opt-in as it was under emit-by-default.
3. **Decisive, and new to this design: under the §6.2 rule, `HasSetter` becomes load-bearing on the *write* side too, and reverting it re-splits both sides.** Because the source-gen path's metadata can only express *"has a public setter"* (a non-public setter yields a `null` delegate — measured, #3342), that is the **only** definition of "settable" the two paths can both express. Revert `HasSetter` to `CanWrite` and the reflection path would emit `{ get; private set; }` while the source-gen path omitted it — reinstating verbatim the #8444 row *"`{ get; private set; }` emits without `[ReflectType]` and is omitted with it"*, which #8444 names as a minimum resolution bar. Reverting would un-fix the ticket this redesign must still answer.

**Verdict: keep `SetMethod?.IsPublic == true`.** It is now the single predicate that makes read *and* write agree across both model paths.

**Honest cost of D6, which must be stated in the PR body.** There is **no single "before"** for `{ get; private set; }` — 0.4.0-preview.1 emitted it on the reflection path and omitted it on the source-gen path. "Reintroduce behavior like before" is therefore under-determined for this shape and the design must pick one. It picks *omit unless opted in*, because that is the branch where the two paths agree and where the write rule matches the read rule. **Reflection-path consumers with `{ get; private set; }` properties will see those keys disappear** relative to 0.4.0. The remedy is discoverable and one token long: add `[JsonWrite]`.

---

## 7. Interactions & Data Flow

All flows are synchronous, in-process, single-assembly. There is no protocol, no message, no network hop. What follows is the property-level decision sequence at each writer site.

### 7.1 `JsonWriter` — sync and async object branches (2 sites)

1. Resolve the model once per object: `Model.GetModel(data.GetType())` — unchanged.
2. For each `IPropertyInfo` in `model.Properties`:
   a. skip if `!HasGetter`;
   b. skip if `Attributes` contains `IgnoreDataMemberAttribute`;
   c. **NEW —** skip if `!HasSetter` **and** `Attributes` contains no `JsonWriteAttribute`;
   d. read the value, apply `ExcludeNullProperties`, resolve the key via `DataMemberAttribute.Name` ?? naming strategy, emit. All unchanged.

Indexers never reach this loop — `ReflectionModel.Properties` filters them and `SourceGenModel` never yields them (§7.4).

### 7.2 `JsonStreamWriter` — sync and async object branches (2 sites)

This path does **not** use the model layer (#8466). It iterates raw `PropertyInfo` and must express the *same* predicate in `System.Reflection` vocabulary:

1. skip if `!property.CanRead`;
2. skip if `property.GetIndexParameters().Length > 0`;
3. skip if `Attribute.IsDefined(property, typeof(IgnoreDataMemberAttribute))`;
4. **NEW —** skip if `property.SetMethod?.IsPublic != true` **and** `!Attribute.IsDefined(property, typeof(JsonWriteAttribute))`.

**Critical instruction for the implementer:** clause 4 must test `SetMethod?.IsPublic != true`, **not** `!property.CanWrite`. `CanWrite` is `true` for a non-public setter, so using it would make this path emit `{ get; private set; }` while the other two omit it — recreating the divergence D6 exists to close. This is the single most likely implementation slip in the change.

### 7.3 How the attribute is honoured on the `JsonStreamWriter` bypass (brief item **c**)

`JsonStreamWriter` never consults `IModel`, so it cannot read `IPropertyInfo.Attributes`. It reads the attribute **directly off `System.Reflection.PropertyInfo`** via `Attribute.IsDefined`. This works, and works *consistently with the other path*, for a measured reason: `ReflectionProperty.Attributes` is itself `propertyInfo.GetCustomAttributes()`, and both APIs honour `AttributeUsage.Inherited` identically. With `Inherited = false` (D3), `Attribute.IsDefined` and the model path return the same answer for every shape measured in §4.

**Residual, and it is real:** on this path a `[ReflectType]` type still pays full runtime reflection, and `[DataMember(Name=…)]` is still ignored, so the same object still emits *different keys* through the two writers. That is **#8466**, pre-existing, and explicitly not fixed here. This design does not widen it — it adds one clause to a guard that already carries three, in a routine that already duplicates the rule.

**DRY math, per #1136 §1 and #114 §0** (required, not optional, for a 4-site inline decision): the new clause is `1` line × `4` sites = **4 lines**, against the ~15–20 threshold. Below it — inline is correct and no helper is extracted. A helper is additionally *not available* here without harm: the two vocabularies are different types (`IPropertyInfo` vs `PropertyInfo`), so a shared helper would need an overload pair — a new abstraction for four one-line call sites, which fails #1136 §4's *can-it-be-inlined* test. The structural cure is #8466 (route the stream writer through `IModel`), which collapses all four sites to two and is correctly a separate unit of work.

### 7.4 Indexers

`AttributeTargets.Property` admits indexers, so `[JsonWrite] public string this[int i] => …` is expressible and would reach `ReflectionProperty.GetValue`, which calls `PropertyInfo.GetValue(instance)` with no index arguments → `TargetParameterCountException`. The existing filters make this unreachable: `ReflectionModel.Properties` and `.GetProperty` both exclude indexers, `SourceGenModel` never emits them, and `JsonStreamWriter` carries its own inline clause. **All three stay.** See D5 in §8.

---

## 8. What Is Reverted and What Is Kept (brief item **d**)

The 0.5.0-preview.1 change was five production edits. They are **not** one unit, and reverting wholesale would re-introduce two separate crashes.

| # | Site | Disposition | Reasoning |
|---|---|---|---|
| **1** | `JsonWriter` sync + async — dropped setter gate | **WITHDRAWN, replaced** | This *is* the regression (#8522). Replaced by the §6.2 predicate, not reverted to the 0.4.0 text — see D6 for why the 0.4.0 text is not the target. |
| **2** | `JsonStreamWriter` sync + async — dropped `CanWrite` gate | **WITHDRAWN, replaced** | Same, via §7.2. |
| **3** | `JsonReader` skip-when-not-assignable arm (sync + async) | **KEPT** — **D7** | Below. |
| **4** | `ReflectionModel.Properties` indexer filter | **KEPT** — **D5** | Below. |
| **5** | `ReflectionProperty.HasSetter => SetMethod?.IsPublic == true` | **KEPT** — **D6** | §6.3. |

**D7 — keep the reader's skip-when-not-assignable arm. Reverting restores a crash.**
This arm is an **independent crash fix**, not part of the writer change. `Pooshit.Reflection`'s generator emits a literal `null` setter delegate for get-only and non-public-set properties; before the arm existed, `JsonReader` called `SetValue` into it unconditionally, so `Json.Read<T>` on **any** `[ReflectType]` type carrying such a property threw `NullReferenceException` (measured at exact HEAD in #8447 round 2, recorded in #8464 and #3342). Decisively for *this* design: **the reader's exposure never depended on the writer.** A wire payload can name any key; whether the writer would have emitted it is irrelevant. Making the writer opt-in therefore does not reduce the arm's reachability by one bit. Reverting it reinstates a hard `NullReferenceException` on the library's primary (source-gen) read path. It also remains the mechanism by which a `[JsonWrite]` property round-trips gracefully — the emitted key is read and discarded rather than throwing. Keep, unchanged, including its `typeof(object)` skip type (confirmed correct in #8447: `property.PropertyType` would turn a type-mismatched stale field into a `FormatException`).

**D5 — keep the `ReflectionModel.Properties` indexer filter.**
Under opt-in, the *default* route to a get-only indexer closes (an unattributed indexer fails the setter clause). But the filter is **not** dead defence-in-depth, for two reasons: (i) `[JsonWrite]` on an indexer is expressible (D2/§7.4) and would crash without it — a **reachable** scenario, so #1136 §6's *defensive-code-for-impossible-scenarios* rule does not apply; (ii) it is a repair of an internal inconsistency — `ReflectionModel.GetProperty` already filtered indexers and `Properties` did not — and it is what makes the two `IModel` implementations agree, which is the abstraction's job (#3342 explicitly forbids pushing this into the writers, because `IPropertyInfo` exposes no index-parameter concept). Keeping it is also the *null action*: it is already in the tree, and removing it would be the change. Keep.

---

## 9. Read Path — Unchanged (concept node #3341 untouched)

`JsonReader` assigns only what is publicly settable. **`[JsonWrite]` has no read-side meaning and must not acquire one.** A get-only or non-public-set property nominated for writing is still not assigned from the wire; the key is read and discarded by the D7 arm.

This preserves the deliberate write/read asymmetry documented in #3340 — but shrinks its blast radius from *"every readable property on every type"* to *"the properties a consumer explicitly nominated"*. `Json.Read<T>(Json.WriteString(x))` is not an identity for a type with `[JsonWrite]` members; the nominated member is recomputed, not restored. That is the correct semantic for a derived value and is why the attribute is named for *writing* specifically.

---

## 10. Anonymous Types — Accepted Consequence (brief item **e**)

**Stated explicitly so nobody rediscovers it as a bug:** anonymous-type properties are get-only and an anonymous type **cannot carry attributes**. Under this design `Json.WriteString(new { name = "gangolf", value = 42 })` therefore emits **`{}`** again.

This is not an oversight and not a regression against 0.4.0 — it is the behaviour of every released package. Consequences to record:

- **#3348 reopens.** Its 2026-08-18 "RESOLVED" section is superseded; the trap it documents is live again, now *by decision* rather than by accident.
- **Trap docs #159 and #1051 (and the third copy in #1356) remain valid and must not be edited.** Their guidance — *use concrete DTOs with `get;set;`* — is correct under this design, with one addition: *or mark the property `[JsonWrite]`*.
- **#8456 becomes moot.** It was filed to amend those docs once the fix shipped, blocked on a release. The fix is being withdrawn, so it should be closed as won't-do rather than left blocked.

**The seam was considered and is priced, not hand-waved.** Anonymous types are compiler-generated, uniformly get-only, and provably acyclic (they are constructed bottom-up, so no anonymous type can reference itself) — so a `CompilerGeneratedAttribute` + name-shape predicate that waives the setter clause for them would be *safe*. It is not *free*: it costs a type-shape heuristic that must live at a site both writer families reach (the model layer serves `JsonWriter`, but `JsonStreamWriter` bypasses it — §7.3 — so it lands in **two** places), plus a matching test matrix on four writer branches, plus a permanent public-behaviour commitment to an undocumented compiler naming convention. Per #1136 §1 YAGNI: **no named consumer is asking for it.** #3348 was a map-discovered ticket, not a user report, and the user's direction here is explicit and points the other way. **Rejected. Apply YAGNI.** If a consumer surfaces with a real need, the seam is still available and will then have a concrete shape to satisfy.

I agree with the operator's framing on this item.

---

## 11. Cycle Detection and Depth Cap — Not Included (brief item **f**)

**Decision: none. No cycle set, no depth counter, no type exclusion list.**

Justification against YAGNI (#1136 §1, §6; #1184), not against prudence:

1. **The reported failure is dissolved, not mitigated.** #8522's crash required the writer to descend into `Exception` / `Type` / `MethodBase`. Every property that carried that descent is get-only and none of them carries `[JsonWrite]` — a BCL type cannot be annotated by a consumer. Under §6.2 the walk never starts. #8522's own resolution section says exactly this: these guards become *"defence in depth at most"*.
2. **Adding them would be paying for a scenario the design has removed.** #1136 §6 names *"defensive code for impossible scenarios"* as fix-on-sight, and a depth cap in particular is a magic number with no named operator and no environment variance (#1136 §3) — it would arrive as a config knob within two releases.
3. **They would not be free.** With no shared writer core (#3340: three hand-maintained routines), a reference-identity set threads through four recursive routines — the highest-duplication change this codebase can absorb, for a scenario it no longer has.

**Named residual risk, honestly.** A *consumer* DTO with plain `{ get; set; }` back-references (`Parent` ↔ `Children`) still recurses without bound and still terminates the process. That is true of **every released version** of this library, is untouched by this design, and has never been reported. It is out of scope here (this design must not grow to cover it), but the operator should decide whether it deserves its own ticket — see open question §16.3. Note the honest framing: opt-in **removes the library's own contribution** to unbounded walks; it does not make the writer cycle-safe in general, and this document does not claim it does.

---

## 12. Contracts & Interfaces (Abstract)

### 12.1 `JsonWriteAttribute` — consumer-facing contract

| Aspect | Contract |
|---|---|
| **Applies to** | A property declaration. Not fields (D2), not types, not parameters. |
| **Means** | "Emit this property even though it has no public setter." |
| **Does not mean** | "Read this property from the wire" (§9); "name this property" (`[DataMember]`); "always emit even if null" (`ExcludeNullProperties` still applies). |
| **On a property that already has a public setter** | No effect. Harmless and not an error — the property was already eligible. |
| **On a set-only property** | No effect; the property stays omitted (§6.2 clause 1). |
| **Combined with `[IgnoreDataMember]`** | `[IgnoreDataMember]` wins; the property is omitted. |
| **Combined with `[DataMember(Name=…)]`** | Orthogonal and composable — nomination from one, key name from the other. |
| **Inheritance** | Applies to the declaration it is written on. A derived class that `override`s a nominated property must re-declare the attribute on the override (D3). A derived class that merely *inherits* the property without overriding needs nothing. |
| **Attribute presence semantics** | Presence is the entire signal; there is no value to configure (D4). |

### 12.2 Internal contracts touched

| Interface | Change |
|---|---|
| `IModel` | **none.** |
| `IPropertyInfo` (external, `Pooshit.Reflection`) | **none** — `Attributes`, `HasGetter`, `HasSetter` already carry everything required (measured, §4). |
| `IJsonWriter` / `IJsonStreamWriter` | **none** — no public method signature changes. |
| `JsonOptions` | **none** — deliberately. Eligibility is a per-property consumer declaration, not a per-call option; a `JsonOptions.WriteGetOnlyProperties` flag would be a global knob with no named operator (#1136 §3) that would reinstate the #8522 walk whenever switched on. **Do not add it.** |

---

## 13. Quality Attributes & Trade-offs

| Attribute | Effect |
|---|---|
| **Safety** | The decisive win. The default flips from *"every type in every consuming repo silently gained wire members"* to *"nothing changes shape unless someone asks"*. #8522's process-kill is removed by construction. |
| **Performance** | Neutral to slightly positive. One extra `Attributes` scan per property, but only for properties that fail the setter test — i.e. exactly those previously *skipped* under 0.4.0. Hot-path types (`{ get; set; }` DTOs) short-circuit on `HasSetter` before touching `Attributes`. **Instruction:** order the clause so `HasSetter` is evaluated first. |
| **Maintainability** | Positive. The write predicate and the read predicate become the *same* predicate (`HasSetter`) with one explicit escape hatch. #3340's "the two paths disagree" axis closes on both sides. |
| **Discoverability** | The main cost. A consumer must know the attribute exists. Mitigated by naming it after the library's own verb (D1) and by a README note (§14, phase 5). Not mitigated by a runtime diagnostic — an "object serialized to `{}`" warning was #3348's fallback suggestion and is rejected under KISS: a serializer that logs is a serializer with a logging dependency. |
| **Compatibility** | Two named breaks, both in §17. Neither is silent in this document; both must appear in the PR body. |

**Alternatives considered and rejected**

| Alternative | Rejected because |
|---|---|
| Keep emit-by-default + add cycle detection and a `Type`/`MemberInfo`/`Exception` exclusion list | Directly contrary to the owner's direction. Also unbounded: the exclusion list is a denylist against an open world, and every consumer type in every repo still silently gained wire members. |
| Spell opt-in with `[DataMember]` | `[DataMember]`'s established meaning *in this library* is **naming**. Overloading it would (a) make every existing `[DataMember(Name=…)]` on a get-only property change behaviour silently, (b) create a rule where a get-only property emits only if it also happens to be renamed — indefensible, and (c) half-adopt BCL `[DataContract]`/`[DataMember]` opt-in semantics that this library does not implement. Two spellings for one concept also fails DRY at the semantic level. |
| `[DataMember]` **or** `[JsonWrite]` both opt in | Same objection, plus a second code path to test. One mechanism (KISS). |
| A `JsonOptions` flag | §12.2. Reinstates #8522 on the flag's "on" branch. |
| Revert to 0.4.0 byte-for-byte, attribute layered on top | Reinstates the #8444 `private set` path split verbatim — see D6. Also under-determined: there is no single 0.4.0 behaviour for that shape. |

---

## 14. Implementation Guidance — ordered build phases

**One PR.** This is a single unit of work: withdrawing the gate and adding its replacement are not separately shippable (a tree with the gate withdrawn and no attribute is #8522; a tree with the attribute and no gate change is inert). No split warranted under the one-feature-one-PR rule.

**Phase 1 — the attribute.** Add `Pooshit.Json/JsonWriteAttribute.cs`: a public sealed-or-not `Attribute` subclass, no members, `[AttributeUsage(AttributeTargets.Property, Inherited = false)]`, with an XML `<summary>` stating the §12.1 contract in one or two sentences (#114 §3 XML docs; §4 forbids `//` comments — the contract goes in the XML doc, nowhere else).

**Phase 2 — `JsonWriter`, both branches.** Apply §7.1 clause (c) at both object-branch sites. `HasSetter` first, `Attributes` scan second.

**Phase 3 — `JsonStreamWriter`, both branches.** Apply §7.2 clause 4 at both sites. **Re-read the §7.2 critical instruction** — `SetMethod?.IsPublic != true`, never `!CanWrite`.

**Phase 4 — test realignment.** The suite is 213/213 at `b1580b1`. Expected-output inversions:

| Existing test | Action |
|---|---|
| `Write_ComputedGetOnlyProperty_EmitsValue` / `WriteAsync_…` | Keep asserting emission; add `[JsonWrite]` to the `ComputedIdData` fixture. |
| `WriteValue_ComputedGetOnlyProperty_EmitsValue` / `WriteValueAsync_…` | Same — stream-writer path. |
| `RoundTrip_ComputedGetOnlyProperty_…` / `RoundTripAsync_…` | Same fixture; assertions unchanged (read side unchanged). |
| `Write_AnonymousType_EmitsRealKeys` / `WriteAsync_…` | **Invert** — assert `{}`. Retarget the `[Description]` to name §10 as a decision, so the next reader does not file it as a bug. |
| `Write_PrivateSetPropertyWithReflectType_Emits` | **Invert** — omitted (D6). |
| `Write_PrivateSetPropertyWithoutReflectType_StillEmits` / `WriteAsync_…` | **Invert** — omitted (D6). This is the §17 break-2 guard; name it accordingly. |
| `Write_InitOnlyProperty_Emits` | Unchanged — `init` setters are public, so `HasSetter` is `true`. Verify, do not assume. |
| `Write_SetOnlyProperty_StaysOmitted`, `WriteValue_SetOnlyProperty_StaysOmitted` | Unchanged. |
| `Write_GetOnlyIndexerOnPlainDto_…` (both) | Unchanged (D5). |
| All `JsonReaderTests` read-refusal tests | Unchanged (§9, D7). |
| `NamingStrategyTests.WriteDataMember` | Should stay green — it was retargeted to `Does.Contain("\"over_the_top\":7\")` and no longer asserts on `SnakeData.Bum` (get-only, now omitted). Confirm by running, not by reading. |

New coverage required — the matrix that pins the design (each cell **with** and **without** `[ReflectType]`, and on all three writer routines where applicable):

1. get-only **without** `[JsonWrite]` → omitted (S3).
2. get-only **with** `[JsonWrite]` → emitted (S2).
3. `{ get; private set; }` **with** `[JsonWrite]` → emitted (D6 remedy is real).
4. `[JsonWrite]` + `[IgnoreDataMember]` → omitted (§6.2 clause 2).
5. `[JsonWrite]` on a **set-only** property → omitted, no throw (§6.2 clause 1).
6. `[JsonWrite]` + `[DataMember(Name=…)]` → emitted under the DataMember name (`JsonWriter` path only — the stream writer ignores DataMember names, #8466; assert the *current* stream behaviour rather than the desired one, or the test becomes a duplicate bug report).
7. **The regression guard, S1:** `Json.WriteString(new Exception("boom"))` completes and produces finite output, on all three routines. This is the test whose absence let #8522 ship — it must exist. Assert termination and shape, not an exact string (BCL exception surface varies across TFMs).
8. `[JsonWrite]` on a base property that a derived class `override`s → **omitted on both paths** (D3 — pins the Inherited=false decision so a future `Inherited` change goes red).

Test naming follows #114 §13.1 (`MethodName_Condition_ExpectedResult`) — this repo's `Write_*_*` convention already matches. `[Description]` is one sentence of intent and **may cite a ticket** (#114 §4 and the 2026-08-08 ruling) — cite #8522 on the exception guard and #3348 on the inverted anonymous-type tests, because both assert something a reader would otherwise mistake for a defect.

**Phase 5 — documentation.** README gets a short section on `[JsonWrite]`: what it does, the one-line rule from §6.2, and the note that `init` and public setters need nothing.

**Phase 6 — graph reconciliation (post-merge, per #3414).** Update **#3340** (write-eligibility rule, superseding its current "readable, no setter required" statement), **#3342** (property-eligibility table + the `Inherited = false` measurement from §4 and its rationale — this is new mechanism knowledge the map does not have), **#3348** (reopen with the §10 decision), and the map root **#3293**. Close **#8456** as won't-do. Add the §4 measurement result to #3342 so no future agent re-measures whether attributes flow.

---

## 15. Pre-Design Checklist Audit (#1136 §5)

Every item, walked. No item silently skipped.

**KISS / DRY / YAGNI**

- ✅ *No new type mirroring an existing type's value-space.* The assembly exports zero attributes; `[DataMember]` was evaluated as a candidate carrier and rejected on stated grounds (§13). No mirror.
- ✅ *No new abstraction with one implementation.* No interface, no base class, no factory. One attribute type and one changed predicate.
- ✅ *No element justified by "we might need X later".* D4 removes attribute parameters on exactly this ground; §11 removes cycle/depth guards on exactly this ground; §12.2 removes the `JsonOptions` flag on exactly this ground.
- ✅ *No deprecation period, feature flag, compatibility shim, or transition window.* None proposed. The break is stated and shipped (§17), not shimmed.
- ✅ *DRY math quoted for the inline-at-N-sites decision.* §7.3: `1 line × 4 sites = 4`, below the ~15–20 threshold; plus the structural reason a shared helper is unavailable across two type vocabularies, plus the pointer to #8466 as the real cure.

**Existing systems first**

- ✅ *Audited whether an existing surface covers the concern.* `[DataMember]`, `[IgnoreDataMember]`, `JsonOptions` and `[ReflectType]` were each examined as carriers; each rejection is argued in §13 / §12.2, not asserted.
- ✅ *New layer justified concretely.* No new layer. The one new **type** is justified by measurement (§4: nothing existing carries the meaning) and by #8444's own finding that the assembly exports no attribute a consumer could use.
- n/a *New persisted data point.* None — no schema, no storage, no SQL in this change.
- ✅ *Consumer chain recursed.* The attribute's consumer chain is named and terminal: consumer declaration → `IPropertyInfo.Attributes` / `PropertyInfo` → the four writer gates → emitted JSON. Not transitive-dead: mamgo-backend's five `Id` properties (#8522) are the named waiting consumer.

**Configurability**

- ✅ *Every new knob has a named operator or environment difference.* **No new knob** — no `JsonOptions` flag (§12.2), no attribute parameters (D4), no depth cap (§11).
- n/a *Telemetry-then-tune knob.* None.
- ✅ *Magic numbers stay `const`.* None introduced.

**Less is better**

- ✅ *can-delete / can-merge / can-inline run on every element.* Attribute: cannot be deleted (it is the feature) and cannot be merged into `[DataMember]` (§13). Predicate clause: inlined at all four sites rather than extracted (§7.3 math). Namespace: merged into `Pooshit.Json` rather than a new sub-namespace (§6.1).
- ✅ *Trade-offs named explicitly where a costlier option loses.* §10 (anonymous-type seam, priced then rejected), §11 (guards, with the residual risk named rather than buried), §13 (alternatives table), D6 (the honest cost stated, not softened).
- ✅ *Radical-clean chosen over compromise where the surface has no consumer.* §10 takes the clean consequence (anonymous types → `{}`) instead of a heuristic half-measure.
- n/a *Reader-inventory / carrier-swap tables.* No field is renamed or removed; nothing to inventory. The nearest analogue — the affected-test inventory — is enumerated exhaustively in Phase 4 rather than by representative sample.

**Data deliverables** — n/a in full. No SQL, no migration, no backfill.

**Document discipline**

- ✅ *Cites Code Contracts (#114) and Design Contracts (#1136) as load-bearing.* Header + inline at each application point.
- ✅ *Inventories explicit.* Reverted-vs-kept (§8), affected tests (Phase 4), out-of-scope with tracking ids (§2).
- ✅ *Out-of-scope items listed explicitly, not merely absent.* §2 table, nine rows, each with a ticket or a section reference.
- ✅ *No multi-paragraph rationale for things that obviously stay.* D5 and D7 are argued because the brief put them genuinely in question; nothing else is defended at length.
- ✅ *Predecessor doc banner.* n/a — `docs/architecture/` did not exist before this document; there is no predecessor design to supersede. The superseded *artifact* is the 0.5.0-preview.1 code itself, handled in §8, and the superseded *ticket state* is #3348's resolution section, handled in §10 and Phase 6.

**Result: PASS.** No KISS/DRY/YAGNI violation is forced by this ask, so nothing is bounced under #1136 §7's architect-side reciprocal. The ask actively *removes* complexity.

---

## 16. Open Questions

**16.1 — Package version and the fate of `0.5.0-preview.1`.** `Pooshit.Json.csproj` reads `0.3.40-preview`, while the published stream is at `0.4.0-preview.1` / `0.5.0-preview.1` — so the version is evidently set at pack time, not in the csproj. Two operator decisions: what version this ships as (recommendation: a new **minor**, since §17 carries two behaviour changes), and whether `0.5.0-preview.1` is unlisted from NuGet. It contains a process-killing defect reachable from any error response; leaving it installable is a live hazard for anyone who upgrades without reading #8522. **For the operator, not for John.**

**16.2 — Attribute name confirmation.** `[JsonWrite]` is my recommendation and is argued in D1, but it is the library's **first exported attribute** and therefore sets the naming precedent for every attribute this library ever adds. Worth thirty seconds of Toni's time before it becomes public API. `[JsonInclude]` is the runner-up; the collision argument against it is in D1.

**16.3 — Should consumer-DTO reference cycles get their own ticket?** §11 declines to add cycle detection, correctly. But a consumer DTO with plain `{ get; set; }` back-references still kills the process, in every released version, and always has. That is genuinely out of scope here and I have deliberately **not** filed a task for it (filing speculative work is exactly what #1184 warns against). Flagging it once, for Toni to rule on: real gap worth a ticket, or accepted library posture?

**16.4 — Should `[JsonWrite]` on an already-settable property be an error?** The design says harmless no-op (§12.1). The alternative — a build-time analyzer or a runtime throw — is out of proportion and would require an analyzer package this library does not have. I am confident in no-op; recording it only because it is the kind of thing a reviewer asks about.

---

## 17. Breaking Changes — both must appear in the PR body

**Break 1 — get-only and computed properties stop being emitted (vs `0.5.0-preview.1` only).** This is the withdrawal itself, and it is the point. Consumers on `0.4.0-preview.1` see **no change**. Remedy: `[JsonWrite]`.

**Break 2 — `{ get; private set; }` / `protected set` / `internal set` stop being emitted on the *reflection* path (vs `0.4.0-preview.1`).** This one is **silent** — a key simply disappears — and it is the consequence of D6 that goes beyond "undo". Consumers on the source-gen path (`[ReflectType]`) see no change, because that path already omitted these. Remedy: `[JsonWrite]`. Full argument and the "there is no single before" framing in §6.3; state both in the PR body, not just the remedy.

**Not a break, but state it:** the read-side refusal of non-public setters (shipped in `0.5.0-preview.1`) is **retained**, so a consumer moving from `0.4.0-preview.1` inherits that change too — reflection-path properties with non-public setters are no longer populated from the wire. It was already documented for `0.5.0-preview.1`; it does not disappear because the writer half is being withdrawn.

---

## 18. Migration Note for the Waiting Consumer

mamgo-backend holds a prepared patch at `C:\dev\claude\_scratch\pooshit-json-050\unit-a-getonly-shape.patch` (#8522). It assumes emit-by-default and will **not** apply as-is. Under this design each of its five `Id` properties becomes:

- drop the no-op `set { }` (the smell #8444 was filed about), and
- add `[JsonWrite]` to the get-only declaration.

The patch remains useful as a **file-and-line index** for which properties are affected, not as something to `git apply`.
