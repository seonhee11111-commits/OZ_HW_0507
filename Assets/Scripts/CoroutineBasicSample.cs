using System.Collections;
using UnityEngine;

public class CoroutineBasicSample : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(MyCoroutine());
    }

    //코루틴 자체는 IEnumerator로 만듦

    private IEnumerator MyCoroutine()
    {
        Debug.Log("yield 전");
        yield return new WaitForSeconds(4.0f);
        //yield 반환 생성(new) 4초 웨이팅
        //new WaitForSeconds = 반환되어야 할 값 변수자리,
        //이 경우에서는 바로 생성
        Debug.Log("yield 후");

        //yield return null; 다음 프레임까지 대기 : 프레임 단위 이동, 페이드. 안전한 순차 call에 활용
        //yield return new WaitForSeconds(1f); Time.timescale 영향을 받는 1초 대기...
        //코루틴 응용구문은 슬라이드 참고

        //코루틴도 코루틴 내부에 new로 생성하는 것보다 awake혹은 start에서 new로 생성해놓고, 호출만 하는 것이 더 효율적
        //캐싱을 미리 해두고 .

        //코루틴을 List나 Array에 담는 것은 권장하지 않음.
        //코루틴 자체가 클래스이기도 하고 덩치가 좀 큼.(연산?)
        //CPU가 코루틴을 위해 담당자를 할당하는 거라고 생각하면 되는데, 3f라고 하면 3초 동안 가만히 카운팅만 하고 있는 담당자가 생기는 것. (비효율...)
        //거기다 매번 new를 하면 담당자를 매번 뽑아내야 하는 것임
        //가비지컬렉션 최적화 필요!



    }

}
