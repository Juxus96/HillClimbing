using System.Collections.Generic;
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
    public float airForce;
    public ShotData shotData;

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
        shotData = new ShotData();
        shotData.currentAngleY = new Ref<float>();
        shotData.currentAngleZ = new Ref<float>();
        shotData.currentForce = new Ref<float>();
        lineManager = GetComponent<LineManager>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            HillClimbing();
            //BruteForce();
        }
    }

    private void BruteForce()
    {
        lineManager.bestShot = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        shotData.currentAngleY.Value = minAngleY;
        while (shotData.currentAngleY.Value < maxAngleY - 1f)
        {
            radianAngleY = Mathf.Deg2Rad * shotData.currentAngleY.Value;
            radianAngleZ = Mathf.Deg2Rad * shotData.currentAngleZ.Value;

            shotData.currentForce.Value = minForce;
            while (shotData.currentForce.Value < maxForce - 1f)
            {
                Vector3[] newShot = CalculateArray();
                float score = CalculateScore(newShot[newShot.Length - 1]); // the score of the new shot
                float timeToHit = (target.position.x - transform.position.x) / (shotData.currentForce.Value * Mathf.Cos(radianAngleY)); //the time needed to hit the ground

                lineManager.NewShot(newShot, score, timeToHit,shotData); // create the new shot
                shotData.currentForce.Value += 1f;
            }
            shotData.currentAngleY.Value += 1f;
        }
    }
    
    private void HillClimbing()
    {
        // Random values
        shotData.currentAngleY.Value = Random.Range(minAngleY, maxAngleY);
        shotData.currentForce.Value = Random.Range(minForce, maxForce);

        //randian angls
        radianAngleY = Mathf.Deg2Rad * shotData.currentAngleY.Value;
        radianAngleZ = Mathf.Deg2Rad * shotData.currentAngleZ.Value;

        // Initial shot
        Vector3[] newShot = CalculateArray();
        float score = CalculateScore(newShot[newShot.Length - 1]); 
        float timeToHit = (target.position.x - transform.position.x) / (shotData.currentForce.Value * Mathf.Cos(radianAngleY)); 

        // create the new shot
        LineSetUp shot = lineManager.NewShot(newShot, score, timeToHit, shotData);

        ////------------------------------------------

        LineSetUp[] scoreArray = new LineSetUp[4];

        
        List<Ref<float>> variablesToChange = new List<Ref<float>>();
        variablesToChange.Add(shotData.currentForce);
        variablesToChange.Add(shotData.currentAngleY);

        float[] tweakStep = {1, -1 };
        bool done = false;
        do
        {
            int counter = 0;
            for (int i = 0; i < variablesToChange.Count; i++)
            {
                float currentParameter = variablesToChange[i].Value;
                for (int j = 0; j < tweakStep.Length; j++)
                {
                    variablesToChange[i].Value += tweakStep[j];
                    //print(variablesToChange[i].Value);

                    radianAngleY = Mathf.Deg2Rad * shotData.currentAngleY.Value;
                    radianAngleZ = Mathf.Deg2Rad * shotData.currentAngleZ.Value;

                    newShot = CalculateArray();
                    score = CalculateScore(newShot[newShot.Length - 1]) ; // the score of the new shot
                    timeToHit = (target.position.x - transform.position.x) / (shotData.currentForce.Value * Mathf.Cos(radianAngleY)); //the time needed to hit the ground

                    scoreArray[counter] = lineManager.NewShot(newShot, score, timeToHit,shotData);
                    counter++;
                    variablesToChange[i].Value = currentParameter;
                }
            }
            //print(CheckBest(scoreArray, shot) == -1);
            if (-1 != CheckBest(scoreArray, shot))
            {
                shot = scoreArray[CheckBest(scoreArray, shot)];
                variablesToChange[0].Value = shot.shotData.currentForce.Value;
                variablesToChange[1].Value = shot.shotData.currentAngleY.Value;
            }
            else
                done = true;
        } while (!done);
    }

    //returns -1 if currentshot is the best, the position of the best on the array otherwise
    private int CheckBest(LineSetUp[] _shotArray, LineSetUp currentShot)
    {
        int pos = -1;
        LineSetUp best = currentShot;
        for (int i = 0; i < _shotArray.Length; i++)
        {
            if (_shotArray[i].score == best.score && best.score == 0)
            {
                if (_shotArray[i].timeToHit < best.timeToHit)
                {
                    best = _shotArray[i];
                    pos = i;
                }
            }
            else
            {
                if (_shotArray[i].score < best.score)
                {
                    best = _shotArray[i];
                    pos = i;
                }
            }
                
        }

        return pos;
    }

    //calculates all the points of the parabola
    private Vector3[] CalculateArray()
    {
        //creates an array of points based on the given resolution
        Vector3[] parabolaPoints = new Vector3[pointsOnParabola + 1]; 

        //the maximun distance a projectile can get (physics equation)
        float maxDistance = (shotData.currentForce.Value * shotData.currentForce.Value * (1 + Mathf.Sqrt(1 + ((2 * g * transform.position.y) / (shotData.currentForce.Value * shotData.currentForce.Value * Mathf.Sin(radianAngleY) * Mathf.Sin(radianAngleY))))) * Mathf.Sin(2 * radianAngleY))/(2*g);

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
        float y = transform.position.y + x * Mathf.Tan(radianAngleY)-((g*x*x)/(2* shotData.currentForce.Value * shotData.currentForce.Value * Mathf.Cos(radianAngleY) * Mathf.Cos(radianAngleY))); // physics equation
        float z = transform.position.z + x * Mathf.Tan(radianAngleZ) - ((airForce * x * x) / (2 * shotData.currentForce.Value * shotData.currentForce.Value * Mathf.Cos(radianAngleZ) * Mathf.Cos(radianAngleZ)));
        return new Vector3(x, y, z);
    }

    //if the last point is between the target radius (0.6) the score is 0 else the score is the distance.
    private float CalculateScore(Vector3 shot)
    {
        return Vector3.Distance(shot, target.position) < 0.3f ? 0 : Vector3.Distance(shot, target.position);
    }

}

public class Ref<T> where T : struct
{
    public T Value { get; set; }
}
