using System;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
	[SerializeField] private LevelSO levelSO;
	private int gridWidth;
	private int gridHeight;
	private Grid<GemGridPosition> grid;
	private int score;
	private int moveCount;
	private int targetGemDestroyed = 0;
	private int targetBoxDestroyed = 0;
	public event EventHandler OnGemGridPositionDestroyed;
	public event EventHandler<GemGrid> OnNewGemGridSpawned;
	public event EventHandler<GemGrid> OnNoPossibleMovesShuffle;
	public event EventHandler<OnLevelSetEventArgs> OnLevelSet;
	public event EventHandler OnGlassDestroyed;
	public event EventHandler OnBoxDestroyed;
	public event EventHandler OnMoveUsed;
	public event EventHandler OnOutOfMoves;
	public event EventHandler OnScoreChanged;
	public event EventHandler OnWin;
	public event EventHandler OnKeepPlaying;

	private void Start()
	{
		SetLevelSO(LoadLevel.Instance.GetLevelSO());
	}

	public void SetLevelSO(LevelSO levelSO)
	{
		this.levelSO = levelSO;
		score = 0;
		moveCount = levelSO.MoveAmount;
		gridWidth = levelSO.Width;
		gridHeight = levelSO.Height;
		grid = new Grid<GemGridPosition>(gridWidth, gridHeight, 1f, Vector3.zero, (Grid<GemGridPosition> g, int x, int y) => new GemGridPosition(g, x, y));

		for (int x = 0; x < gridWidth; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				var levelGridPosition = levelSO.LevelGridPositionList.Find(item => item.x == x && item.y == y);
				if (levelGridPosition != null)
				{
					if (levelGridPosition.isActive && !levelGridPosition.isBox)
					{
						GemSO gem = levelGridPosition.gemSO;
						GemGrid gemGrid = new GemGrid(gem, x, y);
						gemGrid.SetIsRowSpecial(levelGridPosition.isRowSpecial);
						gemGrid.SetIsRowSpecial(levelGridPosition.isColSpecial);
						grid.GetGridObject(x, y).SetGemGrid(gemGrid);
						grid.GetGridObject(x, y).SetHasGlass(levelGridPosition.hasGlass);
					}
					grid.GetGridObject(x, y).SetIsBox(levelGridPosition.isBox);
					grid.GetGridObject(x, y).SetIsActive(levelGridPosition.isActive);
				}
			}
		}
		OnLevelSet?.Invoke(this, new OnLevelSetEventArgs { levelSO = levelSO, grid = grid });
	}

	public void UseMove()
	{
		moveCount--;
		OnMoveUsed?.Invoke(this, EventArgs.Empty);
	}

	public void KeepPlaying()
	{
		moveCount = (int)levelSO.MoveAmount / 2;
		OnMoveUsed?.Invoke(this, null);
		OnKeepPlaying?.Invoke(this, null);
	}

	public bool CanSwapGridPositions(int startX, int startY, int endX, int endY)
	{
		if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return false; // Invalid Position
		if (startX == endX && startY == endY) return false; // Same Position
		SwapGridPositions(startX, startY, endX, endY);
		return HasMatch3Link(startX, startY) || HasMatch3Link(endX, endY);
	}

	public void SwapGridPositions(int startX, int startY, int endX, int endY)
	{
		if (!IsValidPosition(startX, startY) || !IsValidPosition(endX, endY)) return; // Invalid Position
		if (startX == endX && startY == endY) return; // Same Position

		GemGridPosition startGemGridPosition = grid.GetGridObject(startX, startY);
		GemGridPosition endGemGridPosition = grid.GetGridObject(endX, endY);

		GemGrid startGemGrid = startGemGridPosition.GetGemGrid();
		GemGrid endGemGrid = endGemGridPosition.GetGemGrid();

		startGemGrid.SetGemXY(endX, endY);
		endGemGrid.SetGemXY(startX, startY);

		startGemGridPosition.SetGemGrid(endGemGrid);
		endGemGridPosition.SetGemGrid(startGemGrid);
	}

	public bool HasMatch3Link(int x, int y)
	{
		List<GemGridPosition> linkedGemGridPositionList = GetMatch3Links(x, y);
		return linkedGemGridPositionList != null && linkedGemGridPositionList.Count >= 3;
	}

	// Checks given gem if it has match from each side
	public List<GemGridPosition> GetMatch3Links(int x, int y)
	{
		GemSO gemSO = GetGemSO(x, y);
		List<GemGridPosition> linkedGemGridPositionList = new List<GemGridPosition>();
		if (gemSO == null) return null;

		int rightLinkAmount = 0;
		for (int i = 1; i < gridWidth; i++)
		{
			if (IsValidPosition(x + i, y))
			{
				GemSO nextGemSO = GetGemSO(x + i, y);
				if (nextGemSO == gemSO) rightLinkAmount++; // Same Gem
				else break;
			}
			else break;
		}

		int leftLinkAmount = 0;
		for (int i = 1; i < gridWidth; i++)
		{
			if (IsValidPosition(x - i, y))
			{
				GemSO nextGemSO = GetGemSO(x - i, y);
				if (nextGemSO == gemSO) leftLinkAmount++;  // Same Gem
				else break;
			}
			else break;
		}

		int horizontalLinkAmount = 1 + leftLinkAmount + rightLinkAmount; // This Gem + left + right

		if (horizontalLinkAmount >= 3)
		{
			// Has 3 horizontal linked gems
			int leftMostX = x - leftLinkAmount;
			for (int i = 0; i < horizontalLinkAmount; i++)
			{
				linkedGemGridPositionList.Add(grid.GetGridObject(leftMostX + i, y));
				if (!((y + 1 < 0) || (y + 1 >= gridHeight)))
				{
					if (grid.GetGridObject(leftMostX + i, y + 1).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(leftMostX + i, y + 1));
					}
				}
				if (!((y - 1 < 0) || (y - 1 >= gridHeight)))
				{
					if (grid.GetGridObject(leftMostX + i, y - 1).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(leftMostX + i, y - 1));
					}
				}
				if (!((leftMostX + i - 1 < 0) || (leftMostX + i - 1 >= gridWidth)))
				{
					if (grid.GetGridObject(leftMostX + i - 1, y).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(leftMostX + i - 1, y));
					}
				}
				if (!((leftMostX + i + 1 < 0) || (leftMostX + i + 1 >= gridWidth)))
				{
					if (grid.GetGridObject(leftMostX + i + 1, y).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(leftMostX + i + 1, y));
					}
				}
			}
		}

		int upLinkAmount = 0;
		for (int i = 1; i < gridHeight; i++)
		{
			if (IsValidPosition(x, y + i))
			{
				GemSO nextGemSO = GetGemSO(x, y + i);
				if (nextGemSO == gemSO) upLinkAmount++; // Same Gem
				else break;
			}
			else break;
		}

		int downLinkAmount = 0;
		for (int i = 1; i < gridHeight; i++)
		{
			if (IsValidPosition(x, y - i))
			{
				GemSO nextGemSO = GetGemSO(x, y - i);
				if (nextGemSO == gemSO) downLinkAmount++;    // Same Gem
				else break;
			}
			else break;
		}

		int verticalLinkAmount = 1 + downLinkAmount + upLinkAmount; // This Gem + down + up

		if (verticalLinkAmount >= 3)
		{
			// Has 3 vertical linked gems
			int downMostY = y - downLinkAmount;
			for (int i = 0; i < verticalLinkAmount; i++)
			{
				linkedGemGridPositionList.Add(grid.GetGridObject(x, downMostY + i));
				if (!((x + 1 < 0) || (x + 1 >= gridWidth)))
				{
					if (grid.GetGridObject(x + 1, downMostY + i).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(x + 1, downMostY + i));
					}
				}
				if (!(x - 1 < 0 || x - 1 >= gridWidth))
				{
					if (grid.GetGridObject(x - 1, downMostY + i).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(x - 1, downMostY + i));
					}
				}
				if (!((downMostY + i - 1 < 0) || (downMostY + i - 1 >= gridHeight)))
				{
					if (grid.GetGridObject(x, downMostY + i - 1).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(x, downMostY + i - 1));
					}
				}
				if (!((downMostY + i + 1 < 0) || (downMostY + i + 1 >= gridHeight)))
				{
					if (grid.GetGridObject(x, downMostY + i + 1).GetIsBox())
					{
						linkedGemGridPositionList.Add(grid.GetGridObject(x, downMostY + i + 1));
					}
				}
			}
		}

		if (verticalLinkAmount >= 3 || horizontalLinkAmount >= 3) return linkedGemGridPositionList;

		// No links
		return null;
	}

	public bool TryFindMatchesAndDestroyThem()
	{
		List<List<GemGridPosition>> allLinkedGemGridPositionList = GetAllMatch3Links();

		bool foundMatch = false;
		List<Vector2Int> explosionGridPositionList = new();

		// For every match3 link
		foreach (List<GemGridPosition> linkedGemGridPositionList in allLinkedGemGridPositionList)
		{
			// If theres more than or 4 match create special row/column
			if (linkedGemGridPositionList.Count >= 4)
			{
				score += 200;
				// Row special gem
				if (linkedGemGridPositionList[0].GetWorldPosition().x == linkedGemGridPositionList[1].GetWorldPosition().x)
				{
					linkedGemGridPositionList[0].GetGemGrid().SetIsRowSpecial(true);
					linkedGemGridPositionList.RemoveAt(0);
				}
				// Column special gem
				else if (linkedGemGridPositionList[0].GetWorldPosition().y == linkedGemGridPositionList[1].GetWorldPosition().y)
				{
					linkedGemGridPositionList[0].GetGemGrid().SetIsColSpecial(true);
					linkedGemGridPositionList.RemoveAt(0);
				}
			}

			// If any gem was special row/column set for destroying
			foreach (GemGridPosition gemGridPosition in linkedGemGridPositionList)
			{
				if (gemGridPosition.GetIsActive() && gemGridPosition.HasGemGrid())
				{
					//Set up new linked list cijeli row i play animation
					if (gemGridPosition.GetGemGrid().GetIsRowSpecial())
					{
						int y = (Int32)gemGridPosition.GetWorldPosition().y;
						for (int i = 0; i < gridWidth; i++) explosionGridPositionList.Add(new Vector2Int(i, y));

						// For line renderer 
						int x = (Int32)gemGridPosition.GetWorldPosition().x;

						int explosionXfirst = 0;
						for (int firstExplosionGemX = 0; firstExplosionGemX < x; firstExplosionGemX++)
						{
							if (grid.GetGridObject(firstExplosionGemX, y).GetIsActive())
							{
								explosionXfirst = firstExplosionGemX;
								break;
							}
						}
						int explosionXlast = gridWidth - 1;
						for (int lastExplosionGemX = gridWidth - 1; lastExplosionGemX > x; lastExplosionGemX--)
						{
							if (grid.GetGridObject(lastExplosionGemX, y).GetIsActive())
							{
								explosionXlast = lastExplosionGemX;
								break;
							}
						}

						gemGridPosition.GetGemGrid().CallOnExplode(explosionXfirst, explosionXlast);
					}
					else if (gemGridPosition.GetGemGrid().GetIsColSpecial())
					{
						int x = (Int32)gemGridPosition.GetWorldPosition().x;
						for (int i = 0; i < gridHeight; i++) explosionGridPositionList.Add(new Vector2Int(x, i));

						int y = (Int32)gemGridPosition.GetWorldPosition().y;
						// For line renderer
						int explosionYfirst = 0;
						for (int firstExplosionGemY = 0; firstExplosionGemY < y; firstExplosionGemY++)
						{
							if (grid.GetGridObject(x, firstExplosionGemY).GetIsActive())
							{
								explosionYfirst = firstExplosionGemY;
								break;
							}
						}
						int explosionYlast = gridHeight - 1;
						for (int lastExplosionGemY = gridHeight - 1; lastExplosionGemY > x; lastExplosionGemY--)
						{
							if (grid.GetGridObject(x, lastExplosionGemY).GetIsActive())
							{
								explosionYlast = lastExplosionGemY;
								break;
							}
						}

						gemGridPosition.GetGemGrid().CallOnExplode(explosionYfirst, explosionYlast);
					}
				}
			}
			foreach (GemGridPosition gemGridPosition in linkedGemGridPositionList) TryDestroyGemGridPosition(gemGridPosition);
			foundMatch = true;
		}

		foreach (Vector2Int explosionGridPosition in explosionGridPositionList)
		{
			if (IsValidPosition(explosionGridPosition.x, explosionGridPosition.y))
			{
				GemGridPosition gemGridPosition = grid.GetGridObject(explosionGridPosition.x, explosionGridPosition.y);
				TryDestroyGemGridPosition(gemGridPosition);
			}
			else if (!(explosionGridPosition.x < 0 || explosionGridPosition.y < 0 || explosionGridPosition.x >= gridWidth || explosionGridPosition.y >= gridHeight))
			{
				GemGridPosition gemGridPosition = grid.GetGridObject(explosionGridPosition.x, explosionGridPosition.y);
				TryDestroyGemGridPosition(gemGridPosition);
			}
		}

		OnScoreChanged?.Invoke(this, EventArgs.Empty);
		return foundMatch;
	}

	private void TryDestroyGemGridPosition(GemGridPosition gemGridPosition)
	{
		if (gemGridPosition.HasGemGrid())
		{
			score += 100;

			if (levelSO.goalType == LevelSO.GoalType.Gem && levelSO.TargetGem == gemGridPosition.GetGemGrid().GetGem())
			{
				SetTargetGemDestroyed(1);
			}
			gemGridPosition.DestroyGem();
			OnGemGridPositionDestroyed?.Invoke(gemGridPosition, EventArgs.Empty);
			gemGridPosition.ClearGemGrid();
		}

		if (gemGridPosition.HasGlass())
		{
			score += 100;

			gemGridPosition.DestroyGlass();
			OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
		}
		if (gemGridPosition.GetIsBox())
		{
			score += 200;
			if (levelSO.goalType == LevelSO.GoalType.Box)
			{
				SetTargetBoxDestroyed(1);
			}
			gemGridPosition.DestroyBox();
			OnBoxDestroyed?.Invoke(this, EventArgs.Empty);
		}
	}

	public void SpawnNewMissingGridPositions()
	{
		for (int x = 0; x < gridWidth; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

				if (gemGridPosition.IsEmpty() && gemGridPosition.GetIsActive() && !gemGridPosition.GetIsBox())
				{
					GemSO gem = levelSO.GemList[UnityEngine.Random.Range(0, levelSO.GemList.Count)];
					GemGrid gemGrid = new(gem, x, y);

					gemGridPosition.SetGemGrid(gemGrid);

					OnNewGemGridSpawned?.Invoke(gemGrid, gemGrid);

				}
			}
		}
	}

	public void FallGemsIntoEmptyPositions()
	{
		for (int x = 0; x < gridWidth; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

				if (!gemGridPosition.IsEmpty() && gemGridPosition.GetIsActive())
				{
					// Grid Position has Gem
					for (int i = y - 1; i >= 0; i--)
					{
						GemGridPosition nextGemGridPosition = grid.GetGridObject(x, i);
						if (nextGemGridPosition.IsEmpty() && nextGemGridPosition.GetIsActive() && !nextGemGridPosition.GetIsBox())
						{
							gemGridPosition.GetGemGrid().SetGemXY(x, i);
							nextGemGridPosition.SetGemGrid(gemGridPosition.GetGemGrid());
							gemGridPosition.ClearGemGrid();

							gemGridPosition = nextGemGridPosition;
						}
						else if (!nextGemGridPosition.GetIsActive()) { }
						else if (nextGemGridPosition.GetIsBox()) { }
						else break;
					}
				}
			}
		}
	}

	public List<List<GemGridPosition>> GetAllMatch3Links()
	{
		// Finds all the links with the current grid
		List<List<GemGridPosition>> allLinkedGemGridPositionList = new();
		List<List<GemGridPosition>> removeFromList = new();

		// Finds match for every gem and filters out to unique lists
		for (int x = 0; x < gridWidth; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				if (HasMatch3Link(x, y))
				{
					List<GemGridPosition> linkedGemGridPositionList = GetMatch3Links(x, y);

					if (allLinkedGemGridPositionList.Count == 0) allLinkedGemGridPositionList.Add(linkedGemGridPositionList);
					else
					{
						bool uniqueNewLink = true;
						foreach (List<GemGridPosition> tmpLinkedGemGridPositionList in allLinkedGemGridPositionList)
						{
							if (linkedGemGridPositionList.Count == tmpLinkedGemGridPositionList.Count)
							{
								bool allTheSame = true;
								for (int i = 0; i < linkedGemGridPositionList.Count; i++)
								{
									if (linkedGemGridPositionList[i] != tmpLinkedGemGridPositionList[i])
									{
										allTheSame = false;
										break;
									}
								}
								if (allTheSame) uniqueNewLink = false;
							}

							else if (linkedGemGridPositionList.Count > tmpLinkedGemGridPositionList.Count)
							{
								bool allTheSame = true;
								for (int i = 0; i < tmpLinkedGemGridPositionList.Count; i++)
								{
									if (!linkedGemGridPositionList.Contains(tmpLinkedGemGridPositionList[i]))
									{
										allTheSame = false;
										break;
									}
								}
								if (allTheSame)
								{
									uniqueNewLink = true;
									removeFromList.Add(tmpLinkedGemGridPositionList);
								}
							}
							else if (linkedGemGridPositionList.Count < tmpLinkedGemGridPositionList.Count)
							{
								bool allTheSame = true;
								for (int i = 0; i < linkedGemGridPositionList.Count; i++)
								{
									if (!tmpLinkedGemGridPositionList.Contains(linkedGemGridPositionList[i]))
									{
										allTheSame = false;
										break;
									}
								}
								if (allTheSame) uniqueNewLink = false;
							}
						}
						// Add to the total list if it's a unique link
						if (uniqueNewLink) allLinkedGemGridPositionList.Add(linkedGemGridPositionList);

						for (int i = 0; i < removeFromList.Count; i++)
						{
							if (allLinkedGemGridPositionList.Contains(removeFromList[i]))
							{
								allLinkedGemGridPositionList.Remove(removeFromList[i]);
							}
						}
					}
				}
			}
		}
		return allLinkedGemGridPositionList;
	}

	public bool IsValidPosition(int x, int y)
	{
		return !(x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) && grid.GetGridObject(x, y).GetIsActive() && !grid.GetGridObject(x, y).GetIsBox();
	}

	public bool TryIsGameOver()
	{
		bool won = false;
		switch (levelSO.goalType)
		{
			default:
			case LevelSO.GoalType.Score:
				if (score >= levelSO.TargetScore) won = true;
				break;
			case LevelSO.GoalType.Glass:
				if (GetGlassAmount() <= 0) won = true;
				break;
			case LevelSO.GoalType.Gem:
				if (targetGemDestroyed >= levelSO.TargetScore) won = true;
				break;
			case LevelSO.GoalType.Box:
				if (targetBoxDestroyed >= levelSO.TargetScore) won = true;
				break;
		}
		if (won)
		{
			// Bonus Score for moves left
			int moves = moveCount;
			for (int i = 0; i < moves; i++)
			{
				score += 200;
				UseMove();
			}
			// Stars and currency reward
			if (GetScore() <= 500)
			{
				levelSO.SetStars(1);
				PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 5);
			}
			else if (GetScore() <= 1000)
			{
				levelSO.SetStars(2);
				PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 10);
			}
			else if (GetScore() > 1000)
			{
				PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 10);
				PlayerPrefs.SetInt("Gems", PlayerPrefs.GetInt("Gems") + 2);
				levelSO.SetStars(3);
			}
			OnWin?.Invoke(this, EventArgs.Empty);
		}

		if (!HasMoveAvailable() && !won)
		{
			// No more moves, game over
			OnOutOfMoves?.Invoke(this, EventArgs.Empty);
			return true;
		}
		// Not game over
		return won;
	}

	public bool BoosterDestroyGem(int x, int y)
	{
		if (IsValidPosition(x, y))
		{
			GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
			if (gemGridPosition.HasGemGrid())
			{
				score += 100;
				if (levelSO.goalType == LevelSO.GoalType.Gem && levelSO.TargetGem == gemGridPosition.GetGemGrid().GetGem())
				{
					SetTargetGemDestroyed(1);
				}
				gemGridPosition.DestroyGem();
				OnGemGridPositionDestroyed?.Invoke(gemGridPosition, EventArgs.Empty);
				gemGridPosition.ClearGemGrid();
				return true;
			}
		}
		return false;
	}

	public bool BoosterDestroyGlass(int x, int y)
	{
		if (IsValidPosition(x, y))
		{
			GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
			if (gemGridPosition.HasGlass())
			{
				score += 100;

				gemGridPosition.DestroyGlass();
				OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
				return true;
			}
		}
		return false;
	}

	public bool BoosterDestroyBox(int x, int y)
	{
		if (!(x < 0 || y < 0 || x >= gridWidth || y >= gridHeight))
		{
			GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
			if (gemGridPosition.GetIsBox())
			{
				score += 200;
				if (levelSO.goalType == LevelSO.GoalType.Box)
				{
					SetTargetBoxDestroyed(1);
				}
				gemGridPosition.DestroyBox();
				OnBoxDestroyed?.Invoke(this, EventArgs.Empty);
				return true;
			}
		}
		return false;
	}

	// For no possible moves shuffle
	public List<PossibleMove> GetAllPossibleMoves()
	{
		List<PossibleMove> allPossibleMovesList = new();

		for (int y = 0; y < gridHeight; y++)
		{
			for (int x = 0; x < gridWidth; x++)
			{
				// Test Swap: Left, Right, Up, Down
				List<PossibleMove> testPossibleMoveList = new List<PossibleMove>();
				testPossibleMoveList.Add(new PossibleMove(x, y, x - 1, y + 0));
				testPossibleMoveList.Add(new PossibleMove(x, y, x + 1, y + 0));
				testPossibleMoveList.Add(new PossibleMove(x, y, x + 0, y + 1));
				testPossibleMoveList.Add(new PossibleMove(x, y, x + 0, y - 1));

				for (int i = 0; i < testPossibleMoveList.Count; i++)
				{
					PossibleMove possibleMove = testPossibleMoveList[i];

					bool skipPossibleMove = false;

					for (int j = 0; j < allPossibleMovesList.Count; j++)
					{
						PossibleMove tmpPossibleMove = allPossibleMovesList[j];
						if (tmpPossibleMove.startX == possibleMove.startX &&
							tmpPossibleMove.startY == possibleMove.startY &&
							tmpPossibleMove.endX == possibleMove.endX &&
							tmpPossibleMove.endY == possibleMove.endY)
						{
							// Already tested 
							skipPossibleMove = true;
							break;
						}
						if (tmpPossibleMove.startX == possibleMove.endX &&
							tmpPossibleMove.startY == possibleMove.endY &&
							tmpPossibleMove.endX == possibleMove.startX &&
							tmpPossibleMove.endY == possibleMove.startY)
						{
							// Already tested
							skipPossibleMove = true;
							break;
						}
					}

					if (skipPossibleMove) continue;

					SwapGridPositions(possibleMove.startX, possibleMove.startY, possibleMove.endX, possibleMove.endY); // Swap

					List<List<GemGridPosition>> allLinkedGemGridPositionList = GetAllMatch3Links();

					if (allLinkedGemGridPositionList.Count > 0)
					{
						possibleMove.allLinkedGemGridPositionList = allLinkedGemGridPositionList;
						allPossibleMovesList.Add(possibleMove);
					}

					SwapGridPositions(possibleMove.startX, possibleMove.startY, possibleMove.endX, possibleMove.endY); // Swap Back
				}

			}
		}
		return allPossibleMovesList;
	}

	public void NoPossibleMovesShuffle()
	{
		for (int x = 0; x < gridWidth; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

				if (gemGridPosition.GetIsActive() && !gemGridPosition.GetIsBox())
				{
					if (!gemGridPosition.GetGemGrid().GetIsColSpecial() && !gemGridPosition.GetGemGrid().GetIsRowSpecial())
					{
						GemSO gem = levelSO.GemList[UnityEngine.Random.Range(0, levelSO.GemList.Count)];
						GemGrid gemGrid = new(gem, x, y);
						GemGrid oldGemGrid = gemGridPosition.GetGemGrid();
						OnNoPossibleMovesShuffle?.Invoke(gemGrid, oldGemGrid);
						gemGridPosition.SetGemGrid(gemGrid);
					}
				}
			}
		}
	}

	#region gettersAndSetters
	public LevelSO GetLevelSO() => levelSO;

	public int GetScore() => score;

	public int GetTargetScore() => levelSO.TargetScore - score < 0 ? 0 : levelSO.TargetScore - score;

	public bool HasMoveAvailable() => moveCount > 0;

	public int GetMoveCount() => moveCount;

	public int GetGlassAmount()
	{
		int glassAmount = 0;

		for (int x = 0; x < gridWidth; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				GemGridPosition gemGridPosition = grid.GetGridObject(x, y);
				if (gemGridPosition.HasGlass())
				{
					glassAmount++;
				}
			}
		}
		return glassAmount;
	}

	public void SetTargetGemDestroyed(int num) => targetGemDestroyed += num;

	public int GetTargetGemDestroyed() => levelSO.TargetScore - targetGemDestroyed;

	public void SetTargetBoxDestroyed(int num) => targetBoxDestroyed += num;

	public int GetTargetBoxDestroyed() => levelSO.TargetScore - targetBoxDestroyed;

	private GemSO GetGemSO(int x, int y)
	{
		if (!IsValidPosition(x, y)) return null;

		GemGridPosition gemGridPosition = grid.GetGridObject(x, y);

		if (gemGridPosition.GetGemGrid() == null) return null;

		return gemGridPosition.GetGemGrid().GetGem();
	}
	#endregion


	public class GemGridPosition
	{
		private GemGrid gemGrid;
		private Grid<GemGridPosition> grid;
		private bool hasGlass;
		private bool isActive;
		private bool isBox;
		private int x;
		private int y;
		public event EventHandler OnGlassDestroyed;
		public event EventHandler OnBoxDestroyed;

		public GemGridPosition(Grid<GemGridPosition> grid, int x, int y)
		{
			this.grid = grid;
			this.x = x;
			this.y = y;
		}

		public void SetGemGrid(GemGrid gemGrid)
		{
			this.gemGrid = gemGrid;
			grid.TriggerGridObjectChanged(x, y);
		}

		public Vector2 GetWorldPosition() => grid.GetWorldPosition(x, y);

		public GemGrid GetGemGrid() => gemGrid;

		public void ClearGemGrid() => gemGrid = null;

		public void DestroyGem()
		{
			gemGrid?.Destroy();
			grid.TriggerGridObjectChanged(x, y);
		}

		public bool HasGemGrid() => gemGrid != null;

		public bool IsEmpty() => gemGrid == null;

		public bool HasGlass() => hasGlass;

		public void SetHasGlass(bool hasGlass) => this.hasGlass = hasGlass;

		public void DestroyGlass()
		{
			SetHasGlass(false);
			OnGlassDestroyed?.Invoke(this, EventArgs.Empty);
		}

		public bool GetIsActive() => isActive;
		public void SetIsActive(bool active) => isActive = active;

		public bool GetIsBox() => isBox;
		public void SetIsBox(bool isBox) => this.isBox = isBox;

		public void DestroyBox()
		{
			SetIsBox(false);
			OnBoxDestroyed?.Invoke(this, EventArgs.Empty);
		}
	}


	public class GemGrid
	{
		private GemSO gem;
		private int x;
		private int y;
		private bool isRowSpecial;
		private bool isColSpecial;
		public event EventHandler OnDestroyed;
		public event EventHandler<Tuple<int, int>> OnExplode;

		public GemGrid(GemSO gem, int x, int y)
		{
			this.gem = gem;
			this.x = x;
			this.y = y;
		}

		public GemSO GetGem() => gem;

		public Vector3 GetWorldPosition() => new(x, y);

		public void SetGemXY(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public void Destroy() => OnDestroyed?.Invoke(this, EventArgs.Empty);

		public void SetIsRowSpecial(bool isRowSpecial) => this.isRowSpecial = isRowSpecial;

		public void SetIsColSpecial(bool isColSpecial) => this.isColSpecial = isColSpecial;

		public bool GetIsRowSpecial() => isRowSpecial;

		public bool GetIsColSpecial() => isColSpecial;

		public void CallOnExplode(int startPos, int EndPos) => OnExplode?.Invoke(this, new Tuple<int, int>(startPos, EndPos));
	}


	public class OnLevelSetEventArgs : EventArgs
	{
		public LevelSO levelSO;
		public Grid<GemGridPosition> grid;
	}

	public class PossibleMove
	{
		public int startX;
		public int startY;
		public int endX;
		public int endY;
		public List<List<GemGridPosition>> allLinkedGemGridPositionList;

		public PossibleMove(int startX, int startY, int endX, int endY)
		{
			this.startX = startX;
			this.startY = startY;
			this.endX = endX;
			this.endY = endY;
		}
	}
}