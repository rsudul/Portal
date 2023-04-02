using UnityEngine;

[System.Serializable]
public class MouseLook
{
    [SerializeField] private float sensitivityX = 2.0f;
    [SerializeField] private float sensitivityY = 2.0f;

    [SerializeField] private float maxLookUp = 90.0f;
    [SerializeField] private float maxLookDown = -90.0f;

    [SerializeField] private float smoothness = 16.0f;

    private Quaternion targetCharacterRot = Quaternion.identity;
    private Quaternion targetCamRot = Quaternion.identity;

    public void Init(Transform targetCharacter, Transform targetCam)
    {
        targetCharacterRot = targetCharacter.localRotation;
        targetCamRot = targetCam.localRotation;

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LookRotation(Transform targetCharacter, Transform targetCam)
    {
        float yRot = Input.GetAxis("Mouse X") * sensitivityX;
        float xRot = Input.GetAxis("Mouse Y") * sensitivityY;

        targetCharacterRot *= Quaternion.Euler(0.0f, yRot, 0.0f);
        targetCamRot *= Quaternion.Euler(-xRot, 0.0f, 0.0f);

        targetCamRot = Clamp(targetCamRot);

        targetCharacter.localRotation = Quaternion.Slerp(targetCharacter.localRotation, targetCharacterRot, smoothness * Time.deltaTime);
        targetCam.localRotation = Quaternion.Slerp(targetCam.localRotation, targetCamRot, smoothness * Time.deltaTime);
    }

    private Quaternion Clamp(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, maxLookDown, maxLookUp);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}