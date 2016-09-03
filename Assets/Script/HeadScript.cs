using UnityEngine;
using System.Collections;

public class HeadScript : MonoBehaviour
{

    private static readonly float HEAD_DOWN_MIN_VELOCITY = -1f * 8.2f;
    private static readonly float HEAD_DOWN_MIN_HEIGHT = 1.2f * 8.2f; //1.2m * scale 8.2
    
    Vector3 lastHeadPosition;
    System.DateTime lastDownTime;
	
    void Start()
    {
        lastDownTime = System.DateTime.Now;
    }
	// Update is called once per frame
	void Update () {
        Vector3 headV = (transform.position - lastHeadPosition) / Time.deltaTime;
        lastHeadPosition = transform.position;
        if (headV.y < HEAD_DOWN_MIN_VELOCITY || transform.position.y < HEAD_DOWN_MIN_HEIGHT)
        {
            lastDownTime = System.DateTime.Now;
        }
    }
    
    public bool isHeadDown()
    {
        int downDuration = (int)(System.DateTime.Now - lastDownTime).TotalSeconds;
        return downDuration < 1;
    }
}
