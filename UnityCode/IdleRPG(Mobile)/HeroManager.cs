using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class HeroManager
{
    #region Hero Management
    // 생성된 모든 영웅 관리 (인스턴스 ID로 관리)
    private Dictionary<int, Hero> _heroes = new Dictionary<int, Hero>();

    // 배치된 영웅 관리 (슬롯 인덱스로 관리)
    private Dictionary<int, Hero> _deployedHeroes = new Dictionary<int, Hero>();

    // 다음 영웅 인스턴스 ID
    private int _nextHeroInstanceId = 1000;

    // 현재 마스터리 보너스 (퍼센트)
    private float _currentMasteryAttackPercent = 0f;
    private float _currentMasteryDefensePercent = 0f;
    private float _currentMasteryMaxHpPercent = 0f;
    private float _currentMasteryAttackSpeedPercent = 0f;
    private float _currentMasteryCritChancePercent = 0f;
    private float _currentMasteryCritDamagePercent = 0f;

    // 이벤트
    public event Action<Hero> OnHeroCreated;
    public event Action<Hero> OnHeroRemoved;
    public event Action<Hero, int> OnHeroDeployed; // Hero, SlotIndex
    public event Action<Hero, int> OnHeroUndeployed; // Hero, SlotIndex
    public event Action<Hero, Item> OnHeroEquippedItem;
    public event Action<Hero, Item> OnHeroUnequippedItem;
    public event Action<Hero, SkillCube, int> OnHeroEquippedSkill;
    public event Action<Hero, SkillCube, int> OnHeroUnequippedSkill;
    public event Action<int> OnHeroStatsChanged; // TamplateId
    #endregion

    #region Initialization
    public void Init()
    {
        LoadHeroesFromSaveData();
        LoadMasteryFromSaveData();
        Debug.Log($"HeroManager initialized with {_heroes.Count} heroes");
    }

    private void LoadHeroesFromSaveData()
    {
        var saveData = Managers.Game.SaveData;
        if (saveData?.Heroes == null) return;

        // 영웅 정보만 로드 (실제 GameObject는 배치 시 생성)
        foreach (var heroSave in saveData.Heroes)
        {
            if (heroSave.isUnlocked)
            {
                Debug.Log($"Hero {heroSave.templateId} is unlocked (Level: {heroSave.level})");
            }
        }
    }

    private void LoadMasteryFromSaveData()
    {
        // SaveData에서 마스터리 레벨 로드 후 HeroManager에 적용
        var saveData = Managers.Game.SaveData;
        if (saveData == null)
            return;

        // MasteryManager를 통해 현재 마스터리 보너스 계산
        if (Managers.Mastery != null)
        {
            float attackBonus = Managers.Mastery.GetMasteryBonus(1);
            float defenseBonus = Managers.Mastery.GetMasteryBonus(2);
            float maxHpBonus = Managers.Mastery.GetMasteryBonus(3);
            float attackSpeedBonus = Managers.Mastery.GetMasteryBonus(4);
            float critChanceBonus = Managers.Mastery.GetMasteryBonus(5);
            float critDamageBonus = Managers.Mastery.GetMasteryBonus(6);

            // HeroManager의 마스터리 값 초기화
            _currentMasteryAttackPercent = attackBonus;
            _currentMasteryDefensePercent = defenseBonus;
            _currentMasteryMaxHpPercent = maxHpBonus;
            _currentMasteryAttackSpeedPercent = attackSpeedBonus;
            _currentMasteryCritChancePercent = critChanceBonus;
            _currentMasteryCritDamagePercent = critDamageBonus;

            Debug.Log($"Loaded mastery bonuses - Attack: +{attackBonus}%, Defense: +{defenseBonus}%, MaxHP: +{maxHpBonus}%");
        }
    }
    #endregion

    #region Hero Creation & Management
    // 새 영웅 생성 (최초 획득 시)
    public Hero CreateNewHero(int templateId)
    {
        if (!Managers.Data.HeroDataDict.TryGetValue(templateId, out var heroData))
        {
            Debug.LogError($"Hero template {templateId} not found");
            return null;
        }

        // 이미 보유한 영웅인지 확인
        var existingSave = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (existingSave != null)
        {
            Debug.LogWarning($"Hero {heroData.characterName} already owned");
            return null;
        }

        // 세이브 데이터 생성
        var saveData = new HeroSaveData
        {
            templateId = templateId,
            level = 1,
            exp = 0,
            slotIndex = -1,
            isUnlocked = true,
            weaponId = 0,
            armorId = 0,
            accessoryId = 0,
            skills = new List<SkillSaveData>()
        };

        // GameManager의 세이브 데이터에 추가
        Managers.Game.SaveData.Heroes.Add(saveData);

        // 기본 스킬큐브 장착
        EquipDefaultSkillCubes(templateId);

        Debug.Log($"New hero {heroData.characterName} created");

        OnHeroCreated?.Invoke(null); // 실제 Hero 객체는 배치 시 생성

        return null; // 실제 Hero는 배치 시 생성
    }

    // 기본 스킬큐브 장착 (새 영웅 생성 시)
    private void EquipDefaultSkillCubes(int templateId)
    {
        var heroData = Managers.Data.HeroDataDict.GetValueOrDefault(templateId);
        if (heroData == null) return;

        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (saveData == null) return;

        foreach (var skillId in heroData.skillIds)
        {
            // 기본 스킬큐브 생성
            var skillCube = new SkillCube(skillId, 1);
            skillCube.InstanceId = Managers.Game.GenerateSkillInstanceId();

            // 세이브 데이터에 추가
            saveData.skills.Add(new SkillSaveData
            {
                instanceId = skillCube.InstanceId,
                skillId = skillCube.DataId,
                level = skillCube.Level,
                equipSlot = saveData.skills.Count // 순서대로 장착
            });
        }

        Debug.Log($"Equipped default skills to hero {heroData.characterName}");
    }

    // 영웅 인스턴스 ID 생성
    private int GenerateHeroInstanceId()
    {
        return _nextHeroInstanceId++;
    }

    // 영웅 가져오기
    public Hero GetHero(int instanceId)
    {
        _heroes.TryGetValue(instanceId, out var hero);
        return hero;
    }

    // 템플릿 ID로 영웅 찾기
    public Hero GetHeroByTemplateId(int templateId)
    {
        return _heroes.Values.FirstOrDefault(h => h.DataTemplateID == templateId);
    }

    // 모든 영웅 가져오기
    public List<Hero> GetAllHeroes()
    {
        return _heroes.Values.ToList();
    }

    // 배치된 영웅 가져오기
    public List<Hero> GetDeployedHeroes()
    {
        return _deployedHeroes
        .OrderBy(kvp => kvp.Key)
        .Select(kvp => kvp.Value)
        .ToList();
    }
    #endregion

    #region Hero Deployment (for BattleManager)
    // 영웅 배치 (BattleManager에서 호출)
    public Hero DeployHero(int templateId, int slotIndex, Transform slotTransform)
    {
        // 세이브 데이터 찾기
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (saveData == null)
        {
            Debug.LogError($"Hero save data not found for template {templateId}");
            return null;
        }

        // 이미 배치된 영웅인지 확인
        if (_deployedHeroes.Values.Any(h => h.DataTemplateID == templateId))
        {
            Debug.LogWarning($"Hero {templateId} is already deployed");
            return null;
        }

        // Hero GameObject 생성
        Hero hero = Managers.Object.Spawn<Hero>(slotTransform.position, templateId);
        if (hero == null)
        {
            Debug.LogError($"Failed to spawn hero {templateId}");
            return null;
        }

        // 인스턴스 ID 생성
        int instanceId = GenerateHeroInstanceId();

        // Hero 정보 설정 (saveData를 null로 전달하여 기본 스킬 장착되도록)
        // 단, 레벨과 경험치는 따로 설정
        hero.SetHeroInfo(templateId, instanceId, saveData); // null 전달이 중요!


        hero.SlotIndex = slotIndex;
        hero.transform.SetParent(slotTransform);
        hero.transform.localPosition = Vector3.zero;

        // 관리 딕셔너리에 추가
        _heroes[instanceId] = hero;
        _deployedHeroes[slotIndex] = hero;

        //Hero의 OnLevelUp 이벤트 구독 추가
        hero.OnLevelUp += OnHeroLevelUpHandler;

        // 세이브 데이터 업데이트
        saveData.slotIndex = slotIndex;

        // 현재 마스터리 적용
        ApplyMasteryToHero(hero);

        // 장비 복원
        RestoreEquipment(hero, saveData);

        // 스킬 복원 (저장된 스킬이 있다면)
        //if (saveData.skills != null && saveData.skills.Count > 0)
        //{
        //    RestoreSkills(hero, saveData);
        //}
        RestoreSkills(hero, saveData);
        // 없으면 기본 스킬은 이미 SetHeroInfo에서 장착됨

        // 이벤트 발생
        OnHeroDeployed?.Invoke(hero, slotIndex);

        Debug.Log($"Hero {hero.HeroData.characterName} deployed to slot {slotIndex}");
        return hero;
    }

    // 영웅 배치 해제 (BattleManager에서 호출)
    public void UndeployHero(int slotIndex)
    {
        if (!_deployedHeroes.TryGetValue(slotIndex, out var hero))
            return;

        if (hero != null)
        {
            hero.OnLevelUp -= OnHeroLevelUpHandler;
        }
        // 세이브 데이터 업데이트..(장비나스킬큐브처럼 스테이지 클리어할때마다 하도록 만드는게..?)
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == hero.DataTemplateID);
        if (saveData != null)
        {
            saveData.slotIndex = -1;
            saveData.level = hero.Level;
            saveData.exp = hero.Experience;
        }

        // 관리 딕셔너리에서 제거
        _deployedHeroes.Remove(slotIndex);
        _heroes.Remove(hero.HeroInstanceId);

        // 이벤트 발생
        OnHeroUndeployed?.Invoke(hero, slotIndex);

        // GameObject 제거
        Managers.Object.Despawn(hero);

        Debug.Log($"Hero undeployed from slot {slotIndex}");
    }

    #endregion

    #region Mastery Management
    // 마스터리 업데이트 (마스터리 상점에서 호출)
    public void UpdateMastery(float attackPercent, float defensePercent, float maxHpPercent,
                              float attackSpeedPercent, float critChancePercent, float critDamagePercent)
    {
        // 현재 마스터리 값 저장
        _currentMasteryAttackPercent = attackPercent;
        _currentMasteryDefensePercent = defensePercent;
        _currentMasteryMaxHpPercent = maxHpPercent;
        _currentMasteryAttackSpeedPercent = attackSpeedPercent;
        _currentMasteryCritChancePercent = critChancePercent;
        _currentMasteryCritDamagePercent = critDamagePercent;

        // 모든 배치된 영웅에게 적용
        ApplyMasteryToAllHeroes();

        foreach (var hero in _deployedHeroes.Values)
        {
            if (hero != null)
            {
                OnHeroStatsChanged?.Invoke(hero.DataTemplateID);
            }
        }
    }

    // 모든 영웅에게 마스터리 적용
    private void ApplyMasteryToAllHeroes()
    {
        foreach (var hero in _deployedHeroes.Values)
        {
            if (hero != null)
            {
                ApplyMasteryToHero(hero);
            }
        }

        Debug.Log($"Applied mastery bonuses to {_deployedHeroes.Count} deployed heroes");
    }

    // 개별 영웅에게 마스터리 적용
    private void ApplyMasteryToHero(Hero hero)
    {
        if (hero == null) return;

        hero.ApplyMasteryBonus(
            _currentMasteryAttackPercent,
            _currentMasteryDefensePercent,
            _currentMasteryMaxHpPercent,
            _currentMasteryAttackSpeedPercent,
            _currentMasteryCritChancePercent,
            _currentMasteryCritDamagePercent
        );
    }
    #endregion

    #region Equipment Management
    // 장비 장착
    public bool EquipItemToHero(int heroInstanceId, int itemInstanceId)
    {
        var hero = GetHero(heroInstanceId);
        if (hero == null)
        {
            Debug.LogError($"Hero {heroInstanceId} not found");
            return false;
        }

        var item = Managers.Inventory.GetItem(itemInstanceId);
        if (item == null)
        {
            Debug.LogError($"Item {itemInstanceId} not found");
            return false;
        }

        // 장비 데이터 확인
        var equipmentData = item.ItemData as Data.EquipmentData;
        if (equipmentData == null)
        {
            Debug.LogError($"Item {item.ItemData.baseName} is not equipment");
            return false;
        }

        // 클래스 제한 체크
        if (equipmentData.classRestriction > 0 &&
            equipmentData.classRestriction != (int)hero.HeroData.characterClass)
        {
            Debug.Log($"Class restriction: {hero.HeroData.characterName} cannot equip {equipmentData.baseName}");
            return false;
        }

        // 1. 기존 장비 확인 및 해제
        var existingEquipment = hero.GetEquippedItem(equipmentData.equipmentType);
        if (existingEquipment != null)
        {
            // 기존 장비를 인벤토리로 반환
            UnequipItemFromHero(heroInstanceId, equipmentData.equipmentType);
        }

        // 2. 새 장비 장착
        bool equipped = hero.EquipItem(equipmentData);
        if (equipped)
        {
            // 3. 인벤토리에서 제거하고 영웅에게 귀속
            item.EquipSlot = heroInstanceId; // 영웅의 인스턴스 ID를 저장
            Managers.Inventory.RemoveItem(itemInstanceId);

            // 4. 세이브 데이터 업데이트
            UpdateHeroEquipmentSaveData(hero);

            // 5. 이벤트 발생
            OnHeroEquippedItem?.Invoke(hero, item);

            NotifyHeroStatsChanged(hero.DataTemplateID);

            Debug.Log($"Hero {hero.HeroData.characterName} equipped {equipmentData.baseName}");
        }

        return equipped;
    }

    // 장비 해제
    public bool UnequipItemFromHero(int heroInstanceId, EEquipmentType equipmentType)
    {
        var hero = GetHero(heroInstanceId);
        if (hero == null)
        {
            Debug.LogError($"Hero {heroInstanceId} not found");
            return false;
        }

        // 1. 장비 해제
        var unequippedData = hero.UnequipItem(equipmentType);
        if (unequippedData == null)
        {
            Debug.Log($"No equipment in slot {equipmentType}");
            return false;
        }

        // 2. 인벤토리가 가득 찬지 확인
        if (Managers.Inventory.IsItemInventoryFull)
        {
            Debug.LogWarning("Inventory is full! Cannot unequip item.");
            // 장비를 다시 장착
            hero.EquipItem(unequippedData);
            return false;
        }

        // 3. 인벤토리에 아이템 추가
        var item = Managers.Inventory.MakeItem(unequippedData.baseId, 1);
        if (item != null)
        {
            // 4. 세이브 데이터 업데이트
            UpdateHeroEquipmentSaveData(hero);

            // 5. 이벤트 발생
            OnHeroUnequippedItem?.Invoke(hero, item);

            NotifyHeroStatsChanged(hero.DataTemplateID);

            Debug.Log($"Hero {hero.HeroData.characterName} unequipped {unequippedData.baseName}");
            return true;
        }

        // 실패 시 장비 복원
        hero.EquipItem(unequippedData);
        return false;
    }

    // 장비 세이브 데이터 업데이트
    private void UpdateHeroEquipmentSaveData(Hero hero)
    {
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == hero.DataTemplateID);
        if (saveData != null)
        {
            saveData.weaponId = hero.GetEquippedItem(EEquipmentType.Weapon)?.baseId ?? 0;
            saveData.armorId = hero.GetEquippedItem(EEquipmentType.Armor)?.baseId ?? 0;
            saveData.accessoryId = hero.GetEquippedItem(EEquipmentType.Accessory)?.baseId ?? 0;
        }
    }

    // 장비 복원 (영웅 배치 시)
    private void RestoreEquipment(Hero hero, HeroSaveData saveData)
    {
        // 무기 복원
        if (saveData.weaponId > 0)
        {
            RestoreEquipmentItem(hero, saveData.weaponId, EEquipmentType.Weapon);
        }

        // 방어구 복원
        if (saveData.armorId > 0)
        {
            RestoreEquipmentItem(hero, saveData.armorId, EEquipmentType.Armor);
        }

        // 액세서리 복원
        if (saveData.accessoryId > 0)
        {
            RestoreEquipmentItem(hero, saveData.accessoryId, EEquipmentType.Accessory);
        }
    }

    //private void RestoreEquipmentItem(Hero hero, int templateId, EEquipmentType type)
    //{
    //    // 인벤토리에서 해당 아이템 찾기
    //    var items = Managers.Inventory.GetFilteredItems();
    //    var item = items.FirstOrDefault(i =>
    //        i.DataId == templateId &&
    //        i.EquipSlot == hero.HeroInstanceId); // 이 영웅에게 귀속된 아이템

    //    if (item == null)
    //    {
    //        // 인벤토리에 없으면 데이터만으로 복원
    //        if (Managers.Data.EquipmentDic.TryGetValue(templateId, out var equipData))
    //        {
    //            hero.EquipItem(equipData);
    //            Debug.Log($"Restored equipment {equipData.baseName} from data");
    //        }
    //    }
    //    else
    //    {
    //        // 인벤토리에 있으면 실제 아이템으로 복원
    //        var equipData = item.ItemData as Data.EquipmentData;
    //        if (equipData != null)
    //        {
    //            hero.EquipItem(equipData);
    //            Debug.Log($"Restored equipment {equipData.baseName} from inventory");
    //        }
    //    }
    //}
    private void RestoreEquipmentItem(Hero hero, int templateId, EEquipmentType type)
    {
        if (templateId <= 0)
        {
            Debug.Log($"[RestoreEquipment] No {type} to restore for {hero.HeroData.characterName}");
            return;
        }

        // 데이터 테이블에서 장비 정보 찾기
        if (Managers.Data.EquipmentDic.TryGetValue(templateId, out var equipData))
        {
            bool equipped = hero.EquipItem(equipData);

            if (equipped)
            {
                Debug.Log($"[RestoreEquipment] {hero.HeroData.characterName} equipped {equipData.baseName}");
            }
            else
            {
                Debug.LogWarning($"[RestoreEquipment] Failed to equip {equipData.baseName} to {hero.HeroData.characterName}");
            }
        }
        else
        {
            Debug.LogError($"[RestoreEquipment] Equipment data not found: {templateId}");
        }
    }
    #endregion

    #region Skill Management
    // 스킬 장착
    public bool EquipSkillToHero(int heroInstanceId, int skillCubeInstanceId, int slotIndex)
    {
        var hero = GetHero(heroInstanceId);
        if (hero == null)
        {
            Debug.LogError($"Hero {heroInstanceId} not found");
            return false;
        }

        var skillCube = Managers.Inventory.GetSkillCube(skillCubeInstanceId);
        if (skillCube == null)
        {
            Debug.LogError($"SkillCube {skillCubeInstanceId} not found");
            return false;
        }

        // 영웅에게 장착 가능한지 확인
        if (!skillCube.CanEquipToHero(hero))
        {
            Debug.Log($"SkillCube {skillCube.GetName()} cannot be equipped to {hero.HeroData.characterName}");
            return false;
        }

        // 1. 기존 스킬 확인 및 해제
        var existingSkill = hero.GetSkillCubeAtSlot(slotIndex);
        if (existingSkill != null)
        {
            // 기존 스킬을 인벤토리로 반환
            UnequipSkillFromHero(heroInstanceId, slotIndex);
        }

        // 2. 새 스킬 장착
        bool equipped = hero.EquipSkillCube(skillCube, slotIndex);
        if (equipped)
        {
            // 3. 인벤토리에서 제거하고 영웅에게 귀속
            skillCube.EquipSlot = slotIndex;
            skillCube.EquippedHeroId = heroInstanceId;
            Managers.Inventory.RemoveSkillCube(skillCubeInstanceId);

            // 4. 세이브 데이터 업데이트
            UpdateHeroSkillSaveData(hero);

            // 5. 이벤트 발생
            OnHeroEquippedSkill?.Invoke(hero, skillCube, slotIndex);

            //패시브 스킬이면 스탯 변경 알림 추가
            if (skillCube.SkillData.skillType == ESkillType.Passive)
            {
                OnHeroStatsChanged?.Invoke(hero.DataTemplateID);
                Debug.Log($"Passive skill equipped, stats recalculated for {hero.HeroData.characterName}");
            }

            Debug.Log($"Hero {hero.HeroData.characterName} equipped skill {skillCube.GetName()} to slot {slotIndex}");
        }

        return equipped;
    }

    // 스킬 해제
    public bool UnequipSkillFromHero(int heroInstanceId, int slotIndex)
    {
        var hero = GetHero(heroInstanceId);
        if (hero == null)
        {
            Debug.LogError($"Hero {heroInstanceId} not found");
            return false;
        }

        // 1. 스킬 해제
        var unequippedSkill = hero.UnequipSkillCube(slotIndex);
        if (unequippedSkill == null)
        {
            Debug.Log($"No skill in slot {slotIndex}");
            return false;
        }

        // 기본 제공 스킬인지 확인 (인스턴스 ID가 특별한 형식)
        bool isDefaultSkill = unequippedSkill.InstanceId >= 10000000; // 기본 스킬은 큰 ID

        if (!isDefaultSkill)
        {
            // 2. 인벤토리가 가득 찬지 확인
            if (Managers.Inventory.IsSkillInventoryFull)
            {
                Debug.LogWarning("Skill inventory is full! Cannot unequip skill.");
                // 스킬을 다시 장착
                hero.EquipSkillCube(unequippedSkill, slotIndex);
                return false;
            }

            // 3. 인벤토리에 스킬큐브 추가
            unequippedSkill.EquipSlot = -1; // 인벤토리
            unequippedSkill.EquippedHeroId = -1;

            var skillSaveData = new SkillSaveData
            {
                instanceId = unequippedSkill.InstanceId,
                skillId = unequippedSkill.DataId,
                level = unequippedSkill.Level,
                equipSlot = -1
            };

            var addedCube = Managers.Inventory.AddSkillCube(skillSaveData, unequippedSkill.Level);
            if (addedCube != null)
            {
                // 4. 세이브 데이터 업데이트
                UpdateHeroSkillSaveData(hero);

                // 5. 이벤트 발생
                OnHeroUnequippedSkill?.Invoke(hero, unequippedSkill, slotIndex);

                if (unequippedSkill.SkillData.skillType == ESkillType.Passive)
                {
                    OnHeroStatsChanged?.Invoke(hero.DataTemplateID);
                    Debug.Log($"Passive skill unequipped, stats recalculated for {hero.HeroData.characterName}");
                }

                Debug.Log($"Hero {hero.HeroData.characterName} unequipped skill {unequippedSkill.GetName()}");
                return true;
            }
            else
            {
                // 실패 시 스킬 복원
                hero.EquipSkillCube(unequippedSkill, slotIndex);
                return false;
            }
        }
        else
        {

            // 기본 제공 스킬은 그냥 제거
            Debug.Log($"Default skill {unequippedSkill.GetName()} removed");
            UpdateHeroSkillSaveData(hero);
            if (unequippedSkill.SkillData.skillType == ESkillType.Passive)
            {
                OnHeroStatsChanged?.Invoke(hero.DataTemplateID);
            }
            return true;
        }
    }

    // 스킬 세이브 데이터 업데이트
    private void UpdateHeroSkillSaveData(Hero hero)
    {
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == hero.DataTemplateID);
        if (saveData != null)
        {
            saveData.skills.Clear();

            var equippedSkills = hero.GetEquippedSkillCubes();
            foreach (var kvp in equippedSkills)
            {
                if (kvp.Value != null && kvp.Value.InstanceId < 10000000) // 기본 스킬 제외
                {
                    saveData.skills.Add(new SkillSaveData
                    {
                        instanceId = kvp.Value.InstanceId,
                        skillId = kvp.Value.DataId,
                        level = kvp.Value.Level,
                        equipSlot = kvp.Key
                    });
                }
            }
        }
    }

    // 스킬 복원 (영웅 배치 시)
    //private void RestoreSkills(Hero hero, HeroSaveData saveData)
    //{
    //    if (saveData.skills == null || saveData.skills.Count == 0)
    //    {
    //        // 저장된 스킬이 없으면 기본 스킬은 Hero가 알아서 장착함
    //        return;
    //    }

    //    // 먼저 기본 스킬 모두 해제
    //    for (int i = 0; i < Hero.MAX_SKILL_SLOTS; i++)
    //    {
    //        var existingSkill = hero.GetSkillCubeAtSlot(i);
    //        if (existingSkill != null && existingSkill.InstanceId >= 10000000)
    //        {
    //            hero.UnequipSkillCube(i);
    //        }
    //    }

    //    // 저장된 스킬 복원
    //    foreach (var skillSave in saveData.skills)
    //    {
    //        // 인벤토리에서 스킬큐브 찾기
    //        var skillCube = Managers.Inventory.GetSkillCube(skillSave.instanceId);

    //        if (skillCube == null)
    //        {
    //            // 인벤토리에 없으면 새로 생성 (복원)
    //            skillCube = new SkillCube(skillSave.skillId, skillSave.level);
    //            skillCube.InstanceId = skillSave.instanceId;
    //        }

    //        // 영웅에게 장착
    //        if (skillCube != null && skillSave.equipSlot >= 0)
    //        {
    //            hero.EquipSkillCube(skillCube, skillSave.equipSlot);
    //            Debug.Log($"Restored skill {skillCube.GetName()} to slot {skillSave.equipSlot}");
    //        }
    //    }
    //}
    private void RestoreSkills(Hero hero, HeroSaveData saveData)
    {
        // 저장된 스킬이 없으면 기본 스킬 장착
        if (saveData.skills == null || saveData.skills.Count == 0)
        {
            Debug.Log($"[RestoreSkills] {hero.HeroData.characterName} has no saved skills, equipping default skills");
            EquipDefaultSkillsToHero(hero);
            return;
        }

        // 기존 스킬 모두 해제 (기본 스킬 제거)
        ClearAllSkills(hero);

        // 저장된 스킬 복원
        int restoredCount = 0;
        foreach (var skillSave in saveData.skills)
        {
            if (RestoreSkillCube(hero, skillSave))
            {
                restoredCount++;
            }
        }

        Debug.Log($"[RestoreSkills] {hero.HeroData.characterName} restored {restoredCount}/{saveData.skills.Count} skills");
    }
    private void ClearAllSkills(Hero hero)
    {
        for (int i = 0; i < Hero.MAX_SKILL_SLOTS; i++)
        {
            var existingSkill = hero.GetSkillCubeAtSlot(i);
            if (existingSkill != null)
            {
                hero.UnequipSkillCube(i);
            }
        }
    }
    private bool RestoreSkillCube(Hero hero, SkillSaveData skillSave)
    {
        if (skillSave.equipSlot < 0 || skillSave.equipSlot >= Hero.MAX_SKILL_SLOTS)
        {
            Debug.LogWarning($"[RestoreSkills] Invalid slot index: {skillSave.equipSlot}");
            return false;
        }

        // 스킬 데이터 확인
        if (!Managers.Data.SkillDataDict.TryGetValue(skillSave.skillId, out var skillData))
        {
            Debug.LogError($"[RestoreSkills] Skill data not found: {skillSave.skillId}");
            return false;
        }

        // 스킬큐브 생성 (데이터로부터 직접 생성)
        var skillCube = new SkillCube(skillSave.skillId, skillSave.level);
        skillCube.InstanceId = skillSave.instanceId;

        // 영웅에게 장착
        bool equipped = hero.EquipSkillCube(skillCube, skillSave.equipSlot);

        if (equipped)
        {
            skillCube.EquipSlot = skillSave.equipSlot;
            skillCube.EquippedHeroId = hero.HeroInstanceId;

            Debug.Log($"[RestoreSkills] {hero.HeroData.characterName} equipped {skillData.skillName} (Lv.{skillSave.level}) at slot {skillSave.equipSlot}");
            return true;
        }
        else
        {
            Debug.LogWarning($"[RestoreSkills] Failed to equip {skillData.skillName} to {hero.HeroData.characterName}");
            return false;
        }
    }
    private void EquipDefaultSkillsToHero(Hero hero)
    {
        if (hero.HeroData?.skillIds == null || hero.HeroData.skillIds.Count == 0)
        {
            Debug.Log($"[RestoreSkills] {hero.HeroData.characterName} has no default skills");
            return;
        }

        int slotIndex = 0;
        int equippedCount = 0;

        foreach (int skillId in hero.HeroData.skillIds)
        {
            if (slotIndex >= Hero.MAX_SKILL_SLOTS)
                break;

            if (!Managers.Data.SkillDataDict.TryGetValue(skillId, out var skillData))
            {
                Debug.LogWarning($"[RestoreSkills] Skill data not found: {skillId}");
                continue;
            }

            // 기본 스킬큐브 생성 (임시 ID)
            var defaultCube = new SkillCube(skillId, 1);
            defaultCube.InstanceId = skillId * 10000 + hero.HeroInstanceId; // 임시 고유 ID

            // 영웅에게 장착
            bool equipped = hero.EquipSkillCube(defaultCube, slotIndex);

            if (equipped)
            {
                defaultCube.EquipSlot = slotIndex;
                defaultCube.EquippedHeroId = hero.HeroInstanceId;

                Debug.Log($"[RestoreSkills] {hero.HeroData.characterName} equipped default skill: {skillData.skillName} at slot {slotIndex}");
                equippedCount++;
                slotIndex++;
            }
            else
            {
                Debug.LogWarning($"[RestoreSkills] Failed to equip default skill {skillData.skillName}");
            }
        }

        Debug.Log($"[RestoreSkills] {hero.HeroData.characterName} equipped {equippedCount} default skills");
    }
    #endregion
    #region Level System for Leveling Item
    public void GainExperienceToHero(int templateId, int exp)
    {
        // 1. 세이브 데이터 찾기
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (saveData == null || !saveData.isUnlocked)
        {
            Debug.LogError($"Hero {templateId} not found or locked");
            return;
        }

        // 2. 배치된 영웅인지 확인
        Hero deployedHero = GetHeroByTemplateId(templateId);

        if (deployedHero != null)
        {
            // 배치된 영웅: Hero 인스턴스 사용
            deployedHero.GainExperience(exp);
            // OnLevelUp 이벤트가 자동으로 발생 → OnHeroLevelUpHandler 호출
        }
        else
        {
            // 배치 안 된 영웅: 세이브 데이터 직접 수정
            GainExperienceToSaveData(saveData, exp, templateId);
        }
    }

    private void GainExperienceToSaveData(HeroSaveData saveData, int exp, int templateId)
    {
        saveData.exp += exp;

        // 레벨업 체크
        int experienceToNextLevel = saveData.level * 100; // Hero.ExperienceToNextLevel과 동일

        bool leveledUp = false;
        while (saveData.exp >= experienceToNextLevel)
        {
            saveData.exp -= experienceToNextLevel;
            saveData.level++;
            leveledUp = true;

            experienceToNextLevel = saveData.level * 100;

            Debug.Log($"Hero {templateId} leveled up to {saveData.level} (undeployed)");
        }

        // 레벨업했으면 스탯 변경 이벤트 발생
        if (leveledUp)
        {
            OnHeroStatsChanged?.Invoke(templateId);
        }
    }

    public void SetHeroLevel(int templateId, int targetLevel)
    {
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (saveData == null || !saveData.isUnlocked)
        {
            Debug.LogError($"Hero {templateId} not found or locked");
            return;
        }

        if (targetLevel <= saveData.level)
        {
            Debug.LogWarning($"Target level {targetLevel} is not higher than current level {saveData.level}");
            return;
        }

        // 배치된 영웅인지 확인
        Hero deployedHero = GetHeroByTemplateId(templateId);

        if (deployedHero != null)
        {
            // 배치된 영웅: Hero 인스턴스의 레벨 직접 설정
            int levelDiff = targetLevel - deployedHero.Level;
            for (int i = 0; i < levelDiff; i++)
            {
                deployedHero.GainExperience(deployedHero.ExperienceToNextLevel);
            }
            // OnLevelUp 이벤트가 자동으로 발생
        }
        else
        {
            // 배치 안 된 영웅: 세이브 데이터 직접 수정
            saveData.level = targetLevel;
            saveData.exp = 0; // 경험치 초기화

            OnHeroStatsChanged?.Invoke(templateId);

            Debug.Log($"Hero {templateId} level set to {targetLevel} (undeployed)");
        }
    }
    // 경험치 바 조회용 메서드
    public (int currentExp, int expToNextLevel, int level) GetHeroExperience(int templateId)
    {
        // 배치된 영웅 확인
        Hero deployedHero = GetHeroByTemplateId(templateId);
        if (deployedHero != null)
        {
            return (deployedHero.Experience, deployedHero.ExperienceToNextLevel, deployedHero.Level);
        }

        // 배치 안 된 영웅: 세이브 데이터에서
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (saveData != null && saveData.isUnlocked)
        {
            int expToNextLevel = saveData.level * 100; // Hero.ExperienceToNextLevel과 동일
            return (saveData.exp, expToNextLevel, saveData.level);
        }

        return (0, 100, 1); // 기본값
    }

    #endregion

    #region Hero Info (for UI)
    // UI에서 사용할 영웅 정보 구조체
    public struct HeroDisplayInfo
    {
        public int InstanceId;
        public int TemplateId;
        public string Name;
        public int Level;
        public int Experience;
        public bool IsDeployed;
        public int SlotIndex;
        public EHeroClass Class;
        public bool IsUnlocked;
    }
    public struct HeroStats
    {
        public float Attack;
        public float Defense;
        public float MaxHp;
        public float CurrentHp;
        public float AttackSpeed;
        public float CriticalChance;
        public float CriticalDamage;
        public int Level;
        public int Experience;
    }

    // 모든 영웅 정보 가져오기 (UI용)
    public List<HeroDisplayInfo> GetAllHeroDisplayInfo()
    {
        var displayInfos = new List<HeroDisplayInfo>();

        // 세이브 데이터에서 모든 영웅 정보 가져오기
        foreach (var saveData in Managers.Game.SaveData.Heroes)
        {
            if (!saveData.isUnlocked) continue;

            var heroData = Managers.Data.HeroDataDict.GetValueOrDefault(saveData.templateId);
            if (heroData == null) continue;

            // 배치된 영웅이면 실제 인스턴스 찾기
            Hero deployedHero = null;
            if (saveData.slotIndex >= 0)
            {
                _deployedHeroes.TryGetValue(saveData.slotIndex, out deployedHero);
            }

            displayInfos.Add(new HeroDisplayInfo
            {
                InstanceId = deployedHero?.HeroInstanceId ?? 0,
                TemplateId = saveData.templateId,
                Name = heroData.characterName,
                Level = deployedHero?.Level ?? saveData.level,
                Experience = deployedHero?.Experience ?? saveData.exp,
                IsDeployed = saveData.slotIndex >= 0,
                SlotIndex = saveData.slotIndex,
                Class = heroData.characterClass,
                IsUnlocked = saveData.isUnlocked
            });
        }

        return displayInfos;
    }

    // 특정 영웅의 상세 정보 가져오기
    public HeroDisplayInfo? GetHeroDisplayInfo(int templateId)
    {
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        if (saveData == null) return null;

        var heroData = Managers.Data.HeroDataDict.GetValueOrDefault(templateId);
        if (heroData == null) return null;

        Hero deployedHero = null;
        if (saveData.slotIndex >= 0)
        {
            _deployedHeroes.TryGetValue(saveData.slotIndex, out deployedHero);
        }

        return new HeroDisplayInfo
        {
            InstanceId = deployedHero?.HeroInstanceId ?? 0,
            TemplateId = saveData.templateId,
            Name = heroData.characterName,
            Level = deployedHero?.Level ?? saveData.level,
            Experience = deployedHero?.Experience ?? saveData.exp,
            IsDeployed = saveData.slotIndex >= 0,
            SlotIndex = saveData.slotIndex,
            Class = heroData.characterClass,
            IsUnlocked = saveData.isUnlocked
        };
    }
    public HeroStats? GetHeroStats(int templateId)
    {
        // 1. 기본 검증
        if (!ValidateHeroData(templateId, out var heroData, out var saveData))
        {
            return null;
        }

        // 2. 배치된 영웅의 스탯 가져오기
        if (TryGetDeployedHeroStats(saveData, out var deployedStats))
        {
            return deployedStats;
        }

        // 3. 배치되지 않은 영웅의 스탯 계산
        return CalculateStatsFromSaveData(heroData, saveData);
    }
    public Hero GetHeroAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 4)
        {
            Debug.LogWarning($"Invalid slot index: {slotIndex}");
            return null;
        }

        _deployedHeroes.TryGetValue(slotIndex, out var hero);
        return hero;
    }
    public Hero[] GetHeroSlots()
    {
        const int MAX_SLOTS = 4;
        var heroSlots = new Hero[MAX_SLOTS];

        for (int i = 0; i < MAX_SLOTS; i++)
        {
            _deployedHeroes.TryGetValue(i, out heroSlots[i]);
        }

        return heroSlots;
    }
    #endregion
    private bool ValidateHeroData(int templateId, out Data.HeroData heroData, out HeroSaveData saveData)
    {
        saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == templateId);
        heroData = null;

        if (saveData == null || !saveData.isUnlocked)
        {
            return false;
        }

        heroData = Managers.Data.HeroDataDict.GetValueOrDefault(templateId);
        return heroData != null;
    }

    private bool TryGetDeployedHeroStats(HeroSaveData saveData, out HeroStats? stats)
    {
        stats = null;

        if (saveData.slotIndex < 0)
        {
            return false;
        }

        if (_deployedHeroes.TryGetValue(saveData.slotIndex, out var deployedHero))
        {
            stats = CreateStatsFromHero(deployedHero);
            return true;
        }

        return false;
    }

    private HeroStats CreateStatsFromHero(Hero hero)
    {
        return new HeroStats
        {
            Attack = hero.Attack,
            Defense = hero.Defense,
            MaxHp = hero.MaxHp,
            CurrentHp = hero.Hp,
            AttackSpeed = hero.AttackSpeed,
            CriticalChance = hero.CriticalChance,
            CriticalDamage = hero.CriticalDamage,
            Level = hero.Level,
            Experience = hero.Experience
        };
    }
    private HeroStats CalculateStatsFromSaveData(Data.HeroData heroData, HeroSaveData saveData)
    {
        // 1. 기본 스탯
        float baseAttack = heroData.stats.attack;
        float baseDefense = heroData.stats.defense;
        float baseMaxHp = heroData.stats.maxHealth;
        float baseAttackSpeed = heroData.stats.attackSpeed;
        float baseCritChance = heroData.stats.criticalChance;
        float baseCritDamage = heroData.stats.criticalDamage;

        // 2. 레벨 보너스 (Hero.cs의 CalculateLevelBonus와 동일)
        int level = saveData.level;
        float levelBonusAttack = (level - 1) * 2f;  // 레벨당 +2 공격력
        float levelBonusDefense = (level - 1) * 1f; // 레벨당 +1 방어력
        float levelBonusMaxHp = (level - 1) * 10f;  // 레벨당 +10 HP

        // 3. 장비 보너스
        float equipmentBonusAttack = 0f;
        float equipmentBonusDefense = 0f;
        float equipmentBonusMaxHp = 0f;
        float equipmentBonusAttackSpeed = 0f;
        float equipmentBonusCritChance = 0f;
        float equipmentBonusCritDamage = 0f;

        // 무기
        if (saveData.weaponId > 0)
        {
            var weaponData = Managers.Data.EquipmentDic.GetValueOrDefault(saveData.weaponId);
            if (weaponData != null)
            {
                equipmentBonusAttack += weaponData.stats.attack;
                equipmentBonusDefense += weaponData.stats.defense;
                equipmentBonusMaxHp += weaponData.stats.maxHealth;
                equipmentBonusAttackSpeed += weaponData.stats.attackSpeed;
                equipmentBonusCritChance += weaponData.stats.criticalChance;
                equipmentBonusCritDamage += weaponData.stats.criticalDamage;
            }
        }

        // 방어구
        if (saveData.armorId > 0)
        {
            var armorData = Managers.Data.EquipmentDic.GetValueOrDefault(saveData.armorId);
            if (armorData != null)
            {
                equipmentBonusAttack += armorData.stats.attack;
                equipmentBonusDefense += armorData.stats.defense;
                equipmentBonusMaxHp += armorData.stats.maxHealth;
                equipmentBonusAttackSpeed += armorData.stats.attackSpeed;
                equipmentBonusCritChance += armorData.stats.criticalChance;
                equipmentBonusCritDamage += armorData.stats.criticalDamage;
            }
        }

        // 액세서리
        if (saveData.accessoryId > 0)
        {
            var accessoryData = Managers.Data.EquipmentDic.GetValueOrDefault(saveData.accessoryId);
            if (accessoryData != null)
            {
                equipmentBonusAttack += accessoryData.stats.attack;
                equipmentBonusDefense += accessoryData.stats.defense;
                equipmentBonusMaxHp += accessoryData.stats.maxHealth;
                equipmentBonusAttackSpeed += accessoryData.stats.attackSpeed;
                equipmentBonusCritChance += accessoryData.stats.criticalChance;
                equipmentBonusCritDamage += accessoryData.stats.criticalDamage;
            }
        }

        // 4. 마스터리 보너스 적용
        float masteryAttackBonus = (baseAttack + levelBonusAttack + equipmentBonusAttack) *
                                   (_currentMasteryAttackPercent / 100f);
        float masteryDefenseBonus = (baseDefense + levelBonusDefense + equipmentBonusDefense) *
                                    (_currentMasteryDefensePercent / 100f);
        float masteryMaxHpBonus = (baseMaxHp + levelBonusMaxHp + equipmentBonusMaxHp) *
                                  (_currentMasteryMaxHpPercent / 100f);
        float masteryAttackSpeedBonus = (baseAttackSpeed + equipmentBonusAttackSpeed) *
                                        (_currentMasteryAttackSpeedPercent / 100f);
        float masteryCritChanceBonus = (baseCritChance + equipmentBonusCritChance) *
                                       (_currentMasteryCritChancePercent / 100f);
        float masteryCritDamageBonus = (baseCritDamage + equipmentBonusCritDamage) *
                                       (_currentMasteryCritDamagePercent / 100f);

        // 5. 최종 스탯 계산
        float finalAttack = baseAttack + levelBonusAttack + equipmentBonusAttack + masteryAttackBonus;
        float finalDefense = baseDefense + levelBonusDefense + equipmentBonusDefense + masteryDefenseBonus;
        float finalMaxHp = baseMaxHp + levelBonusMaxHp + equipmentBonusMaxHp + masteryMaxHpBonus;
        float finalAttackSpeed = baseAttackSpeed + equipmentBonusAttackSpeed + masteryAttackSpeedBonus;
        float finalCritChance = baseCritChance + equipmentBonusCritChance + masteryCritChanceBonus;
        float finalCritDamage = baseCritDamage + equipmentBonusCritDamage + masteryCritDamageBonus;

        return new HeroStats
        {
            Attack = finalAttack,
            Defense = finalDefense,
            MaxHp = finalMaxHp,
            CurrentHp = finalMaxHp, // 배치되지 않은 영웅은 전체 HP
            AttackSpeed = finalAttackSpeed,
            CriticalChance = finalCritChance,
            CriticalDamage = finalCritDamage,
            Level = level,
            Experience = saveData.exp
        };
    }
    private void NotifyHeroStatsChanged(int templateId)
    {
        OnHeroStatsChanged?.Invoke(templateId);
    }
    public void NotifyStatsChanged(int templateId)
    {
        OnHeroStatsChanged?.Invoke(templateId);
    }
    private void OnHeroLevelUpHandler(Hero hero, int newLevel)
    {
        if (hero == null) return;

        // 스탯 변경 이벤트 발생
        OnHeroStatsChanged?.Invoke(hero.DataTemplateID);

        // 세이브 데이터 업데이트
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == hero.DataTemplateID);
        if (saveData != null)
        {
            saveData.level = hero.Level;
            saveData.exp = hero.Experience;
        }

        Debug.Log($"Hero {hero.HeroData.characterName} leveled up to {newLevel}");
    }
    #region Cleanup
    public void Clear()
    {
        // 모든 배치된 영웅 제거
        var deployedSlots = _deployedHeroes.Keys.ToList();
        foreach (var slot in deployedSlots)
        {
            UndeployHero(slot);
        }

        _heroes.Clear();
        _deployedHeroes.Clear();
    }
    #endregion
}
