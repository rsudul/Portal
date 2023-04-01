using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headbob : MonoBehaviour
{
    private Vector3 startPos = Vector3.zero;
    private Quaternion startRot = Quaternion.identity;

    private float theta = 0.0f;
    private float distance = 0.0f;

    [SerializeField] private float amplitude = 10.0f;
    [SerializeField] private float period = 5.0f;

    void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    public void Play()
    {
        theta = Time.timeSinceLevelLoad / period;
        distance = amplitude * Mathf.Sin(theta);

        transform.localPosition = startPos + Vector3.up * distance;
    }

    Vector3 vel = Vector3.zero;
    public void Stop()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, startPos, ref vel, 1 / period * Time.fixedDeltaTime);
    }
}