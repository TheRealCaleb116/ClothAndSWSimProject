using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaterSimUtil : MonoBehaviour
{
    public void GoToClothSim()
    {
        SceneManager.LoadScene("Cloth Sim Scene");
    }
}
