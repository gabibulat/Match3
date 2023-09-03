using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Match3;

public class Match3Visual : MonoBehaviour
{
    [Header("Match3Visual")]
    [SerializeField] private Transform pfGemGridVisual;
    [SerializeField] private Transform pfGlassGridVisual;
    [SerializeField] private Transform pfBoxGridVisual;
    [SerializeField] private Transform pfBackgroundGridVisual;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform gridObjectParent;
    [SerializeField] private Match3 match3;

    [Header("Match3Sounds")]
    [SerializeField] private AudioSource swipeSound;
    [SerializeField] private AudioSource cantSwipeSound;
    [SerializeField] private AudioSource matchSound;
    [SerializeField] private AudioSource glassBreakSound;
    [SerializeField] private AudioSource boxBreakSound;

    private Grid<GemGridPosition> grid;
    private Dictionary<GemGrid, GemGridVisual> gemGridDictionary;
    private Dictionary<GemGridPosition, GlassGridVisual> glassGridDictionary;
    private Dictionary<GemGridPosition, BoxGridVisual> boxGridDictionary;
    private bool isSetup;
    private State state;
    private float busyTimer;
    private Action onBusyTimerElapsedAction;
    private int startDragX;
    private int startDragY;
    private BoosterItem.BoosterType selectedBoosterType;
    private float waitingForUserTimer = 5f;

    public EventHandler OnBoosterFinished;

    public enum State
    {
        Busy,
        WaitingForUser,
        TryFindMatches,
        GameOver,
        BoosterSelecting
    }

    private void Awake()
    {
        state = State.Busy;
        isSetup = false;
        match3.OnLevelSet += Match3_OnLevelSet;
    }

    private void Match3_OnLevelSet(object sender, OnLevelSetEventArgs e)
    {
        this.match3 = sender as Match3;
        this.grid = e.grid;

        float cameraYOffset = -1f;
        cameraTransform.position = new Vector3(grid.GetWidth() * .5f, grid.GetHeight() * .5f + cameraYOffset, cameraTransform.position.z);

        match3.OnGemGridPositionDestroyed += Match3_OnGemGridPositionDestroyed;
        match3.OnNewGemGridSpawned += Match3_OnNewGemGridSpawned;
        match3.OnNoPossibleMovesShuffle += Match3_OnNoPossibleMovesShuffle;
        match3.OnKeepPlaying += Match3_OnKeepPlaying;

        // Initialize Visual
        gemGridDictionary = new();
        glassGridDictionary = new();
        boxGridDictionary = new();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

                if (gemGridPosition.GetIsActive())
                {
                    GemGrid gemGrid = gemGridPosition.GetGemGrid();

                    Vector3 position = new Vector3(x, -12);
                    if (!gemGridPosition.GetIsBox())
                    {
                        // Gem Visual Transform
                        Transform gemGridVisualTransform = Instantiate(pfGemGridVisual, position, Quaternion.identity, gridObjectParent);
                        GemGridVisual gemGridVisual = new GemGridVisual(gemGridVisualTransform, gemGrid,
                            gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>(),
                            gemGridVisualTransform.Find("SpecalGemEffect").GetComponent<LineRenderer>());

                        gemGridDictionary[gemGrid] = gemGridVisual;
                        gemGrid.OnExplode += GemGrid_OnExplode;

                        // Glass Visual Transform
                        Transform glassGridVisualTransform = Instantiate(pfGlassGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity, gridObjectParent);
                        GlassGridVisual glassGridVisual = new(glassGridVisualTransform, gemGridPosition, glassBreakSound);
                        glassGridDictionary[gemGridPosition] = glassGridVisual;
                    }
                    else
                    {
                        Transform boxGridVisualTransform = Instantiate(pfBoxGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity, gridObjectParent);
                        BoxGridVisual boxGridVisual = new(boxGridVisualTransform, gemGridPosition, boxBreakSound);
                        boxGridDictionary[gemGridPosition] = boxGridVisual;
                    }

                    // Background Grid Visual
                    Instantiate(pfBackgroundGridVisual, grid.GetWorldPosition(x, y), Quaternion.identity, gridObjectParent);
                }
            }
        }
        SetBusyState(.5f, () => SetState(State.TryFindMatches));
        isSetup = true;
    }

    private void Match3_OnKeepPlaying(object sender, EventArgs e)
    {
        SetBusyState(.5f, () => SetState(State.TryFindMatches));
    }

    private void Match3_OnNoPossibleMovesShuffle(object sender, GemGrid oldGemGrid)
    {
        GemGrid newGemGrid = sender as GemGrid;

        gemGridDictionary[oldGemGrid].PlaySwapAnimation();

        gemGridDictionary.Remove(oldGemGrid);

        Vector3 position = newGemGrid.GetWorldPosition();

        Transform gemGridVisualTransform = Instantiate(pfGemGridVisual, position, Quaternion.identity, gridObjectParent);
        GemGridVisual gemGridVisual = new(gemGridVisualTransform, newGemGrid,
            gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>(),
            gemGridVisualTransform.Find("SpecalGemEffect").GetComponent<LineRenderer>());
        gemGridDictionary[newGemGrid] = gemGridVisual;
        newGemGrid.OnExplode += GemGrid_OnExplode;
        SetBusyState(.5f, () => SetState(State.TryFindMatches));
    }

    private void Match3_OnNewGemGridSpawned(object sender, GemGrid gemGrid)
    {
        Vector3 position = gemGrid.GetWorldPosition();
        position = new Vector3(position.x, 12);

        Transform gemGridVisualTransform = Instantiate(pfGemGridVisual, position, Quaternion.identity, gridObjectParent);
        GemGridVisual gemGridVisual = new(gemGridVisualTransform, gemGrid,
            gemGridVisualTransform.Find("sprite").GetComponent<SpriteRenderer>(),
            gemGridVisualTransform.Find("SpecalGemEffect").GetComponent<LineRenderer>());
        gemGridDictionary[gemGrid] = gemGridVisual;
        gemGrid.OnExplode += GemGrid_OnExplode;
    }

    private void Match3_OnGemGridPositionDestroyed(object sender, EventArgs e)
    {
        if (sender is GemGridPosition gemGridPosition && gemGridPosition.GetGemGrid() != null)
        {
            gemGridDictionary.Remove(gemGridPosition.GetGemGrid());
        }
        matchSound.Play();
    }

    private void Update()
    {
        if (!isSetup) return;
        UpdateVisual();

        switch (state)
        {
            case State.Busy:
                busyTimer -= Time.deltaTime;
                if (busyTimer <= 0f)
                {
                    onBusyTimerElapsedAction();
                }
                break;
            case State.WaitingForUser:
                // Check if there are possible moves after some time passes
                waitingForUserTimer -= Time.deltaTime;
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPosition.z = 0f;
                    grid.GetXY(mouseWorldPosition, out startDragX, out startDragY);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPosition.z = 0f;
                    grid.GetXY(mouseWorldPosition, out int x, out int y);

                    // Different X
                    if (x != startDragX)
                    {
                        y = startDragY;
                        if (x < startDragX) x = startDragX - 1;
                        else x = startDragX + 1;
                    }
                    // Different Y
                    else
                    {
                        x = startDragX;
                        if (y < startDragY) y = startDragY - 1;
                        else y = startDragY + 1;
                    }

                    if (match3.CanSwapGridPositions(startDragX, startDragY, x, y))
                    {
                        SwapGridPositions(startDragX, startDragY, x, y);
                        waitingForUserTimer = 5f;
                        swipeSound.Play();
                    }
                    else
                    {
                        match3.SwapGridPositions(startDragX, startDragY, x, y);
                        // Play not match animation
                        if (match3.IsValidPosition(x, y))
                        {
                            gemGridDictionary[grid.GetGridObject(x, y).GetGemGrid()].NotMatchAnimation();
                            cantSwipeSound.Play();
                        }
                        if (match3.IsValidPosition(startDragX, startDragY))
                        {
                            gemGridDictionary[grid.GetGridObject(startDragX, startDragY).GetGemGrid()].NotMatchAnimation();
                            cantSwipeSound.Play();
                        }
                    }
                }
                if (waitingForUserTimer <= 0)
                {
                    waitingForUserTimer = 5f;
                    if (match3.GetAllPossibleMoves().Count == 0)
                    {
                        match3.NoPossibleMovesShuffle();
                    }
                }
                break;
            case State.TryFindMatches:
                if (match3.TryFindMatchesAndDestroyThem())
                {
                    SetBusyState(.3f, () =>
                    {
                        match3.FallGemsIntoEmptyPositions();

                        SetBusyState(.3f, () =>
                        {
                            match3.SpawnNewMissingGridPositions();

                            SetBusyState(.5f, () => SetState(State.TryFindMatches));
                        });
                    });
                }
                else
                {
                    if (match3.TryIsGameOver()) SetState(State.GameOver);
                    else SetState(State.WaitingForUser);
                }
                break;
            case State.BoosterSelecting:
                if (selectedBoosterType != BoosterItem.BoosterType.None)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        mouseWorldPosition.z = 0f;
                        grid.GetXY(mouseWorldPosition, out int x, out int y);
                        switch (selectedBoosterType)
                        {
                            case BoosterItem.BoosterType.DestroyGem:
                                if (match3.BoosterDestroyGem(x, y))
                                {
                                    SetBusyState(.3f, () =>
                                    {
                                        match3.FallGemsIntoEmptyPositions();

                                        SetBusyState(.3f, () =>
                                        {
                                            match3.SpawnNewMissingGridPositions();

                                            SetBusyState(.5f, () => SetState(State.TryFindMatches));
                                        });
                                    });
                                    OnBoosterFinished?.Invoke(this, null);
                                }
                                break;
                            case BoosterItem.BoosterType.DestroyGlass:

                                if (match3.BoosterDestroyGlass(x, y))
                                {
                                    SetBusyState(.3f, () =>
                                    {
                                        match3.FallGemsIntoEmptyPositions();

                                        SetBusyState(.3f, () =>
                                        {
                                            match3.SpawnNewMissingGridPositions();

                                            SetBusyState(.5f, () => SetState(State.TryFindMatches));
                                        });
                                    });
                                    OnBoosterFinished?.Invoke(this, null);
                                }
                                break;
                            case BoosterItem.BoosterType.DestroyBox:
                                if (match3.BoosterDestroyBox(x, y))
                                {
                                    SetBusyState(.3f, () =>
                                    {
                                        match3.FallGemsIntoEmptyPositions();

                                        SetBusyState(.3f, () =>
                                        {
                                            match3.SpawnNewMissingGridPositions();

                                            SetBusyState(.5f, () => SetState(State.TryFindMatches));
                                        });
                                    });
                                    OnBoosterFinished?.Invoke(this, null);
                                }
                                break;
                        }
                    }
                }
                break;
            case State.GameOver:
                break;
        }
    }

    private void UpdateVisual()
    {
        foreach (GemGrid gemGrid in gemGridDictionary.Keys)
        {
            gemGridDictionary[gemGrid].Update();
        }
    }

    public void SwapGridPositions(int startX, int startY, int endX, int endY)
    {
        match3.UseMove();
        SetBusyState(.5f, () => SetState(State.TryFindMatches));
    }

    private void SetBusyState(float busyTimer, Action onBusyTimerElapsedAction)
    {
        SetState(State.Busy);
        this.busyTimer = busyTimer;
        this.onBusyTimerElapsedAction = onBusyTimerElapsedAction;
    }

    public void SetState(State state) => this.state = state;

    private void GemGrid_OnExplode(object sender, Tuple<int, int> size)
    {
        if (sender is Match3.GemGrid gem)
        {
            if (gem.GetIsRowSpecial())
            {
                StartCoroutine(gemGridDictionary[gem].SpecialGemRowEffect(size.Item1, size.Item2));
            }
            else
            {
                StartCoroutine(gemGridDictionary[gem].SpecialGemColEffect(size.Item1, size.Item2));
            }
        }
    }

    public void Pause(bool pause) => isSetup = !pause;

    public void SetSelectedBoosterType(BoosterItem.BoosterType selectedBoosterType) => this.selectedBoosterType = selectedBoosterType;
  
    public BoosterItem.BoosterType GetSelectedBoosterType() => selectedBoosterType;

    public class GemGridVisual
    {
        private Transform transform;
        private GemGrid gemGrid;
        private Animation animation;
        private SpriteRenderer spriteRenderer;
        private LineRenderer lineRenderer;

        public GemGridVisual(Transform transform, GemGrid gemGrid, SpriteRenderer spriteRenderer, LineRenderer lineRenderer)
        {
            this.transform = transform;
            this.gemGrid = gemGrid;
            this.spriteRenderer = spriteRenderer;
            this.lineRenderer = lineRenderer;
            animation = transform.GetComponent<Animation>();

            gemGrid.OnDestroyed += GemGrid_OnDestroyed;
        }

        public void SetGemGrid(GemGrid gemGrid)
        {
            this.gemGrid = gemGrid;
            spriteRenderer.sprite = gemGrid.GetGem().Sprite;
            gemGrid.OnDestroyed += GemGrid_OnDestroyed;
        }

        private void GemGrid_OnDestroyed(object sender, EventArgs e)
        {
            animation.Play("GemGridVisualMatched");
            Destroy(transform.gameObject, 1f);
        }

        public void PlaySwapAnimation()
        {
            animation.Play("GemGridVisualMatched");
            Destroy(transform.gameObject, 0.8f);
        }

        public void Update()
        {
            Vector3 targetPosition = gemGrid.GetWorldPosition();
            Vector3 moveDir = (targetPosition - transform.position);
            float moveSpeed = 10f;
            transform.position += moveSpeed * Time.deltaTime * moveDir;
            // Sprites
            if (gemGrid.GetIsColSpecial()) spriteRenderer.sprite = gemGrid.GetGem().ColumnSprite;
            else if (gemGrid.GetIsRowSpecial()) spriteRenderer.sprite = gemGrid.GetGem().RowSprite;
            else spriteRenderer.sprite = gemGrid.GetGem().Sprite;
        }

        public void NotMatchAnimation() => animation.Play("GemGridVisualNotMatch");

        public IEnumerator SpecialGemRowEffect(int first, int last)
        {
            lineRenderer.SetPosition(0, new Vector3(-first, transform.position.y + 0.4f, 0));
            lineRenderer.SetPosition(1, new Vector3(last, transform.position.y + 0.4f, 0));
            lineRenderer.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            lineRenderer.gameObject.SetActive(false);
        }
        public IEnumerator SpecialGemColEffect(int first, int last)
        {
            lineRenderer.SetPosition(0, new Vector3(transform.position.x + 0.6f, -first, 0));
            lineRenderer.SetPosition(1, new Vector3(transform.position.x + 0.6f, last, 0));
            lineRenderer.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            lineRenderer.gameObject.SetActive(false);
        }
    }

    public class GlassGridVisual
    {
        private Transform transform;
        private GemGridPosition gemGridPosition;
        private AudioSource glassBreakSound;

        public GlassGridVisual(Transform transform, Match3.GemGridPosition gemGridPosition, AudioSource glassBreakSound)
        {
            this.transform = transform;
            this.gemGridPosition = gemGridPosition;
            this.glassBreakSound = glassBreakSound;
            transform.gameObject.SetActive(gemGridPosition.HasGlass());
            gemGridPosition.OnGlassDestroyed += GemGridPosition_OnGlassDestroyed;
        }

        private void GemGridPosition_OnGlassDestroyed(object sender, EventArgs e)
        {
            transform.gameObject.SetActive(gemGridPosition.HasGlass());
            glassBreakSound.Play();
        }
    }

    public class BoxGridVisual
    {
        private Transform transform;
        private GemGridPosition gemGridPosition;
        private Animation animation;
        private AudioSource boxBreakSound;

        public BoxGridVisual(Transform transform, Match3.GemGridPosition gemGridPosition, AudioSource boxBreakSound)
        {
            this.transform = transform;
            this.gemGridPosition = gemGridPosition;
            this.boxBreakSound = boxBreakSound;
            animation = transform.GetComponent<Animation>();
            transform.gameObject.SetActive(gemGridPosition.GetIsBox());
            gemGridPosition.OnBoxDestroyed += GemGridPosition_OnBoxDestroyed;
        }

        private void GemGridPosition_OnBoxDestroyed(object sender, EventArgs e)
        {
            animation.Play("GemGridVisualMatched");
            Destroy(transform.gameObject, 1f);
            boxBreakSound.Play();
        }
    }
}