using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using MultiSave;
using System.Runtime.Caching;
using System.Collections.Generic;

namespace BatchSave
{
    class Program
    {
        static void Main(string[] args)
        {
            Debugger.Launch();
            string source = "";
            bool syncBelow = false;
            bool createFolders = false;
            bool overWrite = false;
            bool conFirm = false; 
            ObjectCache cache = MemoryCache.Default;            
            ArrayList copypaths = new ArrayList();
            //Check for valid file path parameters
            if (args.Length > 2)
            {
                List<string> filePaths = Properties.Settings.Default.filepaths as List<string>;
                if (filePaths == null) { filePaths = new List<string>(); }
                List<string> targetPaths = Properties.Settings.Default.targetpaths as List<string>;
                if (targetPaths == null) { targetPaths = new List<string>(); }

                source = args[0];
                //Set destination args into cache if they have changed
                bool oldCache = false;                   
                List<string> mtarget = new List<string>();                
                for (int v = 2; v < args.Length - 4; v++)
                {
                    //if parameters dont match cache anymore, reset the cache                                     
                    mtarget.Add(args[v]);
                    if (targetPaths.Count>0 && !targetPaths.Contains(args[v]))
                    {
                        oldCache = true;
                    }
                }
                if (oldCache || targetPaths.Count == 0)
                {
                    targetPaths = mtarget;
                    Properties.Settings.Default.targetpaths = targetPaths;
                }
                string syncParam = args[args.Length - 4];
                string createParam = args[args.Length - 3];
                string overWriteParam = args[args.Length - 2];
                string conFirmParam = args[args.Length - 1];
                if (syncParam == "l")
                {
                    syncBelow = true; // Check if subtrees are set to be synced
                }
                if (createParam == "c")
                {
                    createFolders = true; // Check if non-existent folders are to be created
                }
                if (overWriteParam == "o")
                {
                    overWrite = true; // Check if existing files are to be overwritten
                }
                if (conFirmParam == "f")
                {
                    conFirm = true; // Check if confirm dialog is to be shown
                }
                for (int i = 1; i < args.Length-4; i++)
                {
                    if (Directory.Exists(args[i]))
                    {                       
                        copypaths.Add(args[i]);
                    }
                    else if(i>0)
                    {
                        System.Windows.Forms.MessageBox.Show(args[i]+" doesn't exist, please check paths in groups", "ERROR");
                        Properties.Settings.Default.Save();
                        return;
                    }
                }
                string[] ffn = source.Split('\\');
                string filename = ffn[ffn.Length - 1];
                int tfilePaths = filePaths.Count;
                List<string> mpathList = new List<string>();
                if (filePaths != null && filePaths.Count > 0)
                {
                    foreach (string pl in filePaths)
                    {
                        if (filePaths.Count > tfilePaths) { break; }
                        for (int t = 2; t < args.Length - 4; t++)
                        {
                            if (pl.Contains(args[t]))
                            {
                                mpathList.Add(pl);
                            }
                        }
                    }
                }
                foreach (string f in copypaths)
                {
                    if (!source.Contains(f))              
                    {
                        if (syncBelow) {
                            bool found = false;
                            ArrayList compare = new ArrayList();                           
                            bool incache = false;                            
                            //Check cache first
                            for (int i = 1; i < ffn.Length - 1; i++)
                            {                                                               
                                if (tfilePaths > 0)
                                {
                                    for (int t = 0; t < mpathList.Count; t++)
                                    {
                                        if (mpathList[t].Contains(f))
                                        {
                                            string fffn = "";
                                            for (int w = i; w < ffn.Length - i; w++) { fffn += "\\" + ffn[w]; }
                                            if (mpathList[t].Contains(fffn))
                                            {
                                                if (Directory.Exists(mpathList[t]))
                                                {
                                                    string lio = source.Substring(source.LastIndexOf("\\"));
                                                    string file = mpathList[t] + "\\" + lio;
                                                    Copy(source, file, overWrite, conFirm);
                                                    found = true;
                                                    incache = true;
                                                    mpathList[t] = "";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //Then check file system if not extant
                            if (!incache)
                            {
                                for (int i = 1; i < ffn.Length - 1; i++)
                                {
                                    string[] d = null;
                                    try
                                    {
                                        d = Directory.GetDirectories(f, ffn[i], SearchOption.AllDirectories);
                                    }
                                    catch (UnauthorizedAccessException ex)
                                    {
                                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                                        continue;
                                    }
                                    if (d != null && d.Length > 0)
                                    {
                                        string fileend = "";
                                        for (int x = i + 1; x < ffn.Length - 1; x++)
                                        {
                                            fileend += "\\" + ffn[x];
                                        }
                                        for (int n = 0; n < d.Length; n++)
                                        {
                                            string c = d[n] + fileend;
                                            if (createFolders) { Directory.CreateDirectory(c); }
                                            string fp = c;
                                            c += "\\" + filename;
                                            try
                                            {
                                                if (compare.Contains(c)) { break; }
                                                compare.Add(c);
                                                string y = Copy(source, c, overWrite, conFirm);
                                                found = true;
                                                if (y == "Yes")
                                                {
                                                    if (!filePaths.Contains(fp)) { filePaths.Add(fp); }
                                                    break;
                                                }
                                            }
                                            catch (DirectoryNotFoundException ex)
                                            {
                                                System.Windows.Forms.MessageBox.Show("'Create folders' off. Can't write to path as full path does not exist: " + c, "ERROR");
                                            }
                                        }
                                    }
                                }                                                        
                            }
                            if (!found) {System.Windows.Forms.MessageBox.Show("No matches found, no files copied", "UNSUCCESSFUL"); }
                        }
                        else
                        {
                            for (int i = 0; i < copypaths.Count; i++)
                            {
                                string lio = source.Substring(source.LastIndexOf("\\"));
                                string file = copypaths[i] + "\\" + lio;
                                string y=Copy(source, file, overWrite, conFirm);
                                if (y == "Yes")
                                {
                                    if (!filePaths.Contains((string)copypaths[i])) { filePaths.Add((string)copypaths[i]); }
                                    break;
                                }
                            }
                        }
                    }
                }
                Properties.Settings.Default.filepaths = filePaths;
                Properties.Settings.Default.Save();
                System.Windows.Forms.MessageBox.Show("COMPLETE", "COMPLETE");
            }
            else { System.Windows.Forms.MessageBox.Show("There are no linked groups", "NO GROUPS"); }
        }

        //The method to actually copy the file, passing in overwrite and confirm parameters
        static string Copy(string s, string d, bool ow, bool c) {
            System.Windows.Forms.DialogResult con;
            if (c) { con = System.Windows.Forms.MessageBox.Show("Save file to: " + d + "?", "Confirm", System.Windows.Forms.MessageBoxButtons.YesNo); }
            else { con = System.Windows.Forms.DialogResult.Yes; }
            if (con == System.Windows.Forms.DialogResult.Yes)
            {
                if (ow)
                {
                    File.Copy(s, d, true);
                    if (!c) { System.Windows.Forms.MessageBox.Show("File saved to: " + d); }
                }
                else
                {
                    try
                    {
                        File.Copy(s, d);
                        if (!c) { System.Windows.Forms.MessageBox.Show("File saved to: " + d); }
                    }
                    catch (IOException e)
                    {
                        System.Windows.Forms.MessageBox.Show("'Overwrite' off. File already exists, skipping: " + d);
                    }
                }                
                return "Yes";
            }
            return null;
        }

        static string getAssembly() {
            string a = Assembly.GetExecutingAssembly().Location;
            return a;
        }
        
    }
}