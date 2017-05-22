osu-BeatmapAutoLoader
=====================
osu!联机模式下地图下载自动导入

编译环境
=======
Visual Studio 2010 + .Net Framework 3.5

依赖
* EasyHook
* HtmlAgilityPack

使用说明
=======
将InjectLoader.exe置于安装目录下，启动即可

基本原理
=======
* Hook CreateProcessW或者ShellExecuteExW
* 获取BeatMapID
* 转去Bloodcat（镜像站）下载并导入

Acknowledgements
================
@UlyssesWu
