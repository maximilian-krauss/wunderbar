using System;

namespace wunderbar.App.Ui.FlyoutViews {
	public class ShowViewEventArgs : EventArgs {
		public ShowViewEventArgs(IView view)
			: this(view, null) {}

		public ShowViewEventArgs(IView view, object args) {
			View = view;
			Arguments = args;
		}

		public IView View { get; private set; }
		public object Arguments { get; private set; }
	}
}