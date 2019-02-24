using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Shot : MonoBehaviour
{
    private LineRenderer lr;
    public float score;
    public float timeToHit;
    public ShotData shotData;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        shotData = new ShotData();
        shotData.currentAngleY = new Ref<float>();
        shotData.currentAngleZ = new Ref<float>();
        shotData.currentForce = new Ref<float>();
    }
    
    public void SetUpLine(Vector3[] points, float _score, float _timeToHit, ShotData _shotData)
    {
        lr.positionCount = points.Length;
        lr.SetPositions(points);
        score = _score;
        timeToHit = _timeToHit;
        shotData.currentAngleY.Value = _shotData.currentAngleY.Value;
        shotData.currentAngleZ.Value = _shotData.currentAngleZ.Value;
        shotData.currentForce.Value = _shotData.currentForce.Value;
    }

    
}
