[![Language switcher](https://img.shields.io/badge/Language%20%2F%20%E8%AF%AD%E8%A8%80-English%20%2F%20%E8%8B%B1%E8%AF%AD-blue)](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bflat.md)

# 如何使用BFlat编译MOOS

## BFlat是啥?
BFlat是一个C#本地编译器，提供了类似Go的构建体验，可以用来替代MSBuild。你可以在[这里](http://flattened.net)下载BFlat二进制文件，并[在这里](https://github.com/bflattened/bflat)查看工程源码。
BFlat的一些优点：
- 它不需要工程配置文件，而是直接将所有代码在路径结构中构建为单个可执行文件。
- BFlat可以构建在裸机上运行的最小化的二进制文件。

MOOS是在VS中编写的，因此存在复杂的项目结构和大量的描述文件，例如必须为MSBuild提供.csproj文件。此外，MOOS需要ILCompiler，这是一个Nuget包，附带了将Dotnet IL二进制代码编译为本机代码的工具，这是在Dotnet8之前的NativeAOT的编译方式。但是，BFlat可以算是NativeAOT的另一种感觉更直觉一点的编译器。
此外，MOOS还包括了一些MASM和C++代码，这些代码应该被单独编译。这里我们只讨论在MOOS中构建C#代码的问题。
MOOS的一个问题是，它目前只能使用一个古老且修改过的版本的ILComplier构建，这使得它与最新的dotnet运行时不兼容（请参见下面的MOOS运行时更改），这也是我寻找不同且更新的本地编译器（例如BFlat）的部分原因。

事实是，BFlat不能直接构建MOOS，至少BFlat不读取.csproj文件。这就是我写[BFlatA](https://github.com/xiaoyuvax/bflata)的原因，它是一个配合BFlat的套壳编译工具、编译脚本生成器以及用于BFlat的代码文件提取器（如果沿用bflat的哲学，称为“打平器”更有意思），它可以从一个根级csproj文件开始提取构建参数、文件引用、依赖项（如nuget包）和资源文件，然后允许执行BFlat从根.csproj文件开始构建在VS中编写的C#项目。您可以在[这里](https://github.com/xiaoyuvax/bflata)看更多详细信息并找到源代码。

## 总结一下:
为了使用BFlat编译MOOS，你需要：

- 安装BFlat，并将其bin子目录设置在系统环境的%path%中。
- 从源码编译BFlatA（仅一个代码文件）。你可以在VS中编译，也可以简单地通过BFlat编译（推荐），然后你可以将bflata.exe复制到BFlat的/bin/路径中，这样它就可以在任何地方运行，因为%path%已经设置好了。
- 克隆我修改后的MOOS分支，这个版本能够兼容地在VS中用MSBuild + ILCompiler编译也能够使用BFlatA + BFlat编译(但最后都是用MSVC Linker链接)。

## 构建步骤
### 1.准备BFlatA用的编译参数
把下面的参数文本保存到一个叫"moos.bfa"的文件, 其中所有路径应该按你的环境替换。

	#BFlatA的动词和编译目标，必须放在前两行（目前）。
	build
	D:\Repos\MOOS\MOOS\moos.csproj

	#解决方案根路径
	-h:d:\repos\moos 

	#基本库选择:
	#如果.csproj文件中有指定<NoStdLib> 就不用下面这第一行了。
	--stdlib None
	--libc none

	#使用外部链接器：
	#BFlat自带的链接器使用MSVC的静态库的时候有点问题，所以使用--linker指定外部连接器防止BFlat调用自己的链接器。这里我们用MSVC的链接器。
	--linker:"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\link.exe"

	#其他链接器参数：
	#BFlat不生成下面这个.res文件，但好像MOOS如果不嵌入这个文件，就无法工作，所以还是需要引用一下，这个文件得靠MSBuild生成，这个我暂时也很无奈。
	--ldflags "D:\Repos\MOOS\MOOS\obj\debug\net7.0\win-x64\native\MOOS.res"	
	#鉴于BFlat的参数分析Bug，我这里之前用了短文件名。但实际对于MSVC Linker不需要。
	--ldflags "/libpath:C:\Progra~1\Micros~4\2022\Enterprise\VC\Tools\MSVC\14.35.32215\lib\x64"

### 2.确保%PATH%变量中设好了BFlat和BFlatA的路径。
### 3.运行BFlatA 

    bflata -inc:moos.bfa

BFlatA输出:

	BFlatA V1.4.2.0 @github.com/xiaoyuvax/bflata
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
	Output          :<Default>
	TargetFx        :net7.0
	PackageRoot     :<N/A>
	Home            :d:\repos\moos
	BFA Includes    :1
	Args for BFlat  :--stdlib None --libc none -c --ldflags "D:\Repos\MOOS\MOOS\obj\debug\net7.0\win-x64\native\MOOS.res" --ldflags "/libpath:C:\Progra~1\Micros~4\2022\Enterprise\VC\Tools\MSVC\14.35.32215\lib\x64"

	--LIB EXCLU---------------------------
	--LIB CACHE---------------------------


	--PARSING-----------------------------
	Parsing Project:D:\Repos\MOOS\MOOS\moos.csproj ...
		       NativeLib        [Include]       9 items added!
	Parsing Project:D:\Repos\MOOS\Kernel\Kernel.projitems ...
		  CompileInclude        [Include]       6400 items added!
	Parsing Project:D:\Repos\MOOS\Corlib\Corlib.projitems ...
		  CompileInclude        [Include]       12996 items added!

	--SCRIPTING---------------------------
	Generating build script for:moos
	- Found 10 args to be passed to BFlat.
	- Found 215 code files(*.cs)
	- Found 3 dependent native libs(*.lib|*.a)
	Build script's written!


	--BUILDING----------------------------
	Building in FLAT mode:moos...
	- Executing build script: bflat build @build.rsp...
	Compiler exit code:0
	Microsoft (R) Incremental Linker Version 14.35.32215.0
	Copyright (C) Microsoft Corporation.  All rights reserved.

	moos.obj
	/ENTRY:Entry
	/SUBSYSTEM:NATIVE
	/INCREMENTAL:no
	/fixed
	/base:0x10000000
	D:\Repos\MOOS\MOOS\obj\debug\net7.0\win-x64\native\MOOS.res
	/libpath:C:\Progra~1\Micros~4\2022\Enterprise\VC\Tools\MSVC\14.35.32215\lib\x64
	d:\repos\moos\x64\Debug\NativeLib.lib
	d:\repos\moos\x64\Debug\LibC.lib
	d:\repos\moos\x64\Debug\Doom.lib
	NativeLib.lib(interrupts.obj) : warning LNK4075: 忽略“/EDITANDCONTINUE”(由于“/OPT:ICF”规范)
	LINK : warning LNK4217:符号“free”(在“ moos.obj”中定义)已由“NativeLib.lib(lodepng.obj)”(函数“lodepng_free”中)导入LINK : warning LNK4217:符号“malloc”(在“ moos.obj”中定义)已由“NativeLib.lib(lodepng.obj)”(函数“lodepng_malloc”中)导入
	LINK : warning LNK4217:符号“realloc”(在“ moos.obj”中定义)已由“NativeLib.lib(lodepng.obj)”(函数“lodepng_realloc”中)导入
	LINK : warning LNK4281:x64 映像的基址 0x10000000 不适当；将基址设为 4 GB 以上以实现最佳 ASLR 优化
	Linker exit code:0
	--END---------------------------------

## 后续工作
现在你在当前路径会看到moos.exe(除非用在 moos.bfa中指定了-o:<output file>选项，默认是在当前路径)， 剩下的工作就是把MOOS.exe跟用宏汇编编译的loader.o文件合并成kernel.bin，然后最后的一步就是利用Grub2将kernel.bin打包到一个光盘映像文件(.iso)，保证Grub启动后会加载kernel.bin，这些在MOOS.csproj里面都写得很清楚。

## MOOS自带运行时库内的更改

见下面这个跟bflat作者讨论的帖子：
[https://github.com/bflattened/bflat/issues/95](https://github.com/bflattened/bflat/issues/95#issuecomment-1471409976)




