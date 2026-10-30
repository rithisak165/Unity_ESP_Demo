# Unity_ESP_Demo: Native iOS Visual Telemetry Application

A complete standalone Unity iOS application demonstrating 3D-to-2D visual telemetry (Bounding Boxes, 15-Joint Skeleton Tracing, Markers, and Distance Labels) with both an in-engine fallback renderer and a high-performance native Apple Metal overlay.

---

## 1. Project Specifications

* **Bundle Identifier**: `com.rithisak.unityespdemo`
* **Target OS**: iOS 16.0+
* **Architecture**: ARM64
* **Graphics API**: Apple Metal
* **Host Development Environment**: Windows 11
* **Build Engine**: GitHub Actions (`macos-14` Apple Silicon runner)

---

## 2. Directory Architecture

```text
Unity_ESP_Demo/
├── .github/
│   └── workflows/
│       └── build-ios.yml              # Automated macOS cloud compiler & IPA packager
├── ProjectSettings/
│   ├── ProjectVersion.txt             # Unity 2022.3.62f3 LTS specification
│   └── ProjectSettings.asset          # Configures iOS, ARM64, Metal, and com.rithisak.unityespdemo
├── Assets/
│   ├── Plugins/
│   │   └── iOS/
│   │       ├── NativeTelemetryBridge.h # Blittable C-ABI data structures & C function exports
│   │       └── NativeOverlayView.mm   # Metal-backed MTKView overlay attached to UIWindow
│   └── Scripts/
│       ├── Core/
│       │   ├── TargetEntity.cs        # 15-joint Humanoid skeleton auto-detection & registry
│       │   ├── DebugVisualizerConfig.cs # ScriptableObject visual styling & toggles
│       │   ├── DebugVisualizer.cs     # In-engine pure C# rendering engine
│       │   └── NativeTelemetryClient.cs # P/Invoke bridge streaming telemetry to the native dylib
│       ├── UI/
│       │   └── MobileESPController.cs # Touch toggle HUD adapted for iPhone safe-areas
│       └── Environment/
│           └── DemoEnvironmentBuilder.cs # Procedural 3D scene (arena, buildings, 3 dummies)
└── README.md
```

---

## 3. How to Test Locally on Windows (Unity Editor)

1. Open **Unity Hub** on your Windows PC.
2. Click **Add $\to$ Add project from disk** and select:  
   `C:\Users\bitth\OneDrive\Desktop\Software development\Unity_ESP_Demo`
3. Open the project in Unity Editor (2022.3 LTS).
4. In the Project window, create a new Scene or use the sample scene:
   * Create an empty GameObject named `_Environment` and attach `DemoEnvironmentBuilder`.
   * Create an empty GameObject named `_VisualizerManager` and attach:
     * `DebugVisualizer`
     * `MobileESPController`
     * `NativeTelemetryClient`
   * Right-click in `Assets/` $\to$ **Create $\to$ Telemetry $\to$ Visualizer Config**, and drag the created asset into the `Config` field of all three scripts.
5. Press **Play**:
   * The 3D ground, buildings, and three colored test dummies will generate automatically.
   * You will see the dynamic 2D bounding boxes, snaplines, 15-joint skeleton connections, and distance labels tracking each dummy in real time.
   * Click the **"ESP HUD"** button on screen or press **F1** to toggle features.

---

## 4. How to Build the iOS IPA on GitHub Actions (No Mac Required)

1. Initialize git and push this project to a GitHub repository:
   ```cmd
   cd "C:\Users\bitth\OneDrive\Desktop\Software development\Unity_ESP_Demo"
   git init
   git add .
   git commit -m "Initial commit of Unity_ESP_Demo with native Metal overlay"
   git branch -M main
   git remote add origin https://github.com/YOUR_USERNAME/Unity_ESP_Demo.git
   git push -u origin main
   ```
2. In your GitHub repository, go to **Settings $\to$ Secrets and variables $\to$ Actions** and add your GameCI Unity License secrets:
   * `UNITY_LICENSE`: (Your Unity `.alf` activation file content or license string)
   * `UNITY_EMAIL`: Your Unity account email
   * `UNITY_PASSWORD`: Your Unity account password
3. Go to the **Actions** tab $\to$ Select **Build Unity iOS IPA** $\to$ Click **Run workflow**.
4. The macOS runner will compile the Unity project, build the Xcode project, package the `.ipa`, and upload it under **Artifacts** as `Unity_ESP_Demo-Release-IPA`.

---

## 5. Free Sideloadly Installation on iPhone (Windows 11)

1. Download the generated `Unity_ESP_Demo.ipa` from your GitHub Actions run to your Windows PC.
2. Ensure you have **iTunes** and **iCloud** installed on Windows from Apple's direct website (do not use the Microsoft Store versions).
3. Connect your iPhone via USB cable to your ASUS ROG laptop.
4. Launch **Sideloadly**:
   * Drag `Unity_ESP_Demo.ipa` into Sideloadly.
   * Enter your free Apple ID email.
   * Click **Start**.
5. Once installed on your iPhone:
   * Go to **Settings $\to$ General $\to$ VPN & Device Management**.
   * Under *Developer App*, tap your Apple ID and select **Trust**.
   * (iOS 16+): Go to **Settings $\to$ Privacy & Security $\to$ Developer Mode** and toggle it **On** (restart when prompted).
6. Launch **Unity_ESP_Demo** on your iPhone to experience full-speed native Metal telemetry!

