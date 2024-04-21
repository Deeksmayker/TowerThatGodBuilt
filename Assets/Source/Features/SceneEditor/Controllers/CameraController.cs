using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5;
        [SerializeField] private float _rotSpeed = 0.1f;

        private Vector2? _prevMousePos;

        private bool _isInputLocked;
        
        // TODO: Переписать на нормальные импута
        private void Update()
        {
            if (_isInputLocked) return;
            
            KeyboardControl();
            MouseControl();
        }

        public void LockInput()
        {
            _isInputLocked = true;
        }
        
        public void UnlockInput()
        {
            _isInputLocked = false;
        }

        private void KeyboardControl()
        {
            var dv = _moveSpeed * Time.deltaTime;
            
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * dv;
            }
            
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * dv;
            }
            
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * dv;
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * dv;
            }
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.position += transform.up * dv;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                transform.position -= transform.up * dv;
            }
        }

        private void MouseControl()
        {
            if (Input.GetMouseButton(1))
            {
                if (_prevMousePos.HasValue)
                {
                    var dp =  (Vector2) Input.mousePosition - _prevMousePos.Value;
                    var euler = transform.localEulerAngles;
                    var angleY = euler.y + _rotSpeed * dp.x;
                    var angleX = euler.x - _rotSpeed * dp.y;
                    transform.localEulerAngles = new Vector3(angleX, angleY);
                    _prevMousePos = Input.mousePosition;
                }
                else
                {
                    _prevMousePos = Input.mousePosition;
                }
            }
            else
            {
                _prevMousePos = null;
            }
        }
    }
}