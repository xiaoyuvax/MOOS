namespace System
{
    [Flags]
    public enum ConsoleModifiers:byte
    {
        None = 0,
        Alt = 1,
        Shift = 2,
        CapsLock = 3,
        Control = 4,
    }
}