using System.Collections.Generic;
using GameCode.Scripts.Gui;
using GameCode.Scripts.Inputs;
using UnityEngine;

public class SmoothMouseLook : MonoBehaviour
{
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public float frameCounter = 20;

    private float rotationX;
    private float rotationY;

    private List<float> rotArrayX = new List<float>();
    private float rotAverageX;

    private List<float> rotArrayY = new List<float>();
    private float rotAverageY;

    private Quaternion originalRotation;

    private void Update()
    {
        if (GuiManager.Instance.IsAnyGuiOpen() || !InputService.Singleton.IsCursorLocked)
            return;

        rotAverageY = 0f;
        rotAverageX = 0f;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;

        rotArrayY.Add(rotationY);
        rotArrayX.Add(rotationX);

        if (rotArrayY.Count >= frameCounter)
            rotArrayY.RemoveAt(0);
        if (rotArrayX.Count >= frameCounter)
            rotArrayX.RemoveAt(0);

        foreach (var rot in rotArrayY)
            rotAverageY += rot;
        foreach (var rot in rotArrayX)
            rotAverageX += rot;

        rotAverageY /= rotArrayY.Count;
        rotAverageX /= rotArrayX.Count;

        rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
        rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

        var yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
        var xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

        transform.localRotation = originalRotation * xQuaternion * yQuaternion;
    }

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
        originalRotation = transform.localRotation;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if (angle >= -360F && angle <= 360F)
        {
            if (angle < -360F)
                angle += 360F;

            if (angle > 360F)
                angle -= 360F;
        }

        return Mathf.Clamp(angle, min, max);
    }
}