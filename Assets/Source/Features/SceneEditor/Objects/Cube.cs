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
    public class Cube : MonoBehaviour, IMousePointed, IChangeStateListener<EBuildingState>, IChangeStateListener<EInstrumentState>
    {
        public event Action<Cube> DestroyMouseLeftButtonClicked;
        public event Action<Transform> BuildMouseLeftButtonClicked;
        
        private MouseHandler _mouseHandler;
        private GameObject _buildingGhostCubePrefab;
        private GameObject _destroyGhostCubePrefab;
        private GameObject _ghostCube;

        private Vector3 _previousClosestPoint;
        
        private EBuildingState _buildingState;
        private EInstrumentState _instrumentState;

        private CubeData _cubeData;
        
        public void Construct(MouseHandler mouseHandler, GameObject buildingGhostCubePrefab, GameObject destroyGhostCubePrefab, int objectIndex)
        {
            _mouseHandler = mouseHandler;
            _buildingGhostCubePrefab = buildingGhostCubePrefab;
            _destroyGhostCubePrefab = destroyGhostCubePrefab;

            _cubeData = new CubeData(transform.position, transform.rotation, objectIndex);
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
            if (_buildingState == EBuildingState.Build)
            {
                BuildMouseLeftButtonClicked?.Invoke(_ghostCube.transform);

                Destroy(_ghostCube);
                _previousClosestPoint = default;
            }
            else if (_buildingState == EBuildingState.Destroy)
            {
                DestroyMouseLeftButtonClicked?.Invoke(this);
                
                Destroy(_ghostCube);
            }
        }

        public void MouseLeftButtonDown()
        {
            /*if (_instrumentState == EInstrumentState.Tassel)
            {
                var vector = _ghostCube.transform
            }*/
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

        public CubeData GetData()
        {
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
                position + new Vector3(1, 0, 0),
                position + new Vector3(0, 1, 0),
                position + new Vector3(0, 0, 1),
                position + new Vector3(-1, 0, 0),
                position + new Vector3(0, -1, 0),
                position + new Vector3(0, 0, -1)
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