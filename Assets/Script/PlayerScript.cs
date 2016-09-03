using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    public TextMesh messageMesh;
    public Material bloodMaterial;
    public Transform shieldPosition;
    public Transform dragonPosition;
    public Transform dragonHeadPosition;
    public VibrateAndSoundScript sword;
    public VibrateAndSoundScript shield;
    public HeadScript head;
    public AudioSource winBgm;
    public AudioSource gameBgm;

    bool colorIsRed = false;
    float bloodTransparent;
    System.DateTime lastFireTime;
    int playerHp = 5;

    void Start()
    {
        bloodTransparent = 0;
        bloodMaterial.SetColor("_Color", new Color(1, 1, 1,bloodTransparent));
        lastFireTime = System.DateTime.Now;
    }
    
    void Update()
    {
        if (!isPlayerAlive())
        {
            if(sword.isTriggerPress() && shield.isTriggerPress())
            {
                Application.LoadLevel("Main Scene");
            }
            return;
        }
        if (bloodTransparent > 0)
        {
            bloodTransparent -=0.03f;
            if (colorIsRed)
            {
                bloodMaterial.SetColor("_Color", new Color(1, 0, 0, bloodTransparent));
                bloodMaterial.SetColor("_EmissionColor", new Color(1, 0, 0));
            }
            else
            {
                bloodMaterial.SetColor("_Color", new Color(1, 1, 1, bloodTransparent));
                bloodMaterial.SetColor("_EmissionColor", new Color(1, 1, 1));
            }
        }
    }
    public void attackByFire()
    {
        Vector3 targetDirection = (head.transform.position - dragonHeadPosition.position ).normalized;
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
        if (Mathf.Abs(shieldDirection - dragonDirection) > 90)
        {
            if ((System.DateTime.Now - lastFireTime).TotalSeconds > 3)
            {
                lastFireTime = System.DateTime.Now;
                playerHurt();
            }
            showHurt();
        }
        else
        {
            shieldVibrate();
        }
    }
    
    public void attackByFly()
    {
        if(head.isHeadDown())
        {
            return;
        }
        playerHurt();
        showHurt();
        shieldVibrate();
        swordVibrate();
    }

    public void attackByTrain()
    {
        playerHurt();
        showHurt();
        shieldVibrate();
        swordVibrate();
    }

    public void attackByHand()
    {
        playerHurt();
        showHurt();
    }

    public void attackByHead()
    {
        playerHurt();
        showHurt();
    }

    void showHurt()
    {
        if(playerHp <= 0)
        {
            return;
        }
        colorIsRed = true;
        bloodTransparent = 0.5f;
    }

    public void hitHead()
    {
        colorIsRed = false;
        bloodTransparent = 0.5f;
        swordVibrateWithSound();
    }
    
    public void shieldVibrate()
    {
        shield.vibrate(500);
    }

    public void shieldVibrateWithSound()
    {
        shield.vibrateWithSound(500);
    }

    public void swordVibrateWithSound()
    {
        sword.vibrateWithSound(500);
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

    public void playerHurt()
    {
        if (!isPlayerAlive())
        {
            return;
        }
        playerHp--;
        if(playerHp == 0)
        {
            messageMesh.text = "YOU DIED\n\nPress two trigger\nto restart";
            bloodMaterial.SetColor("_Color", new Color(1, 0, 0, 0.2f));
        }
    }

    public bool isPlayerAlive()
    {
        return playerHp > 0;
    }
}
