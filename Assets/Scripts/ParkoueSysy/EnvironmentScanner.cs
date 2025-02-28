using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [Header("Obstacle")]
    [SerializeField] LayerMask obstaclLayer;
    [SerializeField] float forwardRayLength = 0.8f;
    [SerializeField] float heightRayLength = 0.8f;
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] Vector3 climbForwardRayOffset= new Vector3(0, 2.5f, 0);

    [Header("Climbing Check")]
    [SerializeField] float climbingRayLength = 11f;
    [SerializeField] LayerMask climbingLayer;
    public int numberofRays = 12;


    [Header("PredictedObstacle")]
    [SerializeField] LayerMask predictedobstaclLayer;
    [SerializeField] float predictedforwardRayLength = 0.8f;
    [SerializeField] float predictedheightRayLength = 0.8f;
    [SerializeField] Vector3 predictedforwardRayOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] Vector3 predictedclimbForwardRayOffset = new Vector3(0, 2.5f, 0);

    public ObstacleHitData ObstacleCheck()
    {
        var hitData=new ObstacleHitData();
        var forwardOrigin = transform.position + forwardRayOffset;
        hitData.forwardHitFound= Physics.Raycast(forwardOrigin, transform.forward, 
                        out hitData.forwardHit,forwardRayLength,obstaclLayer);
        Debug.DrawRay(forwardOrigin,transform.forward*forwardRayLength,hitData.forwardHitFound?Color.red:Color.green);
        if(hitData.forwardHitFound)
        {
            hitData.hitTag = hitData.forwardHit.collider.tag;

            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
            hitData.distanceBtwObs= Vector3.Distance(transform.position, hitData.forwardHit.point);
            hitData.heightHitFound= Physics.Raycast(heightOrigin,Vector3.down,out hitData.heightHit
                ,heightRayLength,obstaclLayer);
            Debug.DrawRay(heightOrigin, Vector3.down*heightRayLength, hitData.heightHitFound ? Color.red : Color.green);
        }
        return hitData;
    }
    public ObstacleHitData PredictedObstacleCheck()
    {
        var hitData = new ObstacleHitData();
        var forwardOrigin = transform.position + predictedforwardRayOffset;
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward,
                        out hitData.forwardHit, predictedforwardRayLength, predictedobstaclLayer);
        Debug.DrawRay(forwardOrigin, transform.forward * predictedforwardRayLength, hitData.forwardHitFound ? Color.red : Color.green);
        if (hitData.forwardHitFound)
        {
            hitData.hitTag = hitData.forwardHit.collider.tag;

            var heightOrigin = hitData.forwardHit.point + Vector3.up * predictedheightRayLength;
            hitData.distanceBtwObs = Vector3.Distance(transform.position, hitData.forwardHit.point);
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit
                , predictedheightRayLength, predictedobstaclLayer);
            Debug.DrawRay(heightOrigin, Vector3.down * predictedheightRayLength, hitData.heightHitFound ? Color.red : Color.green);
            hitData.topHit=hitData.heightHit;
            hitData.heightHit.point = hitData.forwardHit.collider.bounds.center;
        }
        return hitData;
    }
    public ClimbObstacleHitData ClimbingCheck()
    {
        var climbInfo = new ClimbObstacleHitData();
        Vector3 climbOrigin=transform.position+Vector3.up*1.5f;
        Vector3 climbOffset = new Vector3(0, 0.19f, 0);
        for(int i=0;i<numberofRays;i++)
        {
            Debug.DrawRay(climbOrigin + climbOffset * i,transform.forward,Color.red);
            climbInfo.forwardHitFound = Physics.Raycast(climbOrigin + climbOffset * i, transform.forward
                                        , out climbInfo.forwardHit, climbingRayLength, climbingLayer);
            if (climbInfo.forwardHitFound)
            {
                //climbInfo.hitTag = climbInfo.forwardHit.collider.tag;

                climbInfo.matchPos=climbInfo.forwardHit.transform.position;
                return climbInfo;
            }
        }

        return climbInfo;

    }
}


public struct ClimbObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
    public string hitTag;
    public float distanceBtwObs;
    public Vector3 matchPos;

}
public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
    public RaycastHit topHit;
    public string hitTag;
    public float distanceBtwObs;
   
}
