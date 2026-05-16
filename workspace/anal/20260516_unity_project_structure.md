# Unity 농장 게임 프로젝트 전체 구조 분석

날짜: 2026-05-16

## 요약

Unity 6 (6000.0.21f1) 기반의 2D 탑다운(top-down) 농장 시뮬레이션 게임이다. Udemy 강좌(James Doyle)를 따라 제작된 학습용 프로젝트로, 23개의 C# 스크립트로 구성되며 Singleton 패턴을 핵심 아키텍처로 채택해 씬 간 데이터를 유지한다.

---

## 대상

- 엔진: Unity 6000.0.21f1
- 렌더 파이프라인: Universal Render Pipeline (URP) 17.0.3
- 입력 시스템: New Input System (com.unity.inputsystem 1.11.0)
- UI: TextMesh Pro + UGUI
- 스크립트 총 23개, 모두 `Farming Game/Assets/Scripts/` 위치

---

## 폴더 구조

```
farm_unity/
├── Farming Game/                         # Unity 프로젝트 루트
│   ├── Assets/
│   │   ├── Animations/                   # 플레이어 애니메이션 (.anim × 4, .controller × 1)
│   │   │   ├── Player_Idle.anim
│   │   │   ├── Player_Move.anim
│   │   │   ├── Player_Ploughing.anim
│   │   │   ├── Player_WateringCan.anim
│   │   │   └── Sprite.controller
│   │   ├── Prefabs/
│   │   │   └── Area Switcher.prefab      # 씬 전환 트리거 프리팹
│   │   ├── Scenes/
│   │   │   ├── Main Menu.unity           # 타이틀 화면 (빌드 인덱스 0)
│   │   │   ├── Main.unity                # 야외 농장 (빌드 인덱스 1)
│   │   │   ├── Indoors.unity             # 실내 씬 (빌드 인덱스 2)
│   │   │   ├── Indoors-Copy.unity        # 실내 씬 복사본 (빌드 인덱스 3)
│   │   │   ├── Day End.unity             # 하루 종료 화면 (빌드 인덱스 4)
│   │   │   └── SampleScene.unity         # 빌드 목록 미포함 (테스트용)
│   │   ├── Scripts/                      # 모든 C# 스크립트
│   │   ├── Settings/                     # URP 렌더 설정
│   │   ├── TextMesh Pro/                 # TMP 폰트/리소스
│   │   ├── Tiles/
│   │   │   ├── Indoors/                  # 실내 타일 에셋
│   │   │   ├── Objects/                  # 오브젝트 타일
│   │   │   └── Outdoor Ground/          # 야외 지형 타일
│   │   └── _Udemy Farming Assets - James Doyle/
│   │       ├── Art/
│   │       │   └── Tiny Wonder Farm/
│   │       │       ├── Character/        # 캐릭터 스프라이트 시트 (walk, farming animations)
│   │       │       ├── Objects & Items/  # 가구, 아이템, 식물, 계절 오브젝트 스프라이트
│   │       │       └── Tilemaps/         # 타일맵 스프라이트 (farm inside, summer farm)
│   │       ├── Audio/
│   │       │   ├── Music/                # BGM1~3.ogg, Title Music.ogg
│   │       │   └── SFX/                  # Chest-Buy, Day End, Pickup Crop, Plant, Plough,
│   │       │                             #   UI Select, Wake Up, Water (.ogg × 8)
│   │       └── Font/                     # Pixelnauts SDF 폰트
│   ├── Packages/
│   │   └── manifest.json                 # 패키지 의존성 목록
│   └── ProjectSettings/                  # Unity 프로젝트 설정 일체
└── Udemy+Farming+Assets+-+James+Doyle.unitypackage  # 원본 에셋 패키지
```

---

## 씬 구성 및 전환 흐름

```
[Main Menu] --PlayGame()--> [Main (야외 농장)]
                                   |
                          AreaSwitcher(트리거) --> [Indoors / Indoors-Copy]
                                   |
                          BedController(침대 상호작용)
                          또는 TimeController.EndDay()
                                   |
                                   v
                             [Day End]
                          (아무 키 입력) --> [Main (야외 농장)] (StartDay)
```

- 씬 전환 시 Singleton들은 `DontDestroyOnLoad`로 유지된다.
- `AreaSwitcher`는 `PlayerPrefs`에 전환 이름을 저장해 진입 위치를 복원한다.
- 메인 메뉴 복귀 시 `UIController.MainMenu()`에서 모든 Singleton GameObject를 명시적으로 `Destroy`한다.

---

## 핵심 시스템 분석

### 1. Singleton 관리자 시스템

프로젝트 전체를 관통하는 핵심 패턴이다. 아래 6개 클래스가 Singleton으로 동작하며 `DontDestroyOnLoad`를 적용한다.

| 클래스 | 역할 |
|--------|------|
| `PlayerController` | 플레이어 이동/도구 사용 |
| `UIController` | 전체 UI 허브 (툴바, 시간 표시, 인벤토리, 상점, 일시정지) |
| `TimeController` | 게임 내 시간 진행 및 하루 종료 처리 |
| `GridInfo` | 씬 전환 간 농장 격자 상태 보존 |
| `CropController` | 작물/씨앗 데이터 중앙 관리 |
| `CurrencyController` | 게임 머니 관리 |
| `AudioManager` | BGM/SFX 재생 |

`GridController`는 씬에 종속된 일반 MonoBehaviour로, 씬 로드마다 새로 생성된다.

---

### 2. 농장 격자(Grid) 시스템

**GridController** (씬 종속):
- `minPoint`, `maxPoint` Transform으로 격자 범위를 정의한다.
- `GenerateGrid()`에서 격자 전체를 순회하며 `GrowBlock` 프리팹을 인스턴스화한다.
- `Physics2D.OverlapBox`로 장애물(gridBlockers 레이어)과 겹치는 블록은 `preventUse = true`로 설정해 사용 불가 처리한다.
- `GridInfo.hasGrid`를 확인해 기존 저장 데이터가 있으면 복원하고, 없으면 새 그리드를 생성한다.
- `GetBlock(float x, float y)`로 월드 좌표를 격자 인덱스로 변환해 블록을 반환한다.

**GrowBlock** (각 격자 칸):
- 성장 단계를 `GrowStage` enum으로 관리한다: `barren → ploughed → planted → growing1 → growing2 → ripe`
- 각 도구 동작 조건:
  - `PloughSoil()`: barren 상태에서만 갈 수 있다.
  - `WaterSoil()`: 단계 무관 (preventUse만 체크)
  - `PlantCrop()`: ploughed + isWatered 조건 충족 시 식재, 씨앗 소비
  - `HarvestCrop()`: ripe 상태에서 수확, 작물 인벤토리에 추가
  - `AdvanceCrop()`: isWatered 조건 시 다음 성장 단계로 진행, isWatered를 false로 리셋
- 상태 변경 시마다 `UpdateGridInfo()`를 호출해 `GridInfo`에 동기화한다.

**GridInfo** (Singleton, DontDestroyOnLoad):
- `List<InfoRow>` 구조로 격자 전체 상태를 인메모리에 보관한다.
- `GrowCrop()`: 하루 종료 시 호출. 각 블록의 isWatered 조건 확인 후 `growFailChance`를 적용해 랜덤 성장 실패를 처리한다. 물을 안 준 `ploughed` 블록은 `barren`으로 되돌린다.

---

### 3. 플레이어 조작 시스템

**입력**: New Input System (InputActionReference) + Keyboard.current 직접 폴링 혼용

**이동**: Rigidbody2D.linearVelocity에 정규화된 입력 벡터 × moveSpeed를 적용

**도구 선택**:
- Tab 키: 순환 전환 (plough → wateringCan → seeds → basket → plough)
- 숫자 1~4: 직접 선택

**도구 인디케이터**:
- 마우스 월드 좌표를 추적하되, 플레이어로부터 `toolRange`(3f) 이내로 클램프한다.
- 정수 좌표계에 스냅(FloorToInt + 1f 오프셋)한다.

**도구 사용 쿨다운**: `toolWaitTime`(0.5f) 동안 이동을 차단해 도구 사용 딜레이를 구현한다.

**UI 우선 처리**: 인벤토리/상점/일시정지 화면이 활성이면 이동을 차단한다.

---

### 4. 시간 시스템

- `currentTime`이 `dayStart`에서 `dayEnd`까지 `timeSpeed`(0.25f) 배율로 증가한다.
- `currentTime > dayEnd` 도달 시 `EndDay()` 호출 → `GridInfo.GrowCrop()` → `Day End` 씬 로드
- `UIController.UpdateTimeText()`로 12시간제 AM/PM 텍스트를 갱신한다.

---

### 5. 작물(Crop) 시스템

**CropType** enum: pumpkin, lettuce, carrot, hay, potato, strawberry, tomato, avocado (8종)

**CropInfo** 데이터 클래스 (Inspector 직렬화):
```
cropType, finalCrop(Sprite), seedType(Sprite), planted/growStage1/growStage2/ripe(Sprite),
seedAmount(int), cropAmount(int), growthFailChance(0~100%), seedPrice(float), cropPrice(float)
```

**CropController**:
- `GetCropInfo(CropType)`: 선형 탐색으로 CropInfo 반환
- `UseSeed()`, `AddCrop()`, `AddSeed()`, `RemoveCrop()`: 인벤토리 수량 조작

---

### 6. 경제(Economy) 시스템

**ShopSeedDisplay.BuySeed(int amount)** 흐름:
1. `CurrencyController.CheckMoney(seedPrice × amount)` 잔액 확인
2. `CropController.AddSeed()` 씨앗 추가
3. `CurrencyController.SpendMoney()` 금액 차감
4. UI 갱신 + SFX 재생

**ShopCropDisplay.SellCrop()** 흐름:
1. cropAmount > 0 확인
2. `CurrencyController.AddMoney(cropAmount × cropPrice)` 수익 추가
3. `CropController.RemoveCrop()` 작물 전량 제거 (일괄 판매)
4. UI 갱신 + SFX 재생

---

### 7. UI 시스템

`UIController`가 허브 역할을 하며 다음 요소들을 참조한다:
- `toolbarActivatorIcons[]`: 현재 도구에 해당하는 아이콘만 활성화
- `timeText`: AM/PM 형식 시간 표시
- `moneyText`: 현재 소지금
- `seedImage`: 현재 선택된 씨앗 아이콘
- `theIC` (InventoryController): 인벤토리 패널
- `theShop` (ShopController): 상점 패널
- `pauseScreen`: 일시정지 화면

일시정지 시 `Time.timeScale = 0f`로 게임 루프 전체를 정지한다.

---

### 8. 오디오 시스템

**BGM**: 배열 기반 순차 재생. 현재 트랙 종료 시 자동으로 다음 트랙을 재생한다 (Update에서 `isPlaying` 폴링).

**SFX 인덱스 매핑** (추정):
- 0: Chest-Buy (상점 열기)
- 1: Day End
- 2: Pickup Crop (수확)
- 3: Plant (식재)
- 4: Plough (경작)
- 5: UI Select (UI 조작)
- 6: Wake Up (기상)
- 7: Water (물주기)

`PlaySFXPitchAdjusted()`는 피치를 [0.8, 1.2] 범위로 랜덤화해 단조로움을 방지한다.

---

### 9. 씬 전환 시스템

**AreaSwitcher**: 트리거 콜라이더 방식. 플레이어 진입 시 `PlayerPrefs.SetString("Transition", transitionName)`을 저장하고 씬을 로드한다. 대상 씬의 AreaSwitcher.Start()에서 key를 비교해 플레이어 위치를 `startPoint`로 이동시킨다.

**BedController**: 플레이어가 침대 근처에 있을 때 Space/E/좌클릭으로 `TimeController.EndDay()`를 직접 호출해 조기 종료를 허용한다.

---

### 10. 메인 메뉴 연출

**MainMenuBGObjectSpawner**: `timeBetweenSpawns` 간격으로 오브젝트 풀에서 랜덤 오브젝트를 선택해 화면 상단에서 생성한다.

**MainMenuFallingObject**: 각 오브젝트가 랜덤 낙하 속도 + 랜덤 회전 속도로 떨어지며 `destroyHeight` 이하에서 자동 소멸한다.

---

## 아키텍처 패턴

| 패턴 | 적용 위치 | 목적 |
|------|-----------|------|
| Singleton | Player/UI/Time/Grid/Crop/Currency/Audio Controller | 씬 전환 간 상태 보존, 전역 접근 |
| 데이터-뷰 분리 | CropInfo(데이터) / CropDisplay, SeedDisplay(뷰) | 데이터와 UI 표현 분리 |
| State Machine | GrowBlock.GrowStage enum | 작물 성장 단계 관리 |
| Observer (단순 폴링) | AudioManager.Update의 isPlaying 체크 | BGM 자동 순환 |
| PlayerPrefs 기반 씬 파라미터 | AreaSwitcher | 씬 로드 후 진입 위치 복원 |

---

## 주요 발견

1. **에디터 전용 디버그 코드**: `GrowBlock.Update()`에 `#if UNITY_EDITOR` 블록으로 N키 강제 성장 기능이 있다. `GridInfo.Update()`에는 Y키로 즉시 하루 종료를 처리하는 코드가 조건문 없이 남아 있다 (빌드에도 포함됨).

2. **주석 처리된 한국어 코드**: `GridController`, `GrowBlock`, `PlayerController` 등 여러 파일에 깨진 인코딩의 한국어 주석이 남아 있다. 개발 과정의 학습 메모로 보인다.

3. **GetCropInfo 선형 탐색**: 작물 수가 8종으로 고정이라 실용적 문제는 없지만, 매 프레임 호출되는 경로에서도 Dictionary 없이 List 선형 탐색을 사용한다.

4. **씬 종속 Singleton 위험**: `GridController.instance`는 씬 재로드 시 새로 할당되므로 다른 Singleton들이 이를 null 체크 없이 참조하면 NullReferenceException이 발생할 수 있다. `PlayerController.Update()`에서는 `GridController.instance != null` 체크를 하고 있다.

5. **일괄 판매 방식**: `ShopCropDisplay.SellCrop()`은 해당 작물 전량을 한 번에 판매한다. 부분 판매 기능은 없다.

6. **씩씩한 씬 복사본**: `Indoors-Copy.unity`가 별도로 존재하는데 용도가 코드상 명확하지 않다 (확인 필요). 빌드 설정에는 포함되어 있다.

7. **작물 심기 조건**: `PlantCrop()`은 `ploughed AND isWatered` 상태를 동시에 요구한다. 갈고 물을 준 뒤에야 씨앗을 심을 수 있는 구조다.

---

## 관련 파일

| 파일 | 역할 |
|------|------|
| `Scripts/PlayerController.cs` | 플레이어 이동, 도구 선택, 도구 사용 입력 처리 |
| `Scripts/GridController.cs` | 격자 생성, 블록 조회 |
| `Scripts/GrowBlock.cs` | 개별 격자 칸 상태 및 동작 |
| `Scripts/GridInfo.cs` | 씬 전환 간 격자 상태 영속 보관, 하루 성장 처리 |
| `Scripts/CropController.cs` | 작물/씨앗 데이터 CRUD |
| `Scripts/TimeController.cs` | 게임 시간 진행, 하루 종료 트리거 |
| `Scripts/UIController.cs` | UI 허브, 일시정지, 메인 메뉴 복귀 |
| `Scripts/CurrencyController.cs` | 소지금 증감 |
| `Scripts/AudioManager.cs` | BGM 순환 재생, SFX 피치 조정 재생 |
| `Scripts/AreaSwitcher.cs` | 씬 전환 트리거 + 진입 위치 복원 |
| `Scripts/BedController.cs` | 침대 상호작용으로 하루 조기 종료 |
| `Scripts/CameraController.cs` | 플레이어 추적 카메라, 경계 클램프 |
| `Scripts/ShopController.cs` | 상점 패널 열기/닫기 |
| `Scripts/ShopSeedDisplay.cs` | 씨앗 구매 UI 및 로직 |
| `Scripts/ShopCropDisplay.cs` | 작물 판매 UI 및 로직 |
| `Scripts/InventoryController.cs` | 인벤토리 패널 열기/닫기, 디스플레이 갱신 |
| `Scripts/SeedDisplay.cs` | 인벤토리 씨앗 수량 표시, 씨앗 선택 |
| `Scripts/CropDisplay.cs` | 인벤토리 작물 수량 표시 |
| `Scripts/ShopActivator.cs` | 상점 트리거 존 진입 시 상점 열기 |
| `Scripts/DayEndController.cs` | Day End 씬 UI 및 진행 처리 |
| `Scripts/MainMenuController.cs` | 타이틀 화면 Play/Quit |
| `Scripts/MainMenuBGObjectSpawner.cs` | 타이틀 화면 낙하 오브젝트 스폰 |
| `Scripts/MainMenuFallingObject.cs` | 낙하 오브젝트 이동/회전/소멸 |
| `Packages/manifest.json` | Unity 패키지 의존성 |
| `ProjectSettings/EditorBuildSettings.asset` | 빌드 씬 순서 정의 |
