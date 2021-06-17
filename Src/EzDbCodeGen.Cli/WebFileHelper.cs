using System.Net;
using System.IO;
using System.IO.Compression;
namespace EzDbCodeGen.Cli
{
    public static class WebFileHelper
    {
        /// <summary>
        /// Will change all the text in a file
        /// </summary>
        /// <param name="FileToChange"></param>
        /// <param name="oldString"></param>
        /// <param name="newString"></param>
        /// <returns>The name of the file to change</returns>
        public static string ReplaceAll(this string FileToChange, string oldString, string newString)
        {
            return ReplaceAllText(FileToChange, oldString, newString);
        }

        public static string CopyTo(this string FileToCopy, string sourcePath, string targetPath, string FileToRenameTo = "")
        {
            return WebFileHelper.CopyToPath(FileToCopy, sourcePath, targetPath, FileToRenameTo);
        }

        public static string CopyToPath(string FileToCopy, string sourcePath, string targetPath, string FileToRenameTo="")
        {
            if (FileToRenameTo.Length == 0) FileToRenameTo = FileToCopy;
            var targetFileName = $"{targetPath}{FileToRenameTo}";
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
            File.Copy($"{sourcePath}{FileToCopy}", targetFileName, true);
            System.Console.WriteLine($"Copying {FileToCopy} into {targetFileName}", FileToCopy, targetFileName);
            return targetFileName;
        }

        public static string ReplaceAllText(string FileToChange, string oldString, string newString)
        {
            File.WriteAllText(FileToChange, File.ReadAllText(FileToChange).Replace(oldString, newString));
            return FileToChange;
        }

        public static string CurlGitRepoZip(string destinationPath, string user="rvegajr", string repo="ez-db-codegen-core", string branch="master")
        {

            var sourceUrl = string.Format("https://github.com/{0}/{1}/archive/{2}.zip", user, repo, branch);
            WebClient Client = new WebClient();
            System.IO.Directory.CreateDirectory(destinationPath);
            var targetGitRepoZip = destinationPath + repo + ".zip";
            Client.DownloadFile(sourceUrl, targetGitRepoZip);
            ZipFile.ExtractToDirectory(targetGitRepoZip, destinationPath);
            if (File.Exists(targetGitRepoZip)) File.Delete(targetGitRepoZip);
            System.Console.WriteLine(string.Format("Git repo downloaded and extracted"));
            return destinationPath;
        }
        public static void DownloadFile(string sourceURL, string destinationPath)
        {
            long fileSize = 0;
            int bufferSize = 1024;
            bufferSize *= 1000;
            long existLen = 0;

            System.IO.FileStream saveFileStream;
            if (System.IO.File.Exists(destinationPath))
            {
                System.IO.FileInfo destinationFileInfo = new System.IO.FileInfo(destinationPath);
                existLen = destinationFileInfo.Length;
            }

            if (existLen > 0)
                saveFileStream = new System.IO.FileStream(destinationPath,
                                                          System.IO.FileMode.Append,
                                                          System.IO.FileAccess.Write,
                                                          System.IO.FileShare.ReadWrite);
            else
                saveFileStream = new System.IO.FileStream(destinationPath,
                                                          System.IO.FileMode.Create,
                                                          System.IO.FileAccess.Write,
                                                          System.IO.FileShare.ReadWrite);

            System.Net.HttpWebRequest httpReq;
            System.Net.HttpWebResponse httpRes;
            httpReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(sourceURL);
            httpReq.AddRange((int)existLen);
            System.IO.Stream resStream;
            httpRes = (System.Net.HttpWebResponse)httpReq.GetResponse();
            resStream = httpRes.GetResponseStream();

            fileSize = httpRes.ContentLength;

            int byteSize;
            byte[] downBuffer = new byte[bufferSize];

            while ((byteSize = resStream.Read(downBuffer, 0, downBuffer.Length)) > 0)
            {
                saveFileStream.Write(downBuffer, 0, byteSize);
            }
        }
    }
}
