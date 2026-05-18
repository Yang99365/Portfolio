# 끝없는 모험 (Endless Adventure)

> **Text Roguelike RPG** — Unity 6 · Solo Developer · 4-Week Sprint

[![Unity](https://img.shields.io/badge/Unity-6.0-black?logo=unity)](https://unity.com)
[![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20WebGL-blue)](https://yangasta.itch.io/endless-adventure)
[![Language](https://img.shields.io/badge/Language-C%23-purple)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

영어/한국어 지원

## 🎮 플레이 해보기

**[▶ Itch.io에서 웹으로 플레이 (클릭)](https://yangasta.itch.io/endless-adventure)**

---

## 🗺️ 게임 소개

**"직업과 선택이 만드는 나만의 모험 — 매번 다른 빌드, 매번 다른 결말."**

끝없는 모험은 중세 판타지를 배경으로 한 텍스트 로그라이크 RPG입니다.  
플레이어는 평민·용병·상인 중 직업을 선택하고, 스탯 기반 이벤트 선택과 자동 전투를 통해 30분~1시간의 런을 경험합니다.  
런이 끝나면 획득한 재화로 영구 강화를 구매해 다음 회차를 더 유리하게 시작합니다.

### 3가지 핵심 재미

| 기둥 | 설명 |
|------|------|
| **빌드 다양성** | 레벨업마다 랜덤 보너스 3개 중 선택 + 스킬 트리 자유 배분 → 매 런마다 고유한 빌드 |
| **도박적 전투** | 승리 확률 예측 → 선택적 주사위 굴림 → 자동 전투 실행. 확률을 알아도 긴장감 유지 |
| **분기 스토리** | 스탯 기반 선택지 성공률 + 직업별 전용 선택지 + 플레이 패턴에 따른 멀티 엔딩 |

---

## ⚙️ 핵심 시스템

### 런 루프
```
[거점 선택] → [이벤트 발생] → [선택지 판정] → [결과 (보상/피해/전투)]
     ↑                                                     ↓
     └──────────── [레벨업 → 보너스 선택 + 스킬 포인트] ←───┘
```

### 스탯 시스템
- **1차 스탯 (STR / DEX / INT / CHA / CON)**: 이벤트 선택지 성공률 결정
- **2차 스탯 (ATK, DEF, CritRate, DodgeRate 등)**: 1차 스탯에서 자동 파생, 전투에 사용
- **자원 스탯 (HP / 정신력 / 골드)**: HP 또는 정신력 0 → 런 강제 종료

### 전투 시스템
- 전투 전 **승리 확률 표시** (확실 / 유리 / 반반 / 불리 / 불가능)
- 선택적 **주사위 굴림**으로 전투 보너스/페널티 적용
- **자동 전투**: 장비·스킬·스탯에 따른 결과, 치명타/회피 등 운 요소 포함
- **태그 기반 이펙트 시스템**: CounterTag로 효과 간 상호작용을 데이터 주도 방식으로 처리

### 엔딩 시스템
| 종류 | 조건 | 내용 |
|------|------|------|
| 데스 엔딩 | HP 0 | 사망 에필로그 |
| 포기 엔딩 | 정신력 0 | 모험 포기 에필로그 |
| 스토리 엔딩 | 진행도 100% + 플레이 패턴 | 전설의 용사 / 대상인 / 은둔자 / Etc |

---

## 🏗️ 기술 아키텍처

### 설계 원칙
- **단방향 의존성**: 프레임워크(`@Framework/`)는 게임 코드(`@Scripts/`)를 참조하지 않음
- **SOLID 원칙 엄수**: SRP 적용 예시 — EquipmentManager를 `EquipmentManager` + `LootGenerator` + `ShopInventoryGenerator`로 분리
- **EventBus 패턴**: 매니저 간 직접 호출 없이 이벤트로 통신. 20여 개 이벤트 구조체 정의
- **데이터 주도 설계**: 모든 게임 수치는 JSON. 코드에 매직 넘버 없음

### 아키텍처 다이어그램
```
┌──────────────────────────────────────────────┐
│              Managers (Singleton Hub)         │
│  ResourceManager  DataManager  UIManager      │
│  SoundManager     SaveManager  SceneManager   │
│  GameManager      CombatManager EventManager  │
│  LevelUpManager   MetaManager  EquipmentManager│
│              EventBus<T> (static, generic)    │
└──────────────────────────────────────────────┘
         ↑ Game code subscribes/publishes

Stateless Helpers (static):
  StatCalculator · EffectTagResolver · ChoiceEvaluator
  EndingResolver · LootGenerator · ShopInventoryGenerator
```

### 씬 구조
- **씬 2개만 사용** (TitleScene + GameScene)
- 런 진행 중 씬 전환 없음 — 모든 화면 전환은 **UI Popup 스택**으로 처리
- UIManager가 Popup 스택을 관리, UI_Base의 enum+Bind 컨벤션으로 모든 UI 구현

### 주목할 만한 구현

#### 🗺️ World Map + 커스텀 셰이더
- 대륙 지도 위에 마우스 오버 지역을 실시간으로 하이라이트하는 `WorldMapHighlight` 셰이더
- 현재 지역과 호버 지역에 각각 독립적인 마스크 컬러 적용
- DOTween으로 플레이어 마커가 지도 위를 걸어서 이동하는 연출

#### 📖 Journal 시스템
- 런 도중 **엔딩 예언 탭**: 각 엔딩의 달성 조건(태그/플래그)과 현재 달성 수치를 실시간으로 표시
- **보너스 기록 탭**: 이번 런에서 선택한 레벨업 보너스를 순서대로 열람
- EndingResolver와 연동하여 현재 플레이 패턴이 어느 엔딩으로 향하는지 힌트 제공

#### 📄 Book Page Flip 애니메이션
- 셰이더·외부 에셋 없이 `MaskableGraphic` 상속 + CPU 메시 변형으로 3D 페이지 넘김 효과 구현
- 20개 컬럼 분할, 코사인 함수로 X축 압축, 자유 엣지 밝기/높이 변화로 입체감 표현
- 프레임워크 레벨(`@Framework/UI/`)에 구현되어 모든 팝업에 재사용

### 주요 기술 결정

| 항목 | 선택 | 이유 |
|------|------|------|
| 씬 수 | 2개 | 텍스트 RPG는 런 중 씬 전환 불필요 |
| 전투 처리 | 코루틴 기반 틱 | ECS 없이 단순하게 구현 |
| 이펙트 시스템 | 태그 기반 매칭 | 하드코딩 없이 데이터로 효과 상호작용 정의 |
| 저장 시점 | 이벤트·전투 후 즉시 | 모바일 앱 킬 대응 |
| 에셋 로딩 | Addressables | Resources.Load 직접 사용 배제 |
| 직렬화 | Newtonsoft.Json | Dictionary 직렬화 지원 필요 |

### 폴더 구조
```
Assets/
├── @Framework/          # 재사용 가능한 게임 프레임워크 (게임 코드 참조 금지)
│   ├── Managers/        # Resource, Data, Pool, UI, Sound, Scene, Save, Ad
│   └── UI/              # UI_Base, UI_Scene, UI_Popup, UI_SubItem, EventHandler
│
└── @Scripts/            # 게임 전용 코드
    ├── Managers/Contents/  # GameManager, CombatManager, EventManager, LevelUpManager...
    ├── Data/               # Data.Classes.cs, Data.Enemies.cs, Data.Events.cs...
    ├── Systems/            # Stateless helpers (StatCalculator, EffectTagResolver...)
    ├── Runtime/            # RunData, CombatUnit, EffectInstance
    ├── Events/             # 20+ EventBus 이벤트 구조체
    └── UI/                 # Scene / Popup / SubItem / Title
```

---

## 🛠️ 기술 스택

| 항목 | 내용 |
|------|------|
| **엔진** | Unity 6 (URP 2D) |
| **언어** | C# |
| **에셋 관리** | Unity Addressables |
| **직렬화** | Newtonsoft.Json |
| **UI** | UGUI + TextMeshPro |
| **애니메이션** | DOTween |
| **빌드 타겟** | PC · WebGL |

---

## 📅 개발 기간 및 규모

| 항목 | 내용 |
|------|------|
| **개발 기간** | 약 4주 (주말 포함 스프린트) |
| **개발 인원** | 솔로 (기획·프로그래밍·아트 디렉팅 1인) |
| **C# 파일** | 110+ (Framework 24 + Game Scripts 85+) |
| **JSON 데이터** | 12+ 종류 |
| **이벤트 콘텐츠** | 36+ |

---

## 📄 라이선스

사용된 픽셀아트 에셋은 각 에셋 제작자의 라이선스를 따릅니다.

---

<div align="center">

**개발자**: Yang99365 · yps46000@gmail.com

</div>
