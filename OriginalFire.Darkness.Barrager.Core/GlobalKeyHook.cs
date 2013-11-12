using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OriginalFire.Darkness.Barrager.Core
{
	/// <summary>
	/// 封装全局键盘钩子操作。
	/// </summary>
	public class GlobalKeyHook : IDisposable
	{
		private static GlobalKeyHook instance;

		/// <summary>
		/// 获取唯一实例。
		/// </summary>
		public static GlobalKeyHook Instance
		{
			get
			{
				if (instance == null)
					instance = new GlobalKeyHook();
				return instance;
			}
		}

		/// <summary>
		/// 释放此GlobalKeyHook所持有的实例。
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		/// <summary>
		/// 表示按键处理器。
		/// </summary>
		/// <param name="key">将要处理的按键。</param>
		/// <returns>如果事件应该被截获并停止传递，则为true；否则为false。</returns>
		public delegate bool KeyProcessor(Keys key);

		private GlobalKeyHook()
		{
			this.processors = new Dictionary<Keys, KeyProcessor>();
			this.procDelegate = new ApiHelper.HookProc(ProcessKey);
		}

		private GlobalKeyHook(IDictionary<Keys, KeyProcessor> processors)
		{
			this.processors = new Dictionary<Keys, KeyProcessor>(processors);
			this.procDelegate = new ApiHelper.HookProc(ProcessKey);
		}

		/// <summary>
		/// 开始全局键盘钩子。
		/// </summary>
		public void Start()
		{
			if (hookId != IntPtr.Zero)
				return;
			hookId = ApiHelper.SetWindowsHookEx(ApiHelper.HookType.WH_KEYBOARD_LL, this.procDelegate, ApiHelper.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
			if (hookId == IntPtr.Zero)
				throw new InvalidOperationException("无法设置消息钩子");
		}

		/// <summary>
		/// 停止全局键盘钩子。
		/// </summary>
		public void Stop()
		{
			if (hookId == IntPtr.Zero)
				return;
			if (!ApiHelper.UnhookWindowsHookEx(hookId))
				throw new InvalidOperationException("无法卸载消息钩子");
			hookId = IntPtr.Zero;
		}

		private IntPtr ProcessKey(Int32 nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				if (wParam == (IntPtr)ApiHelper.WM_KEYDOWN || wParam == (IntPtr)ApiHelper.WM_SYSKEYDOWN)
				{
					ApiHelper.KBDLLHOOKSTRUCT kbdStruct = (ApiHelper.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(ApiHelper.KBDLLHOOKSTRUCT));
					switch ((Keys)kbdStruct.vkCode)
					{
						case Keys.LMenu:
						case Keys.RMenu:
							altKeyDown = true;
							break;
						case Keys.LControlKey:
						case Keys.RControlKey:
							ctrlKeyDown = true;
							break;
						case Keys.LShiftKey:
						case Keys.RShiftKey:
							shiftKeyDown = true;
							break;
						default:
							{
								Keys k = (Keys)kbdStruct.vkCode;
								if (altKeyDown)
									k |= Keys.Alt;
								if (ctrlKeyDown)
									k |= Keys.Control;
								if (shiftKeyDown)
									k |= Keys.Shift;
								if (processors.ContainsKey(k))
									if (processors[k](k))
										return (IntPtr)1;
							}
							break;
					}
				}
				else if (wParam == (IntPtr)ApiHelper.WM_KEYUP || wParam == (IntPtr)ApiHelper.WM_SYSKEYUP)
				{
					ApiHelper.KBDLLHOOKSTRUCT kbdStruct = (ApiHelper.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(ApiHelper.KBDLLHOOKSTRUCT));
					switch ((Keys)kbdStruct.vkCode)
					{
						case Keys.LMenu:
						case Keys.RMenu:
							altKeyDown = false;
							break;
						case Keys.LControlKey:
						case Keys.RControlKey:
							ctrlKeyDown = false;
							break;
						case Keys.LShiftKey:
						case Keys.RShiftKey:
							shiftKeyDown = false;
							break;
					}
				}
			}
			return ApiHelper.CallNextHookEx(hookId, nCode, wParam, lParam);
		}

		/// <summary>
		/// 设置按键处理器。
		/// </summary>
		/// <param name="key">要处理的按键。</param>
		/// <param name="processor">要使用的按键处理器。</param>
		public void SetProcessor(Keys key, KeyProcessor processor)
		{
			if (processor == null)
				throw new ArgumentNullException("processor", "processor不能为空");
			processors[key] = processor;
		}

		/// <summary>
		/// 撤销按键处理器。
		/// </summary>
		/// <param name="key">要处理的按键。</param>
		public void RemoveProcessor(Keys key)
		{
			if (processors.ContainsKey(key))
				processors.Remove(key);
		}

		private bool altKeyDown;

		private bool ctrlKeyDown;

		private bool shiftKeyDown;

		private IntPtr hookId;

		private ApiHelper.HookProc procDelegate;

		private Dictionary<Keys, KeyProcessor> processors;
	}
}
