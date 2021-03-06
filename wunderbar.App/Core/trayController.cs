﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using wunderbar.Api.dataContracts;
using wunderbar.App.Data;
using System.Windows.Media;
using FontStyle = System.Drawing.FontStyle;

namespace wunderbar.App.Core {
	internal sealed class trayController : baseController {
		private const int _maxHeaderLength = 25;
		private const string _trayTitleTemplate = "{0} version {1}";

		private readonly TaskbarIcon _trayIcon;
		private readonly BrushConverter _brushConverter;
		private const string _overdueColor = "#7F0000";
		private List<Control> _persistentMenuItems; //List of items which will never removed from the ContextMenu.
		private Icon[] _animationIcons;
		private Timer _animationTimer;
		private int _currentAnimationIndex;
		private Icon _animationBaseIcon;
		private System.Drawing.Image _plainTrayBaseImage;

		private MenuItem mnuExit;
		private MenuItem mnuSettings;
		private MenuItem mnuAbout;
		private Separator mnuSeparatorMain;
		private MenuItem mnuLogin;
		private MenuItem mnuLogout;
		private MenuItem mnuSynchronize;

		public trayController(applicationSession session):base(session) {
			_trayIcon = new TaskbarIcon {
											Icon = readIconFromResource("tray"),
											ToolTipText = string.Format(_trayTitleTemplate, session.applicationName, session.displayVersion)
										};
			_trayIcon.TrayLeftMouseUp += _trayIcon_TrayLeftMouseUp;
			_brushConverter = new BrushConverter();
			initializeAnimation();
			initializeMenu();
			Session.trayContextUpdateRequired += (o, e) => updateMenu();
			Session.Settings.PropertyChanged += Settings_PropertyChanged;
		}

		void _trayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e) {
			if (_animationTimer.Enabled) //Do nothing when an operation is running
				return;

			Session.showFlyout();
		}

		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "showDueTasksInTrayIcon")
				showDueTasksInTrayIcon();
			if (e.PropertyName == "showDueTasksOnTop" || e.PropertyName == "sortByDueDate")
				updateMenu();
		}

		/// <summary>Initializes the ContextMenu for the first time.</summary>
		private void initializeMenu() {
			_trayIcon.ContextMenu = new ContextMenu();

			//Persistent Items
			mnuExit = new MenuItem {Header = "Exit"};
			mnuExit.Click += (o, e) => Session.closeApplication();

			mnuSettings = new MenuItem {Header = "Settings..."};
			mnuSettings.Click += (o, e) => new Ui.Dialogs.settingsDialog {DataContext = Session.Settings}.ShowDialog();

			mnuAbout = new MenuItem {Header = "About..."};
			mnuAbout.Click += (o, e) => new Ui.Dialogs.aboutDialog {DataContext = Session}.ShowDialog();
			mnuSeparatorMain = new Separator();

			//Non-Persistent Items
			mnuLogin = new MenuItem {Header = "Login", Icon = readImageControlFromResource("auth")};
			mnuLogin.Click += (o, e) => Session.Login();

			mnuLogout = new MenuItem {Header = "Logout", Icon = readImageControlFromResource("auth")};
			mnuLogout.Click += (o, e) => Session.Logout();

			mnuSynchronize = new MenuItem {Header = "Synchronize", Icon = readImageControlFromResource("cloud_sync")};
			mnuSynchronize.Click += (o, e) =>
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Session.Synchronize()));
			
			_trayIcon.ContextMenu.Items.Add(mnuSeparatorMain);
			_trayIcon.ContextMenu.Items.Add(mnuAbout);
			_trayIcon.ContextMenu.Items.Add(mnuSettings);
			_trayIcon.ContextMenu.Items.Add(mnuExit);
			_persistentMenuItems = new List<Control>(new Control[] {mnuExit, mnuSettings, mnuAbout, mnuSeparatorMain});
			updateMenu();
		}
		private void initializeAnimation() {

			//Read Images from Resources
			_animationBaseIcon = readIconFromResource("tray");
			_plainTrayBaseImage = readImageFromResource("plain-tray");

			//Create animation images
			_animationIcons = new Icon[9];
			for (int i = 0; i <= 8; i++) {
				using (var bitmap = new Bitmap(16, 16)) {
					using (var g = Graphics.FromImage(bitmap)) {
						g.DrawImage(_animationBaseIcon.ToBitmap(), 0, 0, 16, 16);
						g.DrawImage(readImageFromResource(string.Format("loadingAnimation/loadingAnimation{0}", i)), 0, 0, 16, 16);
					}
					_animationIcons[i] = Icon.FromHandle(bitmap.GetHicon());
				}
			}

			//Initialize Animationtimer
			_animationTimer = new Timer(85);
			_animationTimer.Elapsed += _animationTimer_Elapsed;
		}

		void _animationTimer_Elapsed(object sender, ElapsedEventArgs e) {
			_trayIcon.Icon = _animationIcons[_currentAnimationIndex];
			_currentAnimationIndex++;
			if (_currentAnimationIndex > 8)
				_currentAnimationIndex = 0;
		}

		public void startAnimation() {
			if(!_animationTimer.Enabled)
				_animationTimer.Start();
		}
		public void stopAnimation() {
			_animationTimer.Stop();
			_trayIcon.Icon = _animationBaseIcon;
		}

		/// <summary>Updates the ContextMenu based on the State the Session returns.</summary>
		private void updateMenu() {
			//Remove all non-persistent items
			for (int i = _trayIcon.ContextMenu.Items.Count - 1; i >= 0; i--)
				if (!_persistentMenuItems.Contains((Control) _trayIcon.ContextMenu.Items[i]))
					_trayIcon.ContextMenu.Items.RemoveAt(i);

			var state = Session.trayContextType;

			//Set the tooltip title of the trayicon
			_trayIcon.ToolTipText = string.Format(_trayTitleTemplate, Session.applicationName, Session.displayVersion);
			if (Session.wunderClient.loggedIn)
				_trayIcon.ToolTipText = _trayIcon.ToolTipText + "\r\n" + Session.Settings.eMail;

			if (state == trayContextTypes.loginRequired) {
				_trayIcon.ContextMenu.Items.Insert(0, mnuLogin);
			}
			else {
				_trayIcon.ContextMenu.Items.Insert(0, mnuLogout);
				_trayIcon.ContextMenu.Items.Insert(0, mnuSynchronize);

				addDueTasks();
			}
			addError();
			showDueTasksInTrayIcon(); //This needs to be called every time the menu updates
		}

		private void addDueTasks() {

			if (!Session.Settings.showDueTasksOnTop)
				return;

			int tasksAdded = 0;
			foreach (var task in Session.wunderClient.Tasks.dueTasks) {
				tasksAdded++;
				var mnuTask = createTaskMenuItem(task);
				//Make nice headerlength
				string header = (string) mnuTask.Header;
				if (header.Length > _maxHeaderLength)
					header = header.Substring(0, _maxHeaderLength) + "...";
				mnuTask.Header = header;
				_trayIcon.ContextMenu.Items.Insert(0, mnuTask);
			}

			if (tasksAdded == 0) {
				_trayIcon.ContextMenu.Items.Insert(0, new MenuItem {Header = "Yay! Nothing todo :)", IsEnabled = false});
				tasksAdded++;
			}

			_trayIcon.ContextMenu.Items.Insert(tasksAdded, new Separator());
		}
		
		public void notifyError(string message) {
			_trayIcon.ShowBalloonTip(Session.applicationName, message, BalloonIcon.Error);
		}

		public void notifyInformation(string message) {
			notifyInformation(message, Session.applicationName);
		}
		public void notifyInformation(string message, string title) {
			_trayIcon.ShowBalloonTip(title, message, BalloonIcon.Info);
		}

		/// <summary>Adds an "Error"-item on top of the ContextMenu if an error occoured</summary>
		private void addError() {
			if (Session.lastError == null)
				return;

			_trayIcon.ContextMenu.Items.Insert(0, new MenuItem {
																Header = string.Format("Couldn't connect: {0}", Session.lastError.Message),
																Icon = readImageControlFromResource("error")
															   });
		}

		private MenuItem createTaskMenuItem(taskType task) {
			var mnuTask = new MenuItem {
				Header = task.Name,
				DataContext = task,
				Icon = (task.Important == 1 ? readImageControlFromResource("Tasks/important") : null)
			};
			if (task.isOverdue)
				mnuTask.Foreground = (System.Windows.Media.Brush) _brushConverter.ConvertFrom(_overdueColor);

			var taskLocal = task; //Looks awkward but it's important to copy that variable, see: http://confluence.jetbrains.net/display/ReSharper/Access+to+modified+closure
			mnuTask.Click += (o, e) => Session.showTask(taskLocal);
			return mnuTask;
		}

		private void showDueTasksInTrayIcon() {
			if (_animationTimer != null && _animationTimer.Enabled) //Do nothing when the animation is enabled :-)
				return;

			if (!Session.wunderClient.loggedIn || !Session.Settings.showDueTasksInTrayIcon || !Session.wunderClient.Tasks.dueTasks.Any()) {
				_trayIcon.Icon = _animationBaseIcon;
				return;
			}

			using (var bitmap = new Bitmap(16, 16)) {
				using (var g = Graphics.FromImage(bitmap)) {
					g.DrawImage(_plainTrayBaseImage, 0, 0, 16, 16);
					using (var font = new Font("Verdana", 9, FontStyle.Bold, GraphicsUnit.Pixel)) {
						var textRect = new RectangleF(-4, 0, 24, 16);
						var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
						int dueTasks = Session.wunderClient.Tasks.dueTasks.Count();
						g.DrawString(
							dueTasks >= 100 ? "+" : dueTasks.ToString(CultureInfo.InvariantCulture),
							font,
							System.Drawing.Brushes.White,
							textRect,
							sf
							);
					}
				}
				_trayIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
			}

		}

		private Icon readIconFromResource(string iconName) {
			Stream iconStream = Application.GetResourceStream(
				new Uri(string.Format("pack://application:,,,/wunderbar.App;component/Gfx/Icons/{0}.ico", iconName))).Stream;
			return new Icon(iconStream);
		}
		private System.Drawing.Image readImageFromResource(string resourceName) {
			Stream imageStream = Application.GetResourceStream(
					new Uri(string.Format("pack://application:,,,/wunderbar.App;component/Gfx/Images/{0}.png", resourceName))).Stream;
			return System.Drawing.Image.FromStream(imageStream);
		}
		private System.Windows.Controls.Image readImageControlFromResource(string resourceName) {
			return new System.Windows.Controls.Image {
														Source =
															new BitmapImage(
															new Uri(string.Format("pack://application:,,,/wunderbar.App;component/Gfx/Images/{0}.png",resourceName)))
													 };
		}

		public override void Dispose() {
			base.Dispose();
			_animationTimer.Stop();
			_animationTimer.Dispose();
			_trayIcon.Dispose();
		}

	}
}