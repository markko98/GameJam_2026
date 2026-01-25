using System.Collections.Generic;

public sealed class TutorialProgressStorage : ITutorialProgressStorage
{
    public void SetStepDone(string tutorialId, string stepId, bool done = true)
        => ServiceProvider.storage.SaveTutorial($"{tutorialId}:{stepId}", done);

    public bool IsStepDone(string tutorialId, string stepId)
        => ServiceProvider.storage.LoadTutorial($"{tutorialId}:{stepId}");

    public bool AreAllDone(string tutorialId, IEnumerable<string> stepIds)
    {
        foreach (var step in stepIds)
            if (!IsStepDone(tutorialId, step)) return false;
        return true;
    }

    public void SetTutorialDone(string tutorialId, bool done = true)
        => ServiceProvider.storage.SaveTutorial($"{tutorialId}:_DONE_", done);

    public bool IsTutorialDone(string tutorialId)
        => ServiceProvider.storage.LoadTutorial($"{tutorialId}:_DONE_");

    public void ClearStep(string tutorialId, string stepId)
    {
        SetStepDone(tutorialId, stepId, false);
    }

    public void ClearTutorial(string tutorialId)
    {
        SetTutorialDone(tutorialId, false);
    }
}