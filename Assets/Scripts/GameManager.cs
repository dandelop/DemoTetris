
using UnityEngine;

public class GameManager: MonoBehaviour
{
    // singleton
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject container;
    
    [SerializeField] private Sprite grayMino;
    
    private GameManager()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            if (_instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
    // game state 
    private bool _isPaused;
    public bool IsPaused => _isPaused;
    
    // tetromino manager
    private TetrominoManager _tetrominoManager;
    public TetrominoManager TetrominoManager { get => _tetrominoManager; set => _tetrominoManager = value; }

    
    // game field (grid)
    private GameField _gameField;
    public GameField GameField { get => _gameField; set => _gameField = value; }

    public void GameOver()
    {
        Debug.Log("Game Over");
        gameOverText.SetActive(true);
        //change color of all tetrominos to gray
        foreach (Transform mino in container.GetComponentInChildren<Transform>())
        {
            mino.GetComponent<SpriteRenderer>().sprite = grayMino;
        }
        Time.timeScale = 0;
    }
}
