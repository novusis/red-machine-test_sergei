using System;
using Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Singleton;


namespace Player.ActionHandlers
{
    public class InputHandler : DontDestroyMonoBehaviourSingleton<InputHandler>
    {
        [SerializeField] private float clickToDragDuration;

        public event Action<Vector2> PointerDownEvent;
        public event Action<Vector2> ClickEvent;
        public event Action<Vector2> PointerUpEvent;
        public event Action<Vector2> DragStartEvent;
        public event Action<Vector2> DragEndEvent;

        private Vector2 _pointerDownPosition;

        private bool _isClick;
        private bool _isDrag;
        private float _clickHoldDuration;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                _isClick = true;
                _clickHoldDuration = .0f;

                _pointerDownPosition = CameraController.Instance.GetInputPosition();

                PointerDownEvent?.Invoke(_pointerDownPosition);

                _pointerDownPosition = new Vector2(_pointerDownPosition.x, _pointerDownPosition.y);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var pointerUpPosition = CameraController.Instance.GetInputPosition();

                if (_isDrag)
                {
                    DragEndEvent?.Invoke(pointerUpPosition);

                    _isDrag = false;
                }
                else
                {
                    ClickEvent?.Invoke(pointerUpPosition);
                }

                PointerUpEvent?.Invoke(pointerUpPosition);

                _isClick = false;
            }
        }

        private void LateUpdate()
        {
            if (!_isClick)
                return;

            _clickHoldDuration += Time.deltaTime;
            if (_clickHoldDuration >= clickToDragDuration)
            {
                DragStartEvent?.Invoke(_pointerDownPosition);

                _isClick = false;
                _isDrag = true;
            }
        }

        public void SetDragEventHandlers(Action<Vector2> dragStartEvent, Action<Vector2> dragEndEvent)
        {
            ClearEvents(dragStartEvent, dragEndEvent);

            DragStartEvent += dragStartEvent;
            DragEndEvent += dragEndEvent;
        }

        public void ClearEvents(Action<Vector2> dragStartEvent, Action<Vector2> dragEndEvent)
        {
            DragStartEvent -= dragStartEvent;
            DragEndEvent -= dragEndEvent;
        }
    }
}