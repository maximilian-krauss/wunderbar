using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;

/*
	LICENSE
	====================================================
	WPF HotKey Stuff
	(c) Henning Dieterichs (http://www.codeproject.com/Tips/274003/Global-Hotkeys-in-WPF#)
	Licensed under the terms and conditions of the Code Project Open License (CPOL) (http://www.codeproject.com/info/cpol10.aspx)
 */

namespace wunderbar.App.Core {
	/// <summary>
	/// Represents an hotKey
	/// </summary>
	[Serializable]
	public class hotKey : INotifyPropertyChanged, ISerializable, IEquatable<hotKey> {
		/// <summary>
		/// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
		/// </summary>
		public hotKey() { }

		/// <summary>
		/// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
		public hotKey(Key key, ModifierKeys modifiers) : this(key, modifiers, true) { }

		/// <summary>
		/// Creates an HotKey object. This instance has to be registered in an HotKeyHost.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
		/// <param name="enabled">Specifies whether the HotKey will be enabled when registered to an HotKeyHost</param>
		public hotKey(Key key, ModifierKeys modifiers, bool enabled) {
			Key = key;
			Modifiers = modifiers;
			Enabled = enabled;
		}


		private Key key;
		/// <summary>
		/// The Key. Must not be null when registering to an HotKeyHost.
		/// </summary>
		public Key Key {
			get { return key; }
			set {
				if (key != value) {
					key = value;
					OnPropertyChanged("Key");
				}
			}
		}

		private ModifierKeys modifiers;
		/// <summary>
		/// The modifier. Multiple modifiers can be combined with or.
		/// </summary>
		public ModifierKeys Modifiers {
			get { return modifiers; }
			set {
				if (modifiers != value) {
					modifiers = value;
					OnPropertyChanged("Modifiers");
				}
			}
		}

		private bool enabled;
		public bool Enabled {
			get { return enabled; }
			set {
				if (value != enabled) {
					enabled = value;
					OnPropertyChanged("Enabled");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}


		public override bool Equals(object obj) {
			hotKey hotKey = obj as hotKey;
			if (hotKey != null)
				return Equals(hotKey);
			else
				return false;
		}

		public bool Equals(hotKey other) {
			return (Key == other.Key && Modifiers == other.Modifiers);
		}

		public override int GetHashCode() {
			return (int)Modifiers + 10 * (int)Key;
		}

		public override string ToString() {
			return string.Format("{0} + {1} ({2}Enabled)", Key, Modifiers, Enabled ? "" : "Not ");
		}

		/// <summary>
		/// Will be raised if the hotkey is pressed (works only if registed in HotKeyHost)
		/// </summary>
		public event EventHandler<hotKeyEventArgs> HotKeyPressed;

		protected virtual void OnHotKeyPress() {
			if (HotKeyPressed != null)
				HotKeyPressed(this, new hotKeyEventArgs(this));
		}

		internal void RaiseOnHotKeyPressed() {
			OnHotKeyPress();
		}


		protected hotKey(SerializationInfo info, StreamingContext context) {
			Key = (Key)info.GetValue("Key", typeof(Key));
			Modifiers = (ModifierKeys)info.GetValue("Modifiers", typeof(ModifierKeys));
			Enabled = info.GetBoolean("Enabled");
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("Key", Key, typeof(Key));
			info.AddValue("Modifiers", Modifiers, typeof(ModifierKeys));
			info.AddValue("Enabled", Enabled);
		}
	}
}
