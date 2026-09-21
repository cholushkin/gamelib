# RandomTree / StateNode System

A simple, lightweight, and deterministic system for controlling Unity GameObject hierarchies using **state-driven nodes**.

---

## 🧠 Mental Model

This system is built around a strict separation of concerns:

```
RandomTree → drives evaluation
Node       → defines behavior
NodeState  → represents data (which children are active)
```

* **Node** = state transformer  
* **NodeState** = dumb data  
* **RandomTree** = execution system  

There is no hidden state. The hierarchy itself is the source of truth.

---

## 🎯 Purpose

Provide a simple, lightweight, and deterministic system for:

* Randomized hierarchy activation  
* Manual control using the same API  
* Predictable behavior without overengineering  

---

## ⚙️ Core Principles

* MonoBehaviour-based only  
* Transform hierarchy defines structure  
* Nodes operate on **direct children only**  
* No recursion inside nodes  
* No stored state  
* No mode flags (manual vs random)  
* Full determinism from seed  

---

## 🧩 Architecture

### RandomTree (Controller)

Responsibilities:

* Own and manage seed  
* Create RNG instance  
* Build impact tree (Nodes in depth-first order)  
* Evaluate all nodes  

```csharp
RandomTree → node.SetState(rng)
```

---

### Node (Behavior)

A node that:

* Generates or accepts a **NodeState**  
* Validates it  
* Applies it to its direct children  

```csharp
SetState(Random rng)      // automatic (random)
SetState(NodeState state) // manual
GetState()                // derived from hierarchy
```

---

### NodeState (Data)

A minimal, transferable representation of state:

```csharp
NodeState = indices of active children
```

Properties:

* Immutable  
* Dumb container (no validation)  
* Can be shared between nodes  
* Nodes are the authority, not NodeState  

---

## 🔄 State Flow

### Automatic (Random)

```
RNG → GenerateState → Apply
```

### Manual

```
External → SetState → Validate → Apply
```

Both paths use the same system.

---

## 🧱 Base API (Node)

```csharp
SetState(Random rng)
SetState(NodeState state)
GetState()
```

---

## ⚠️ Validation Strategy

* Nodes validate incoming state  
* Invalid states produce **warnings**  
* No normalization (for now)  
* State is still applied (non-blocking)  

---

## 🎲 Determinism

The system guarantees identical results given:

* Same seed  
* Same hierarchy  
* Same transform order  

---

## 🚫 Explicit Limitations

This system intentionally does NOT include:

* Weights or probabilities per child  
* ScriptableObjects  
* Events or callbacks  
* Stored selection state  
* Runtime hierarchy mutation handling  

---

## 💡 Future Extensions

* State normalization  
* Zero-allocation NodeState  
* Partial subtree evaluation  
* Additional node types (Weighted, Probability, etc.)

---

## 🧭 Philosophy

* Keep it simple  
* Keep it explicit  
* Deterministic by design  
