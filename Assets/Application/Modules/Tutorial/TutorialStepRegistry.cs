using System.Collections.Generic;
using UnityEngine;

public static class TutorialStepRegistry
{
    public static readonly Dictionary<string, TutorialController.Step> AllSteps =
        new Dictionary<string, TutorialController.Step>
        {
            [TutorialAnchorIds.MainMenuPlaneSelection] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuPlaneSelection,
                stepId = TutorialStepIds.MainMenuPlaneSelection,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Plane selection",
                    description = "Before each run, here you can choose which plane you would like to fly with.",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.MainMenuTabHangar] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuTabHangar,
                stepId = TutorialStepIds.MainMenuTabHangar,
                dismissOnAnchorAction = true,
                modalRequest = new ModalRequest
                {
                    title = "Hangar",
                    description = "Let’s go ahead and upgrade your plane now.\nGo to hangar!",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.MainMenuHangarPlane] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuHangarPlane,
                stepId = TutorialStepIds.MainMenuHangarPlane,
                dismissOnAnchorAction = true,
                delayBeforeShow = 0.5f,
                modalRequest = new ModalRequest
                {
                    title = "Plane select",
                    description = "Click on the plane to show its details.",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            
            [TutorialAnchorIds.MainMenuHangarPlaneLevelUpCost] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuHangarPlaneLevelUpCost,
                stepId = TutorialStepIds.MainMenuHangarPlaneLevelUpCost,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Plane level up cost",
                    description = "Leveling planes costs coins and mechanical parts.\nHere you can see the requirements for each plane.\nWe have enough coins to upgrade this plane to Level 2!",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.MainMenuHangarPlaneLevelUp] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuHangarPlaneLevelUp,
                stepId = TutorialStepIds.MainMenuHangarPlaneLevelUp,
                dismissOnAnchorAction = true,
                modalRequest = new ModalRequest
                {
                    title = "Plane level up",
                    description = "Level up the plane to increase its stats.",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                    positionMode = ModalPositionMode.ViewportNormalized,
                    position = new Vector2(0.5f, 0.25f),
                    pivotOverride = new Vector2(0.5f, 0f),
                    clampToViewport = true,
                    viewportPadding = 16f,
                }
            },
            [TutorialAnchorIds.MainMenuHangarPlaneLevelText] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuHangarPlaneLevelText,
                stepId = TutorialStepIds.MainMenuHangarPlaneLevelText,
                dismissOnAnchorAction = false,
                delayBeforeShow = 1f,
                modalRequest = new ModalRequest
                {
                    title = "Plane level",
                    description = "You leveled up!\nHere you will see the current level of each plane.\nEach has a level limit where the max stats are reached.",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.MainMenuHangarPlaneStats] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuHangarPlaneStats,
                stepId = TutorialStepIds.MainMenuHangarPlaneStats,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Plane stats",
                    description = "As you level up your stats increase.\nHere you can track the state of each plane.",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.MainMenuTabPlay] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuTabPlay,
                stepId = TutorialStepIds.MainMenuTabPlay,
                dismissOnAnchorAction = true,
                modalRequest = new ModalRequest
                {
                    title = "Play",
                    description = "Lets go for another run",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                    pivotOverride = new Vector2(0.5f, 0f),
                }
            },

            [TutorialAnchorIds.MainMenuTabProfile] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuTabProfile,
                stepId = TutorialStepIds.MainMenuTabProfile,
                dismissOnAnchorAction = true,
                modalRequest = new ModalRequest
                {
                    title = "Profile",
                    description = "Check your profile",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                    pivotOverride = new Vector2(0.5f, 0f),
                }
            },
            
            [TutorialAnchorIds.MainMenuTabShop] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuTabShop,
                stepId = TutorialStepIds.MainMenuTabShop,
                dismissOnAnchorAction = true,
                modalRequest = new ModalRequest
                {
                    title = "Shop",
                    description = "Check shop",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.MainMenuPlayButton] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuPlayButton,
                stepId = TutorialStepIds.MainMenuPlayButton,
                dismissOnAnchorAction = true,
                delayBeforeShow = 0.5f,
                modalRequest = new ModalRequest
                {
                    title = "Play",
                    description = "Press play to go to game.",
                    showConfirm = false,
                    showClose = false,
                    showBackgroundBlur = false,
                    pivotOverride = new Vector2(0.5f, 0f),
                }
            },
            
            [TutorialAnchorIds.MainMenuTutorialChapterProgress] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.MainMenuTutorialChapterProgress,
                stepId = TutorialStepIds.MainMenuTutorialChapterProgress,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Chapter progress",
                    description = "Get 5 medals during the tutorial to unlock the next chapter, including the Hangar where you can unlock and upgrade new planes!",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.GameplayFuel] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplayFuel,
                stepId = TutorialStepIds.GameplayFuel,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Fuel",
                    description = "Your fuel is burning quick, make sure you pick it up under the floating balloons.",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                    // sprite = SpriteProvider.GetTutorialSprite(TutorialSpriteType.Fuel),
                }
            },
            [TutorialAnchorIds.GameplayFuelLow] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplayFuelLow,
                stepId = TutorialStepIds.GameplayFuelLow,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Fuel low",
                    description = "You are low on fuel!\n\nFly under the balloons to collect the gas canisters and refuel your plane or you will crash!",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                    // sprite = SpriteProvider.GetTutorialSprite(TutorialSpriteType.Fuel),
                    placement = ModalPlacement.Center,
                    positionMode = ModalPositionMode.ViewportNormalized,
                    position = new Vector2(0.5f, 0.5f),
                    pivotOverride = new Vector2(0.5f, 0.5f),
                    clampToViewport = true,
                    viewportPadding = 16f,
                }
            },
            [TutorialAnchorIds.GameplayJoystick] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplayJoystick,
                stepId = TutorialStepIds.GameplayJoystick,
                dismissOnAnchorAction = false,
                delayBeforeShow = 1f,
                modalRequest = new ModalRequest
                {
                    title = "Input",
                    description = "Move with joystick",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                    placement = ModalPlacement.Above,
                    positionMode = ModalPositionMode.ViewportNormalized,
                    position = new Vector2(0.5f, 0.5f),
                    pivotOverride = new Vector2(0.5f, 0.5f),
                    clampToViewport = true,
                    viewportPadding = 16f,
                }
            },
            [TutorialAnchorIds.GameplaySpeedMeter] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplaySpeedMeter,
                stepId = TutorialStepIds.GameplaySpeedMeter,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Speed",
                    description = "Here is your current speed",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.GameplayTimeGoal] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplayTimeGoal,
                stepId = TutorialStepIds.GameplayTimeGoal,
                dismissOnAnchorAction = false,
                delayBeforeShow = 0.25f,
                modalRequest = new ModalRequest
                {
                    title = "Time goal",
                    description = "In each run your goal is to reach the finish line as fast as possible. Beat the time scores shown before each run to earn medals!",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                }
            },
            [TutorialAnchorIds.GameplayCompletedRunTime] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplayCompletedRunTime,
                stepId = TutorialStepIds.GameplayCompletedRunTime,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Time goal",
                    description = "After each run, if you reached the finish line you will see your result time on this screen.",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                    positionMode = ModalPositionMode.ViewportNormalized,
                    position = new Vector2(0.5f, 0f),
                    pivotOverride = new Vector2(0.5f, 0f),
                    clampToViewport = true,
                    viewportPadding = 16f,
                    animationShowOverride = AnimationType.SlideInUp,
                    animationHideOverride = AnimationType.SlideOutDown,
                }
            },
            [TutorialAnchorIds.GameplayCompletedRunProgress] = new TutorialController.Step
            {
                anchorId = TutorialAnchorIds.GameplayCompletedRunProgress,
                stepId = TutorialStepIds.GameplayCompletedRunProgress,
                dismissOnAnchorAction = false,
                modalRequest = new ModalRequest
                {
                    title = "Progress",
                    description = "Each medal you get counts towards the pool of rewards for played chapter. When you collected all the medals in the current chapter you will unlock the next one.",
                    showConfirm = true,
                    showClose = false,
                    showBackgroundBlur = false,
                    positionMode = ModalPositionMode.ViewportNormalized,
                    position = new Vector2(0.5f, 0f),
                    pivotOverride = new Vector2(0.5f, 0f),
                    clampToViewport = true,
                    viewportPadding = 16f,
                    animationShowOverride = AnimationType.SlideInUp,
                    animationHideOverride = AnimationType.SlideOutDown,
                }
            },
        };

    public static List<TutorialController.Step> GetSteps(List<string> anchorIds)
    {
        var list = new List<TutorialController.Step>();
        foreach (var id in anchorIds)
        {
            if (AllSteps.TryGetValue(id, out var step))
                list.Add(step);
            else
                Debug.LogWarning($"[TutorialRegistry] Step not found for anchorId: {id}");
        }

        return list;
    }
}