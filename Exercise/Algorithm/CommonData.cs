namespace Exercise.Algorithm
{
    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class Location
    {
        public Point leftTop { get; set; }
        public Point rightBottom { get; set; }
    }


    public enum AreaType : int
    {
        SingleChoice = 0,
        FillBlank = 1,
        Answer = 2
    }

    public enum PagingInfo : int
    {
        None = 0,
        Down = 1,
        Up = 2, 
        UpDown = 3
    }

    public class Result<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public T data;
    }

}
