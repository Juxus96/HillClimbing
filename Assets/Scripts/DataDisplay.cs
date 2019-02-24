using TMPro;
using UnityEngine;

public class DataDisplay : MonoBehaviour
{
    public static DataDisplay instance;

    public Transform scoresPanel;
    public GameObject backgroundPanel;
    public GameObject scorePanelPrefab;
    public Transform goalTransform;

    public TextMeshProUGUI totalShotsText;
    public TextMeshProUGUI bestShotText;

    private int shotCounter = 1;

    private void Awake()
    {
        instance = this;
    }

    public void DisplayData(Shot shot)
    {
        Transform dataDisplay = Instantiate(scorePanelPrefab, scoresPanel).transform;
        dataDisplay.GetChild(0).GetComponent<TextMeshProUGUI>().text = shotCounter++ + "";
        dataDisplay.GetChild(1).GetComponent<TextMeshProUGUI>().text = shot.score.ToString("0.00") + "";
        dataDisplay.GetChild(2).GetComponent<TextMeshProUGUI>().text = shot.timeToHit.ToString("0.00");
        dataDisplay.GetChild(3).GetComponent<TextMeshProUGUI>().text = shot.shotData.currentForce.Value.ToString("0.00") + "";
        dataDisplay.GetChild(4).GetComponent<TextMeshProUGUI>().text = shot.shotData.currentAngleY.Value.ToString("0.00") + "";
        dataDisplay.GetChild(5).GetComponent<TextMeshProUGUI>().text = shot.shotData.currentAngleZ.Value.ToString("0.00") + "";

        TextMeshProUGUI resultText = dataDisplay.GetChild(6).GetComponent<TextMeshProUGUI>();
        if (shot.score == 0)
        {
            resultText.text = "Collision";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text = "No Collision";
            resultText.color = Color.red;
        }
    }

    public void ResetDataDisplay()
    {
        shotCounter = 1;
        foreach (Transform child in scoresPanel)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Toggles panel that displays data
    /// </summary>
    public void ToggleDataDisplay()
    {
        backgroundPanel.SetActive(!backgroundPanel.activeSelf);
    }

    public void DisplayTotalData(int shotCount, Shot bestShot)
    {
        totalShotsText.text = shotCount + "";
        bestShotText.text = bestShot.timeToHit.ToString("0.00");
    }

    /// <summary>
    /// Moves the goal in the given x position
    /// </summary>
    /// <param name="value"></param>
    public void MoveGoalHorizontally(float value)
    {
        goalTransform.position = new Vector3(value, goalTransform.position.y, goalTransform.position.z);
    }

    public void MoveGoalDepth(float value)
    {
        goalTransform.position = new Vector3(goalTransform.position.x, goalTransform.position.y, value);
    }
}
