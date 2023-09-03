using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public enum GoalType
    {
        Glass,
        Score,
        Gem,
        Box
    }

    public int LevelNumber;
    public List<GemSO> GemList;
    public int Width;
    public int Height;

    public int MoveAmount;
    public GoalType goalType;
    public int TargetScore;
    public GemSO TargetGem;
    public Sprite TargetGlassSprite;
    public Sprite TargetBoxSprite;

    public List<LevelGridPosition> LevelGridPositionList;
    // treba spremat samo ovo
    public int Stars = 0;
    public bool IsLocked;

    private void OnEnable()
    {
        //load level progress
        if (PlayerPrefs.HasKey("Stars" + LevelNumber.ToString())) Stars = PlayerPrefs.GetInt("Stars" + LevelNumber.ToString());
        if (PlayerPrefs.HasKey("isLocked" + LevelNumber.ToString())) IsLocked = PlayerPrefs.GetInt("isLocked" + LevelNumber.ToString()) == 1;
    }

    public void SetStars(int stars)
    {
        this.Stars = stars;
        PlayerPrefs.SetInt("Stars" + LevelNumber.ToString(), stars);
    }

    public void SetIsLocked(bool isLocked)
    {
        this.IsLocked = isLocked;
        PlayerPrefs.SetInt("isLocked" + LevelNumber.ToString(), isLocked ? 1 : 0);
    }

    [System.Serializable]
    public class LevelGridPosition
    {
        public GemSO gemSO;
        public int x;
        public int y;
        public bool hasGlass;
        public bool isBox;
        public bool isActive = true;
        public bool isRowSpecial = false;
        public bool isColSpecial = false;
    }
}