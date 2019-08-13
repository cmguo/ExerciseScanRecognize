using Base.Events;

namespace Assistant.Update
{
    [Topic("app/update")]
    public class UpdateMessage
    {
        public string Url { get; set; }
        public string Md5 { get; set; }
        public string Version { get; set; }
        public string Modify { get; set; }
    }
}
