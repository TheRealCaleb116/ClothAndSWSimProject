using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game instance { get; private set; }

    public ParticleSystem ps;
    public WindZone wind;
    public float windSpeedOffset = 1.0f;
    public float baseEmitRate = 5.0f;
    public float emitScaler =1.0f;

    public bool doAirDrag = true;
    public Vector3 windVec = Vector3.zero;


    public void UpdateWindX(float value){ windVec.x = value; UpdateWind();}
    public void UpdateWindY(float value){ windVec.y = value; UpdateWind();}
    public void UpdateWindZ(float value){ windVec.z = value; UpdateWind();}

    public void UpdateWind()
    {
        if (doAirDrag == false || windVec == Vector3.zero)
        {
            ps.Stop();
        }
        else
        {
            ps.Play();
        }

        //Update wind particle sim
        wind.transform.rotation = Quaternion.LookRotation(windVec.normalized, Vector3.up);

        wind.windMain = windVec.magnitude * windSpeedOffset;

        ps.emissionRate = baseEmitRate + (windVec.magnitude * emitScaler);


    }


    public void UpdateDrag(bool value)
    {
        //Get all cloth sims
        ClothSim[] cloths = FindObjectsByType<ClothSim>(FindObjectsSortMode.None);
        foreach (ClothSim c in cloths)
        {
            c.doADrag = value;
        }
        doAirDrag = value;

        UpdateWind();

    }

    // Start is called before the first frame update
    void Awake()
    {
        //Setup singleton
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        UpdateWind();
    }


    public void GoToWaterSim()
    {
        SceneManager.LoadScene("ShallowWaterSim");
    }
}
