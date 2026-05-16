# 씬 전환 버그 수정 보고서

## 수정 대상

- `Farming Game/Assets/Scripts/PlayerController.cs`
- `Farming Game/Assets/Scripts/UIController.cs`

---

## Bug 1: 씬 전환 후 캐릭터 이동 불가

### 원인

`PlayerController`는 DontDestroyOnLoad 싱글턴이다. Main 씬에 `Player` 오브젝트가 존재하므로, Main 씬이 재로드될 때 두 번째 `PlayerController` 인스턴스가 생성된다.

Unity 실행 순서: `Awake` → `OnEnable`

- 중복 인스턴스의 `Awake`에서 `Destroy(gameObject)` 호출
- 그 직후 같은 오브젝트의 `OnEnable`이 실행됨
- `moveInput.action`, `actionInput.action`은 InputActionAsset의 공유 인스턴스를 참조
- 중복 인스턴스의 `OnDisable`이 실행되면서 공유 Action을 Disable → 원본의 입력도 차단됨

### 수정 (`PlayerController.cs`)

`OnEnable` / `OnDisable`에 `instance != this` 가드 추가:

```csharp
private void OnEnable()
{
    if (instance != this) return;
    moveInput.action.Enable();
    actionInput.action.Enable();
}

private void OnDisable()
{
    if (instance != this) return;
    moveInput.action.Disable();
    actionInput.action.Disable();
}
```

`Awake`에서 `instance`는 원본이 이미 설정한 값이므로, 중복 인스턴스의 `OnEnable`/`OnDisable`에서는 `instance != this`가 true → 조기 리턴.

---

## Bug 2: "There can be only one active Event System" 경고

### 원인

씬 구조 확인 결과:
- Main.unity의 `EventSystem`은 `UI Canvas`(UIController Canvas GameObject)의 자식 오브젝트
- `UIController`가 DontDestroyOnLoad이므로 `UI Canvas`와 그 하위 `EventSystem`이 씬 전환 후에도 유지됨
- Main 씬 재진입 시 새 `UI Canvas`와 함께 새 `EventSystem`이 생성되고 `Awake` 이후 `OnEnable`이 실행되면서 경고 발생
- 이후 `UIController.Awake`의 중복 브랜치에서 `Destroy(gameObject)`가 호출되지만, `EventSystem.OnEnable`은 이미 실행된 후

Unity Awake 일괄 처리 후 OnEnable이 실행되는 순서가 원인.

### 수정 (`UIController.cs`)

`using UnityEngine.EventSystems` 추가, 중복 브랜치에서 EventSystem을 `DestroyImmediate`로 즉시 제거:

```csharp
else
{
    var dupES = GetComponentInChildren<EventSystem>(true);
    if (dupES != null) DestroyImmediate(dupES.gameObject);
    Destroy(gameObject);
}
```

`DestroyImmediate`를 사용하는 이유: `Destroy`는 프레임 말에 제거하므로 `OnEnable`이 먼저 실행됨. `DestroyImmediate`는 즉시 제거하여 `EventSystem.OnEnable`이 실행되기 전에 오브젝트를 없앰.

---

## 검증된 사실

- Indoors 씬에는 Player 오브젝트 없음 → Main ↔ Indoors 전환 시 PlayerController 중복 생성은 Main 씬 재진입 시에만 발생
- Main Menu, Day End 씬에는 EventSystem 없음 → 중복 EventSystem은 Main 씬에서만 발생
- 다른 6개 DontDestroyOnLoad 싱글턴(AudioManager, CropController, CurrencyController, GridInfo, TimeController, UIController)에는 OnEnable/OnDisable 없음 → 추가 수정 불필요
