using System;
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
											ToolTipText = string.Format("{0} version {1}", session.applicationName, session.displayVersion)
										};
			_brushConverter = new BrushConverter();
			initializeAnimation();
			initializeMenu();
			Session.trayContextUpdateRequired += (o, e) => updateMenu();
			Session.Settings.PropertyChanged += Settings_PropertyChanged;
		}

		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "showDueTasksInTrayIcon")
				showDueTasksInTrayIcon();
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
			mnuAbout.Click += (o, e) => {
			                  	var dialog = new Ui.Dialogs.aboutDialog {DataContext = Session};
			                  	dialog.ShowDialog();
			                  };
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
			if (state == trayContextTypes.loginRequired) {
				_trayIcon.ContextMenu.Items.Insert(0, mnuLogin);
			}
			else {
				_trayIcon.ContextMenu.Items.Insert(0, mnuLogout);
				_trayIcon.ContextMenu.Items.Insert(0, mnuSynchronize);

				//Add Tasks and Lists
				foreach (var list in Session.wunderClient.Lists.Where(l => l.Deleted == 0).OrderByDescending(l => l.Position)) {
					_trayIcon.ContextMenu.Items.Insert(0, buildTaskTree(list));
				}

				addDueTasks();
			}
			addError();
			showDueTasksInTrayIcon(); //This needs to be called every time the menu updates
		}

		private MenuItem buildTaskTree(listType list) {
			var listRoot = new MenuItem {Header = list.Name};
			if (list.Inbox == 1)
				listRoot.Icon = readImageControlFromResource("Tasks/inbox");
			
			//Add 'Add new Task' Item
			var mnuAddNewTask = new MenuItem {Header = "Add new task", Icon = readImageControlFromResource("Tasks/plus")};
			mnuAddNewTask.Click += (o, e) => Session.showTask(list.Id);
			listRoot.Items.Add(mnuAddNewTask);

			foreach (var task in Session.wunderClient.Tasks.Where(t => t.listId == list.Id && t.Done == 0 && t.Deleted == 0).OrderBy(t => t.Position))
				listRoot.Items.Add(createTaskMenuItem(task));

			//Only add the Separator if there are one or more tasks in this list
			if (listRoot.Items.Count > 1) 
				listRoot.Items.Insert(1, new Separator());

			return listRoot;
		}

		private void addDueTasks() {
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

			if (tasksAdded > 0)
				_trayIcon.ContextMenu.Items.Insert(tasksAdded, new Separator());
		}
		
		public void notifyError(string message) {
			_trayIcon.ShowBalloonTip(Session.applicationName, message, BalloonIcon.Error);
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