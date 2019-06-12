using System.Threading.Tasks;

namespace Exercise.Algorithm
{
    public class Algorithm
    {
        public QRCodeData GetCode(PageRaw page)
        {
            return new QRCodeData() { paperInfo = "", studentInfo = "" };
        }

        public AnswerData GetAnswer(PageData page)
        {
            return new AnswerData() { imgWidth = 720, imgHeight = 1080 };
        }
    }
}
