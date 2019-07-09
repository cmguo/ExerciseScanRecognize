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
            string json = JsonConvert.SerializeObject(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static async Task<T> Load<T>(string path)
        {
            string json = null;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                await fs.ReadAsync(bytes, 0, bytes.Length);
                json = Encoding.UTF8.GetString(bytes);
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

    }
}
