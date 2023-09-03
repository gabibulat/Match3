using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Match3UI : MonoBehaviour
{
    [SerializeField] private Match3 match3;
    [SerializeField] private GameObject loseWindow;
    [SerializeField] private Image gameBackground;
    [Header("HUD")]
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text scoreText;

    [Header("WinWindow")]
    [SerializeField] private GameObject winWindow;
    [SerializeField] private TMP_Text winScoreText;
    [SerializeField] private List<Image> starsImages;
    [SerializeField] private GameObject coinGO;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private GameObject gemGO;
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private Sprite fullStar;
    [SerializeField] private Sprite emptyStar;

    [Header("Sounds")]
    [SerializeField] private AudioSource winSound;
    [SerializeField] private AudioSource loseSound;

    private LevelSO levelSO;
    [SerializeField] private GameObject shufflingWindow;

    private void Awake()
    {
        gameBackground.sprite = LoadLevel.Instance.GetWallpaper();
        match3.OnLevelSet += (object sender, Match3.OnLevelSetEventArgs e) =>
        {
            levelSO = match3.GetLevelSO();
            UpdateText();
        };
        match3.OnMoveUsed += Match3_OnTargetTextChanged;
        match3.OnGlassDestroyed += Match3_OnTargetTextChanged;
        match3.OnBoxDestroyed += Match3_OnTargetTextChanged;
        match3.OnScoreChanged += Match3_OnTargetTextChanged;
        match3.OnOutOfMoves += Match3_OnOutOfMoves;
        match3.OnWin += Match3_OnWin;
        match3.OnNoPossibleMovesShuffle += Match3_OnNoPossibleMovesShuffle;
    }

    private void Match3_OnNoPossibleMovesShuffle(object sender, Match3.GemGrid e)
    {
        shufflingWindow.SetActive(true);
        StartCoroutine(ShufflingWindow());
    }

    private IEnumerator ShufflingWindow()
    {
        yield return new WaitForSeconds(2);
        shufflingWindow.SetActive(false);
    }

    private void Match3_OnWin(object sender, System.EventArgs e)
    {
        winWindow.SetActive(true);
        winSound.Play();
        winScoreText.text = "Score: " + match3.GetScore().ToString();
        // Display reward coins and gold
        if (levelSO.Stars == 1)
        {
            coinGO.SetActive(true);
            coinText.text = "5";
        }
        else if (levelSO.Stars == 2)
        {
            coinGO.SetActive(true);
            coinText.text = "10";
        }
        else
        {
            coinGO.SetActive(true);
            coinText.text = "10";
            gemGO.SetActive(true);
            gemText.text = "2";
        }

        for (int i = 0; i < levelSO.Stars; i++) starsImages[i].sprite = fullStar;
        for (int i = levelSO.Stars; i < 3; i++) starsImages[i].sprite = emptyStar;
        LoadLevel.Instance.SetIsLevelWon(true);
    }

    private void Match3_OnOutOfMoves(object sender, System.EventArgs e)
    {
        loseWindow.SetActive(true);
        loseSound.Play();
    }
    private void Match3_OnTargetTextChanged(object sender, System.EventArgs e) => UpdateText();

    private void UpdateText()
    {
        movesText.text = match3.GetMoveCount().ToString();
        scoreText.text = match3.GetScore().ToString();
        switch (levelSO.goalType)
        {
            default:
            case LevelSO.GoalType.Glass:
                targetText.text = match3.GetGlassAmount().ToString();
                targetImage.sprite = levelSO.TargetGlassSprite;
                targetImage.gameObject.SetActive(true);
                break;
            case LevelSO.GoalType.Score:
                targetText.text = match3.GetTargetScore().ToString();
                targetImage.gameObject.SetActive(false);
                break;
            case LevelSO.GoalType.Gem:
                if (match3.GetTargetGemDestroyed() < 0) targetText.text = "0";
                else targetText.text = match3.GetTargetGemDestroyed().ToString();
                targetImage.sprite = levelSO.TargetGem.Sprite;
                targetImage.gameObject.SetActive(true);
                break;
            case LevelSO.GoalType.Box:
                if (match3.GetTargetBoxDestroyed() < 0) targetText.text = "0";
                else targetText.text = match3.GetTargetBoxDestroyed().ToString();
                targetImage.sprite = levelSO.TargetBoxSprite;
                targetImage.gameObject.SetActive(true);
                break;
        }
    }

    public void QuitLevel()
    {
        LoadLevel.Instance.SetIsLevelWon(false);
        LoadLevel.Instance.ChangeSceneMenu();
    }

    public void ReturnToMenuAfterWin()
    {
        PlayerPrefs.SetInt("Hearts", PlayerPrefs.GetInt("Hearts") + 1);
        LoadLevel.Instance.ChangeSceneMenu();
    }
}