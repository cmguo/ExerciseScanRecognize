using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Base.Misc
{
    public class JsonPersistent
    {
        public static T Load<T>(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public static void Save<T>(string path, T data)
        {
            using (FileStream fs = new FileStream(path + "temp", FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Serialize(jsonWriter, data);
                jsonWriter.Flush();
            }
            if (File.Exists(path))
                File.Delete(path);
            File.Move(path + "temp", path);
        }

        public static async Task SaveAsync<T>(string path, T data)
        {
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path + "temp", FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer ser = new JsonSerializer();
                    ser.Serialize(jsonWriter, data);
                    jsonWriter.Flush();
                }
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(path + "temp", path);
            });
        }

        public static async Task<T> LoadAsync<T>(string path)
        {
            return await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer ser = new JsonSerializer();
                    return ser.Deserialize<T>(jsonReader);
                }
            });
        }

    }
}
