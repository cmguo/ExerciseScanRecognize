﻿namespace Exercise.Algorithm
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Location
    {
        public Point LeftTop { get; set; }
        public Point RightBottom { get; set; }
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
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

}
