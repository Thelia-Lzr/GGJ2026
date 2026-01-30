using System.Collections.Generic;
using UnityEngine;

public enum SkillStepType
{
    MoveToTarget,
    PlayStrike,
    ApplyDamage,
    ApplyStatus,
    TriggerPassives,
    ReturnToOrigin,
    ResolveDeath,
    PlayEffect,
    Wait
}

public class SkillStep
{
    public SkillStepType StepType { get; set; }
    public float Duration { get; set; }
    public object Data { get; set; }
    
    public SkillStep(SkillStepType stepType, float duration = 0f, object data = null)
    {
        StepType = stepType;
        Duration = duration;
        Data = data;
    }
}

public class SkillSequence
{
    private List<SkillStep> steps = new List<SkillStep>();
    private int currentStepIndex = 0;
    
    public UnitController Initiator { get; set; }
    public BattleUnit Target { get; set; }
    public ActionCommand Command { get; set; }
    
    public IReadOnlyList<SkillStep> Steps => steps.AsReadOnly();
    public int CurrentStepIndex => currentStepIndex;
    public bool IsComplete => currentStepIndex >= steps.Count;
    
    public SkillSequence()
    {
    }
    
    public void AddStep(SkillStep step)
    {
        steps.Add(step);
    }
    
    public void AddStep(SkillStepType stepType, float duration = 0f, object data = null)
    {
        steps.Add(new SkillStep(stepType, duration, data));
    }
    
    public SkillStep GetCurrentStep()
    {
        if (IsComplete)
            return null;
        
        return steps[currentStepIndex];
    }
    
    public void AdvanceStep()
    {
        currentStepIndex++;
    }
    
    public void Reset()
    {
        currentStepIndex = 0;
    }
}
