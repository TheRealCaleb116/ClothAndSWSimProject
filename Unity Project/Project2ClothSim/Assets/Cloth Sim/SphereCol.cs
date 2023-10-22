using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCol : MonoBehaviour
{
    public float radius = 1.0f/2;
    void Awake()
    {
        radius = transform.localScale.x/2;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == true)
        {
            Gizmos.color = Color.red;
           // Gizmos.DrawSphere(transform.position, radius);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
