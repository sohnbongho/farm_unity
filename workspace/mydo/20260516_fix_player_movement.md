# Unity 6.4 업그레이드 후 캐릭터 이동 불가 버그 수정

## 원인

`InputActionReference`는 명시적으로 `.Enable()`을 호출하지 않으면 항상 `Vector2.zero`를 반환한다.

- `PlayerController.cs`는 `moveInput.action.ReadValue<Vector2>()`로 이동 입력을 읽지만, 어디에도 `.Enable()` 호출이 없었다.
- 씬에 `PlayerInput` 컴포넌트가 없어서 action map을 자동으로 활성화해주는 주체가 없었다.
- Input System 1.11.x 시절에는 내부 동작 차이로 우연히 동작했을 가능성이 있으나, 1.19.0에서는 명시적 Enable이 필수다.

## 확인 내용

| 항목 | 확인 결과 |
|------|-----------|
| `com.unity.inputsystem` 실제 버전 | 1.19.0 (manifest.json 기준) |
| `activeInputHandler` (ProjectSettings) | 2 = Both (New + Legacy) |
| 씬 내 `PlayerInput` 컴포넌트 | 없음 |
| 스크립트 전체에서 `.Enable()` 호출 | 없음 |
| `Rigidbody2D.linearVelocity` API | 문제 없음 (Unity 6 신 API, 이미 적용됨) |
| inputactions WASD 컴포지트 타입 `Dpad` | 호환 유지됨, 변경 불필요 |

## 수정 내용

**파일**: `/mnt/d/01-git/farm_unity/Farming Game/Assets/Scripts/PlayerController.cs`

`OnEnable` / `OnDisable` 추가:

```csharp
private void OnEnable()
{
    moveInput.action.Enable();
    actionInput.action.Enable();
}

private void OnDisable()
{
    moveInput.action.Disable();
    actionInput.action.Disable();
}
```

`DontDestroyOnLoad` 싱글턴이므로 `OnDisable`의 Disable도 함께 처리해야 메모리 누수 없이 안전하다.
