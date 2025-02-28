using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="ParkoueSysy/Parkour Action")]

public class ParkourAction : ScriptableObject
{
    [SerializeField] string animName;
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    [SerializeField] public bool rotateToObs;


    [SerializeField] bool targetMatching;
    [SerializeField] AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchTargetWeight=new Vector3(0,1,0);

    [SerializeField]  float postAnimeDelay;

    public Vector3 matchPos {  get; set; }
    public Quaternion targetRotation { get; set; }
    public bool CheckPossible(ObstacleHitData hitData,Transform player)
    {
        var height= hitData.heightHit.point.y-player.position.y;
        
        Debug.Log(height);
        if (height>=minHeight&&height<=maxHeight)
        {
            if (rotateToObs)
            {
                targetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);
            }
            if(targetMatching)
            {
                matchPos=hitData.heightHit.point;
            }
            return true;
        }
        
        return false;
    }
    public bool CheckPillerToObsPossible(ObstacleHitData hitData, Transform player)
    {
        var height = hitData.topHit.point.y - player.position.y;

        Debug.Log(height);
        if (height >= minHeight && height <= maxHeight)
        {
            if (rotateToObs)
            {
                targetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);
            }
            if (targetMatching)
            {
                matchPos = hitData.topHit.point;
            }
            return true;
        }

        return false;
    }
    public bool CheckPredictedPossible(ObstacleHitData hitData, Transform player)
    {
        var height =  player.position.y- hitData.heightHit.point.y;

        Debug.Log(height);
        if (height >= minHeight && height <= maxHeight)
        {
            if (rotateToObs)
            {
                Vector3 direction = hitData.forwardHit.collider.transform.position - player.position;
                direction.y = 0; 
                targetRotation = Quaternion.LookRotation(direction);
               
            }
            if (targetMatching)
            {
                matchPos = hitData.heightHit.point;
            }
            return true;
        }

        return false;
    }
    public string AnimName()=> animName;
    public bool TargetMatching=>targetMatching;
    public  AvatarTarget MatchBodyPart=>matchBodyPart;
    public  float MatchStartTime=>matchStartTime;
    public  float MatchTargetTime=>matchTargetTime;
    public float PostAnimeDelay => postAnimeDelay;
    public Vector3 MatchTargetWeight => matchTargetWeight;

}
