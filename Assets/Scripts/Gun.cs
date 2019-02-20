using UnityEngine.UI;
using UnityEngine;

public class Gun : MonoBehaviour {
    
    [Header("Visuals")]
    [Tooltip("The resolution of the parabola")]
    public int pointsOnParabola;
    public Text angleText;
    public Text velocityText;
    public Text rotationText;
    public Transform target;
    
    [Space]
    [Header("Variables to optimize")]
    public float currentForce;
    public float currentAngleY;
    public float angleY;
    public float angleZ;
    public float airForce;

    public float minAngleY;
    public float maxAngleY;
    public float minForce;
    public float maxForce;

    private float g;
    private float radianAngleY;
    private float radianAngleZ;

    private LineManager lineManager;

    private void Awake()
    {
        g = Mathf.Abs(Physics.gravity.y); //absolute force of the gravity for physics
        lineManager = GetComponent<LineManager>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            HillClimbing();
            //BruteForce();
        }
        //angle and velocity adjustments
        currentForce += 0.1f * Input.GetAxis("Horizontal");
        velocityText.text = "Velocity: " + currentForce;
        angleY = Mathf.Clamp(angleY + 1 * Input.GetAxis("Vertical"),0.1f,90);
        angleText.text = "Angle: " + angleY;
       // angleZ = Mathf.Clamp(angleZ + 1 * Input.GetAxis("Vertical"), -89.01f, 89.01f);
        rotationText.text = "Rotation: " + angleZ;
    }

    private void BruteForce()
    {
        lineManager.bestShot = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        currentAngleY = minAngleY;
        while (currentAngleY < maxAngleY - 1f)
        {
            radianAngleY = Mathf.Deg2Rad * currentAngleY;
            radianAngleZ = Mathf.Deg2Rad * angleZ;

            currentForce = minForce;
            while (currentForce < maxForce - 1f)
            {
                Vector3[] newShot = CalculateArray();
                float score = CalculateScore(newShot[newShot.Length - 1]); // the score of the new shot
                float timeToHit = (target.position.x - transform.position.x) / (currentForce * Mathf.Cos(radianAngleY)); //the time needed to hit the ground

                lineManager.NewShot(newShot, score, timeToHit); // create the new shot
                currentForce += 1f;
            }
            currentAngleY += 1f;
        }
    }
    
    private void HillClimbing()
    {
        // Random values
        currentAngleY = Random.Range(minAngleY, maxAngleY);
        currentForce = Random.Range(minForce, maxForce);
        radianAngleY = Mathf.Deg2Rad * currentAngleY;
        radianAngleZ = Mathf.Deg2Rad * angleZ;
        // Initial shot
        Vector3[] newShot = CalculateArray();
        float score = CalculateScore(newShot[newShot.Length - 1]); // the score of the new shot
        float timeToHit = (target.position.x - transform.position.x) / (currentForce * Mathf.Cos(radianAngleY)); //the time needed to hit the ground

        // create the new shot
        LineSetUp shot = lineManager.NewShot(newShot, score, timeToHit);

        ////------------------------------------------

        LineSetUp[] scoreArray = new LineSetUp[4];

        float[] variablesToChange = { currentForce , currentAngleY };
        float[] tweakStep = {1,-1 };

        ////do
        ////{
        int counter = 0;
        for (int i = 0; i < variablesToChange.Length; i++)
        {
            float currentParameter = variablesToChange[i];
            for (int j = 0; j < tweakStep.Length; j++)
            {
                variablesToChange[i] += tweakStep[j];
                print(currentForce);
                newShot = CalculateArray();
                score = CalculateScore(newShot[newShot.Length - 1]) ; // the score of the new shot

                timeToHit = (target.position.x - transform.position.x) / (currentForce * Mathf.Cos(radianAngleY)); //the time needed to hit the ground

                scoreArray[counter] = lineManager.NewShot(newShot, score, timeToHit);
                counter++;
                variablesToChange[i] = currentParameter;
            }
        }




        // } while (CheckScore(scoreArray, shot));
    }

    private bool CheckScore(LineSetUp[] _shotArray, LineSetUp currentShot)
    {
        bool check = false;

        for (int i = 0; i < _shotArray.Length; i++)
        {
            if (currentShot.score > _shotArray[i].score)
            {
                check = true;
                //lineManager.bestShot.
                break;
            }
        }

        return check;
    }

    //calculates all the points of the parabola
    private Vector3[] CalculateArray()
    {
        //creates an array of points based on the given resolution
        Vector3[] parabolaPoints = new Vector3[pointsOnParabola + 1]; 

        //the maximun distance a projectile can get (physics equation)
        float maxDistance = (currentForce * currentForce * (1 + Mathf.Sqrt(1 + ((2 * g * transform.position.y) / (currentForce * currentForce * Mathf.Sin(radianAngleY) * Mathf.Sin(radianAngleY))))) * Mathf.Sin(2 * radianAngleY))/(2*g);

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
        float y = transform.position.y + x * Mathf.Tan(radianAngleY)-((g*x*x)/(2*currentForce*currentForce*Mathf.Cos(radianAngleY) * Mathf.Cos(radianAngleY))); // physics equation
        float z = transform.position.z + x * Mathf.Tan(radianAngleZ) - ((airForce * x * x) / (2 * currentForce * currentForce * Mathf.Cos(radianAngleZ) * Mathf.Cos(radianAngleZ)));
        return new Vector3(x, y, z);
    }

    //if the last point is between the target radius (0.6) the score is 0 else the score is the distance.
    private float CalculateScore(Vector3 shot)
    {
        return Vector3.Distance(shot, target.position) < 0.3f ? 0 : Vector3.Distance(shot, target.position);
    }

}
