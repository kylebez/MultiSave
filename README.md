# MultiSave
A utility to save a file to multiple locations simultaneously.

*MUST BE RUN IN ADMIN MODE*

To use:

1. Select/remove the folder(s) that you want to set as the destination group.

2. Select the optional parameters you wish to enable:

  **Match Path**
  Used when copying a file down the folder tree to a location in the destination location9s) with a similar folder tree path.
  *EXAMPLE:*
  For the source *C://Users/username/Documents/Working/source.txt*,
  
  Instead of copying to
  *//Drive/Destination/source.txt*, as is the default, 
  if *//Drive/Destination/Document* exists 
  it will copy to
  *//Drive/Destination/Document/Working/source.txt*
  or if *//Drive/Destination/Users* exists
  it will copy to
  *//Drive/Destination/Users/username/Document/Working/source.txt*
  
  The process will find the FIRST matched subfolder and attempt to store in the remainder of the subtree path beginning from that folder. If the subfolders do not exist, the process will fail UNLESS *Create Folders* is checked, which will create the necessary subfolders.
  If there is no subfolder match found under any of the destination locations, please use File Explorer to create a matching subfolder at the folder tree level you wish the file to be copied to.

  **Create Folders**
  *Only applicable with match path - see above*
  This will always have a confirmation dialog display the created folder path(s).

  **Overwrite**
  Overwrite files that already exist in the destination group locations

  **Confirm**
  Enables a confirmation box to display to verify the source file is saved to the correct path - will display a notice for each       destination location
  *Use cases*
  * Used to skip locations in the destination group - will prevent that location from being added to the cache if run for the first time with a new destination group.
  * Used to verify that the correct locations are found when using *match path*.
  
  *It is suggested to open the MultiSave dialog again and, if you are making no other changes, toggle this option off once you are comfortable that the proper locations are cached with Match Path. This will speed up the process for subsequent 

3. Hit Apply & Close to set the destination group and parameters in the registry.

4. Right-click source file(s) in File Explorer and select **Save to Linked Group**

5. Program will run, displaying confirmation boxes if eneabled, or displaying error messages if applicable.

6. Will display a confirmation COMPLETE dialog when successfully finished.

*If match path enabled:*
The program may take some time to complete on first run with a specific destination group, but the found paths will be added to cache. Please be patient and wait for the COMPLETE dialog box.
When run again, if detination group has not changed, it will run from the cached locations and be much faster.
*Cache will reset whenever the destination group is changed.*

**OTHER NOTES**
* Copying to network paths that require credentials will **fail** unless the required connection is made in File Explorer. (Credentials will have to be re-entered every time the computer is restart for example).
* Mapped network drives do not carry over to the MultiSave dialog box. When adding a network location, you will have to remap the network drives - (right click *This PC* in the *Browse for Folder* dialog). Hopefully I will be able to release an update to resolve this.




