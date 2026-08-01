using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Debugging
{
    public class DebugCamera : MonoBehaviour
    {
        [SerializeField] FieldParameter.Float _baseMoveSpeed;
        [SerializeField] float _cameraSensitivity;
        [SerializeField] float _scrollSensitivity;

        [SerializeField] bool _startFocused;
        [SerializeField] GameObject _focusedObjects;

        [SerializeField] RawImage _crosshair;

        [MethodButton(nameof(SnapToSceneView))]
        [SerializeField, Space(10)] MethodButton m_0;

        bool IsFocused => _focusedObjects.activeSelf;

        public static readonly UnityEvent<GameObject, Vector3> OnClickedObject = new();

        Camera _camera;
        Vector2 _lookRotation;

        float _moveSpeed;

        readonly KeyCode[] _moveKeys = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.LeftShift };
        readonly Vector3[] _moveDirections = new Vector3[6];

        const float CAMERA_PITCH_ANGLE_LIMIT = 90;

        const float CLICK_RAY_MAX_DISTANCE = 100;

        void Start()
        {
            _camera = GetComponentInChildren<Camera>(true);

            _lookRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
            _moveSpeed = _baseMoveSpeed.baseValue;

            SetFocused(_startFocused);
        }

        void Update()
        {
            if (Cursor.visible || Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                return;
            }

            if (Input.GetKey(KeyCode.Tab))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SetFocused(!IsFocused);
                }
                return;
            }

            if (IsFocused == false)
            {
                return;
            }

            float _mouseX = Input.GetAxis("Mouse X");
            float _mouseY = Input.GetAxis("Mouse Y");

            _lookRotation.x += _mouseX * _cameraSensitivity;
            _lookRotation.y -= _mouseY * _cameraSensitivity;
            _lookRotation.y = Mathf.Clamp(_lookRotation.y, -CAMERA_PITCH_ANGLE_LIMIT, CAMERA_PITCH_ANGLE_LIMIT);
            transform.rotation = Quaternion.Euler(_lookRotation.y, _lookRotation.x, 0);

            _moveDirections[0] =  transform.forward;
            _moveDirections[1] = -transform.forward;
            _moveDirections[2] = -transform.right;
            _moveDirections[3] =  transform.right;
            _moveDirections[4] =  transform.up;
            _moveDirections[5] = -transform.up;

            if (Input.mouseScrollDelta != Vector2.zero)
            {
                _moveSpeed += Input.mouseScrollDelta.y * _scrollSensitivity;
                _moveSpeed = _baseMoveSpeed.minMaxValue.GetClampedValue(_moveSpeed);
            }

            var _moveDirection = Vector3.zero;
            for (int i = 0; i < _moveKeys.Length; i++)
            {
                if (Input.GetKey(_moveKeys[i]))
                {
                    _moveDirection += _moveDirections[i];
                }
            }
            transform.position += _moveDirection.normalized * (Time.deltaTime * _moveSpeed);

            if (Input.GetKey(KeyCode.F))
            {
                _crosshair.gameObject.SetActive(true);

                var lookObject = GetLookObject(out var hitPoint);

                if (Input.GetMouseButtonUp(0))
                {
                    OnClickedObject.Invoke(lookObject, hitPoint);
                }
            }
            else
            {
                _crosshair.gameObject.SetActive(false);
            }
        }

        void SetFocused(bool isFocused)
        {
            _focusedObjects.SetActive(isFocused);
        }

        GameObject GetLookObject(out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, CLICK_RAY_MAX_DISTANCE) == false)
            {
                return null;
            }

            SceneDebug.HighlightObject(hit.transform.gameObject, true, hit.collider.bounds, ColorUtils.WHITE.SetAlpha(0.5f));

            hitPoint = hit.point;
            return hit.collider.gameObject;
        }

    #if UNITY_EDITOR
        void SnapToSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                SystemLog.Error("No active scene view");
                return;
            }

            var t = sceneView.camera.transform;
            transform.SetPositionAndRotation(t.position, t.rotation);
        }
    #endif
    }
}
