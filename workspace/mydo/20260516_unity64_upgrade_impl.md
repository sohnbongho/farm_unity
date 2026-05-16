# Unity 6.4 업그레이드 구현 결과 보고서

날짜: 2026-05-16

## 구현 완료 항목

### 0단계: git 백업 브랜치 생성

- 브랜치 `backup/before-unity64-upgrade` 생성 완료 (현재 main HEAD 기준)
- 미커밋 WIP가 다수 존재하나 브랜치는 HEAD 포인터만 생성하므로 WIP와 무관하게 처리
- `git branch` 결과: `backup/before-unity64-upgrade` 확인됨

### 3단계: manifest.json 패키지 버전 업데이트

파일: `Farming Game/Packages/manifest.json`

| 패키지 | 이전 | 이후 |
|---|---|---|
| `com.unity.render-pipelines.universal` | 17.0.3 | 17.4.0 |
| `com.unity.inputsystem` | 1.11.0 | 1.13.0 |

**주의사항**: `com.unity.render-pipelines.core`와 `com.unity.shadergraph`는 manifest.json에 직접 명시되지 않고 URP의 간접 의존성으로 관리됩니다. URP를 17.4.0으로 올리면 packages-lock.json에서 자동 동반됩니다 (Unity 에디터가 프로젝트를 열 때 Package Manager가 처리).

**버전 검증 권고**: 설정된 버전(URP 17.4.0, Input System 1.13.0)은 Unity 6.4 호환 추정 버전입니다. 실제 Unity 6000.4.5f1 에디터로 프로젝트를 열었을 때 Package Manager가 "Update Available" 또는 더 높은 권장 버전을 제안하면 그에 따라 업데이트하십시오.

### 4단계: 코드 수정

#### `.tag ==` → `CompareTag()` 변경 (5곳)

| 파일 | 변경 위치 | 변경 내용 |
|---|---|---|
| `AreaSwitcher.cs` | `OnTriggerEnter2D` 31번째 줄 | `collision.tag == "Player"` → `collision.CompareTag("Player")` |
| `BedController.cs` | `OnTriggerEnter2D` 29번째 줄 | `collision.tag == "Player"` → `collision.CompareTag("Player")` |
| `BedController.cs` | `OnTriggerExit2D` 37번째 줄 | `collision.tag == "Player"` → `collision.CompareTag("Player")` |
| `ShopActivator.cs` | `OnTriggerEnter2D` 25번째 줄 | `collision.tag == "Player"` → `collision.CompareTag("Player")` |
| `ShopActivator.cs` | `OnTriggerExit2D` 33번째 줄 | `collision.tag == "Player"` → `collision.CompareTag("Player")` |

grep 결과 Scripts 폴더 전체에서 `.tag ==` 패턴은 위 5곳이 전부였으며, `.tag !=` 패턴은 없음.

#### `GridInfo.cs` Y 키 디버그 코드 Editor 전용 처리

`GridInfo.cs`의 `Update()` 메서드 전체를 `#if UNITY_EDITOR` / `#endif` 블록으로 감쌈.

```csharp
#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            GrowCrop();
        }
    }
#endif
```

빌드 결과물에서 Y 키 디버그 코드가 포함되지 않습니다.

### 7단계: 변경사항 커밋

커밋 해시: `8d961f4`
커밋 메시지: `upgrade: Unity 6.4 준비 - 패키지 버전 업데이트 및 코드 정리`

스테이징 대상 (5개 파일만 선택적 add — 기존 WIP는 제외):
- `Farming Game/Packages/manifest.json`
- `Farming Game/Assets/Scripts/AreaSwitcher.cs`
- `Farming Game/Assets/Scripts/BedController.cs`
- `Farming Game/Assets/Scripts/ShopActivator.cs`
- `Farming Game/Assets/Scripts/GridInfo.cs`

## 건너뛴 항목 (수동 필요)

- Step 1: Unity Hub에서 6000.4.5f1 에디터 설치
- Step 2: Unity Editor에서 프로젝트 열기 및 마이그레이션
- Step 5: 씬별 스모크 테스트
- Step 6: URP 빌드 검증

## 다음 수동 작업 안내

1. Unity Hub → Installs → Unity 6000.4.5f1 설치
2. 프로젝트를 6000.4.5f1로 열기 → "Change Version" 클릭 → Library 재구축 대기
3. Package Manager에서 URP, Input System 권장 버전 확인 및 필요시 추가 업데이트
4. Console 에러 0건 확인 후 씬별 스모크 테스트 진행
