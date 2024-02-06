using System;
using System.Collections.Generic;

public class Maze
{
    public Space Root { get; set; }
    public List<Space> Spaces { get; } = new();
    public void Reset()
    {
        foreach (var space in Spaces)
            space.Reset();
    }

    public static Maze Prim(int sx, int sy, bool nontree = false)
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

        for (int i = 0; i < 48; i++)
        {
            for (int j = 0; j < 27; j++)
            {
                if (j > 0)
                    vertices[i, j].RealTop = vertices[i, j - 1];
                if (j < 26)
                    vertices[i, j].RealBottom = vertices[i, j + 1];
                if (i > 0)
                    vertices[i, j].RealLeft = vertices[i - 1, j];
                if (i < 47)
                    vertices[i, j].RealRight = vertices[i + 1, j];
                
                if (i == 24 && j == 13)
                    maze.Root = vertices[i, j];
            }
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
            
            Func<bool> nonTreeCond = () => 
                nontree && Random.Shared.NextSingle() < 0.1f 
                && i > 1 && i < 46 && j > 1 && j < 25;
            if (min == top || nonTreeCond())
            {
                var newSpace = add(i, j - 1);
                crr.Top = newSpace;
                newSpace.Bottom = crr;
            }
            if (min == lef || nonTreeCond())
            {
                var newSpace = add(i - 1, j);
                crr.Left = newSpace;
                newSpace.Right = crr;
            }
            if (min == rig || nonTreeCond())
            {
                var newSpace = add(i + 1, j);
                crr.Right = newSpace;
                newSpace.Left = crr;
            }
            if (min == bot || nonTreeCond())
            {
                var newSpace = add(i, j + 1);
                crr.Bottom = newSpace;
                newSpace.Top = crr;
            }

            add(i, j);
        }
    }
}