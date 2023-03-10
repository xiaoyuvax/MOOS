internal static unsafe class NativeCS
{
    public static void Stosd(void* p, uint value, ulong count)
    {
        ulong* destp = (ulong*)p;
        ulong data = value;
        while (count-- > 0) *destp++ = data;
    }

    public static void Stosb(void* p, byte value, ulong count)
    {
        byte* destp = (byte*)p;
        while (count-- > 0) *destp++ = value;
    }

    public static void Movsb(void* dest, void* source, ulong count)
    {
        byte* destp = (byte*)dest;
        byte* sourcep = (byte*)source;

        while (count-- > 0) *destp++ = *sourcep++;
    }

    public static void Movsd(uint* dest, uint* source, ulong count)
    {
        while (count-- > 0) *dest++ = *source++;
    }
}