using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class WayPoint : MonoBehaviour {
    public enum turnType {
        straight,
        ninetyRight,
        ninetyLeft,
        semiCircleRight,
        semiCircleLeft
    }


    public struct WayPointSequence
    {
        public Transform transform;
        public int number;
        public turnType turnType;
    };

    public WayPointSequence[] wayPoints;

	// Use this for initialization
	void Start () {
		
	}

    private void Awake()
    {
        //初始化WayPoint标志物，进行排序
        GameObject[] wayPointsTemp = GameObject.FindGameObjectsWithTag("WayPoint");
        wayPoints = new WayPointSequence[wayPointsTemp.Length];//wayPonits Length=Temp.Length+1
        //temp variable in loop
        GameObject wayPointTemp = null;
        string wayPointName = null;
        int wayPointNumber = -1;

        string pattern = "[a-zA-Z_]+";
        string[] result = null;

        for (int i = 0; i < wayPointsTemp.Length; ++i)
        {
            wayPointTemp = wayPointsTemp[i];
            wayPointName = wayPointTemp.name;
            result = Regex.Split(wayPointName, pattern);
            wayPointNumber = Convert.ToInt32(result[1]);

            wayPoints[wayPointNumber-1].transform = wayPointTemp.transform;
            wayPoints[wayPointNumber-1].number = wayPointNumber;
            if(result.Length==2)
            {
                wayPoints[wayPointNumber-1].turnType = turnType.straight;
            }
            else
            {
                if (result[2] == "+90")
                    wayPoints[wayPointNumber - 1].turnType = turnType.ninetyRight;
                else if (result[2] == "-90")
                    wayPoints[wayPointNumber - 1].turnType = turnType.ninetyLeft;
                //TODO:正负180
            }
            //for (int j = 0; j < result.Length; j++)
            //{
            //    Debug.Log(j.ToString() + ":" + result[j]);
            //}
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
