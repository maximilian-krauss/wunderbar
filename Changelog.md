Changelog
=========

### wunderbar 1.0 b6
* [Feature] Deleting tasks is now possible!
* [Feature] Icon for shared lists (again)
* [Bug] Scrolling with mouse wheel didn't work properly in flyout window
* [Feature] Loginview moved to flyout
* [Feature] Implemented sharing: Share/unshare a list, add/remove dudes from shared lists.
* [Feature] Several UI improvements

### wunderbar 1.0 b5
* [Feature] Huge UI improvements
* [Feature] New flyout window
* [Bug] No done date was set

### wunderbar 1.0 b4
* [Feature] Sort tasks by due date
* [Feature] wunderbar displays a traybubble if the user openend the application the first time.
* [Feature] Global shortcuts for creating a new task and syncing lists and tasks.
* [Feature] wunderbar detects man-in-the-middle attacks with manipulated ssl-certificates (Ok, maybe a bit over the top, but it was interesting to implement :) )
* [Bug] Traywindow dummy appeared sometimes

### wunderbar 1.0 b3
* [Bug] In some cases the transmitted date-value can be null which caused an NullReferenceException.
* [Bug] Problem when inserting more as one task between one syncinterval (epic fail).
* [Bug] If a task is marked as done wunderbar sets now the doneDate-property properly.
* [Feature] It's now possible to configure if due/overdue tasks should appear on top of the contextmenu.
* [Feature] Icon for shared lists
* [Feature] Click on trayicon creates new task
* [Feature] "Cancel"-Button when creating a new task

### wunderbar 1.0 b2
* [Feature] Settingswindow (yay!)
* [Feature] Load wunderbar when windows starts (optional, of course)
* [Feature] wunderbar now logs errors (logdirectory is located here: *%appdata%\wunderbar\1\logs*)
* [Feature] Your "Inbox"-list is now marked with an icon.
* [Bug] Fixed a crash when opening the aboutwindow the second time.
* [Feature] Support for corporate proxyserver with NTLM-Authentication (ISA, I mean you!)
* [Bug] The calendar now closes immediatly when the selection changes.
* [Bug] Fixed some annoying DateTime-bugs
* [Bug] Some dialogs crashed on XP because of an unsupported iconformat.