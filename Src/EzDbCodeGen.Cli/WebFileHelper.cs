using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System;
using System.Threading.Tasks;
namespace EzDbCodeGen.Cli;

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

    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<string> CurlGitRepoZip(string destinationPath, string user="rvegajr", string repo="ez-db-codegen-core", string branch="master")
    {
        var sourceUrl = string.Format("https://github.com/{0}/{1}/archive/{2}.zip", user, repo, branch);
        System.IO.Directory.CreateDirectory(destinationPath);
        var targetGitRepoZip = destinationPath + repo + ".zip";

        using (var response = await httpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(targetGitRepoZip))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        ZipFile.ExtractToDirectory(targetGitRepoZip, destinationPath);
        if (File.Exists(targetGitRepoZip)) File.Delete(targetGitRepoZip);
        System.Console.WriteLine("Git repo downloaded and extracted");
        return destinationPath;
    }
    public static async Task DownloadFile(string sourceURL, string destinationPath)
    {
        var fileMode = File.Exists(destinationPath) ? FileMode.Append : FileMode.Create;
        
        using (var response = await httpClient.GetAsync(sourceURL, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(destinationPath, fileMode, FileAccess.Write, FileShare.ReadWrite))
            {
                if (fileMode == FileMode.Append)
                {
                    fileStream.Seek(0, SeekOrigin.End);
                }
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}
