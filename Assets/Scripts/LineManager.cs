using UnityEngine;

public class LineManager : MonoBehaviour {
 
    // Best shot
    public GameObject bestShot;



    public GameObject LineRendererPrefab;
    public Material hitMat, missMat, bestMat;
	
    //creates a new parabola and sets the color and the points
	public LineSetUp NewShot(Vector3[] points, float score, float timeToHit, ShotData shotData)
    {
        GameObject newLine = Instantiate(LineRendererPrefab, transform);
        LineRenderer lr = newLine.GetComponent<LineRenderer>();

        newLine.GetComponent<LineSetUp>().SetUpLine(points,score, timeToHit, shotData);
        lr.material = score == 0 ? hitMat : missMat;

        if (bestShot == null)
        {
            bestShot = newLine;
            lr.material = bestMat;
        }
        else if (IsNewBest(newLine))
        {
            bestShot.GetComponent<LineRenderer>().material = bestShot.GetComponent<LineSetUp>().score == 0 ? hitMat : missMat;
            bestShot = newLine;
            lr.material = bestMat;
        }

        return newLine.GetComponent<LineSetUp>();
    }

    //if the shot hit compares the time it get to hit, else compares the score of the hit
    private bool IsNewBest(GameObject shot)
    {
        LineSetUp bslsu = bestShot.GetComponent<LineSetUp>();
        LineSetUp slsu = shot.GetComponent<LineSetUp>();

        if (slsu.score == bslsu.score && bslsu.score == 0)
            return slsu.timeToHit < bslsu.timeToHit;
        else
            return (slsu.score < bslsu.score);
    }
}
