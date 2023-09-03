using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 *   Number Keys 1-6 = Set Gem Type
 *   Right Click = Toggle Glass
 *   Keypad 0 = Toggle active
 *   Keypad 1 = Toggle Row Special Gem
 *   Keypad 2 = Toggle Column Special Gem
 *   Keypad 3 = Toggle Box
 * */

public class LevelEditor : MonoBehaviour
{

    [SerializeField] private LevelSO levelSO;
    [SerializeField] private Transform pfGemGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform pfBoxGridVisual;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Transform pfBackgroundGridVisual;

    private Grid<GridPosition> grid;

    private void Awake()
    {
        grid = new Grid<GridPosition>(levelSO.Width, levelSO.Height, 1f, Vector3.zero, (Grid<GridPosition> g, int x, int y) => new GridPosition(levelSO, g, x, y));

        levelText.text = levelSO.name;

        if (levelSO.LevelGridPositionList == null || levelSO.LevelGridPositionList.Count != levelSO.Width * levelSO.Height)
        {
            // Create new Level
            Debug.Log("Creating new level...");
            levelSO.LevelGridPositionList = new List<LevelSO.LevelGridPosition>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    GemSO gem = levelSO.GemList[Random.Range(0, levelSO.GemList.Count)];

                    LevelSO.LevelGridPosition levelGridPosition = new LevelSO.LevelGridPosition { x = x, y = y, gemSO = gem };

                    levelSO.LevelGridPositionList.Add(levelGridPosition);

                    CreateVisual(grid.GetGridObject(x, y), levelGridPosition);
                }
            }
        }
        else
        {
            // Load Level
            Debug.Log("Loading level...");
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {

                    LevelSO.LevelGridPosition levelGridPosition = null;

                    foreach (LevelSO.LevelGridPosition tmpLevelGridPosition in levelSO.LevelGridPositionList)
                    {
                        if (tmpLevelGridPosition.x == x && tmpLevelGridPosition.y == y)
                        {
                            levelGridPosition = tmpLevelGridPosition;
                            break;
                        }
                    }

                    if (levelGridPosition == null)
                    {
                        Debug.LogError("Error! Null!");
                    }

                    CreateVisual(grid.GetGridObject(x, y), levelGridPosition);
                }
            }
        }

        cameraTransform.position = new Vector3(grid.GetWidth() * .5f, grid.GetHeight() * .5f, cameraTransform.position.z);
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;
        grid.GetXY(mouseWorldPosition, out int x, out int y);

        if (IsValidPosition(x, y))
        {
            if (grid.GetGridObject(x, y).GetIsActive())
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[0]);
                if (Input.GetKeyDown(KeyCode.Alpha2)) grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[1]);
                if (Input.GetKeyDown(KeyCode.Alpha3)) grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[2]);
                if (Input.GetKeyDown(KeyCode.Alpha4)) grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[3]);
                if (Input.GetKeyDown(KeyCode.Alpha5)) grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[4]);    
                if (Input.GetKeyDown(KeyCode.Alpha6)) grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[5]);
                if (Input.GetMouseButtonDown(1)) grid.GetGridObject(x, y).SetHasGlass(!grid.GetGridObject(x, y).GetHasGlass());
                if (Input.GetKeyDown(KeyCode.Keypad3))
                {
                    grid.GetGridObject(x, y).SetIsBox(!grid.GetGridObject(x, y).GetIsBox());
                    if (grid.GetGridObject(x, y).GetIsBox())
                    {
                        grid.GetGridObject(x, y).SetHasGlass(false);
                        grid.GetGridObject(x, y).RemoveGem();
                    }
                    else
                    {
                        grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[0]);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                grid.GetGridObject(x, y).SetIsActive(!grid.GetGridObject(x, y).GetIsActive());
                if (grid.GetGridObject(x, y).GetIsActive())
                {
                    grid.GetGridObject(x, y).SetGemSO(levelSO.GemList[0]);
                }
                else
                {
                    grid.GetGridObject(x, y).RemoveGem();
                    grid.GetGridObject(x, y).SetHasGlass(false);
                    grid.GetGridObject(x, y).SetIsBox(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                //Row
                if (grid.GetGridObject(x, y).GetHasGem())
                {
                    grid.GetGridObject(x, y).SetIsRowSpecial(!grid.GetGridObject(x, y).GetIsRowSpecial());
                }
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                //Column
                if (grid.GetGridObject(x, y).GetHasGem())
                {
                    grid.GetGridObject(x, y).SetIsColSpecial(!grid.GetGridObject(x, y).GetIsColSpecial());
                }
            }
        }
    }

    private void CreateVisual(GridPosition gridPosition, LevelSO.LevelGridPosition levelGridPosition)
    {
        gridPosition.GlassGO = Instantiate(pfGlassGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity).gameObject;
        gridPosition.BackgroundGO = Instantiate(pfBackgroundGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity).gameObject;
        gridPosition.GemSpriteRenderer = Instantiate(pfGemGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity).Find("sprite").GetComponent<SpriteRenderer>(); ;
        gridPosition.BoxGO = Instantiate(pfBoxGridVisual, gridPosition.GetWorldPosition(), Quaternion.identity).gameObject;
        gridPosition.LevelGridPosition = levelGridPosition;

        if (levelGridPosition.isActive && !levelGridPosition.isBox)
        {
            gridPosition.SetGemSO(levelGridPosition.gemSO);
            if (levelGridPosition.isRowSpecial) gridPosition.SetIsRowSpecial(true);
            else if (levelGridPosition.isColSpecial) gridPosition.SetIsColSpecial(true);
        }
        else gridPosition.RemoveGem();

        gridPosition.SetIsActive(levelGridPosition.isActive);
        gridPosition.SetHasGlass(levelGridPosition.hasGlass);
        gridPosition.SetIsBox(levelGridPosition.isBox);
    }

    private bool IsValidPosition(int x, int y)
    {
        if (x < 0 || y < 0 || x >= grid.GetWidth() || y >= grid.GetHeight()) return false;
        else return true;
    }

    private class GridPosition
    {
        public LevelSO.LevelGridPosition LevelGridPosition;
        public SpriteRenderer GemSpriteRenderer;
        public GameObject BackgroundGO;
        public GameObject GlassGO;
        public GameObject BoxGO;
        private LevelSO levelSO;
        private Grid<GridPosition> grid;
        private int x;
        private int y;

        public GridPosition(LevelSO levelSO, Grid<GridPosition> grid, int x, int y)
        {
            this.levelSO = levelSO;
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public void RemoveGem()
        {
            GemSpriteRenderer.sprite = null;
            LevelGridPosition.gemSO = null;
        }

        public void SetGemSO(GemSO gemSO)
        {
            GemSpriteRenderer.sprite = gemSO.Sprite;
            LevelGridPosition.gemSO = gemSO;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public void SetHasGlass(bool hasGlass)
        {
            LevelGridPosition.hasGlass = hasGlass;
            GlassGO.SetActive(hasGlass);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public void SetIsActive(bool active)
        {
            LevelGridPosition.isActive = active;
            BackgroundGO.SetActive(active);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public void SetIsRowSpecial(bool isRowSpecial)
        {
            LevelGridPosition.isRowSpecial = isRowSpecial;
            if (isRowSpecial)
            {
                GemSpriteRenderer.sprite = LevelGridPosition.gemSO.RowSprite;
                LevelGridPosition.isColSpecial = false;
            }
            else
            {
                GemSpriteRenderer.sprite = LevelGridPosition.gemSO.Sprite;
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public void SetIsColSpecial(bool isColSpecial)
        {
            LevelGridPosition.isColSpecial = isColSpecial;
            if (isColSpecial)
            {
                GemSpriteRenderer.sprite = LevelGridPosition.gemSO.ColumnSprite;
                LevelGridPosition.isRowSpecial = false;
            }
            else
            {
                GemSpriteRenderer.sprite = LevelGridPosition.gemSO.Sprite;
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public void SetIsBox(bool isBox)
        {
            LevelGridPosition.isBox = isBox;
            BoxGO.SetActive(isBox);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(levelSO);
#endif
        }

        public Vector3 GetWorldPosition() => grid.GetWorldPosition(x, y);
        public bool GetIsBox() => LevelGridPosition.isBox;
        public bool GetHasGlass() => LevelGridPosition.hasGlass;
        public bool GetIsActive() => LevelGridPosition.isActive;
        public bool GetIsRowSpecial() => LevelGridPosition.isRowSpecial;
        public bool GetIsColSpecial() => LevelGridPosition.isColSpecial;
        public bool GetHasGem() => LevelGridPosition.gemSO != null;
    }
}