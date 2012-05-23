using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace wunderbar.App {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			//Trigger Exceptionhandler
			Dispatcher.UnhandledException += Dispatcher_UnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			showExceptionDialog((Exception)e.ExceptionObject);
		}
		void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
			showExceptionDialog(e.Exception);
			e.Handled = true;
		}
		void showExceptionDialog(Exception exc) {
			var dialog = new Ui.Dialogs.exceptionDialog {DataContext = exc};
			dialog.ShowDialog();
			if (MainWindow != null)
				MainWindow.Close();
		}

	}
}
