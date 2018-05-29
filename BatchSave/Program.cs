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
                string lio = source.Substring(source.LastIndexOf("\\"));
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
                    targetPaths.Clear();
                    filePaths.Clear();
                    Properties.Settings.Default.targetpaths = mtarget;
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
                string[] ffn = source.Split('\\'); //Split the source filepath
                int tfilePaths = filePaths.Count;
                List<string> mpathList = new List<string>();
                if (filePaths != null && filePaths.Count > 0)
                {
                    foreach (string pl in filePaths)
                    {
                        if (filePaths.Count > tfilePaths) { break; }
                        for (int t = 0; t < copypaths.Count; t++)
                        {
                            if (pl.Contains((string)copypaths[t]))
                            {
                                mpathList.Add(pl);// List from cache
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
                            ArrayList destinationpaths = new ArrayList();                            

                            //CHECK IN CACHE
                            bool inCache = false;
                            foreach (string m in mpathList) {
                                if (m.Contains(f)) {
                                    if (Directory.Exists(m)) {
                                        destinationpaths.Add(m);
                                        inCache = true;
                                        found = true;
                                    }
                                }
                            }
                            //CHECK NOT IN CACHE
                            if (!inCache) {
                                char[] ch = new char[] { '\\' };
                                //string[] cfn = f.Split(ch, StringSplitOptions.RemoveEmptyEntries); //Split the copypath
                                for (int i = 1; i < ffn.Length - 1; i++)
                                {
                                    string tp = f + "\\" + ffn[i];
                                    string[] exist = Directory.GetDirectories(f, ffn[i], SearchOption.AllDirectories);
                                    if (exist.Length > 0)
                                    { //found list of matched folders in directory
                                      //check if the rest of path are in these folders
                                        foreach (string p in exist)
                                        {
                                            string rp = "";
                                            for (int t = i; t < ffn.Length - 1; t++)
                                            {
                                                if (t == i) { continue; }
                                                rp += "\\" + ffn[t];
                                            }
                                            string fp = tp + rp;
                                            //if (networkPath) { fp = "\\\\" + fp; }
                                            bool rpexist = Directory.Exists(fp);
                                            if (rpexist) { destinationpaths.Add(fp); found = true; }
                                            else if (createFolders)
                                            { //if not, but created folders param is on, then create the path (based on first matched folder, then subseq if unconfirmed)
                                                System.Windows.Forms.DialogResult createcon;
                                                createcon = System.Windows.Forms.MessageBox.Show("Create path: " + fp + " ?", "Confirm", System.Windows.Forms.MessageBoxButtons.YesNo);
                                                if (createcon == System.Windows.Forms.DialogResult.Yes)
                                                {
                                                    Directory.CreateDirectory(fp);
                                                    destinationpaths.Add(fp);
                                                    found = true;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            } 
                            
                            if (!found && createFolders) { System.Windows.Forms.MessageBox.Show("No matches found, no files copied", "UNSUCCESSFUL"); return; }
                            else if (!found && !createFolders) { System.Windows.Forms.MessageBox.Show("No matches found, no files copied - Try the create folders parameter", "UNSUCCESSFUL"); return; }
                            else {
                                if (destinationpaths.Count < 1) { System.Windows.Forms.MessageBox.Show("Application error", "CRITICAL ERROR"); return; }
                                foreach (string dest in destinationpaths)
                                {
                                    string file = dest + "\\" + lio;
                                    string y = Copy(source, file, overWrite, conFirm);
                                    if (y == "Yes")
                                    {
                                        if (!filePaths.Contains((string)dest)) { filePaths.Add((string)dest); }
                                        break;
                                    }
                                }                                
                            }
                        }
                        else // Straight copy, no path matching
                        {
                            for (int i = 0; i < copypaths.Count; i++)
                            {                                
                                string file = copypaths[i] + "\\" + lio;
                                string y=Copy(source, file, overWrite, conFirm);
                                /*if (y == "Yes")
                                {
                                    if (!filePaths.Contains((string)copypaths[i])) { filePaths.Add((string)copypaths[i]); }
                                    break;
                                }*/
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
            if (c) { con = System.Windows.Forms.MessageBox.Show("Save "+s+" to: " + d + "?", "Confirm", System.Windows.Forms.MessageBoxButtons.YesNo); }
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