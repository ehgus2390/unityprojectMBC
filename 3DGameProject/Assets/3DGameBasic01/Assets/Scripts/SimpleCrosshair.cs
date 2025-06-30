using UnityEngine;

public class SimpleCrosshair : MonoBehaviour
{
    public float crosshairSize = 10f;
    public Color crosshairColor = Color.white;

    void OnGUI()
    {
        Color oldColor = GUI.color;
        GUI.color = crosshairColor;

        float xMin = (Screen.width / 2) - (crosshairSize / 2);
        float yMin = (Screen.height / 2) - (crosshairSize / 2);

        // 가로선
        GUI.DrawTexture(new Rect(xMin, Screen.height / 2, crosshairSize, 1), Texture2D.whiteTexture);
        // 세로선
        GUI.DrawTexture(new Rect(Screen.width / 2, yMin, 1, crosshairSize), Texture2D.whiteTexture);

        GUI.color = oldColor;
    }
}
