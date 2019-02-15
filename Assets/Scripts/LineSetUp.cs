using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class LineSetUp : MonoBehaviour {

    private LineRenderer lr;
    public float score;
    public float timeToHit;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }
    
    public void SetUpLine(Vector3[] points, float _score, float _timeToHit)
    {
        lr.positionCount = points.Length;
        lr.SetPositions(points);
        score = _score;
        timeToHit = _timeToHit;
    }

    
}
