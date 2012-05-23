using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using wunderbar.Api.dataContracts;
using wunderbar.App.Data;

namespace wunderbar.App.Core {
	internal sealed class trayController : baseController {
		private const int _maxHeaderLength = 25;

		private readonly TaskbarIcon _trayIcon;
		private List<Control> _persistentMenuItems; //List of Items which will never removed from the ContextMenu.
		private System.Drawing.Image[] _loadingImages;
		private Timer _animationTimer;
		private int _currentAnimationIndex;
		private Icon _animationBaseIcon;
		
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
			                            	ToolTipText = string.Format("{0} Version {1}", session.applicationName, session.applicationVersion)
			                            };
			initializeMenu();
			initializeAnimation();
			Session.trayContextUpdateRequired += (o, e) => updateMenu();
		}

		/// <summary>Initializes the ContextMenu for the first time.</summary>
		private void initializeMenu() {
			_trayIcon.ContextMenu = new ContextMenu();

			//Persistent Items
			mnuExit = new MenuItem {Header = "Exit"};
			mnuExit.Click += (o, e) => Session.closeApplication();

			mnuSettings = new MenuItem {Header = "Settings...", IsEnabled = false};
			mnuAbout = new MenuItem {Header = "About..."};
			mnuAbout.Click += (o, e) => Ui.Dialogs.aboutDialog.Instance.ShowDialog();
			mnuSeparatorMain = new Separator();

			//Non-Persistent Items
			mnuLogin = new MenuItem {Header = "Login", Icon = readImageControlFromResource("auth")};
			mnuLogin.Click += (o, e) => Session.Login();

			mnuLogout = new MenuItem {Header = "Logout", Icon = readImageControlFromResource("auth")};
			mnuLogout.Click += (o, e) => Session.Logout();

			mnuSynchronize = new MenuItem {Header = "Synchronize", Icon = readImageControlFromResource("cloud_sync")};
			mnuSynchronize.Click += (o, e) => Session.Synchronize();
			
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
			_loadingImages = new System.Drawing.Image[9];
			for (int i = 0; i <= 8; i++)
				_loadingImages[i] = readImageFromResource(string.Format("loadingAnimation/loadingAnimation{0}", i));
			
			//Initialize Animationtimer
			_animationTimer = new Timer(85);
			_animationTimer.Elapsed += _animationTimer_Elapsed;
		}

		void _animationTimer_Elapsed(object sender, ElapsedEventArgs e) {

			//TODO: Cache this generated Icons, this is wasted performance...
			using (var bitmap = new Bitmap(16, 16)) {
				using (var g = Graphics.FromImage(bitmap)) {
					g.DrawImage(_animationBaseIcon.ToBitmap(), 0, 0, 16, 16);
					g.DrawImage(_loadingImages[_currentAnimationIndex], 0, 0, 16, 16);
				}
				_trayIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
			}
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
		}

		private MenuItem buildTaskTree(listType list) {
			var listRoot = new MenuItem {Header = list.Name};
			
			//Add 'Add new Task' Item
			var mnuAddNewTask = new MenuItem {Header = "Add new task", Icon = readImageControlFromResource("Tasks/plus")};
			mnuAddNewTask.Click += (o, e) => Session.showTask(list.Id);
			listRoot.Items.Add(mnuAddNewTask);

			foreach (var task in Session.wunderClient.Tasks.Where(t => t.listId == list.Id && t.Done == 0 && t.Deleted == 0).OrderBy(t => t.Position))
				listRoot.Items.Add(createTaskMenuItem(task));

			//Only add the Separator if there are one or more tasks in this list
			if(listRoot.Items.Count > 1)
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
			//TODO: If task is overdue, mark menuitem red
			var mnuTask = new MenuItem {
				Header = task.Name,
				DataContext = task,
				Icon = (task.Important == 1 ? readImageControlFromResource("Tasks/important") : null)
			};

			var taskLocal = task; //Looks awkward but it's important to copy that variable, see: http://confluence.jetbrains.net/display/ReSharper/Access+to+modified+closure
			mnuTask.Click += (o, e) => Session.showTask(taskLocal);
			return mnuTask;
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