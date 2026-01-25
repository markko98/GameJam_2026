using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class TutorialService
{
    private sealed class TutorialControllerData
    {
        public TutorialController controller;
        public List<string> stepIds;
        public Action<string> onStepCompleted;
    }

    private readonly ITutorialProgressStorage storage;
    private readonly Dictionary<string, TutorialControllerData> tutorials = new();

    private readonly HashSet<string> knownIds = new();
    private readonly Dictionary<string, bool> completedCache = new();

    public TutorialService()
    {
        storage = new TutorialProgressStorage();
    }

    /// <summary>
    /// <summary>Register a controller and auto-complete on end.</summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="controller"></param>
    public void RegisterController(string tutorialId, TutorialController controller, IEnumerable<string> stepIds)
    {
        if (string.IsNullOrEmpty(tutorialId)) throw new ArgumentException("tutorialId is null/empty");
        if (controller == null) throw new ArgumentNullException(nameof(controller));
        if (stepIds == null) throw new ArgumentNullException(nameof(stepIds));

        UnregisterController(tutorialId);

        var ordered = new List<string>(stepIds);
        var lastStepId = ordered.Count > 0 ? ordered[^1] : null;

        var data = new TutorialControllerData()
        {
            controller = controller,
            stepIds = new List<string>(stepIds)
        };

        data.onStepCompleted = (stepId) =>
        {
            storage.SetStepDone(tutorialId, stepId, true);

            if (!string.IsNullOrEmpty(lastStepId) && stepId == lastStepId)
            {
                storage.SetTutorialDone(tutorialId, true);
            }
        };

        controller.StepCompleted += data.onStepCompleted;
        tutorials[tutorialId] = data;
    }

    /// <summary>Unregister and detach events</summary>
    public void UnregisterController(string tutorialId)
    {
        if (!tutorials.TryGetValue(tutorialId, out var data)) return;

        if (data.onStepCompleted != null && data.controller != null)
            data.controller.StepCompleted -= data.onStepCompleted;

        tutorials.Remove(tutorialId);
    }

    /// <summary>Begin the tutorial if not already completed</summary>
    public void BeginIfNeeded(string tutorialId, bool beginFromLastStep = true)
    {
        if (!tutorials.TryGetValue(tutorialId, out var data)) return;
        if (IsCompleted(tutorialId)) return;
        if (beginFromLastStep)
        {
            BeginFromLastIncomplete(tutorialId);
        }
        else
        {
            data.controller.Begin();
        }
    }

    /// <summary>Show exactly one step now; will End() after that step completes</summary>
    public void ShowSingleStep(string tutorialId, string stepId)
    {
        if (!tutorials.TryGetValue(tutorialId, out var data)) return;
        if (storage.IsStepDone(tutorialId, stepId)) return;

        data.controller.ShowStep(stepId, singleStep: true);
    }

    public bool IsCompleted(string tutorialId) => storage.IsTutorialDone(tutorialId);
    public bool IsStepCompleted(string tutorialId, string stepId) => storage.IsStepDone(tutorialId, stepId);

    /// <summary>Force-complete a whole tutorial (marks all done + controller.End())</summary>
    public void MarkCompleted(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId)) return;

        storage.SetTutorialDone(tutorialId, true);

        if (tutorials.TryGetValue(tutorialId, out var data) && data.controller.IsRunning)
            data.controller.End();
    }
    
    public void BeginFromLastIncomplete(string tutorialId)
    {
        if (!tutorials.TryGetValue(tutorialId, out var rec))
            return;

        if (storage.IsTutorialDone(tutorialId))
            return;

        // find the first step that is not yet completed
        string stepToResume = rec.stepIds.FirstOrDefault(s => !storage.IsStepDone(tutorialId, s));

        if (stepToResume == null)
        {
            storage.SetTutorialDone(tutorialId);
            return;
        }

        rec.controller.BeginFrom(stepToResume);
    }


    /// <summary>Reset a whole tutorial</summary>
    public void ResetTutorial(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId)) return;

        storage.ClearTutorial(tutorialId);

        if (tutorials.TryGetValue(tutorialId, out var data))
        {
            foreach (var sid in data.stepIds)
                storage.ClearStep(tutorialId, sid);
        }
    }

    /// <summary>Reset only a specific step</summary>
    public void ResetStep(string tutorialId, string stepId)
    {
        if (string.IsNullOrEmpty(tutorialId) || string.IsNullOrEmpty(stepId)) return;
        storage.ClearStep(tutorialId, stepId);
    }

    /// <summary>Reset every registered tutorial</summary>
    public void ResetAll()
    {
        foreach (var kv in tutorials)
            ResetTutorial(kv.Key);
    }
}