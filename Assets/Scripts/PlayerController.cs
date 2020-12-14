using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    private float applySpeed;
    [SerializeField]
    private float crouchSpeed;

    [SerializeField]
    private float jumpForce;

    //땅 착지 여부를 위한 컴포넌트
    private CapsuleCollider capsuleCollder;

    //상태 변수
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;
    private bool isWalk = false;

    //움직임 체크 변수
    private Vector3 lastPos;

    //앉았을때 얼마나 앉을지 결정하는 변수.
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;


    //카메라 민감도
    [SerializeField]
    private float lookSensitivity;

    //카메라 한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotaionX = 0f;

    //카메라 컴포넌트
    [SerializeField]
    private Camera theCamera;


    private Crosshair theCrosshair;

    private Rigidbody myRigid;

    private GunController theGunController;
    private StatusController theStatusController;

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollder = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();

        theCrosshair = FindObjectOfType<Crosshair>();

        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;// 카메라의 캐릭터에 대한 상대위치
        applyCrouchPosY = originPosY;
        theGunController = FindObjectOfType<GunController>();
        theStatusController = FindObjectOfType<StatusController>();
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }

    private void FixedUpdate()
    {
        MoveCheck();
    }

    //앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    //앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);
        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        StartCoroutine(CrouchCoroutine());
    }

    //부드러운 앉기 동작
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;
        while(true)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, _posY, theCamera.transform.localPosition.z);
            if (count > 15)
                break;
            yield return null; // 1프레임 대기
        }
        theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
    }

    private void IsGround()
    {
        // 만약 Vector3.down 이 아니라 -transform.up 을 쓰면 캐릭터가 뒤집어져 있을때 이상해짐
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollder.bounds.extents.y + 0.3f); //bounds.extents 는 절반 크기, 0.1f로 약간의 여유(경사같은곳에서 안닿았다고 착각할수 있음)
        theCrosshair.JumpingAnimation(!isGround);
    }

    //점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && theStatusController.GetCurrentSp() > 0)
        {
            Jump();
        }
    }

    //점프 동작
    private void Jump()
    {
        if (isCrouch) // 앉은 상태에서 점프하면 앉은 상태 해제
            Crouch();
        theStatusController.DecreaseStamina(100);
        myRigid.velocity = transform.up * jumpForce;
    }

    //달리기 시도
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift) && theStatusController.GetCurrentSp() > 0)
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || theStatusController.GetCurrentSp() <= 0)
        {
            RunningCancel();
        }
    }

    //달리기
    private void Running()
    {
        if (isCrouch) //앉은 상태에서 달리기 할시 앉은 상태 해제
            Crouch();
        theGunController.CancelFineSight();
        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        theStatusController.DecreaseStamina(10);
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Vector3.Distance(lastPos, transform.position) >= 0.0000001f)
                isWalk = true;
            else
                isWalk = false;
            theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }
    }

    private void CameraRotation()// 상하 카메라 회전
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotaionX -= _cameraRotationX;
        currentCameraRotaionX = Mathf.Clamp(currentCameraRotaionX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotaionX, 0f, 0f);
    }

    private void CharacterRotation()//좌우 캐릭터 회전
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        //Debug.Log(myRigid.rotation);
        //Debug.Log(myRigid.rotation.eulerAngles);
    }
}
