using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public sealed class TutorialModalService : IModalService
{
    private const string resourcesPath = "TutorialModalView";
    private ModalTheme theme;

    private IModalPresenter presenter;
    private readonly Queue<(ModalRequest, TaskCompletionSource<ModalResult>)> queue = new();
    private bool isPresenting;

    public Task<ModalResult> ShowAsync(ModalRequest req)
    {
        return Enqueue(req);
    }

    public void Show(ModalRequest req, Action<ModalResult> onResult = null)
    {
        _ = ShowAsync(req).ContinueWith(t => {
            if (t.IsCompletedSuccessfully) onResult?.Invoke(t.Result);
        });
    }
    private async Task<ModalResult> Enqueue(ModalRequest req)
    {
        var tcs = new TaskCompletionSource<ModalResult>();

        queue.Enqueue((req, tcs));
        if (!isPresenting)
            _ = ProcessQueue();

        return await tcs.Task;
    }

    private async Task ProcessQueue()
    {
        isPresenting = true;

        while (queue.Count > 0)
        {
            var (req, tcs) = queue.Dequeue();

            try
            {
                if (presenter == null)
                    presenter = await ModalLocator.BuildPresenter(resourcesPath, theme, PersistentReferences.Instance.tutorialModalViewport);

                var result = await presenter.Present(req);
                tcs.TrySetResult(result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                tcs.TrySetResult(ModalResult.None);
            }
        }

        isPresenting = false;
    }
    
    public void DismissActive(ModalResult reason = ModalResult.DismissedBackground)
        => presenter?.DismissActive(reason);

    public void DismissById(string correlationId, ModalResult reason = ModalResult.DismissedBackground)
        => presenter?.DismissById(correlationId, reason);
}