using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    public Material bloodMaterial;
    public Transform headPosition;
    public Transform shieldPosition;
    public Transform dragonPosition;
    public VibrateAndSoundScript sword;
    public VibrateAndSoundScript shield;
    public AudioSource winBgm;
    public AudioSource gameBgm;

    bool colorIsRed = false;

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
            if (colorIsRed)
            {
                bloodMaterial.SetColor("_Color", new Color(1, 0, 0, bloodTransparent));
            }else
            {
                bloodMaterial.SetColor("_Color", new Color(1, 1, 1, bloodTransparent));
            }
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
            }else
            {
                shieldVibrate();
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
        shieldVibrate();
        swordVibrate();
    }

    public void attackByTrain()
    {
        showHurt();
        shieldVibrate();
        swordVibrate();
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
        colorIsRed = true;
        bloodTransparent = 0.5f;
    }

    public void hitHead()
    {
        colorIsRed = false;
        bloodTransparent = 0.5f;
        swordVibrate();
    }
    
    public void shieldVibrate()
    {
        shield.vibrate(500);
    }

    public void shieldVibrateWithSound()
    {
        shield.vibrateWithSound(500);
    }

    public void swordVibrate()
    {
        sword.vibrate(500);
    }

    public void win()
    {
        gameBgm.Stop();
        winBgm.Play();
    }
}
