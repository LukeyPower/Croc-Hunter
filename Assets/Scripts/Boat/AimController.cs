using Blamcon.Lightguns;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CrocHunter
{
    public class AimController : MonoBehaviour
    {
        public Vector2 ScreenPosition { get; private set; }

        void Awake()
        {
            ScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        void Update()
        {
            // Direct mouse polling so the crosshair always tracks even if PlayerInput isn't wired
            if (Mouse.current != null)
                ScreenPosition = Mouse.current.position.ReadValue();
        }

        // Called by PlayerInput event (lightgun / override)
        public void OnPoint(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            Vector2 raw = default;
            if (context.control is Vector2Control)
                raw = context.ReadValue<Vector2>();
            else if (context.control.device is Mouse)
                raw = Mouse.current.position.ReadValue();
            else if (context.control.device is Lightgun)
                raw = Lightgun.current.position.ReadValue();

            ScreenPosition = new Vector2(
                Mathf.Clamp(raw.x, 0f, Screen.width),
                Mathf.Clamp(raw.y, 0f, Screen.height)
            );
        }
    }
}
