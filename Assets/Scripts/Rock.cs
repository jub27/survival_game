using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField]
    private int hp;//바위의 체력

    [SerializeField]
    private float destroyTime;//파편 삭제 시간

    [SerializeField]
    private SphereCollider col;//구체 콜라이더

    [SerializeField]
    private GameObject go_rockBody;//일반 바위
    [SerializeField]
    private GameObject go_debris;//깨진 바위
    [SerializeField]
    private GameObject go_effect_prefabs;
    [SerializeField]
    private GameObject go_rock_item_prefab;//돌멩이 아이템 프리팹 

    //돌멩이 아이템 등장 개수
    [SerializeField]
    private int count;

    [SerializeField]
    private string strike_sound;
    [SerializeField]
    private string destroy_sound;

    public void Mining()
    {
        SoundManager.instance.PlaySE(strike_sound);
        GameObject clone = Instantiate(go_effect_prefabs, col.bounds.center, Quaternion.identity);
        Destroy(clone, destroyTime);
        hp--;
        if(hp <= 0)
        {
            Destruction();
        }
    }

    private void Destruction()
    {
        SoundManager.instance.PlaySE(destroy_sound);
        col.enabled = false;

        for (int i = 0; i < count; i++)
        {
            Instantiate(go_rock_item_prefab, go_rockBody.transform.position, Quaternion.identity);
        }

        Destroy(go_rockBody);

        go_debris.SetActive(true);
        Destroy(go_debris, destroyTime);
    }

}
