using System.Collections.Generic;
using System.Threading.Tasks;

public static class ModalSequence
{
    public static async Task<bool> ShowAsync(
        IList<ModalRequest> steps,
        ModalSequenceOptions opt = null)
    {
        if (steps == null || steps.Count == 0) return false;
        opt ??= new ModalSequenceOptions();

        int i = 0;

        int total = steps.Count;

        while (i >= 0 && i < total)
        {
            opt.onStepShown?.Invoke(i);

            var isLast = (i == total - 1);
            var step = steps[i];

            string progress = $" ({i + 1}/{total})";
            step.progress = progress;

            step.showConfirm = true;
            step.confirmLabel = isLast ? "Finish" : "Next";

            if (opt.allowBack && i > 0)
            {
                step.showClose = true;
                step.closeLabel = "Back";
            }
            else
            {
                step.showClose = opt.allowSkip;
                step.closeLabel = opt.allowBack && i > 0 ? "Back" : "Skip";
            }

            var res = await ServiceProvider.modalService.ShowAsync(step);

            if (res == ModalResult.Confirmed)
            {
                i++;
            }
            else if (res == ModalResult.Closed)
            {
                if (i > 0 && opt.allowBack)
                {
                    i--;
                }
                else
                {
                    opt.onCanceled?.Invoke();
                    return false;
                }
            }
            else if (res == ModalResult.DismissedBackground)
            {
                opt.onCanceled?.Invoke();
                return false;
            }
            else
            {
                opt.onCanceled?.Invoke();
                return false;
            }
        }

        opt.onCompleted?.Invoke();
        return true;
    }
}
