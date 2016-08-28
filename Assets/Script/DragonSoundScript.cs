using UnityEngine;
using System.Collections;

public class DragonSoundScript : MonoBehaviour
{

    public Animator dragonAnimator;
    public AudioSource fireVoice;
    public AudioSource dieVoice;
    public AudioSource sleepVoice;
    public AudioSource tauntVoice;
    public AudioSource flyVoice;
    public AudioSource headAttackVoice;
    public AudioSource handAttackVoice;
    public AudioSource flyAttackVoice;
    public AudioSource runVoice;
    public AudioSource downVoice;

    int lastAnimationName;

    void Update()
    {
        int nameHash = dragonAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        if (lastAnimationName != nameHash)
        {
            lastAnimationName = nameHash;
            updateSound();
        }
    }

    void updateSound()
    {
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Sleep_1"))
        {
            startSleep();
        }
        else
        {
            stopSleep();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Taunt1"))
        {
            playTaunt();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fly_1")
            || dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Take_off")
            || dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Land"))
        {
            startFly();
        }
        else
        {
            stopFly();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack_1"))
        {
            playHeadAttack();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack_2"))
        {
            playHandAttack();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fly_3"))
        {
            playFlyAttack();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            startRun();
        }
        else
        {
            stopRun();
        }
        if (dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Die_1")
            || dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Wake_up"))
        {
            playDown();
        }
    }

    public void startFire()
    {
        fireVoice.Play();
    }

    public void stopFire()
    {
        fireVoice.Stop();
    }

    public void playDie()
    {
        dieVoice.Play();
    }

    public void startSleep()
    {
        sleepVoice.Play();
    }

    public void stopSleep()
    {
        sleepVoice.Stop();
    }

    public void playTaunt()
    {
        tauntVoice.Play();
    }

    public void startFly()
    {
        flyVoice.Play();
    }

    public void stopFly()
    {
        flyVoice.Stop();
    }

    public void playHeadAttack()
    {
        headAttackVoice.Play();
    }

    public void playHandAttack()
    {
        handAttackVoice.Play();
    }

    public void playFlyAttack()
    {
        flyAttackVoice.Play();
    }

    public void startRun()
    {
        runVoice.loop = true;
        runVoice.Play();
    }

    public void stopRun()
    {
        runVoice.loop = false;
    }

    public void playDown()
    {
        downVoice.Play();
    }
}
