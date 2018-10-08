using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField]
    public float velocity;
    [SerializeField]
    private float velocityDelta;
    int count;
    float delta;

    // Use this for initialization
    void Awake () {
        count = 0;
	}
	
	// Update is called once per frame
	void Update () {
        VelocityChange();
        //gameObject.GetComponent<Transform>().Rotate(new Vector3(0, 10, 0) * Time.deltaTime, Space.Self);
        //Debug.Log(gameObject.GetComponent<Transform>().rotation.eulerAngles.y);
	}

    private void VelocityChange()
    {
        Random.InitState(Random.Range(0, 100));
        float randomNum = Random.value;
        if (velocity < 0.1) VelocityChangeMould(randomNum, (float)0.95);
        else if (velocity >= 0.1 && velocity < 0.2) VelocityChangeMould(randomNum, (float)0.8);
        else if (velocity >= 0.2 && velocity < 0.3) VelocityChangeMould(randomNum, (float)0.5);
        else if (velocity >= 0.3 && velocity < 0.4) VelocityChangeMould(randomNum, (float)0.2);
        else if (velocity >= 0.4 && velocity < 0.5) VelocityChangeMould(randomNum, (float)0.05);
        else if (velocity < 0.05) velocity += velocityDelta;
        else if (velocity > 0.6) velocity -= velocityDelta;
    }

    private void VelocityChangeMould(float randomNum,float threshold)
    {
        if (randomNum > threshold) velocity -= velocityDelta;
        else velocity += velocityDelta;
    }

   
}
