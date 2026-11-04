using System.Runtime.CompilerServices;

// Grants the EditMode test assembly access to internal setters on SceneFlow's ScriptableObject
// assets (SceneDefinition, RunProfile, SceneFlowSettings), so tests can build fixtures directly
// instead of going through AssetDatabase or reflection.
[assembly: InternalsVisibleTo("GameLib.SceneFlow.Tests")]
