using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ParkourController : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] List<ParkourAction> obsParkourActions;
    [SerializeField] List<ParkourAction> obsVaultable;
    [SerializeField] ParkourAction slindingAction;

    EnvironmentScanner scanner;
    Animator anim;
    private CinemachineTransposer transposer;
    PlayerController playerController;
    bool inAction=false;
    bool inControl=true;
    public bool isSliding=false;
    bool isVaulting;

    [SerializeField] CinemachineFreeLook mainCam;
    [SerializeField] CinemachineVirtualCamera slidingCam;


    [SerializeField] ParkourAction pillerJump;
    [SerializeField] ParkourAction pillerToObsJump;


    [SerializeField] Vector3 pillerOffset;
    [SerializeField] float pillerJumpDelay;
    private void Awake()
    {
        scanner = GetComponent<EnvironmentScanner>();
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        inAction = anim.GetBool("inAction");
        inControl = anim.GetBool("inControle");
        var hitdata=scanner.ObstacleCheck();
        var predictedHitData=scanner.PredictedObstacleCheck();




        //For Jump And Climb
        if (Input.GetButtonDown("Jump") && !inAction&&!playerController.isHanging)
        {
            if (predictedHitData.forwardHitFound)
            {
                if(predictedHitData.forwardHit.collider.tag=="Piller")
                {
                    if (pillerJump.CheckPredictedPossible(predictedHitData, transform))
                    {
                        playerController.onPiller = true;
                        pillerJump.matchPos = predictedHitData.forwardHit.collider.transform.position + pillerOffset;
                        StartCoroutine(DoParkourAction(pillerJump));

                    }
                }
                else if(predictedHitData.forwardHit.collider.tag == "PillerToObs"&&playerController.onPiller)
                {
                    if (pillerToObsJump.CheckPillerToObsPossible(predictedHitData, transform))
                    {
                        playerController.onPiller = false;
                        Debug.Log(pillerToObsJump.matchPos);
                        StartCoroutine(DoParkourAction(pillerToObsJump));

                    }
                }
                
                
            }
            else if (hitdata.forwardHitFound )
            {
                Debug.Log(hitdata.distanceBtwObs);
                bool bre = false;
                if (hitdata.hitTag =="Vault")
                {
                    playerController.fallTimer = 0;
                    bre = TryRandomVaultAction(hitdata);
                    isVaulting = true;
                }
                else
                {
                    foreach (var action in obsParkourActions)
                    {
                        if (action.CheckPossible(hitdata, transform))
                        {
                            StartCoroutine(DoParkourAction(action));
                            bre = true;
                            break;
                        }
                    }
                }
                
                if (!bre)
                {
                    playerController.Jump();
                }
                

            }
            else
            {
                playerController.onPiller = false;
                playerController.Jump();
            }
        }

        //For Sliding
        if (Input.GetKeyDown(KeyCode.C) && !inAction)
        {
            if(hitdata.hitTag=="Slide")
            {
                isSliding=true;
                
                StartCoroutine(DoParkourAction(slindingAction));
                SwitchCamera(slidingCam, true);
            }
        }

    }
   
    private bool TryRandomVaultAction(ObstacleHitData hitdata)
    {
        HashSet<int> usedIndexes = new HashSet<int>();
        
        while (usedIndexes.Count < obsVaultable.Count)
        {
            int randomIndex = UnityEngine.Random.Range(0, obsVaultable.Count);

            if (usedIndexes.Contains(randomIndex))
                continue;

            usedIndexes.Add(randomIndex);

            var action = obsVaultable[randomIndex];

            if (action.CheckPossible(hitdata, transform))
            {
                StartCoroutine(DoParkourAction(action));
                return true;
            }
        }

        return false;
    }
    IEnumerator DoParkourAction(ParkourAction action)
    {
        playerController.SetControle(false, isVaulting);
        inAction =true;
        anim.SetBool("inAction", inAction);
        anim.SetBool("inControle", false);
        anim.CrossFade(action.AnimName(), 0.1f);
        yield return null;
        var animTime = anim.GetNextAnimatorStateInfo(0);
        if(!animTime.IsName(action.AnimName()))
        {
            Debug.Log("Wrong Animation name");
        }
        float timer = 0;
        while(timer <= animTime.length+((playerController.onPiller)?pillerJumpDelay:0))
        {
            timer += Time.deltaTime;
            if(action.rotateToObs)
            {
                transform.rotation=Quaternion.RotateTowards(transform.rotation, action.targetRotation, rotationSpeed * Time.deltaTime);
            }
            if(action.TargetMatching)
            
            {
                MatchTarget(action);
            }
            
            yield return null;
        }
        
        yield return new WaitForSeconds(action.PostAnimeDelay);
        
        inAction = false;
        if(isSliding) {SwitchCamera(slidingCam,false); }
        isSliding = false;
        isVaulting = false;
        
        anim.SetBool("inAction", inAction);
        anim.SetBool("inControle", true);
        playerController.SetControle(true, isVaulting);
    }
    void MatchTarget(ParkourAction action)
    {
        if(anim.isMatchingTarget)
        {
            return;
        }
        anim.MatchTarget(action.matchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchTargetWeight, 0), action.MatchStartTime, action.MatchTargetTime);
    }
    
    public void SwitchCamera(CinemachineVirtualCamera virtualCamera,bool active)
    {
        virtualCamera.Priority = active ? 30 : 5;
    }
    public bool IsVaulting=>isVaulting;
}
