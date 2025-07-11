using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UI.Stage;
public class HomeScreenManager : MonoBehaviour
{
    [Header("Home Screen UI Elements")]
    public Button enterButton;
    public Button stageSelectButton;
    public GameObject homeScreenUI;
    
    [Header("Stage Portal")]
    public GameObject portalVisual;
    public Text portalStageNameText;
    public Text portalStageDescriptionText;
    
    [Header("Animation")]
    public float fadeSpeed = 1.0f;
    public Animator portalAnimator;
    
    // Reference to Stage Selection Manager
    private StageSelectionManager stageSelectionManager;
    
    // Currently selected stage
    private int currentStageIndex = 0;
    
    // Accessible stages (unlocked)
    private List<bool> stageUnlocked = new List<bool>();
    
    void Start()
    {
        stageSelectionManager = FindObjectOfType<StageSelectionManager>();
        
        // Setup button listeners
        if (enterButton != null)
            Debug.Log("Enter Button found and set up.");
            enterButton.onClick.AddListener(EnterSelectedStage);
            
        if (stageSelectButton != null)
            stageSelectButton.onClick.AddListener(OpenStageSelectScreen);
            
        // Initialize unlocked stages
        InitializeStages();
        
        // Update the portal to show the current stage
        UpdatePortalDisplay();
    }
    
    private void InitializeStages()
    {
        // In a real game, this would likely be loaded from save data
        stageUnlocked = new List<bool>(10); // Assuming 10 stages total
        
        // At minimum, stage 1 is always unlocked
        stageUnlocked.Add(true);
        
        // Check PlayerPrefs or other save data for unlocked stages
        for (int i = 1; i < 10; i++)
        {
            bool isUnlocked = PlayerPrefs.GetInt("Stage_" + i + "_Unlocked", 0) == 1;
            stageUnlocked.Add(isUnlocked);
        }
        
        // Set current stage to the latest unlocked stage
        for (int i = stageUnlocked.Count - 1; i >= 0; i--)
        {
            if (stageUnlocked[i])
            {
                currentStageIndex = i;
                break;
            }
        }
    }
    
    public void EnterSelectedStage()
    {
        // Make sure the stage is unlocked
        if (currentStageIndex < stageUnlocked.Count && stageUnlocked[currentStageIndex])
        {
            // Play portal animation if available
            if (portalAnimator != null)
            {
                portalAnimator.SetTrigger("Enter");
                StartCoroutine(LoadStageAfterDelay(1.0f)); // Delay to allow animation to play
            }
            else
            {
                LoadSelectedStage();
            }
        }
        else
        {
            Debug.Log("Stage is locked!");
            // Could show a UI message here
        }
    }
    
    private IEnumerator LoadStageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadSelectedStage();
    }
    
    private void LoadSelectedStage()
    {
        // Stage scenes would be named like "Stage1", "Stage2", etc.
        SceneManager.LoadScene("Stage" + (currentStageIndex + 1));
    }
    
    public void OpenStageSelectScreen()
    {
        // Hide home screen
        homeScreenUI.SetActive(false);
        
        // Show stage selection screen
        if (stageSelectionManager != null)
        {
            stageSelectionManager.ShowStageSelection(currentStageIndex);
        }
    }
    
    public void ReturnFromStageSelection(int selectedStage)
    {
        // Update current stage if it's unlocked
        if (selectedStage < stageUnlocked.Count && stageUnlocked[selectedStage])
        {
            currentStageIndex = selectedStage;
        }
        
        // Show home screen
        homeScreenUI.SetActive(true);
        
        // Update portal display
        UpdatePortalDisplay();
    }
    
    private void UpdatePortalDisplay()
    {
        // Assuming we have a DataManager or similar that stores stage info
        StageData stageData = StageDataManager.GetStageData(currentStageIndex);
        
        if (stageData != null)
        {
            portalStageNameText.text = stageData.stageName;
            portalStageDescriptionText.text = stageData.stageDescription;
            
            // Could also update portal visuals based on stage
            // portalVisual.GetComponent<Image>().sprite = stageData.portalSprite;
        }
    }
    
    public void StageCleared(int stageIndex)
    {
        // Unlock the next stage
        if (stageIndex < stageUnlocked.Count - 1)
        {
            stageUnlocked[stageIndex + 1] = true;
            PlayerPrefs.SetInt("Stage_" + (stageIndex + 1) + "_Unlocked", 1);
            PlayerPrefs.Save();
            
            // Update current stage to next stage
            currentStageIndex = stageIndex + 1;
            UpdatePortalDisplay();
        }
    }
}
