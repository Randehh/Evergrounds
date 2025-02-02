
using Godot;
using System;

public class ExperienceService : IService, IWorldSaveable
{
    private const int BASE_LEVEL_REQUIREMENT = 10;
    private const float LEVEL_EXPONENT = 1.25f;

    private Godot.Collections.Dictionary<ExperienceType, int> experienceDictionary = new();
    private System.Collections.Generic.Dictionary<ExperienceType, ExperienceAnimator> experienceAnimators = new();

    public static int GetLevelForExperience(float totalExperience)
    {
        float totalExpLeft = totalExperience;
        int expRequired = BASE_LEVEL_REQUIREMENT;
        int currentLevel = 1;
        while (!Mathf.IsZeroApprox(totalExpLeft))
        {
            if (expRequired > totalExpLeft) break;
            totalExpLeft -= expRequired;
            currentLevel++;
            expRequired = Mathf.RoundToInt(expRequired * LEVEL_EXPONENT);
        }

        return currentLevel;
    }

    private static int GetExperienceForLevel(int level)
    {
        int currentLevel = 1;
        int totalExp = 0;
        int expRequired = BASE_LEVEL_REQUIREMENT;
        while (currentLevel < level)
        {
            currentLevel++;
            totalExp += expRequired;
            expRequired = Mathf.RoundToInt(expRequired * LEVEL_EXPONENT);
        }
        return totalExp;
    }

    public void OnInit()
    {
        
    }

    public void OnReady()
    {

    }

    public void OnDestroy()
    {

    }

    public void Process(double delta)
    {
        foreach (System.Collections.Generic.KeyValuePair<ExperienceType, ExperienceAnimator> pair in experienceAnimators)
        {
            if(!pair.Value.Update())
            {
                continue;
            }

            ServiceLocator.GameNotificationService.OnExperienceUpdated.Execute(pair.Value.GetUpdatePayload());
        }
    }

    public void AddExperience(ExperienceType experienceType, int experience)
    {
        int currentExp = GetExperience(experienceType);

        SetExperience(experienceType, currentExp + experience, true);
    }

    public void SetExperience(ExperienceType experienceType, int experience, bool fireAnimationEvents)
    {
        experienceDictionary[experienceType] = experience;

        if(!experienceAnimators.TryGetValue(experienceType, out ExperienceAnimator animator))
        {
            animator = new ExperienceAnimator(experienceType, experience, 0);
            experienceAnimators[experienceType] = animator;
        }

        animator.SetTarget(experience, fireAnimationEvents);
    }

    public int GetExperience(ExperienceType experienceType)
    {
        int currentExp = 0;
        if (experienceDictionary.TryGetValue(experienceType, out int existingExp))
        {
            currentExp = existingExp;
        }

        return currentExp;
    }

    public int GetLevel(ExperienceType experienceType)
    {
        int experience = GetExperience(experienceType);
        return GetExperienceForLevel(experience);
    }

    public Godot.Collections.Dictionary<string, Variant> GetSaveData()
    {
        Godot.Collections.Dictionary<string, Variant> data = new();

        foreach (System.Collections.Generic.KeyValuePair<ExperienceType, int> pair in experienceDictionary)
        {
            data.Add(pair.Key.ToString(), pair.Value);   
        }

        return data;
    }

    public void SetSaveData(Godot.Collections.Dictionary<string, Variant> data)
    {
        experienceDictionary.Clear();

        foreach (System.Collections.Generic.KeyValuePair<string, Variant> pair in data)
        {
            SetExperience((ExperienceType)Enum.Parse(typeof(ExperienceType), pair.Key), pair.Value.AsInt32(), false);
        }
    }

    private class ExperienceAnimator
    {
        public bool ShouldAnimate => target - current > 0.001f;

        private readonly ExperienceType experienceType;
        private int target;
        private float current;

        public ExperienceAnimator(ExperienceType experienceType, int target, float current)
        {
            this.experienceType = experienceType;
            this.target = target;
            this.current = current;
        }

        public void SetTarget(int target, bool animate)
        {
            this.target = target;

            if(!animate)
            {
                current = target;
            }
        }

        public bool Update()
        {
            if(!ShouldAnimate)
            {
                return false;
            }

            current = Mathf.Lerp(current, target, 0.05f);
            return true;
        }

        public ExperienceUpdatePayload GetUpdatePayload()
        {
            return new()
            {
                experienceType = experienceType,
                level = GetLevelForExperience(target),
                experience = target,
                animatedLevel = GetLevelForExperience(current),
                normalizedExperience = current
            };
        }
    }
}

public enum ExperienceType
{
    FARMING,
    LOGGING,
    MINING,
    CRAFTING
}

public struct ExperienceUpdatePayload
{
    public ExperienceType experienceType;
    public int level;
    public int experience;
    public int animatedLevel;
    public float normalizedExperience;
}