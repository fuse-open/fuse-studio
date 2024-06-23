using System;
using System.IO;
using System.Net;
using System.Text;
using Uno.UXNinja.PerformanceTests.Core.Loggers.LoggersEntities;

namespace Uno.UXNinja.PerformanceTests.Core.Loggers
{
    public class ResultFtpLogger : BaseResultLogger
    {
        public ResultFtpLogger(string logDirectoryName, string branchName, string buildNumber)
            : base(logDirectoryName, branchName, buildNumber)
        {
            CreateDirectory(_logDirectoryPath);
        }

        #region Override Methods

        public override void ProjectStarted(string projectName)
        {
            base.ProjectStarted(projectName);
            DeleteFileIfExists(_logFileName);
        }

        public override void ProjectFinished()
        {
            if (_project != null)
                UploadFile(_logFileName);
        }

        protected override string GetLogDirectoryFullPath(DateTime currDate, string logDirectoryName, string buildNumber)
        {
            var sessionDirectoryName = string.IsNullOrEmpty(buildNumber) ? currDate.Ticks.ToString() : buildNumber;

            var baseUri = new Uri(logDirectoryName);
            var dateDirectoryUri = string.IsNullOrEmpty(_branchName) ? new Uri(baseUri, string.Format("unnamed/{0}/", currDate.ToString("MM_dd_yyy")))
                                                                     : new Uri(baseUri, string.Format("{0}/{1}/", _branchName, currDate.ToString("MM_dd_yyy")));

            var num = GetNumberOfSessionDirectories(dateDirectoryUri.ToString());
            return new Uri(dateDirectoryUri, string.Format("{0}__{1}/", num + 1, sessionDirectoryName)).ToString();
        }

        #endregion

        #region Private Methods

        private void DeleteFileIfExists(string path)
        {
            var request = GetFtpWebRequest(path);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                response.Close();
                if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw ex;
                }
            }
        }

        private void CreateDirectory(string path)
        {
            var request = GetFtpWebRequest(path);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                response.Close();
                if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw ex;
                }
            }
        }

        private void UploadFile(string path)
        {
            var content = Encoding.UTF8.GetBytes(XmlTools.ToXmlString<ProjectEntity>(_project));
            var request = GetFtpWebRequest(path);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.ContentLength = content.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(content, 0, content.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
        }

        private int GetNumberOfSessionDirectories(string path)
        {
            var res = 0;
            var request = GetFtpWebRequest(path);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    res = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length;
                }
                response.Close();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                response.Close();
                if (response.StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    throw ex;
                }
            }
            return res;
        }

        private FtpWebRequest GetFtpWebRequest(string requestUri)
        {
            return (FtpWebRequest)FtpWebRequest.Create(requestUri);
        }

        #endregion
    }
}
