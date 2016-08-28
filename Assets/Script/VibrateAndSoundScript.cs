using UnityEngine;
using System.Collections;

public class VibrateAndSoundScript : MonoBehaviour {

    public SteamVR_TrackedObject controller;
    public AudioSource effectAudio;
    SteamVR_Controller.Device device;

    System.DateTime startVibrateTime;
    int vibrateMilliSecond;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        System.TimeSpan duration = System.DateTime.Now - startVibrateTime;
        if (duration.TotalMilliseconds < vibrateMilliSecond)
        {
            device = SteamVR_Controller.Input((int)controller.index);
            device.TriggerHapticPulse();
        }
    }

    public void vibrate(int milliSecond)
    {
        //effectAudio.Play();
        startVibrateTime = System.DateTime.Now;
        vibrateMilliSecond = milliSecond;
    }
}
