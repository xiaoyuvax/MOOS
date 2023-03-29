[![Language switcher](https://img.shields.io/badge/Language%20%2F%20%E8%AF%AD%E8%A8%80-English%20%2F%20%E8%8B%B1%E8%AF%AD-blue)](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bflat.md)

# 如何使用BFlat编译MOOS

## BFlat是啥?
BFlat是一个C#本地编译器，提供了类似Go的构建体验，可以用来替代MSBuild。你可以在[这里](http://flattened.net)下载BFlat二进制文件，并[在这里](https://github.com/bflattened/bflat)查看工程源码。
BFlat的一些优点：
- 它不需要工程配置文件，而是直接将所有代码在路径结构中构建为单个可执行文件。
- BFlat可以构建在裸机上运行的最小化的二进制文件。

MOOS是在VS中编写的，因此存在复杂的项目结构和大量的描述文件，例如必须为MSBuild提供.csproj文件。此外，MOOS需要ILCompiler，这是一个Nuget包，附带了将Dotnet IL二进制代码编译为本机代码的工具，这是在Dotnet8之前的NativeAOT的编译方式。但是，BFlat可以算是NativeAOT的另一种感觉更直觉一点的编译器。
此外，MOOS还包括了一些NASM和C++代码，这些代码应该被单独编译。这里我们只讨论在MOOS中构建C#代码的问题。
MOOS的一个问题是，它目前只能使用一个古老且修改过的版本的ILComplier构建，这使得它与最新的dotnet运行时不兼容（请参见下面的MOOS运行时更改），这也是我寻找不同且更新的本地编译器（例如BFlat）的部分原因。

事实是，BFlat不能直接构建MOOS，至少BFlat不读取.csproj文件。所以我写了[BFlatA](https://github.com/xiaoyuvax/bflata)这个工具，它是一个配合BFlat的套壳编译工具、编译脚本生成器以及用于BFlat的代码文件提取器（如果沿用bflat的哲学，称为“打平器”更有意思），它可以从一个根级csproj文件开始提取构建参数、文件引用、依赖项（如nuget包）和资源文件，然后允许执行BFlat从根.csproj文件开始构建在VS中编写的C#项目。您可以在[这里](https://github.com/xiaoyuvax/bflata)看更多详细信息并找到源代码。

特别说明：由于BFlat自带的链接器可能跟MSVC库有一些不兼容（至少我使用的版本，但不一定在所有版本上都有问题），这里采用了MSVC的链接器取代了BFlat的自带链接器。

## 总结一下:
为了使用BFlat编译MOOS，你需要：

- 安装BFlat，并将其bin子目录设置在系统环境的%path%中。
- 从源码编译BFlatA（仅一个代码文件）。你可以在VS中编译，也可以简单地通过BFlat编译（推荐），然后你可以将bflata.exe复制到BFlat的/bin/路径中，这样它就可以在任何地方运行，因为%path%已经设置好了。
- 你的系统上需要装有MSVC链接器(link.exe)。如果已经安装了VS和C++组件，那么就应该已经有了。
- 克隆我的这个MOOS分支版本，这个版本中做了相应的修改以兼容既能够在VS中用MSBuild + ILCompiler编译也能够使用BFlatA + BFlat编译(但最后都是用MSVC Linker链接)。

## 编译步骤
### 1.准备BFlatA用的编译参数
把下面的参数文本保存到一个叫"moos.bfa"的文件(或者直接在[这里](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bfa)下载), 其中所有路径应该按你的环境替换。

	# BFlatA 动词以及需要编译的项目
	## 这两行必须依次列在开头
	build
	D:\Repos\MOOS\MOOS\MOOS.csproj

	# 解决方案路径
	-h:d:\repos\moos 

	# 基本库选择：
	## 如果csproj文件中指定了<NoStdLib>拿可以不用写下面这行。
	--stdlib None
	--libc none

	# 使用外部链接器：
	## BFlat的链接器跟我用的MSVC库有点不兼容，所以这里使用了配套的链接器。你也可以尝试去掉这行直接使用bflat带的链接器。
	--linker:"...\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\link.exe"

	# 其他连接参数：
	## 由于BFlat参数分析的bug，参数中带空格的路径无法工作，所以可以考虑采用无空格的短路径格式或者在内侧使用“'”。
	--ldflags "/libpath:...\VC\Tools\MSVC\14.35.32215\lib\x64"
	## 如果不需要生成调试版本，可以去掉下面这行。
	## --ldflags "/DEBUG"

	# 编译前动作：
	-pra:"'$(MSBuildStartupDirectory)\Tools\nasm.exe' -fbin '$(MSBuildStartupDirectory)\Tools\Trampoline.asm' -o trampoline.o"
	-pra:"'$(MSBuildStartupDirectory)\Tools\nasm.exe' -fbin '$(MSBuildStartupDirectory)\Tools\EntryPoint.asm' -o loader.o"

	# 编译后动作：
	-poa:cmd.exe /c copy /b loader.o + moos.exe "$(MSBuildStartupDirectory)\Tools\grub2\boot\kernel.bin"
	-poa:"'$(MSBuildStartupDirectory)\Tools\mkisofs.exe' -relaxed-filenames -J -R -o MOOS.iso -b boot/grub/i386-pc/eltorito.img -no-emul-boot -boot-load-size 4 -boot-info-table  '$(MSBuildStartupDirectory)\Tools\grub2'"
	-poa:"'D:\Program Files (x86)\VMware\VMware Player\vmplayer.exe' '$(MSBuildStartupDirectory)\Tools\VMWare\MOOS\MOOS.flat.vmx'"

### 2.确保%PATH%变量中设好了BFlat和BFlatA的路径。
### 3.运行BFlatA 

    bflata -inc:moos.bfa

所有的输出都将位于当前目录，包括编译脚本，诸如 build.rsp, link.rsp；二进制文件输出，如MOOS.obj, MOOS.exe等；还有光盘ISO映像MOOS.iso。你必须在指定的VMWare虚拟机配置文件MOOS.flat.vmx指定到此iso映像的正确路径才能正常启动虚拟机。

BFlatA输出:

	BFlatA V1.4.2.2 @github.com/xiaoyuvax/bflata
	Description:
	  A wrapper/build script generator for BFlat, a native C# compiler, for recusively building .csproj file with:
	    - Referenced projects
	    - Nuget package dependencies
	    - Embedded resources
	  Before using BFlatA, you should get BFlat first at https://flattened.net.


	--ARGS--------------------------------
	Action          :Build
	BuildMode       :Flat
	DepositDep      :Off
	Target          :Exe
	TargetOS        :windows
	Output          :<Default>
	TargetFx        :net7.0
	PackageRoot     :<N/A>
	Home            :d:\repos\moos
	BFA Includes    :1
	Args for BFlat  :--stdlib None --libc none --ldflags "/libpath:C:\Progra~1\Micros~4\2022\Enterprise\VC\Tools\MSVC\14.35.32215\lib\x64" -c

	--LIB EXCLU---------------------------
	--LIB CACHE---------------------------


	--PARSING-----------------------------
	Parsing Project:D:\Repos\MOOS\MOOS\MOOS.csproj ...
		       NativeLib        [Include]       9 items added!
	Parsing Project:D:\Repos\MOOS\Kernel\Kernel.projitems ...
		  CompileInclude        [Include]       6400 items added!
	Parsing Project:D:\Repos\MOOS\Corlib\Corlib.projitems ...
		  CompileInclude        [Include]       12769 items added!

	--SCRIPTING---------------------------
	Generating build script for:MOOS
	- Found 7 args to be passed to BFlat.
	- Found 216 code files(*.cs)
	- Found 3 dependent native libs(*.lib|*.a)
	Script:build.rsp written!


	--PREBUILD-ACTIONS-------------------
	Prebuild actions exit code:0 - ["'d:\repos\moos\Tools\nasm.exe' -fbin 'd:\repos\moos\Tools\Trampoline.asm' -o trampoline.o"]
	d:\repos\moos\Tools\EntryPoint.asm:338: warning: uninitialized space declared in .text section: zeroing [-w+zeroing]
	d:\repos\moos\Tools\EntryPoint.asm:342: warning: uninitialized space declared in .text section: zeroing [-w+zeroing]
	d:\repos\moos\Tools\EntryPoint.asm:344: warning: uninitialized space declared in .text section: zeroing [-w+zeroing]
	d:\repos\moos\Tools\EntryPoint.asm:346: warning: uninitialized space declared in .text section: zeroing [-w+zeroing]
	Prebuild actions exit code:0 - ["'d:\repos\moos\Tools\nasm.exe' -fbin 'd:\repos\moos\Tools\EntryPoint.asm' -o loader.o"]

	--BUILDING----------------------------
	Building in FLAT mode:MOOS...
	- Executing build script: bflat build @build.rsp...
	Compiler exit code:0
	Script:link.rsp written!

	Microsoft (R) Incremental Linker Version 14.35.32215.0
	Copyright (C) Microsoft Corporation.  All rights reserved.

	MOOS.obj
	/fixed
	/base:0x10000000
	/map:Kernel.map
	/ENTRY:Entry
	/SUBSYSTEM:NATIVE
	/INCREMENTAL:no
	/libpath:C:\Progra~1\Micros~4\2022\Enterprise\VC\Tools\MSVC\14.35.32215\lib\x64
	d:\repos\moos\x64\Debug\NativeLib.lib
	d:\repos\moos\x64\Debug\LibC.lib
	d:\repos\moos\x64\Debug\Doom.lib
	NativeLib.lib(interrupts.obj) : warning LNK4075: 忽略“/EDITANDCONTINUE”(由于“/OPT:ICF”规范)
	LINK : warning LNK4217:符号“free”(在“ MOOS.obj”中定义)已由“NativeLib.lib(lodepng.obj)”(函数“lodepng_free”中)导入
	LINK : warning LNK4217:符号“malloc”(在“ MOOS.obj”中定义)已由“NativeLib.lib(lodepng.obj)”(函数“lodepng_malloc”中)导入
	LINK : warning LNK4217:符号“realloc”(在“ MOOS.obj”中定义)已由“NativeLib.lib(lodepng.obj)”(函数“lodepng_realloc”中)导入
	LINK : warning LNK4281:x64 映像的基址 0x10000000 不适当；将基址设为 4 GB 以上以实现最佳 ASLR 优化
	Linker exit code:0

	--POSTBUILD-ACTIONS------------------
	loader.o
	MOOS.exe
	已复制         1 个文件。
	Postbuild actions exit code:0 - [cmd.exe /c copy /b loader.o + moos.exe "d:\repos\moos\Tools\grub2\boot\kernel.bin"]
	mkisofs: Warning: -rock has same effect as -rational-rock on this platform.
	Warning: creating filesystem that does not conform to ISO-9660.
	Using PART_000.MOD;1 for  d:\repos\moos\Tools\grub2/boot/grub/i386-pc/part_sunpc.mod (part_sun.mod)
	Size of boot image is 4 sectors -> No emulation
	 27.69% done, estimate finish Wed Mar 29 16:26:48 2023
	 55.28% done, estimate finish Wed Mar 29 16:26:48 2023
	 82.97% done, estimate finish Wed Mar 29 16:26:48 2023
	Total translation table size: 2048
	Total rockridge attributes bytes: 3636
	Total directory bytes: 10720
	Path table size(bytes): 50
	18089 extents written (35 MB)
	Postbuild actions exit code:0 - ["'d:\repos\moos\Tools\mkisofs.exe' -relaxed-filenames -J -R -o MOOS.iso -b boot/grub/i386-pc/eltorito.img -no-emul-boot -boot-load-size 4 -boot-info-table  'd:\repos\moos\Tools\grub2'"]

## MOOS自带运行时库内的更改

见下面这个跟bflat作者讨论的帖子：
[https://github.com/bflattened/bflat/issues/95](https://github.com/bflattened/bflat/issues/95#issuecomment-1471409976)




