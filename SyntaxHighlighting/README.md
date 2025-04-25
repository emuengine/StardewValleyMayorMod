# StardewValleyEventsUDL
A user defined language for Stardew Valley events using Notepad++

### Example
![example](https://github.com/user-attachments/assets/606d2428-3525-4c55-9997-d0bb7a72afab)

## How to import into Notepad++

- On the toolbar Select `Language -> User Defined Language -> Define your language...`
- Click the `Import...` button.
- Select the `UDL\StardewValleyEvents.xml` file.
- Restart Notepad++.
- In the `Language` toolbar you can now select `StardewValleyEvents` to enable highlighting.

## How to expand event actions onto new lines
### Manual
- The example shown can be done by simply Find/Replacing `/` with `\n/` to expand out the event actions onto separate lines.
- The opposite can then be done in order to collapse the event actions back to a single line.

### Macro for expanding and inlining
This can also be done using a macro. This can be installed by:
- Editing the shortcuts file which is located at `%AppData%\Notepad++\shortcuts.xml`
- Add the following under the `Macros` element
  ```
          <Macro name="ExpandStardewEvent" Ctrl="yes" Alt="no" Shift="no" Key="69">
            <Action type="3" message="1700" wParam="0" lParam="0" sParam="" />
            <Action type="3" message="1601" wParam="0" lParam="0" sParam="/" />
            <Action type="3" message="1625" wParam="0" lParam="1" sParam="" />
            <Action type="3" message="1602" wParam="0" lParam="0" sParam="\n/" />
            <Action type="3" message="1702" wParam="0" lParam="896" sParam="" />
            <Action type="3" message="1701" wParam="0" lParam="1609" sParam="" />
        </Macro>
        <Macro name="InlineStardewEvent" Ctrl="yes" Alt="no" Shift="yes" Key="69">
            <Action type="3" message="1700" wParam="0" lParam="0" sParam="" />
            <Action type="3" message="1601" wParam="0" lParam="0" sParam="[\r\n]+" />
            <Action type="3" message="1625" wParam="0" lParam="2" sParam="" />
            <Action type="3" message="1602" wParam="0" lParam="0" sParam="" />
            <Action type="3" message="1702" wParam="0" lParam="896" sParam="" />
            <Action type="3" message="1701" wParam="0" lParam="1609" sParam="" />
        </Macro>
  ```
- Save and restart Notepad++.
  
In `Macros` on the toolbar you should now have `ExpandStardewEvent` and `InlineStardewEvent`. \
You can now select the event and use the `Ctrl+E` shortcut to expand the event actions and the `Ctrl+Shift+E` shortcut to collapse the event actions to one line.
