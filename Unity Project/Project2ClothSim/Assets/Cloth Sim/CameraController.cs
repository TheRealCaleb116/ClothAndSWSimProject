using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float speed = 10.0f;
    public float vertSpeed = 5.0f;
    public float rotSpeedX = 2.0f;
    public float rotSpeedY = 2.0f;

    float rotX = 0.0f;
    float rotY = 0.0f;
    void Update()
    {

        if (Input.GetMouseButton(1))
        {
            //Do rotation
            rotX += rotSpeedX * Input.GetAxis("Mouse X");
            rotY -= rotSpeedY * Input.GetAxis("Mouse Y");

            //Rotate
            transform.eulerAngles = new Vector3(rotY, rotX, 0.0f);
        }
        
        //Move along look vector
        float deltaForward = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        //Strafe
        float deltaSide = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        //Up Down
        float deltaVert = 0.0f;
        if (Input.GetKey("space"))
        {
            deltaVert = 1.0f;
        }else if (Input.GetKey("left shift"))
        {
            deltaVert = -1.0f;
        }
        deltaVert = deltaVert * vertSpeed * Time.deltaTime;
        
        //Sum delta
        Vector3 deltaPos = Vector3.up* deltaVert +  transform.right * deltaSide + transform.forward * deltaForward;
        //Update pos
        transform.position += deltaPos;

    }



}
