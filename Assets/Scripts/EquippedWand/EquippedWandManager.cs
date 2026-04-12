using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ステージ持ち込み用の杖選択状態を管理するクラス。
/// 3スロット固定・重複装備不可で、PlayerPrefs に永続化します。
/// </summary>
public class EquippedWandManager : MonoBehaviour
{
    private const string PLAYERPREFS_KEY_WANDS = "EquippedWands_Types";
    private const int SLOT_COUNT = 3;

    private static EquippedWandManager _instance;
    public static EquippedWandManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(nameof(EquippedWandManager));
                _instance = singletonObject.AddComponent<EquippedWandManager>();
                DontDestroyOnLoad(singletonObject);
                _instance.Initialize();
            }
            return _instance;
        }
    }

    private Wand[] _equippedWands = new Wand[SLOT_COUNT];
    private WandDataAsset _wandDataAsset;
    private IEquippedWandsObserver _observer;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (_wandDataAsset == null)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        _wandDataAsset = Resources.Load<WandDataAsset>("WandDataAsset");
        if (_wandDataAsset == null)
        {
            Debug.LogError("Resources/WandDataAsset が見つかりません。");
        }

        LoadEquippedWands();
    }

    public void RegisterObserver(IEquippedWandsObserver observer)
    {
        _observer = observer;
        NotifyEquippedWandsChanged();
    }

    public IReadOnlyList<Wand> GetEquippedWands()
    {
        return new List<Wand>(_equippedWands);
    }

    public IReadOnlyList<Wand> GetAllWands()
    {
        if (_wandDataAsset == null || _wandDataAsset.wands == null)
        {
            return new List<Wand>();
        }

        return _wandDataAsset.wands
            .Where(e => e != null && e.wand != null)
            .Select(e => e.wand)
            .ToList();
    }

    public bool IsWandUnlocked(Wand wand)
    {
        if (wand == null || WandUnlockManager.Instance == null)
        {
            return false;
        }

        return WandUnlockManager.Instance.IsWandUnlocked(wand.type);
    }

    public bool IsWandNewlyUnlocked(Wand wand)
    {
        if (wand == null || WandUnlockManager.Instance == null)
        {
            return false;
        }

        return WandUnlockManager.Instance.IsWandNewlyUnlocked(wand.type);
    }

    public void ClearWandNewBadge(Wand wand)
    {
        if (wand != null && WandUnlockManager.Instance != null)
        {
            WandUnlockManager.Instance.ClearWandNewBadge(wand.type);
        }
    }

    public bool IsWandEquipped(Wand wand)
    {
        if (wand == null)
        {
            return false;
        }

        return _equippedWands.Any(w => w != null && w.type == wand.type);
    }

    public int FindEquippedSlotIndex(Wand wand)
    {
        if (wand == null)
        {
            return -1;
        }

        for (int i = 0; i < _equippedWands.Length; i++)
        {
            if (_equippedWands[i] != null && _equippedWands[i].type == wand.type)
            {
                return i;
            }
        }

        return -1;
    }

    public bool SetWand(int slotIndex, Wand wand)
    {
        if (slotIndex < 0 || slotIndex >= SLOT_COUNT)
        {
            Debug.LogError($"無効なスロット index: {slotIndex}");
            return false;
        }

        if (wand == null)
        {
            Debug.LogError("SetWand に null は設定できません。解除は RemoveWand を使用してください。");
            return false;
        }

        if (!IsWandUnlocked(wand))
        {
            Debug.LogWarning($"未開放の杖は選択できません: {wand.type}");
            return false;
        }

        // 同じ杖は1本しか持ち込めないため、既存スロットから外す
        int existingIndex = FindEquippedSlotIndex(wand);
        if (existingIndex >= 0)
        {
            _equippedWands[existingIndex] = null;
        }

        _equippedWands[slotIndex] = wand;

        SaveEquippedWands();
        NotifyEquippedWandsChanged();
        return true;
    }

    public void RemoveWand(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SLOT_COUNT)
        {
            Debug.LogError($"無効なスロット index: {slotIndex}");
            return;
        }

        _equippedWands[slotIndex] = null;
        SaveEquippedWands();
        NotifyEquippedWandsChanged();
    }

    /// <summary>
    /// 空きスロットがあれば、指定された杖を自動で装備します。
    /// </summary>
    public void AutoEquipIfSpaceAvailable(WandType wandType)
    {
        if (_wandDataAsset == null) return;

        Wand wand = _wandDataAsset.GetWand(wandType);
        if (wand == null) return;

        if (IsWandEquipped(wand)) return;

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (_equippedWands[i] == null)
            {
                SetWand(i, wand);
                break;
            }
        }
    }

    /// <summary>
    /// ステージ開始時に持ち込む杖を返します。
    /// 未選択時は従来どおり開放済み杖をフォールバックとして返します。
    /// </summary>
    public Wand[] GetWandsForStage()
    {
        var selected = _equippedWands.Where(w => w != null).ToArray();
        if (selected.Length > 0)
        {
            return selected;
        }

        return WandUnlockManager.Instance != null
            ? WandUnlockManager.Instance.GetUnlockedWands()
            : new Wand[0];
    }

    private void SaveEquippedWands()
    {
        var typeStrings = _equippedWands
            .Select(wand => wand == null ? string.Empty : wand.type.ToString())
            .ToArray();

        PlayerPrefs.SetString(PLAYERPREFS_KEY_WANDS, string.Join(",", typeStrings));
        PlayerPrefs.Save();
    }

    private void LoadEquippedWands()
    {
        _equippedWands = new Wand[SLOT_COUNT];

        if (_wandDataAsset == null)
        {
            return;
        }

        if (!PlayerPrefs.HasKey(PLAYERPREFS_KEY_WANDS))
        {
            ApplyDefaultSelection();
            return;
        }

        string raw = PlayerPrefs.GetString(PLAYERPREFS_KEY_WANDS, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            ApplyDefaultSelection();
            return;
        }

        string[] typeStrings = raw.Split(',');
        for (int i = 0; i < SLOT_COUNT && i < typeStrings.Length; i++)
        {
            if (string.IsNullOrEmpty(typeStrings[i]))
            {
                continue;
            }

            if (!System.Enum.TryParse(typeStrings[i], out WandType wandType))
            {
                continue;
            }

            Wand wand = _wandDataAsset.GetWand(wandType);
            if (wand == null)
            {
                continue;
            }

            // ロックされた杖は選択状態から除外
            if (WandUnlockManager.Instance != null && !WandUnlockManager.Instance.IsWandUnlocked(wand.type))
            {
                continue;
            }

            _equippedWands[i] = wand;
        }
    }

    private void ApplyDefaultSelection()
    {
        if (_wandDataAsset == null)
        {
            return;
        }

        var defaultEntry = _wandDataAsset.wands
            .FirstOrDefault(e => e != null && e.wand != null && e.type == WandType.Default);

        if (defaultEntry != null)
        {
            _equippedWands[0] = defaultEntry.wand;
        }

        SaveEquippedWands();
    }

    private void NotifyEquippedWandsChanged()
    {
        _observer?.OnEquippedWandsChanged(GetEquippedWands());
    }
}

public interface IEquippedWandsObserver
{
    void OnEquippedWandsChanged(IReadOnlyList<Wand> equippedWands);
}
