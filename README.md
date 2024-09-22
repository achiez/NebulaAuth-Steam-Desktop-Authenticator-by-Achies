# NebulaAuth

   * [Русский](README-RU.md)
   * [Українська](README-UA.md)

## Description

<h3 align="center" style="margin-bottom:0">
  <a href="https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/releases/latest">Download latest version</a>
</h3>

<h3 align="center">NebulaAuth is an application for emulating actions from the Steam Mobile App. Which replaces your smartphone when operating on Steam. </h3>



## Main advantages

- **Localization in three languages**: English, Russian and Ukrainian.
- **Full functionality of Steam Desktop Authenticator** reimagining [old app](https://github.com/Jessecar96/SteamDesktopAuthenticator)
- **Proxy support** in all account work processes.
- **Mafile grouping** for improved management.
- **Automatic confirmations of trades/market actions** to save time.
- **Bulk import of .mafiles** via Drag'n'Drop or CTRL+V for convenience.
- **Design customization** to personalize the interface.
- **Ability to confirm account login without entering a code** for easier access.
- **Auto-update** program to use the latest features.
- **Automatic relogin in case of problems with the session** for continuous operation.
- **Intuitive interface** with tips and conveniences
- **Continious support** of application code and other features.


## Installation

1. If the application does not start, you need to install [.net desktop runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.1-windows-x64-installer)
2. [Download the program from the releases of this repository on Github](https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/releases/latest)
     * *For the safety of your data, download the application only from here*
4. Unpack the .zip file to any folder
5. Run the file **NebulaAuth.exe**

## Usage
   
![gh-main-window-eng](https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/assets/106531132/15c0e870-1766-43a5-9e8c-2f34d5436beb)

1. Control panel.
   - file management and settings
   - account management (login, linking, unlinking)
   - grouping
   - proxy selection
   - an indicator with a hint about the proxy used (lit either yellow or red, when hovered it will display additional information)
   - timers for automatic confirmation of trade offers/sale offers on the marketplace
   - confirmation timer interval (in seconds)
2. List of your accounts
3. Login confirmation code (click to copy)
4. Main confirmation window
5. Search by login or SteamID (7xxxxxxxxxxxxx)
6. Confirm login on another device
7. Hyperlink to the official application page with attribution

## Settings
![gh-settings-eng](https://github.com/achiez/NebulaAuth-Steam-Desktop-Authenticator-by-Achies/assets/106531132/cd704495-d2df-45a7-a73e-40c19410eb72)

1. Background mode. Use it if you want to disable default or set custom background of application (put file 'Background.png' to your application folder)
2. Localization language
3. Disable timers when switching between accounts
4. Hide to tray on minimize
5. Indicator with color. Small ellipse on your task-bar icon with custom color. Useful when using multiple windows
6. Custom background color of application
7. Current encryption password. If set you can save encrypted passwords to mafile to help re-login on session troubles. (Not recommended)
8. Legacy mafile mode. Mafile compability mode for another applications (SDA and etc). If checked application will save mafiles with old standart format (Default: checked)
9. Allow auto-update without confirmation
   
  

## [License](/LICENSE.md)

Commercial use prohibited. When redistributing modified code, you must indicate the original authorship.
