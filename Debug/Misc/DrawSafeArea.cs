//using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DrawSafeArea : MonoBehaviour
{
    public Canvas Canvas;

    private void OnDrawGizmos()
    {
        // Draw screen resolution as a Gizmo label
        string combinedInfo = "";
        Gizmos.color = Color.white;

        // Get the screen resolution
        Resolution screenResolution = Screen.currentResolution;
        combinedInfo += "Resolution: " + screenResolution.width + "x" + screenResolution.height + "\n";

        // Get CanvasScaler parameters as a string
        if (Canvas != null)
        {
            CanvasScaler canvasScaler = Canvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                combinedInfo += "Canvas Scaler\n" +
                                "Reference: " + canvasScaler.referenceResolution + "\n" +
                                "Mode: " + canvasScaler.uiScaleMode + "\n" +
                                "Scale Factor: " + canvasScaler.scaleFactor + "\n";

                // Get the Canvas size
                RectTransform canvasRect = Canvas.GetComponent<RectTransform>();
                combinedInfo += "Canvas Size: " + canvasRect.sizeDelta.x + "x" + canvasRect.sizeDelta.y + "\n";
            }
        }

        // Get the safe area of the screen
        Rect safeArea = Screen.safeArea;
        combinedInfo += "Safe Area: X=" + safeArea.x + ", Y=" + safeArea.y + ", Width=" + safeArea.width + ", Height=" + safeArea.height;

        // Draw combined information as a Gizmo label
        if (!string.IsNullOrEmpty(combinedInfo))
        {
            Gizmos.color = Color.white;
            //Handles.Label(transform.position, combinedInfo);
        }

        DrawSafeAreaImpl();
    }


    private void DrawSafeAreaImpl()
    {
        // Get the safe area of the screen
        Rect safeArea = Screen.safeArea;

        // Calculate the corners of the safe area
        Vector3 bottomLeft = new Vector3(safeArea.x, safeArea.y, 0);
        Vector3 bottomRight = new Vector3(safeArea.x + safeArea.width, safeArea.y, 0);
        Vector3 topLeft = new Vector3(safeArea.x, safeArea.y + safeArea.height, 0);
        Vector3 topRight = new Vector3(safeArea.x + safeArea.width, safeArea.y + safeArea.height, 0);

        // Draw the safe area using Gizmos lines
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}