using System;
using System.Collections.Generic;
using Source.Features.SceneEditor.Controllers;
using Source.Features.SceneEditor.Data;
using Source.Features.SceneEditor.Enums;
using Source.Features.SceneEditor.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Source.Features.SceneEditor.Objects
{
    public class Cube : MonoBehaviour, IMousePointed, ISelectListener,
        IChangeStateListener<EBuildingState>, IChangeStateListener<EInstrumentState>
    {
        public event Action<ISelectListener> Selected;
        public event Action<Cube> DestroyMouseLeftButtonClicked;
        public event Action<Transform> BuildMouseLeftButtonClicked;
     
        [SerializeField] private ECubeType _type;
        [SerializeField] private EnemyType _enemyType;
        [SerializeField] private HighlightComponent _highlightComponent;
        
        private MouseHandler _mouseHandler;
        private GameObject _buildingGhostCubePrefab;
        private GameObject _destroyGhostCubePrefab;
        private GameObject _ghostCube;

        private Vector3 _previousClosestPoint;
        
        private EBuildingState _buildingState;
        private EInstrumentState _instrumentState;
        private bool _isSelected;
        
        private CubeData _cubeData;
        
        public void Construct(GameObject buildingGhostCubePrefab, GameObject destroyGhostCubePrefab, int objectIndex)
        {
            _mouseHandler = MouseHandler.Instance;
            _buildingGhostCubePrefab = buildingGhostCubePrefab;
            _destroyGhostCubePrefab = destroyGhostCubePrefab;

            _cubeData = new CubeData(transform.position, transform.rotation, objectIndex, _type, _enemyType);
        }
        
        public void MouseExit()
        {
            Destroy(_ghostCube);
            _previousClosestPoint = default;
        }

        public void MouseEnter()
        {
            if (_buildingState == EBuildingState.Build)
                CreateBuildGhostCube(transform.position);
            else if (_buildingState == EBuildingState.Destroy)
                CreateDestroyGhostCube(transform);
        }

        public void MouseLeftButtonUp()
        {
            switch (_buildingState)
            {
                case EBuildingState.Build:
                    BuildMouseLeftButtonClicked?.Invoke(_ghostCube.transform);
                    Destroy(_ghostCube);
                    _previousClosestPoint = default;
                    break;
                case EBuildingState.Destroy:
                    DestroyMouseLeftButtonClicked?.Invoke(this);
                    Destroy(_ghostCube);
                    break;
                case EBuildingState.Disabled:
                    Selected?.Invoke(this);
                    break;
                default:
                    Debug.LogError($"Building State: {_buildingState.ToString()} not implemented!");
                    break;
            }
        }

        public void MouseLeftButtonDown()
        {
            
        }
        
        public void OnStateChange(EBuildingState state)
        {
            if (_ghostCube)
                Destroy(_ghostCube);
            
            _buildingState = state;
        }
        
        public void OnStateChange(EInstrumentState state)
        {
            _instrumentState = state;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public ECubeType GetType()
        {
            return _type;
        }

        public void OnSelectStateChange(bool isSelected)
        {
            _isSelected = isSelected;

            if (_isSelected)
            {
                _highlightComponent.SetHighlight();
            }
            else
            {
                _highlightComponent.SetDefault();
            }
        }

        public CubeData GetData()
        {
            var position = transform.position;
            var rotation = transform.rotation.eulerAngles;
            
            _cubeData.X = position.x;
            _cubeData.Y = position.y;
            _cubeData.Z = position.z;
            
            _cubeData.XRotation = rotation.x;
            _cubeData.YRotation = rotation.y;
            _cubeData.ZRotation = rotation.z;
            
            return _cubeData;
        }
        
        private void CreateBuildGhostCube(Vector3 position)
        {
            var closestPoint = GetClosestSpawnPoint(position);

            if (_previousClosestPoint != default && closestPoint == _previousClosestPoint)
                return;
            
            _previousClosestPoint = closestPoint;
            
            if (_ghostCube)
                Destroy(_ghostCube);
            
            _ghostCube = Instantiate(_buildingGhostCubePrefab, closestPoint, Quaternion.identity);
        }

        private void CreateDestroyGhostCube(Transform spawnTransform)
        {
            if (_ghostCube)
                return;
            
            _ghostCube = Instantiate(_destroyGhostCubePrefab, spawnTransform.position, spawnTransform.rotation);
        }

        private Vector3 GetClosestSpawnPoint(Vector3 position)
        {
            var minDistance = float.MaxValue;

            var points = new List<Vector3>()
            {
                position + new Vector3(4, 0, 0),
                position + new Vector3(0, 4, 0),
                position + new Vector3(0, 0, 4),
                position + new Vector3(-4, 0, 0),
                position + new Vector3(0, -4, 0),
                position + new Vector3(0, 0, -4)
            };
            
            var closestPointFromList = points[0];
            
            foreach (Vector3 point in points)
            {
                var distance = (point - _mouseHandler.GetHitPoint()).magnitude;
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPointFromList = point;
                }
            }

            return closestPointFromList;
        }
    }
}