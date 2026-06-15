using System;
using Corrnect.Core;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Corrnect.Input
{
    public class InputManager : MonoBehaviour
    {
        public event Action<Direction> TurnInput;

        [SerializeField] private bool inputEnabled = true;

        public bool InputEnabled
        {
            get => inputEnabled;
            set => inputEnabled = value;
        }

        private void Update()
        {
            if (!inputEnabled)
                return;

            if (TryGetDirection(out var direction))
                TurnInput?.Invoke(direction);
        }

        private static bool TryGetDirection(out Direction direction)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                {
                    direction = Direction.Up;
                    return true;
                }

                if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                {
                    direction = Direction.Down;
                    return true;
                }

                if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                {
                    direction = Direction.Left;
                    return true;
                }

                if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                {
                    direction = Direction.Right;
                    return true;
                }
            }
#endif
            if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                direction = Direction.Up;
                return true;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                direction = Direction.Down;
                return true;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                direction = Direction.Left;
                return true;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                direction = Direction.Right;
                return true;
            }

            direction = default;
            return false;
        }
    }
}
