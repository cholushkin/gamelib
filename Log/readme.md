Here’s your polished **README.md** — fully in Markdown, concise, and tailored to your system’s latest design (manual reload, lazy reinitialization, local logger wrapper, etc.) 👇

---

# 🧩 Unity Logging System (ZLogger-based)

A **modular, high-performance logging framework** for Unity built on **[ZLogger](https://github.com/Cysharp/ZLogger)** and **Microsoft.Extensions.Logging**.
It uses **ScriptableObject-based configuration** for complete flexibility and **lazy, on-demand initialization** for maximum efficiency.

---

## ✨ Features

### ⚙️ ScriptableObject Configuration

* Central **`LogManagerAsset`** controls all logging behavior.
* Each log provider (Unity Console, file, etc.) is defined as its own **ScriptableObject**.
* Global and per-provider:

  * **Hard Floor** (absolute minimum log level)
  * **Default Minimum Level**
  * **Solo / Mute flags**
  * **Category filters**

### 🔄 Dynamic & Lazy Reloading

* The logger factory is **lazily initialized** — it builds itself the first time any logger is requested.
* You can **rebuild all loggers on demand** in one click:

  * Use the **“Reload Logger Factory Now”** button in the `LogManagerAsset` inspector.
  * Or call `LogManagerAsset.Instance.ReloadNow()` at runtime.

### 📦 Local Logger Configuration

* Each component can have its own lightweight `Logger` wrapper:

  * Local enable/disable (`LocalIsEnabled`)
  * Local minimum log level (`LocalLogLevel`)
  * Category name
* Loggers automatically detect configuration changes via a **version number** managed by the `LogManagerAsset`.

### ⚡ Ultra-Efficient String Interpolation

* Powered by **ZLogger**, which uses compile-time templates for structured logs:

  * No boxing, no GC allocations, even with string interpolation.
  * Ideal for high-frequency logging in performance-critical systems.

---

## 🧱 Architecture Overview

```
[LogManagerAsset] ─→ [LoggerConfiguration]
       │                   ├── ProviderConfig (ZLogger Unity Debug, File, etc.)
       │                   └── Category filters and thresholds
       │
       └─ lazy builds → ILoggerFactory
                             └─ used by → [Example.VersionAwareLogger.Logger]
```

---

## 🧰 Usage

### 1. Create Configuration Assets

* In Unity: **Assets → Create → Logging → Log Manager Asset**
* Create and assign a `LoggerConfiguration` and provider assets.

### 2. Add Local Logger Field

```csharp
public Example.VersionAwareLogger.Logger Logger;

[Button]
void Start()
{
    Logger.Instance().ZLog(Logger.Level(LogLevel.Trace), $"hi");
}
```

* The first call to `Logger.Instance()` will **lazily initialize** the global factory if it hasn’t been built yet.
* Each call automatically checks if the configuration version changed and rebuilds its underlying logger if needed.

### 3. Manually Rebuild All Loggers

* In the Unity Inspector: click **“Reload Logger Factory Now”**.
* Or in code:

  ```csharp
  Logging.Runtime.LogManagerAsset.Instance.ReloadNow();
  ```

---

## 🚀 Benefits

* 🧱 Centralized yet modular configuration.
* 🧠 Lazy initialization ensures minimal startup cost.
* 🔄 Reloadable at runtime or via Editor button.
* ⚡ Zero-allocation structured logging via ZLogger.
* 🔍 Per-object local log filtering and enable/disable control.
* 🕹 Works seamlessly in both **Editor** and **runtime** builds (including mobile & IL2CPP).

---

## 📜 License

MIT © YourName
Built with ❤️ on top of [ZLogger](https://github.com/Cysharp/ZLogger) and [Microsoft.Extensions.Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging).

---

Would you like me to include a short “Best Practices” section (like recommended log levels per provider or example category naming)?
