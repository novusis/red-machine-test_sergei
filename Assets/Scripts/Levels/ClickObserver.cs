using Connection;
using Events;
using Player.ActionHandlers;
using UnityEngine;

namespace Levels
{
    public class ClickObserver : MonoBehaviour
    {
        [SerializeField] private ColorConnectionManager colorConnectionManager;
        
        private InputHandler _inputHandler;

        private void Awake()
        {
            _inputHandler = InputHandler.Instance;
            
            _inputHandler.PointerDownEvent += OnPointerDown;
            _inputHandler.PointerUpEvent += OnPointerUp;
        }

        private void OnDestroy()
        {
            _inputHandler.PointerDownEvent -= OnPointerDown;
            _inputHandler.PointerUpEvent -= OnPointerUp;
        }
        
        private void OnPointerDown(Vector2 position)
        {
            colorConnectionManager.TryGetColorNodeInPosition(position, out var node);
            
            if (node != null)
                EventsController.Fire(new EventModels.Game.NodeTapped());
            else
                EventsController.Fire(new EventModels.Game.WorldTapped());
        }
        
        private void OnPointerUp(Vector2 position)
        {
            EventsController.Fire(new EventModels.Game.PlayerFingerRemoved());
        }
    }
}