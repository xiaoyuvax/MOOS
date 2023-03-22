
# How to compile Moos with BFlat

## What is BFlat?
BFlat is a C# native compiler offering a Go like buidling experience, which you may use to replace MSBuild. You can download BFlat binary [here]{http://flattened.net}, and check out the project [here](https://github.com/bflattened/bflat). 
Some charming features of BFlat: 
- it doesn't need a project file, but simply builds all codes in a path structure directly into a single executable.
- BFlat can build minimalist binary which runs on bare metal.


MOOS is written in VS, so there is a complicated project structure and piles of descriptive files such as the .csproj files must be served for MSBuild to accomplish the build. In addition, MOOS requires ILCompiler, a Nuget package comes with tools to compile Dotnet IL binary to native binary. The process is more curved than MSBuild with NativeAOT, but the same in nature, While BFlat is more likely on the same route of NativeAOT according to my knowledge (since its author is in deep involvement in the CoreRT etc. project), which builds code to native directly but more straightly intuitive.
Otherwise, MOOS also involves some MASM and C++ codes which should be built separatedly. Here we only discuss about building the C# codes in MOOS.
A problem with MOOS is that it can only be built with an acnient and modified version of ILComplier, as makes it not compatiable with the newest dotnet runtime and is a partial reason that i sought a different and newer native compiler, such as BFlat.

The fact is, BFlat cannot build MOOS directly, at least BFlat doesn't read .csproj file. That's why(of course not for MOOS only) I wrote [BFlatA](https://github.com/xiaoyuvax/bflata), a small wrapper and buildscript generater as well as project flattener for BFlat, which can extract build arguments, file references, dependencies(such as nuget package) and resources from project hierachy starting from a root csproj file, and then allowing executing BFlat to build a C# project written in VS starting from a root .csproj file. You can read more details and find the source code [here](https://github.com/xiaoyuvax/bflata).

## A summary:
In order to build MOOS with BFlat, you need:
- BFlat installed and the bin subdirectory set in %path% of system environment.
- BFlatA built from source (one code file only), you may build it in VS or by BFlat simply (recommended), and then you may copy bflata.exe to the /bin/ path of BFlat, so that it can be run anywhere, since the %path%'s already set.
- Clone my forked version of MOOS which has been modified to be compatiable with building by both MSBuild + ILCompiler in VS and BFlatA+BFlat.





