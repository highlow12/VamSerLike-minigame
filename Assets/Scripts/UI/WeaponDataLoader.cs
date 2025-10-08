using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class WeaponDataLoader : MonoBehaviour
{
    private List<EquipmentItem> loadedWeapons = new List<EquipmentItem>();

    public List<EquipmentItem> LoadedWeapons => loadedWeapons;

    [System.Serializable]
    public class ItemSpritePair
    {
        public string itemName;
        public Sprite sprite;
    }
    [SerializeField]
    public List<ItemSpritePair> itemSpriteList;
    private Dictionary<string, Sprite> itemSpritesDict = new Dictionary<string, Sprite>();


    private void Awake()
    {
        LoadWeaponData();
        itemSpritesDict.Clear();
        foreach (var pair in itemSpriteList)
        {
            if (!itemSpritesDict.ContainsKey(pair.itemName))
            {
                itemSpritesDict.Add(pair.itemName, pair.sprite);
            }
            else
            {
                Debug.LogWarning($"중복된 itemName: {pair.itemName}이(가) itemSpriteList에 있습니다.");
            }
        }
    }

    public Sprite GetItemSprite(string itemName)
    {
        // 아이템 이름으로 스프라이트 가져오기
        if (itemSpritesDict.TryGetValue(itemName, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"{itemName}에 해당하는 스프라이트가 없습니다.");
            return null;
        }
    }

    public void LoadWeaponData()
    {
        loadedWeapons.Clear();

        // WeaponStatProvider에서 모든 무기 데이터 가져오기
        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("WeaponStatProvider.Instance가 null입니다. WeaponStatProvider가 초기화되었는지 확인하세요.");
            return;
        }

        var allWeaponStats = WeaponStatProvider.Instance.weaponStats;
        if (allWeaponStats == null || allWeaponStats.Count == 0)
        {
            Debug.LogError("WeaponStatProvider에서 무기 데이터를 찾을 수 없습니다.");
            return;
        }

        foreach (var weaponStat in allWeaponStats)
        {
            try
            {
                // WeaponStat을 EquipmentItem으로 변환
                EquipmentItem weapon = new EquipmentItem(
                    weaponStat.displayName,
                    weaponStat.displayWeaponRare,
                    (int)weaponStat.weaponRare, // 열거형을 int로 캐스팅
                    (int)weaponStat.weaponType, // 열거형을 int로 캐스팅
                    $"{weaponStat.displayWeaponRare} 등급의 무기입니다.", // 간단한 설명
                    (int)weaponStat.attackDamage,
                    (int)weaponStat.attackSpeed,
                    weaponStat.attackRange,
                    weaponStat.attackTarget,
                    (int)weaponStat.projectileCount,
                    (int)weaponStat.projectileSpeed
                );

                loadedWeapons.Add(weapon);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"무기 데이터 변환 오류: {e.Message}, 무기: {weaponStat.displayName}");
            }
        }

        Debug.Log($"{loadedWeapons.Count}개의 무기 데이터를 로드했습니다.");
    }

    // 특정 무기 타입의 무기 가져오기
    public EquipmentItem GetWeaponByType(int weaponType)
    {
        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("WeaponStatProvider.Instance가 null입니다.");
            return null;
        }

        // 직접 루프로 찾기
        foreach (var weapon in WeaponStatProvider.Instance.weaponStats)
        {
            if ((int)weapon.weaponType == weaponType)
            {
                return new EquipmentItem(
                    weapon.displayName,
                    weapon.displayWeaponRare,
                    (int)weapon.weaponRare,
                    (int)weapon.weaponType,
                    $"{weapon.displayWeaponRare} 등급의 무기입니다.",
                    (int)weapon.attackDamage,
                    (int)weapon.attackSpeed,
                    weapon.attackRange,
                    weapon.attackTarget,
                    (int)weapon.projectileCount,
                    (int)weapon.projectileSpeed
                );
            }
        }

        Debug.LogWarning($"weaponType {weaponType}에 해당하는 무기를 찾을 수 없습니다.");
        return null;
    }

    // 특정 희귀도의 무기 가져오기
    public EquipmentItem GetWeaponByRare(int weaponRare)
    {
        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("WeaponStatProvider.Instance가 null입니다.");
            return null;
        }

        // 직접 루프로 찾기
        foreach (var weapon in WeaponStatProvider.Instance.weaponStats)
        {
            if ((int)weapon.weaponRare == weaponRare)
            {
                return new EquipmentItem(
                    weapon.displayName,
                    weapon.displayWeaponRare,
                    (int)weapon.weaponRare,
                    (int)weapon.weaponType,
                    $"{weapon.displayWeaponRare} 등급의 무기입니다.",
                    (int)weapon.attackDamage,
                    (int)weapon.attackSpeed,
                    weapon.attackRange,
                    weapon.attackTarget,
                    (int)weapon.projectileCount,
                    (int)weapon.projectileSpeed
                );
            }
        }

        Debug.LogWarning($"weaponRare {weaponRare}에 해당하는 무기를 찾을 수 없습니다.");
        return null;
    }

    // 타입과 희귀도를 함께 사용해 무기 가져오기
    public EquipmentItem GetWeaponByTypeAndRare(int weaponType, int weaponRare)
    {
        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("WeaponStatProvider.Instance가 null입니다.");
            return null;
        }

        // 직접 루프로 찾기
        foreach (var weapon in WeaponStatProvider.Instance.weaponStats)
        {
            if ((int)weapon.weaponType == weaponType && (int)weapon.weaponRare == weaponRare)
            {
                return new EquipmentItem(
                    weapon.displayName,
                    weapon.displayWeaponRare,
                    (int)weapon.weaponRare,
                    (int)weapon.weaponType,
                    $"{weapon.displayWeaponRare} 등급의 무기입니다.",
                    (int)weapon.attackDamage,
                    (int)weapon.attackSpeed,
                    weapon.attackRange,
                    weapon.attackTarget,
                    (int)weapon.projectileCount,
                    (int)weapon.projectileSpeed
                );
            }
        }

        Debug.LogWarning($"weaponType {weaponType}와 weaponRare {weaponRare}에 해당하는 무기를 찾을 수 없습니다.");
        return null;
    }

    // 여러 조건으로 무기 검색하기 (옵션 파라미터 사용)
    public EquipmentItem FindWeapon(int? weaponType = null, int? weaponRare = null, string displayName = null)
    {
        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("WeaponStatProvider.Instance가 null입니다.");
            return null;
        }

        // 직접 루프로 찾기
        foreach (var weapon in WeaponStatProvider.Instance.weaponStats)
        {
            bool match = true;

            if (weaponType.HasValue && (int)weapon.weaponType != weaponType.Value)
                match = false;

            if (weaponRare.HasValue && (int)weapon.weaponRare != weaponRare.Value)
                match = false;

            if (!string.IsNullOrEmpty(displayName) && weapon.displayName != displayName)
                match = false;

            if (match)
            {
                return new EquipmentItem(
                    weapon.displayName,
                    weapon.displayWeaponRare,
                    (int)weapon.weaponRare,
                    (int)weapon.weaponType,
                    $"{weapon.displayWeaponRare} 등급의 무기입니다.",
                    (int)weapon.attackDamage,
                    (int)weapon.attackSpeed,
                    weapon.attackRange,
                    weapon.attackTarget,
                    (int)weapon.projectileCount,
                    (int)weapon.projectileSpeed
                );
            }
        }

        string criteria = "";
        if (weaponType.HasValue) criteria += $"무기 타입: {weaponType.Value}, ";
        if (weaponRare.HasValue) criteria += $"희귀도: {weaponRare.Value}, ";
        if (!string.IsNullOrEmpty(displayName)) criteria += $"이름: {displayName}, ";

        if (!string.IsNullOrEmpty(criteria))
            criteria = criteria.TrimEnd(',', ' ');

        Debug.LogWarning($"조건({criteria})에 해당하는 무기를 찾을 수 없습니다.");
        return null;
    }

    // 여러 조건으로 무기 목록 필터링하기
    public List<EquipmentItem> FilterWeapons(int? weaponType = null, int? weaponRare = null)
    {
        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("WeaponStatProvider.Instance가 null입니다.");
            return new List<EquipmentItem>();
        }

        List<EquipmentItem> result = new List<EquipmentItem>();

        // 직접 루프로 찾기
        foreach (var weapon in WeaponStatProvider.Instance.weaponStats)
        {
            bool match = true;

            if (weaponType.HasValue && (int)weapon.weaponType != weaponType.Value)
                match = false;

            if (weaponRare.HasValue && (int)weapon.weaponRare != weaponRare.Value)
                match = false;

            if (match)
            {
                result.Add(new EquipmentItem(
                    weapon.displayName,
                    weapon.displayWeaponRare,
                    (int)weapon.weaponRare,
                    (int)weapon.weaponType,
                    $"{weapon.displayWeaponRare} 등급의 무기입니다.",
                    (int)weapon.attackDamage,
                    (int)weapon.attackSpeed,
                    weapon.attackRange,
                    weapon.attackTarget,
                    (int)weapon.projectileCount,
                    (int)weapon.projectileSpeed
                ));
            }
        }

        return result;
    }

    // MirrorUI에서 무기 데이터 가져오기//MirrorUI에서 이코드를 활용하여 무기 데이터를 받으로 온다는 뜻인듯
    public List<EquipmentItem> GetWeapons()
    {
        if (loadedWeapons.Count == 0)
        {
            LoadWeaponData();
        }

        return new List<EquipmentItem>(loadedWeapons);
    }

    // 무기 데이터 업데이트하기
    public void UpdateWeaponData(JsonData updatedData)
    {
        if (LocalDataManager.Instance == null)
        {
            Debug.LogError("LocalDataManager.Instance가 null입니다. LocalDataManager가 초기화되었는지 확인하세요.");
            return;
        }

        bool success = LocalDataManager.Instance.UpdateLocalUserMainWeaponData(updatedData);
        if (success)
        {
            Debug.Log("무기 데이터가 성공적으로 업데이트되었습니다.");
            LoadWeaponData(); // 업데이트 후 무기 데이터 다시 로드
        }
        else
        {
            Debug.LogError("무기 데이터를 업데이트하는 데 실패했습니다.");
        }
    }
}