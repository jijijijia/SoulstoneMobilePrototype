using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    private const int MaxActiveSkills = 6;

    private readonly List<SkillInstance> activeSkills = new();
    private readonly List<SkillData> weaponSkillPool = new();
    private readonly List<SkillData> globalSkillPool = new();

    private CharacterSystem owner;
    public event System.Action ActiveSkillsChanged;

    public IReadOnlyList<SkillInstance> ActiveSkills => activeSkills;
    public int MaxSkills => MaxActiveSkills;

    public void Initialize(CharacterSystem characterSystem)
    {
        owner = characterSystem;
        ResetState();
        globalSkillPool.Clear();
        weaponSkillPool.Clear();

        if (owner.CharacterData != null)
        {
            foreach (WeaponData weapon in owner.CharacterData.AvailableWeapons)
            {
                if (weapon == null)
                {
                    continue;
                }

                if (weapon.UniqueSkillPool == null)
                {
                    continue;
                }

                foreach (SkillData skill in weapon.UniqueSkillPool)
                {
                    AddUnique(globalSkillPool, skill);
                }
            }
        }
    }

    public void RegisterWeapon(WeaponData weaponData)
    {
        weaponSkillPool.Clear();

        if (weaponData == null || weaponData.UniqueSkillPool == null)
        {
            return;
        }

        foreach (SkillData skill in weaponData.UniqueSkillPool)
        {
            AddUnique(weaponSkillPool, skill);
        }
    }

    public IEnumerable<SkillData> GetNewSkillOffers()
    {
        IEnumerable<SkillData> pool = weaponSkillPool.Concat(globalSkillPool);
        return pool.Where(skill => skill != null && !HasSkill(skill)).Distinct();
    }

    public IEnumerable<SkillData> GetUpgradeableSkills()
    {
        return activeSkills
            .Where(skill => skill.Data != null && skill.Rank < skill.Data.MaxRank)
            .Select(skill => skill.Data);
    }

    public bool HasSkill(SkillData skillData)
    {
        return activeSkills.Exists(skill => skill.Data == skillData);
    }

    public SkillInstance GetSkillInstance(SkillData skillData)
    {
        return activeSkills.Find(skill => skill.Data == skillData);
    }

    public bool AcquireSkill(SkillData skillData, bool locked = false, SkillData replaceSkill = null)
    {
        if (skillData == null)
        {
            return false;
        }

        SkillInstance existingSkill = GetSkillInstance(skillData);

        if (existingSkill != null)
        {
            existingSkill.Rank = Mathf.Min(existingSkill.Data.MaxRank, existingSkill.Rank + 1);
            existingSkill.Runtime.SetRank(existingSkill.Rank);
            NotifyActiveSkillsChanged();
            return true;
        }

        if (activeSkills.Count >= MaxActiveSkills)
        {
            if (replaceSkill == null)
            {
                return false;
            }

            ReplaceSkill(replaceSkill, skillData);
            return true;
        }

        SkillBehaviourBase runtime = SkillFactory.CreateRuntime(owner, skillData, 1);

        if (runtime == null)
        {
            Debug.LogError($"Skill '{skillData.name}' is missing a valid Runtime Definition.");
            return false;
        }

        activeSkills.Add(new SkillInstance
        {
            Data = skillData,
            Rank = 1,
            Locked = locked,
            Runtime = runtime
        });

        NotifyActiveSkillsChanged();
        return true;
    }

    public SkillData GetReplacementCandidate()
    {
        SkillInstance replaceableSkill = activeSkills
            .Where(skill => !skill.Locked)
            .OrderBy(skill => skill.Rank)
            .FirstOrDefault();

        return replaceableSkill?.Data;
    }

    private void ReplaceSkill(SkillData oldSkill, SkillData newSkill)
    {
        SkillInstance existing = GetSkillInstance(oldSkill);

        if (existing == null)
        {
            return;
        }

        if (existing.Runtime != null)
        {
            existing.Runtime.enabled = false;
            Destroy(existing.Runtime);
        }

        activeSkills.Remove(existing);
        AcquireSkill(newSkill);
    }

    public void ResetState()
    {
        foreach (SkillInstance activeSkill in activeSkills)
        {
            if (activeSkill.Runtime != null)
            {
                activeSkill.Runtime.enabled = false;
                Destroy(activeSkill.Runtime);
            }
        }

        activeSkills.Clear();
        weaponSkillPool.Clear();
        globalSkillPool.Clear();
        NotifyActiveSkillsChanged();
    }

    private static void AddUnique(List<SkillData> list, SkillData skillData)
    {
        if (skillData != null && !list.Contains(skillData))
        {
            list.Add(skillData);
        }
    }

    private void NotifyActiveSkillsChanged()
    {
        ActiveSkillsChanged?.Invoke();
    }
}
