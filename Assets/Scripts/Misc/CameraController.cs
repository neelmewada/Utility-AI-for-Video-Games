using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothTime = 0.2f;
    public bool enableFollow = true;

    private Vector3 m_Velocity;

    private void FixedUpdate()
    {
        if (target == null || !enableFollow)
            return;

        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref m_Velocity, smoothTime);
    }
}
