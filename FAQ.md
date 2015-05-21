## Summary ##
  * [General Questions](FAQ#General_Questions.md):
    1. Why versioning ActionScript based projects?
    1. There are already other versioning utilities and plugins availble for FlashDevelop, mainly Version by Jean Louis Persat, so why AutoVersion?
  * [AutoVersion Questions](FAQ#AutoVersion_Questions.md):
    1. Where can I locate the AutoVersion menu?
    1. OK, how do I use it?
    1. I'd like to use latest SVN revision number for my versioning scheme, could you add support for it?
    1. The class created by AutoVersion doesn't meet the format I use or I'd like, could you change it?.
    1. This is all great, but I don't use FlashDevelop to write AS3 projects, can I still use AutoVersion?
    1. AutoVersion creates a file named <project name>.version under my project path, what is it?
    1. Is it possible to version two or more different projects located on the same folder?
    1. I'd like to use AutoVersion as a build counter, is the feature available?
    1. Now that FlashDevelop 4 is available, will later versions of AutoVersion be available for FD 3 as well?
  * [BVI Questions](FAQ#BVI_Questions.md):
    1. Why using BVI as the base of AutoVersion?
    1. I'd like to use a BVI plugin with AutoVersion, is it possible?


## General Questions ##

  * Why versioning ActionScript based projects?
> Unlike native OS binaries, SWF files don't have a native version info associaed with them. To overcome this, Adobe added the possibility to embed XMP data into the compiled SWFs, however, using this method is not always possible or desirable.
> Versioning SWF files can be of interest, specially on large projects, to track down changes, bugs, etc.

  * There are already other versioning utilities and plugins availble for FlashDevelop, mainly Version by Jean Louis Persat, so why AutoVersion?
> The other tools don't provide the versioning features my team need, so AutoVersion was developed in order to cover them. I just wanted to publicly release it so other people could use it as well, and help/force me to improve it, nothing else. People should choose whatever tool suits them better, either this one or any other.


## AutoVersion Questions ##

  * Where can I locate the AutoVersion menu?
> AutoVersion dialog is by default located in the Project menu as the last item.

  * OK, how do I use it?
> In order to enable it you must first enable the "Update version data" option, and choose whatever versioning style fits in your project.

  * I'd like to use latest SVN revision number for my versioning scheme, could you add support for it?
> Sorry, but this versioning style isn't natively supported. You could try to convert [Happy Turtle](http://happyturtle.codeplex.com/) for AutoVersion. Look at the [BVI Questions section](FAQ#BVI_Questions.md) for more details.
> _**Note:**_ In the downloads section you can find a compiled binary of Happy Turtle with support for AutoVersion, but bear in mind that this version is unofficial and unsupported.

  * The class created by AutoVersion doesn't meet the format I use or I'd like, how could I change it?.
> You can create your own template and configure the plugin to use it instead of the default template that comes together with AutoVersion.
> The template syntax uses the same arguments as common FlashDevelop templates, along with $(Major), $(Minor), $(Build) and $(Revision) to print the different version parts.
> Also, as of AutoVersion 1.0 the templates aren't embedded inside the assembly, so you can head to your user FlashDevelop templates folder, and modify the default templates. However, take into account that they may be overwritten with each new plugin release.

  * This is all great, but I don't use FlashDevelop to write AS3 projects, can I still use AutoVersion?
> Yes, since version 1.0 AutoVersion also works for ActionScript 2 and haXe projects, and you can always use the custom template feature in order to further customize your versioning file.

  * AutoVersion creates a file named <project name>.version under my project path, what is it?
> That .version file is just a simple XML containing the AutoVersion project settings. It is saved that way so it can be easily shared between team members, and store it in a repository with no problems.

  * Is it possible to version two or more different projects located on the same folder?
> Yes, but you must make sure that the version data file name is different for all of them.

  * I'd like to use AutoVersion as a build counter, is the feature available?
> You can already use the Increment versioning style and your own template in order to have a build counter.

  * Now that FlashDevelop 4 is available, will later versions of AutoVersion be available for FD 3 as well?
> I'm not using FD 3 anymore, so it won't be targetted, however, if people ask for it, I don't mind releasing compatible versions of the plugin.

## BVI Questions ##

  * Why using BVI as the base of AutoVersion?
> The people behind BVI is doing a great work with the project, making AutoVersion extensible is something I wanted from the start and they already provided it, they also provide the versioning scheme my team uses. Lastly, porting BVI code to make a FlashDevelop plugin was rather easy. Of course, all the hard work merit is for them.

  * I'd like to use a BVI plugin with AutoVersion, is it possible?
> BVI currently uses an abstract class to extend its versioning styles. However, this abstract class is located inside the BVI assembly, so, in theory, you could use BVI plugins by changing their reference of BVI to the AutoVersion assembly, change the plugin incrementor base class to the AutoVersion one, and just recompile it.