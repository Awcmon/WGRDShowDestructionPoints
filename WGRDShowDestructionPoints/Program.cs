using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace WGRDShowDestructionPoints
{
	class Program
	{
		const int PROCESS_WM_READ = 0x0010;

		[DllImport("kernel32.dll")]
		static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern Int32 CloseHandle(IntPtr hProcess);

		static void Main(string[] args)
		{
			Process[] wargames = Process.GetProcessesByName("wargame3"); //pray there isnt another program called wargame3.exe
			if(wargames.Length < 1)
			{
				Console.WriteLine("Wargame needs to be running first.");
				return;
			}
			Process wargame = wargames[0]; //just pick the first instance.

			IntPtr handle = OpenProcess(PROCESS_WM_READ, false, wargame.Id);

			IntPtr bytesRead;
			byte[] buffer = new byte[4];

			IntPtr baseAddr = wargame.MainModule.BaseAddress;

			int p1Kills = 0;
			int p2Kills = 0;

			int newKills = 0;

			bool shouldRefresh = true;

			while(true)
			{
				IntPtr kills = IntPtr.Add(baseAddr, 0x1D1FE0C);
				ReadProcessMemory(handle, kills, buffer, buffer.Length, out bytesRead);

				IntPtr p1KillsPPP = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(buffer, 0) + 0x10);
				ReadProcessMemory(handle, p1KillsPPP, buffer, buffer.Length, out bytesRead);

				IntPtr p1KillsPP = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(buffer, 0) + 0x134);
				ReadProcessMemory(handle, p1KillsPP, buffer, buffer.Length, out bytesRead);

				IntPtr p1KillsP = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(buffer, 0) + 0x30);
				ReadProcessMemory(handle, p1KillsP, buffer, buffer.Length, out bytesRead);

				newKills = BitConverter.ToInt32(buffer, 0);
				if(newKills != p1Kills)
				{
					p1Kills = newKills;
					shouldRefresh = true;
				}

				ReadProcessMemory(handle, kills, buffer, buffer.Length, out bytesRead); //read the mem addr pointed at by "kills" back into the buffer

				IntPtr p2KillsPPP = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(buffer, 0) + 0x14);
				ReadProcessMemory(handle, p2KillsPPP, buffer, buffer.Length, out bytesRead);

				IntPtr p2KillsPP = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(buffer, 0) + 0x134);
				ReadProcessMemory(handle, p2KillsPP, buffer, buffer.Length, out bytesRead);

				IntPtr p2KillsP = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(buffer, 0) + 0x30);
				ReadProcessMemory(handle, p2KillsP, buffer, buffer.Length, out bytesRead);

				newKills = BitConverter.ToInt32(buffer, 0);
				if (newKills != p2Kills)
				{
					p2Kills = newKills;
					shouldRefresh = true;
				}

				if(shouldRefresh)
				{
					Console.Clear();
					Console.WriteLine("P1 Kills: " + p1Kills);
					Console.WriteLine("P2 Kills: " + p2Kills);
				}

				shouldRefresh = false;

				Thread.Sleep(100);
			}
		}
	}
}
