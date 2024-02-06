public class Space
{
    public int X { get; set; }
    public int Y { get; set; }
    public Space Top { get; set; } = null;
    public Space Left { get; set; } = null;
    public Space Right { get; set; } = null;
    public Space Bottom { get; set; } = null;
    public bool Visited { get; set; } = false;
    public bool IsSolution { get; set; } = false;
    public bool Exit { get; set; } = false;
    public Space RealTop { get; set; } = null;
    public Space RealLeft { get; set; } = null;
    public Space RealRight { get; set; } = null;
    public Space RealBottom { get; set; } = null;

    public void Reset()
    {
        IsSolution = false;
        Visited = false;
    }
}