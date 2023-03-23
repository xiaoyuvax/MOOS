[![Language switcher](https://img.shields.io/badge/Language%20%2F%20%E8%AF%AD%E8%A8%80-Chinese%20%2F%20%E4%B8%AD%E6%96%87-blue)](https://github.com/xiaoyuvax/bflata/blob/main/README.zh-cn.md)

# How to compile MOOS with BFlat

## What is BFlat?
BFlat is a C# native compiler offering a Go like buidling experience, which you may use to replace MSBuild. You can download BFlat binary [here]{http://flattened.net}, and check out the project [here](https://github.com/bflattened/bflat). 
Some charming features of BFlat: 
- it doesn't need a project file, but simply builds all codes in a path structure directly into a single executable.
- BFlat can build minimalist binary which runs on bare metal.


MOOS is written in VS, so there is a complicated project structure and piles of descriptive files such as the .csproj files must be served for MSBuild to accomplish the build. In addition, MOOS requires ILCompiler, a Nuget package comes with tools to compile Dotnet IL binary to native, which is what NativeAOT means before Dotnet8. However BFlat is exacrtly another version of NativeAOT but more intuitive.
Otherwise, MOOS also involves some MASM and C++ codes which should be built separatedly. Here we only discuss about building the C# codes in MOOS.
A problem with MOOS is that it can only be built with an acnient and modified version of ILComplier, as makes it not compatiable with the newest dotnet runtime (see below MOOS Runtime Change) and is a partial reason that i sought a different and newer native compiler, such as BFlat.

The fact is, BFlat cannot build MOOS directly, at least BFlat doesn't read .csproj file. That's why(of course not for MOOS only) I wrote [BFlatA](https://github.com/xiaoyuvax/bflata), a small wrapper and buildscript generater as well as project flattener for BFlat, which can extract build arguments, file references, dependencies(such as nuget package) and resources from project hierachy starting from a root csproj file, and then allowing executing BFlat to build a C# project written in VS starting from a root .csproj file. You can read more details and find the source code [here](https://github.com/xiaoyuvax/bflata).

## A summary:
In order to build MOOS with BFlat, you need:
- BFlat installed and the bin subdirectory set in %path% of system environment.
- BFlatA built from source (one code file only), you may build it in VS or by BFlat simply (recommended), and then you may copy bflata.exe to the /bin/ path of BFlat, so that it can be run anywhere, since the %path%'s already set.
- Clone my forked version of MOOS which has been modified to be compatiable with building by both MSBuild + ILCompiler in VS and BFlatA+BFlat.

## Buiding Steps
### 1.Prepare args for BFlatA
Save the following text to a "moos.bfa" file, all paths inside shall be reviewed to suit your environment.

	#BFlatA verb and project to build, these two lines must present at the start in order.
	build
	D:\Repos\MOOS\MOOS\moos.csproj

	#Solution Home:
	-h:d:\repos\moos 

	#Base lib selection:
	#if there's <NoStdLib> tag in .csproj, you don't have to add this line below
	--stdlib None
	--libc none

	#Use external linker:
	#The linker comes with BFlat has some problem with MSVC libs, so use --linker option to supress bflat invoke its own linker by supplying another. We'll use MSVC Linker instead.
	--linker:"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\link.exe"

	#Additional linker args:
	#Bflat doesn't produce this .res file which seems a must for MOOS image, but actually contains no much data.
	--ldflags "D:\Repos\MOOS\MOOS\obj\debug\net7.0\win-x64\native\MOOS.res"
	#Due to bflat's arg parsing bug, spaces in path does not work, must be replaced with short filenames like below. It's not neccessary with MSVC Linker.
	--ldflags "/libpath:C:\Progra~1\Micros~4\2022\Enterprise\VC\Tools\MSVC\14.35.32215\lib\x64"

### 2.Ensure BFlat and BFlatA are both set in %PATH%.
### 3.Run BFlatA 

    bflata -inc:moos.bfa

BFlatA output:

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

## Followups
Now you get moos.exe at current directory (unless you specify -o:<output file> in moos.bfa file), the rest work is to merge MOOS.exe with loader.o to get the kernel.bin, and the last step is to make an iso which is bootable by using Grub2 which load the kernel.bin after startup. As you can see in moos.csproj.

## Modifications in MOOS Runtime

As described in:
[https://github.com/bflattened/bflat/issues/95](https://github.com/bflattened/bflat/issues/95#issuecomment-1471409976)




