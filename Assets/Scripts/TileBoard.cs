using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TileBoard : MonoBehaviour {

    protected Sprite[] minesweeperSprites;

    private bool bClickToFlag = false;

    public UnityEngine.UI.Image numLeft, numRight, faceTile, clickTile;

    public Sprite face_Default, face_Win, face_Lose, flag, mine;

    public enum GameState
    {
        Uninitialized,
        Playing,
        Win,
        GameOver
    }

    protected GameState gameState;

    public GameState State
    {
        get
        {
            return gameState;
        }
    }

    public int iHiddenTileCount = 0;

    public int iClickButton = 0;
    public int iFlagButton = 1;

    public void SwitchClickAndFlagButtons()
    {
        int iTemp = iFlagButton;
        iFlagButton = iClickButton;
        iClickButton = iTemp;
        bClickToFlag = !bClickToFlag;
        if (bClickToFlag) clickTile.sprite = flag;
        else clickTile.sprite = mine;
    }

    public int iTileXSize, iTileYSize, iMineCount;

    protected Tile[,] tiles;

    public Tile this[int x, int y]
    {
        get { return tiles[x, y]; }
        set { tiles[x, y] = value; }
    }

    private static TileBoard _instance;

    public static TileBoard Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gO = new GameObject();
                _instance = gO.AddComponent<TileBoard>();
            }
            return _instance;
        }
    }


    private int iRealMinesFlagged;
    private int iFlagsUsed;

    public int MinesRemaining
    {
        get
        {
            return iMineCount - iFlagsUsed;
        }
    }

    // Use this for initialization
    void Start ()
    { 
        _instance = this;
        DontDestroyOnLoad(_instance);
        gameState = GameState.Uninitialized;
        minesweeperSprites = Resources.LoadAll<Sprite>("Sprites/MinesweeperSpriteSheet");
        StartGame();
    }
	
    private void GameOver()
    {
        gameState = GameState.GameOver;
        faceTile.sprite = face_Lose;
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(iClickButton))
        {
            if (gameState == GameState.Playing)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                int x = (int)Mathf.Round(pos.x / 0.16f);
                int y = (int)Mathf.Round(pos.y / 0.16f);
                if (x >= tiles.GetLength(0) || y >= tiles.GetLength(1))
                    return;
                if (tiles[x, y].tileState == Tile.TileState.Opened) return;
                if (!ClickTile(x, y))
                {
                    GameOver();
                }
                else
                {
                    if (iHiddenTileCount == (iMineCount - iRealMinesFlagged))
                    {
                        gameState = GameState.Win;
                        faceTile.sprite = face_Win;
                    }
                    else
                    {
                        SetTileDetails();
                        //AI.Instance.Move();
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(iFlagButton))
        {
            if (gameState == GameState.Playing)
            {
                if (iFlagsUsed >= iMineCount) return;
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                int x = (int)Mathf.Round(pos.x / 0.16f);
                int y = (int)Mathf.Round(pos.y / 0.16f);
                if (x >= tiles.GetLength(0) || y >= tiles.GetLength(1) || x < 0 || y < 0)
                    return;
                if (tiles[x, y].tileState == Tile.TileState.Inactive)
                {
                    tiles[x, y].Flag();
                    iHiddenTileCount--;
                    iFlagsUsed++;
                    if (tiles[x, y].bIsMine)
                    {
                        iRealMinesFlagged++;
                        if (iRealMinesFlagged == iMineCount)
                        {
                            gameState = GameState.Win;
                            faceTile.sprite = face_Win;
                            numRight.sprite = minesweeperSprites[0];
                            numLeft.sprite = minesweeperSprites[0];
                            return;
                        }
                    }
                }
                else if (tiles[x, y].tileState == Tile.TileState.Flagged)
                {
                    tiles[x, y].UnFlag();
                    iFlagsUsed--;
                    iHiddenTileCount++;
                    if (tiles[x, y].bIsMine)
                        iRealMinesFlagged--;
                }
                SetTileDetails();
                numLeft.sprite = minesweeperSprites[((iMineCount - iFlagsUsed) / 10) % 10];
                numRight.sprite = minesweeperSprites[(iMineCount - iFlagsUsed) % 10];
                //AI.Instance.Move();
            }
        }
    }

    public void SimClick(int x, int y, int flag)
    {
        if (flag == 0)
        {
            if (x >= tiles.GetLength(0) || y >= tiles.GetLength(1))
                return;
            if (tiles[x, y].tileState == Tile.TileState.Opened) return;
            if (!ClickTile(x, y))
            {
                GameOver();
            }
            else
            {
                if (iHiddenTileCount == (iMineCount - iRealMinesFlagged))
                {
                    gameState = GameState.Win;
                    faceTile.sprite = face_Win;
                }
                else
                {
                    SetTileDetails();
                    //AI.Instance.Move();
                }
            }
        }
        else if (flag == 1)
        {
            if (x >= tiles.GetLength(0) || y >= tiles.GetLength(1) || x < 0 || y < 0)
                return;
            if (tiles[x, y].tileState == Tile.TileState.Inactive)
            {
                tiles[x, y].Flag();
                iFlagsUsed++;
                if (tiles[x, y].bIsMine)
                {
                    iRealMinesFlagged++;
                    if (iRealMinesFlagged == iMineCount)
                    {
                        gameState = GameState.Win;
                        faceTile.sprite = face_Win;
                        numRight.sprite = minesweeperSprites[0];
                        numLeft.sprite = minesweeperSprites[0];
                        return;
                    }
                }
            }
            else if (tiles[x, y].tileState == Tile.TileState.Flagged)
            {
                tiles[x, y].UnFlag();
                iFlagsUsed--;
                if (tiles[x, y].bIsMine)
                    iRealMinesFlagged--;
            }
            else if (tiles[x, y].tileState == Tile.TileState.Opened)
            {
                return;
            }
            SetTileDetails();
            numLeft.sprite = minesweeperSprites[((iMineCount - iFlagsUsed) / 10) % 10];
            numRight.sprite = minesweeperSprites[(iMineCount - iFlagsUsed) % 10];
            //AI.Instance.Move();
        }
    }

    public void StartGame()
    {
        gameState = GameState.Uninitialized;
        AI.Instance.ClearTargets();
        if (tiles != null)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Destroy(tiles[x, y].gameObject);
                    tiles[x, y] = null;
                }
            }
        }

        tiles = new Tile[iTileXSize, iTileYSize];
        for (int x = 0; x < iTileXSize; x++)
        {
            for (int y = 0; y < iTileYSize; y++)
            {
                tiles[x, y] = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Tile")).GetComponent<Tile>();
                tiles[x, y].xPos = x;
                tiles[x, y].yPos = y;
                tiles[x, y].gameObject.transform.position = new Vector3(0.16f * x, 0.16f * y);
            }
        }
        int iMinesCreated = 0;
        iHiddenTileCount = iTileXSize * iTileYSize;
        System.Random rand = new System.Random();
        while (iMinesCreated < iMineCount)
        {
            int iX = rand.Next(0, iTileXSize - 1);
            int iY = rand.Next(0, iTileYSize - 1);
            if (tiles[iX, iY].bIsMine) continue;
            tiles[iX, iY].bIsMine = true;
            iMinesCreated++;
        }
        SetInitialTileDetails();
        iRealMinesFlagged = 0;
        iFlagsUsed = 0;
        numLeft.sprite = minesweeperSprites[((iMineCount / 10) % 10)];
        numRight.sprite = minesweeperSprites[(iMineCount % 10)];
        gameState = GameState.Playing;
        faceTile.sprite = face_Default;
    }

    protected void SetInitialTileDetails()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y].bTileResolved = false;
                if (tiles[x, y].bIsMine) continue;
                if (CheckIsMine(x - 1, y)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x, y - 1)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x - 1, y - 1)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x, y + 1)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x + 1, y)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x + 1, y + 1)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x - 1, y + 1)) tiles[x, y].iNearbyBombs++;
                if (CheckIsMine(x + 1, y - 1)) tiles[x, y].iNearbyBombs++;
            }
        }
    }

    protected bool CheckIfTileIsHidden(int x, int y)
    {
        if (x < 0 || y < 0 || x >= tiles.GetLength(0) || y >= tiles.GetLength(1)) return false;
        return tiles[x,y].tileState == Tile.TileState.Inactive;
    }

    protected bool CheckIfTileIsFlagged(int x, int y)
    {
        if (x < 0 || y < 0 || x >= tiles.GetLength(0) || y >= tiles.GetLength(1)) return false;
        return tiles[x, y].tileState == Tile.TileState.Flagged;
    }

    protected void SetTileDetails()
    {
        //Update tiles' number of hidden tiles and nearby flagged tiles
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y].highlightColor = Color.white;
                tiles[x, y].linked = false;
                if (tiles[x, y].iNearbyBombs == tiles[x,y].iFlaggedBombs || tiles[x,y].tileState != Tile.TileState.Opened) continue;
                tiles[x, y].nearbyHiddenTiles.Clear();
                tiles[x, y].iNearbyHidden = 0;
                tiles[x, y].iFlaggedBombs = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        if (CheckIfTileIsHidden(x + i, y + j))
                        {
                            tiles[x, y].nearbyHiddenTiles.Add(tiles[x + i, y + j]);
                            tiles[x, y].iNearbyHidden++;
                        }
                        if (CheckIfTileIsFlagged(x + i, y + j))
                        {
                            tiles[x, y].iFlaggedBombs++;
                        }
                    }
                }
                tiles[x, y].bTileResolved = tiles[x, y].iNearbyBombs == tiles[x, y].iFlaggedBombs;
            }
        }
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y].iNearbyBombs == tiles[x, y].iFlaggedBombs || tiles[x,y].tileState != Tile.TileState.Opened) continue;
                tiles[x, y].linkedTiles = null;
                tiles[x, y].highlightColor = Color.white;
                if ((tiles[x, y].iNearbyBombs - tiles[x, y].iFlaggedBombs == 1) && (tiles[x, y].iNearbyHidden > 1))
                {
                    List<Tile> linkedTiles = new List<Tile>();
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (CheckIfTileIsHidden(x + i, y + j))
                            {
                                linkedTiles.Add(tiles[x + i, y + j]);
                            }
                        }
                    }

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (CheckIfTileIsHidden(x + i, y + j))
                            {
                                tiles[x + i, y + j].linkedTiles = linkedTiles;
                                tiles[x + i, y + j].linked = true;
                            }
                        }
                    }
                }
            }
        }
    }

    protected bool CheckIsMine(int x, int y)
    {
        if (x < 0 || y < 0 || x >= tiles.GetLength(0) || y >= tiles.GetLength(1)) return false;
        return tiles[x, y].bIsMine;
    }

    public bool ClickTile(int x, int y, bool bInRecursion = false)
    {
        try
        {
            if (!(x >= 0 && y >= 0 && x < tiles.GetLength(0) && y < tiles.GetLength(1))) return false;
            if (bInRecursion && ( tiles[x, y].tileState == Tile.TileState.Flagged || tiles[x, y].tileState == Tile.TileState.Opened)) return true;
            if (!bInRecursion && tiles[x,y].tileState == Tile.TileState.Flagged)
            {
                tiles[x, y].Click();
                if (tiles[x, y].bIsMine) return false;
                iFlagsUsed--;
                iHiddenTileCount--;
                return true;
            }
            tiles[x,y].Click();
            if (tiles[x,y].bIsMine) return false;
            iHiddenTileCount--;
            if (tiles[x, y].iNearbyBombs != 0) return true;
            ClickTile(x - 1, y - 1, true);
            ClickTile(x, y - 1, true);
            ClickTile(x - 1, y, true);
            ClickTile(x + 1, y + 1, true);
            ClickTile(x, y + 1, true);
            ClickTile(x + 1, y, true);
            ClickTile(x + 1, y - 1, true);
            ClickTile(x - 1, y + 1, true);
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public bool FlagTile(int x, int y)
    {
        if (!(x >= 0 && y >= 0 && x < tiles.GetLength(0) && y < tiles.GetLength(1))) return false;
        if (tiles[x, y].tileState == Tile.TileState.Opened) return false;
        if (tiles[x, y].tileState == Tile.TileState.Flagged)
        {
            tiles[x, y].UnFlag();
            iHiddenTileCount++;
            return false;
        }
        if (tiles [x,y].tileState == Tile.TileState.Inactive)
        {
            tiles[x, y].Flag();
            iHiddenTileCount--;
            return tiles[x, y].bIsMine;
        }
        return false;
    }

    public int[,] GetTileSquare(int x, int y)
    {
        if (x < 1 || y < 1 || x > iTileXSize - 2 || y > iTileYSize - 2)
            return null;

        int[,] tileSquare = new int[3, 3];
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                tileSquare[x + 1, y + 1] = tiles[x + xOffset, y + yOffset].GetStateValue();
            }
        }
        return tileSquare;
    }
}
