using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class FileUtils
    {
        public class FileResult
        {
            public readonly bool IsSuccess;
            public readonly string FailMessage;

            public FileResult(bool isSuccess, string failMessage = "")
            {
                IsSuccess   = isSuccess;
                FailMessage = failMessage;
            }

            public static FileResult Success()               => new(true);
            public static FileResult Failure(string message) => new(false, message);
            public static FileResult Failure(Exception ex)   => Failure($"{ex.Message}\n{ex.StackTrace}");
        }

        public class FileResult<T> : FileResult
        {
            public readonly T Data;

            public FileResult(T data, bool isSuccess, string failMessage = "") : base(isSuccess, failMessage)
            {
                Data = data;
            }

            public static     FileResult<T> Success(T data)         => new(data   , true);
            public static new FileResult<T> Failure(string message) => new(default, false, message);
            public static new FileResult<T> Failure(Exception ex)   => Failure($"{ex.Message}\n{ex.StackTrace}");
        }

        const string TEMP_FILE_EXTENSION   = ".tmp";
        const string BACKUP_FILE_EXTENSION = ".bak";

        public static string JoinAllPaths(params string[] paths)
        {
            string path = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                path = Path.Join(path, paths[i]);
            }
            return path;
        }

        public static string GetPersistentDataPath(params string[] paths)
        {
            return Path.Join(Application.persistentDataPath, JoinAllPaths(paths));
        }

    #if UNITY_EDITOR
        public static string GetEditorPath(params string[] paths)
        {
            return Path.Join(Application.dataPath, JoinAllPaths(paths));
        }
    #endif

        public static string GetStreamingAssetsPath(params string[] paths)
        {
            return Path.Join(Application.streamingAssetsPath, JoinAllPaths(paths));
        }

        public static void ReplaceFile(string sourcePath, string targetPath, string backupPath = null)
        {
            if (File.Exists(targetPath))
            {
                if (backupPath != null)
                {
                    DeleteFileIfExists(backupPath);
                    File.Copy(targetPath, backupPath);
                }

                File.Delete(targetPath);
            }

            File.Copy(sourcePath, targetPath);

            DeleteFileIfExists(sourcePath);

            if (backupPath != null)
            {
                DeleteFileIfExists(backupPath);
            }
        }

        public static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static async Task<FileResult> SaveJson(string filePath, object obj)
        {
            return await SaveFileText(filePath, JsonUtility.ToJson(obj));
        }

        public static async Task<FileResult<T>> LoadJson<T>(string path)
        {
            var result = await LoadFileText(path);
            if (result.IsSuccess == false)
            {
                return FileResult<T>.Failure(result.FailMessage);
            }

            var obj = await Task.Run(() => JsonUtility.FromJson<T>(result.Data));
            return FileResult<T>.Success(obj);
        }

        public static async Task<FileResult> SaveFileText(string path, string text)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllTextAsync(path, text);
                return FileResult.Success();
            }
            catch (Exception ex)
            {
                return FileResult.Failure(ex);
            }
        }

        public static async Task<FileResult<string>> LoadFileText(string path)
        {
            if (File.Exists(path) == false)
            {
                return FileResult<string>.Failure($"File does not exist: {path}");
            }

            string text = await File.ReadAllTextAsync(path);
            return FileResult<string>.Success(text);
        }

        public static async Task<FileResult> WriteBinary(string path, Action<BinaryWriter> write)
        {
            Exception writeException = null;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await Task.Run(() =>
                {
                    string tempPath   = path + TEMP_FILE_EXTENSION;
                    string backupPath = path + BACKUP_FILE_EXTENSION;
                    try
                    {
                        DeleteFileIfExists(tempPath);
                        using var stream = File.Create(tempPath);
                        using var writer = new BinaryWriter(stream);
                        write(writer);
                    }
                    catch (Exception ex)
                    {
                        writeException = ex;
                        DeleteFileIfExists(tempPath);
                    }

                    ReplaceFile(tempPath, path, backupPath);
                });
            }
            catch (Exception ex)
            {
                return FileResult.Failure(ex);
            }

            return writeException == null ? FileResult.Success() : FileResult.Failure(writeException);
        }

        public static async Task<FileResult<T>> ReadBinary<T>(string path, Func<BinaryReader, T> read)
        {
            if (File.Exists(path) == false)
            {
                return FileResult<T>.Failure($"File does not exist: {path}");
            }

            return await Task.Run(() =>
            {
                try
                {
                    using var stream = File.OpenRead(path);
                    using var reader = new BinaryReader(stream);
                    return FileResult<T>.Success(read(reader));
                }
                catch (Exception ex)
                {
                    return FileResult<T>.Failure(ex);
                }
            });
        }
    }
}
