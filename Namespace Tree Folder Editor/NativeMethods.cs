using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NamespaceTreeFolderEditor
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, ENUMRESNAMEPROC lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        [DllImport("psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetMappedFileName(IntPtr hProcess, IntPtr lpv, StringBuilder lpFilename, int nSize);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int PickIconDlg(IntPtr hwndOwner, System.Text.StringBuilder lpstrFile, int nMaxFile, ref int lpdwIconIndex);
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
    internal delegate bool ENUMRESNAMEPROC(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
}