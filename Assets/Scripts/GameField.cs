
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameField : MonoBehaviour
{
    // list of pieces sprites
    [SerializeField] private List<Sprite> pieceSprites;

    // dictionary of piece colors -> sprites
    private Dictionary<PieceColor, Sprite> pieceColorSprites;

    // grid occupied positions
    private bool[,] _grid;

    // grid color occupied positions
    private PieceColor[,] _gridColor;

    // dictionary of grid positions -> game objects
    private GameObject[,] _gridObjects;

    // grid size
    [SerializeField] private Vector2Int size = new Vector2Int(10, 20);
    private Vector2Int _size0;

    // grid screen position offset
    //[SerializeField] private Vector2Int offset = new Vector2Int(5, 10);
    // grid sprite position offset
    [SerializeField] private Vector2 spriteOffset = new Vector2(4.5f, 9.5f);

    List<GameObject> minosToDestroy = new List<GameObject>();


    void Awake()
    {
        GameManager.Instance.GameField = this;
        // calculate matrix size
        _size0 = size - Vector2Int.one;
        
        // initialize grid
        _grid = new bool[size.x, size.y];
        _gridColor = new PieceColor[size.x, size.y];
        _gridObjects = new GameObject[size.x, size.y];
        
        // initialize grid occupied positions
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                _grid[i, j] = false;
            }
        }

        // verify sprite colors list length
        if ((pieceSprites == null) || (pieceSprites.Count < PieceColor.GetNames(typeof(PieceColor)).Length))
        {
            Debug.LogError("PieceColors list length must be " + PieceColor.GetNames(typeof(PieceColor)).Length);
        }

        // initialize dictionary
        pieceColorSprites = new Dictionary<PieceColor, Sprite>();
        // fill dictionary
        for (int i = 0; i < pieceSprites.Count; i++)
        {
            pieceColorSprites.Add((PieceColor)i, pieceSprites[i]);
        }
    }

    public bool isInside(Vector2Int gridPosition)
    {
        // verify horizontals and verticals limits
        if ((gridPosition.x < 0) || (gridPosition.x > _size0.x) || (gridPosition.y < 0) || (gridPosition.y > _size0.y))
        {
            return false;
        }

        return true;
    }

    public bool isOccupied(Vector2Int gridPosition)
    {
        // consult grid
        if (gridPosition.y == -1)
        {
            return true;
        }

        if (!((gridPosition.x < 0) || (gridPosition.x > _size0.x) || (gridPosition.y < 0) ||
              (gridPosition.y > _size0.y)))
        {
            return _grid[gridPosition.x, gridPosition.y];
        }

        return false;
    }

    public void fixPiece(Tetromino tetromino)
    {
        // get piece rotation
        Vector2Int[] pieceRotation = Tetromino.PieceRotations[tetromino.Type][tetromino.Rotation];
        // draw mimos in grid positions
        for (int i = 0; i < pieceRotation.Length; i++)
        {
            // get grid position
            Vector2Int gridPosition = tetromino.Position + pieceRotation[i];
            createMino(gridPosition, (PieceColor)tetromino.Type);
        }

        // verify lines
        verifyLines();

        // when fix a piece, call tetromino manager to generate a new piece
        GameManager.Instance.TetrominoManager.Generate();
    }

    private void createMino(Vector2Int gridPosition, PieceColor color)
    {
        // set grid position as occupied
        _grid[gridPosition.x, gridPosition.y] = true;
        // set grid position color
        _gridColor[gridPosition.x, gridPosition.y] = color;
        // draw mimo in grid position
        // create a new sprite
        GameObject newSprite = new GameObject();
        // save sprite in dictionary
        _gridObjects[gridPosition.x, gridPosition.y] = newSprite;
        // set sprite parent
        newSprite.transform.parent = this.transform;
        // set sprite position
        newSprite.transform.localPosition = gridPosition - spriteOffset;
        // add sprite renderer
        SpriteRenderer spriteRenderer = newSprite.AddComponent<SpriteRenderer>();
        // set sprite sorting layer
        spriteRenderer.sortingLayerName = "Pieces";
        // set sprite renderer sprite
        spriteRenderer.sprite = pieceColorSprites[color];
    }

    private void verifyLines()
    {
        // verify lines
        for (int j = 0; j < size.y; j++)
        {
            // verify if line is full
            bool lineFull = true;
            for (int i = 0; i < size.x; i++)
            {
                if (!_grid[i, j])
                {
                    lineFull = false;
                    break;
                }
            }

            // if line is full, remove line
            if (lineFull)
            {
                removeLine(j);
                // verify "next line"
                j--;
            }
        }
    }

    IEnumerator DestroyMino(List<GameObject> minos)
    {
        float t = 0f;
        float duration = 0.1f;
        Vector3 initialSize = Vector3.one;
        Vector3 targetSize = new Vector3(0.1f, 0.1f, 0.1f);
    
        while (t < duration)
        {
            t += Time.deltaTime;
            foreach (GameObject mino in minos)
            {
                mino.transform.localScale = Vector3.Lerp(initialSize, targetSize, t / duration);
            }
            yield return null;
        }
        
        foreach (GameObject mino in minos)
        {
            Destroy(mino);
        }
    }
    
    private void removeLine(int line)
    {
        // remove line
        for (int i = 0; i < size.x; i++)
        {
            // set grid position as unoccupied
            _grid[i, line] = false;
            _gridColor[i, line] = PieceColor.Gray; // set Gray as default color
            minosToDestroy.Add(_gridObjects[i, line]);
        }
        
        StartCoroutine(DestroyMino(minosToDestroy));
        
        // move lines above
        for (int j = line; j < size.y - 1; j++)
        {
            for (int i = 0; i < size.x; i++)
            {
                // set grid position as occupied
                _grid[i, j] = _grid[i, j + 1];
                // set grid position color
                _gridColor[i, j] = _gridColor[i, j + 1];
                // move mimo in grid position
                if (_gridObjects[i, j + 1] != null)
                {
                    _gridObjects[i, j + 1].transform.localPosition =
                        new Vector2(i - spriteOffset.x, j - spriteOffset.y);
                    _gridObjects[i, j] = _gridObjects[i, j + 1];
                }
                else
                {
                    _gridObjects[i, j] = null;
                }
            }
        }

        // set last line as unoccupied
        for (int i = 0; i < size.x; i++)
        {
            if (_gridObjects[i, size.y - 1] != null)
            {
                Destroy(_gridObjects[i, size.y - 1]);
            }

            // set grid position as unoccupied
            _grid[i, size.y - 1] = false;
            _gridColor[i, size.y - 1] = PieceColor.Gray; // set Gray as default color
        }
    }
}