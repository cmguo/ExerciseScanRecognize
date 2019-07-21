using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Base.Misc
{
    public class JsonPersistent
    {
        public static async Task Save<T>(string path, T data)
        {
            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path + "temp", FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
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

        public static async Task<T> Load<T>(string path)
        {
            return await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (StreamReader reader = new StreamReader(fs))
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer ser = new JsonSerializer();
                    return ser.Deserialize<T>(jsonReader);
                }
            });
        }

    }
}
