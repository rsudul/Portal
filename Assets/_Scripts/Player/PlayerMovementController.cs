using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour, IPlayerMovementController, ISceneInjectee
{
    private Vector3 movement = Vector3.zero;

    private Vector2 moveInput = Vector2.zero;
    private float inputModifyFactor = 0.7071f;

    [Inject] private IPlayerCameraProvider playerCamera;
    [Inject] private IPlayerProvider player;

    private CharacterController characterController;

    [SerializeField] private float moveSpeed = 4.0f;

    [Header("Mouse Look")]
    [SerializeField] private MouseLook mouseLook = new MouseLook();

    private void Awake()
    {
        if (!TryGetComponent<CharacterController>(out characterController))
        {
            Debug.LogError(gameObject.name + ": CharacterController component not found.");
        }
    }

    private void Update()
    {
        ReadMovementInput();

        mouseLook.LookRotation(player.GetTransform(), playerCamera.GetTransform());
    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void ReadMovementInput()
    {
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
    }

    public void UpdateMovement()
    {
        movement.x = moveInput.x * (moveInput.x != 0.0f ? inputModifyFactor : 1.0f);
        movement.z = moveInput.y * (moveInput.y != 0.0f ? inputModifyFactor : 1.0f);

        movement = transform.TransformDirection(movement) * moveSpeed;

        characterController.Move(movement * Time.fixedDeltaTime);
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public void OnInjected()
    {
        mouseLook.Init(player.GetTransform(), playerCamera.GetTransform());
    }

    public void PrepareForTeleport()
    {
        characterController.enabled = false;
    }

    public void AfterTeleport()
    {
        characterController.enabled = true;
    }

    public void SetMovement(Vector3 n_movement)
    {
        movement = n_movement;
    }

    public Vector3 GetMovement()
    {
        return movement;
    }
}