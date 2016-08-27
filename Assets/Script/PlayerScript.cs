using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    public Material bloodMaterial;
    public Transform headPosition;
    public Transform shieldPosition;
    public Transform dragonPosition;

    float bloodTransparent;
    System.DateTime lastFireTime;
    void Start()
    {
        bloodTransparent = 0;
        bloodMaterial.SetColor("_Color", new Color(1, 0, 0, bloodTransparent));
        lastFireTime = System.DateTime.Now;
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
        Vector3 targetDirection = ( headPosition.position - dragonPosition.position ).normalized;
        Quaternion direction = Quaternion.identity;
        float dragonDirection = -Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg - 90;
        while (dragonDirection < 0)
        {
            dragonDirection += 360;
        }
        float shieldDirection = shieldPosition.rotation.eulerAngles.y;
        while (shieldDirection < 0)
        {
            shieldDirection += 360;
        }
        if (Mathf.Abs(shieldDirection - dragonDirection) > 120)
        {
            if ((System.DateTime.Now - lastFireTime).TotalSeconds < 5)
            {
                lastFireTime = System.DateTime.Now;
                // hp--
            }
            showHurt();
        }
    }
    
    public void attackByFly()
    {
        //1.3m * scale 8.2
        if(headPosition.position.y < 1.3f * 8.2f)
        {
            return;
        }
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
