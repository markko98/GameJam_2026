using System.Collections.Generic;

public interface ITutorialProgressStorage
{
    void SetStepDone(string tutorialId, string stepId, bool done = true);
    bool IsStepDone(string tutorialId, string stepId);

    bool AreAllDone(string tutorialId, IEnumerable<string> stepIds);

    void SetTutorialDone(string tutorialId, bool done = true);
    bool IsTutorialDone(string tutorialId);

    void ClearStep(string tutorialId, string stepId);
    void ClearTutorial(string tutorialId);
}