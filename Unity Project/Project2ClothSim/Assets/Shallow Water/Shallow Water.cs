using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This sim is based on http://motioncore-umh.cs.umn.edu/5611/swe.html
[RequireComponent(typeof(MeshFilter))]
public class ShallowWater : MonoBehaviour
{
    // Start is called before the first frame update

    float[] heights;
    float[] momentums;

    public float cellWidth = 0.5f;
    public int cells = 10;

    public float gravity = 1;
    public float damp = 0.9f;

    public float timestepScale = 1;

    public bool drawMesh = true;
    private MeshFilter mf;

    void Awake()
    {
        //Init sim heights
        heights = new float[cells];
        momentums = new float[cells];

        for (int i=0; i < cells; i++)
        {
            heights[i] = (1 * Mathf.Sin(3.0f * i /(float)cells) + 1 );
            //Debug.Log(heights[i]);
            momentums[i] = 0.0f;
        }

        mf = GetComponent<MeshFilter>();

        BuildMesh();
    }

    void SimSW(float dt)
    {
        float[] dhdt = new float[cells]; // Height Derivative
        float[] dhudt = new float[cells]; // Momentum Derivative

        float[] hMid = new float[cells]; // Height MidPoint
        float[] huMid = new float[cells]; // Momentum Midpoint

        float[] dhdtMid = new float[cells]; // Height Derivative MidPoint
        float[] dhudtMid = new float[cells]; // Momentum Derivative Midpoint

        //Compute midpoint heights and momentums
        for (int i=0; i <cells-1;i++)
        {
            hMid[i] = (heights[i] + heights[i + 1]) / 2;
            huMid[i] = (momentums[i] + momentums[i + 1]) / 2;
        }

        //Compute Dir of midpoints
        for (int i = 0; i < cells-1; i++)
        {
            //Compute dh/dt (mid)
            float dhudxMid = (momentums[i + 1] - momentums[i]) / cellWidth;
            dhdtMid[i] = -dhudxMid;

            //Compute dhu/dt (mid)
            float dhu2dxMid = (Mathf.Pow(momentums[i + 1], 2) / heights[i + 1] - Mathf.Pow(momentums[i], 2) / heights[i]) / cellWidth;
            float dgh2dxMid = (gravity * Mathf.Pow(heights[i + 1], 2) - Mathf.Pow(heights[i], 2)) / cellWidth;
            dhudtMid[i] = -(dhu2dxMid + 0.5f * dgh2dxMid);
        }

        //Update Midpoints for 1/2 a timestep based on midpoint derivatives
        for (int i = 0; i < cells; i++)
        {
            hMid[i] += dhdtMid[i] * dt / 2;
            huMid[i] += dhudtMid[i] * dt / 2;
        }

        //Compute height and momentum updates (non-midpoint)
        for (int i = 1; i < cells-1; i++)
        {
            //Compute dh/dt
            float dhudx = (huMid[i] - huMid[i-1])/ cellWidth;
            dhdt[i] = -dhudx;

            //Compute dhu/dt
            float dhu2dx = (Mathf.Pow(huMid[i], 2) / hMid[i] - Mathf.Pow(huMid[i - 1], 2) / hMid[i - 1]) / cellWidth;
            float dgh2dx = (gravity * Mathf.Pow(hMid[i], 2) - Mathf.Pow(hMid[i - 1], 2)) / cellWidth;
            dhudt[i] = -(dhu2dx + 0.5f * dgh2dx);
        }

        //Update values (non midpoint) based on full timestep
        for (int i=0; i < cells; i++) {
            heights[i] += damp * dhdt[i] * dt;
            momentums[i] += damp * dhudt[i] * dt;
        }

        //Reflect the boundry conditions
        heights[0] = heights[1];
        heights[cells-1] = heights[cells-2];
        momentums[0] = -momentums[1];
        momentums[cells-1] = -momentums[cells-2];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SimSW(Time.fixedDeltaTime * timestepScale);
        UpdateMesh();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            for (int x=0; x<cells; x++)
            {
                //Calculate positions
                Vector3 basePoint = new Vector3(transform.position.x + (cellWidth * x), transform.position.y, transform.position.z);
                Vector3 peakPoint = new Vector3(transform.position.x + (cellWidth * x), transform.position.y + heights[x], transform.position.z);

                Gizmos.DrawLine(peakPoint, basePoint);
            }
        }
    }
    
    private void BuildMesh()
    {
        var mesh = new Mesh()
        {
            name = "Water Mesh"

        };
        
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3[] verts = new Vector3[cells * 2];
        Vector2[] uvs = new Vector2[cells * 2];

        //Setup Verts
        for (int x=0; x< cells; x++)
        {
            verts[(x * 2)] = new Vector3(x * cellWidth, heights[x], 0.0f);
            verts[(x*2) + 1] = new Vector3(x * cellWidth, 0.0f, 0.0f);

            uvs[(x * 2)] = new Vector2(x / (float)cells, 0.0f);
            uvs[(x * 2) + 1] = new Vector2(x / (float)cells, 1.0f);
        }

        //Asign verts and uvs
        mesh.vertices = verts;
        mesh.uv = uvs;

        //Add tri's
        int[] tris = new int[6*cells];

        for (int x=0; x <cells-1; x++)
        {
            int pos = x * 2;
            //Debug.Log(pos);
            tris[x * 6] = pos;
            tris[x * 6 + 1] = pos + 3;
            tris[x * 6 + 2] = pos + 1;

            tris[x * 6 + 3] = pos;
            tris[x * 6 + 4] = pos + 2;
            tris[x * 6 + 5] = pos + 3;
        }
        mesh.triangles = tris;

        //Recalc the mesh
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.mesh.MarkDynamic();

    }

    private void UpdateMesh()
    {
        Vector3[] verts = new Vector3[cells * 2];

        //Setup Verts
        for (int x = 0; x < cells; x++)
        {
            verts[(x * 2)] = new Vector3(x * cellWidth, heights[x], 0.0f);
            verts[(x * 2) + 1] = new Vector3(x * cellWidth, 0.0f, 0.0f);
        }

        mf.mesh.SetVertices(verts);
        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();
    }


}
