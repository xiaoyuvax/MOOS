using Internal.Runtime;
#if BFLAT
using System.Runtime.CompilerServices;
#else
using Internal.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;

namespace System
{
    public unsafe class Object
    {
        // The layout of object is a contract with the compiler.
        internal unsafe EEType* m_pEEType;

        [StructLayout(LayoutKind.Sequential)]
        private class RawData
        {
            public byte Data;
        }

        internal ref byte GetRawData()
        {
            return ref Unsafe.As<RawData>(this).Data;
        }

        internal uint GetRawDataSize()
        {
#if Kernel
            MOOS.ComDebugger.Debug("MethodTable", $"BaseSize={m_pEEType->BaseSize}");
#endif
            return m_pEEType->BaseSize - (uint)sizeof(ObjHeader) - (uint)sizeof(EEType*);
        }

        public Object() { }
        ~Object() { }

        public virtual bool Equals(object o) => false;

        public virtual int GetHashCode() => 0;

        public virtual string ToString() => "System.Object";

        public virtual void Dispose()
        {
            var obj = this;
            free(Unsafe.As<object, IntPtr>(ref obj));
        }

        public static implicit operator bool(object obj) => obj != null;

        public static implicit operator IntPtr(object obj) => Unsafe.As<object, IntPtr>(ref obj);

        [DllImport("*")]
        static extern ulong free(nint ptr);
    }
}
