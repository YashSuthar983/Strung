using UnityEngine;

public class JumpPredictor : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int resolution = 30;
    public float timeStep = 0.1f;

    public void ShowTrajectory(Vector3 start, Vector3 velocity)
    {
        Vector3[] points = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float time = i * timeStep;
            points[i] = start + velocity * time + 0.5f * Physics.gravity * time * time;
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    public void HideTrajectory()
    {
        lineRenderer.positionCount = 0;
    }
}
