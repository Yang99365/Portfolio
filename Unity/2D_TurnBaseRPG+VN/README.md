# Dungeon of Eternity

**Unity 6 기반 턴제 던전 크롤러 RPG — 1인 개발 포트폴리오 프로젝트**

![Unity](https://img.shields.io/badge/Unity_6-URP_2D-blue)
![C#](https://img.shields.io/badge/C%23-10-purple)
![Solo](https://img.shields.io/badge/1인_개발-약_4주-green)
![Scripts](https://img.shields.io/badge/스크립트-136개+-orange)

---

## 게임 개요

Dungeon of Eternity는 전략적 파티 전투, Slay the Spire 스타일 던전 탐색, 비주얼 노벨 시스템을 결합한 2D 턴제 던전 크롤러 RPG입니다. 타이틀 → 캠프 → 던전 탐색 → 전투 → 결과까지 전체 게임 루프가 동작합니다.

> 이 프로젝트는 **시스템 프로그래밍, 아키텍처 설계, 데이터 기반 콘텐츠 파이프라인** 포트폴리오 프로젝트입니다.

---

## 기술 하이라이트

### 전투 FSM + Command 패턴

전투 시스템은 제네릭 `StateMachine<CombatState>`로 **6개 상태**(Setup, TurnStart, PlayerSelect, EnemyTurn, Execute, End)를 관리합니다. 공격, 스킬, 방어, 아이템 사용 등 모든 전투 행동은 `ICombatCommand`(Execute/Undo)로 구현되어 행동 히스토리 추적과 UniTask 기반 비동기 실행을 지원합니다.

**설계 이유:** 액션 로직과 턴 흐름을 분리하여, 새로운 행동 타입 추가 시 커맨드 클래스 하나만 작성하면 됩니다. 상태 머신은 수정할 필요가 없습니다.

### 이벤트 기반 아키텍처 (EventBus)

제네릭 static `EventBus<T>`가 타입 기반 O(1) 이벤트 디스패치를 제공합니다. 전투, 던전, VN, UI 등 모든 주요 시스템은 이벤트로만 통신하며, 매니저 간 직접 참조가 없습니다.

**설계 이유:** 시스템 간 완전한 디커플링. 전투 시스템은 UI의 존재를 모릅니다 — `DamageDealtEvent`를 발행하면 UI가 독립적으로 구독합니다.

### 데이터 기반 콘텐츠 파이프라인 (JSON → ScriptableObject)

에디터 전용 16단계 임포트 파이프라인이 JSON 파일을 Unity ScriptableObject로 변환합니다. 메뉴 한 번 클릭(`DoE > Content > Import All`)으로 전체 파이프라인이 실행되며, GUID 보존 Upsert 패턴으로 재임포트 시에도 참조가 깨지지 않습니다.

- `ContentImporterBase` — Upsert 프레임워크 (SerializedObject 기반 필드 쓰기)
- `AssetRefResolver` — 크로스 임포터 참조 해결 + 세션 캐시
- `ContentImporterMenu` — 1클릭 파이프라인 실행

**설계 이유:** JSON을 Single Source of Truth로 사용하여 배치 콘텐츠 작성이 가능합니다. 세션 캐시 패턴은 `AssetDatabase.StartAssetEditing()` 배치 내에서 아직 인덱싱되지 않은 에셋 간 참조를 해결합니다.

### AI Strategy 패턴

적 AI는 교체 가능한 `AIStrategyData` ScriptableObject를 사용합니다. 4가지 기본 전략(Aggressive/Defensive/Support/Random)과 HP 임계값 기반 페이즈 전환이 가능한 `BossAIStrategyData`가 있습니다. 각 전략은 후보 행동을 점수로 평가한 뒤 최적 행동을 `ICombatCommand`로 변환합니다.

**설계 이유:** 새로운 적 행동 패턴은 SO 에셋 생성만으로 추가 가능합니다. 보스 페이즈도 데이터 기반이라 HP 임계값에 따라 전략이 자동 교체됩니다.

### 던전 탐색 (Slay the Spire 스타일)

시드 기반 결정적 맵 생성기가 분기형 노드 경로를 만듭니다. `DungeonManager`는 자체 내부 FSM(8개 상태)을 운영하며, Strategy 패턴의 7가지 `IRoomEventHandler`(Combat, Elite, Boss, Rest, Treasure, Shop, Event)를 통해 방 이벤트를 처리합니다.

**설계 이유:** 새 방 타입 추가 시 핸들러 클래스 하나와 enum 값 하나만 필요합니다. 던전 FSM과 전투 FSM은 독립적이며, 이벤트를 통해 깔끔하게 조합됩니다.

### VN 대화 + 호감도 시스템

노드 그래프 기반 `DialogueData` SO가 조건 분기(스토리 플래그, 호감도 등급 게이트)와 인라인 액션 실행(아이템 지급, 호감도 변경, 플래그 설정)을 지원합니다. 현재 이벤트 노드에서 간단한 테스트만 작동 가능합니다.

### MVP UI 아키텍처

모든 UI 화면이 **Model-View-Presenter** 패턴을 따릅니다. View는 순수 UGUI MonoBehaviour(표시만 담당), Model은 데이터, Presenter는 로직과 EventBus 구독을 처리합니다. 전투, 인벤토리, 던전 맵, VN 대화, 캠프, 결과 화면 전체에 일관되게 적용했습니다.

### 세이브/로드 시스템

Newtonsoft JSON 직렬화와 `SORegistry` 기반 ScriptableObject 참조 복원을 사용합니다. 파티 상태, 인벤토리 슬롯 레이아웃, 호감도 진행도, 스토리 플래그, 던전 클리어 이력을 저장합니다.

---

## 프로젝트 구조

```
Scripts/                          (136개 파일)
├── Core/           GameManager, ServiceLocator, EventBus, StateMachine, GameSession
├── Combat/         CombatManager, TurnSystem, SkillExecutor, DamageCalculator, Commands
│   └── State/      6개 전투 FSM 상태
├── Character/      CharacterStats, Inventory, EquipmentLoadout, ProgressionSystem
├── AI/             AIStrategyData (4종), BossAIStrategyData, AIAction
├── Dungeon/        DungeonManager, DungeonMapGenerator, DungeonRewardSystem
│   └── RoomHandlers/   7개 IRoomEventHandler 구현
├── VN/             DialogueManager, AffinitySystem, BranchingController
├── Data/           ScriptableObject 정의 (Skill, Class, Enemy, Equipment 등)
├── Save/           SaveManager, SaveData, SORegistry
├── UI/             화면별 MVP 폴더 (Combat, Inventory, Dungeon, VN, Camp, Result, Title)
└── Utils/          ObjectPool, Extensions

Editor/Content/                   (에디터 전용)
├── ContentImporterBase.cs        Upsert 프레임워크
├── AssetRefResolver.cs           크로스 임포터 참조 해결
├── ContentImporterMenu.cs        1클릭 파이프라인 실행
└── [16개 임포터]                  콘텐츠 타입별 1개
```

---

## 기술 스택

| 구분 | 기술 |
|------|------|
| 엔진 | Unity 6000.3.2f1 (URP 2D) |
| 언어 | C# 10 |
| 비동기 | UniTask |
| 애니메이션 | DOTween |
| 직렬화 | Newtonsoft JSON |
| UI 텍스트 | TextMeshPro |

---

## 개발 타임라인

| 단계 | 내용 | 기간 |
|------|------|------|
| **Phase 1** | 기반 구조 & 핵심 전투 — FSM, EventBus, Command 패턴, 4v4 턴제 전투 | 1~2주차 |
| **Phase 2** | 캐릭터 & 스킬 시스템 — 5개 클래스, 스킬 로드아웃, 장비, 슬롯 기반 인벤토리 | 3~4주차 |
| **Phase 3** | 던전 & AI — Slay the Spire 맵 생성, 방 핸들러, 4+1종 AI 전략, 보스 페이즈 | 5~6주차 |
| **Phase 4** | VN & 호감도 — 대화 노드 그래프, 조건 분기,호감도 | 7주차 |
| **Phase 5** | 게임 루프 — 세이브/로드, 씬 흐름 (Boot→Title→Camp→Dungeon→Result), 전체 루프 완성 | 8주차 |
| **Phase 6** | 콘텐츠 파이프라인 — JSON→SO 임포터,콘텐츠 에셋, 게임플레이/UI 통합 | 9~10주차 |

---

## 사용된 디자인 패턴

| 패턴 | 적용처 |
|------|--------|
| **State Machine (FSM)** | 게임 흐름, 전투 턴, 던전 탐색, AI 페이즈 |
| **Command** | 모든 전투 행동 (Execute/Undo, 히스토리 추적) |
| **Observer / EventBus** | 시스템 전체 디커플링 통신 |
| **Strategy** | 적 AI 행동, 방 이벤트 핸들러 |
| **MVP** | 모든 UI 화면 |
| **Service Locator** | 글로벌 서비스 (Save, Audio, Registry) |
| **Object Pool** | VFX, 데미지 숫자 등 빈번 생성 오브젝트 |
| **Data-Driven (SO)** | 모든 게임 콘텐츠 (스킬, 아이템, 적, 대화, 던전) |

---

## 회고

- **스코프 관리**: 각 페이즈에 명확한 마일스톤을 설정하고, 비필수 기능(Forge, Library, 모네타이제이션)은 과감히 후순위로 밀어 핵심 루프를 기간 내에 완성했습니다.
- **시스템 설계 vs 폴리싱**: 아키텍처(EventBus, Command, MVP, 콘텐츠 파이프라인)에 집중 투자하여, 향후 콘텐츠와 기능 추가가 최소한의 수정으로 가능한 구조를 만들었습니다. 아트/VFX/사운드 폴리싱은 의도적으로 후순위 배치했습니다.
- **콘텐츠 파이프라인**: Phase 6에서 16단계 JSON→SO 임포터를 구축한 것이 가장 효율적인 결정이었습니다. 수일간의 수동 Unity Editor 작업이 1클릭 임포트로 대체되었고, 콘텐츠 이터레이션이 즉시 가능해졌습니다.
---

## 실행 방법

### 빌드 파일
- 이 저장소의 빌드 파일을 다운로드하여 실행할 수 있습니다.

### Unity 에디터
- **Unity 6000.3.2f1** 이상 필요
- **Boot** 씬에서 Play — 타이틀 화면부터 시작됩니다.

---

