using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class FileUtils
    {
        public readonly struct FileResult<T>
        {
            public readonly T Data;
            public readonly bool IsSuccess;

            public FileResult(T data, bool isSuccess)
            {
                Data      = data;
                IsSuccess = isSuccess;
            }

            public static FileResult<T> Success(T data) => new(data   , true);
            public static FileResult<T> Failure()       => new(default, false);
        }

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

        public static async Task SaveJson(string filePath, object obj)
        {
            await SaveFileText(filePath, JsonUtility.ToJson(obj));
        }

        public static async Task<FileResult<T>> LoadJson<T>(string path)
        {
            var result = await LoadFileText(path);
            if (result.IsSuccess == false)
            {
                return FileResult<T>.Failure();
            }

            var obj = await Task.Run(() => JsonUtility.FromJson<T>(result.Data));
            return FileResult<T>.Success(obj);
        }

        public static async Task SaveFileText(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.WriteAllTextAsync(path, text);
        }

        public static async Task<FileResult<string>> LoadFileText(string path)
        {
            if (File.Exists(path) == false)
            {
                return FileResult<string>.Failure();
            }

            string text = await File.ReadAllTextAsync(path);
            return FileResult<string>.Success(text);
        }
    }
}
