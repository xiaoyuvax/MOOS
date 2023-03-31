using System;
using System.Runtime;

#if BFLAT
using System.Runtime.CompilerServices;
#else

using Internal.Runtime.CompilerServices;

#endif

using System.Runtime.InteropServices;

namespace Internal.Runtime.CompilerHelpers
{
    public unsafe class StartupCodeHelpers
    {
        [RuntimeExport("__imp_GetCurrentThreadId")]
        public static int __imp_GetCurrentThreadId() => 0;

        [RuntimeExport("__CheckForDebuggerJustMyCode")]
        public static int __CheckForDebuggerJustMyCode() => 0;

        [RuntimeExport("__fail_fast")]
        private static void FailFast()
        { while (true) ; }

        [RuntimeExport("memset")]
        private static unsafe void MemSet(byte* ptr, int c, int count)
        {
            for (byte* p = ptr; p < ptr + count; p++) *p = (byte)c;
        }

        [RuntimeExport("memcpy")]
        private static unsafe void MemCpy(byte* dest, byte* src, ulong count)
        {
            for (ulong i = 0; i < count; i++) dest[i] = src[i];
        }

        [RuntimeExport("RhpFallbackFailFast")]
        private static void RhpFallbackFailFast()
        { while (true) ; }

        [RuntimeExport("RhpReversePInvoke2")]
        private static void RhpReversePInvoke2(IntPtr frame)
        { }

        [RuntimeExport("RhpReversePInvokeReturn2")]
        private static void RhpReversePInvokeReturn2(IntPtr frame)
        { }

        [RuntimeExport("RhpReversePInvoke")]
        private static void RhpReversePInvoke(IntPtr frame)
        { }

        [RuntimeExport("RhpReversePInvokeReturn")]
        private static void RhpReversePInvokeReturn(IntPtr frame)
        { }

        [RuntimeExport("RhpPInvoke")]
        private static void RhpPinvoke(IntPtr frame)
        { }

        [RuntimeExport("RhpPInvokeReturn")]
        private static void RhpPinvokeReturn(IntPtr frame)
        { }

        [RuntimeExport("RhpNewFast")]
        private static unsafe object RhpNewFast(EEType* pEEType)
        {
            var size = pEEType->BaseSize;

            // Round to next power of 8
            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            var data = malloc(size);
            var obj = Unsafe.As<IntPtr, object>(ref data);
            MemSet((byte*)data, 0, (int)size);
            *(IntPtr*)data = (IntPtr)pEEType;

            return obj;
        }

        [DllImport("*")]
        public static extern nint malloc(ulong size);

        [RuntimeExport("RhpNewArray")]
        internal static unsafe object RhpNewArray(EEType* pEEType, int length)
        {
            var size = pEEType->BaseSize + (ulong)length * pEEType->ComponentSize;

            // Round to next power of 8
            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            var data = malloc(size);
            var obj = Unsafe.As<IntPtr, object>(ref data);
            MemSet((byte*)data, 0, (int)size);
            *(IntPtr*)data = (IntPtr)pEEType;

            var b = (byte*)data;
            b += sizeof(IntPtr);
            MemCpy(b, (byte*)(&length), sizeof(int));

            return obj;
        }

        [RuntimeExport("RhpAssignRef")]
        private static unsafe void RhpAssignRef(void** address, void* obj)
        {
            *address = obj;
        }

        [RuntimeExport("RhpByRefAssignRef")]
        private static unsafe void RhpByRefAssignRef(void** address, void* obj)
        {
            *address = obj;
        }

        [RuntimeExport("RhpCheckedAssignRef")]
        private static unsafe void RhpCheckedAssignRef(void** address, void* obj)
        {
            *address = obj;
        }

        [RuntimeExport("RhpStelemRef")]
        private static unsafe void RhpStelemRef(Array array, int index, object obj)
        {
            fixed (int* n = &array._numComponents)
            {
                var ptr = (byte*)n;
                ptr += sizeof(void*);   // Array length is padded to 8 bytes on 64-bit
                ptr += index * array.m_pEEType->ComponentSize;  // Component size should always be 8, seeing as it's a pointer...
                var pp = (IntPtr*)ptr;
                *pp = Unsafe.As<object, IntPtr>(ref obj);
            }
        }

        [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
        public static unsafe object RhTypeCast_IsInstanceOfClass(EEType* pTargetType, object obj)
        {
            if (obj == null)
                return null;

            if (pTargetType == obj.m_pEEType)
                return obj;

            var bt = obj.m_pEEType->RawBaseType;

            while (true)
            {
                if (bt == null)
                    return null;

                if (pTargetType == bt)
                    return obj;

                bt = bt->RawBaseType;
            }
        }

        private static bool SupportsRelativePointers = false;

        public static void InitializeModules(IntPtr Modules)
        {
#if BFLAT
        SupportsRelativePointers =true;
#endif
            for (int i = 0; ; i++)
            {
                if (((IntPtr*)Modules)[i].Equals(IntPtr.Zero))
                    break;

                var header = (ReadyToRunHeader*)((IntPtr*)Modules)[i];
                var sections = (ModuleInfoRow*)(header + 1);

                if (header->Signature != ReadyToRunHeaderConstants.Signature)
                {
                    FailFast();
                }

                for (int k = 0; k < header->NumberOfSections; k++)
                {
                    if (sections[k].SectionId == ReadyToRunSectionType.GCStaticRegion)
                        InitializeStatics(sections[k].Start, sections[k].End);
                    if (sections[k].SectionId == ReadyToRunSectionType.EagerCctor)
                        RunEagerClassConstructors(sections[k].Start, sections[k].End);
                }
            }

            DateTime.s_daysToMonth365 = new int[]{
                0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
            DateTime.s_daysToMonth366 = new int[]{
                0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
        }

        private static unsafe void RunEagerClassConstructors(IntPtr cctorTableStart, IntPtr cctorTableEnd)
        {
            for (IntPtr* tab = (IntPtr*)cctorTableStart; tab < (IntPtr*)cctorTableEnd; tab++)
            {
                ((delegate*<void>)(*tab))();
            }
        }

        private static void* ReadRelPtr32(void* address) => (byte*)address + *(int*)address;

        private static unsafe void InitializeStatics(IntPtr rgnStart, IntPtr rgnEnd)
        {
            var rgnEndPtr = (byte*)rgnEnd;
            for (byte* block = (byte*)rgnStart; block < rgnEndPtr; block += SupportsRelativePointers ? sizeof(int) : sizeof(nint))
            {
                IntPtr* pBlock = SupportsRelativePointers ? (IntPtr*)ReadRelPtr32(block) : *(IntPtr**)block;
                nint blockAddr = SupportsRelativePointers ? (nint)ReadRelPtr32(pBlock) : *pBlock;
                if ((blockAddr & GCStaticRegionConstants.Uninitialized) == GCStaticRegionConstants.Uninitialized)
                {
                    var obj = RhpNewFast((EEType*)(blockAddr & ~GCStaticRegionConstants.Mask));

                    if ((blockAddr & GCStaticRegionConstants.HasPreInitializedData) == GCStaticRegionConstants.HasPreInitializedData)
                    {
                        //IntPtr pPreInitDataAddr = *(pBlock + 1);
                        void* pPreInitDataAddr = SupportsRelativePointers ? ReadRelPtr32((int*)pBlock + 1) : (void*)*(pBlock + 1);
                        fixed (byte* p = &obj.GetRawData())
                        {
                            MemCpy(p, (byte*)pPreInitDataAddr, obj.GetRawDataSize());
                        }
                    }

                    var handle = malloc((ulong)sizeof(IntPtr));
                    *(IntPtr*)handle = Unsafe.As<object, IntPtr>(ref obj);
                    *pBlock = handle;
                }
            }
        }
    }
}