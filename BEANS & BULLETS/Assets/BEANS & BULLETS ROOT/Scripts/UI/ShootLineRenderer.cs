using UnityEngine;
using System.Collections.Generic;

public class ShootLineRenderer : MonoBehaviour
{
    public static ShootLineRenderer Instance;

    private List<LineData> activeLines = new List<LineData>();
    private Material lineMaterial;

    private struct LineData
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public float expireTime;
    }

    void Awake()
    {
        Instance = this;
    }

    public void AddLine(Vector3 start, Vector3 end, Color color, float duration)
    {
        activeLines.Add(new LineData
        {
            start = start,
            end = end,
            color = color,
            expireTime = Time.time + duration
        });
    }

    void OnPostRender()
    {
        if (activeLines.Count == 0) return;

        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Disabled);
        }

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        GL.Begin(GL.LINES);

        for (int i = activeLines.Count - 1; i >= 0; i--)
        {
            if (Time.time > activeLines[i].expireTime)
            {
                activeLines.RemoveAt(i);
                continue;
            }

            GL.Color(activeLines[i].color);
            GL.Vertex(activeLines[i].start);
            GL.Vertex(activeLines[i].end);
        }

        GL.End();
        GL.PopMatrix();
    }
}