using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Player.ActionHandlers;
using UnityEngine;
using Utils.Singleton;

namespace Camera
{
    public class CameraController : DontDestroyMonoBehaviourSingleton<CameraController>
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private float stopDuration = 0.3f;
        [SerializeField] private AnimationCurve stopAnimation = new AnimationCurve();
        [SerializeField] private float _moveDuration = 0.3f;
        [SerializeField] private AnimationCurve cameraMoveToAnimation = new AnimationCurve();
        [SerializeField] private Vector2 limitOffset = new Vector2(1f, 0.5f);

        public UnityEngine.Camera MainCamera => mainCamera;
        public Vector2 GetInputPosition() => MainCamera.ScreenToWorldPoint(Input.mousePosition);

        private InputHandler _inputHandler;
        private Vector2 _startPosition;
        private bool _isDrag;
        private Rect _limits;
        private Queue<Vector2> _speeds = new();
        private Vector2 _speed;
        private float _timeStop;
        private Coroutine _moveAnimation;

        private void Start()
        {
            _inputHandler = InputHandler.Instance;
            _inputHandler.SetDragEventHandlers(OnDragStart, OnDragEnd);
        }

        private void OnDestroy()
        {
            StopMoveTo();
            _inputHandler.ClearEvents(OnDragStart, OnDragEnd);
        }

        private void LateUpdate()
        {
            if (_isDrag)
            {
                var position = GetInputPosition();
                var moveDelta = _startPosition - position;
                _speeds.Enqueue(moveDelta);
                if (_speeds.Count > 15)
                {
                    _speeds.Dequeue();
                }

                MainCamera.transform.Translate(moveDelta);
            }
            else
            {
                if (_speeds.Count > 0)
                {
                    _timeStop = Time.deltaTime;
                    var sum = _speeds.Aggregate(Vector2.zero, (current, item) => current + item);
                    _speed = sum / _speeds.Count;
                    _speeds.Clear();
                }

                _timeStop += Time.unscaledDeltaTime;
                var t = _timeStop / stopDuration;
                if (t < 1f)
                {
                    t = stopAnimation.Evaluate(t);
                    var speed = Vector2.Lerp(_speed, Vector2.zero, t);
                    MainCamera.transform.Translate(speed);
                }
            }

            var pos = transform.position;
            if (!_limits.Contains(pos))
            {
                float newX = Mathf.Clamp(pos.x, _limits.min.x, _limits.max.x);
                float newY = Mathf.Clamp(pos.y, _limits.min.y, _limits.max.y);
                var deltaX = pos.x < _limits.min.x ? pos.x - _limits.min.x : pos.x > _limits.max.x ? pos.x - _limits.max.x : 0f;
                var deltaY = pos.y < _limits.min.y ? pos.y - _limits.min.y : pos.y > _limits.max.y ? pos.y - _limits.max.y : 0f;
                transform.position = new Vector3(newX + deltaX / 2f, newY + deltaY / 2f, pos.z);
            }
        }

        private void OnDragStart(Vector2 startPosition)
        {
            if (PlayerController.PlayerState != PlayerState.Scrolling || IsMoving())
                return;
            _startPosition = startPosition;
            _isDrag = true;
        }

        private bool IsMoving()
        {
            return _moveAnimation != null;
        }

        private void OnDragEnd(Vector2 finishPosition)
        {
            if (PlayerController.PlayerState != PlayerState.Scrolling)
                return;
            _isDrag = false;
        }

        public void SetLimits(Rect limits)
        {
            _limits = limits;
            _limits.x -= limitOffset.x / 2f;
            _limits.y -= limitOffset.y / 2f;
            _limits.width += limitOffset.x;
            _limits.height += limitOffset.y;
        }

        public void MoveTo(Vector2 position)
        {
            _moveAnimation = StartCoroutine(MoveToCoroutine(transform.position, position));
        }

        private IEnumerator MoveToCoroutine(Vector3 currentPosition, Vector3 targetPosition)
        {
            targetPosition.z = currentPosition.z;
            var t = 0f;
            while (t < _moveDuration)
            {
                t += Time.unscaledDeltaTime;
                var transformPosition = Vector3.Lerp(currentPosition, targetPosition, cameraMoveToAnimation.Evaluate(t / _moveDuration));
                MainCamera.transform.position = transformPosition;
                yield return null;
            }

            _moveAnimation = null;
        }

        private void StopMoveTo()
        {
            if (IsMoving())
            {
                StopCoroutine(_moveAnimation);
                _moveAnimation = null;
            }
        }
    }
}