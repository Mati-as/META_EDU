using System.Threading;
using UnityEngine;

public class SingletonAppChecker : MonoBehaviour
{
    #if !UNITY_EDITOR
    private static Mutex mutex;

    void Awake()
    {
        bool isNewInstance;
        string mutexName = "META_EDU"; // 시스템 전체에서 유일한 이름 사용

        mutex = new Mutex(true, mutexName, out isNewInstance);

        if (!isNewInstance)
        {
            Debug.Log("앱이 이미 실행 중입니다.");
            Application.Quit(); // 실행 중인 인스턴스가 있으면 종료
        }

        DontDestroyOnLoad(gameObject); // 선택적으로 씬 전환 시 유지
    }

    void OnApplicationQuit()
    {
        if (mutex != null)
        {
            mutex.ReleaseMutex();
            mutex.Close();
            mutex = null;
        }
    }
    #endif
}