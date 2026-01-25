using System.Threading.Tasks;
using UnityEngine;

public class ModalTutorialData
{
    public ModalViewController vc;
    public GameObject go;
    public TaskCompletionSource<ModalResult> tcs;

    public ModalTutorialData(ModalViewController vc, GameObject go, TaskCompletionSource<ModalResult> tcs)
    {
        this.vc = vc;
        this.go = go;
        this.tcs = tcs;
    }
}