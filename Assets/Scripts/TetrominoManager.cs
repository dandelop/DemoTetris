using System;
using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    // tetromino prefab
    [SerializeField] private GameObject tetrominoPrefab;
    
    [SerializeField] private GameObject nextPiece;
    [SerializeField] private GameObject actualPiece;

    [SerializeField] private PieceType nextPieceType;
    [SerializeField] private PieceType actualPieceType;
    
    private void Awake()
    {
        // register in game manager
        GameManager.Instance.TetrominoManager = this;
        
        nextPieceType = (PieceType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(PieceType)).Length); 
        
        actualPieceType = (PieceType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(PieceType)).Length);
    }
    
    
    
    public void Generate()
    { 
        if (nextPiece != null)
        {
            Destroy(nextPiece);
        }
        
        actualPiece = Instantiate(tetrominoPrefab, transform);
        
        nextPiece = Instantiate(tetrominoPrefab, transform);
        
        actualPiece.GetComponent<Tetromino>().Type = actualPieceType;
        
        nextPiece.GetComponent<Tetromino>().Type = nextPieceType;
        
        actualPiece.GetComponent<Tetromino>().Position = new Vector2Int(4, 20);
        
        nextPiece.GetComponent<Tetromino>().Position = new Vector2Int(20, -14);
        
        nextPiece.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        
        nextPiece.transform.rotation = Quaternion.Euler(0, 0, 90);
        
        actualPieceType = nextPieceType;
        
        nextPieceType = (PieceType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(PieceType)).Length);
    }

    private void Start()
    {
        Generate();
    }
}
