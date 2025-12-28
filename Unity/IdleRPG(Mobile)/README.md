Idle RPG - Unity Mobile Game

자동 전투 기반의 방치형 RPG 게임 - Unity와 C#으로 개발한 모바일 게임 프로젝트
1인 개발 프로젝트

## 주요 기능

### 핵심 시스템
- **자동 전투 시스템**: AI 기반 영웅/몬스터 전투
- **영웅 관리**: 4명의 영웅 고용, 배치, 레벨업
- **스킬큐브 시스템**: 장비처럼 장착/해제 가능한 독특한 스킬 시스템
- **인벤토리 관리**: 장비, 소비 아이템, 재료 분류 관리
- **마스터리 시스템**: 영구 스탯 강화 시스템
- **상점 시스템**: 리롤 가능한 랜덤 아이템 상점
- **스테이지 진행**: 보스 스테이지 포함 무한 진행

###  기술적 특징
- **완전한 세이브/로드 시스템**: Newtonsoft.Json 기반 다형성 지원
- **이벤트 드리븐 아키텍처**: Manager 패턴 + 이벤트 시스템
- **오브젝트 풀링**: 성능 최적화
- **모바일 최적화**: SafeArea 대응, 터치 UI


## 🛠️ 기술 스택

### 개발 환경
- **Engine**: Unity 6000.0.54f1
- **Language**: C# 9.0
- **IDE**: Visual Studio 2022

### 주요 라이브러리
- **Newtonsoft.Json**: 세이브/로드 직렬화
- **Unity Addressables**: 리소스 관리

### 아키텍처 패턴
- **Singleton Manager Pattern**: 게임 시스템 관리
- **Factory Pattern**: 아이템/스킬 생성
- **Observer Pattern**: 이벤트 시스템
- **Object Pooling**: 성능 최적화

## 📂 프로젝트 구조

```
Assets/
├── @Resources/
│   ├── Data/           # JSON 데이터 파일
│   ├── Prefabs/        # UI, 캐릭터 프리팹
│   ├── Animations/     # 캐릭터 애니메이션
│   └── Sprites/        # 이미지 리소스
├── @Scripts/
│   ├── Controllers/    # AI, 오브젝트 처리
│   ├── Managers/       # 게임 시스템 관리
│   │   ├── Core/      # DataManager, UIManager 등
│   │   └── Contents/  # GameManager, BattleManager 등
│   ├── Contents/      # 스킬,아이템 정의
│   ├── Data/          # 데이터 구조 정의
│   ├── UI/            # UI 컴포넌트
│   └── Utils/         # 유틸리티 클래스
└── @Scenes/
    ├── TitleScene     # 타이틀 화면
    └── GameScene      # 메인 게임 화면
```

## 🏗️ 핵심 시스템 설명

### 1. Manager 시스템
**Singleton 패턴**으로 구현된 중앙 집중식 관리 시스템

```csharp
public class Managers : MonoBehaviour
{
    // Core Managers
    public static DataManager Data { get; }
    public static UIManager UI { get; }
    public static ResourceManager Resource { get; }
    
    // Contents Managers
    public static GameManager Game { get; }
    public static BattleManager Battle { get; }
    public static HeroManager Hero { get; }
    public static InventoryManager Inventory { get; }
}
```

**특징**:
- DontDestroyOnLoad로 씬 전환 시에도 유지
- 각 매니저는 독립적인 책임과 명확한 역할 분담

### 2. 전투 시스템 (BattleManager)
**AI 코루틴 기반 자동 전투**

```csharp
- 영웅/몬스터 슬롯 시스템 (4x4)
- 자동 타겟팅 및 공격
- 스킬 자동 발동 (쿨타임 관리)
- 버프/디버프 시스템
```

**주요 로직**:
- 매 프레임 타겟 탐색 → 유효 타겟 발견 → 공격
- 스킬 쿨타임 관리 및 자동 발동
- 타겟 사망 시 자동 타겟 갱신

### 3. 스킬큐브 시스템
**독창적인 시스템**: 스킬을 장비처럼 장착/해제 가능

```csharp
public class SkillCube
{
    public int InstanceId;      // 고유 인스턴스 ID
    public int SkillId;         // 스킬 템플릿 ID
    public int Level;           // 스킬 레벨
    public int EquipSlot;       // 장착 슬롯 (-1: 인벤토리)
}
```

**특징**:
- 영웅당 4개 스킬 슬롯
- 자유로운 스킬 조합 가능
- 같은 스킬도 레벨별로 별도 인스턴스

### 4. 세이브/로드 시스템
**Newtonsoft.Json 기반 다형성 지원**

```csharp
public void SaveGame()
{
    var settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,  // 다형성 지원
        Formatting = Formatting.Indented
    };
    
    string json = JsonConvert.SerializeObject(SaveData, settings);
    File.WriteAllText(SavePath, json);
}
```

**저장 데이터**:
- 영웅 정보 (레벨, 경험치, 장비, 스킬)
- 아이템/스킬큐브 인벤토리
- 마스터리 레벨
- 현재 스테이지, 골드/젬

**기술적 도전**:
- Unity JsonUtility의 다형성 미지원 문제 해결
- EquipmentItem/ConsumableItem 등 상속 구조 직렬화
- Factory 패턴으로 역직렬화 시 올바른 타입 복원

### 5. 이벤트 시스템
**느슨한 결합**을 위한 Observer 패턴

```csharp
// HeroManager
public event Action<Hero> OnHeroCreated;
public event Action<Hero, int> OnHeroDeployed;

// UI에서 구독
Managers.Hero.OnHeroDeployed += OnHeroDeployedHandler;
```

**장점**:
- Manager ↔ UI 직접 참조 제거
- 시스템 간 독립성 유지
- 확장 용이

## 🎯 개발 과정 및 문제 해결

### 주요 기술적 도전

#### 1. JsonUtility의 다형성 미지원
**문제**: Unity의 기본 JsonUtility는 상속 클래스를 제대로 직렬화하지 못함
```csharp
// Item → EquipmentItem/ConsumableItem 역직렬화 실패
var item = JsonUtility.FromJson<Item>(json); // ❌ 항상 Item으로만 복원
```

**해결**: Newtonsoft.Json + Factory 패턴
```csharp
var settings = new JsonSerializerSettings { 
    TypeNameHandling = TypeNameHandling.Auto 
};
var item = Item.CreateItem(itemData); // Factory로 올바른 타입 생성
item.Init(); // 타입별 초기화
```

#### 2. UI 갱신 타이밍 문제
**문제**: 게임 로드 후 영웅탭 UI가 구 데이터로 표시
```
UI 생성 → 데이터 로드 → SetInfo() 호출
→ 하지만 이미 열린 팝업은 갱신 안 됨!
```

**해결**: 강제 갱신 메커니즘
```csharp
public void SetInfo()
{
    Refresh();
    ForceRefreshCurrentPopup(); // 현재 열린 팝업 강제 갱신
}
```

#### 3. 메모리 누수 (이벤트 구독)
**문제**: 영웅 배치/해제 시 이벤트 중복 구독
```csharp
// 매번 RestoreSkills() 호출 시 중복 구독
cube.OnSkillUsed += hero.OnSkillUsedHandler; // 누적됨!
```

**해결**: Dictionary로 구독 추적
```csharp
private Dictionary<SkillCube, bool> _subscribedSkills;

if (!_subscribedSkills.ContainsKey(cube)) {
    cube.OnSkillUsed += OnSkillUsedHandler;
    _subscribedSkills[cube] = true;
}
```

#### 4. 모바일 UI Safe Area
**문제**: 노치가 있는 기기에서 UI 잘림

**해결**: SafeAreaFitter 컴포넌트
```csharp
// 디바이스별 Safe Area 자동 대응
Rect safeArea = Screen.safeArea;
anchorMin = safeArea.position;
anchorMax = safeArea.position + safeArea.size;
```

## 📈 성능 최적화

### 1. Object Pooling
```csharp
// 몬스터, 이펙트, UI 요소 재사용
Managers.Pool.Push(monster); // 풀에 반환
Managers.Pool.Pop(prefab);   // 풀에서 꺼내기
```
### 2. 이벤트 구독 관리
```csharp
// OnDestroy에서 확실히 해제
private void OnDestroy()
{
    Managers.Hero.OnHeroDeployed -= OnHeroDeployedHandler;
    // ... 모든 이벤트 해제
}
```

## 🚀 향후 개발 계획
- [ ] 더 많은 영웅과 스킬
- [ ] 장비 강화 시스템

## 📝 라이선스

이 프로젝트는 포트폴리오 목적으로 제작되었습니다.

## 👤 개발자

**[양찬우]**
- GitHub: https://github.com/Yang99365
- Email: yps46000@gmail.com
- Portfolio: https://github.com/Yang99365/Portfolio

---
