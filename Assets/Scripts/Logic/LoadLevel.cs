using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    [SerializeField] private LevelSO levelSO;
    private bool levelWon = false;
    private Sprite levelWallpaper;
    public static LoadLevel Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeSceneLevel()
    {
        SceneManager.LoadScene("Match3", LoadSceneMode.Single);
        PlayerPrefs.SetInt("Hearts", PlayerPrefs.GetInt("Hearts") - 1);
    }

    public void SetLevelSO(LevelSO level) => levelSO = level;
    public void ChangeSceneMenu() => SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    public LevelSO GetLevelSO() => levelSO;
    public bool GetIsLevelWon() => levelWon;
    public bool SetIsLevelWon(bool won) => levelWon = won;
    public void SetWallpaper(Sprite wallpaper) => levelWallpaper = wallpaper;
    public Sprite GetWallpaper() => levelWallpaper;
}