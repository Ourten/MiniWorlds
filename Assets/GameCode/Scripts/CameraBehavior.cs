using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public float cameraVelocity = 20f;
    public Vector3 moveVector;
    public Vector3 moveVector2;
    public int cameraZoomMax = 50;
    public int cameraZoomMin = 10;

    void Start()
    {
    }

    void Update()
    {
        moveVector = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        moveVector = transform.TransformDirection(moveVector);
        moveVector.y = 0;
        moveVector.Normalize();
        moveVector *= cameraVelocity * Time.deltaTime;

        transform.Translate(moveVector, Space.World);

        moveVector2 = new Vector3(0, Input.GetAxis("Vertical"), 0);

        moveVector2 = transform.TransformDirection(moveVector2);
        moveVector2.y = 0;
        moveVector2.Normalize();
        moveVector2 *= cameraVelocity * Time.deltaTime;

        transform.Translate(moveVector2, Space.World);

        //Vector3 zoom variable.
        Vector3 zoom = transform.position;

        if (zoom.y < cameraZoomMin)
        {
            zoom.y = cameraZoomMin;
            transform.position = zoom;
        }
        if (zoom.y > cameraZoomMax)
        {
            zoom.y = cameraZoomMax;
            transform.position = zoom;
        }
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKey(KeyCode.Alpha2))
        {
            if (zoom.y < cameraZoomMax)
            {
                transform.Translate(Vector3.back * Time.deltaTime * 80);
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKey(KeyCode.Alpha1))
        {
            if (zoom.y > cameraZoomMin)
            {
                transform.Translate(Vector3.forward * Time.deltaTime * 80);
            }
        }
    }
}