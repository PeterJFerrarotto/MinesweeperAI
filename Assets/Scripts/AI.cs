using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AI : MonoBehaviour {

    public void ClearTargets()
    {
        try
        {
            targets.Clear();
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
    }

    protected static AI _instance;
    public static AI Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject();
                _instance = obj.AddComponent<AI>();
            }
            return _instance;
        }
    }

    protected Queue<Vector3> targets;
    protected Vector3 currTarget;
	// Use this for initialization
	void Start () {
        _instance = this;
        DontDestroyOnLoad(_instance);
        targets = new Queue<Vector3>();
        currTarget = new Vector3(-1, -1, -1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Move()
    {
        if (targets.Count == 0)
        {
            GetTargetsBasic();
            if (targets.Count == 0)
            {
                GetTargetsLinked();
            }
            if (targets.Count == 0)
            {
                if (TileBoard.Instance[TileBoard.Instance.iTileXSize - 1, TileBoard.Instance.iTileYSize - 1].tileState == Tile.TileState.Inactive) targets.Enqueue(new Vector3(TileBoard.Instance.iTileXSize - 1, TileBoard.Instance.iTileYSize - 1, 0));
            }
        }

        if (currTarget.x != -1 && currTarget.y != -1)
        {
            TileBoard.Instance[(int)currTarget.x, (int)currTarget.y].highlightColor = Color.white;
        }
        bool bSearchAgain = true;
        do
        {
            if (targets.Count == 0)
            {
                if (bSearchAgain) GetTargetsBasic();
                else return;
            }
            if (targets.Count == 0) return;
            currTarget = targets.Dequeue();
        } while (TileBoard.Instance[(int)currTarget.x, (int)currTarget.y].tileState != Tile.TileState.Inactive);
        if (currTarget.z == 1) TileBoard.Instance[(int)currTarget.x, (int)currTarget.y].highlightColor = Color.red;
        else TileBoard.Instance[(int)currTarget.x, (int)currTarget.y].highlightColor = Color.green;
        TileBoard.Instance.SimClick((int)currTarget.x, (int)currTarget.y, (int)currTarget.z);
    }

    private void GetTargetsBasic()
    {
        for (int x = 0; x < TileBoard.Instance.iTileXSize; x++)
        {
            for (int y = 0; y < TileBoard.Instance.iTileYSize; y++)
            {
                if (TileBoard.Instance[x,y].tileState == Tile.TileState.Opened && TileBoard.Instance[x,y].iNearbyBombs >= 1 && TileBoard.Instance[x,y].iNearbyHidden >= 1 && (TileBoard.Instance[x, y].iNearbyBombs == TileBoard.Instance[x, y].iFlaggedBombs || TileBoard.Instance[x, y].iNearbyBombs - TileBoard.Instance[x,y].iFlaggedBombs == TileBoard.Instance[x, y].iNearbyHidden))
                {
                    int iFlag = 0;
                    if (TileBoard.Instance[x, y].iNearbyBombs == TileBoard.Instance[x, y].iFlaggedBombs)
                    {
                        iFlag = 0;
                    }
                    else if (TileBoard.Instance[x, y].iNearbyBombs - TileBoard.Instance[x,y].iFlaggedBombs == TileBoard.Instance[x, y].iNearbyHidden)
                    {
                        iFlag = 1;
                    }
                    else
                        continue;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (x + i < 0 || x + i >= TileBoard.Instance.iTileXSize || y + j < 0 || y + j >= TileBoard.Instance.iTileYSize) continue;
                            if (TileBoard.Instance[x, y].nearbyHiddenTiles.Contains(TileBoard.Instance[x + i, y + j]))
                            {
                                Vector3 vec = new Vector3(x + i, y + j, iFlag);
                                if (targets.Contains(vec)) continue;
                                if (TileBoard.Instance[x + i, y + j].tileState == Tile.TileState.Inactive) targets.Enqueue(vec);
                            }
                        }
                    }
                }
            }
        }
        if (targets.Count == 0)
        {
            GetTargetsLinked();
        }
    }

    private void GetTargetsLinked()
    {
        for (int x = 0; x < TileBoard.Instance.iTileXSize; x++)
        {
            for (int y = 0; y < TileBoard.Instance.iTileYSize; y++)
            {
                if (TileBoard.Instance[x,y].tileState == Tile.TileState.Opened)
                {
                    if (TileBoard.Instance[x,y].iNearbyBombs > 1 && TileBoard.Instance[x,y].iNearbyBombs - TileBoard.Instance[x,y].iFlaggedBombs > 1)
                    {
                        Tile currTile = TileBoard.Instance[x, y];
                        foreach (Tile hiddenTile in currTile.nearbyHiddenTiles)
                        {
                            if (hiddenTile.linked)
                            {
                                int numberLinked = 0;
                                List<Tile> adjacentLinkedTiles = new List<Tile>();
                                foreach (Tile linkedTile in hiddenTile.linkedTiles)
                                {
                                    if (currTile.nearbyHiddenTiles.Contains(linkedTile))
                                    {
                                        numberLinked++;
                                        adjacentLinkedTiles.Add(linkedTile);
                                    }
                                }
                                if (numberLinked > 1)
                                {
                                    if (currTile.nearbyHiddenTiles.Count - (numberLinked - 1) == currTile.iNearbyBombs)
                                    {
                                        foreach (Tile hiddenTileToAdd in currTile.nearbyHiddenTiles)
                                        {
                                            if (!adjacentLinkedTiles.Contains(hiddenTileToAdd))
                                            {
                                                Vector3 vec = new Vector3(hiddenTile.xPos, hiddenTileToAdd.yPos, 1);
                                                if (!targets.Contains(vec))
                                                {
                                                    targets.Enqueue(vec);
                                                }
                                            }
                                        }
                                    }
                                    if (targets.Count == 0)
                                    {
                                        System.Console.WriteLine("Fuck");
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}
