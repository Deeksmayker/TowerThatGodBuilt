using System;
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

        public void BuildGrid()
        {
            _cells = new Cell[_width, _height];
            
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _cells[x, y] = Instantiate(_cellPrefab, 
                        new Vector3(x * _cellPrefab.GetCellSize().x, y * _cellPrefab.GetCellSize().y),
                        Quaternion.identity, transform);
                    _cells[x, y].Clicked += OnCellClicked;
                }
            }
        }

        private void OnCellClicked(Cell cell)
        {
            CellClicked?.Invoke(cell);
        }
    }
}