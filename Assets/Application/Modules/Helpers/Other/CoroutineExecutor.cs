using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class CoroutineExecutor : MonoBehaviour
{
    private static CoroutineExecutor _instance;

    public static CoroutineExecutor Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CoroutineExecutor");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<CoroutineExecutor>();
            }

            return _instance;
        }
    }

    public static void Run(IEnumerator coroutine)
    {
        Instance.StartCoroutine(coroutine);
    }
    public static void Run(Task<ModalResult> task, Action<ModalResult> onComplete)
    {
        Instance.StartCoroutine(Wait(task, onComplete));
    }

    private static IEnumerator Wait(Task<ModalResult> task, Action<ModalResult> onComplete)
    {
        while (!task.IsCompleted) yield return null;
        onComplete?.Invoke(task.IsCompletedSuccessfully ? task.Result : ModalResult.None);
    }
}