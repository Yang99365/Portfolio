# FlowLab - 논리 회로 시뮬레이션 툴

> **프로젝트 타입**: 팀 프로젝트 (3인) - 대학교 졸업 작품  
> **개발 기간**: 2025.02 ~ 2025.11  
> **담당 역할**: UI/UX 시스템 구현, 논리 노드 개발, 테스트/QA  
> **개발 환경**: Unity 6000.2.6f2, C#

---

## 📌 프로젝트 소개

FlowLab은 다양한 논리 회로 노드를 캔버스에 배치하고 연결하여 실시간으로 시뮬레이션할 수 있는 비주얼 프로그래밍 툴입니다. IronPython을 활용한 커스텀 노드 제작 기능을 통해 사용자가 직접 노드를 확장할 수 있습니다.

### 주요 기능
- 📊 **노드 기반 비주얼 프로그래밍**: 드래그 앤 드롭으로 논리 회로 구성
- ⚡ **실시간 시뮬레이션**: 신호 전파 및 상태 변화 시각화
- 🔧 **커스텀 노드 제작**: IronPython 스크립트를 통한 노드 확장
- 🎨 **직관적인 UI/UX**: 설정 시스템, 키맵 커스터마이징, 컷신 시스템

---

## 🎯 담당 업무

### 1. UI/UX 시스템 구현 

#### 설정 시스템
```csharp
- 설정 데이터 직렬화/역직렬화 (OdinSerializer)
- AudioMixer 연동 VFX 볼륨 관리
- 시뮬레이션 속도 조절 (Frame/FixedTime/Immediately 모드)
- Temp/Current 설정 분리 구조
```

**구현 내용:**
- 게임 설정을 JSON 파일로 저장/로드
- AudioMixer와 연동하여 실시간 볼륨 조절 (데시벨 변환 로직 포함)
- 시뮬레이션 실행 모드 전환 (프레임 단위, 고정 시간, 즉시 실행)
- 설정 변경 전 임시 저장 기능 (Apply 버튼을 눌러야 실제 적용)

#### 커스텀 키맵 설정 UI
```csharp
- UniTask 기반 비동기 키 입력 감지
- Modifier(Ctrl, Shift) + Action Key 조합 지원
- 실시간 키맵 중복 검증
- 동적 UI 생성 및 관리
```

**구현 내용:**
- 사용자가 버튼을 누르면 키 입력을 대기하고, 입력된 키 조합을 감지
- Ctrl+Z, Shift+A 등 복합키 조합 지원
- 키맵 중복 감지 시 자동으로 None으로 변경 (빨간색 경고 표시)
- InputManager와 통합하여 입력 블로킹 처리

#### 컷신 시스템
```csharp
- JSON 기반 데이터 주도 컷신 관리
- 5가지 화면 전환 효과 (Fade, CrossFade, SlideLeft, SlideRight, None)
- TextDisplay와의 이벤트 기반 연동
- UniTask 비동기 처리
```

**구현 내용:**
- 컷신 데이터베이스를 JSON에서 로드하여 시퀀스 단위로 재생
- Fade, CrossFade, Slide 등 다양한 화면 전환 효과 구현
- 대화 시스템(TextDisplay)과 연동하여 대화 종료 시 자동으로 다음 컷신 진행
- Duration 설정을 통한 컷신 자동 진행 및 수동 진행 지원

---

### 2. 논리 노드 구현

팀에서 정의한 Node 프레임워크를 기반으로 다양한 논리 노드를 구현했습니다.

**주요 구현 특징:**

```csharp
public class Add : DynamicIONode, INodeAdditionalArgs<int>
{
    protected override int DefaultInputCount => 2;
    
    protected override void OnAfterInit()
    {
        SplitterSupport.Initialize(InputCount, value =>
        {
            InputCount = value;
            ReportChanges();
        });
    }
}

// 2. 타입 전환 시스템 - 우클릭 컨텍스트 메뉴로 타입 변경
protected override List<ContextElement> ContextElements
{
    get
    {
        if (_contexts == null)
        {
            _contexts = base.ContextElements;
            _contexts.Add(new ContextElement($"Int → In", () => SetInputType(TransitionType.Int)));
            _contexts.Add(new ContextElement($"Float → In", () => SetInputType(TransitionType.Float)));
        }
        return _contexts;
    }
}

// 3. 상태 업데이트 - 입력 신호 변화 시 자동 계산
protected override void StateUpdate(TransitionEventArgs args)
{
    if (InputToken.HasOnlyNull)
    {
        OutputToken[0].State = TransitionType.Bool.Null();
        return;
    }
    
    OutputToken[0].State = InputToken[0].State && InputToken[1].State;
}
```

**노드 구현 과정:**
1. 노드 베이스 클래스(Node, DynamicIONode)를 상속
2. 입력/출력 핀 이름, 타입, 개수 정의
3. StateUpdate 메서드에서 노드의 연산 로직 구현
4. ContextMenu를 통한 노드 설정 기능 추가

---

### 3. 테스트 및 QA

- 다양한 노드 조합 시나리오 테스트
- 시뮬레이션 성능 최적화를 위한 버그 리포트 작성
- UI/UX 개선을 위한 사용성 테스트

---

## 🛠️ 기술 스택

### 주요 라이브러리
- **UniTask**: 비동기 처리 (키 입력 감지, 컷신 재생)
- **OdinSerializer**: 복잡한 데이터 구조 직렬화
- **IronPython**: 커스텀 노드 스크립트 실행 (팀원 구현)
- **TextMeshPro**: UI 텍스트 렌더링

### 주요 개발 기법
- **이벤트 기반 아키텍처**: UI와 시스템 간 느슨한 결합
- **데이터 주도 개발**: JSON 기반 설정 및 컷신 관리
- **템플릿 메서드 패턴**: Node 베이스 클래스를 통한 노드 구현 표준화

---

## 💡 배운 점

### 1. 프레임워크 기반 개발
- 팀장이 설계한 Node 프레임워크를 이해하고 활용하는 경험
- 템플릿 메서드 패턴을 통한 일관된 노드 구현 방법 습득

### 2. UI/UX 시스템 설계
- 복잡한 설정 시스템의 Temp/Current 분리 구조 설계
- 이벤트 기반 아키텍처를 통한 UI와 로직의 분리

### 3. 비동기 프로그래밍
- UniTask를 활용한 비동기 입력 처리 및 컷신 시스템 구현
- CancellationToken을 통한 안전한 비동기 작업 취소 처리

### 4. 데이터 주도 개발
- JSON 기반 컷신 데이터베이스 관리
- OdinSerializer를 통한 복잡한 데이터 구조 직렬화

---

## 🔗 기술 상세

### 주요 구현 사항

#### 1. 설정 시스템 아키텍처
```
SettingData (직렬화 데이터)
    ↓
Setting (정적 클래스 - 전역 설정 관리)
    ↓
UI_Settings (UI 컴포넌트)
    ↓
SettingKeyManager (키맵 중복 검증)
```

#### 2. 컷신 시스템 플로우
```
JSON Database 로드
    ↓
ShowCutsceneSequence() 호출
    ↓
이미지 전환 효과 재생
    ↓
DialogueSystem 연동 (선택 사항)
    ↓
다음 컷신 or 종료
```

#### 3. 키맵 감지 시스템
```
버튼 클릭
    ↓
KeyMapDetector.GetKeyMapAsync() 호출
    ↓
InputManager 블로킹 활성화
    ↓
키 입력 대기 (UniTask)
    ↓
Modifier + Action Key 조합 반환
    ↓
중복 검증 후 UI 업데이트
```


## 📝 프로젝트 회고

팀 프로젝트에서 팀장이 설계한 아키텍처를 이해하고, 그에 맞춰 기능을 구현하는 경험을 했습니다. 특히 UI/UX 시스템과 논리 노드를 직접 구현하면서 프레임워크 기반 개발과 이벤트 주도 설계를 체득할 수 있었습니다.

비록 핵심 시뮬레이션 로직을 직접 설계하지는 못했지만, 완성도 있는 UI 시스템과 다양한 논리 노드를 구현하며 Unity 프로젝트의 전체 구조를 이해할 수 있었습니다. 향후 개인 프로젝트에서는 설계부터 구현까지 전 과정을 주도하여 더 깊이 있는 개발 경험을 쌓고자 합니다.

---


