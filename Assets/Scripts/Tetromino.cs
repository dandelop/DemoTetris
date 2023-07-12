using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    // minos by piece
    private const int MINOS = 4;
    
    // types of piece
    [SerializeField] private PieceType type;
    public PieceType Type
    {
        get => type;
        set => type = value;
    }

    // color of piece
    [SerializeField] private Sprite[] sprites;

    // array of spritesRendereres on Childs GameObjects
    private SpriteRenderer[] spriteRenderers;
    
    // position of piece in grid
    [SerializeField] private Vector2Int position;
    public Vector2Int Position
    {
        get => position;
        set => position = value;
    }

    // falldowan speed
    [SerializeField] private float fallDownTime = 1f;
    private float _fallDownTimer;
    
    // dictionary of piece rotations allowing for easy access
    private static readonly Dictionary<PieceType, int> rotations = new Dictionary<PieceType, int>
    {
        { PieceType.O, 1 },
        { PieceType.S, 2 },
        { PieceType.Z, 2 },
        { PieceType.T, 4 },
        { PieceType.L, 4 },
        { PieceType.J, 4 },
        { PieceType.I, 2 }
        //{ PieceType.P, 1 },
    };
    
    // actual rotation of piece
    [SerializeField] private int rotation;
    public int Rotation => rotation;

    // dictionary of piece types
    public static readonly Dictionary<PieceType, Vector2Int[][]> PieceRotations = new Dictionary<PieceType, Vector2Int[][]>
    {
        { PieceType.O, new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }
        } },
        { PieceType.S, new Vector2Int[][] 
        { 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) }, 
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(0, 2) } 
        } },
        { PieceType.Z, new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(2, 0) }, 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) }
        } },
        { PieceType.T, new Vector2Int[][] 
        { 
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) }, 
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 0) },
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 1) }
        } },
        { PieceType.L, new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1) },
            new Vector2Int[] { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(1, 2) }
        } },
        { PieceType.J, new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 0) },
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) },
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) }
        } },
        { PieceType.I, new Vector2Int[][]
        {
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) },
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3) }
        } }
        //{ PieceType.P, new Vector2Int[][] { new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0)} } },
    };
    
    // local reference to GameField (grid)
    private GameField gameField;
    
    // delay auto shift
    public float _das = 0.2f;
    private float _dasTimer = 0f;
    private bool firtMove = true;
    
    private void Awake()
    {
        // get sprite renderers
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        // verify number of sprite renderers
        if (spriteRenderers.Length != 4)
        {
            throw new Exception("Piece must have 4 sprite renderers");
        }
        // verify sprites
        if (sprites == null || sprites.Length == 0 || sprites.Length < PieceType.GetNames(typeof(PieceType)).Length)
        {
            throw new Exception("Piece sprites must have " + PieceType.GetNames(typeof(PieceType)).Length + " elements");
        }
    }

    private void Start()
    {
        // draw piece
        DrawPiece();
        // get local reference to GameField
        gameField = GameManager.Instance.GameField;
    }

    private void Update()
    {
        // verify time to falldown
        _fallDownTimer += Time.deltaTime;
        if (_fallDownTimer >= fallDownTime)
        {
            // reset timer
            _fallDownTimer = 0;
            // move piece
            try
            {
                Move(new Vector2Int(0, -1));
            }
            catch (Exception e)
            {
                GameManager.Instance.GameOver();
            }
        }
        
        // get inputs keys
        // move
        if ((Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.RightArrow)) || (Input.GetKey(KeyCode.DownArrow)))
        {
            // delay auto shift
            if (firtMove)
            {   
                // move piece
                if (Input.GetKey(KeyCode.LeftArrow))
                    Move(new Vector2Int(-1, 0));
                else if (Input.GetKey(KeyCode.RightArrow))
                    Move(new Vector2Int(1, 0));
                else if (Input.GetKey(KeyCode.DownArrow))
                    try
                    {
                        Move(new Vector2Int(0, -1));
                    }
                    catch (Exception e)
                    {
                        GameManager.Instance.GameOver();
                    }
                firtMove = false;
                _dasTimer = _das;
            }
            else
            {
                // after firt move, move only when dasTimer is 0 or less
                _dasTimer -= Time.deltaTime;
                if (_dasTimer <= 0)
                {
                    // move piece
                    if (Input.GetKey(KeyCode.LeftArrow))
                        Move(new Vector2Int(-1, 0));
                    else if (Input.GetKey(KeyCode.RightArrow))
                        Move(new Vector2Int(1, 0));
                    else if (Input.GetKey(KeyCode.DownArrow))
                        Move(new Vector2Int(0, -1));
                    _dasTimer = _das / 10;  // minimum delay
                }
            }
        }
        /*
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // move piece
            Move(new Vector2Int(0, 1));
        }
        */
        // rotate (with keydown)
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // rotate piece
            Rotate();
        }
        /*
        // fix piece (with keydown)
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // fix me
            FixMe();
        }
        */
        else
        {
            // initialize delay auto shift
            firtMove = true;
        }
        
        // draw piece
        DrawPiece();
    }
    
    
    // draw piece
    private void DrawPiece()
    {
        // get piece rotation
        Vector2Int[] pieceRotation = PieceRotations[type][rotation];
        // set sprite renderers
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = sprites[(int)type];
            spriteRenderers[i].transform.localPosition = (Vector2)(position + pieceRotation[i]);
        }
    }

    // rotate piece
    public void Rotate()
    {
        int desiredRotation = (rotation + 1) % rotations[type];

        // find borders of piece (left, right, bottom)
        Vector2Int[] leftRightBottomBorders = FindBorders(PieceRotations[type][desiredRotation]);
        // verify if piece can rotate according to borders
        if (!gameField.isInside(leftRightBottomBorders[0] + position))
        {
            return;
        }
        if (!gameField.isInside(leftRightBottomBorders[1] + position))
        {
            return;
        }
        if (!gameField.isInside(leftRightBottomBorders[2] + position))
        {
            return;
        } 
        
        // verify if piece can rotate according to other pieces (using ALL minos of piece)
        foreach (Vector2Int mino in PieceRotations[type][desiredRotation])
        {
            // verify if mino is occupied
            if (gameField.isOccupied(mino + position))
            {
                return;
            }
        }
        
        // rotate piece
        rotation = (rotation + 1) % rotations[type];
    }
    
    // move piece
    public void Move(Vector2Int direction)
    {
        Vector2Int desiredPosition = position + direction;

        // find borders of piece (left, right, bottom)
        Vector2Int[] leftRightBottomBorders = FindBorders(PieceRotations[type][rotation]);
        // find all borders mino of piece by rows and move direction
        List<Vector2Int> bordersMinos = new List<Vector2Int>();
        // verify if piece can move according to borders (inside game field)
        if (direction.x < 0)
        {
            if (!gameField.isInside(leftRightBottomBorders[0] + desiredPosition))
            {
                return;
            }
            // setup borders minos (OK) or minos in border (ERROR: ghost piece can move through other pieces)
            //bordersMinos = FindMinosInBorder(PieceRotations[type][rotation], Direction.Left);
            bordersMinos = FindMinosBorderByRowCol(PieceRotations[type][rotation], Direction.Left);
        }
        else if (direction.x > 0)
        {
            if (!gameField.isInside(leftRightBottomBorders[1] + desiredPosition))
            {
                return;
            }
            // setup borders minos (OK) or minos in border (ERROR: ghost piece can move through other pieces)
            //bordersMinos = FindMinosInBorder(PieceRotations[type][rotation], Direction.Right);
            bordersMinos = FindMinosBorderByRowCol(PieceRotations[type][rotation], Direction.Right);
        }
        if (direction.y < 0)
        {
            // when move down is necessary verify if fix piece before verify outside game field
            // setup borders minos (OK) or minos in border (ERROR: ghost piece can move through other pieces)
            //bordersMinos = FindMinosInBorder(PieceRotations[type][rotation], Direction.Down);
            bordersMinos = FindMinosBorderByRowCol(PieceRotations[type][rotation], Direction.Down);
            // verify if fix piece when move down (if piece is in bottom or over other piece)
            // 2. Verify ONLY borders minos of piece
            foreach (Vector2Int borderMino in bordersMinos)
            {
                if (gameField.isOccupied(borderMino + desiredPosition))
                {
                    FixMe();
                    return;
                }
            }
            // verify inside game field after verification of fix piece
            if (!gameField.isInside(leftRightBottomBorders[2] + desiredPosition))
            {
                return;
            }
        }
        
        // Two method to verify if piece can move according to occuped minos
        // 1. Verify ALL minos of piece
        foreach (Vector2Int mino in PieceRotations[type][rotation])
        {
            if (gameField.isOccupied(mino + desiredPosition))
            {
                return;
            }
        }
        // 2. Verify ONLY borders minos of piece
        foreach (Vector2Int borderMino in bordersMinos)
        {
            if (gameField.isOccupied(borderMino + desiredPosition))
            {
                return;
            }
        }
            
        // move piece
        position = desiredPosition;
        //Debug.Log("Move: " + position);
    }
    
    
    // find borders of piece (left, right, bottom)
    public Vector2Int[] FindBorders(Vector2Int[] pieceRotation)
    {
        // find borders of piece (left, right, bottom)
        Vector2Int leftBorder = new Vector2Int(int.MaxValue, 0);
        Vector2Int rightBorder = new Vector2Int(int.MinValue, 0);
        Vector2Int bottomBorder = new Vector2Int(0, int.MaxValue);
        // find borders
        for (int i = 0; i < pieceRotation.Length; i++)
        {
            // left border
            if (pieceRotation[i].x < leftBorder.x)
            {
                leftBorder = pieceRotation[i];
            }
            // right border
            if (pieceRotation[i].x > rightBorder.x)
            {
                rightBorder = pieceRotation[i];
            }
            // bottom border
            if (pieceRotation[i].y < bottomBorder.y)
            {
                bottomBorder = pieceRotation[i];
            }
        }
        // return borders
        return new Vector2Int[] { leftBorder, rightBorder, bottomBorder };
    }
    
    // find borders of piece by rows and move direction
    public List<Vector2Int> FindMinosInBorder(Vector2Int[] pieceRotation, Direction direction)
    {
        // list of minos with borders (max 4 minos)
        List<Vector2Int> minos = new List<Vector2Int>();
        // find borders of piece by rows and move direction
        Vector2Int leftBorder = new Vector2Int(int.MaxValue, 0);
        Vector2Int rightBorder = new Vector2Int(int.MinValue, 0);
        Vector2Int bottomBorder = new Vector2Int(0, int.MaxValue);

        // find borders minos according to move direction
        switch (direction)
        {
            case Direction.Left:
                // find left border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    // left border
                    if (pieceRotation[i].x < leftBorder.x)
                    {
                        leftBorder = pieceRotation[i];
                    }
                }

                // find minos with left border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    if (pieceRotation[i].x == leftBorder.x)
                    {
                        minos.Add(pieceRotation[i]);
                    }
                }

                break;
            case Direction.Right:
                // find right border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    // right border
                    if (pieceRotation[i].x > rightBorder.x)
                    {
                        rightBorder = pieceRotation[i];
                    }
                }

                // find minos with right border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    if (pieceRotation[i].x == rightBorder.x)
                    {
                        minos.Add(pieceRotation[i]);
                    }
                }

                break;
            case Direction.Down:
                // find bottom border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    // bottom border
                    if (pieceRotation[i].y < bottomBorder.y)
                    {
                        bottomBorder = pieceRotation[i];
                    }
                }

                // find minos with bottom border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    if (pieceRotation[i].y == bottomBorder.y)
                    {
                        minos.Add(pieceRotation[i]);
                    }
                }

                break;
        }

        // return borders
        return minos;
    }
    
    
    // find minos border by rows/cols of piece according move direction
    public List<Vector2Int> FindMinosBorderByRowCol(Vector2Int[] pieceRotation, Direction direction)
    {
        // Dictionary of minos by row/col (max 4 minos)
        Dictionary<int, Vector2Int> minos = new Dictionary<int, Vector2Int>();
        // find borders of piece by rows and move direction
        Vector2Int leftBorder = new Vector2Int(int.MaxValue, 0);
        Vector2Int rightBorder = new Vector2Int(int.MinValue, 0);
        Vector2Int bottomBorder = new Vector2Int(0, int.MaxValue);
        // initialize minos according to move direction
        switch (direction)
        {
            case Direction.Left:
                // setup for initial left border
                for (int i = 0; i < MINOS; i++)
                {
                    minos[i] = leftBorder;
                }

                break;
            
            case Direction.Right:
                // setup for initial right border
                for (int i = 0; i < MINOS; i++)
                {
                    minos[i] = rightBorder;
                }

                break;
            
            case Direction.Down:
                // setup for initial bottom border
                for (int i = 0; i < MINOS; i++)
                {
                    minos[i] = bottomBorder;
                }

                break;

        }

        // find borders minos according to move direction
        switch (direction)
        {
            case Direction.Left:
                // find left border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    // left border
                    if (pieceRotation[i].x < minos[pieceRotation[i].y].x)
                    {
                        minos[pieceRotation[i].y] = pieceRotation[i];
                    }
                }

                break;
            case Direction.Right:
                // find right border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    // right border
                    if (pieceRotation[i].x > minos[pieceRotation[i].y].x)
                    {
                        minos[pieceRotation[i].y] = pieceRotation[i];
                    }
                }

                break;
            case Direction.Down:
                // find bottom border
                for (int i = 0; i < pieceRotation.Length; i++)
                {
                    // bottom border
                    if (pieceRotation[i].y < minos[pieceRotation[i].x].y)
                    {
                        minos[pieceRotation[i].x] = pieceRotation[i];
                    }
                }

                break;
        }

        // return mino borders by row/col (distinct to default value)
        return minos.Values.Where(x => x != leftBorder && x != rightBorder && x != bottomBorder).ToList();
    }

    // fix me method
    public void FixMe()
    {
        // fix piece
        gameField.fixPiece(this);
        // destroy piece
        Destroy(gameObject);
    }
    
}
