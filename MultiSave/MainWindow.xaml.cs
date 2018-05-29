using System;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;

namespace MultiSave
{    
    //??: A WAY TO CARRY MAPPED DRIVES TO ADMIN MODE
    //OR
    //??: A WAY TO PERFORM THE REGISTRY TASK WITH ELEVATED PRIVILEGES INDEPENDENT OF THE REST OF THE APP
    
    //TODO: Add/manage groups
    //TODO: Make tray icon
    //TODO: Always/never/ask overwrite options
    //TODO: Progress bar for Match Path
    //TODO: Add feature for symlinks?
    //TODO: Handler when network folder not connected (and therefore not visible)
    //TODO: Remove registry key on uninstall
    //TODO: A way to view/clear destination group cache

    public partial class MainWindow : Window
    {
        private FolderBrowserDialog folderBrowserDialog;
        private string batch = @"C:\ProgramData\MultiSave\BatchSave.exe";
        private string folderName;
        private string pathparameters = "";
        private const string contextCommand = "*\\shell\\MultiSave";
        private const string contextCommand2 = "*\\shell\\MultiSave\\command";
        ArrayList fl;
        ArrayList pl;
        public MainWindow()
        {
            // Get persisted list and persisted parameters
            InitializeComponent();
            //Debugger.Launch();
            fl = (ArrayList)this.FindResource("folderList");
            pl = (ArrayList)this.FindResource("paramList");
            for (int i=0; i<pl.Count;i++)
                {
                    switch (i) {
                    case 0: if (Convert.ToBoolean(pl[i])) { syncBelow.IsChecked = true; } else { syncBelow.IsChecked = false; }
                        break;
                    case 1: if (Convert.ToBoolean(pl[i])) { createNotexist.IsChecked = true; } else { createNotexist.IsChecked = false; }
                        break;
                    case 2: if (Convert.ToBoolean(pl[i])) { overWrite.IsChecked = true; } else { overWrite.IsChecked = false; }
                        break;
                    case 3: if (Convert.ToBoolean(pl[i])) { conFirm.IsChecked = true; } else { conFirm.IsChecked = false; }
                        break;
                    }
                }
            if (syncBelow.IsChecked == true)
            {
                createNotexist.IsEnabled = true;
            }
            linkedFolderList.ItemsSource = fl;
            if (fl.Count <= 0) { btnRemoveFolder.IsEnabled = false; }
        }

        //Adding a folder to group, in listBox and in persistance
        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select a folder to be linked to the group.";
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer; 
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                folderName = folderBrowserDialog.SelectedPath;
                folderName = Pathing.GetUNCPath(folderName);
                //Check if selection does not already exist
                if (fl.Contains(folderName)) {
                    System.Windows.Forms.MessageBox.Show("Path already added, please select another", "Already exists");
                    return;
                }
                //update listBox
                linkedFolderList.ItemsSource = null;
                fl.Add(folderName);
                linkedFolderList.ItemsSource = fl;
                if (fl.Count > 0) { btnRemoveFolder.IsEnabled = true; }
            }            
        }

        //Removing a folder from group, in listBox and in persistance
        private void btnRemoveFolder_Click(object sender, EventArgs e)
        {
            System.Collections.IList sitems = linkedFolderList.SelectedItems;
            if (sitems.Count > 0)
            {
                int siteint = sitems.Count;
                Object[] siteobj = new Object[siteint];
                for (int i = 0; i < siteint; i++)
                {
                   siteobj[i] = sitems[i];
                }
                for (int i = 0; i < siteint; i++) {
                    //update listBox
                    linkedFolderList.ItemsSource = null;
                    fl.Remove(siteobj[i]);
                    linkedFolderList.ItemsSource = fl;
                }
                if (fl.Count <= 0) { btnRemoveFolder.IsEnabled = false; }
            }
        }

        //When Apply & Close is clicked
        private void btnClose_Click(object sender, EventArgs e) {
            this.SetExe();
        }

        //Configures context menu command
        private void addContextMenu() {
            //check if running as administrator
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            bool admin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!admin) { System.Windows.Forms.MessageBox.Show("Registry not updated, please run in admin mode.", "NOT ADMIN");return; }
            //rewrite context menu command with parameters including options and grouped paths
            RegistryKey key;
            key = Registry.ClassesRoot.CreateSubKey(contextCommand,true);
            key.SetValue(null, "Save to Linked Group", RegistryValueKind.String);
            key = Registry.ClassesRoot.CreateSubKey(contextCommand2, true);
            key.SetValue(null,batch+" %1" +pathparameters, RegistryValueKind.String);
        }

        //Some UI stuff
        private void syncBelow_Click(object sender, RoutedEventArgs e)
        {
            if (syncBelow.IsChecked == false)
            {
                createNotexist.IsEnabled = false;
            }
            else { createNotexist.IsEnabled = true; conFirm.IsChecked = true; }
        }

        //When closing from X -- Doesnt WORK
        private void Main_FormClosing(object sender, FormClosingEventArgs e) {
            this.SetExe();
        }

        private void SetExe() {
            //set folderlist as parameters for context menu command
            foreach (string f in fl)
            {
                pathparameters += " \"" + f + "\"";
            }
            pl.Clear();
            if (syncBelow.IsChecked == true && fl.Count > 0) { pathparameters += " l"; pl.Add(true); }//Set sync parameter
            else { pathparameters += " -l"; pl.Add(false); }
            if (createNotexist.IsChecked == true && fl.Count > 0 && createNotexist.IsEnabled == true) { pathparameters += " c"; pl.Add(true); }//Set create folders parameter
            else { pathparameters += " -c"; pl.Add(false); }
            if (overWrite.IsChecked == true && fl.Count > 0) { pathparameters += " o"; pl.Add(true); }//Set overwriting parameter
            else { pathparameters += " -o"; pl.Add(false); }
            if (conFirm.IsChecked == true && fl.Count > 0) { pathparameters += " f"; pl.Add(true); }//Set cofirm dialog parameter
            else { pathparameters += " -f"; pl.Add(false); }
            addContextMenu();
            this.Close();
        }
    }

    //Gets the UNC path of mapped network drives
    public static class Pathing
    {
        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WNetGetConnection(
            [MarshalAs(UnmanagedType.LPTStr)] string localName,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
            ref int length);
        /// <summary>
        /// Given a path, returns the UNC path or the original. (No exceptions
        /// are raised by this function directly). For example, "P:\2008-02-29"
        /// might return: "\\networkserver\Shares\Photos\2008-02-09"
        /// </summary>
        /// <param name="originalPath">The path to convert to a UNC Path</param>
        /// <returns>A UNC path. If a network drive letter is specified, the
        /// drive letter is converted to a UNC or network path. If the 
        /// originalPath cannot be converted, it is returned unchanged.</returns>
        public static string GetUNCPath(string originalPath)
        {
            StringBuilder sb = new StringBuilder(512);
            int size = sb.Capacity;

            // look for the {LETTER}: combination ...
            if (originalPath.Length > 2 && originalPath[1] == ':')
            {
                // don't use char.IsLetter here - as that can be misleading
                // the only valid drive letters are a-z && A-Z.
                char c = originalPath[0];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    int error = WNetGetConnection(originalPath.Substring(0, 2),
                        sb, ref size);
                    if (error == 0)
                    {
                        DirectoryInfo dir = new DirectoryInfo(originalPath);

                        string path =System.IO.Path.GetFullPath(originalPath)
                            .Substring(System.IO.Path.GetPathRoot(originalPath).Length);
                        return System.IO.Path.Combine(sb.ToString().TrimEnd(), path);
                    }
                }
            }
            return originalPath;
        }
    }
}
