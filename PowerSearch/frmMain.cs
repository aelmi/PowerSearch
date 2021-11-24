using Ookii.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace PowerSearch
{
    public partial class frmMain : Form
    {
        AppFilter appFilter;

        private int ActiveThread = 0;
        public DataTable dtFiles;
        bool includeSubfolder = false;

        public frmMain()
        {
            InitializeComponent();

            dtFiles = new DataTable();
            dtFiles.TableName = "Files";
            dtFiles.Columns.Add("Filename", typeof(string));
            dtFiles.Columns.Add("Extension", typeof(string));
            dtFiles.Columns.Add("Size[KB]", typeof(int));
            dtFiles.Columns.Add("Content", typeof(string));
            dtFiles.Columns.Add("CreationTime", typeof(DateTime));
            dtFiles.Columns.Add("GetLastAccessTime", typeof(DateTime));
        }

        public DataTable getDataTableFiles(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                long size = new System.IO.FileInfo(file).Length / 1024;
                if (size == 0)
                    size = 1;

                string ext = Path.GetExtension(file).Replace(".", "");
                if (isExcludeExtension(ext))
                    continue;
                if (isExcludeFolder(Path.GetDirectoryName(file)))
                    continue;

                if (lbFileExtension.Items.Count == 0)
                {
                    DataRow dr = dtFiles.NewRow();
                    dr["Filename"] = file;
                    dr["Extension"] = ext;
                    dr["Size[KB]"] = size;
                    dr["CreationTime"] = File.GetCreationTime(file);
                    dr["GetLastAccessTime"] = File.GetLastAccessTime(file);
                    dtFiles.Rows.Add(dr);
                }
                else
                {
                    for (int j = 0; j < lbFileExtension.Items.Count; j++)
                    {
                        if ((lbFileExtension.Items[j].ToString().ToUpper() == ext.ToUpper()))
                        {
                            DataRow dr = dtFiles.NewRow();
                            dr["Filename"] = file;
                            dr["Extension"] = ext;
                            dr["Size[KB]"] = size;
                            dr["CreationTime"] = File.GetCreationTime(file);
                            dr["GetLastAccessTime"] = File.GetLastAccessTime(file);
                            dtFiles.Rows.Add(dr);
                        }
                    }
                }
            }
            return dtFiles;
        }

        private bool isExcludeExtension(string ext)
        {
            for (int i = 0; i < lbFileExcludeExtension.Items.Count; i++)
            {
                if (lbFileExcludeExtension.Items[i].ToString().ToUpper() == ext.ToUpper())
                    return true;
            }

            return false;
        }


        private void btnExtensionClear_Click(object sender, EventArgs e)
        {
            lbFileExtension.Items.Clear();
        }

        private void btnFolderClear_Click(object sender, EventArgs e)
        {
            lbFolder.Items.Clear();
        }

        private void btnDelFolder_Click(object sender, EventArgs e)
        {
            if (lbFolder.SelectedItem != null)
                lbFolder.Items.Remove(lbFolder.Items[lbFolder.SelectedIndex]);
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.IO.Directory.Exists(txtFolder.Text))
                {
                    MessageBox.Show("The path does not exist or invalid.");
                    return;
                }

                for (int i = 0; i < lbFolder.Items.Count; i++)
                {
                    if (lbFolder.Items[i].ToString() == txtFolder.Text)
                    {
                        DialogResult result = MessageBox.Show(this, "This folder has been already added" + Environment.NewLine + "Would you like to select it?",
                            "Duplicate extension", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                            lbFolder.SetSelected(i, true);
                        return;
                    }
                }
                lbFolder.Items.Add(txtFolder.Text);
            }
            finally
            {
                txtFolder.Clear();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //if (backgroundWorker1.IsBusy != true)
            //{
            //    // Start the asynchronous operation.
            //    backgroundWorker1.RunWorkerAsync();
            //}


            //string[] allFiles[];
            dtFiles.Clear();
            if (cbIncludeSubfolder.CheckState == CheckState.Checked)
                includeSubfolder = true;
            else
                includeSubfolder = false;

            for (int i = 0; i < lbFolder.Items.Count; i++)
            {
                if (!isExcludeFolder(lbFolder.Items[i].ToString()))
                {
                    // var files = FileHelper.getFilesInFolder(lbFolder.Items[i].ToString(), includeSubfolder);
                    var folders = FileHelper.getSubfolder(lbFolder.Items[i].ToString());
                    for (int j = 0; j < folders.Length; j++)
                        getFileAndFolders(folders[j]);

                }
                //gcFiles.DataSource = dtFiles;
            }
            gcFiles.DataSource = dtFiles;

            //this.gvFiles.Columns["Extension"].Width = 60;
            ActiveThread = 0;
            this.gvFiles.OptionsView.ColumnAutoWidth = true;
        }

        // This event handler is where the time-consuming work is done.


        private void getFileAndFolders(string Foldername)
        {
            try
            {
                if (isExcludeFolder(Path.GetDirectoryName(Foldername)))
                    return;

                getFiles(Foldername);

                var folders = FileHelper.getSubfolder(Foldername);
                if (folders.Length > 0)
                {
                    for (int i = 0; i < folders.Length; i++)
                    {
                        getFileAndFolders(folders[i]);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void getFiles(string Foldername)
        {
            try
            {
                if (ActiveThread < 30) // It can be configurable or depends on memory - TODO
                {
                    var files = FileHelper.getFilesInFolder(Foldername);
                    getDataTableFiles(files);
                }
                else
                {
                    System.Threading.Thread.Sleep(200);
                    getFiles(Foldername);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool isExcludeFolder(string FolderName)
        {
            for (int i = 0; i < lbExcludeFolder.Items.Count; i++)
            {
                if (lbExcludeFolder.Items[i].ToString().ToUpper() == FolderName.ToUpper())
                    return true;
            }
            return false;
        }

        private void btnLoadFileContent_Click(object sender, EventArgs e)
        {
            btnSearch_Click(null, null);
            for (int i = 0; i < dtFiles.Rows.Count; i++)
            {
                try
                {
                    var fc = File.ReadAllText(dtFiles.Rows[i]["Filename"].ToString());
                    dtFiles.Rows[i]["Content"] = fc;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            this.gvFiles.OptionsView.ColumnAutoWidth = true;
        }

        private void btnAddExtension_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtExtension.Text.Trim()))
                    return;

                for (int i = 0; i < lbFileExtension.Items.Count; i++)
                {
                    if (lbFileExtension.Items[i].ToString() == txtExtension.Text)
                    {
                        DialogResult result = MessageBox.Show(this, "This extension has been already added" + Environment.NewLine + "Would you like to select it?",
                            "Duplicate extension", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                            lbFileExtension.SetSelected(i, true);
                        return;
                    }
                }
                lbFileExtension.Items.Add(txtExtension.Text);
            }
            finally
            {
                txtExtension.Clear();
            }
        }

        private void btnDelExtension_Click(object sender, EventArgs e)
        {
            if (lbFileExtension.SelectedItem != null)
                lbFileExtension.Items.Remove(lbFileExtension.Items[lbFileExtension.SelectedIndex]);
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog _sampleVistaFolderBrowserDialog = new VistaFolderBrowserDialog();
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if (_sampleVistaFolderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                txtFolder.Text = _sampleVistaFolderBrowserDialog.SelectedPath;
        }

        private void btnAddExcludeFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.IO.Directory.Exists(txtExcludeFolder.Text))
                {
                    MessageBox.Show("The path does not exist or invalid.");
                    return;
                }

                for (int i = 0; i < lbExcludeFolder.Items.Count; i++)
                {
                    if (lbExcludeFolder.Items[i].ToString() == txtExcludeFolder.Text)
                    {
                        DialogResult result = MessageBox.Show(this, "This folder has been already added" + Environment.NewLine + "Would you like to select it?",
                            "Duplicate extension", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                            lbExcludeFolder.SetSelected(i, true);
                        return;
                    }
                }
                lbExcludeFolder.Items.Add(txtExcludeFolder.Text);
            }
            finally
            {
                txtExcludeFolder.Clear();
            }
        }

        private void btnSelectExcludeFolder_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog _sampleVistaFolderBrowserDialog = new VistaFolderBrowserDialog();
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            if (_sampleVistaFolderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                txtExcludeFolder.Text = _sampleVistaFolderBrowserDialog.SelectedPath;
        }

        private void btnDelExcludeFolder_Click(object sender, EventArgs e)
        {
            if (lbExcludeFolder.SelectedItem != null)
                lbExcludeFolder.Items.Remove(lbExcludeFolder.Items[lbExcludeFolder.SelectedIndex]);
        }

        private void btnFolderExcludeClear_Click(object sender, EventArgs e)
        {
            lbExcludeFolder.Items.Clear();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; i <= 10; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.

                }
            }

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            gvFiles.OptionsView.ShowAutoFilterRow = true;
            this.WindowState = FormWindowState.Maximized;
        }

        private void LoadFilters()
        {
            string filterFolder = Application.StartupPath + @"\Filter";
            if (!Directory.Exists(filterFolder))
                System.IO.Directory.CreateDirectory(filterFolder);

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = filterFolder;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var serializer = new XmlSerializer(typeof(AppFilter));
                using (var reader = XmlReader.Create(dlg.FileName))
                {
                    appFilter = (AppFilter)serializer.Deserialize(reader);
                }

                lbFileExtension.Items.Clear();
                lbFileExcludeExtension.Items.Clear();
                lbFolder.Items.Clear();
                lbExcludeFolder.Items.Clear();

                for (int i = 0; i < appFilter.FileExtension.Count; i++)
                    lbFileExtension.Items.Add(appFilter.FileExtension[i]);

                for (int i = 0; i < appFilter.ExcludeExtension.Count; i++)
                    lbFileExcludeExtension.Items.Add(appFilter.ExcludeExtension[i]);

                for (int i = 0; i < appFilter.Folder.Count; i++)
                    lbFolder.Items.Add(appFilter.Folder[i]);

                if (appFilter.IncludeSubfolder)
                    cbIncludeSubfolder.Checked = true;
                else
                    cbIncludeSubfolder.Checked = false;

                for (int i = 0; i < appFilter.ExcludeSubfolder.Count; i++)
                    lbExcludeFolder.Items.Add(appFilter.ExcludeSubfolder[i]);
            }
        }

        private void btnFilterEditor_Click(object sender, EventArgs e)
        {
            if (gcFiles.DataSource != null && (((DataTable)gcFiles.DataSource).Rows.Count > 0))
                gvFiles.ShowFilterEditor(gvFiles.Columns[3]);
            else
                MessageBox.Show("There is no file in the list");
        }

        private void btnRepetitive_Click(object sender, EventArgs e)
        {
            frmRepetitive fr = new frmRepetitive();
            fr.Show();
        }

        private void btnExcludeExtensionClear_Click(object sender, EventArgs e)
        {
            lbFileExcludeExtension.Items.Clear();
        }

        private void btnAddExcludeExtension_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtExcludeExtension.Text.Trim()))
                    return;

                for (int i = 0; i < lbFileExcludeExtension.Items.Count; i++)
                {
                    if (lbFileExcludeExtension.Items[i].ToString() == txtExcludeExtension.Text)
                    {
                        DialogResult result = MessageBox.Show(this, "This extension has been already added" + Environment.NewLine + "Would you like to select it?",
                            "Duplicate extension", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                            lbFileExcludeExtension.SetSelected(i, true);
                        return;
                    }
                }
                lbFileExcludeExtension.Items.Add(txtExcludeExtension.Text);
            }
            finally
            {
                txtExcludeExtension.Clear();
            }
        }

        private void btnDelExcludeExtension_Click(object sender, EventArgs e)
        {
            if (lbFileExcludeExtension.SelectedItem != null)
                lbFileExcludeExtension.Items.Remove(lbFileExcludeExtension.Items[lbFileExcludeExtension.SelectedIndex]);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(lbFolder.SelectedItem.ToString()); }
            catch { }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(lbFileExtension.SelectedItem.ToString()); }
            catch { }
        }

        private void toolStripMenuItemFileExcludeExtension_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(lbFileExcludeExtension.SelectedItem.ToString()); }
            catch { }
        }

        private void toolStripMenuItemExcludeFolder_Click(object sender, EventArgs e)
        {
            try { Clipboard.SetText(lbExcludeFolder.SelectedItem.ToString()); }
            catch { }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            gvFiles.ShowPrintPreview();
        }

        private void btnSaveFilter_Click(object sender, EventArgs e)
        {
            appFilter = new AppFilter();

            for (int i = 0; i < lbFileExtension.Items.Count; i++)
                appFilter.FileExtension.Add(lbFileExtension.Items[i].ToString());

            for (int i = 0; i < lbFileExcludeExtension.Items.Count; i++)
                appFilter.ExcludeExtension.Add(lbFileExcludeExtension.Items[i].ToString());

            for (int i = 0; i < lbFolder.Items.Count; i++)
                appFilter.Folder.Add(lbFolder.Items[i].ToString());

            if (cbIncludeSubfolder.Checked)
                appFilter.IncludeSubfolder = true;

            for (int i = 0; i < lbExcludeFolder.Items.Count; i++)
                appFilter.ExcludeSubfolder.Add(lbExcludeFolder.Items[i].ToString());


            string filterFolder = Application.StartupPath + @"\Filter";
            if (!Directory.Exists(filterFolder))
                System.IO.Directory.CreateDirectory(filterFolder);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = "filter";
            dlg.InitialDirectory = filterFolder;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var serializer = new XmlSerializer(appFilter.GetType());
                using (var writer = XmlWriter.Create(dlg.FileName))
                {
                    serializer.Serialize(writer, appFilter);
                }
            }


        }

        private void btnLoadFilter_Click(object sender, EventArgs e)
        {
            LoadFilters();
        }
    }
}
