using System.Runtime.CompilerServices;

// Grants the EditMode test assembly access to SceneFlowValidator's internal validation methods, so
// tests exercise the exact same pass/fail logic as the "Validate SceneFlow" menu item.
[assembly: InternalsVisibleTo("GameLib.SceneFlow.Tests")]
