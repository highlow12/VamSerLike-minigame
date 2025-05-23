using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;

public class MapMove : MonoBehaviour
{
    public float offset = 2;
    public GameObject[] obstarcles;
    public GameObject[] obstarcles_Optional;
    public bool changeAngle = false;
    void Start()
    {
        setObstacle(obstarcles);
        if (obstarcles_Optional.Length > 0)
        {
            setObstacle(obstarcles_Optional);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {

        if (!collision.CompareTag("Player"))
        {
            return;
        }
        
        //플레이어의 position
        Vector3 playerpos = GameManager.Instance.player.transform.position;
        //이 오브젝트의 position
        Vector3 myPos = transform.position;
        //x축과 y축 거리 계산 => Math.Abs()함수를 사용해 절댓값으로 
        float diffX = Mathf.Abs(playerpos.x - myPos.x);
        float diffY = Mathf.Abs(playerpos.y - myPos.y);


        //플레이어 방향을 저장하기 위한 변수 추가 
        Vector3 playerDir = GameManager.Instance.player.inputVec;
        //3항 연산자 사용 (조건) ? (true일때 값) : (false때 값)
        //만약 normalized 가 없으면 굳이 이건 할 필욘 x
        float dirX = playerDir.x < 0 ? -1 : 1;
        float dirY = playerDir.y < 0 ? -1 : 1;

        if (diffX > diffY)
        {
            transform.Translate(Vector3.right * dirX * offset);
        }
        else if (diffX < diffY)
        {
            transform.Translate(Vector3.up * dirY * offset);
        }

        setObstacle(obstarcles);
        if (obstarcles_Optional.Length > 0)
        {
            setObstacle(obstarcles_Optional);
        }
    }

    void setObstacle(GameObject[] _obstarcles)
    {
        foreach (var item in _obstarcles)
        {
            item.transform.position = transform.position + new Vector3(Random.Range(-offset/4, offset/4), Random.Range(-offset/4, offset/4), 0);
            //item의 각도를 90도 간격으로 랜덤하게 변화
            if (changeAngle) item.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90);
        }
    }
}
