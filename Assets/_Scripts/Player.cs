using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera playerCam;

    [Header("Look")]
    [SerializeField] private float lookSensitivityX = 4.5f;
    [SerializeField] private float lookSensitivityY = 4.5f;

    [SerializeField] private float maxLookUp = 90.0f;
    [SerializeField] private float maxLookDown = -90.0f;

    [SerializeField] private float lookSmoothness = 64.0f;

    [Header("Movement")]
    [SerializeField] private float gravity = 20.0f;

    [SerializeField] private float moveSpeed = 4.0f;

    private CharacterController characterController;

    private Quaternion targetCharacterRot = Quaternion.identity;
    private Quaternion targetCamRot = Quaternion.identity;

    private float inputLookX = 0.0f;
    private float inputLookY = 0.0f;

    private float inputMoveX = 0.0f;
    private float inputMoveY = 0.0f;

    private float inputModifyFactor = 0.0f;

    private bool isGrounded = false;
    private Vector3 movement = Vector3.zero;

    private Headbob headbob;

    private bool lockMoveInput = false;
    private bool lockMouseInput = false;

    void Awake()
    {
        ServiceLocator.RegisterService<Player>(this);

        characterController = GetComponent<CharacterController>();
        InitLook();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        headbob = playerCam.GetComponent<Headbob>();
    }

    void Update()
    {
        if (!lockMouseInput)
        {
            inputLookX = Input.GetAxis("Mouse X");
            inputLookY = Input.GetAxis("Mouse Y");

            inputMoveX = Input.GetAxis("Horizontal");
            inputMoveY = Input.GetAxis("Vertical");

            targetCharacterRot *= Quaternion.Euler(0.0f, inputLookX * lookSensitivityX, 0.0f);
            targetCamRot *= Quaternion.Euler(-(inputLookY * lookSensitivityY), 0.0f, 0.0f);

            targetCamRot = MathUtility.ClampQuaternion(targetCamRot, maxLookDown, maxLookUp);

            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetCharacterRot, lookSmoothness * Time.deltaTime);
            playerCam.transform.localRotation = Quaternion.Slerp(playerCam.transform.localRotation, targetCamRot, lookSmoothness * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (!lockMoveInput)
        {
            inputModifyFactor = (inputLookX != 0.0f && inputLookY != 0.0f) ? 0.7071f : 1.0f;

            if (isGrounded)
            {
                movement = new Vector3(inputMoveX * inputModifyFactor, 0.0f, inputMoveY * inputModifyFactor);
                movement = transform.TransformDirection(movement) * moveSpeed;
                if (inputMoveX != 0.0f || inputMoveY != 0.0f)
                {
                    headbob.Play();
                }
                else
                {
                    headbob.Stop();
                }
            }
            else
            {
                headbob.Stop();
            }

            movement.y -= gravity * Time.fixedDeltaTime;

            isGrounded = (characterController.Move(movement * Time.fixedDeltaTime) & CollisionFlags.Below) != 0;
        }
    }

    private void InitLook()
    {
        targetCharacterRot = transform.localRotation;
        targetCamRot = playerCam.transform.localRotation;
    }

    public void SwitchLockMoveInput(bool locked)
    {
        lockMoveInput = locked;
    }

    public void SwitchLockMouseInput(bool locked)
    {
        lockMouseInput = locked;
    }

    public void Teleport(Vector3 teleportPosition)
    {
        Debug.Log("teleport position: " + teleportPosition);

        characterController.enabled = false;
        transform.position = teleportPosition;
        characterController.enabled = true;
    }
}
