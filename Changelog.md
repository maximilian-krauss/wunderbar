Changelog
=========

### wunderbar 1.0 b4 (not released yet!)
* [Feature] Sort tasks by due date
* [Feature] wunderbar displays a traybubble if the user openend the application the first time.

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