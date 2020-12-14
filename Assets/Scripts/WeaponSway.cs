using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    private Vector3 originPos;

    private Vector3 currentPos;

    //무기가 최대 얼마나 흔들리지
    [SerializeField]
    private Vector3 limitPos;

    //정조준일때 최대 얼마나 흔들리지
    [SerializeField]
    private Vector3 fineSightLimitPos;

    //부드러움 정도
    [SerializeField]
    private Vector3 smoothSway;

    //필요한 컴포넌트
    [SerializeField]
    private GunController theGunController;

    // Start is called before the first frame update
    void Start()
    {
        originPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        TrySway();
    }

    private void TrySway()
    {
        if(Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
        {
            Swaying();
        }
        else
        {
            BackToOriginPos();
        }
    }

    private void Swaying()
    {
        float move_x = Input.GetAxisRaw("Mouse X");
        float move_y = Input.GetAxisRaw("Mouse Y");
        if (!theGunController.isFineSightMode)
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -move_x, smoothSway.x), -limitPos.x, limitPos.x),
                            Mathf.Clamp(Mathf.Lerp(currentPos.y, -move_y, smoothSway.y), -limitPos.y, limitPos.y),
                            originPos.z);
        }
        else
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -move_x, smoothSway.x), -fineSightLimitPos.x, fineSightLimitPos.x),
                               Mathf.Clamp(Mathf.Lerp(currentPos.y, -move_y, smoothSway.y), -fineSightLimitPos.y, fineSightLimitPos.y),
                               originPos.z);

        }
        transform.localPosition = currentPos;
    }

    private void BackToOriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}
