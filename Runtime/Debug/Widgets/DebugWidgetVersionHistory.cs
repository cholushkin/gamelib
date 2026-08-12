using System.Text;
using GameLib.VersionHistory;
using UnityEngine;

namespace GameLib
{
    public class DebugWidgetVersionHistory : DebugWidgetImageAndText
    {
        public VersionHistory.VersionHistory VersionHistory;

        private void Start()
        {
            ApplyState();
        }

        private void Reset()
        {
            ApplyState();
        }

        public void ApplyState()
        {
            if (VersionHistory == null) return;

            var sb = new StringBuilder();

            foreach (var version in VersionHistory.Versions)
            {
                // Version Header
                sb.AppendLine($"<size=110%><b>{version.VersionName}</b></size>");

                // Version Changes
                foreach (var change in version.Changes)
                {
                    if (!change.IsIncluded) continue;

                    string colorHex = GetCategoryColor(change.Category);
                    sb.AppendLine($"  • <color={colorHex}><b>[{change.Category}]</b></color> {change.Description}");
                }
                
                sb.AppendLine(); // Spacer between versions
            }

            // Assuming base class has a SetText(string, Color) method
            SetText(sb.ToString().TrimEnd(), Color.white);
        }

        private string GetCategoryColor(ChangeCategory category)
        {
            return category switch
            {
                ChangeCategory.Feature => "#4CAF50",        // Green
                ChangeCategory.Fix => "#F44336",            // Red
                ChangeCategory.Performance => "#2196F3",    // Blue
                ChangeCategory.BreakingChange => "#FF9800", // Orange
                ChangeCategory.Internal => "#9E9E9E",       // Grey
                ChangeCategory.Build => "#795548",          // Brown
                ChangeCategory.Documentation => "#9C27B0",  // Purple
                _ => "#FFFFFF"                              // Default White
            };
        }
    }
}