using Internal.Runtime.CompilerHelpers;
using MOOS.Driver;
using MOOS.FS;
using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace MOOS.Misc
{
    internal static unsafe class EntryPoint
    {
        [RuntimeExport("Entry")]
        public static void Entry(MultibootInfo* Info, IntPtr Modules, IntPtr Trampoline)
        {
            Serial.Initialise(); //COM1 would be used to output debug message
            Allocator.Initialize((IntPtr)0x20000000);
            ComDebugger.Info("Allocator", "OK!");
            StartupCodeHelpers.InitializeModules(Modules);
            ComDebugger.Info("StartupCodeHelpers", "OK!");
            PageTable.Initialise();
            ComDebugger.Info("PageTable", "OK!");
            ASC16.Initialise();
            ComDebugger.Info("ASC16", "OK!");
            VBEInfo* info = (VBEInfo*)Info->VBEInfo;
            if (info->PhysBase != 0)
            {
                Framebuffer.Initialize(info->ScreenWidth, info->ScreenHeight, (uint*)info->PhysBase);
                Framebuffer.Graphics.Clear(0x0);
            }
            else
            {
                for (; ; ) Native.Hlt();
            }
            ComDebugger.Info("FrameBuffer", "OK!");

            Console.Setup();
            ComDebugger.Info("Console", "OK!");
            IDT.Disable();
            ComDebugger.Info("IDT", "Disabled!");
            GDT.Initialise();
            ComDebugger.Info("GDT", "OK!");
            IDT.Initialize();
            ComDebugger.Info("IDT", "Initialized!");
            Interrupts.Initialize();
            ComDebugger.Info("Interrupts", "OK!");
            IDT.Enable();
            ComDebugger.Info("Interrupts", "Enabled!");

            SSE.enable_sse();
            ComDebugger.Info("SSE", "Enabled!");
            //AVX.init_avx();

            ACPI.Initialize();
            ComDebugger.Info("ACPI", "OK!");
#if UseAPIC
            PIC.Disable();
            LocalAPIC.Initialize();
            ComDebugger.Info("LocalAPIC", "OK!");
            IOAPIC.Initialize();
            ComDebugger.Info("IOAPIC", "OK!");
#else
        PIC.Enable();
#endif
            Timer.Initialize();

            ComDebugger.Info("Timer", "OK!");
            Keyboard.Initialize();
            ComDebugger.Info("Keyboard", "OK!");

            PS2Controller.Initialize();
            ComDebugger.Info("PS2Controller", "OK!");
            VMwareTools.Initialize();
            ComDebugger.Info("VMwareTools", "OK!");

            SMBIOS.Initialise();
            ComDebugger.Info("SMBIOS", "OK!");

            PCI.Initialise();
            ComDebugger.Info("PCI", "OK!");

            IDE.Initialize();
            ComDebugger.Info("IDE", "OK!");

            SATA.Initialize();
            ComDebugger.Info("SATA", "OK!");

            ThreadPool.Initialize();
            ComDebugger.Info("ThreadPool", "OK!");

            Console.WriteLine($"[SMP] Trampoline: 0x{((ulong)Trampoline).ToString("x2")}");
            NativeCS.Movsb((byte*)SMP.Trampoline, (byte*)Trampoline, 512);

            SMP.Initialize((uint)SMP.Trampoline);
            ComDebugger.Info("SMP", "OK!");
            ComDebugger.IsTimerInitialized = true;
#if ComDebugger
            Console.Write("[Debugger] Waiting for COM Debugger connection, send any char from COM1 to continue...");
            Serial.ReadSerial();
#endif
            //Only fixed size vhds are supported!
            Console.Write("[Initrd] Initrd: 0x");
            Console.WriteLine((Info->Mods[0]).ToString("x2"));
            Console.WriteLine("[Initrd] Initializing Ramdisk");
            new Ramdisk((IntPtr)(Info->Mods[0]));
            ComDebugger.Info("Ramdisk", "OK!");
            //new FATFS();
            new TarFS();
            ComDebugger.Info("TarFS", "OK!");

            KMain();
        }

        [DllImport("*")]
        public static extern void KMain();
    }
}