using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Xml;

using Update.Core;

namespace Update
{

    public struct FileList
    {
        public string Name;
        public string Ver;
        public string Path;
        public DateTime Time;
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<FileList> localFileInfo = new List<FileList>();
        List<FileList> updateFileInfo = new List<FileList>();
        List<FileList> updateFileList = new List<FileList>();

        WebClient wc = new WebClient();
        bool IsDown = false;
        bool updateCheck = false;
        bool localXmlFileExists = false;
        string ftpServerUri = "ftp서버";
        string xmlName = "UpFileList.xml";
        string localXmlPath = Application.StartupPath + @"\FileList.xml";

        int fileCount = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadList();

            CheckUpdate();

            label5.Text = "업데이트 확인 완료. 총 " + updateFileList.Count + "개의 업데이트가 있습니다.";

        }

        private void LoadList()
        {
            label5.Text = "파일 목록 가져오는중...";
            listView1.Clear();
            int localFileLoadResult = FileListLoad(localXmlPath);
            if (localFileLoadResult == 0)
            {
                int result = UpFileListLoad(ftpServerUri + xmlName);

                for (int i = 0; i < localFileInfo.Count; i++)
                {
                    label5.Text = "파일 목록 추가 하는중...";
                    listView1.Items.Add(localFileInfo[i].Name);
                    listView1.Items[i].SubItems.Add(localFileInfo[i].Ver.ToString());
                    if (result != -1)
                    {
                        listView1.Items[i].SubItems.Add(updateFileInfo[i].Ver.ToString());
                        listView1.Items[i].SubItems.Add("업데이트 버전 확인중.");
                    }
                    else
                    {
                        listView1.Items[i].SubItems.Add("확인 실패!");
                        listView1.Items[i].SubItems.Add("업데이트 버전 확인 실패!");
                    }

                    listView1.Items[i].SubItems.Add(localFileInfo[i].Path);
                }
            }
            else if (localFileLoadResult == 1)
            {
                localXmlFileExists = false;
                UpFileListLoad(ftpServerUri + xmlName);

                MessageBox.Show("파일목록 파일이 존재하지 않아 파일을 다운로드 받습니다. ", "알림");
            }
            else
            {
                MessageBox.Show("로컬 파일 목록을 가져오지 못했습니다.", "오류");
            }
        }

        private void NewFileAdd()
        {
            if(localFileInfo.Count < updateFileInfo.Count)
            {
                int count = updateFileInfo.Count-localFileInfo.Count;
                for (int i = localFileInfo.Count; i < count; i++)
                {
                    updateFileList.Add(updateFileInfo[i]);
                }
            }
        }

        private void CheckUpdate()
        {
            label5.Text = "버전 확인 ...";
            if (localFileInfo.Count > 0 && updateFileInfo.Count > 0)
            {
                for (int i = 0; i < localFileInfo.Count; i++)
                {
                    if (string.IsNullOrEmpty(localFileInfo[i].Ver) == false && string.IsNullOrEmpty(updateFileInfo[i].Ver) == false)
                    {
                        if (Ver.VersionCompare(localFileInfo[i].Ver, updateFileInfo[i].Ver) == 1) //ver2가 더 높음
                        {
                            updateFileList.Add(updateFileInfo[i]);
                            listView1.Items[i].SubItems[3].Text = "업데이트 필요!";
                        }
                        else
                            listView1.Items[i].SubItems[3].Text = "최신!";
                        updateCheck = true;
                    }
                    else if (localFileInfo[i].Time != DateTime.MinValue && updateFileInfo[i].Time != DateTime.MinValue)
                    {
                        if (DateTime.Compare(localFileInfo[i].Time, updateFileInfo[i].Time) == -1) // t2가 더 높음
                        {
                            updateFileList.Add(updateFileInfo[i]);
                            listView1.Items[i].SubItems[3].Text = "업데이트 필요!";
                        }
                        else
                            listView1.Items[i].SubItems[3].Text = "최신!";
                        updateCheck = true;
                    }
                    else
                    {
                        updateCheck = false;
                        label5.Text = "오류> 확인할 수 버전 또는 날짜가 없습니다.!";
                        listView1.Items[i].SubItems[3].Text = "오류> 확인 실패!";
                    }
                }
                NewFileAdd();
            }
            else if (localFileInfo.Count == 0 && updateFileInfo.Count >= 0 && !localXmlFileExists)
            {
                for (int i = 0; i < updateFileInfo.Count; i++)
                {
                    updateFileList.Add(updateFileInfo[i]);
                }
                listView1.Items.Add("파일 다운로드 준비 완료.");
                updateCheck = true;
            }
            else
            {
                updateCheck = false;
                label5.Text = "오류> 파일 목록이 존재 하지 않습니다.!";
                MessageBox.Show("파일 목록이 존재하지 않습니다.", "오류");
            }

        }

        private void XmlSave(string contents)
        {
            File.WriteAllText(localXmlPath, contents,Encoding.UTF8);
        }


        public int UpFileListLoad(string loadpath)
        {
            int result = 0;
            try
            {
                FtpWebRequest fwrq = (FtpWebRequest)WebRequest.Create(loadpath);
                fwrq.Method = WebRequestMethods.Ftp.DownloadFile;
                fwrq.Credentials = new NetworkCredential("aaa", "");

                FtpWebResponse fwrp = (FtpWebResponse)fwrq.GetResponse();

                using (Stream stream = fwrp.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string contents = sr.ReadToEnd();

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(contents);
                        XmlNodeList root = doc.SelectNodes("UPDATE/FILE");

                        foreach (XmlNode str in root)
                        {
                            FileList fileList = new FileList();
                            fileList.Name = str.Attributes["name"].InnerText;
                            fileList.Ver = str.Attributes["ver"].InnerText;
                            fileList.Path = str.Attributes["path"].InnerText;
                            fileList.Time = Convert.ToDateTime(str.Attributes["time"].InnerText);
                            updateFileInfo.Add(fileList);
                        }

                        if (!localXmlFileExists)
                        {
                            XmlSave(contents);
                        }
                    }
                }

                fwrp.Close();
            }
            catch (Exception ex)
            {
                result = -1;
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        public int FileListLoad(string file)
        {
            int result = 0;
            try
            {
                if (File.Exists(file))
                {
                    localXmlFileExists = true;
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    XmlNodeList root = doc.SelectNodes("UPDATE/FILE");
                    foreach (XmlNode str in root)
                    {
                        FileList fileList = new FileList();
                        fileList.Name = str.Attributes["name"].InnerText;
                        fileList.Ver = str.Attributes["ver"].InnerText;
                        fileList.Path = str.Attributes["path"].InnerText;
                        fileList.Time = Convert.ToDateTime(str.Attributes["time"].InnerText);
                        localFileInfo.Add(fileList);
                    }
                }
                else
                {
                    result = 1;
                }
            }
            catch (Exception ex)
            {
                result = -1;
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        private long GetFileSize(string path)
        {
            long size = 0;
            try
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(path);
                ftpWebRequest.Credentials = new NetworkCredential("aaa", "");
                ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;

                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                size = ftpWebResponse.ContentLength;
                ftpWebResponse.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return size;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (updateCheck)
            {
                if (updateFileList.Count > 0)
                {
                    //기타 셋팅
                    progressBar1.Maximum = localFileInfo.Count;
                    progressBar1.Step = 1;

                    while (fileCount < updateFileList.Count)
                    {
                        if (!IsDown)
                        {
                            FileDownload();
                            label5.Text = "업데이트 중...";
                        }
                        Application.DoEvents(); //while 에서 해당 함수가 없으면 이벤트 진행이 안됨
                    }
                }
                else
                    MessageBox.Show("업데이트 할 파일이 없습니다.", "알림");
            }
        }

        private void FileDownload()
        {
            string fileUri = updateFileList[fileCount].Path;
            string fileName = updateFileList[fileCount].Name;
            int fileSize = (int)GetFileSize(ftpServerUri + fileUri + fileName);
            fileCount++;
            IsDown = true;

            progressBar2.Maximum = fileSize;

            wc.Credentials = new NetworkCredential("aaa", "");
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(FileCompleted);
            wc.DownloadFileAsync(new Uri(ftpServerUri + fileUri + fileName), Application.StartupPath + @"\" + fileUri + fileName);
        }

        private void FileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            progressBar1.PerformStep();
            progressBar2.Value = 0;
            IsDown = false;

            if (fileCount == updateFileList.Count)
            {
                updateFileList.Clear();
                label5.Text = "업데이트 완료.";
                MessageBox.Show("업데이트 완료!", "알림");
                LoadList();

                CheckUpdate();
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //progressBar2.Maximum = (int)e.TotalBytesToReceive;
            progressBar2.Value = (int)e.BytesReceived;
        }
    }
}
