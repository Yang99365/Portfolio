# ShooterArena

> 실시간 멀티플레이어 탑다운 슈터 .io — Unity 클라이언트 프로그래머 포트폴리오.  
> 서버 권위 네트워킹, 클라이언트 예측, 측정 기반 성능 최적화를 구현합니다.

**엔진:** Unity 6 · **네트워킹:** Photon Fusion 2 (Dedicated Server 모드) · **플랫폼:** PC · **개발:** 솔로 , 개발기간 3일

---

## 기술 핵심

- **완전한 서버 권위** — 이동·피격·리스폰·매치 점수 등 모든 상태 변경은 전용 서버에서만 검증·적용됩니다. 클라이언트는 UX 피드백 역할만 합니다.
- **클라이언트 예측 + 서버 조정** — 입력을 로컬에서 즉시 반영하고, 서버 스냅샷이 도착하면 오예측을 조정합니다. 100ms 시뮬레이션 RTT 환경에서 검증했습니다.
- **결정론적 투사체** — 발사 사실(틱, 방향)만 동기화합니다. 서버와 클라이언트 양쪽이 `tick × playerId` 시드 기반의 동일한 궤적을 재현합니다. 발사체당 `NetworkObject`가 없으며, 오예측 피격 철회도 없습니다.
- **3-split 매니저 허브** — `Managers.Shared / .Server / .Client` 구조로 권위 경계를 메서드별 분기 대신 코드 구조 수준에서 강제합니다.

---

## 아키텍처

### 매니저 3-split (Shared / Server / Client)

매니저 레이어를 Fusion의 권위 토폴로지에 직접 대응하는 세 서브허브로 나눴습니다.

| 서브허브 | Fusion 역할 | 담당 |
|---|---|---|
| `Managers.Shared` | 권위 독립 | DataManager(JSON), `Define.cs` 상수, `DeterministicRng` |
| `Managers.Server` | StateAuthority | 피격 판정, 피해, 리스폰, 봇 시뮬레이션, 매치 상태, AOI |
| `Managers.Client` | InputAuthority | 입력 폴링, 클라이언트 예측, 보간, 이펙트, UI, 오브젝트 풀 |

서브허브 간 교차 접근은 코드 리뷰 실패 조건입니다. `Managers.Server.*`는 UI나 클라이언트 예측 코드에서 절대 호출하지 않습니다.

### 권위 모델

```
클라이언트 → PlayerInput (INetworkInput) → Fusion 틱
                                               ↓
                                  서버: FixedUpdateNetwork
                                    - 입력 검증
                                    - 이동 / 발사 / 피해 적용
                                    - [Networked] 상태 변경
                                               ↓
                                  클라이언트: 스냅샷 수신, 예측 조정
                                    - Render(): 비주얼 보간, 투사체 FX 스폰
```

- **투사체는 `NetworkObject`가 아닙니다.** 서버는 발사와 레이캐스트 히트 판정을 동일 틱에 처리합니다. 클라이언트는 로컬 `ProjectileView`를 스폰해 독립적으로 애니메이션합니다. 권위 있는 투사체 위치는 복제되지 않습니다.
- **EventBus는 클라이언트 로컬 전용입니다.** 네트워크 경계를 절대 넘지 않습니다. 서버 코드는 Fusion 네이티브 메커니즘(`[Networked]` + RPC)만 사용합니다.
- **결정론적 경로에서 `UnityEngine.Random`은 금지입니다.** `DeterministicRng`가 `(tick, playerId)` 시드로 서버와 예측 클라이언트에서 동일한 값을 생성합니다.

### 봇 시뮬레이션

서버에 `StateAuthority`를 가진 `BotNetworkObject`가 봇마다 존재합니다. `BOT_AI_TICK_INTERVAL` 게이팅으로 10Hz 2-상태 AI(배회/공격)를 구동하며, 경로 추종은 60Hz로 실행됩니다. A* Pathfinding Pro를 사용해 장애물을 회피합니다. 봇의 발사도 플레이어와 동일한 결정론적 투사체 파이프라인을 사용하며, 서버가 틱별 탄환 시뮬레이션과 레이캐스트 히트 판정을 처리합니다.

---

## 성능 최적화

커스텀 CSV 로거로 기준값을 측정한 뒤 세 가지 변수를 조정했습니다.

- **AOI 반경 축소 (20u → 15u)**: 60봇 환경에서 클라이언트 수신 대역폭 약 50% 감소
- **투사체 뷰 오브젝트 풀링**: `Stack<ProjectileView>` 기반 풀 도입으로 GC gen0 수집 약 37% 감소. 프레임 타임 변화 없음 — 효과는 처리량이 아닌 GC 스파이크 완화
- **봇 AI 결정 주파수 (60Hz → 10Hz 게이팅)**: GC 약 37%, 대역폭 약 55% 감소. CPU 절감 아닌 A* `Path` 할당 빈도와 `[Networked]` 상태 복제 횟수 감소가 주효
- **SendRate 스윕**: SR-half(30Hz)에서 SR-quarter(15Hz)로 낮춰도 추가 절감 6%에 그침 — Fusion 프로토콜 오버헤드가 하한을 형성. SR-half를 최적 동작점으로 확정

---

## 무기 시스템

| | Rapid | Cannon |
|---|---|---|
| 피해 | 10 | 50 |
| 발사 속도 | 10발/초 (기본) | 0.67발/초 |
| 투사체 속도 | 15u/초 | 6u/초 |
| 역할 | 근접 난전, 관용적 | 장거리 저격, 앞 조준 필요 |
| 히트 모델 | 즉시 레이캐스트 (서버) | 틱별 단계 캐스트 (양쪽 피어, 서버 권위) |

양 무기는 스폰 시부터 소지하며, **Q** 키로 자유롭게 전환합니다. 무기 상태는 `[Networked]`이며 클라이언트에서 예측됩니다 — 오예측된 전환은 고스트 발사 없이 조정됩니다.

---

## 게임플레이 루프

- **WASD** 이동 · **마우스** 조준 · **LMB** 발사 · **Q** 무기 전환
- 봇 처치 +1점 / 플레이어 처치 +(상대 킬카운트 + 2)점
- 킬당 성장: 최대 체력 +10, 이동속도 +0.25u/초, 발사 쿨다운 −5ms (최대 10킬까지)
- 30점 먼저 달성 시 매치 승리 → 5초 후 자동 재시작

---

## 실행 방법

### 요구 사항

- Unity 6 (URP 2D)
- Photon Fusion 2 SDK
- A* Pathfinding Project Pro (봇 경로 탐색)
- DOTween (히트/사망 이펙트)

### 로컬 실행 (서버 + 클라이언트)

```
# 서버 폴더 속 서버 빌드 실행 (SERVER_BUILD define, -batchmode -nographics)
ShooterArena.exe -batchmode -nographics --bots=10

# 클라이언트 폴더 속 클라이언트 빌드 실행
ShooterArena.exe (관리자권한)
```

클라이언트는 `localhost`에 자동 접속합니다. 추가 클라이언트 인스턴스를 실행해 멀티 클라이언트 테스트를 할 수 있습니다.

### CLI 플래그 (서버 빌드 전용)

| 플래그 | 기본값 | 설명 |
|---|---|---|
| `--bots=N` | 1 | 서버 시작 시 N봇 스폰 |
| `--ai-hz=N` | 10 | 봇 AI 결정 주파수 Hz (1–60) |
| `--log-scenario=<label>` | — | CSV 측정 로깅 활성화 |
| `--log-duration=<초>` | 300 | 측정 윈도우 길이 |

---

## 프로젝트 구조

```
Assets/Scripts/
├── Core/
│   ├── Managers.cs / Managers.Shared.cs / Managers.Server.cs / Managers.Client.cs
│   ├── EventBus.cs                  # 클라이언트 로컬 발행/구독
│   └── Events/                      # OnLocalPlayerDamaged, OnLocalBotDied 등
├── Network/
│   ├── GameBootstrap.cs             # NetworkRunner 라이프사이클, 스폰 허브
│   └── PlayerInput.cs               # INetworkInput 구조체
├── Player/
│   └── PlayerNetworkObject.cs       # 예측, Rapid/Cannon 발사, 킬당 성장, AOI
├── Bot/
│   └── BotNetworkObject.cs          # 서버 전용 AI, A* 경로 추종, 봇 발사 파이프라인
├── Server/
│   ├── BotManager.cs                # --bots=N 스폰, 리스폰 타이머
│   ├── PlayerRespawnManager.cs      # 3초 리스폰, MatchScore 유지
│   ├── MatchManager.cs              # 승리 조건, 매치 재시작
│   └── SpawnManager.cs              # 스폰 포인트 선택
├── Match/
│   └── MatchStateNetworkObject.cs   # Phase/Winner 복제, OnLocalMatchEnded
├── Client/
│   ├── ProjectileView.cs            # 비주얼 투사체, selfFlashable 자기 피격 패스스루
│   ├── ProjectilePool.cs            # Stack<ProjectileView> 풀 (Rapid ×20, Cannon ×5)
│   ├── FxManager.cs                 # 봇/플레이어 사망 버스트, 피해 피드백
│   ├── DeathBurstEffect.cs          # DOTween 바디 클론 + 파편 버스트
│   ├── CameraFollow.cs              # 데드존 추종 + 화면 흔들림
│   └── IHitFlashable.cs             # 히트 플래시 인터페이스 (플레이어, 봇)
├── UI/
│   ├── UI_Base.cs                   # Bind<T>/Get<T> 열거형 키 바인딩 패턴
│   ├── UI_GameScene.cs              # HUD: 체력, 킬, 점수, 무기
│   └── UI_ResultPopup.cs            # 승패 팝업, 매치 재시작
├── Data/
│   ├── Define.cs                    # 프로젝트 전체 상수
│   ├── DataManager.cs               # 제네릭 ILoader<T> JSON 로딩
│   └── MapData.cs                   # 30×30 아레나 정의
├── Shared/
│   └── DeterministicRng.cs          # tick × playerId 시드 RNG
└── Utils/
    ├── BaselineLogger.cs            # CSV 벤치마크 로거 (--log-scenario 전용)
    └── Util.cs                      # FindChild 헬퍼
```

---

## 기술 스택

| | |
|---|---|
| 엔진 | Unity 6 (URP 2D) |
| 언어 | C# |
| 네트워킹 | Photon Fusion 2 — Dedicated Server 모드 |
| 경로 탐색 | A* Pathfinding Project Pro |
| 트위닝 | DOTween |
| 빌드 토폴로지 | 헤드리스 서버 exe + 스탠드얼론 클라이언트 exe |
| 측정 도구 | 커스텀 CSV 로거 + Fusion FusionStatistics |
