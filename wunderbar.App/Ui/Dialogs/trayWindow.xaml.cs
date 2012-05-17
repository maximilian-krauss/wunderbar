using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using wunderbar.App.Core;

namespace wunderbar.App.Ui.Dialogs {
	/// <summary>
	/// Interaction logic for trayWindow.xaml
	/// </summary>
	public partial class trayWindow : Window {
		private readonly applicationSession _session;
		public trayWindow() {
			InitializeComponent();
			_session = new applicationSession(this);
			_session.runApplication();
		}
	}
}