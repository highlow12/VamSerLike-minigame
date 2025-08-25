using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StageSelectManager : Singleton<StageSelectManager>
{
    int currentStageIndex = 1;
    GameObject[] StageImages;
    int StageImagesOffset = 7;
    Vector3[] targetPositions; // Store target positions for smooth movement
    float lerpSpeed = 5f; // Speed of interpolation

    #region UI Buttons
    public void SelectButton()
    {
        StageLoadManager.Instance.LoadSceneAsync("Stage " + currentStageIndex);
    }
    public void LeftButton()
    {
        if (currentStageIndex > 1)
        {
            currentStageIndex--;
            UpdateTargetPositions();
        }
    }
    public void RightButton()
    {
        if (currentStageIndex < StageImages.Length)
        {
            currentStageIndex++;
            UpdateTargetPositions();
        }
    }
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        // Find all stage images
        StageImages = GameObject.FindGameObjectsWithTag("StageImage");
        
        // Sort them by name to ensure proper order
        System.Array.Sort(StageImages, (a, b) =>
        {
            int numA = int.Parse(new string(a.name.Where(char.IsDigit).ToArray()));
            int numB = int.Parse(new string(b.name.Where(char.IsDigit).ToArray()));
            return numA.CompareTo(numB);
        });
        
        // Initialize target positions
        targetPositions = new Vector3[StageImages.Length];
        UpdateTargetPositions();
    }

    void UpdateTargetPositions()
    {
        for (int i = 0; i < StageImages.Length; i++)
        {
            // Calculate the target position based on currentStageIndex
            float xPos = (i - (currentStageIndex - 1)) * StageImagesOffset;
            targetPositions[i] = new Vector3(xPos, StageImages[i].transform.localPosition.y, StageImages[i].transform.localPosition.z);
        }
    }

    void Update()
    {
        // Smoothly move each image to its target position
        for (int i = 0; i < StageImages.Length; i++)
        {
            // Check the distance to the target position
            if (Vector3.Distance(StageImages[i].transform.localPosition, targetPositions[i]) < 0.01f)
            {
                // Snap to the target position if close enough
                StageImages[i].transform.localPosition = targetPositions[i];
            }
            else
            {
                // Otherwise, interpolate smoothly
                StageImages[i].transform.localPosition = Vector3.Lerp(
                    StageImages[i].transform.localPosition,
                    targetPositions[i],
                    Time.deltaTime * lerpSpeed
                );
            }
        }
    }
    #endregion
}
