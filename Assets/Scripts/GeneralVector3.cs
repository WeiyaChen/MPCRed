using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralVector3  {

    public static Vector3 Vector3NoHeight(Vector3 position)
    {
        position.y = 0;
        return position;
    }

    public static float GetSlope2Points(Vector3 pointA,Vector3 pointB)
    {
        float result = (pointA.z - pointB.z) / (pointA.x - pointB.x);
        if (result == Mathf.NegativeInfinity || result == Mathf.Infinity)
            return Mathf.Infinity;
        else
            return result;
    }

    public static float GetConstant2Points(Vector3 pointA, Vector3 pointB)
    {
        float result = (pointB.x * pointA.z - pointA.x * pointB.z) / (pointA.x - pointB.x);
        if (result == Mathf.NegativeInfinity || result == Mathf.Infinity)
            return pointA.x;
        else
            return result;
    }

    public static Vector3 RotateCounterClockwise(Vector3 originPoint,Vector3 rotatePoint,float degree)
    {
        float x = (rotatePoint.x - originPoint.x) * Mathf.Cos(degree) - (rotatePoint.z - originPoint.z) * Mathf.Sin(degree) + originPoint.x;
        float z = (rotatePoint.x - originPoint.x) * Mathf.Sin(degree) + (rotatePoint.z - originPoint.z) * Mathf.Cos(degree) + originPoint.z;
        return new Vector3(x, 0, z);
    }

    public static float PointDistanceSeg(float k,float b, Vector3 point)
    {
        float result;
        if (k == Mathf.Infinity)
            result = Mathf.Abs(point.x - b);
        else
            result = Mathf.Abs((k * point.x - point.z + b) / Mathf.Sqrt(k * k + 1));
        return result;
    }

    public static Vector3 GetRealPoint(Vector3 originPoint,Vector3 xAxisPoint,Vector3 zAxisPoint,Vector3 virtualPoint)
    {
        bool xPositive, zPositive;//决定象限
        float x = PointDistanceSeg(GetSlope2Points(zAxisPoint, originPoint), GetConstant2Points(zAxisPoint, originPoint), Vector3NoHeight(virtualPoint));
        Debug.Log(x);
        float z = PointDistanceSeg(GetSlope2Points(xAxisPoint, originPoint), GetConstant2Points(xAxisPoint, originPoint), Vector3NoHeight(virtualPoint));
        Debug.Log(z);
        Vector3 xAxis = xAxisPoint - originPoint;
        Vector3 zAxis = zAxisPoint - originPoint;
        Vector3 pointSeg = Vector3NoHeight(virtualPoint) - originPoint;
        if (Vector3.Dot(xAxis, pointSeg) >=0)
        {
            xPositive = true;
        }
        else xPositive = false;
        if (Vector3.Dot(zAxis, pointSeg) >=0)
        {
            zPositive = true;
        }
        else zPositive = false;
        if (xPositive == true && zPositive == true) return new Vector3(x, 0, z);
        else if (xPositive == true && zPositive == false) return new Vector3(x, 0, -z);
        else if (xPositive == false && zPositive == false) return new Vector3(-x, 0, -z);
        else if (xPositive == false && zPositive == true) return new Vector3(-x, 0, z);
        return new Vector3(-100,-100,-100);
    }

    public static Vector3 LineDistance(Vector3 current,Vector3 target,float distance)
    {
        Ray ray=new Ray(current, target - current);
        return (ray.GetPoint(distance));
    }

 

}
