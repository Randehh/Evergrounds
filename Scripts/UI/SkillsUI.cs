using Godot;
using System;

[GlobalClass]
public partial class SkillsUI : Control
{
   
    [Export]
    private VBoxContainer skillsListParent;

    [Export]
    private PackedScene skillScene;

    public override void _Ready()
    {
        foreach(ExperienceType experienceType in Enum.GetValues(typeof(ExperienceType)))
        {
            ExperienceDisplay skillDisplay = skillScene.Instantiate<ExperienceDisplay>();
            skillsListParent.AddChild(skillDisplay);

            skillDisplay.SetTypeToTrack(experienceType);
        }
    }
}