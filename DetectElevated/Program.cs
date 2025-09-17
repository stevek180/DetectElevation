using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;
using System.Security.Principal;

class Program
{
    const int TokenElevation = 20;
    const uint TOKEN_QUERY = 0x0008;

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, IntPtr TokenInformation, int TokenInformationLength, out int ReturnLength);
    
    public enum ElevationStatus
    {
        NotElevated,
        Elevated,
        AssumeElevated
    }
    static ElevationStatus IsProcessElevated(Process process)
{
    try
    {
        if (!OpenProcessToken(process.Handle, TOKEN_QUERY, out IntPtr tokenHandle))
        {
            int error = Marshal.GetLastWin32Error();
            if (error == 5) // Access Denied
                return ElevationStatus.AssumeElevated;

            return ElevationStatus.AssumeElevated;
        }

        IntPtr elevation = Marshal.AllocHGlobal(sizeof(int));
        bool success = GetTokenInformation(tokenHandle, TokenElevation, elevation, sizeof(int), out _);
        bool isElevated = Marshal.ReadInt32(elevation) != 0;

        Marshal.FreeHGlobal(elevation);
        return success && isElevated ? ElevationStatus.Elevated : ElevationStatus.NotElevated;
    }
    catch
    {
        return ElevationStatus.AssumeElevated;
    }
}

    static string GetProcessOwner(int processId)
    {
        try
        {
            string query = $"SELECT * FROM Win32_Process WHERE ProcessId = {processId}";
            using var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject obj in searcher.Get())
            {
                var outParams = new string[2];
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", outParams));
                if (returnVal == 0)
                {
                    return $"{outParams[1]}\\{outParams[0]}"; // Domain\User
                }
            }
        }
        catch
        {
            // Ignore errors
        }
        return null;
    }

    static void Main(string[] args)
    {
        string targetProcessName;
        if (args.Length != 0)
        {
            targetProcessName = args[0];

        }
        else
        {
            Console.WriteLine("Please enter a process name as an argument.");
            return;
        }
        

        
        var currentUser = WindowsIdentity.GetCurrent().Name;

        foreach (var proc in Process.GetProcessesByName(targetProcessName))
        {
            string owner = GetProcessOwner(proc.Id);
            Console.WriteLine($"Process ID: {proc.Id}, Owner: {owner}");

            Console.WriteLine($"ProcessName = {targetProcessName}  Status = {IsProcessElevated(proc)}");
        }
    }
}
