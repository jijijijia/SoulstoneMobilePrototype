using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ContentValidationTools
{
    private const string MenuPath = "Soulstone/Validate Content";
    private const string LegacyPathMarker = "/_Legacy/";

    [MenuItem(MenuPath)]
    public static void ValidateContent()
    {
        StringBuilder report = new();
        int issueCount = 0;

        ValidateIds<CharacterData>(report, ref issueCount, "Character", data => data.CharacterId);
        ValidateIds<WeaponData>(report, ref issueCount, "Weapon", data => data.WeaponId);
        ValidateIds<SkillData>(report, ref issueCount, "Skill", data => data.SkillId);
        ValidateIds<EnemyData>(report, ref issueCount, "Enemy", data => data.EnemyId);
        ValidateIds<EnemyAbilityConfig>(report, ref issueCount, "Enemy ability", data => data.AbilityId);
        ValidateIds<MapData>(report, ref issueCount, "Map", data => data.MapId);
        ValidateIds<UpgradeData>(report, ref issueCount, "Upgrade", data => data.UpgradeId);
        ValidateCharacterRoster(report, ref issueCount);
        ValidateWeapons(report, ref issueCount);
        ValidateSkills(report, ref issueCount);
        ValidateEnemies(report, ref issueCount);
        ValidateEnemyAbilities(report, ref issueCount);
        ValidateMaps(report, ref issueCount);

        if (issueCount == 0)
        {
            Debug.Log("Soulstone content validation passed. No blocking content issues found.");
            return;
        }

        Debug.LogWarning($"Soulstone content validation found {issueCount} issue(s):\n{report}");
    }

    private static void ValidateIds<T>(StringBuilder report, ref int issueCount, string label, System.Func<T, string> idGetter)
        where T : Object
    {
        Dictionary<string, string> pathsById = new();
        foreach (T asset in LoadAssets<T>())
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string id = idGetter(asset);

            if (string.IsNullOrWhiteSpace(id))
            {
                AppendIssue(report, ref issueCount, $"{label} '{asset.name}' has an empty id. Path: {path}");
                continue;
            }

            string normalizedId = id.Trim().ToLowerInvariant();

            if (pathsById.TryGetValue(normalizedId, out string existingPath))
            {
                AppendIssue(report, ref issueCount, $"{label} id '{id}' is duplicated.\nFirst: {existingPath}\nSecond: {path}");
                continue;
            }

            pathsById.Add(normalizedId, path);
        }
    }

    private static void ValidateCharacterRoster(StringBuilder report, ref int issueCount)
    {
        CharacterRoster roster = Resources.Load<CharacterRoster>("CharacterRoster");

        if (roster == null)
        {
            AppendIssue(report, ref issueCount, "Resources/CharacterRoster is missing.");
            return;
        }

        if (roster.Characters == null || roster.Characters.Length == 0)
        {
            AppendIssue(report, ref issueCount, "CharacterRoster has no characters.");
            return;
        }

        HashSet<CharacterData> seen = new();

        for (int i = 0; i < roster.Characters.Length; i++)
        {
            CharacterData character = roster.Characters[i];

            if (character == null)
            {
                AppendIssue(report, ref issueCount, $"CharacterRoster slot {i} is empty.");
                continue;
            }

            if (!seen.Add(character))
            {
                AppendIssue(report, ref issueCount, $"CharacterRoster contains duplicate character '{character.name}'.");
            }

            if (character.StartingWeapon == null)
            {
                AppendIssue(report, ref issueCount, $"Character '{character.name}' has no starting weapon.");
            }

            if (!HasStat(character.BaseStats, StatType.MaxHealth))
            {
                AppendIssue(report, ref issueCount, $"Character '{character.name}' has no MaxHealth stat.");
            }

            if (!HasStat(character.BaseStats, StatType.MoveSpeed))
            {
                AppendIssue(report, ref issueCount, $"Character '{character.name}' has no MoveSpeed stat.");
            }

            if (!HasStat(character.BaseStats, StatType.PickupRadius))
            {
                AppendIssue(report, ref issueCount, $"Character '{character.name}' has no PickupRadius stat.");
            }

            if (character.AvailableWeapons == null || character.AvailableWeapons.Length == 0)
            {
                AppendIssue(report, ref issueCount, $"Character '{character.name}' has no available weapons.");
            }
            else if (character.StartingWeapon != null && System.Array.IndexOf(character.AvailableWeapons, character.StartingWeapon) < 0)
            {
                AppendIssue(report, ref issueCount, $"Character '{character.name}' starting weapon is not present in AvailableWeapons.");
            }
        }
    }

    private static void ValidateWeapons(StringBuilder report, ref int issueCount)
    {
        foreach (WeaponData weapon in LoadAssets<WeaponData>())
        {
            if (weapon.StartingSkill == null)
            {
                AppendIssue(report, ref issueCount, $"Weapon '{weapon.name}' has no starting skill.");
            }

            if (weapon.UniqueSkillPool == null || weapon.UniqueSkillPool.Length == 0)
            {
                AppendIssue(report, ref issueCount, $"Weapon '{weapon.name}' has no unique skill pool.");
            }
            else if (weapon.StartingSkill != null && System.Array.IndexOf(weapon.UniqueSkillPool, weapon.StartingSkill) < 0)
            {
                AppendIssue(report, ref issueCount, $"Weapon '{weapon.name}' starting skill is not present in UniqueSkillPool.");
            }

            ValidateUniqueArray(report, ref issueCount, weapon.UniqueSkillPool, $"Weapon '{weapon.name}' skill pool");
        }
    }

    private static void ValidateSkills(StringBuilder report, ref int issueCount)
    {
        foreach (SkillData skill in LoadAssets<SkillData>())
        {
            bool hasAttackDefinitions = skill.AttackDefinitions != null && skill.AttackDefinitions.Length > 0;

            if (skill.RuntimeDefinition == null && !hasAttackDefinitions)
            {
                AppendIssue(report, ref issueCount, $"Skill '{skill.name}' has no runtime definition or attack definitions.");
            }

            if (!hasAttackDefinitions)
            {
                AppendIssue(report, ref issueCount, $"Skill '{skill.name}' has no attack definitions.");
                continue;
            }

            for (int i = 0; i < skill.AttackDefinitions.Length; i++)
            {
                AttackDefinition attack = skill.AttackDefinitions[i];

                if (attack == null)
                {
                    AppendIssue(report, ref issueCount, $"Skill '{skill.name}' attack slot {i} is empty.");
                    continue;
                }

                if (attack.Targeting == null)
                {
                    AppendIssue(report, ref issueCount, $"Attack '{attack.name}' has no targeting definition.");
                }

                if (attack.Delivery == null)
                {
                    AppendIssue(report, ref issueCount, $"Attack '{attack.name}' has no delivery definition.");
                }

                if (attack.Impacts == null || attack.Impacts.Length == 0)
                {
                    AppendIssue(report, ref issueCount, $"Attack '{attack.name}' has no impacts.");
                }
            }
        }
    }

    private static void ValidateEnemies(StringBuilder report, ref int issueCount)
    {
        foreach (EnemyData enemy in LoadAssets<EnemyData>())
        {
            if (enemy.Prefab == null)
            {
                AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has no prefab.");
            }

            if (enemy.AbilityConfig == null)
            {
                AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has no enemy ability config and will use legacy fallback fields.");
            }

            if (enemy.BaseStats == null || enemy.BaseStats.Length == 0)
            {
                AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has no base stats.");
            }
            else
            {
                if (!HasStat(enemy.BaseStats, StatType.MaxHealth) && !HasStat(enemy.BaseStats, StatType.EnemyHealth))
                {
                    AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has no MaxHealth or EnemyHealth stat.");
                }

                if (!HasStat(enemy.BaseStats, StatType.MoveSpeed))
                {
                    AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has no MoveSpeed stat.");
                }

                if (!HasStat(enemy.BaseStats, StatType.ContactDamage))
                {
                    AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has no ContactDamage stat.");
                }
            }

            if (enemy.ExperienceReward <= 0)
            {
                AppendIssue(report, ref issueCount, $"Enemy '{enemy.name}' has non-positive experience reward.");
            }
        }
    }

    private static void ValidateEnemyAbilities(StringBuilder report, ref int issueCount)
    {
        foreach (EnemyAbilityConfig ability in LoadAssets<EnemyAbilityConfig>())
        {
            if (ability.DamageMultiplier <= 0f)
            {
                AppendIssue(report, ref issueCount, $"Enemy ability '{ability.name}' has non-positive damage multiplier.");
            }

            if (ability.Cooldown <= 0.1f)
            {
                AppendIssue(report, ref issueCount, $"Enemy ability '{ability.name}' cooldown is suspiciously low.");
            }

            if (ability.PreferredDistance <= 0.5f && ability.DeliveryType != EnemyAbilityDeliveryType.MeleeContact)
            {
                AppendIssue(report, ref issueCount, $"Enemy ability '{ability.name}' is ranged but has melee-like preferred distance.");
            }

            if (ability.DeliveryType == EnemyAbilityDeliveryType.Projectile && ability.ProjectileLifetime <= 0.1f)
            {
                AppendIssue(report, ref issueCount, $"Enemy ability '{ability.name}' projectile lifetime is too low.");
            }

            if (ability.DeliveryType == EnemyAbilityDeliveryType.AreaPulse && ability.AreaRadius <= 0.25f)
            {
                AppendIssue(report, ref issueCount, $"Enemy ability '{ability.name}' area radius is too low.");
            }
        }
    }

    private static void ValidateMaps(StringBuilder report, ref int issueCount)
    {
        MapDatabase database = Resources.Load<MapDatabase>("MapDatabase");

        if (database == null)
        {
            AppendIssue(report, ref issueCount, "Resources/MapDatabase is missing.");
            return;
        }

        ValidateUniqueArray(report, ref issueCount, database.Maps, "MapDatabase maps");

        foreach (MapData map in LoadAssets<MapData>())
        {
            if (map.SpawnTimeline == null)
            {
                AppendIssue(report, ref issueCount, $"Map '{map.name}' has no spawn timeline.");
            }

            if (map.MapPrefab == null)
            {
                AppendIssue(report, ref issueCount, $"Map '{map.name}' has no map prefab.");
            }

            if (map.EnemyPool == null || map.EnemyPool.Length == 0)
            {
                AppendIssue(report, ref issueCount, $"Map '{map.name}' has no enemy pool.");
            }
            else
            {
                ValidateUniqueArray(report, ref issueCount, map.EnemyPool, $"Map '{map.name}' enemy pool");
            }

            if (map.PlayerSpawnClearRadius < 0.5f)
            {
                AppendIssue(report, ref issueCount, $"Map '{map.name}' player spawn clear radius is too small.");
            }

            if (map.OverrideMapBounds)
            {
                if (map.MapMin.x >= map.MapMax.x || map.MapMin.y >= map.MapMax.y)
                {
                    AppendIssue(report, ref issueCount, $"Map '{map.name}' has invalid map bounds.");
                }

                Vector2 spawn = new(map.PlayerSpawnPosition.x, map.PlayerSpawnPosition.z);

                if (spawn.x < map.MapMin.x || spawn.x > map.MapMax.x || spawn.y < map.MapMin.y || spawn.y > map.MapMax.y)
                {
                    AppendIssue(report, ref issueCount, $"Map '{map.name}' player spawn is outside map bounds.");
                }
            }
        }
    }

    private static bool HasStat(StatValue[] values, StatType statType)
    {
        if (values == null)
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].statType == statType)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateUniqueArray<T>(StringBuilder report, ref int issueCount, T[] values, string label)
        where T : Object
    {
        if (values == null)
        {
            return;
        }

        HashSet<T> seen = new();

        for (int i = 0; i < values.Length; i++)
        {
            T value = values[i];

            if (value == null)
            {
                AppendIssue(report, ref issueCount, $"{label} slot {i} is empty.");
                continue;
            }

            if (!seen.Add(value))
            {
                AppendIssue(report, ref issueCount, $"{label} contains duplicate asset '{value.name}'.");
            }
        }
    }

    private static IEnumerable<T> LoadAssets<T>()
        where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            if (string.IsNullOrWhiteSpace(path) || path.Replace('\\', '/').Contains(LegacyPathMarker))
            {
                continue;
            }

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
            {
                yield return asset;
            }
        }
    }

    private static void AppendIssue(StringBuilder report, ref int issueCount, string message)
    {
        issueCount++;
        report.Append(issueCount).Append(". ").AppendLine(message);
    }
}
