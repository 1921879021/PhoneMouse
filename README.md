# Phone Mouse

> 鐢ㄦ墜鏈烘祻瑙堝櫒鍦ㄥ眬鍩熺綉鍐呮帶鍒?Windows 榧犳爣銆佽緭鍏ユ枃瀛楋紝骞舵敮鎸佸井淇″畨鍏ㄥ彂閫併€?
**褰撳墠婧愮爜鐗堟湰锛欰lpha 0.7.2.3**

Phone Mouse 鏄竴涓?Windows + 鎵嬫満娴忚鍣ㄧ殑灞€鍩熺綉閬ユ帶宸ュ叿銆傜數鑴戠杩愯 WPF 妗岄潰绋嬪簭骞跺惎鍔ㄦ湰鍦?ASP.NET Core 鏈嶅姟锛屾墜鏈洪€氳繃娴忚鍣ㄨ繛鎺ョ數鑴戯紝鏃犻渶瀹夎鎵嬫満 App銆?
## 鍔熻兘

- 鎵嬫満瑙︽帶鏉挎帶鍒?Windows 榧犳爣
  - 鍗曟寚绉诲姩
  - 宸﹂敭 / 鍙抽敭
  - 鍙屽嚮
  - 闀挎寜鎷栨嫿
  - 鍙屾寚婊氳疆
- 鎺у埗浣撻獙鍙皟
  - 榧犳爣鐏垫晱搴?  - 婊氳疆閫熷害
  - 鑷劧婊氬姩
  - 闀挎寜鏃堕棿
- 瀹夊叏閰嶅
  - 涓€娆℃€?Pair Token
  - 闀挎湡 Device Token
  - 宸查厤瀵硅澶囩鐞?  - 鍦ㄧ嚎 / 绂荤嚎鐘舵€?  - 鍗曡澶囨挙閿€ / 鍏ㄩ儴鎾ら攢
- 鏈湴浜岀淮鐮佽繛鎺?- 鎵嬫満璇煶 / 鏂囧瓧杈撳叆
  - 浣跨敤鎵嬫満绯荤粺閿洏鐨勯害鍏嬮杩涜璇煶杞枃瀛?  - 鍐欏叆 `VoiceNotes.txt`
  - 杈撳叆鍒扮數鑴戝綋鍓嶇獥鍙?- 寰俊瀹夊叏鍙戦€?  - 鑷姩妫€娴嬬數鑴戝綋鍓嶅墠鍙扮獥鍙ｆ槸鍚︿负寰俊
  - 闈炲井淇＄獥鍙ｆ椂闃绘鍙戦€?  - 鏀寔 `Enter` / `Ctrl + Enter`
- 璇煶 / 鏂囧瓧椤甸潰鍐呭祵灏忓瀷瑙︽帶鏉?
## 椤圭洰鏋舵瀯

```text
PhoneMouse
鈹溾攢 src
鈹? 鈹溾攢 PhoneMouse.Core
鈹? 鈹? 鈹溾攢 Input
鈹? 鈹? 鈹溾攢 Native
鈹? 鈹? 鈹斺攢 Windows
鈹? 鈹?鈹? 鈹溾攢 PhoneMouse.Server
鈹? 鈹? 鈹溾攢 Network
鈹? 鈹? 鈹溾攢 Notes
鈹? 鈹? 鈹溾攢 Security
鈹? 鈹? 鈹溾攢 Settings
鈹? 鈹? 鈹溾攢 Web
鈹? 鈹? 鈹斺攢 ServerHost.cs
鈹? 鈹?鈹? 鈹斺攢 PhoneMouse.Desktop
鈹?    鈹溾攢 Services
鈹?    鈹溾攢 MainWindow.xaml
鈹?    鈹斺攢 MainWindow.xaml.cs
鈹?鈹溾攢 PhoneMouse.sln
鈹斺攢 global.json
```

### PhoneMouse.Core

璐熻矗 Windows 鏈湴杈撳叆涓庣獥鍙ｈ兘鍔涳細

- `WindowsMouseController`
- `WindowsKeyboardController`
- `NativeInput`
- `NativeKeyboardInput`
- `WindowsForegroundWindowService`

搴曞眰浣跨敤 Win32 `SendInput`銆?
### PhoneMouse.Server

璐熻矗灞€鍩熺綉 HTTP / WebSocket 鏈嶅姟锛?
- Kestrel
- WebSocket 鎺у埗閫氶亾
- 璁惧璁よ瘉
- 閰嶅 Token
- 鎺у埗璁剧疆鍚屾
- TXT 鍐欏叆
- 鎵嬫満缃戦〉

榛樿鐩戝惉锛?
```text
http://0.0.0.0:9527
```

### PhoneMouse.Desktop

Windows WPF 鎺у埗涓績锛?
- 鏄剧ず灞€鍩熺綉璁块棶鍦板潃
- 鏄剧ず浜岀淮鐮?- 绠＄悊宸查厤瀵硅澶?- 璋冩暣榧犳爣涓庢粴杞弬鏁?- 璁剧疆寰俊鍙戦€佸揩鎹烽敭

## 寮€鍙戠幆澧?
鎺ㄨ崘锛?
- Windows 10 / 11 x64
- .NET SDK 10.0.400
- Visual Studio 2026 鎴栨敮鎸?.NET 10 鐨?Visual Studio
- Git

浠撳簱涓殑 `global.json` 浼氬浐瀹?.NET SDK 鐗堟湰銆?
鏌ョ湅 SDK锛?
```powershell
dotnet --list-sdks
```

## 缂栬瘧

```powershell
git clone https://github.com/1921879021/PhoneMouse.git

cd PhoneMouse

dotnet build PhoneMouse.sln --no-incremental
```

杩愯锛?
```powershell
dotnet ".\src\PhoneMouse.Desktop\bin\Debug\net10.0-windows\PhoneMouse.Desktop.dll"
```

鎴栬€咃細

```powershell
dotnet run --project src\PhoneMouse.Desktop
```

## 浣跨敤

1. 鍦?Windows 鐢佃剳杩愯 Phone Mouse銆?2. 纭繚鎵嬫満鍜岀數鑴戝浜庡悓涓€涓?Wi-Fi / 灞€鍩熺綉銆?3. 鐢佃剳绐楀彛浼氭樉绀虹被浼硷細

```text
http://192.168.1.102:9527
```

4. 鎵嬫満鎵弿鐢佃剳鏄剧ず鐨勪竴娆℃€т簩缁寸爜銆?5. 棣栨瀹屾垚瀹夊叏閰嶅鍚庯紝鍗冲彲鎺у埗榧犳爣銆?6. 鍚庣画宸叉巿鏉冭澶囧彲鐩存帴璁块棶鏅€氬眬鍩熺綉鍦板潃銆?
## 鎵嬫満璇煶杈撳叆

Phone Mouse 褰撳墠涓嶇洿鎺ヨ皟鐢ㄦ祻瑙堝櫒 Speech API銆?
浣跨敤娴佺▼锛?
```text
鎵嬫満鎵撳紑鈥滆闊?/ 鏂囧瓧鈥?鈫?鐐瑰嚮鈥滄墦寮€鎵嬫満閿洏 / 璇煶杈撳叆鈥?鈫?鐐瑰嚮鎵嬫満绯荤粺閿洏鑷甫楹﹀厠椋?鈫?绯荤粺瀹屾垚璇煶杞枃瀛?鈫?Phone Mouse 灏嗘枃瀛楀彂閫佺粰鐢佃剳
```

杩欐牱鍙互閬垮厤 HTTP 灞€鍩熺綉椤靛湪涓嶅悓 Android / iOS 娴忚鍣ㄤ腑鐨勯害鍏嬮鏉冮檺鍏煎闂銆?
## VoiceNotes.txt

鎵嬫満鐐瑰嚮锛?
```text
鍐欏叆 VoiceNotes.txt
```

榛樿鍐欏叆锛?
```text
Windows 鏂囨。鐩綍\PhoneMouse\VoiceNotes.txt
```

渚嬪锛?
```text
C:\Users\<User>\Documents\PhoneMouse\VoiceNotes.txt
```

鏂囦欢浣跨敤 UTF-8 BOM锛屾柟渚?Windows PowerShell 5.1 鍜岃浜嬫湰姝ｇ‘璇嗗埆涓枃銆?
## 寰俊瀹夊叏鍙戦€?
鎵嬫満绔細鎸佺画妫€鏌ョ數鑴戝綋鍓嶅墠鍙扮獥鍙ｃ€?
妫€娴嬪埌寰俊锛?
```text
鉁?褰撳墠鐢佃剳鍓嶅彴绐楀彛锛氬井淇?```

姝ゆ椂鍏佽锛?
```text
寰俊瀹夊叏鍙戦€?```

濡傛灉鐢佃剳褰撳墠涓嶆槸寰俊锛屽垯鍓嶇鎸夐挳閿佸畾锛屽苟涓旀湇鍔＄涔熶細鍐嶆妫€鏌ワ紝闃叉璇彂閫併€?
鏀寔锛?
```text
Enter
Ctrl + Enter
```

鍙湪鐢佃剳绔?Phone Mouse 璁剧疆涓垏鎹€?
## 瀹夊叏璇存槑

褰撳墠鐗堟湰浣跨敤锛?
```text
HTTP
ws://
```

杩樻病鏈?HTTPS / WSS銆?
鍥犳鍙缓璁湪浠ヤ笅鐜浣跨敤锛?
- 瀹跺涵 Wi-Fi
- 鍙俊浠诲叕鍙稿唴缃?- 涓汉鐑偣

涓嶈鍦ㄤ笉鍙俊鍏叡 Wi-Fi 涓婁娇鐢ㄣ€?
鐢佃剳绔繚瀛樼殑鏄?Device Token 鐨?SHA-256 鍝堝笇锛岃€屼笉鏄師濮?Token銆?
璁惧鏁版嵁浣嶄簬锛?
```text
%LOCALAPPDATA%\PhoneMouse\
```

涓昏鍖呮嫭锛?
```text
trusted-devices.json
control-settings.json
```

## Windows 闃茬伀澧?
Phone Mouse 榛樿鐩戝惉 TCP `9527`銆?
绗竴娆¤繍琛屾椂 Windows 鍙兘璇㈤棶鏄惁鍏佽缃戠粶璁块棶銆?
寤鸿鍙厑璁革細

```text
涓撶敤缃戠粶
```

涓嶅缓璁叧闂?Windows Defender 鎴栨暣涓槻鐏銆?
## Portable 鐗堟湰

椤圭洰宸茬粡棰勭暀 Portable 鍙戝竷娴佺▼銆?
鐩爣鍙戝竷褰㈠紡锛?
```text
PhoneMouse.exe
```

娴嬭瘯鐢ㄦ埛鏃犻渶瀹夎 Visual Studio 鎴?.NET SDK銆?
鍙戝竷鐩稿叧璇存槑瑙侊細

```text
docs/BUILD-RELEASE.md
```

## 浜屾寮€鍙?
璇峰厛闃呰锛?
- [寮€鍙戞寚鍗梋(docs/DEVELOPMENT.md)
- [WebSocket 鍗忚](docs/PROTOCOL.md)
- [鏋勫缓涓庡彂甯僝(docs/BUILD-RELEASE.md)
- [瀹夊叏璇存槑](SECURITY.md)
- [璐＄尞鎸囧崡](CONTRIBUTING.md)

## 宸茬煡闄愬埗

- 褰撳墠鍙敮鎸?Windows銆?- 褰撳墠涓哄眬鍩熺綉妯″紡銆?- HTTP / WebSocket 灏氭湭鍔犲瘑銆?- 寰俊妫€娴嬩緷璧?Windows 鍓嶅彴杩涚▼璇嗗埆銆?- 涓嶅悓鐗堟湰寰俊鐨勮繘绋嬪悕鍜屽揩鎹烽敭琛屼负鍙兘瀛樺湪宸紓銆?- 褰撳墠璇煶璇嗗埆渚濊禆鎵嬫満绯荤粺閿洏鎻愪緵鐨勮闊宠緭鍏ヨ兘鍔涖€?- 浠嶅浜?Alpha 闃舵锛屼笉寤鸿鐢ㄤ簬鐢熶骇鐜鎴栧叧閿笟鍔°€?
## Roadmap

璁″垝涓殑鏂瑰悜锛?
- Portable 鍗曟枃浠跺彂甯?- 姝ｅ紡 Windows 瀹夎鍖?- HTTPS / WSS
- 鏇村畬鍠勭殑杈撳叆娉曚笌蹇嵎閿敮鎸?- 鏂囦欢浼犺緭
- 濯掍綋鎺у埗
- 婕旂ず鏂囩鎺у埗
- 鑷畾涔夊揩鎹烽敭
- 鏇村畬鍠勭殑璁惧鏉冮檺妯″瀷

## License

褰撳墠浠撳簱灏氭湭鎸囧畾寮€婧愯鍙瘉銆?
鍦ㄦ寮忛€夋嫨璁稿彲璇佷箣鍓嶏紝璇蜂笉瑕侀粯璁よ椤圭洰宸茬粡鍏佽浠绘剰鍟嗕笟浣跨敤銆佸啀鍙戝竷鎴栭棴婧愰泦鎴愩€?
濡傛灉璁″垝寮€鏀句簩娆″紑鍙戯紝寤鸿鍚庣画鏄庣‘閫夋嫨 MIT / Apache-2.0 / GPL 绛夎鍙瘉涔嬩竴銆?
