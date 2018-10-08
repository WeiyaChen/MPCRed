using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redirector : MonoBehaviour {
    
    public struct MPCResult
    {
        public float cost;
        public ActionTaken action;
    };

    public enum RedirectorType
    {
        Absent,
        Rotation,
        Curvature
    };

    public enum ActionTaken
    {
        Zero=0,
        PositiveRotation=1,
        NegativeRotation=2,
        PositiveReset=3,
        NegativeReset=4,
        ClockwiseCurvature=5,
        CounterClockwiseCurvature=6
    };
    private int MPCResultNum=7;

    private int curvatureCalFrequence=25;

    [SerializeField]
    private GameObject player;
    private Transform playerTransform;
    [SerializeField]
    private GameObject sceneObject;
    [SerializeField]
    private float declineFactor;
    [SerializeField]
    private GameObject realRoom;
    private Transform realRoomTransform;

    public float curvatureRadius;

    [SerializeField]
    private float rotationCost;
    [SerializeField]
    private float resetCost;
    [SerializeField]
    private float curvatureCost;
    private float distanceThreshlod;

    [SerializeField]
    private float distanceCenterFactor;
    public float distanceCostMax;
    [SerializeField]
    private float parallelFactor;
    public float parallelCostMax;
    [SerializeField]
    private float actionFactor;

    //tracked space scale
    [SerializeField]
    private float width;
    [SerializeField]
    private float length;

    public float rotateGainEnlarge;
    public float rotateGainDecrease;
    public bool curvatureGain;

    private Vector3 lastPos;
    private float lastDiv;

    public int timeDepth;
    private float timeHorizon;

    private void Awake()
    {
        playerTransform = player.GetComponent<Transform>();
        if (playerTransform == null) DebugNull("playerTransform/Redirector");
        realRoomTransform = realRoom.GetComponent<Transform>();
        if (realRoomTransform == null) DebugNull("realRoomTransform/Redirector");
        timeHorizon = gameObject.GetComponent<DataRecord>().timeHorizon;
    }

    private void Start()
    {
        InitPlayerState();
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void MovePlayer()
    {
        
    }

    //virtual simulate VR Redirection
    public void ApplyRedirection(RedirectorType type, Transform player, float deltaPos, float deltaDiv, bool clockwise = true, float gain=0,float radius=0)
    {
        float rotateAngle;
        switch (type)
        {
            case RedirectorType.Absent: break;
            case RedirectorType.Curvature:
                rotateAngle = deltaPos * 360 / (2 * Mathf.PI * radius);
                if (!clockwise) rotateAngle = -rotateAngle; 
                sceneObject.GetComponent<Transform>().RotateAround(GeneralVector3.Vector3NoHeight(player.position), Vector3.up,rotateAngle);
                break;
            case RedirectorType.Rotation:
                rotateAngle = deltaDiv * (1-gain);//与VR redirection存在差别
                sceneObject.GetComponent<Transform>().RotateAround(GeneralVector3.Vector3NoHeight(player.position), Vector3.up, rotateAngle);
                break;
        }
    }

    private void DebugNull(string componentName)
    {
        Debug.Log("Cant Find " + componentName + " !");
    }

    private void InitPlayerState()
    {
        lastPos = GeneralVector3.Vector3NoHeight(playerTransform.position);
        lastDiv = playerTransform.rotation.eulerAngles.y;
    }

    public MPCResult MPCRedirect(DataRecord.Data data, DataRecord.WayPointsReal[] wayPoints, int depth)
    {
        MPCResult result =new MPCResult{ cost = 0, action = ActionTaken.Zero };

        if (depth == 0) return result;
        else
        {
            MPCResult tempResult = new MPCResult {  };
            MPCResult loopBestResult = new MPCResult { cost = Mathf.Infinity };

            int matchNum = NearestWayPoint(data.realPosition, wayPoints);
            //foreach(var wayPoint in wayPoints)
            //{
            //    Debug.Log(wayPoint.realPosition);
            //}

            //loop action
            for(int i=0;i<MPCResultNum;++i)
            {
                int nearestNumTemp;
                float distanceCost;
                float parallelCost;
                float rotationAngle;
                Vector3 endPosition;
                Vector3 endDir;
                DataRecord.WayPointsReal[] tempWayPoints;
                DataRecord.Data tempPlayerData= new DataRecord.Data();
                switch ((ActionTaken)i)
                {
                    case ActionTaken.Zero:
                        //foreach(var wayPoint in wayPoints)
                        //{
                        //    Debug.Log(wayPoint.realPosition);
                        //}
                        endPosition = ZeroAcition(wayPoints[matchNum], wayPoints[matchNum + 1], data.velocity);
                        //Debug.Log(endPosition);
                        nearestNumTemp = NearestWayPoint(endPosition, wayPoints);
                        distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                        endDir = new Vector3(wayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, wayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                        parallelCost = ParallelWallCost(endDir, parallelFactor);
                        tempPlayerData.realPosition = endPosition;
                        tempPlayerData.velocity = data.velocity;

                        tempResult.action = ActionTaken.Zero;
                        tempResult.cost = distanceCost + parallelCost + MPCRedirect(tempPlayerData, wayPoints, depth - 1).cost;
                        break;

                    case ActionTaken.PositiveRotation:
                        if (rotationCost > loopBestResult.cost) break;
                        else if (wayPoints[matchNum].turnType != WayPoint.turnType.ninetyLeft&& wayPoints[matchNum].turnType != WayPoint.turnType.ninetyRight) break;
                        else
                        {
                            if (wayPoints[matchNum].turnType == WayPoint.turnType.ninetyRight)
                            {
                                rotationAngle = (rotateGainEnlarge - 1) * 90;
                                tempWayPoints = RotateWayPoints(data.realPosition, wayPoints, rotationAngle);
                                endPosition = ZeroAcition(wayPoints[matchNum], tempWayPoints[matchNum + 1], data.velocity);
                                nearestNumTemp = NearestWayPoint(endPosition, tempWayPoints);
                                distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                                endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                                parallelCost = ParallelWallCost(endDir, parallelFactor);

                                tempPlayerData.realPosition = endPosition;
                                tempPlayerData.velocity = data.velocity;

                                tempResult.action = ActionTaken.PositiveRotation;
                                tempResult.cost = distanceCost + parallelCost +rotationCost+ MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost;
                            }
                            else if(wayPoints[matchNum].turnType==WayPoint.turnType.ninetyLeft)
                            {
                                rotationAngle = 360-(rotateGainEnlarge - 1) * 90;
                                tempWayPoints = RotateWayPoints(data.realPosition, wayPoints, rotationAngle);
                                endPosition = ZeroAcition(wayPoints[matchNum], tempWayPoints[matchNum + 1], data.velocity);
                                nearestNumTemp = NearestWayPoint(endPosition, tempWayPoints);
                                distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                                endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                                parallelCost = ParallelWallCost(endDir, parallelFactor);

                                tempPlayerData.realPosition = endPosition;
                                tempPlayerData.velocity = data.velocity;

                                tempResult.action = ActionTaken.PositiveRotation;
                                tempResult.cost = rotationCost+distanceCost + parallelCost+rotationCost + MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost;
                            }
                        }
                        break;
                    case ActionTaken.NegativeRotation:
                        if (rotationCost > loopBestResult.cost) break;
                        else if (wayPoints[matchNum].turnType != WayPoint.turnType.ninetyLeft && wayPoints[matchNum].turnType != WayPoint.turnType.ninetyRight) break;
                        else
                        {
                            if (wayPoints[matchNum].turnType == WayPoint.turnType.ninetyRight)
                            {
                                rotationAngle = 360-(1-rotateGainDecrease) * 90;
                                tempWayPoints = RotateWayPoints(data.realPosition, wayPoints, rotationAngle);
                                endPosition = ZeroAcition(wayPoints[matchNum], tempWayPoints[matchNum + 1], data.velocity);
                                nearestNumTemp = NearestWayPoint(endPosition, tempWayPoints);
                                distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                                endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                                parallelCost = ParallelWallCost(endDir, parallelFactor);

                                Debug.Log(distanceCost);

                                tempPlayerData.realPosition = endPosition;
                                tempPlayerData.velocity = data.velocity;

                                tempResult.action = ActionTaken.NegativeRotation;
                                tempResult.cost = rotationCost+distanceCost + parallelCost + MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost;
                            }
                            else if (wayPoints[matchNum].turnType == WayPoint.turnType.ninetyLeft)
                            {
                                rotationAngle = (1-rotateGainDecrease) * 90;
                                tempWayPoints = RotateWayPoints(data.realPosition, wayPoints, rotationAngle);
                                endPosition = ZeroAcition(wayPoints[matchNum], tempWayPoints[matchNum + 1], data.velocity);
                                nearestNumTemp = NearestWayPoint(endPosition, tempWayPoints);
                                distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                                endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                                parallelCost = ParallelWallCost(endDir, parallelFactor);

                                tempPlayerData.realPosition = endPosition;
                                tempPlayerData.velocity = data.velocity;

                                tempResult.action = ActionTaken.NegativeRotation;
                                tempResult.cost = rotationCost + distanceCost + parallelCost + MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost;
                            }
                        }
                        break;
                    case ActionTaken.PositiveReset:
                        if (resetCost > loopBestResult.cost) break;
                        else
                        {
                            tempWayPoints = RotateWayPoints(data.realPosition, wayPoints, 180);
                            endPosition = ZeroAcition(tempWayPoints[matchNum], tempWayPoints[matchNum + 1], data.velocity);
                            nearestNumTemp = NearestWayPoint(endPosition, tempWayPoints);
                            distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                            endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0,tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                            parallelCost = ParallelWallCost(endDir, parallelFactor);

                            tempPlayerData.realPosition = endPosition;
                            tempPlayerData.velocity = data.velocity;

                            tempResult.action = ActionTaken.PositiveReset;
                            tempResult.cost = distanceCost + parallelCost + MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost+resetCost;
                            break;

                        }
                    //case ActionTaken.NegativeReset:
                    //    if (resetCost > loopBestResult.cost) break;
                    //    else break;
                    case ActionTaken.ClockwiseCurvature:
                        Debug.Log("Here"+ loopBestResult.cost);
                        if (curvatureCost > loopBestResult.cost) break;
                        else
                        {
                            tempWayPoints = wayPoints;
                            endPosition = tempWayPoints[matchNum].realPosition;
                            int tempMatchNum = matchNum;
                            nearestNumTemp = matchNum;
                            float deltaPos = data.velocity * timeHorizon / curvatureCalFrequence;
                            float deltaAngle = 360 - (deltaPos * 360 / (2 * Mathf.PI * curvatureRadius));
                            for (int j=0;j<curvatureCalFrequence;j++)
                            {
                                endPosition = GeneralVector3.LineDistance(endPosition, tempWayPoints[tempMatchNum + 1].realPosition, deltaPos);
                                if (Vector3.Distance(endPosition, tempWayPoints[tempMatchNum + 1].realPosition) < distanceThreshlod)
                                {
                                    tempMatchNum++;
                                    nearestNumTemp = tempMatchNum;
                                }
                                tempWayPoints = RotateWayPoints(endPosition, tempWayPoints, deltaAngle);
                            }
                            distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                            endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                            parallelCost = ParallelWallCost(endDir, parallelFactor);

                            Debug.Log(distanceCost);

                            tempPlayerData.realPosition = endPosition;
                            tempPlayerData.velocity = data.velocity;

                            tempResult.action = ActionTaken.ClockwiseCurvature;
                            tempResult.cost = distanceCost + parallelCost + MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost + curvatureCost;
                            break;
                        }
                    case ActionTaken.CounterClockwiseCurvature:
                        if (curvatureCost > loopBestResult.cost) break;
                        else
                        {
                            tempWayPoints = wayPoints;
                            endPosition = tempWayPoints[matchNum].realPosition;
                            int tempMatchNum = matchNum;
                            nearestNumTemp = matchNum;
                            float deltaPos = data.velocity * timeHorizon / curvatureCalFrequence;
                            float deltaAngle = deltaPos * 360 / (2 * Mathf.PI * curvatureRadius);
                            for (int j = 0; j < curvatureCalFrequence; j++)
                            {
                                endPosition = GeneralVector3.LineDistance(endPosition, tempWayPoints[tempMatchNum + 1].realPosition, deltaPos);
                                if (Vector3.Distance(endPosition, tempWayPoints[tempMatchNum + 1].realPosition) < distanceThreshlod)
                                {
                                    tempMatchNum++;
                                    nearestNumTemp = tempMatchNum;
                                }
                                tempWayPoints = RotateWayPoints(endPosition, tempWayPoints, deltaAngle);
                            }
                            distanceCost = TowardWallCost(endPosition, width, length, distanceCenterFactor);
                            endDir = new Vector3(tempWayPoints[nearestNumTemp + 1].realPosition.x - endPosition.x, 0, tempWayPoints[nearestNumTemp + 1].realPosition.z - endPosition.z);
                            parallelCost = ParallelWallCost(endDir, parallelFactor);

                            tempPlayerData.realPosition = endPosition;
                            tempPlayerData.velocity = data.velocity;

                            tempResult.action = ActionTaken.CounterClockwiseCurvature;
                            tempResult.cost = distanceCost + parallelCost + MPCRedirect(tempPlayerData, tempWayPoints, depth - 1).cost + curvatureCost;
                            break;
                        }
                    default:break;
                }
                if (tempResult.cost < loopBestResult.cost) loopBestResult = tempResult;
            }
            Debug.Log(loopBestResult.cost);
            result = loopBestResult;

            //multply declinefactor
            if(depth!=timeDepth)
            {
                result.cost *= declineFactor;
            }
            //Debug.Log(result.cost);
            return result;
        }
    }

    private int NearestWayPoint(Vector3 playerPos, DataRecord.WayPointsReal[] wayPoints)
    {
        float minDistance = Mathf.Infinity;
        int matchNum = 0;
        float tempDistance;
        for (int j = 0; j < wayPoints.Length; ++j)
        {
            tempDistance = Vector3.Distance(playerPos, wayPoints[j].realPosition);
            if (tempDistance < minDistance)
            {
                minDistance = tempDistance;
                matchNum = j;
            }
        }
        return matchNum;
    }

    private DataRecord.WayPointsReal[] RotateWayPoints(Vector3 playerPos, DataRecord.WayPointsReal[] rotateWayPoint, float angle)
    {
        int length = rotateWayPoint.Length;
        DataRecord.WayPointsReal[] newWayPoints = new DataRecord.WayPointsReal[length];
        for (int i = 0; i < length; ++i)
        {
            newWayPoints[i].realPosition = GeneralVector3.RotateCounterClockwise(playerPos, rotateWayPoint[i].realPosition, angle);
            newWayPoints[i].turnType = rotateWayPoint[i].turnType;
        }
        return newWayPoints;
    }

    //cost
    private float TowardWallCost(Vector3 playerPos,float width,float length,float distanceDampFactor)
    {
        Debug.Log(playerPos);
        float widthDistance = playerPos.x <= (width - playerPos.x) ? playerPos.x : (width - playerPos.x);
        float lengthDistance=playerPos.z<=(length-playerPos.z)? playerPos.z : (length - playerPos.z);
        float result= widthDistance <= lengthDistance ? (1.0f / widthDistance * distanceDampFactor) : (1.0f / lengthDistance * distanceDampFactor);
        if (result == Mathf.Infinity || result == Mathf.NegativeInfinity||result>distanceCostMax) result=distanceCostMax;
        return result;
    }

    private float ParallelWallCost(Vector3 playerDir,float parallelDampFactor)
    {
        //float horizontalWall = Mathf.Abs(Vector3.Dot(Vector3.right, playerDir) / Vector3.Magnitude(playerDir));
        //float verticalWall = Mathf.Abs(Vector3.Dot(Vector3.forward, playerDir) / Vector3.Magnitude(playerDir));
        //float result= horizontalWall <= verticalWall ? 1.0f / horizontalWall * parallelDampFactor : 1.0f / verticalWall * parallelDampFactor;
        //if (result == Mathf.Infinity || result == Mathf.NegativeInfinity || result > parallelCostMax) result=parallelCostMax;
        //Debug.Log(result);
        return 0f;
    }
    

    //ZeroAction
    private Vector3 ZeroAcition(DataRecord.WayPointsReal playerPos, DataRecord.WayPointsReal wayPoint,float velocity=0)
    {
        Vector3 endPosition= GeneralVector3.LineDistance(playerPos.realPosition, wayPoint.realPosition, velocity * timeHorizon);
        return endPosition;
    }

    
   

}
