using System;

public class ProgressionChoice
{
    public string TypeLabel { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string RankText { get; set; }
    public UnityEngine.Sprite Icon { get; set; }
    public Action ApplyAction { get; set; }

    public void Apply()
    {
        ApplyAction?.Invoke();
    }
}
