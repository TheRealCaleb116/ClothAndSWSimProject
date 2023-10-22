using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ClothSim : MonoBehaviour
{
    //TODO: 
    // - The cloth sim code could be cleaned up extensively, I ran out of time.

    public Vector2 cellSize = new Vector2(0.5f, 0.5f);
    public int clothNodesX = 10;
    public int clothNodesY = 10;

    //Spring Parameters
    public float ks = 800;       //Spring Constant
    public float kd = 80.0f;       //Dapening Factor

    //Air Drag Parameters
    float colVertexMargin = 0.05f;
    float sepMargin = 0.05f;

    public Vector3 gravity = new Vector3(0.0f, 10.0f, 0.0f);

    public float pDenAir = 1.38f;
    public float drgCoeficient = 1.0f;

    public bool doADrag = false;

    public float miscDamp = -0.001f; //Added this to remove the micro stutter that tends to happen when the cloth is almost stationary

    public ClothVertex[,] clothNodes = null;

    public Vector3 clothOrign;

    public bool drawMesh = true;

    //public Vector3 vAir = new Vector3(0, 0, 0);


    private MeshFilter mf;
    //private Mesh mesh;

    void Awake()
    {

        mf = GetComponent<MeshFilter>();

        //Create the node array
        clothNodes = new ClothVertex[clothNodesX, clothNodesY];
        //clothOrign = transform.position;

        //Create nodes for the cloth
        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX; x++)
            {
                //Create the vertex at the proper position

                //The local position of the vertexes
                Vector3 locPoint = new Vector3((x * cellSize.x), 0, (y * cellSize.y));

                ClothVertex v = new ClothVertex(transform.TransformPoint(locPoint));
                if (y == 0) { v.isFixed = true; }


                clothNodes[x, y] = v;
            }
        }

        if (drawMesh)
        {
            BuildClothMesh();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //Calculate gravity
        foreach (ClothVertex v in clothNodes)
        {
            if (!v.isFixed)
            {
                v.newVelocity += gravity * Time.fixedDeltaTime;

                //Added this to remove the micro stutter that tends to happen when the cloth is almost stationary
                //Cheasy dampening but it helps. Got the idea from opencloth
                v.newVelocity += miscDamp * v.velocity;
            }
        }

        //Simulate the Cloth

        //Do Vertical Links
        for (int x = 0; x < clothNodesX; x++)
        {
            for (int y = 0; y < clothNodesY - 1; y++)
            {

                //get the difference between positions
                Vector3 delta = clothNodes[x, y + 1].position - clothNodes[x, y].position;
                float len = delta.magnitude;

                //Get the velocity along the delta vector
                float vel1 = Vector3.Dot(delta.normalized, clothNodes[x, y].velocity);
                float vel2 = Vector3.Dot(delta.normalized, clothNodes[x, y + 1].velocity);

                //Calulate the spring force
                float f = -ks * (cellSize.y - len) - kd * (vel1 - vel2);

                //Set new velocities
                clothNodes[x, y].newVelocity += f * delta.normalized * Time.fixedDeltaTime;
                clothNodes[x, y + 1].newVelocity -= f * delta.normalized * Time.fixedDeltaTime;
            }
        }

        //Do Horizontal Links
        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX - 1; x++)
            {

                //get the difference between positions
                Vector3 delta = clothNodes[x + 1, y].position - clothNodes[x, y].position;
                float len = delta.magnitude;

                //Get the velocity along the delta vector
                float vel1 = Vector3.Dot(delta.normalized, clothNodes[x, y].velocity);
                float vel2 = Vector3.Dot(delta.normalized, clothNodes[x + 1, y].velocity);

                //Calulate the spring force
                float f = -ks * (cellSize.x - len) - kd * (vel1 - vel2);


                //Set new velocities
                clothNodes[x, y].newVelocity += f * delta.normalized * Time.fixedDeltaTime;
                clothNodes[x + 1, y].newVelocity -= f * delta.normalized * Time.fixedDeltaTime;
            }
        }


        //Calculate AreoDrag
        if (doADrag)
        {
            for (int y = 0; y < clothNodesY - 1; y++)
            {
                for (int x = 0; x < clothNodesX - 1; x++)
                {
                    //For each of the two tris
                    Vector3 r1 = clothNodes[x, y + 1].position;
                    Vector3 r2 = clothNodes[x + 1, y + 1].position;
                    Vector3 r3 = clothNodes[x, y].position;

                    Vector3 avgVelocity = ((clothNodes[x, y].velocity + clothNodes[x + 1, y].velocity + clothNodes[x, y + 1].velocity + clothNodes[x + 1, y + 1].velocity) / 4) - Game.instance.windVec;
                    Vector3 nStar = Vector3.Cross((r2 - r1), (r3 - r1));
                    Vector3 mv2an = ((avgVelocity.magnitude * Vector3.Dot(avgVelocity, nStar)) / (2 * nStar.magnitude)) * nStar;

                    Vector3 fAero = pDenAir * drgCoeficient * (-mv2an / 2);

                    clothNodes[x, y].newVelocity += (fAero / 4) * Time.fixedDeltaTime;
                    clothNodes[x + 1, y + 1].newVelocity += (fAero / 4) * Time.fixedDeltaTime;
                    clothNodes[x, y + 1].newVelocity += (fAero / 4) * Time.fixedDeltaTime;
                    clothNodes[x + 1, y].newVelocity += (fAero / 4) * Time.fixedDeltaTime;

                }
            }
        }

        //Apply movement
        foreach (ClothVertex v in clothNodes)
        {
            //If fixed, cancel velocity
            if (v.isFixed)
            {
                v.newVelocity = Vector3.zero;
            }

            //Update the positons of everything
            v.position += v.newVelocity * Time.fixedDeltaTime;
        }



        //Check for collisions
        //Get list of all Spheres
        SphereCol[] cols = FindObjectsByType<SphereCol>(FindObjectsSortMode.None);
        foreach (SphereCol sphere in cols)
        {
            //Check for collisions against every vertex in the cloth
            //Based on the slides
            for (int y = 0; y < clothNodesY; y++)
            {
                for (int x = 0; x < clothNodesX; x++)
                {
                    //Get dist
                    float d = Vector3.Distance(sphere.transform.position, clothNodes[x, y].position);
                    if (d < sphere.radius + colVertexMargin)
                    {
                        //We are colliding
                        Vector3 sNormal = -1.0f * (sphere.transform.position - clothNodes[x, y].position).normalized;
                        Debug.DrawLine(sphere.transform.position, clothNodes[x, y].position, Color.red);


                        Vector3 bounce = sNormal * Vector3.Dot(clothNodes[x, y].velocity, sNormal);

                        //Update Velocity
                        clothNodes[x, y].newVelocity -= 1.5f * bounce;

                        //Update Position
                        clothNodes[x, y].position += (sepMargin + sphere.radius - d) * sNormal;

                    }
                }
            }



        }


        //Update mesh
        UpdateVertexPos();

        //Update velcoties
        foreach (ClothVertex v in clothNodes)
        {
            //Update the positons of everything
            v.velocity = v.newVelocity;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == true)
        {
            bool test = false;
            foreach (ClothVertex v in clothNodes)
            {
                Gizmos.color = Color.yellow;
                if (test == false)
                {
                    test = true;
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawSphere(v.position, 0.05f);

            }
        }
    }

    void BuildClothMesh()
    {
        var mesh = new Mesh()
        {
            name = "Procedural Mesh"
        };

        GetComponent<MeshFilter>().mesh = mesh;

        //Build the mesh the first time

        //Build verts
        Vector3[] verts = new Vector3[clothNodes.Length * 2];
        Vector2[] uvs = new Vector2[clothNodes.Length * 2];

        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX; x++)
            {
                Vector3 point = transform.InverseTransformPoint(clothNodes[x, y].position);
                Vector3 Uv = new Vector2(x / ((float)clothNodesX - 1), 1 - y / (float)(clothNodesY - 1));
                //Add all the verts
                verts[(y * clothNodesX) + x] = point;
                uvs[(y * clothNodesX) + x] = Uv;

                //Duplicate for backside
                verts[clothNodes.Length + (y * clothNodesX) + x] = point;
                uvs[clothNodes.Length + (y * clothNodesX) + x] = Uv;

            }
        }
        //Asign verts and uvs
        mesh.vertices = verts;
        mesh.uv = uvs;


        //Add tri's
        int[] tris = new int[6 * clothNodesX * clothNodesY * 2];

        //Add a triangle
        for (int y = 0; y < clothNodesY - 1; y++)
        {
            for (int x = 0; x < clothNodesX - 1; x++)
            {
                //Add two triangles from the start vertex
                //Curent tri pos
                int pos = ((y * clothNodesX) + x);

                tris[pos * 6] = pos;
                tris[pos * 6 + 1] = pos + 1;
                tris[pos * 6 + 2] = pos + clothNodesX + 1;

                tris[pos * 6 + 3] = pos;
                tris[pos * 6 + 4] = pos + clothNodesX + 1;
                tris[pos * 6 + 5] = pos + clothNodesX;

                pos = (clothNodesX*clothNodesY) + ((y * clothNodesX) + x);

                //Make back tris
                tris[pos * 6] = pos;
                tris[pos * 6 + 1] = pos + clothNodesX + 1;
                tris[pos * 6 + 2] = pos + 1;

                tris[pos * 6 + 3] = pos;
                tris[pos * 6 + 4] = pos + clothNodesX;
                tris[pos * 6 + 5] = pos + clothNodesX + 1;

            }
        }
        //Assign the Tris
        mesh.triangles = tris;

        //Unity calculates the normals for us
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        mf.mesh.MarkDynamic();


    }

    void UpdateVertexPos()
    {

        Vector3[] verts = new Vector3[clothNodes.Length * 2];
        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX; x++)
            {
                Vector3 point = transform.InverseTransformPoint(clothNodes[x, y].position);

                //Add all the verts
                verts[(y * clothNodesX) + x] = point;

                //Duplicate for backside
                verts[clothNodes.Length + (y * clothNodesX) + x] = point; 

            }
        }

        mf.mesh.SetVertices(verts);
        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateTangents();
        mf.mesh.RecalculateBounds();
    }

}



public class ClothVertex
{
    public bool isFixed = false;

    public Vector3 newVelocity;
    public Vector3 position; //Global pos
    public Vector3 velocity;

    public ClothVertex(Vector3 startPos)
    {
        //Set inital position and velocity
        position = startPos;
        velocity = Vector3.zero;
    }

}







/*
[RequireComponent(typeof(MeshFilter))]
public class ClothSim : MonoBehaviour
{

    //TODO: 
    // - Meshing and Rendering
    // - Drag and wind
    // - Making the mesh transformable
    // - Camera controller
    // - Collision against sphere objects
    // - Second simulation?
    // - Tune Spring params
    // Is my dampeing force working right?
    // Optomize things
    //local space?

    public Vector2 cellSize = new Vector2(0.5f, 0.5f);
    public int clothNodesX = 10;
    public int clothNodesY = 10;

    //Spring Parameters
    public float ks = 800;       //Spring Constant
    public float kd = 80.0f;       //Dapening Factor
    float l0 = 0.0f;    //Rest Length

    //Air Drag Parameters
    float colVertexMargin = 0.05f;
    float sepMargin = 0.05f;

    public Vector3 gravity = new Vector3(0.0f, 10.0f, 0.0f);

    public float pDenAir = 1.38f;
    public float drgCoeficient = 1.0f;

    public bool doADrag = false;

    public ClothVertex[,] clothNodes = null;

    public Vector3 clothOrign;

    public bool drawMesh = true;

    public Vector3 vAir = new Vector3(0, 0, 0);


    private MeshFilter mf;
    //private Mesh mesh;

    void Awake()
    {
        l0 = cellSize.x;

        mf = GetComponent<MeshFilter>();

        //Create the node array
        clothNodes = new ClothVertex[clothNodesX, clothNodesY];
        //clothOrign = transform.position;

        //Create nodes for the cloth
        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX; x++)
            {
                //Create the vertex at the proper position

                //The local position of the vertexes
                Vector3 locPoint = new Vector3((x * cellSize.x), 0, (y * cellSize.y));

                // ClothVertex v = new ClothVertex(new Vector3(clothOrign.x + (x * cellSize.x), clothOrign.y, clothOrign.z - (y * cellSize.y)));
                ClothVertex v = new ClothVertex(transform.TransformPoint(locPoint));
                if (y == 0) { v.isFixed = true; }

                clothNodes[x, y] = v;
            }
        }

        if (drawMesh)
        {
            BuildClothMesh();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Apply the movment


        //Update acceleration
        //Calculate gravity
        foreach (ClothVertex v in clothNodes)
        {
            //calculate velocity for each particle based on positions
            //v.velocity = (v.position - v.lastPosition);

            //Add gravity a acceleration
            if (!v.isFixed)
            {
                v.acc = gravity * Time.deltaTime;
            }
        }

        //Simulate the Cloth
        
        //Do Vertical Links
        for (int x = 0; x < clothNodesX; x++)
        {
            for (int y = 0; y < clothNodesY - 1; y++)
            {

                //get the difference between positions
                Vector3 delta = clothNodes[x, y + 1].position - clothNodes[x, y].position;
                float len = delta.magnitude;

                //Get the velocity along the delta vector
                float vel1 = Vector3.Dot(delta.normalized, clothNodes[x, y].velocity);
                float vel2 = Vector3.Dot(delta.normalized, clothNodes[x, y + 1].velocity);

                //Calulate the spring force
                float f = -ks * (cellSize.y - len) - kd * (vel1 - vel2);

                //Set new velocities
                clothNodes[x, y].acc += f * delta.normalized * Time.fixedDeltaTime;
                clothNodes[x, y + 1].acc -= f * delta.normalized * Time.fixedDeltaTime;
            }
        }
        
        //Do Horizontal Links
        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX - 1; x++)
                {

                //get the difference between positions
                Vector3 delta = clothNodes[x + 1, y].position - clothNodes[x, y].position;
                float len = delta.magnitude;

                //Get the velocity along the delta vector
                float vel1 = Vector3.Dot(delta.normalized, clothNodes[x, y].velocity);
                float vel2 = Vector3.Dot(delta.normalized, clothNodes[x + 1, y].velocity);

                //Calulate the spring force
                float f = -ks * (cellSize.x - len) - kd * (vel1 - vel2);


                //Set new velocities
                clothNodes[x, y].acc += f * delta.normalized * Time.fixedDeltaTime;
                clothNodes[x + 1, y].acc -= f * delta.normalized * Time.fixedDeltaTime;
            }
        }
        
        
        //Calculate AreoDrag
        if (doADrag)
        {
            for (int y = 0; y < clothNodesY - 1; y++)
            {
                for (int x = 0; x < clothNodesX - 1; x++)
                {
                    //For each of the two tris
                    Vector3 r1 = clothNodes[x, y + 1].position; 
                    Vector3 r2 = clothNodes[x + 1, y + 1].position;
                    Vector3 r3 = clothNodes[x, y].position; 

                    Vector3 avgVelocity = ((clothNodes[x, y].velocity + clothNodes[x + 1, y].velocity + clothNodes[x, y + 1].velocity + clothNodes[x + 1, y + 1].velocity) / 4) - vAir;
                    Vector3 nStar = Vector3.Cross((r2 - r1), (r3 - r1));
                    Vector3 mv2an = ((avgVelocity.magnitude * Vector3.Dot(avgVelocity, nStar)) / (2 * nStar.magnitude)) * nStar;

                    Vector3 fAero = pDenAir * drgCoeficient * (-mv2an / 2);
                    
                    clothNodes[x, y].acc += (fAero / 4) * Time.deltaTime;
                    clothNodes[x + 1, y + 1].acc += (fAero / 4) * Time.deltaTime;
                    clothNodes[x, y + 1].acc += (fAero / 4) * Time.deltaTime;
                    clothNodes[x + 1, y].acc += (fAero / 4) * Time.deltaTime;
                }
            }
        }
        

        /*
        //Apply movement
        foreach (ClothVertex v in clothNodes)
        {
            //If fixed, cancel velocity
            if (v.isFixed)
            {
                v.newVelocity = Vector3.zero;
            }

            //Update the positons of everything
            v.position += v.newVelocity * Time.fixedDeltaTime;
        }
        */
/*
        //Apply movement
        foreach (ClothVertex v in clothNodes)
        {
            //If not fixed update position
            if (!v.isFixed)
            {
                //v.temp = v.position;
                v.position += v.velocity * Time.fixedDeltaTime + 0.5f * (v.acc * Time.fixedDeltaTime * Time.fixedDeltaTime);

                v.velocity += ((v.acc + v.lastAcc)/2) * Time.fixedDeltaTime;
                v.lastAcc = v.acc;
            }
        }




        //Check for collisions
        //Get list of all Spheres
        
        SphereCol[] cols = FindObjectsByType<SphereCol>(FindObjectsSortMode.None);
        foreach (SphereCol sphere in cols)
        {
            //Check for collisions against every vertex in the cloth
            //Based on the slides
            for (int y = 0; y < clothNodesY; y++)
            {
                for (int x = 0; x < clothNodesX; x++)
                {
                    //Get dist
                    float d = Vector3.Distance(sphere.transform.position, clothNodes[x, y].position);
                    if (d < sphere.radius + colVertexMargin)
                    {
                        //We are colliding
                        Vector3 sNormal = -1.0f * (sphere.transform.position - clothNodes[x, y].position).normalized;
                        Debug.DrawLine(sphere.transform.position, clothNodes[x, y].position, Color.red);


                        Vector3 bounce = sNormal * Vector3.Dot(clothNodes[x, y].velocity, sNormal);

                        //Update Velocity
                        clothNodes[x, y].velocity -= 1.5f * bounce;

                        //Update Position
                        clothNodes[x, y].position += (sepMargin + sphere.radius - d) * sNormal;

                    }
                }
            }



        }
        

        //Update mesh
        UpdateVertexPos();

        //Update velcoties
        /*
        foreach (ClothVertex v in clothNodes)
        {
            //Update the positons of everything
            v.lastPosition = v.position;
        }
        */
/*
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == true)
        {
            bool test = false;
            foreach (ClothVertex v in clothNodes)
            {
                Gizmos.color = Color.yellow;
                if (test == false)
                {
                    test = true;
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawSphere(v.position, 0.05f);

            }
        }
    }
    void BuildClothMesh()
    {
        var mesh = new Mesh()
        {
            name = "Procedural Mesh"
        };

        GetComponent<MeshFilter>().mesh = mesh;

        //Build the mesh the first time

        //Build verts
        Vector3[] verts = new Vector3[clothNodes.Length];
        Vector2[] uvs = new Vector2[clothNodes.Length];

        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX; x++)
            {
                //Add all the verts
                verts[(y * clothNodesX) + x] = clothNodes[x, y].position;
                uvs[(y * clothNodesX) + x] = new Vector2(x / ((float)clothNodesX - 1), 1 - y / (float)(clothNodesY - 1));

            }
        }
        //Asign verts and uvs
        mesh.vertices = verts;
        mesh.uv = uvs;


        //Add tri's
        int[] tris = new int[6 * clothNodesX * clothNodesY];

        //Add a triangle
        for (int y = 0; y < clothNodesY - 1; y++)
        {
            for (int x = 0; x < clothNodesX - 1; x++)
            {
                //Add two triangles from the start vertex
                //Curent tri pos
                int pos = ((y * clothNodesX) + x);

                tris[pos * 6] = pos;
                tris[pos * 6 + 1] = pos + 1;
                tris[pos * 6 + 2] = pos + clothNodesX + 1;

                tris[pos * 6 + 3] = pos;
                tris[pos * 6 + 4] = pos + clothNodesX + 1;
                tris[pos * 6 + 5] = pos + clothNodesX;
            }
        }
        //Assign the Tris
        mesh.triangles = tris;

        //Unity calculates the normals for us
        mesh.RecalculateNormals();
        mf.mesh.MarkDynamic();


    }
    void UpdateVertexPos()
    {
        Vector3[] verts = new Vector3[clothNodes.Length];

        for (int y = 0; y < clothNodesY; y++)
        {
            for (int x = 0; x < clothNodesX; x++)
            {
                //Add all the verts
                verts[(y * clothNodesX) + x] = transform.InverseTransformPoint(clothNodes[x, y].position);
            }
        }
        mf.mesh.SetVertices(verts);
        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();
    }

}


public class ClothVertex
{
    public bool isFixed = false;

    //public Vector3 lastPosition;
    //public Vector3 temp;
    public Vector3 position; //Global pos
    public Vector3 velocity;
    public Vector3 acc;
    public Vector3 lastAcc;

    public ClothVertex(Vector3 startPos)
    {
        //Set inital position and velocity
        position = startPos;
        //lastPosition = startPos;
        velocity = Vector3.zero;
        acc = Vector3.zero;
        lastAcc = Vector3.zero;
        //temp = Vector3.zero;
    }

}



//Tri 2
/*
r1 = p2;
r2 = p1;
r3 = p4;

avgVelocity = ((clothNodes[x, y].newVelocity + clothNodes[x + 1, y].newVelocity + clothNodes[x + 1, y + 1].newVelocity) / 3) - vAir;
nStar = Vector3.Cross((r2 - r1), (r3 - r1));
mv2an = ((avgVelocity.magnitude * Vector3.Dot(avgVelocity, nStar)) / (2 * nStar.magnitude)) * nStar;

fAero = pDenAir * drgCoeficient * (-mv2an / 2);

clothNodes[x, y].newVelocity += (fAero / 3) * Time.fixedDeltaTime;
clothNodes[x + 1, y + 1].newVelocity += (fAero / 3) * Time.fixedDeltaTime;
clothNodes[x + 1, y + 1].newVelocity += (fAero / 3) * Time.fixedDeltaTime;

*/
