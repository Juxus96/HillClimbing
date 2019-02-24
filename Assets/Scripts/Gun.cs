using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    
    private float gravity;
    private LineManager lineManager;

    [Header("Visuals")]
    [Tooltip("The resolution of the parabola")]
    public int pointsOnParabola;
    public Transform target;
    public GameObject cannonBall;
    
    [Space]
    [Header("Variables to optimize")]
    public float airForce;
    public float test;

    [Space]
    [Header("Brute force range")]
    public float minAngleY;
    public float maxAngleY;
    public float minAngleZ;
    public float maxAngleZ;
    public float minForce;
    public float maxForce;

    private void Awake()
    {
        // Get the absolute value of the force of gravity in order to calculate shots
        gravity = Mathf.Abs(Physics.gravity.y); 
        lineManager = GetComponent<LineManager>();
    }

    /// <summary>
    /// Clears the previous shots
    /// </summary>
    private void ClearShots()
    {
        lineManager.bestShot = null;
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Brutes forces shots looping through the established ranges
    /// </summary>
    public void BruteForce()
    {
        // Reset Data
        ClearShots();
        DataDisplay.instance.ResetDataDisplay();
        

        // Create shot data
        ShotData shotData = new ShotData();
        shotData.currentAngleY = new Ref<float>();
        shotData.currentAngleZ = new Ref<float>();
        shotData.currentForce = new Ref<float>();

        shotData.currentAngleY.Value = minAngleY;
        shotData.currentAngleZ.Value = minAngleZ;

        float radianAngleY;
        float radianAngleZ;

        Shot bestShot = null;
        int totalShots = 0;

        // Angle Loop
        while (shotData.currentAngleZ.Value < maxAngleZ - 1f)
        {
            shotData.currentAngleY.Value = minAngleY;
            while (shotData.currentAngleY.Value < maxAngleY - 1f)
            {
                radianAngleY = Mathf.Deg2Rad * shotData.currentAngleY.Value;
                radianAngleZ = Mathf.Deg2Rad * shotData.currentAngleZ.Value;
                shotData.currentForce.Value = minForce;

                // Force Loop
                while (shotData.currentForce.Value < maxForce - 1f)
                {
                    Vector3[] newShot = CalculateArray(shotData, radianAngleY, radianAngleZ);
                    // Calculate the score of the given shot
                    float score = CalculateScore(newShot[newShot.Length - 1]);
                    // Calculate time to hit the ground
                    float timeToHit = (target.position.x - transform.position.x) / (shotData.currentForce.Value * Mathf.Cos(radianAngleY));

                    // Create new shot with the given data
                    Shot currentShot = lineManager.NewShot(newShot, score, timeToHit, shotData);
                    DataDisplay.instance.DisplayData(currentShot);

                    // Saves the best shot
                    if (!bestShot || (bestShot.score >= currentShot.score && bestShot.timeToHit > currentShot.timeToHit))
                        bestShot = currentShot;

                    totalShots++;
                    shotData.currentForce.Value += 1f;
                }
                shotData.currentAngleY.Value += 1f;
            }
            shotData.currentAngleZ.Value += 1f;
        }

        DataDisplay.instance.DisplayTotalData(totalShots, bestShot);
    }
    
    /// <summary>
    /// Hill Climbing Algorith that shoots the best shot at the end
    /// </summary>
    public void HillClimbing()
    {
        // Reset from previous shot
        ClearShots();
        DataDisplay.instance.ResetDataDisplay();

        // Create shot data
        ShotData shotData = new ShotData();
        shotData.currentAngleY = new Ref<float>();
        shotData.currentAngleZ = new Ref<float>();
        shotData.currentForce = new Ref<float>();

        // Random values
        shotData.currentAngleY.Value = Random.Range(minAngleY, maxAngleY);
        shotData.currentAngleZ.Value = Random.Range(minAngleZ, maxAngleZ);
        shotData.currentForce.Value = Random.Range(minForce, maxForce);

        // Radian Angles
        float radianAngleY = Mathf.Deg2Rad * shotData.currentAngleY.Value;
        float radianAngleZ = Mathf.Deg2Rad * shotData.currentAngleZ.Value;

        // Initial shot
        Vector3[] newShot = CalculateArray(shotData, radianAngleY, radianAngleZ);
        float score = CalculateScore(newShot[newShot.Length - 1]); 
        float timeToHit = (target.position.x - transform.position.x) / (shotData.currentForce.Value * Mathf.Cos(radianAngleY)); 

        // New Shot with initial random values
        Shot shot = lineManager.NewShot(newShot, score, timeToHit, shotData);

        // Algorith Loops
        int totalShots = 0;

        List<Ref<float>> variablesToChange = new List<Ref<float>>();
        variablesToChange.Add(shotData.currentForce);
        variablesToChange.Add(shotData.currentAngleY);
        variablesToChange.Add(shotData.currentAngleZ);

        float[] tweakStep = {1, -1 };

        Shot[] scoreArray = new Shot[variablesToChange.Count*tweakStep.Length];
        bool done = false;
        do
        {
            int counter = 0;
            for (int i = 0; i < variablesToChange.Count; i++)
            {
                float currentParameter = variablesToChange[i].Value;
                for (int j = 0; j < tweakStep.Length; j++, totalShots++)
                {
                    variablesToChange[i].Value += tweakStep[j];

                    radianAngleY = Mathf.Deg2Rad * shotData.currentAngleY.Value;
                    radianAngleZ = Mathf.Deg2Rad * shotData.currentAngleZ.Value;

                    newShot = CalculateArray(shotData, radianAngleY, radianAngleZ);
                    // Calculate the score of the given shot
                    score = CalculateScore(newShot[newShot.Length - 1]) ;
                    // Calculate time to hit the ground
                    timeToHit = (target.position.x - transform.position.x) / (shotData.currentForce.Value * Mathf.Cos(radianAngleY)); 

                    Shot currentShot = lineManager.NewShot(newShot, score, timeToHit, shotData);
                    scoreArray[counter] = currentShot;
                    DataDisplay.instance.DisplayData(currentShot);
                    counter++;
                    
                    variablesToChange[i].Value = currentParameter;
                }
            }

            if (-1 != CheckBest(scoreArray, shot))
            {
                shot = scoreArray[CheckBest(scoreArray, shot)];
                variablesToChange[0].Value = shot.shotData.currentForce.Value;
                variablesToChange[1].Value = shot.shotData.currentAngleY.Value;
                variablesToChange[2].Value = shot.shotData.currentAngleZ.Value;
            }
            else done = true;
        } while (!done);

        DataDisplay.instance.DisplayTotalData(totalShots, shot);
        // Shoot best projectile
        ShootProjectile(shot);
    }

    /// <summary>
    /// Shoots the given projectile physically
    /// </summary>
    /// <param name="shot"></param>
    private void ShootProjectile(Shot shot)
    {
        GameObject projectile = Instantiate(cannonBall, transform.position, cannonBall.transform.rotation);
        float bestShotAngleY = Mathf.Deg2Rad * shot.shotData.currentAngleY.Value;
        projectile.AddComponent<Rigidbody>().AddForce(new Vector3(Mathf.Cos(bestShotAngleY), Mathf.Sin(bestShotAngleY), Mathf.Sin(shot.shotData.currentAngleZ.Value * Mathf.Deg2Rad)) * shot.shotData.currentForce.Value * 50);
        Destroy(projectile, 5);
    }

    /// <summary>
    /// Returns -1 if currentshot is the best, the position of the best on the array otherwise
    /// </summary>
    /// <param name="_shotArray"></param>
    /// <param name="currentShot"></param>
    /// <returns></returns>
    private int CheckBest(Shot[] _shotArray, Shot currentShot)
    {
        int pos = -1;
        Shot best = currentShot;
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

    /// <summary>
    /// Returns the points of the parabola with the given parameters
    /// </summary>
    /// <param name="shotData"></param>
    /// <param name="radianAngleY"></param>
    /// <param name="radianAngleZ"></param>
    /// <returns></returns>
    private Vector3[] CalculateArray(ShotData shotData, float radianAngleY, float radianAngleZ)
    {
        //creates an array of points based on the given resolution
        Vector3[] parabolaPoints = new Vector3[pointsOnParabola + 1]; 

        //the maximun distance a projectile can get (physics equation)
        float maxDistance = (shotData.currentForce.Value * shotData.currentForce.Value * (1 + Mathf.Sqrt(1 + ((2 * gravity * transform.position.y) / (shotData.currentForce.Value * shotData.currentForce.Value * Mathf.Sin(radianAngleY) * Mathf.Sin(radianAngleY))))) * Mathf.Sin(2 * radianAngleY))/(2*gravity);

        //find the coordinates for each point of the parabola
        for (int i = 0; i <= pointsOnParabola; i++)
        {
            float distanceBetweenPoints = i / (float)pointsOnParabola;
            parabolaPoints[i] = calculateParabolaPoint(distanceBetweenPoints,maxDistance, shotData, radianAngleY, radianAngleZ);

        }

        return parabolaPoints;
    }

    /// <summary>
    /// Returns the parabola point with the given parameters
    /// </summary>
    /// <param name="distanceBetweenPoints"></param>
    /// <param name="maxDistance"></param>
    /// <param name="shotData"></param>
    /// <param name="radianAngleY"></param>
    /// <param name="radianAngleZ"></param>
    /// <returns></returns>
    Vector3 calculateParabolaPoint(float distanceBetweenPoints, float maxDistance, ShotData shotData, float radianAngleY, float radianAngleZ)
    {
        // Calculating point through physics
        float x = transform.position.x + distanceBetweenPoints * maxDistance;
        float y = transform.position.y + x * Mathf.Tan( radianAngleY ) - (( gravity * x * x )/( 2 * shotData.currentForce.Value * shotData.currentForce.Value * Mathf.Cos(radianAngleY) * Mathf.Cos(radianAngleY)));
        float z = transform.position.z + x * Mathf.Tan( radianAngleZ ) - (( airForce * x * x) / ( 2 * shotData.currentForce.Value * shotData.currentForce.Value * Mathf.Cos(radianAngleZ) * Mathf.Cos(radianAngleZ)));
        return new Vector3(x, y, z);
    }

    //if the last point is between the target radius (0.3) the score is 0 else the score is the distance.
    private float CalculateScore(Vector3 shot)
    {
        float distance = Vector3.Distance(shot, target.position);
        return distance < 0.3f ? 0 : distance;
    }
}

[System.Serializable]
public class Ref<T> where T : struct
{
    public T Value { get; set; }
}
