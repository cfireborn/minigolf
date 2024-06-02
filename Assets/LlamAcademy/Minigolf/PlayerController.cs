using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace LlamAcademy.Minigolf
{
    [RequireComponent(typeof(Camera))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody Ball;
        [SerializeField] private Image PowerImage;
        [SerializeField] private float MaxForce;
        [SerializeField] private Gradient PowerGradient;
        [SerializeField] private CinemachineInputProvider RotationInputProvider;

        private Camera Camera;
        private int Putts = 0;
        private Vector2 InitialTouchPosition;
        private bool ShouldShowPower;

        private void Awake()
        {
            Camera = GetComponent<Camera>();
        }

        private void Start()
        {
            // these need testing
            // TouchSimulation.Enable(); apparently doesn't work in Unity 6, but with monobehavior it does
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += TouchOnFingerDown;
            Touch.onFingerMove += TouchOnFingerMove;
            Touch.onFingerUp += TouchOnFingerUp;
        }

        private void TouchOnFingerUp(Finger finger)
        {
            RotationInputProvider.enabled = false;
            if (!ShouldShowPower || PowerImage.rectTransform.sizeDelta.magnitude < 0.01f) return;
            PowerImage.gameObject.SetActive(false);
            Vector3 forceDirection = (Ball.transform.position - transform.position).normalized;
            forceDirection.y = 0;
            Ball.AddForce(MaxForce * GetForce(finger) * forceDirection);
            Putts++;
        }

        private void TouchOnFingerMove(Finger finger)
        {
            if (ShouldShowPower)
            {
                PowerImage.rectTransform.sizeDelta = new Vector2(PowerImage.rectTransform.sizeDelta.x, GetForce(finger));
                PowerImage.color = PowerGradient.Evaluate(PowerImage.rectTransform.sizeDelta.y / 100);
            }
        }

        private void TouchOnFingerDown(Finger finger)
        {
            Ray cameraRay = Camera.ScreenPointToRay(finger.screenPosition);
            InitialTouchPosition = finger.screenPosition;

            if (!Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Ball"))
                || hit.transform != Ball.transform)
            {
                RotationInputProvider.enabled = true;
                ShouldShowPower = false;
                return;
            }

            if (Ball.linearVelocity.magnitude > 0.01f)
            {
                ShouldShowPower = false;
                return;
            }
            ShouldShowPower = true;

            PowerImage.gameObject.SetActive(true);
            PowerImage.rectTransform.sizeDelta = new Vector2(PowerImage.rectTransform.sizeDelta.x, 0);
        }

        private float GetForce(Finger finger) => Mathf.Clamp(InitialTouchPosition.y - finger.screenPosition.y, 0, 100);

    }
}
