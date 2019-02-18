using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public enum TileState
    {
        Inactive,
        Flagged,
        Opened
    }

    [HideInInspector]
    public Color highlightColor;

    [HideInInspector]
    public bool bTileResolved;

    [HideInInspector]
    public bool linked;

    [HideInInspector]
    public TileState tileState;

    //[HideInInspector]
    public bool bIsMine;

    [HideInInspector]
    public List<Tile> linkedTiles;

    [HideInInspector]
    public List<Tile> nearbyHiddenTiles;

    [SerializeField]
    protected Sprite inactiveSprite;
    [SerializeField]
    protected Sprite mineSprite, detMineSprite, noMineSprite;
    [SerializeField]
    protected Sprite emptySprite;
    [SerializeField]
    protected Sprite flagSprite;
    [SerializeField]
    protected Sprite oneSprite, twoSprite, threeSprite, fourSprite, fiveSprite, sixSprite, sevenSprite, eightSprite;
    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    [HideInInspector]
    public int iNearbyBombs;
    [HideInInspector]
    public int iFlaggedBombs;
    [HideInInspector]
    public int iNearbyHidden;

    public int xPos, yPos;

    // Use this for initialization
    void Start()
    {
        highlightColor = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        if (TileBoard.Instance.State == TileBoard.GameState.GameOver)
        {
            spriteRenderer.color = Color.white;
            if (bIsMine && tileState != TileState.Flagged)
            {
                spriteRenderer.sprite = detMineSprite;
            }
            else if (!bIsMine && tileState == TileState.Flagged)
            {
                spriteRenderer.sprite = noMineSprite;
            }
            else if (tileState != TileState.Opened && !(bIsMine && tileState == TileState.Flagged))
            {
                Click();
            }
        }
        else if (TileBoard.Instance.State == TileBoard.GameState.Win)
        {
            spriteRenderer.color = Color.white;
            if (bIsMine && tileState != TileState.Flagged)
            {
                spriteRenderer.sprite = mineSprite;
            }
            else if (!bIsMine && tileState != TileState.Flagged)
            {
                Click();
            }
        }
        else if (TileBoard.Instance.State == TileBoard.GameState.Playing)
        {
            spriteRenderer.color = highlightColor;
        }
    }

    public void ResetTile()
    {
        bIsMine = false;
        iNearbyBombs = 0;
        iNearbyHidden = 0;
        iFlaggedBombs = 0;
        nearbyHiddenTiles.Clear();
        linkedTiles = null;
        highlightColor = Color.white;
        spriteRenderer.sprite = inactiveSprite;
    }

    public void Click()
    {
        highlightColor = Color.white;
        tileState = TileState.Opened;
        if (bIsMine)
        {
            spriteRenderer.sprite = detMineSprite;
        }
        else
        {
            switch (iNearbyBombs)
            {
                case 0:
                    spriteRenderer.sprite = emptySprite;
                    bTileResolved = true;
                    break;
                case 1:
                    spriteRenderer.sprite = oneSprite;
                    break;
                case 2:
                    spriteRenderer.sprite = twoSprite;
                    break;
                case 3:
                    spriteRenderer.sprite = threeSprite;
                    break;
                case 4:
                    spriteRenderer.sprite = fourSprite;
                    break;
                case 5:
                    spriteRenderer.sprite = fiveSprite;
                    break;
                case 6:
                    spriteRenderer.sprite = sixSprite;
                    break;
                case 7:
                    spriteRenderer.sprite = sevenSprite;
                    break;
                case 8:
                    spriteRenderer.sprite = eightSprite;
                    break;
                default:
                    break;
            }
        }
    }

    public void Flag()
    {
        highlightColor = Color.white;
        tileState = TileState.Flagged;
        spriteRenderer.sprite = flagSprite;
    }

    public void UnFlag()
    {
        highlightColor = Color.white;
        tileState = TileState.Inactive;
        spriteRenderer.sprite = inactiveSprite;
    }

    public int GetStateValue()
    {
        int iStateValue = -1;
        if (this.tileState == TileState.Opened && !this.bIsMine)
        {
            iStateValue = this.iNearbyBombs;
        }
        return iStateValue;
    }

}
