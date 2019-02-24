using UnityEngine;

public class LineManager : MonoBehaviour {
 
    // Best shot
    public GameObject bestShot;


    public GameObject LineRendererPrefab;
    public Material hitMat, missMat, bestMat;
	
    // Creates a new parabola sets the color and the points with the given data before returning the shot
	public Shot NewShot(Vector3[] points, float score, float timeToHit, ShotData shotData)
    {
        GameObject newLine = Instantiate(LineRendererPrefab, transform);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        newLine.GetComponent<Shot>().SetUpLine(points,score, timeToHit, shotData);

        // If the shot hit the target set the line to green 
        if (score == 0)
        {
            lineRenderer.material = hitMat;
            lineRenderer.endWidth *= 2f;
        }
        // Otherwise just paint it red
        else
        {
            lineRenderer.material = missMat;
            lineRenderer.startWidth *= 0.5f;
            lineRenderer.endWidth *= 0.7f;
        }
            
        // If no best shot has been set (first time) then set it as best shot
        if (bestShot == null)
        {
            bestShot = newLine;
            lineRenderer.material = bestMat;
        }
        // If the current shot is the best then also set it as best shot
        else if (IsNewBest(newLine))
        {
            bestShot.GetComponent<LineRenderer>().material = bestShot.GetComponent<Shot>().score == 0 ? hitMat : missMat;
            bestShot = newLine;
            lineRenderer.material = bestMat;
        }

        return newLine.GetComponent<Shot>();
    }

    // Returns if the given shot is better than the best shot
    private bool IsNewBest(GameObject shot)
    {
        Shot bslsu = bestShot.GetComponent<Shot>();
        Shot slsu = shot.GetComponent<Shot>();

        if (slsu.score == bslsu.score && bslsu.score == 0)
            return slsu.timeToHit < bslsu.timeToHit;
        else
            return (slsu.score < bslsu.score);
    }
}
