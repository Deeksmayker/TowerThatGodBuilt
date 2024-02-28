using System;
using Source.Features.SceneEditor.Controllers;
using UnityEngine;

namespace Source.Features.SceneEditor.Objects
{
    public class Grid : MonoBehaviour
    {
        public event Action<Cell> CellClicked;
        
        [SerializeField] private int _width;
        [SerializeField] private int _height;

        [SerializeField] private Cell _cellPrefab;
        
        private Cell[,] _cells;

        private void Awake()
        {
            _cells = new Cell[_width, _height];
        }

        public void BuildGrid()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _cells[x, y] = Instantiate(_cellPrefab, 
                        new Vector3(x * _cellPrefab.GetCellSize().x, 0 ,y * _cellPrefab.GetCellSize().y),
                        Quaternion.identity, transform);
                    _cells[x, y].Clicked += OnCellClicked;
                }
            }
        }

        public void ClearGrid()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    Destroy(_cells[x, y].gameObject);
                }
            }
            
            _cells = new Cell[_width, _height];
        }

        public Cell[,] GetCells()
        {
            return _cells;
        }

        private void OnCellClicked(Cell cell)
        {
            CellClicked?.Invoke(cell);
        }
    }
}