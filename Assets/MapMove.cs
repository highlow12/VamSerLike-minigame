using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMove : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;
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

        switch (transform.tag)
        {
            case "Ground":
                if(diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }
                //이 조건에선 플레이어가 y축보다 x축으로 이동을 하고 있는 것이니 타일맵도 마찬가지로 x축으로 이동시켜준다
                //Translate() : 지정된 값 만큼 현재위치에서 이동 
                //어느정도만큼 이동하겠다는 것이기 때문에 좌표가 아닌 이동할 양을 알려주는 것
                //
                //방향은 무조건 Vector3. 
                //Vector3.right에 마우스 올려놓으면 (1,0,0) 값 확인가능
                //하지만 플레이어가 오른쪽뿐 아니라 왼쪽으로 갈 수 도 있기 때문에 아까 만든 dirX를 곱한다 (왼쪽이면 -1)
                //타일맵 4개를 활용해 40X40사이즈이므로 *40
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 40);
                }


                break;
            case "Enemy":

                break;
        }
}
