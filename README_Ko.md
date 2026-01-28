
<!-- =========================================================
MemoApp.exe - README_Ko.md (Test v1)
GitHub에서 바로 보기 좋게: HTML + Markdown 혼합 템플릿
========================================================= -->

<div align="center">

# MemoApp (Test v1)

로컬 저장 기반 Windows 데스크톱 메모 앱입니다.  
좌측 목록/필터 + 우측 편집의 2-Pane UX로 빠르게 정리하고 바로 저장합니다.

<!-- 배지(원하면 수정/삭제) -->
<p>
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows-0078D6">
  <img alt="Status" src="https://img.shields.io/badge/status-Test%20v1-orange">
  <img alt="License" src="https://img.shields.io/badge/license-TBD-lightgrey">
</p>

<!-- 빠른 이동 링크 -->
<p>
  <a href="#-미리보기ui">미리보기</a> ·
  <a href="#-주요-기능">기능</a> ·
  <a href="#-단축키지원">단축키</a> ·
  <a href="#-설치실행">설치</a> ·
  <a href="#-로드맵">로드맵</a>
</p>

</div>

---

## 📸 미리보기(UI)

<!-- 스크린샷 넣기(레포에 images 폴더 만든 후 경로 맞추면 가장 안정적) -->
<div align="center">
  <img
    src="./images/screenshot_main.png"
    alt="MemoApp 메인 화면"
    width="980"
  />
  <br/>
  <sub>좌측: 검색/필터/메모 리스트 · 우측: 메모 편집/저장/내보내기/삭제</sub>
</div>

> 스크린샷 경로 예시: `images/screenshot_main.png`  
> 현재 레포에 이미지가 없으면 위 이미지는 깨집니다(정상) → `images/` 폴더에 추가하세요.

---

## ✅ 주요 기능

<table>
  <thead>
    <tr>
      <th width="22%">영역</th>
      <th>설명</th>
      <th width="22%">UX 포인트</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td><b>메모 작성/편집</b></td>
      <td>
        우측 편집 영역에서 제목/본문을 입력하고 바로 저장합니다.
        <br/>카테고리, 태그, 우선순위, 즐겨찾기(★)를 함께 관리합니다.
      </td>
      <td>작성 흐름 끊김 최소화</td>
    </tr>
    <tr>
      <td><b>검색/필터</b></td>
      <td>
        좌측 검색 입력 + 카테고리/우선순위 필터로 목록을 즉시 좁힙니다.
      </td>
      <td>재탐색 비용 절감</td>
    </tr>
    <tr>
      <td><b>즐겨찾기</b></td>
      <td>
        자주 쓰는 메모를 ★로 고정해 빠르게 재접근합니다.
      </td>
      <td>자주 쓰는 항목 최단 접근</td>
    </tr>
    <tr>
      <td><b>저장 위치 노출</b></td>
      <td>
        상단에 “저장 위치 경로”를 표시해 데이터 위치를 명확히 합니다.
      </td>
      <td>백업/이관 불안 제거</td>
    </tr>
    <tr>
      <td><b>내보내기/삭제</b></td>
      <td>
        내보내기로 외부 파일 추출, 삭제로 선택 메모를 제거합니다.
      </td>
      <td>정리 루틴 단순화</td>
    </tr>
  </tbody>
</table>

---

## 🧭 화면 구성(스크린샷 기준)

<details>
  <summary><b>좌측(탐색 영역)</b> — 검색/필터/리스트</summary>

- **검색**: 문자열로 메모 제목/본문(또는 일부) 탐색  
- **카테고리 필터**: 전체/기본/할 일/아이디어 등  
- **우선순위 필터**: 전체/일반/중요 등  
- **메모 카드 리스트**: 제목/스니펫/카테고리 배지/시간 표시
</details>

<details>
  <summary><b>우측(편집 영역)</b> — 메모 내용/메타/액션</summary>

- **제목 입력**
- **카테고리 선택**, **태그 입력**, **우선순위 선택**
- **즐겨찾기(★)** 토글
- 상단 액션: **저장 / 내보내기 / 삭제**
</details>

---

## ⌨️ 단축키(지원)

단축키 기능은 지원됩니다.  
다만 Test v1에서 “정확한 키 조합” 문서화가 누락된 상태라 아래는 플레이스홀더입니다.

<ul>
  <li><b>저장</b>: <code>Ctrl + ?</code></li>
  <li><b>새 메모</b>: <code>Ctrl + ?</code></li>
  <li><b>검색 포커스</b>: <code>Ctrl + ?</code></li>
  <li><b>즐겨찾기 토글</b>: <code>Ctrl + ?</code></li>
  <li><b>삭제</b>: <code>Del</code> 또는 <code>Ctrl + ?</code></li>
</ul>

> TODO: 실제 키 확인 후 위 `?`만 치환하면 README가 즉시 “확정 문서”가 됩니다.

---

## 📦 설치/실행

### 실행(배포본)
1. 릴리즈에서 실행 파일/압축 파일 다운로드  
2. 압축 해제  
3. 실행 파일 실행  

### 개발(로컬 빌드)
1. 레포 클론  
2. Visual Studio에서 솔루션/프로젝트 열기  
3. `Debug` 또는 `Release`로 빌드/실행  

---

## 💾 데이터/저장 정책(권장 표기)

> UI에 저장 경로가 보이므로, 아래를 README에 명시하면 운영 안정성이 올라갑니다.

- 저장 포맷: `JSON / TXT / SQLite` 중 실제 구현값 기재
- 파일명 규칙: 예) `yyyyMMdd_HHmmss_title.json`
- 삭제 정책: 소프트 삭제(휴지통) / 하드 삭제 여부
- 내보내기 포맷: 예) `.txt`, `.md`, `.json` 등

---

## 🗺️ 로드맵

- [ ] 단축키 목록 확정 및 문서화
- [ ] 태그 자동완성/태그 관리
- [ ] 정렬 옵션(최신순/우선순위/즐겨찾기 우선)
- [ ] 휴지통(복구 가능한 삭제)
- [ ] 내보내기 포맷 추가(Markdown/PDF 등)
- [ ] 백업/동기화 옵션(선택 기능)

---

## 🧾 라이선스
- TBD ( ? )

---

## 🧩 기여/이슈
- 버그 이슈: 재현 절차 + 스크린샷 + 로그(가능하면) 첨부 권장
- PR: 기능 단위로 작게, UI 변경 시 스크린샷 첨부 권장
