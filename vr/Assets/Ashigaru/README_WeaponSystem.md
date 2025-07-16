# Ashigaru 무기 시스템 사용법

## 개요
이 시스템은 Ashigaru 캐릭터에 다양한 무기를 동적으로 장착할 수 있게 해주는 스크립트 모음입니다.

## 포함된 스크립트

### 1. WeaponController.cs
- 무기 장착/해제를 담당하는 메인 스크립트
- 무기 소켓(장착 위치) 관리
- 다양한 무기 프리팹 지원

### 2. WeaponUI.cs
- UI 버튼을 통한 무기 장착/해제
- Canvas와 Button 컴포넌트 필요

### 3. WeaponInput.cs
- 키보드 입력을 통한 무기 장착/해제
- 기본 키: 1(카타나), 2(크로스스피어), 0(무기해제)

## 설정 방법

### 1. Ashigaru 프리팹 설정
1. Unity 에디터에서 `Ashigaru.prefab`을 더블클릭하여 편집 모드로 진입
2. Ashigaru의 자식으로 빈 GameObject를 생성하고 이름을 "WeaponSocket"으로 지정
3. WeaponSocket의 위치를 무기를 장착할 적절한 위치로 조정 (보통 손이나 허리)
4. Ashigaru에 `WeaponController` 스크립트 추가
5. WeaponController의 Inspector에서:
   - Weapon Socket: 방금 생성한 WeaponSocket을 할당
   - Katana Prefab: Katana.prefab을 할당
   - Cross Spear Prefab: Cross Spear.prefab을 할당

### 2. UI 설정 (선택사항)
1. Canvas 생성
2. Canvas에 3개의 Button 추가 (카타나, 크로스스피어, 무기해제)
3. Canvas에 `WeaponUI` 스크립트 추가
4. WeaponUI의 Inspector에서 각 버튼과 WeaponController 연결

### 3. 키보드 입력 설정 (선택사항)
1. Ashigaru에 `WeaponInput` 스크립트 추가
2. WeaponInput의 Inspector에서 WeaponController 연결
3. 필요에 따라 키 설정 변경

## 사용법

### 런타임에서 무기 변경
- **키보드**: 1(카타나), 2(크로스스피어), 0(무기해제)
- **UI 버튼**: 각 버튼 클릭
- **스크립트**: `weaponController.EquipKatana()`, `weaponController.EquipCrossSpear()` 등

### 코드에서 무기 장착
```csharp
WeaponController weaponController = GetComponent<WeaponController>();
weaponController.EquipKatana(); // 카타나 장착
weaponController.EquipCrossSpear(); // 크로스스피어 장착
weaponController.UnequipWeapon(); // 무기 해제
```

## 주의사항
- WeaponSocket의 위치와 회전을 적절히 조정하여 무기가 자연스럽게 보이도록 설정
- 무기 프리팹들이 올바르게 할당되었는지 확인
- 런타임에서 무기를 변경할 때 기존 무기는 자동으로 제거됨

## 확장 방법
새로운 무기를 추가하려면:
1. 무기 프리팹 생성
2. WeaponController에 새로운 무기 변수 추가
3. 새로운 장착 메서드 추가
4. UI나 입력 시스템에 새로운 무기 옵션 추가 