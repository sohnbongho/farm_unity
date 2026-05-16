# Unity 6.4 업그레이드 구현 계획

## 요약

Unity 6 (6000.0.21f1) → Unity 6.4 (6000.4.5f1) 마이너 버전 업그레이드 계획이다.
동일 LTS 라인 내 버전 상승이므로 코드 API 브레이킹은 거의 없으며, 주요 작업은
Unity Hub 에디터 설치 → 프로젝트 재임포트 → 패키지 버전 정합성 확인 → 씬별 기능 검증 순서로 진행한다.

---

## 현황 분석

### 확인된 파일

| 파일 | 경로 |
|---|---|
| 버전 확인 | `Farming Game/ProjectSettings/ProjectVersion.txt` |
| 패키지 의존성 | `Farming Game/Packages/manifest.json` |
| 패키지 잠금 | `Farming Game/Packages/packages-lock.json` |
| 씬 빌드 설정 | `Farming Game/ProjectSettings/EditorBuildSettings.asset` |
| 입력 액션 | `Farming Game/Assets/InputSystem_Actions.inputactions` |

### 현재 환경

- **현재 Unity**: 6000.0.21f1 (Unity 6.0)
- **목표 Unity**: 6000.4.5f1 (Unity 6.4)
- **렌더 파이프라인**: URP 17.0.3 (com.unity.render-pipelines.universal)
- **Input System**: 1.11.0 (com.unity.inputsystem)
- **2D Feature**: 2.0.1 (com.unity.feature.2d) — animation, aseprite, psdimporter, spriteshape, tilemap.extras 포함
- **빌드 씬 수**: 5개 (Main Menu / Main / Indoors / Indoors-Copy / Day End)

### 스크립트 현황 (코드 변경 필요성 선검토)

현재 코드가 사용 중인 API를 확인한 결과, 이미 Unity 6 신 API를 사용하고 있어
Unity 6.0 → 6.4 전환에서 코드 수정이 요구될 가능성이 낮다.

| 스크립트 | 사용 API | 상태 |
|---|---|---|
| `PlayerController.cs` | `Rigidbody2D.linearVelocity`, `Keyboard.current.*`, `Mouse.current.position`, `InputActionReference` | 이미 Unity 6 API |
| `GridInfo.cs` | `Keyboard.current.yKey` (디버그 코드) | 변경 없음 |
| `UIController.cs` | `Keyboard.current.*` 직접 폴링 | 변경 없음 |
| `DayEndController.cs` | `Keyboard.current.anyKey`, `Mouse.current.leftButton` | 변경 없음 |
| `AreaSwitcher.cs` | `collision.tag` (deprecated 경고 가능성?) | 확인 필요 |
| `CameraController.cs` | `Camera.main`, `PlayerController.instance` | 변경 없음 |
| `TimeController.cs` | `SceneManager.LoadScene(string)` | 변경 없음 |

> `collision.tag` 대신 `collision.CompareTag()` 권장이 Unity 6.x 에서도 계속 경고될 수 있으나 컴파일 에러는 아님.

---

## 업그레이드 단계

### 0단계: 사전 준비 — git 백업 브랜치 생성

- **작업 위치**: git 저장소 루트 `/mnt/d/01-git/farm_unity`
- **작업 내용**:
  1. 현재 작업 상태를 `main` 브랜치에 커밋 (미커밋 변경사항이 있으면 stash 또는 commit)
  2. 롤백용 브랜치 생성: `git checkout -b backup/before-unity64-upgrade`
  3. `main` 브랜치로 복귀: `git checkout main`
- **백업 대상**: `ProjectSettings/`, `Packages/`, `Assets/` — git으로 관리됨
- **백업 불필요 폴더**: `Library/`, `Temp/`, `obj/` — Unity가 자동 재생성
- **완료 기준**: `git branch` 실행 시 `backup/before-unity64-upgrade` 브랜치가 존재함

---

### 1단계: Unity Hub에서 6000.4.5f1 에디터 설치

- **작업 위치**: Unity Hub 애플리케이션
- **작업 내용**:
  1. Unity Hub → Installs → Install Editor
  2. Unity 6 (6000.4.5f1) 선택 (Archive에서 검색 시 "6000.4.5f1" 입력)
  3. 추가 모듈 선택:
     - 기존 6000.0.21f1 에 설치된 모듈과 동일하게 선택 (Windows Build Support, WebGL 등)
     - Microsoft Visual Studio 통합 체크 (Visual Studio 2022 대응)
  4. 설치 완료 대기 (약 3~5 GB 다운로드)
- **완료 기준**: Unity Hub Installs 탭에서 Unity 6000.4.5f1 상태가 "설치됨"으로 표시됨

---

### 2단계: 신 에디터로 프로젝트 열기 (자동 마이그레이션)

- **작업 위치**: Unity Hub → Projects
- **작업 내용**:
  1. Projects 탭에서 `farm_unity` 프로젝트 우클릭 → Open with → 6000.4.5f1 선택
  2. "Editor Version Mismatch" 경고창 → "Change Version" (버전 변경) 클릭
  3. Unity 에디터가 열리면서 `Library/` 폴더 전체 재구축 시작 (5~30분 소요)
  4. 임포트 완료 후 Console 창에 에러/경고 목록 확인
- **예상 자동 처리 항목**:
  - `ProjectVersion.txt`가 `6000.4.5f1`로 자동 갱신됨
  - URP Renderer Asset 마이그레이션 다이얼로그 표시 가능 → "Yes" 클릭
- **완료 기준**: Unity 에디터 타이틀바에 "6000.4.5f1" 표시, Console에 0 컴파일 에러

---

### 3단계: 패키지 의존성 버전 정합성 확인 및 조정

- **대상 파일**: `Farming Game/Packages/manifest.json`
- **작업 내용**: Package Manager (Window → Package Manager) 에서 아래 항목들의 업데이트 여부 확인

  #### 필수 확인 항목 (URP — 엔진 버전 추종)

  | 패키지 | 현재 버전 | 6.4 권장 버전 | 우선순위 |
  |---|---|---|---|
  | `com.unity.render-pipelines.universal` | 17.0.3 | 17.4.x | 높음 |
  | `com.unity.render-pipelines.core` | 17.0.3 | 17.4.x | 높음 (자동 동반 상승) |
  | `com.unity.shadergraph` | 17.0.3 | 17.4.x | 높음 (자동 동반 상승) |

  > URP는 Unity 엔진 버전과 연동되어 Package Manager에서 "Update Available" 표시 가능.
  > 에디터가 자동으로 권장 버전을 제안하면 수락한다.

  #### 선택 확인 항목 (독립 패키지 — 직접 업데이트)

  | 패키지 | 현재 버전 | 업데이트 이유 |
  |---|---|---|
  | `com.unity.inputsystem` | 1.11.0 | 버그 픽스 포함 가능 |
  | `com.unity.feature.2d` | 2.0.1 | aseprite, animation 종속 패키지 신버전 포함 가능 |
  | `com.unity.collab-proxy` | 2.8.2 | 선택 사항 |
  | `com.unity.ide.visualstudio` | 2.0.22 | 선택 사항 |

- **작업 방법**:
  1. Package Manager → In Project 목록에서 "Update" 버튼 있는 항목 확인
  2. URP 관련 패키지부터 업데이트, 컴파일 확인
  3. Input System 업데이트, 컴파일 확인
  4. 나머지 패키지 순서대로 업데이트
- **완료 기준**: `packages-lock.json` 변경 이후 Console 에러 0건

---

### 4단계: 컴파일 에러 및 경고 해소

- **대상 파일**: `Farming Game/Assets/Scripts/` 내 전체 `.cs` 파일 (23개)
- **예상 이슈 및 확인 방법**:

  | 증상 | 발생 가능 파일 | 대응 방향 |
  |---|---|---|
  | `collision.tag` 사용 obsolete 경고 | `AreaSwitcher.cs:31` | 경고 수준이면 무시 가능, 에러이면 `CompareTag()` 로 변경 명시 |
  | Input System API 변경 경고 | `PlayerController.cs`, `UIController.cs`, `GridInfo.cs`, `DayEndController.cs` | 실제 에러인지 경고인지 구분하여 대응 |
  | URP Shader 컴파일 에러 | 셰이더/머티리얼 파일 | URP 패키지 업데이트 후 자동 해소 대기 |
  | `linearVelocity` 관련 | `PlayerController.cs:57,67,77,87,92,94,98` | 이미 신 API 사용 중 — 에러 없음 예상 |

- **완료 기준**: Console 창 Error 탭 0건 (경고는 허용)

---

### 5단계: 씬별 동작 스모크 테스트

#### 테스트 전 필수 확인

- `Farming Game/ProjectSettings/EditorBuildSettings.asset`의 씬 순서가 다음과 동일한지 확인:
  - 인덱스 0: `Assets/Scenes/Main Menu.unity`
  - 인덱스 1: `Assets/Scenes/Main.unity`
  - 인덱스 2: `Assets/Scenes/Indoors.unity`
  - 인덱스 3: `Assets/Scenes/Indoors-Copy.unity`
  - 인덱스 4: `Assets/Scenes/Day End.unity`

#### 씬별 체크리스트

**[Main Menu 씬]**
- [ ] 씬 Play 시 타이틀 BGM 재생됨 (`AudioManager.PlayTitle()`)
- [ ] "Play" 버튼 클릭 시 Main 씬으로 전환됨
- [ ] "Quit" 버튼 클릭 시 에디터에서 Play 모드 종료됨

**[Main 씬 — 야외 농장]**
- [ ] 플레이어 스폰 및 WASD/화살표 이동 정상
- [ ] 마우스 위치 추종하는 도구 인디케이터(toolIndicator) 표시됨
- [ ] Tab 키 누를 때 도구 순환 (plough → wateringCan → seeds → basket → plough)
- [ ] 1/2/3/4 키로 도구 직접 전환 및 UI 툴바 아이콘 반응
- [ ] 마우스 좌클릭으로 현재 도구 사용:
  - plough: 흙 갈기 애니메이션 + 타일 변경
  - wateringCan: 물주기 애니메이션 + 타일 변경
  - seeds: 씨앗 심기 (인벤토리에 씨앗이 있는 경우)
  - basket: 작물 수확
- [ ] I 키로 인벤토리 열기/닫기 (열린 동안 플레이어 이동 불가)
- [ ] ESC 또는 P 키로 일시정지 화면 표시/해제
- [ ] 시간 UI 텍스트(AM/PM)가 시간 경과에 따라 변경됨
- [ ] 실내(Indoors) 진입 트리거에 닿으면 씬 전환됨

**[Indoors / Indoors-Copy 씬]**
- [ ] Indoors 씬 진입 시 플레이어가 AreaSwitcher의 `startPoint` 위치에 스폰됨 (PlayerPrefs "Transition" 키 검증)
- [ ] 실내에서 야외로 나가는 트리거 작동 → Main 씬 복귀

**[Day End 씬]**
- [ ] 시간이 dayEnd 값에 도달하면 자동으로 Day End 씬으로 전환됨
- [ ] 씬 진입 시 "Day X" 텍스트가 올바른 일수로 표시됨
- [ ] 씬 진입 시 BGM이 일시정지되고 Day End SFX(인덱스 1) 재생됨
- [ ] 아무 키 또는 마우스 좌클릭으로 Main 씬(Wake Up)으로 복귀됨
- [ ] 복귀 시 플레이어 위치가 침대 근처에 스폰됨 (PlayerPrefs "Transition" = "Wake Up")
- [ ] BGM 재개됨

**[전체 씬 공통]**
- [ ] URP 2D 라이팅 / Light 컴포넌트가 이전과 동일하게 렌더링됨
- [ ] TMP 텍스트가 정상 폰트로 표시됨 (Pixelnauts SDF)
- [ ] 타일맵 스프라이트가 깨지지 않음

---

### 6단계: 빌드 검증 (선택 — 실제 배포 전 필수)

- **작업 내용**: File → Build Settings → Build (또는 Build And Run)
- **확인 사항**:
  - `GridInfo.Update()`의 Y 키 디버그 코드는 이 단계에서도 빌드에 포함됨 (별도 계획에서 제거 권장)
  - 빌드 후 실행 파일에서 씬 전환 정상 여부 확인
- **완료 기준**: 빌드 성공 및 Main Menu 씬에서 Play 버튼 클릭 시 게임 진행 가능

---

### 7단계: 변경사항 커밋

- **대상 파일**:
  - `Farming Game/ProjectSettings/ProjectVersion.txt` (버전 변경)
  - `Farming Game/Packages/manifest.json` (패키지 버전 변경)
  - `Farming Game/Packages/packages-lock.json` (패키지 잠금 파일 갱신)
  - `Farming Game/ProjectSettings/` 내 기타 변경된 `.asset` 파일
- **작업 내용**: `git add` 후 `git commit -m "upgrade: Unity 6000.0.21f1 → 6000.4.5f1"`
- **완료 기준**: `git log`에서 커밋 확인

---

## 리스크

- [ ] **URP Renderer Asset 마이그레이션 다이얼로그**: 에디터가 열릴 때 2D Renderer Data 또는 URP Asset 마이그레이션을 요청하는 경우 "Yes" 선택 후 씬 전체의 라이팅/렌더링 결과를 시각 비교 필요
- [ ] **Aseprite Importer 버전 상승**: `com.unity.2d.aseprite` 신버전에서 스프라이트 시트 재임포트 결과가 달라질 가능성 있음 → 모든 캐릭터/오브젝트 스프라이트 에니메이션을 Play 모드에서 육안 확인
- [ ] **2D Animation 패키지 업데이트**: `com.unity.2d.animation` 신버전에서 bone/skin 데이터 마이그레이션 알림이 표시될 수 있음 (현재 프로젝트가 단순 Sprite 스왑 애니메이션이면 무관)
- [ ] **Library 재구축 시간**: 첫 임포트 시 모든 에셋을 재처리하므로 5~30분 소요 예상 — 진행 중 에디터 강제 종료 금지
- [ ] **입력 처리 회귀**: Input System 패키지 업데이트 시 `Keyboard.current`가 특정 프레임에서 null을 반환하는 경우가 드물게 보고됨 → 씬 전환 직후 키 입력이 정상인지 확인
- [ ] **`collision.tag` 사용**: `AreaSwitcher.cs:31`에서 `collision.tag == "Player"` 대신 `collision.CompareTag("Player")` 사용이 권장됨 — 에러가 아닌 경고이나 Unity 미래 버전에서 제거될 수 있음

---

## 예상 소요

- **단계 수**: 7단계
- **핵심 파일**: 3개 (`ProjectVersion.txt`, `manifest.json`, `packages-lock.json`)
- **코드 수정 가능성이 있는 파일**: 1개 (`AreaSwitcher.cs` — `collision.tag` 경고 수준에 따라)
- **에디터 설치 + Library 재구축 시간**: 약 30분~1시간 (네트워크/PC 성능에 따라)
- **테스트 시간**: 약 30분 (씬별 체크리스트 수동 검증)

---

## 업그레이드 이후 추가 권장 사항 (이번 계획 범위 외)

- `GridInfo.cs` Update() 의 Y 키 디버그 코드 제거 (`#if UNITY_EDITOR` 감싸거나 삭제)
- `AreaSwitcher.cs:31` `collision.tag` → `collision.CompareTag("Player")` 변경
- `collision.tag` 와 동일한 패턴이 있는 다른 스크립트 전수 검색 후 일괄 변경

