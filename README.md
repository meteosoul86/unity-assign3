# unity-assign3

## Gesticulator

---

### 원본
[https://github.com/Svito-zar/gesticulator](https://github.com/Svito-zar/gesticulator)

### 원본에서 수정한 의존성 버전
#### Assets/gesticulator/gesticulator/requirements.txt
- dataclasses 삭제
- pysptk 0.1.21로 변경
- rsa 버전 4.0으로 변경
- torch 버전 1.8.0으로 변경
- typing 삭제
- urllib3 버전 1.25.11로 변경

## 실행 방법

---

### ffmpeg 설치
- MAC
  ```sh
  $> brew install ffmpeg
  ```
- Windows
  - [설치 방법 링크](https://www.lainyzine.com/ko/article/how-to-install-ffmpeg-on-windows-10/)

### Anaconda 파이썬 가상환경 실행
```sh
$> conda create -n {가상환경 이름} python=3.8
$> conda activate {가상환경 이름}
```

### Gesticulator 의존성 설치
  ```sh
  (가상환경 이름)$> cd {유니티 프로젝트 경로}
  (가상환경 이름)$> python Assets/gesticulator/install_script.py
  ```

### Azure Speech API 생성
- [Azure](https://azure.microsoft.com/ko-kr 가입 후 로그인
- Cognitive Services 검색 후 클릭
- 음성 > 만들기
- 만들어진 리소스를 클릭하여 키(키1) 및 위치/지역 값 확인

### 변수 입력(Variables Game Obejct)
- Azure Subscription Key
  - Azure에서 확인한 키1 값 입력
- Azure Service Region
  - Azure에서 확인한 위치/지역 값 입력
- Unity Project Path
  - 역슬래시 2개로 구분 예) ```C:\\Users\\foo-user\\UnityProjects\\bar-project```
- Py Venv Path
  - 역슬래시 2개로 구분 예) ```C:\\Users\\foo-user\\anaconda3\\envs\\py38```
- Py Dll File Name
  - Py Venv Path안에 있는 파이썬 DLL 파일 이름 예) ```python38.dll```

### 발화 입력
- 프로젝트 Play
- MIC ON 버튼을 누르고 녹음 시작 후 녹음이 끝나면 MIC OFF 버튼 누름
- 녹음된 발화에 대해 제스처 자동 재생

