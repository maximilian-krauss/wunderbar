using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Interop;

/*
	LICENSE
	====================================================
	WPF HotKey Stuff
	(c) Henning Dieterichs (http://www.codeproject.com/Tips/274003/Global-Hotkeys-in-WPF#)
	Licensed under the terms and conditions of the Code Project Open License (CPOL) (http://www.codeproject.com/info/cpol10.aspx)
 */

namespace wunderbar.App.Core {

	/// <summary>
	/// The HotKeyHost needed for working with hotKeys.
	/// </summary>
	public sealed class hotKeyHost : IDisposable {
		/// <summary>
		/// Creates a new HotKeyHost
		/// </summary>
		/// <param name="hwndSource">The handle of the window. Must not be null.</param>
		public hotKeyHost(HwndSource hwndSource) {
			if (hwndSource == null)
				throw new ArgumentNullException("hwndSource");

			this.hook = new HwndSourceHook(WndProc);
			this.hwndSource = hwndSource;
			hwndSource.AddHook(hook);
		}

		#region HotKey Interop

		private const int WM_HotKey = 786;

		[DllImport("user32", CharSet = CharSet.Ansi,
				   SetLastError = true, ExactSpelling = true)]
		private static extern int RegisterHotKey(IntPtr hwnd,
				int id, int modifiers, int key);

		[DllImport("user32", CharSet = CharSet.Ansi,
				   SetLastError = true, ExactSpelling = true)]
		private static extern int UnregisterHotKey(IntPtr hwnd, int id);

		#endregion

		#region Interop-Encapsulation

		private HwndSourceHook hook;
		private HwndSource hwndSource;

		private void RegisterHotKey(int id, hotKey hotKey) {
			if ((int)hwndSource.Handle != 0) {
				RegisterHotKey(hwndSource.Handle, id, (int)hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
				int error = Marshal.GetLastWin32Error();
				if (error != 0) {
					Exception e = new Win32Exception(error);

					if (error == 1409)
						throw new hotKeyAlreadyRegisteredException(e.Message, hotKey, e);
					else
						throw e;
				}
			}
			else
				throw new InvalidOperationException("Handle is invalid");
		}

		private void UnregisterHotKey(int id) {
			if ((int)hwndSource.Handle != 0) {
				UnregisterHotKey(hwndSource.Handle, id);
				int error = Marshal.GetLastWin32Error();
				if (error != 0)
					throw new Win32Exception(error);
			}
		}

		#endregion

		/// <summary>
		/// Will be raised if any registered hotKey is pressed
		/// </summary>
		public event EventHandler<hotKeyEventArgs> HotKeyPressed;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == WM_HotKey) {
				if (hotKeys.ContainsKey((int)wParam)) {
					hotKey h = hotKeys[(int)wParam];
					h.RaiseOnHotKeyPressed();
					if (HotKeyPressed != null)
						HotKeyPressed(this, new hotKeyEventArgs(h));
				}
			}

			return new IntPtr(0);
		}


		void hotKey_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			var kvPair = hotKeys.FirstOrDefault(h => h.Value == sender);
			if (kvPair.Value != null) {
				if (e.PropertyName == "Enabled") {
					if (kvPair.Value.Enabled)
						RegisterHotKey(kvPair.Key, kvPair.Value);
					else
						UnregisterHotKey(kvPair.Key);
				}
				else if (e.PropertyName == "Key" || e.PropertyName == "Modifiers") {
					if (kvPair.Value.Enabled) {
						UnregisterHotKey(kvPair.Key);
						RegisterHotKey(kvPair.Key, kvPair.Value);
					}
				}
			}
		}


		private Dictionary<int, hotKey> hotKeys = new Dictionary<int, hotKey>();


		public class SerialCounter {
			public SerialCounter(int start) {
				Current = start;
			}

			public int Current { get; private set; }

			public int Next() {
				return ++Current;
			}
		}

		/// <summary>
		/// All registered hotKeys
		/// </summary>
		public IEnumerable<hotKey> HotKeys { get { return hotKeys.Values; } }


		private static readonly SerialCounter idGen = new SerialCounter(1); //Annotation: Can be replaced with "Random"-class

		/// <summary>
		/// Adds an hotKey.
		/// </summary>
		/// <param name="hotKey">The hotKey which will be added. Must not be null and can be registed only once.</param>
		public void AddHotKey(hotKey hotKey) {
			if (hotKey == null)
				throw new ArgumentNullException("value");
			if (hotKey.Key == 0)
				throw new ArgumentNullException("value.Key");
			if (hotKeys.ContainsValue(hotKey))
				throw new hotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);

			int id = idGen.Next();
			if (hotKey.Enabled)
				RegisterHotKey(id, hotKey);
			hotKey.PropertyChanged += hotKey_PropertyChanged;
			hotKeys[id] = hotKey;
		}

		/// <summary>
		/// Removes an hotKey
		/// </summary>
		/// <param name="hotKey">The hotKey to be removed</param>
		/// <returns>True if success, otherwise false</returns>
		public bool RemoveHotKey(hotKey hotKey) {
			var kvPair = hotKeys.FirstOrDefault(h => h.Value == hotKey);
			if (kvPair.Value != null) {
				kvPair.Value.PropertyChanged -= hotKey_PropertyChanged;
				if (kvPair.Value.Enabled)
					UnregisterHotKey(kvPair.Key);
				return hotKeys.Remove(kvPair.Key);
			}
			return false;
		}


		#region Destructor

		private bool disposed;

		private void Dispose(bool disposing) {
			if (disposed)
				return;

			if (disposing) {
				hwndSource.RemoveHook(hook);
			}

			for (int i = hotKeys.Count - 1; i >= 0; i--) {
				RemoveHotKey(hotKeys.Values.ElementAt(i));
			}


			disposed = true;
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		~hotKeyHost() {
			this.Dispose(false);
		}

		#endregion
	}
}
