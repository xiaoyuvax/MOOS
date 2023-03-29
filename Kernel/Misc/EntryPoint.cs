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
            Serial.Initialise();
            Allocator.Initialize((IntPtr)0x20000000);
            Serial.WriteLine("Allocator...OK!");
            StartupCodeHelpers.InitializeModules(Modules);
            Serial.WriteLine("StartupCodeHelpers...OK!");
            PageTable.Initialise();
            Serial.WriteLine("PageTable...OK!");
            ASC16.Initialise();
            Serial.WriteLine("ASC16...OK!");
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
            Serial.WriteLine("VBEInfo...OK!");

            Console.Setup();
            Serial.WriteLine("Console...OK!");
            IDT.Disable();
            Serial.WriteLine("IDT...Disabled!");
            GDT.Initialise();
            Serial.WriteLine("GDT...OK!");
            IDT.Initialize();
            Serial.WriteLine("IDT...Initialized!");
            Interrupts.Initialize();
            Serial.WriteLine("Interrupts...OK!");
            IDT.Enable();
            Serial.WriteLine("Interrupts...Enabled!");

            SSE.enable_sse();
            Serial.WriteLine("SSE...Enabled!");
            //AVX.init_avx();

            ACPI.Initialize();
            Serial.WriteLine("ACPI...OK!");
#if UseAPIC
            PIC.Disable();
            LocalAPIC.Initialize();
            Serial.WriteLine("LocalAPIC...OK!");
            IOAPIC.Initialize();
            Serial.WriteLine("IOAPIC...OK!");
#else
        PIC.Enable();
#endif
            Timer.Initialize();
            Serial.WriteLine("Timer...OK!");
            Keyboard.Initialize();
            Serial.WriteLine("Keyboard...OK!");

            PS2Controller.Initialize();
            Serial.WriteLine("PS2Controller...OK!");
            VMwareTools.Initialize();
            Serial.WriteLine("VMwareTools...OK!");

            SMBIOS.Initialise();
            Serial.WriteLine("SMBIOS...OK!");

            PCI.Initialise();
            Serial.WriteLine("PCI...OK!");

            IDE.Initialize();
            Serial.WriteLine("IDE...OK!");

            SATA.Initialize();
            Serial.WriteLine("SATA...OK!");

            ThreadPool.Initialize();
            Serial.WriteLine("ThreadPool...OK!");

            Console.WriteLine($"[SMP] Trampoline: 0x{((ulong)Trampoline).ToString("x2")}");
            NativeCS.Movsb((byte*)SMP.Trampoline, (byte*)Trampoline, 512);
            

            SMP.Initialize((uint)SMP.Trampoline);
            Serial.WriteLine("SMP...OK!");

            //Only fixed size vhds are supported!
            Console.Write("[Initrd] Initrd: 0x");
            Console.WriteLine((Info->Mods[0]).ToString("x2"));
            Console.WriteLine("[Initrd] Initializing Ramdisk");
            new Ramdisk((IntPtr)(Info->Mods[0]));
            Serial.WriteLine("Ramdisk...OK!");
            //new FATFS();
            new TarFS();
            Serial.WriteLine("TarFS...OK!");

            KMain();
        }

        [DllImport("*")]
        public static extern void KMain();
    }
}