namespace Exercise.Algorithm
{
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Location
    {
        public Point LeftTop { get; set; }
        public Point RightBottom { get; set; }
    }


    public enum AreaType : int
    {
        Choice = 0, // Single, Multiple, Judge
        FillBlank = 1,
        Answer = 2,
        Judge = 3,
    }

    public enum QuestionType : int
    {
        None = 0,
        SingleChoice = 1, 
        MultipleChoice = 2, 
        Judge = 3,
        FillBlank = 3,
        Answer = 4,
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
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

}
