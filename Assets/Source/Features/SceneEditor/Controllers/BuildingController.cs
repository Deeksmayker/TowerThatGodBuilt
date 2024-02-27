using System;
using Source.Features.SceneEditor.Objects;
using UnityEngine;
using Grid = Source.Features.SceneEditor.Objects.Grid;

namespace Source.Features.SceneEditor.Controllers
{
    public class BuildingController : MonoBehaviour
    {
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private Grid _grid;
        
        [SerializeField] private GameObject[] _objectPrefabs;
        
        private GameObject _currentObjectPrefab;

        private void Start()
        {
            _grid.BuildGrid();
            
            OnAlphaPressed(0);
        }

        private void OnEnable()
        {
            _inputHandler.AlphaButtonPressed += OnAlphaPressed;
            _grid.CellClicked += OnCellClicked;
        }

        private void OnDisable()
        {
            _inputHandler.AlphaButtonPressed -= OnAlphaPressed;
            _grid.CellClicked -= OnCellClicked;
        }

        private void OnAlphaPressed(int objectIndex)
        {
            if (objectIndex < 0 || objectIndex >= _objectPrefabs.Length)
            {
                Debug.LogError($"Pressed invalid alpha button.\n" +
                               $"Current: {objectIndex}, but max: {_objectPrefabs.Length - 1}");
                return;
            }

            _currentObjectPrefab = _objectPrefabs[objectIndex];
        }

        private void OnCellClicked(Cell cell)
        {
            Instantiate(_currentObjectPrefab, cell.transform);
        }
    }
}