using System;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Objects;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class GridController : MonoBehaviour
    {
        public event Action<Cell, EBuildingState> CellClicked;

        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private float _cellSize = 1;

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
                        new Vector3(x * _cellSize, 0, y * _cellSize),
                        Quaternion.identity, transform);
                    _cells[x, y].transform.localScale = Vector3.one * _cellSize;
                    
                    _cells[x, y].Clicked += OnCellClicked;
                    _cells[x, y].Hide();
                }
            }
        }

        public void ShowGrid()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _cells[x, y].Show();
                }
            }
        }

        public void ClearGrid()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (!_cells[x, y])
                        continue;

                    Destroy(_cells[x, y].gameObject);
                }
            }

            _cells = new Cell[_width, _height];
        }

        public Cell[,] GetCells()
        {
            return _cells;
        }

        public int GetHeight()
        {
            return _height;
        }

        public int GetWidth()
        {
            return _width;
        }

        public float GetCellSize()
        {
            return _cellSize;
        }

        private void OnCellClicked(Cell cell, EBuildingState buildingState)
        {
            CellClicked?.Invoke(cell, buildingState);
        }
    }
}