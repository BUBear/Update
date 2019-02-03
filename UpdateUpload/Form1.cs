using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace UpdateUpload
{

    public struct FileList
    {
        public string Name;
        public string Ver;
        public string Path;
        public DateTime Time;
        public string LocalPath;
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string ftpServerUri = "ftp";
        string xmlName = @"\upFileList.xml";
        int fileCount = 0;
        bool IsDown = false;
        List<FileList> updateFile = new List<FileList>();

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropFile = (string[])e.Data.GetData(DataFormats.FileDrop);
            for (int i = 0; i < dropFile.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(dropFile[i]);
                if (!updateFile.Exists(file => file.Name == fileInfo.Name)) //중복 파일 확인
                {
                    FileList list = new FileList();
                    list.Name = fileInfo.Name;
                    list.Time = fileInfo.LastWriteTime;
                    list.Path = fileInfo.Directory.Name + "\\";
                    list.Ver = FileVersionInfo.GetVersionInfo(dropFile[i]).FileVersion;
                    list.LocalPath = fileInfo.FullName;
                    updateFile.Add(list);

                    listView1.Items.Add(list.Name);
                    listView1.Items[updateFile.Count - 1].SubItems.Add(list.Ver);
                    listView1.Items[updateFile.Count - 1].SubItems.Add(list.Time.ToString());
                    listView1.Items[updateFile.Count - 1].SubItems.Add(list.Path);
                }
            }
            label5.Text = "파일 가져오기 성공!";
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveXml();
        }

        private void SaveXml()
        {
            try
            {
                if (updateFile.Count > 0)
                {
                    label5.Text = "Xml저장..";
                    XmlWriterSettings setting = new XmlWriterSettings();
                    setting.Encoding = Encoding.UTF8;
                    setting.Indent = true;
                    XmlWriter xmlWriter = XmlWriter.Create(Application.StartupPath + xmlName, setting);
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("UPDATE");
                    for (int i = 0; i < updateFile.Count; i++)
                    {
                        xmlWriter.WriteStartElement("FILE");
                        xmlWriter.WriteAttributeString("name", updateFile[i].Name);
                        xmlWriter.WriteAttributeString("ver", updateFile[i].Ver);
                        xmlWriter.WriteAttributeString("time", updateFile[i].Time.ToString());
                        xmlWriter.WriteAttributeString("path", updateFile[i].Path);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.Close();
                    label5.Text = "Xml저장 성공!";
                }
                else
                {
                    label5.Text = "Xml저장 오류";
                    MessageBox.Show("업로드 파일 목록이 없습니다. 업로드 파일을 가져온뒤 다시 시도 하세요.", "오류");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private long GetFileSize(string path)
        {
            long size = 0;
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                size = fileInfo.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return size;
        }

        private void AddListXml()
        {
            if (File.Exists(Application.StartupPath + xmlName))
            {
                FileList fileList = new FileList();
                fileList.Name = xmlName.Remove(0, 1); // @제거
                fileList.LocalPath = Application.StartupPath + xmlName;
                updateFile.Insert(0, fileList);
                listView1.Refresh();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //기타 셋팅
            progressBar1.Maximum = updateFile.Count;
            progressBar1.Step = 1;

            //업로드
            if (updateFile.Count > 0)
            {
                AddListXml();

                while (fileCount < updateFile.Count)
                {
                    if (!IsDown)
                    {
                        Upload();
                        label5.Text = "업로드 시작..";
                    }
                    Application.DoEvents();
                }
            }
            else
            {
                label5.Text = "대기";
                MessageBox.Show("업로드 파일 목록이 없습니다. 업로드 파일을 가져온뒤 다시 시도 하세요.", "오류");
            }
        }

        private void Upload()
        {
            try
            {
                string fileUri = updateFile[fileCount].Path;
                string fileLocalName = updateFile[fileCount].LocalPath;
                string fileName = updateFile[fileCount].Name;
                int fileSize = (int)GetFileSize(updateFile[fileCount].LocalPath);
                fileCount++;
                IsDown = true;

                progressBar2.Maximum = fileSize;

                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential("aaa", "");
                wc.UploadProgressChanged += new UploadProgressChangedEventHandler(UploadProgess);
                wc.UploadFileCompleted += new UploadFileCompletedEventHandler(UploadCompleted);
                wc.UploadFileAsync(new Uri(ftpServerUri + fileUri + fileName), WebRequestMethods.Ftp.UploadFile, fileLocalName);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            progressBar1.PerformStep();
            progressBar2.Value = 0;
            IsDown = false;
            if (fileCount == updateFile.Count)
            {
                if (updateFile[0].Name == "FileList.xml")
                {
                    updateFile.RemoveAt(0); // xml 제거
                }
                label5.Text = "업로드 완료.";
                MessageBox.Show("업로드 완료!", "알림");
            }
        }

        private void UploadProgess(object sender, UploadProgressChangedEventArgs e)
        {
            progressBar2.Value = (int)e.BytesReceived;
        }
    }
}
