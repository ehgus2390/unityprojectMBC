# ArrowSpawner 시스템 사용법

## 개요
`ArrowSpawner`는 VR 환경에서 오른손에 자동으로 화살을 생성하는 시스템입니다. 이 시스템을 사용하면 플레이어가 화살을 수동으로 찾거나 생성할 필요 없이 자동으로 화살을 받을 수 있습니다.

## 설정 방법

### 1. ArrowSpawner 컴포넌트 추가
1. 씬에서 빈 GameObject를 생성합니다.
2. 이름을 "ArrowSpawner"로 변경합니다.
3. `ArrowSpawner` 스크립트를 추가합니다.

### 2. 필수 설정
- **Arrow Prefab**: 생성할 화살 프리팹을 할당합니다.
- **Right Hand Spawn Point**: 오른손 컨트롤러의 Transform을 할당합니다.
- **Right Hand Interactor**: 오른손 XR Direct Interactor를 할당합니다.

### 3. 선택적 설정
- **Spawn Interval**: 화살 생성 간격 (기본값: 2초)
- **Max Arrows**: 최대 화살 개수 (기본값: 5개)
- **Auto Spawn**: 자동 생성 활성화 여부 (기본값: true)
- **Enable Debug Logs**: 디버그 로그 활성화 여부

## 자동 설정
만약 수동으로 설정하지 않아도 시스템이 자동으로 다음을 수행합니다:
- 오른손 컨트롤러 자동 검색 (이름에 "Right", "R_" 포함된 것)
- 스폰 포인트를 오른손 컨트롤러 위치로 설정
- 화살 프리팹이 없으면 경고 메시지 출력

## 기능

### 자동 화살 생성
- 설정된 간격마다 자동으로 화살을 생성합니다.
- 최대 화살 개수에 도달하면 생성을 중지합니다.
- 화살이 파괴되면 카운트가 감소하여 다시 생성됩니다.

### 수동 화살 생성
- Inspector에서 "Spawn Arrow" 버튼을 클릭하여 수동 생성 가능
- 코드에서 `SpawnArrow()` 메서드 호출 가능

### 화살 관리
- `GetCurrentArrowCount()`: 현재 화살 개수 반환
- `GetMaxArrowCount()`: 최대 화살 개수 반환
- `ClearAllArrows()`: 모든 화살 제거

## 문제 해결

### 화살이 생성되지 않는 경우
1. **Arrow Prefab이 설정되었는지 확인**
2. **오른손 컨트롤러가 올바르게 할당되었는지 확인**
3. **Console에서 에러 메시지 확인**

### 화살이 잘못된 위치에 생성되는 경우
1. **Right Hand Spawn Point를 올바른 위치로 설정**
2. **오른손 컨트롤러의 Transform 확인**

### 성능 문제
1. **Max Arrows 값을 줄여보세요**
2. **Spawn Interval을 늘려보세요**
3. **Enable Debug Logs를 비활성화하세요**

## 코드 예제

```csharp
// ArrowSpawner 참조 가져오기
ArrowSpawner arrowSpawner = FindObjectOfType<ArrowSpawner>();

// 수동으로 화살 생성
arrowSpawner.SpawnArrow();

// 자동 생성 시작/중지
arrowSpawner.StartAutoSpawn();
arrowSpawner.StopAutoSpawn();

// 현재 화살 개수 확인
int currentArrows = arrowSpawner.GetCurrentArrowCount();
```

## 주의사항
- 화살 프리팹에는 `XRGrabInteractable`, `Rigidbody`, `ArrowController`, `ArrowInteractable` 컴포넌트가 필요합니다.
- 시스템이 자동으로 누락된 컴포넌트를 추가하지만, 성능을 위해 미리 설정하는 것을 권장합니다.
- VR 컨트롤러의 이름이 표준과 다르면 수동으로 할당해야 할 수 있습니다. 