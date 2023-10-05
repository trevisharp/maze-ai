using System;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

App.Open(new MazeView());

public class Maze
{
    public Space Root { get; set; }
    public List<Space> Spaces { get; } = new();
    public void Reset()
    {
        foreach (var space in Spaces)
            space.Reset();
    }

    public static Maze Prim(int sx, int sy)
    {
        Maze maze = new Maze();
        var priority = new PriorityQueue<(int i, int j), byte>();
        byte[,] topgrid = new byte[48, 27];
        byte[,] rightgrid = new byte[48, 27];
        Space[,] vertices = new Space[48, 27];
        int verticeCount = 0;

        for (int i = 0; i < 48; i++)
        {
            for (int j = 0; j < 27; j++)
            {
                topgrid[i, j] = (byte)Random.Shared.Next(255);
                rightgrid[i, j] = (byte)Random.Shared.Next(255);
            }
        }

        maze.Root = add(0, 0);

        while (priority.Count > 0)
        {
            var pos = priority.Dequeue();
            connect(pos.i, pos.j);
        }

        return maze;

        Space add(int i, int j)
        {
            if (vertices[i, j] is null)
            {
                var newSpace = new Space
                {
                    X = i,
                    Y = j,
                    Exit = sx == i && sy == j
                };
                maze.Spaces.Add(newSpace);
                vertices[i, j] = newSpace;
                verticeCount++;
            }
            
            byte top = j == 0  || vertices[i, j - 1] is not null ? byte.MaxValue : topgrid[i, j],
                 bot = j == 26 || vertices[i, j + 1] is not null ? byte.MaxValue : topgrid[i, j + 1],
                 rig = i == 47 || vertices[i + 1, j] is not null ? byte.MaxValue : rightgrid[i, j],
                 lef = i == 0 || vertices[i - 1, j] is not null ? byte.MaxValue : rightgrid[i - 1, j];
            var min = byte.Min(
                byte.Min(top, bot),
                byte.Min(lef, rig)
            );
            if (min == byte.MaxValue)
                return vertices[i, j];

            priority.Enqueue((i, j), min);

            return vertices[i, j];
        }

        void connect(int i, int j)
        {   
            var crr = vertices[i, j];

            byte top = j == 0  || vertices[i, j - 1] is not null ? byte.MaxValue : topgrid[i, j],
                 bot = j == 26 || vertices[i, j + 1] is not null ? byte.MaxValue : topgrid[i, j + 1],
                 rig = i == 47 || vertices[i + 1, j] is not null ? byte.MaxValue : rightgrid[i, j],
                 lef = i == 0 || vertices[i - 1, j] is not null ? byte.MaxValue : rightgrid[i - 1, j];
            var min = byte.Min(
                byte.Min(top, bot),
                byte.Min(lef, rig)
            );
            if (min == byte.MaxValue)
                return;
            
            if (min == top)
            {
                var newSpace = add(i, j - 1);
                crr.Top = newSpace;
                newSpace.Bottom = crr;
            }
            else if (min == lef)
            {
                var newSpace = add(i - 1, j);
                crr.Left = newSpace;
                newSpace.Right = crr;
            }
            else if (min == rig)
            {
                var newSpace = add(i + 1, j);
                crr.Right = newSpace;
                newSpace.Left = crr;
            }
            else if (min == bot)
            {
                var newSpace = add(i, j + 1);
                crr.Bottom = newSpace;
                newSpace.Top = crr;
            }

            add(i, j);
        }
    }

}

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

    public void Reset()
    {
        IsSolution = false;
        Visited = false;
    }
}

public class MazeView : View
{
    int solx = 20;
    int soly = 20;
    bool update = false;
    bool solve = false;
    public Maze Maze { get; set; }
    Solver Solver = new Solver();

    protected override void OnStart(IGraphics g)
    {
        this.Maze = Maze.Prim(solx, soly);
        this.Solver.Maze = this.Maze;
        g.SubscribeKeyDownEvent(key =>
        {
            if (key == Input.Escape)
                App.Close();
            
            if (key == Input.Space)
            {
                this.Maze = Maze.Prim(solx, soly);
                this.Solver.Maze = this.Maze;
                Invalidate();
            }

            if (key == Input.S)
            {
                solve = !solve;
                if (!solve)
                {
                    Maze.Reset();
                    Invalidate();
                }
            }

            if (key == Input.U)
            {
                update = !update;
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.Black);
        foreach (var space in Maze.Spaces)
            drawSpace(space, g);
    }

    protected override void OnFrame(IGraphics g)
    {
        if (update)
        {
            var dx = g.Width / 48;
            var dy = g.Height / 27;
            solx = (int)(g.Cursor.X / dx);
            soly = (int)(g.Cursor.Y / dy);

            foreach (var space in Maze.Spaces)
            {
                space.Exit = space.X == solx && space.Y == soly;
            }

            Invalidate();
        }

        if (solve)
        {
            Maze.Reset();
            Solver.Solve();

            Invalidate();
        }
    }

    void drawSpace(Space space, IGraphics g)
    {
        if (space is null)
            return;
        
        var dx = g.Width / 48;
        var dy = g.Height / 27;
        var x = space.X * dx;
        var y = space.Y * dy;
        g.FillRectangle(x, y, dx, dy,
            space.Exit ? Brushes.Green :
            space.Visited ? Brushes.Blue : Brushes.White
        );

        if (space.IsSolution)
        {
            connect(space, space?.Top, g);
            connect(space, space?.Left, g);
            connect(space, space?.Right, g);
            connect(space, space?.Bottom, g);
        }

        if (space.Left is null)
            g.FillRectangle(x, y, 5, dy, Brushes.Black);
        if (space.Top is null)
            g.FillRectangle(x, y, dx, 5, Brushes.Black);
    }

    void connect(Space a, Space b, IGraphics g)
    {
        if (a is null || b is null)
            return;
        
        if (!a.IsSolution || !b.IsSolution)
            return;

        var dx = g.Width / 48;
        var dy = g.Height / 27;

        var x1 = a.X * dx + dx / 2;
        var y1 = a.Y * dy + dy / 2;
        
        var x2 = b.X * dx + dx / 2;
        var y2 = b.Y * dy + dy / 2;

        var wid = x2 - x1;
        var hei = y2 - y1;
        if (wid < 0)
            wid = -wid;
        if (hei < 0)
            hei = -hei;

        var x = 0;
        var y = 0;
        
        if (wid == 0)
        {
            wid = 12;
            x = x1 - 6;
            if (y1 < y2)
            {
                y = y1 - 3;
                hei += 6;
            }
            else
            {
                y = y2 - 3;
                hei += 6;
            }
        }
        if (hei == 0)
        {
            hei = 12;
            y = y1 - 6;
            if (x1 < x2)
            {
                x = x1 - 3;
                wid += 6;
            }
            else
            {
                x = x2 - 3;
                wid += 6;
            }
        }

        g.FillRectangle(x, y, wid, hei, Brushes.Red);
    }
}