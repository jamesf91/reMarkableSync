# reMarkable OneNote Addin
This is an OneNote AddIn for importing digitized notes from the reMarkable tablet.

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