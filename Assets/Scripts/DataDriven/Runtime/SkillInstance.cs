public class SkillInstance
{
    public SkillData Data { get; set; }
    public int Rank { get; set; }
    public bool Locked { get; set; }
    public SkillBehaviourBase Runtime { get; set; }
}
