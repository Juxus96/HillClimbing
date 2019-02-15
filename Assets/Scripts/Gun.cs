using UnityEngine.UI;
using UnityEngine;

public class Gun : MonoBehaviour {
    
    [Header("Visuals")]
    [Tooltip("The resolution of the parabola")]
    public int pointsOnParabola;
    public Text angleText;
    public Text velocityText;
    public Vector3 targetPos;
    
    [Space]
    [Header("Variables to optimize")]
    public float velocity;
    public float angle;

    private float g;
    private float radianAngle;
    
    private void Awake()
    {
        g = Mathf.Abs(Physics.gravity.y); //absolute force of the gravity for physics
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            radianAngle = Mathf.Deg2Rad * angle;

            Vector3[] newShot = CalculateArray();
            float score = CalculateScore(newShot[newShot.Length-1]); // the score of the new shot
            float timeToHit = (targetPos.x - transform.position.x) / (velocity * Mathf.Cos(radianAngle)); //the time needed to hit the ground

            GetComponent<LineManager>().NewShot(newShot, score, timeToHit); // create the new shot
        }
        //angle and velocity adjustments
        velocity += 0.1f * Input.GetAxis("Horizontal");
        velocityText.text = "Velocity: " + velocity;
        angle = Mathf.Clamp(angle + 1 * Input.GetAxis("Vertical"),0.1f,90);
        angleText.text = "Angle: " + angle;
    }

    //calculates all the points of the parabola
    private Vector3[] CalculateArray()
    {
        //creates an array of points based on the given resolution
        Vector3[] parabolaPoints = new Vector3[pointsOnParabola + 1]; 

        //the maximun distance a projectile can get (physics equation)
        float maxDistance = (velocity * velocity * (1 + Mathf.Sqrt(1 + ((2 * g * transform.position.y) / (velocity * velocity * Mathf.Sin(radianAngle) * Mathf.Sin(radianAngle))))) * Mathf.Sin(2 * radianAngle))/(2*g);

        //find the coordinates for each point of the parabola
        for (int i = 0; i <= pointsOnParabola; i++)
        {
            float distanceBetweenPoints = i / (float)pointsOnParabola;
            parabolaPoints[i] = calculateParabolaPoint(distanceBetweenPoints,maxDistance);

        }

        return parabolaPoints;
    }

    //calculates a single point of the parabola for the given distanceBetweenPoints
    Vector3 calculateParabolaPoint(float distanceBetweenPoints, float maxDistance)
    {
        float x = transform.position.x + distanceBetweenPoints * maxDistance; // the x coordinate is linear
        float y = transform.position.y + x * Mathf.Tan(radianAngle)-((g*x*x)/(2*velocity*velocity*Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle))); // physics equation
        return new Vector3(x, y);
    }

    //if the last point is between the target radius (0.6) the score is 0 else the score is the distance.
    private float CalculateScore(Vector3 shot)
    {
        return Vector3.Distance(shot, targetPos) < 0.3f ? 0 : Vector3.Distance(shot, targetPos);
    }

}
