using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    EnvironmentScanner scanner;
    PlayerController playerController;
    Animator anim;
    [SerializeField] float rotationSpeed;
    ClimbingPoint currentClimbPoint;

    Quaternion targetRotation;
     Vector3 matchPos { get; set; }
    bool isHanging;


    [Header("ClimbAnimation")]
    [SerializeField] ParkourAction IdelToClimb;
    [SerializeField] ParkourAction climbHoopUp;
    [SerializeField] ParkourAction climbHoopDown;
    [SerializeField] ParkourAction climbHoopRight;
    [SerializeField] ParkourAction climbhoopLeft;
    [SerializeField] ParkourAction climbShimmyLeft;
    [SerializeField] ParkourAction climbShimmyRight;
    [SerializeField] ParkourAction climbJumpFromWall;
    [SerializeField] ParkourAction climbToTop;



    [SerializeField] Vector3 ClimbhandOffset;
    [SerializeField] Vector3 ClimbShimmyhandOffset;

    bool doingAction;



    private void Start()
    {
        scanner = GetComponent<EnvironmentScanner>(); 
        playerController = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        var hitdata = scanner.ClimbingCheck();
        if (!playerController.isHanging)
        {
            if (Input.GetButtonDown("Jump") && !playerController.inAction && !doingAction)
            {
                if (hitdata.forwardHitFound)
                {
                    currentClimbPoint = hitdata.forwardHit.transform.GetComponent<ClimbingPoint>();
                    targetRotation = Quaternion.LookRotation(-hitdata.forwardHit.normal);
                    matchPos = currentClimbPoint.transform.position +
                           (currentClimbPoint.transform.right * ClimbhandOffset.x) +
                           (Vector3.up * ClimbhandOffset.y) +
                           (currentClimbPoint.transform.forward * ClimbhandOffset.z);
                    doingAction = true;
                    StartCoroutine(ClimbToLedge(IdelToClimb));
                }
            }
        }

        else
        {
            if(Input.GetButton("LeaveWall")&&!playerController.inAction)
            {
                StartCoroutine(JumpFromWall(climbJumpFromWall));
                return;
            }


            float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));
            float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
            var inputDirection = new Vector2(horizontal, vertical);

            if (playerController.inAction || inputDirection == Vector2.zero) return;

            if(currentClimbPoint.MountClimbToChrouchPoint&&inputDirection.y==1&&!doingAction)
            {
                StartCoroutine(ClimbToTop(climbToTop));
                return;
            }


            var neighbour = currentClimbPoint.GetNeighbour(inputDirection);
            if ((neighbour == null)) return;
            if(neighbour.connectionType==ConnectionType.Jump&&Input.GetButton("Jump")&&!doingAction)
            {
                currentClimbPoint=neighbour.ClimbingPoint;
                //matchPos=currentClimbPoint.transform.position+ClimbhandOffset;
             
                matchPos = currentClimbPoint.transform.position +
                           (currentClimbPoint.transform.right * ClimbhandOffset.x) +
                           (Vector3.up * ClimbhandOffset.y) +
                           (currentClimbPoint.transform.forward * ClimbhandOffset.z);

                targetRotation = Quaternion.LookRotation(-currentClimbPoint.transform.right, currentClimbPoint.transform.up);
                if (neighbour.pointDirection.y==1)
                {
                    doingAction = true;
                    StartCoroutine(ClimbToLedge(climbHoopUp));
                }
                else if (neighbour.pointDirection.y == -1)
                {
                    doingAction = true;
                    StartCoroutine(ClimbToLedge(climbHoopDown));
                }
                else if (neighbour.pointDirection.x == 1)
                {
                    doingAction=true;
                    StartCoroutine(ClimbToLedge(climbHoopRight));
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    doingAction = true;
                    StartCoroutine(ClimbToLedge(climbhoopLeft));
                }
            }
            else if ((neighbour.connectionType == ConnectionType.Move)&&!doingAction)
            {
                if (neighbour.ClimbingPoint == null)
                {
                    Debug.LogError("Neighbour's ClimbingPoint is null!");
                    return;
                }

                currentClimbPoint = neighbour.ClimbingPoint;

                
                matchPos = currentClimbPoint.transform.position+ClimbShimmyhandOffset;

                if (neighbour.pointDirection.x == 1)
                {
                    doingAction=true;
                    StartCoroutine(ClimbToLedge(climbShimmyRight));
                }
                else if (neighbour.pointDirection.x == -1)
                {
                    doingAction=true;
                    StartCoroutine(ClimbToLedge(climbShimmyLeft));
                }
            }
            //else if(neighbour.connectionType==ConnectionType.Move)
            //{
            //    currentClimbPoint = neighbour.ClimbingPoint;
            //    if(neighbour.pointDirection.x==1)
            //    {
            //        StartCoroutine(ClimbToLedge(climbShimmyRight));
            //    }
            //    else if (neighbour.pointDirection.x == -1)
            //    {
            //        StartCoroutine(ClimbToLedge(climbShimmyLeft));
            //    }
            //}
        }
    }

    IEnumerator ClimbToLedge(ParkourAction action)
    {
        action.matchPos=matchPos;
        Debug.Log(matchPos);
        playerController.isHanging = true;
        //anim.SetBool("inAction", true);
        anim.SetBool("inControle", false);
        playerController.SetControle(false, false);
        anim.CrossFadeInFixedTime(action.AnimName(), 0.2f);
        yield return null;
        var animTime = anim.GetNextAnimatorStateInfo(0);
        if (!animTime.IsName(action.AnimName()))
        {
            Debug.Log("Wrong Animation name");
        }
        float timer = 0;
        while (timer <= animTime.length)
        {
            timer += Time.deltaTime;
            if (action.rotateToObs)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            if (action.TargetMatching)
            {
                MatchTarget(action);
            }
            if (anim.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(action.PostAnimeDelay);
        doingAction = false;
;
    }
    IEnumerator JumpFromWall(ParkourAction action)
    {
        
        playerController.inAction = true;
        anim.SetBool("inAction", playerController.inAction);
        anim.SetBool("inControle", true);
        playerController.SetControle(true, false);
        anim.CrossFade(action.AnimName(), 0.1f);
        yield return null;
        var animTime = anim.GetNextAnimatorStateInfo(0);
        if (!animTime.IsName(action.AnimName()))
        {
            Debug.Log("Wrong Animation name");
        }
        float timer = 0;
        while (timer <= animTime.length)
        {
            timer += Time.deltaTime;
            if (action.rotateToObs)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.targetRotation, rotationSpeed * Time.deltaTime);
            }
            if (action.TargetMatching)
            {
                MatchTarget(action);
            }
            if (anim.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(action.PostAnimeDelay);

        playerController.inAction = false;

        playerController.isHanging = false;
        anim.SetBool("inAction", playerController.inAction);
        anim.SetBool("inControle", true);
    }
    IEnumerator ClimbToTop(ParkourAction action)
    {


        playerController.inAction = true;
        anim.SetBool("inAction", playerController.inAction);
        anim.SetBool("inControle", false);
        playerController.SetControle(false, false);
        anim.CrossFade(action.AnimName(), 0.2f);
        yield return null;
        var animTime = anim.GetNextAnimatorStateInfo(0);
        if (!animTime.IsName(action.AnimName()))
        {
            Debug.Log("Wrong Animation name");
        }
        float timer = 0;
        while (timer <= animTime.length)
        {
            timer += Time.deltaTime;
            
            if (action.TargetMatching)
            {
                MatchTarget(action);
            }
            if (action.rotateToObs)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            if (anim.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(action.PostAnimeDelay);

        playerController.inAction = false;
        playerController.SetControle(true, false);
        playerController.isHanging = false;
        anim.SetBool("inAction", playerController.inAction);
        anim.SetBool("inControle", true);
    }
    void MatchTarget(ParkourAction action)
    {
        if (anim.isMatchingTarget)
        {
            return;
        }
        anim.MatchTarget(action.matchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchTargetWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }
}
