using UnityEngine;

public sealed class MainMenuHeroPreviewBuilder
{
    private const string PreviewSpawnPointName = "PreviewSpawnPoint";
    private const string RuntimeHeroName = "RuntimeHeroPrefab";

    public void Refresh(Transform heroRoot, CharacterData selectedCharacter, Color accentColor)
    {
        if (heroRoot == null)
        {
            return;
        }

        Transform previewSpawnPoint = ResolvePreviewSpawnPoint(heroRoot);
        ClearRuntimePreview(previewSpawnPoint);

        if (TryCreateSelectedCharacterPrefab(previewSpawnPoint, selectedCharacter))
        {
            SetSceneFallbackVisible(previewSpawnPoint, false);
            return;
        }

        SetSceneFallbackVisible(previewSpawnPoint, true);
        RefreshFallbackAccent(previewSpawnPoint, selectedCharacter, accentColor);
    }

    private static Transform ResolvePreviewSpawnPoint(Transform heroRoot)
    {
        Transform spawnPoint = heroRoot.Find(PreviewSpawnPointName);

        if (spawnPoint != null)
        {
            return spawnPoint;
        }

        Debug.LogWarning($"Main menu hero preview is missing '{PreviewSpawnPointName}'. Using '{heroRoot.name}' as a fallback slot, but manual scene objects may be affected.", heroRoot);
        return heroRoot;
    }

    private static bool TryCreateSelectedCharacterPrefab(Transform previewSpawnPoint, CharacterData selectedCharacter)
    {
        if (selectedCharacter == null || selectedCharacter.ModelPrefab == null)
        {
            return false;
        }

        GameObject model = Object.Instantiate(selectedCharacter.ModelPrefab, previewSpawnPoint);
        model.name = RuntimeHeroName;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        if (HasVisibleRenderer(model))
        {
            WeaponData selectedWeapon = SelectedLoadoutStore.ResolveSelectedWeapon(selectedCharacter);
            CharacterWeaponVisualAttacher weaponAttacher = model.GetComponent<CharacterWeaponVisualAttacher>();

            if (weaponAttacher == null)
            {
                weaponAttacher = model.AddComponent<CharacterWeaponVisualAttacher>();
            }

            weaponAttacher.AttachWeapon(selectedWeapon);
            return true;
        }

        DestroyObject(model);
        return false;
    }

    private static void ClearRuntimePreview(Transform previewSpawnPoint)
    {
        for (int i = previewSpawnPoint.childCount - 1; i >= 0; i--)
        {
            Transform child = previewSpawnPoint.GetChild(i);

            if (child != null && child.name == RuntimeHeroName)
            {
                DestroyObject(child.gameObject);
            }
        }
    }

    private static void SetSceneFallbackVisible(Transform previewSpawnPoint, bool visible)
    {
        bool foundFallback = false;

        for (int i = 0; i < previewSpawnPoint.childCount; i++)
        {
            Transform child = previewSpawnPoint.GetChild(i);

            if (child == null || child.name == RuntimeHeroName)
            {
                continue;
            }

            child.gameObject.SetActive(visible);
            foundFallback = true;
        }

        if (visible && !foundFallback)
        {
            Debug.LogWarning($"'{previewSpawnPoint.name}' has no scene-authored fallback hero children. Add a placeholder capsule/model there for characters without a prefab.", previewSpawnPoint);
        }
    }

    private static bool HasVisibleRenderer(GameObject root)
    {
        if (root == null)
        {
            return false;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private static Color GetHeroColor(CharacterData character)
    {
        if (character == null || string.IsNullOrWhiteSpace(character.CharacterId))
        {
            return new Color(0.32f, 0.18f, 0.09f, 1f);
        }

        return character.CharacterId switch
        {
            "viking" => new Color(0.38f, 0.22f, 0.12f, 1f),
            "knight" => new Color(0.42f, 0.42f, 0.46f, 1f),
            "pyromancer" => new Color(0.55f, 0.12f, 0.05f, 1f),
            "ranger" => new Color(0.17f, 0.36f, 0.16f, 1f),
            "necromancer" => new Color(0.18f, 0.12f, 0.27f, 1f),
            "paladin" => new Color(0.62f, 0.52f, 0.28f, 1f),
            "rune_smith" => new Color(0.29f, 0.29f, 0.32f, 1f),
            _ => new Color(0.25f, 0.16f, 0.1f, 1f)
        };
    }

    private static void RefreshFallbackAccent(Transform previewSpawnPoint, CharacterData selectedCharacter, Color accentColor)
    {
        Color robeColor = GetHeroColor(selectedCharacter);
        ApplyRendererColor(previewSpawnPoint.Find("Body_Robe"), robeColor);
        ApplyRendererColor(previewSpawnPoint.Find("Belt"), accentColor);
        ApplyRendererColor(previewSpawnPoint.Find("Shoulder_Left"), accentColor);
        ApplyRendererColor(previewSpawnPoint.Find("Shoulder_Right"), accentColor);
        ApplyRendererColor(previewSpawnPoint.Find("BackWeapon_Blade"), accentColor);
    }

    private static void ApplyRendererColor(Transform target, Color color)
    {
        if (target == null)
        {
            return;
        }

        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer != null)
        {
            MaterialPropertyBlock properties = new();
            renderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
        }
    }

    private static void DestroyObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
            return;
        }

        Object.DestroyImmediate(target);
    }
}
