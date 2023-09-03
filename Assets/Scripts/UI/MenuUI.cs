using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
	[Header("Levels")]
	[SerializeField] private List<LevelSO> levelsSO;
	[SerializeField] private List<GameObject> levelsGO;
	[SerializeField] private Sprite fullStar;
	[SerializeField] private Sprite emptyStar;

	[Header("LevelWindow")]
	[SerializeField] private GameObject loadLevelMenu;
	[SerializeField] private List<Image> loadLevelStars;
	[SerializeField] private TMP_Text levelTitle;
	[SerializeField] private TMP_Text goalDescription;
	[SerializeField] private Image goalDescriptionImage;
	[SerializeField] private Button playButton;

	[Header("HUD")]
	[SerializeField] static TMP_Text heartText;
	[SerializeField] static TMP_Text coinText;
	[SerializeField] static TMP_Text gemText;

	private void Start()
	{
		if (LoadLevel.Instance.GetIsLevelWon())
		{
			int i = levelsSO.IndexOf(LoadLevel.Instance.GetLevelSO());
			if (i + 1 < levelsSO.Count) levelsSO[i + 1].SetIsLocked(false);
		}

		for (int i = 0; i < levelsGO.Count; i++)
		{
			Button candyButton = levelsGO[i].GetComponent<Button>();
			GameObject lockedGO = levelsGO[i].transform.GetChild(1).gameObject;
			GameObject starsGO = levelsGO[i].transform.GetChild(0).gameObject;

			// If level unlocked
			if (!levelsSO[i].IsLocked)
			{
				lockedGO.SetActive(false);
				candyButton.interactable = true;

				// Stars
				for (int j = 0; j < levelsSO[i].Stars; j++) starsGO.transform.GetChild(j).GetComponent<Image>().sprite = fullStar;
				for (int j = levelsSO[i].Stars + 1; j < 3; j++) starsGO.transform.GetChild(j).GetComponent<Image>().sprite = emptyStar;
			}
		}

		heartText = GameObject.FindGameObjectWithTag("HeartText").GetComponent<TMP_Text>();
		// Hearts
		if (!PlayerPrefs.HasKey("Hearts")) PlayerPrefs.SetInt("Hearts", 5);
		heartText.text = PlayerPrefs.GetInt("Hearts").ToString();

        // Currency
        coinText = GameObject.FindGameObjectWithTag("CoinText").GetComponent<TMP_Text>();
		if (!PlayerPrefs.HasKey("Coins")) PlayerPrefs.SetInt("Coins", 0);
		coinText.text = PlayerPrefs.GetInt("Coins").ToString();

		gemText = GameObject.FindGameObjectWithTag("GemText").GetComponent<TMP_Text>();
		if (!PlayerPrefs.HasKey("Gems")) PlayerPrefs.SetInt("Gems", 0);
		gemText.text = PlayerPrefs.GetInt("Gems").ToString();


        // TESTING
        // PlayerPrefs.SetInt("Hearts", 5);
        // PlayerPrefs.SetInt("Gems", 30);
        // PlayerPrefs.SetInt("Coins", 100);
    }

    public void SetUpLoadLevel(LevelSO levelSO)
	{
		// Tilte
		levelTitle.text = "Level " + levelSO.LevelNumber.ToString();

		// Stars
		for (int j = 0; j < levelSO.Stars; j++) loadLevelStars[j].sprite = fullStar;
		for (int j = levelSO.Stars; j < 3; j++) loadLevelStars[j].sprite = emptyStar;

		// GoalDescription
		switch (levelSO.goalType)
		{
			case LevelSO.GoalType.Score:
				goalDescriptionImage.gameObject.SetActive(false);
				goalDescription.text = "Score " + levelSO.TargetScore.ToString() + "!";
				break;
			case LevelSO.GoalType.Gem:
				goalDescriptionImage.sprite = levelSO.TargetGem.Sprite;
				goalDescriptionImage.gameObject.SetActive(true);
				goalDescription.text = "Collect " + levelSO.TargetScore.ToString() + " gems!";
				break;
			case LevelSO.GoalType.Glass:
				goalDescriptionImage.sprite = levelSO.TargetGlassSprite;
				goalDescriptionImage.gameObject.SetActive(true);
				goalDescription.text = "Break " + levelSO.TargetScore.ToString() + " glass blocks!";
				break;
			case LevelSO.GoalType.Box:
				goalDescriptionImage.sprite = levelSO.TargetBoxSprite;
				goalDescriptionImage.gameObject.SetActive(true);
				goalDescription.text = "Break " + levelSO.TargetScore.ToString() + " boxes!";
				break;
		}

		loadLevelMenu.SetActive(true);
        LoadLevel.Instance.SetLevelSO(levelSO);
		playButton.interactable = CanPlay();
		playButton.onClick.AddListener(LoadLevel.Instance.ChangeSceneLevel);
	}

	private bool CanPlay() => PlayerPrefs.GetInt("Hearts") > 0;

	public void CloseLevelWinow()
	{
		loadLevelMenu.SetActive(false);
		playButton.onClick.RemoveAllListeners();
	}

	public static void UpdateTextHUD()
	{
		heartText.text = PlayerPrefs.GetInt("Hearts").ToString();
		gemText.text = PlayerPrefs.GetInt("Gems").ToString();
		coinText.text = PlayerPrefs.GetInt("Coins").ToString();
	}
}