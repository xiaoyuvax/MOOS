using System.Runtime;
using System.Runtime.InteropServices;

namespace MOOS.App
{
    public static unsafe class Interop
    {
        #region Route Framework Calls to System Calls

        [RuntimeExport("malloc")]
        public static nint malloc(ulong size) => Allocate(size);

        [DllImport("Allocate")]
        public static extern nint Allocate(ulong size);

        [RuntimeExport("free")]
        public static ulong free(nint ptr) => AFree(ptr);

        [DllImport("Free")]
        public static extern ulong AFree(nint ptr);

        [RuntimeExport("Lock")]
        public static void Lock() => ALock();

        [RuntimeExport("Unlock")]
        public static void UnLock() => AUnLock();

        [DllImport("Lock")]
        public static extern void ALock();

        [DllImport("Unlock")]
        public static extern void AUnLock();

        #endregion Route Framework Calls to System Calls

        [DllImport("WriteLine")]
        public static extern void WriteLine();

        [DllImport("WriteString")]
        public static extern void WriteString(string str);

        [DllImport("DebugWrite")]
        public static extern void DebugWrite(string str);

        [DllImport("Write")]
        public static extern void Write(char c);
    }
}