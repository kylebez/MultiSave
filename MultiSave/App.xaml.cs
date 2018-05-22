using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections;

namespace MultiSave
{
    public partial class App : Application
    {
        string filename = "folderList.bin";
        string paramname = "paramList.bin";
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Restore application-scope property from isolated storage
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, storage))
                using (StreamReader reader = new StreamReader(stream))
                {
                    // Restore each application-scope property individually
                    while (!reader.EndOfStream)
                    {
                        string fol = reader.ReadLine();
                        ArrayList fl = (ArrayList)this.FindResource("folderList");
                        fl.Add(fol);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "ERROR");
            }
            try
            {
                using (IsolatedStorageFileStream stream2 = new IsolatedStorageFileStream(paramname, FileMode.OpenOrCreate, storage))
                using (StreamReader reader2 = new StreamReader(stream2))
                {
                    // Restore each application-scope property individually
                    while (!reader2.EndOfStream)
                    {
                        string pol = reader2.ReadLine();
                        ArrayList pl = (ArrayList)this.FindResource("paramList");
                        pl.Add(pol);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "ERROR");
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Persist application-scope property to isolated storage
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            ArrayList fl = (ArrayList)this.FindResource("folderList");
            ArrayList pl = (ArrayList)this.FindResource("paramList");
            using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(filename, FileMode.Create, storage))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                // Persist each application-scope property individually
                foreach (string fol in fl)
                {
                    writer.WriteLine(fol);
                }
            }
            using (IsolatedStorageFileStream stream2 = new IsolatedStorageFileStream(paramname, FileMode.Create, storage))
            using (StreamWriter writer2 = new StreamWriter(stream2))
            {
                // Persist each application-scope property individually
                for (int p=0; p<pl.Count; p++)
                {
                    bool bp = Convert.ToBoolean(pl[p]);
                    writer2.WriteLine(bp);
                }
            }
        }
    }
}

