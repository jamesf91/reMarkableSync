# reMarkable OneNote Addin
This is a OneNote AddIn for importing digitized notes from the reMarkable tablet.

## Beta version 4.0
Updated the original version to support the new reMarkable file format.
Decoding the new format was based on the work of [Rick Lupton](https://github.com/ricklupton/rmscene).

* There is no support for the new text yet.
* When text is added to a notebook, the layout of the generated images is not correct.


## New in Version 3
Version 3 is a major version update that adds support for the new (2022) reMarkable cloud API.

Big shoutout to [juruen/rmapi](https://github.com/juruen/rmapi) for working out all the nitty gritty of the new API.

Please note that while the new cloud API is now supported, due to the way it works, it is **SIGNIFICANTLY** slower than the previous cloud API. For example, getting the whole document tree using the previous API required only a single HTTPS request, regardless of how many documents are in the tree. Using the new API, it now requires 4 HTTPS requests for **EACH** document/folder (i.e. if your device has 200 document, 800 HTTPS requests are now needed to build up the document tree). While optimizations such as caching and multithreaded requests are used, it is still much much slower to populate the document tree, especially the first time the AddIn syncs to the device using the new API. 

## Installation
Latest release installer available at <https://github.com/jamesf91/RemarkableSync/releases/latest>  


Choose either the 32-bit or the 64-bit Windows installer depending on whether the OneNote version is 32-bit or 64-bits:  
In OneNote, go to File menu -> Account -> About OneNote. The first line in the About window should say something like "Microsoft OneNote ... 64-bit or 32-bit"

Once the installer completed, start OneNote. You should see a new tab in the ribbon bar called "reMarkable".

## Setup
Before using the addin, there is a one time setup to link the addin to your reMarkable table and configure the hand writing recognition service.

### Linking reMarkable tablet
Go to the reMarkable tab in the OneNote ribbon and click on the Settings icon. The Settings window should appear.  
Go to <https://my.remarkable.com/device/desktop/connect> for a new desktop client one-time code. Copy it into the "One Time Code" field in Settings window and click Apply. This will link OneNote as a new desktop client to your reMarkable Cloud account.

### Configure hand writing recognition
The reMarkable OneNote addin uses MyScript (<https://www.myscript.com/>) as an external service to convert the handwritten stroke from reMarkable tablet into text. It is the same service used by the reMarkable device itself when selecting "Convert to text and send".

Luckily MyScript provides 2000 free conversions a month, which is plenty for normal usage. All we have to do is register for a free account at <https://sso.myscript.com/register>. Once registered and logged in, go to <https://developer.myscript.com/getting-started/web>, select Web platform and click "Send email" to receive your private application key via email. In the email you should receive 2 keys: the application key and the HMAC key. Copy each key into the corresponding field in the Settings windows of the addin and hit Apply to save the configuration.

## Troubleshooting
This addin does not produce any log by default. To generate log for troubleshooting, open up Windows Registry and navigate to the following key:
* "Computer\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Office\OneNote\AddInsData\RemarkableSync.OnenoteAddin" for 64 bit-install or,
* "Computer\HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\Microsoft\Office\OneNote\AddInsData\RemarkableSync.OnenoteAddin" for 32-bit install

Then add a new string-valued entry named "LogFile". The value should be a full file path to the log file to be generated, e.g. "C:\temp\remarkable_sync_log.txt".
Once OneNote is restarted, the addin will start generating log.

## Technical details
Since OneNote only supports out of process COM addin, this addin was created as a C# local server COM class. While this addin is loaded, you will see an instance of RemarkableSync.OnenoteAddin.exe process running at the same time as OneNote.exe.

When the Fetch window is opened, the addin will retrieve all the notebooks that have been synced to reMarkable cloud and show them in a tree structure according to the folder they are in. Selecting a notebook in the tree and clicking Enter will download the .lines files for all the pages in that notebook, parse these files into individual pen strokes and send them to MyScript for conversion into plain text. The converted text is then inserted as a new page into the current OneNote notebook and tab.

## Acknowledgement
This is a personal project of mine that was created out of what to me was a missing piece in the overall workflow of using reMarkable tablet for taking work and study notes.

I'd like to thanks for following people and/or sites for inspiration, information and help:
- [MyScript](https://www.myscript.com) for providing the awesome free hand writing recongnitions services.
- [reHackable/awesome-reMarkable](https://github.com/reHackable/awesome-reMarkable) for compiling the list of existing reMarkable hacks out there.
- [ddvk/rmfakecloud](https://github.com/ddvk/rmfakecloud) for introducing MyScipt as the hand writing recognition services used by reMarkable.
- [subutux/rmapy](https://github.com/subutux/rmapy) for showing me how to interface with the reMarkable cloud.
- [bsdz/remarkable-layers](https://github.com/bsdz/remarkable-layers) for the Python parser for the .line format used by reMarkable, which I ported to C# for this project.
- [Lim Bio Liong at CodeProject](https://www.codeproject.com/Articles/12579/Building-COM-Servers-in-NET) for providing the boilerplate code for creating a C# .NET COM server.
- [juruen/rmapi](https://github.com/juruen/rmapi) for working out how the new cloud api works.
- [ricklupton/rmscene](https://github.com/ricklupton/rmscene) for decoding the new binary format.