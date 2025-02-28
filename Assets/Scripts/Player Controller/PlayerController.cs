using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.UI.Image;

public class PlayerController : MonoBehaviour
{

    private float moveInput;
    private float turnInput;


    private CharacterController cc;
    public Camera cam;
    private Animator anim;
    ParkourController parkourController;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float sprintTransSpeed = 5f;

    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float gravity = 9.8f;

    [SerializeField] Vector3 gravityRayOrigin;
    [SerializeField] float gravityRayLength = 0.1f;


    private float verticalVel;
    private float speed = 0;
    private Quaternion targetRotation;


    public bool inControl = true;
    public bool isGrounded;
    public bool isFalling;
    public bool isHanging;
    public bool inAction;
    public bool onPiller;
    public bool isSlidingDown;

    Vector3 move = Vector3.zero;
    Vector3 velocity;


    [Header("SlidingDown")]
    [SerializeField] LayerMask slidableLayer;
    [SerializeField] CinemachineVirtualCamera slidingDownCam;
    [SerializeField] float slidingSpeed = 5f;



    [Header("for jump ")]
    public bool isJumping;
   

    [Header("Landing Anime Name")]
    public string fallingAnime;
    public string landAnime1m;
    public string landAnime3m;
    public string landAnimem6m;
    public float fallTimer;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        parkourController = GetComponent<ParkourController>();
    }
    //private void Update()
    //{
    //    inAction = anim.GetBool("inAction");
    //    if (!inControl ||isHanging)
    //    {

    //        return;
    //    }
    //    InputManage();
    //    if (Mathf.Abs(moveInput) > 0 || Mathf.Abs(turnInput) > 0)
    //    {
    //        Turn(new Vector3(turnInput, 0, moveInput));
    //    }

    //    if (isSlidingDown)
    //    {
    //        if (IsOnSteepSlope(out Vector3 slideDirection))
    //        {
    //            SlideDownSlope(slideDirection);
    //        }
    //    }

    //    anim.SetFloat("Speed", speed);
    //    if(onPiller)return;

    //    Movement();
    //    if (isGrounded)
    //    {
    //        if (isFalling)
    //        {
    //            isFalling = false;
    //            isJumping = false;
    //            performLandingAction();
    //        }
    //        fallTimer = 0;
    //        isFalling = false;
    //    }
    //    else
    //    {
    //        if (isJumping)
    //        {
    //            fallTimer += Time.deltaTime / 2;
    //        }
    //        else
    //        {
    //            fallTimer += Time.deltaTime;
    //        }


    //        isFalling = true;
    //        anim.CrossFade(fallingAnime, .1f);
    //    }

    //    isGrounded = CheckGrounded();
    //}
    void Update()
    {
        inAction = anim.GetBool("inAction");
        if (!inControl || isHanging)
        {
            return;
        }

        isGrounded = CheckGrounded();

     
        if (IsOnSteepSlope(out Vector3 slideDirection))
        {
            if(!isSlidingDown)
            {
                parkourController.SwitchCamera(slidingDownCam, true);
            }
            
            isSlidingDown = true;
            SlideDownSlope(slideDirection);

     
            if (Input.GetButtonDown("Jump"))
            {
                SlideJump();
            }
            
            return;
        }
        else
        {
            parkourController.SwitchCamera(slidingDownCam, false);
            isSlidingDown = false;
            anim.SetBool("isSlidingDown", false);
        }

      
        InputManage();
        Turn(new Vector3(turnInput, 0, moveInput));

        if (onPiller) return;
        anim.SetFloat("Speed", speed);
        Movement();

       
        if (isGrounded)
        {
            if (isFalling)
            {
                isFalling = false;
                isJumping = false;
                performLandingAction();
            }
            fallTimer = 0;
        }
        else
        {
            if (isJumping)
            {
                fallTimer += Time.deltaTime / 2;
            }
            else
            {
                fallTimer += Time.deltaTime;
            }

            isFalling = true;
            anim.CrossFade(fallingAnime, 0.1f);
        }
    }
    void performLandingAction()
    {

        if (fallTimer < .45f)
        {
            anim.CrossFade(landAnime1m, 0.2f);
        }
        else if (fallTimer >= .45f && fallTimer <= .61f)
        {
            StartCoroutine(DoLandingAction(landAnime3m));
        }
        else if (fallTimer > .61f)
        {
            StartCoroutine(DoLandingAction(landAnimem6m));
        }
    }
    private void Movement()
    {
        Debug.Log("ff");
        move = new Vector3(turnInput, 0, moveInput);
        move = cam.transform.TransformDirection(move);

        if (Input.GetKey(KeyCode.LeftShift) && move != Vector3.zero)
        {
            speed = Mathf.Lerp(speed, sprintSpeed, sprintTransSpeed * Time.deltaTime * 2);
        }
        else if (move == Vector3.zero)
        {
            speed = Mathf.Lerp(speed, 0, sprintTransSpeed * Time.deltaTime * 2);
        }
        else
        {
            speed = Mathf.Lerp(speed, walkSpeed, sprintTransSpeed * Time.deltaTime * 2);
        }
        if (isJumping || isFalling) speed /= 1.05f;
        move *= speed;
        move.y = VerticalVel();
        cc.Move(move * Time.deltaTime);

    }
    float VerticalVel()
    {
        if (isGrounded && !isJumping)
        {
            verticalVel = -2f;
        }
        else
        {
            verticalVel -= gravity * Time.deltaTime;
        }

        return verticalVel;
    }
    public void Jump()
    {
        if (isGrounded)
        {
            anim.CrossFade("JumpUp", 0.2f);
            isJumping = true;
            verticalVel = Mathf.Sqrt(jumpForce * 2f * gravity);
        }
    }
    private void Turn(Vector3 inputDirection)
    {

        if (inputDirection.magnitude > 0.1f)
        {

            inputDirection = cam.transform.TransformDirection(inputDirection);

            inputDirection.Normalize();
            inputDirection.y = 0;

            targetRotation = Quaternion.LookRotation(inputDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }
    private void InputManage()
    {
        moveInput = UnityEngine.Input.GetAxis("Vertical");
        turnInput = UnityEngine.Input.GetAxis("Horizontal");
    }
    public void SetControle(bool inControle, bool isVaulting)
    {
        this.inControl = inControle;
        cc.enabled = inControle;

        if (!inControle)
        {
            anim.SetFloat("Speed", 0);
            speed = 0;
            targetRotation = transform.rotation;
        }

    }
    private bool CheckGrounded()
    {
        Vector3 origin = transform.position + gravityRayOrigin;

        float distance = gravityRayLength;
        Debug.DrawRay(origin, Vector3.down * distance, Color.red);
        return Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance);
    }
    IEnumerator DoLandingAction(string landName)
    {
        anim.SetBool("inAction", true);
        anim.SetBool("inControle", false);
        SetControle(false, parkourController.IsVaulting);
        anim.CrossFade(landName, 0.1f);
        yield return null;
        var animTime = anim.GetNextAnimatorStateInfo(0);
        float timer = 0;
        while (timer <= animTime.length)
        {
            timer += Time.deltaTime;
            if (anim.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }
            yield return null;
        }
        SetControle(true, parkourController.IsVaulting);
        anim.SetBool("inAction", false);
        anim.SetBool("inControle", true);
    }

    bool IsOnSteepSlope(out Vector3 slideDirection)
    {
        slideDirection = Vector3.zero;
        Vector3 origin = transform.position + gravityRayOrigin;

        float distance = gravityRayLength*3;
        Debug.DrawRay(origin, Vector3.down * distance, Color.red);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance,slidableLayer))
        {
            float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);

            if (slopeAngle > cc.slopeLimit)
            {
                slideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                return true;
            }
        }

        return false;
    }

    void SlideDownSlope(Vector3 slideDirection)
    {
        
        move = Vector3.zero;
        velocity = slideDirection * slidingSpeed;
        cc.Move(velocity * Time.deltaTime);
        anim.SetBool("isSlidingDown", true);
        
    }
    void SlideJump()
    {
        
        verticalVel = Mathf.Sqrt(jumpForce * 4f * gravity);
        isJumping = true;
        parkourController.SwitchCamera(slidingDownCam, false);
        isSlidingDown = false;
        anim.SetBool("isSlidingDown", false);
        anim.CrossFade("JumpUp", 0.2f);
    }


}