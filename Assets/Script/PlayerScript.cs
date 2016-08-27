using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {
    public Material bloodMaterial;
    float bloodTransparent;
    void Start()
    {
        bloodTransparent = 0;
        bloodMaterial.SetColor("_Color", new Color(1, 0, 0, bloodTransparent));
    }
    void Update()
    {
        if (bloodTransparent > 0)
        {
            bloodTransparent -=0.03f;
            bloodMaterial.SetColor("_Color", new Color(1, 0, 0, bloodTransparent));
        }
    }
    public void attackByFire()
    {
        //check shield
        showHurt();
    }
    
    public void attackByFly()
    {
        //check height
        showHurt();
    }

    public void attackByTrain()
    {
        showHurt();
    }

    public void attackByHand()
    {
        showHurt();
    }

    public void attackByHead()
    {
        showHurt();
    }

    void showHurt()
    {
        bloodTransparent = 0.5f;
    }
}
