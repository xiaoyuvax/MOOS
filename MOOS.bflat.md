[![Language switcher](https://img.shields.io/badge/Language%20%2F%20%E8%AF%AD%E8%A8%80-Chinese%20%2F%20%E4%B8%AD%E6%96%87-blue)](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bflat.CN.md)

# How to compile MOOS with BFlat

## What is BFlat?
BFlat is a C# native compiler offering a Go like building experience, which you may use to replace MSBuild. You can download BFlat binary [here](http://flattened.net), and check out the project [here](https://github.com/bflattened/bflat).

Some charming features of BFlat: 
- it doesn't need a project file, but simply builds all codes in a path structure directly into a single executable.
- BFlat can build minimalist binary which runs on bare metal.


MOOS is written in VS, so there is a complicated project structure and piles of descriptive files such as the .csproj files must be served for MSBuild to accomplish the build. In addition, MOOS requires ILCompiler, a Nuget package comes with tools to compile Dotnet IL binary to native, which is what NativeAOT means before Dotnet8. However BFlat is exactly another version of NativeAOT but more intuitive.
Otherwise, MOOS also involves some NASM and C++ codes which should be built separatedly. Here we focus on building the C# codes in MOOS.
A problem with MOOS is that it can only be built with an acnient and modified version of ILComplier, as makes it not compatiable with the newest dotnet runtime (see below [MOOS Runtime Changes](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bflat.md#modifications-in-moos-runtime)) and is a partial reason that i sought a different and newer native compiler, such as BFlat.

The fact is, BFlat cannot build MOOS directly, at least BFlat doesn't read .csproj file. That's why(of course not for MOOS only) I wrote [BFlatA](https://github.com/xiaoyuvax/bflata), a small wrapper and buildscript generater as well as project flattener for BFlat, which can extract build arguments, file references, dependencies(such as nuget package) and resources from project hierachy starting from a root csproj file, and then allowing executing BFlat to build a C# project written in VS starting from a root .csproj file. You can read more details and find the source code [here](https://github.com/xiaoyuvax/bflata).

Special Note: Since the linker comes with BFlat is not compatiable with the MSVC static libs referenced(at least version that i used, maybe not for other), so i used the MSVC linker comes with that lib as to avoid problem.

## A summary:
In order to build MOOS with BFlat, you need:
- [BFlat](https://github.com/bflattened/bflat) installed and the bin subdirectory set in %path% of system environment.
- [BFlatA](https://github.com/xiaoyuvax/bflata) built from source (one code file only), you may build it in VS or by BFlat simply (recommended), and then you may copy bflata.exe to the /bin/ path of BFlat, so that it can be run anywhere, since the %path%'s already set.
- Make sure MSVC linker, namely "link.exe" is present on your system, if you have VS installed with C++ workload installed, then you've already had it.
- Clone my forked version of MOOS which has been modified to be compatiable with building by both MSBuild + ILCompiler in VS and BFlatA+BFlat.

## Building Steps
### 1.Prepare args for BFlatA 
Save the following text to a "moos.bfa" file(or download it [here](https://github.com/xiaoyuvax/MOOS/blob/master/MOOS.bfa)), all paths inside shall be reviewed to suit your environment.Make sure you have all needed files meantioned below including the MSVC linker of proper version in your system.
This .bfa file have included all Prebuild Actions such as invoking nasm.exe to compile the .asm codes and Postbuild Actions such as packing MOOS into ramdisk, making ISO image and starting VMWare Player.

	# BFlatA verb and project to build
	## these two lines must present at the start in order.
	build
	D:\Repos\MOOS\MOOS\MOOS.csproj

	# Solution Home:
	-h:d:\repos\moos 

	# Base lib selection:
	## If there's <NoStdLib> tag in .csproj, you don't have to add this line below
	--stdlib None
	--libc none

	# Use external linker:
	## The linker comes with BFlat has some problem with MSVC libs, we'll use MSVC Linker instead.Otherwise You may try remove this line as to use bflat attached linker.
	--linker:"...\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\link.exe"

	# Additional linker args:	
	## Due to bflat's arg parsing bug, spaces in path does not work, may be replaced with short filenames like below or use single quotes "'" as inner quotation.
	--ldflags "/libpath:...\VC\Tools\MSVC\14.35.32215\lib\x64"
	## The following line is not neccessary if you want an optimized release build
	## --ldflags "/DEBUG"

	# Prebuild actions:
	-pra:"'$(MSBuildStartupDirectory)\Tools\nasm.exe' -fbin '$(MSBuildStartupDirectory)\Tools\Trampoline.asm' -o trampoline.o"
	-pra:"'$(MSBuildStartupDirectory)\Tools\nasm.exe' -fbin '$(MSBuildStartupDirectory)\Tools\EntryPoint.asm' -o loader.o"

	# Postbuild actions:
	-poa:cmd.exe /c copy /b loader.o + moos.exe "$(MSBuildStartupDirectory)\Tools\grub2\boot\kernel.bin"
	-poa:"'$(MSBuildStartupDirectory)\Tools\mkisofs.exe' -relaxed-filenames -J -R -o MOOS.iso -b boot/grub/i386-pc/eltorito.img -no-emul-boot -boot-load-size 4 -boot-info-table  '$(MSBuildStartupDirectory)\Tools\grub2'"
	-poa:"'D:\Program Files (x86)\VMware\VMware Player\vmplayer.exe' '$(MSBuildStartupDirectory)\Tools\VMWare\MOOS\MOOS.flat.vmx'"

### 2.Ensure BFlat and BFlatA are both set in %PATH%.
### 3.Run BFlatA with the .bfa file.

    bflata -inc:moos.bfa

All file output should be at current directary, including build scripts like build.rsp, link.rsp, binary output like MOOS.obj and MOOS.exe, etc., and ISO image MOOS.iso, which shall be correctly specified in the MOOS.flat.vmx file for VMWare to locate the disc image.

BFlatA output:

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

Then you will see MOOS launched in VMWare:
![image](https://user-images.githubusercontent.com/6511226/228498471-0baf5415-b000-45f8-9c20-b35b3f634089.png)

## Modifications in MOOS Runtime

Check out:
[https://github.com/bflattened/bflat/issues/95](https://github.com/bflattened/bflat/issues/95#issuecomment-1471409976)




