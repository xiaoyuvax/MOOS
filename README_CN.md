[![Language switcher](https://img.shields.io/badge/Language%20%2F%20%E8%AF%AD%E8%A8%80-English%20%2F%20%E8%8B%B1%E8%AF%AD-blue)](https://github.com/xiaoyuvax/MOOS/blob/master/README.md)

<p align="center">
    <img width=300 src="MOOS-Logo.svg"/>
</p>

<p align="center">
    <a href="https://github.com/nifanfa/moos/issues"><img alt="GitHub issues" src="https://img.shields.io/github/issues/nifanfa/moos"></a>
    <a href="https://github.com/nifanfa/moos/network"><img alt="GitHub forks" src="https://img.shields.io/github/forks/nifanfa/moos"></a>
    <a href="https://github.com/nifanfa/moos/stargazers"><img alt="GitHub stars" src="https://img.shields.io/github/stars/nifanfa/moos"></a>
    <a href="https://github.com/nifanfa/moos"><img alt="GitHub license" src="https://img.shields.io/github/license/nifanfa/moos"></a>
    <a href="https://github.com/nifanfa/MOOS/blob/main/LICENSE"><img alt="GitHub license" src="https://img.shields.io/github/license/nifanfa/moos"></a>
    <a href="https://discord.gg/uJstXbx8Pt"><img src="https://discordapp.com/api/guilds/987075686256762890/widget.png?style=shield" alt="Discord Shield"/></a>
</p>

# MOOS

MOOS ( **M**y **O**wn **O**perating **S**ystem )是一个使用.NET Native AOT技术编译的C# 64位操作系统。  
作者QQ: 3244735564  

## 编译
这个分支修改了相关代码和配置以便能够用BFlat，一个可以替代的C#本机代码编译器来编译MOOS。编译方法详情见：[如何用BFlat编译MOOS](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bflat.cn.md). 当然你仍然可以继续用VS编译MOOS，但MOOS目前只能用一个古老的ILComplier来编译，有点不爽。

关于编译MOOS的信息，请阅读 [编译维基页面](https://github.com/nifanfa/MOOS/wiki/How-do-you-build-or-compile-MOOS%3F)。

### 编译要求
编译器/IDE :
- Visual studio 2022 (MSBuild + ILCompiler) - https://visualstudio.microsoft.com/  
或
- BFlat 7.0 and above - http://flattened.net/ + BFlatA - https://github.com/xiaoyuvax/bflata

环境：
- VMware Workstation Player - https://www.vmware.com/products/workstation-player.html
- QEMU - https://www.qemu.org/download 或 VMWare ( 注意，VMware不支持USB键鼠模拟。 )
- Windows 10-11 x64或x86
- 8GB Ram

<br/>
<hr/>
<br/>

![截图](Screenshot3.png)

## 特色

| Feature | Working in VM | Working on hardware | Information |
| ------- | ------------- | ------------------- | ----------- |
| 应用程序 .mue(MOOS用户可执行文件) | 🟩 | 🟩 |
| 抛出/捕获错误 | 🟥 | 🟥 | 
| GC | 🟨 | ⬜ | Not safe |
| 多处理器 | 🟩 | 🟩 |
| 多线程 | 🟩 | 🟩 |
| EHCI (USB2.0) | 🟩 | 🟩 |
| USB键盘 | 🟨 | ⬜ |
| USB鼠标 | 🟩 | ⬜ |
| USB HUB | 🟥 | 🟥 |
| PS2 鼠标/键盘(USB 兼容) | 🟩 | 🟩 |
| 红白机模拟器 | 🟩 | 🟩 |
| DOOM(doomgeneric) | 🟩 | 🟩 |
| Intel® 千兆位以太网控制器 | 🟩 | 🟩 |
| 瑞昱 RTL8139 | 🟩 | ⬜ |
| ExFAT | 🟩 | 🟩 |
| I/O APIC | 🟩 | 🟩 |
| Local APIC | 🟩 | 🟩 |
| SATA | 🟩 | ⬜ |
| IDE | 🟩 | 🟩 |
| SMBIOS | 🟩 | 🟩 |
| ACPI | 🟩 | 🟩 |
| IPv4 | 🟩 | 🟩 |
| IPv6 | 🟥 | 🟥 |
| TCP(WIP) | 🟨 | 🟥 |
| UDP | 🟩 | ⬜ |
| Lan | 🟩 | 🟩 |
| Wan | 🟩 | 🟩 |

| 颜色 | 意思 |
| ----- | ------- |
| 🟩 | 能用 |
| 🟥 | 不能用 |
| 🟨 | 还没完成 |
| ⬜ | 未知 |
