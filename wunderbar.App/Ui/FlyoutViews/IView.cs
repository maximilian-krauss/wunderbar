using System;
using wunderbar.App.Core;

namespace wunderbar.App.Ui.FlyoutViews {
	public interface IView {
		event EventHandler<ShowViewEventArgs> ShowView;

		string Title { get; }
		applicationSession Session { get; set; }

		void ViewLoaded(object args);

		bool SupportsBack { get; }
		void GoBack();

		string ActionName { get; }
		Action Action { get; set; }
	}
}