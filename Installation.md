## 1. Install .NET SDK
- [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
- Run the following command to install MAUI:
    ```bash
    dotnet workload install maui
    ```
- Verify installation:
    ```bash
    dotnet workload list
    ```

## 2. Install Android Studio
- [developer.android.com/studio](https://developer.android.com/studio) and follow the installation instructions.
- Goto device manager and download a device

## 3. Set Up Android SDK Environment Variables
- Add `ANDROID_HOME` System Variable:
    - **Variable value**: `C:\Users\<YourUserName>\AppData\Local\Android\Sdk`
- Add SDK Tools to your system `PATH`:
    - `C:\Users\<YourUserName>\AppData\Local\Android\Sdk\emulator`
    - `C:\Users\<YourUserName>\AppData\Local\Android\Sdk\platform-tools`
    - `C:\Users\<YourUserName>\AppData\Local\Android\Sdk\build-tools\35.0.0`

## 4. Verify Android SDK Installation
- Run the following commands:
    ```bash
    adb --version
    adb devices
    ```

## 5. Create a New MAUI Project
- Create a new project by running:
    ```bash
    dotnet new maui -n HelloWorldMaui
    ```

## 6. Run the Android App
- Start the Android Emulator:
    ```bash
    emulator -avd <Your_AVD_Name>
    ```
- Build and run the Android app:
    ```bash
    dotnet build -t:Run -f net7.0-android /p:_DeviceName=emulator-5554
    ```
    *(Replace `emulator-5554` with your actual emulator name.)*

## 7. Run the Windows App
- Build and run the Windows app:
    ```bash
    dotnet build -t:Run -f net7.0-windows
    ```
