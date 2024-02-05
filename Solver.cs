using System;
using System.Linq;
using System.Collections.Generic;
using System.Media;

public class Solver
{
    public int Option { get; set; }
    public bool RogueMode { get; set; }
    public Maze Maze { get; set; }
    
    public void Solve()
    {
        var goal = Maze.Spaces
            .FirstOrDefault(s => s.Exit);

        if (RogueMode)
        {
            rogueAStar(Maze.Root, goal);
        }
        else
        {
            if (Option % 4 == 0)
                DFS(Maze.Root, goal);
            else if (Option % 4 == 1)
                BFS(Maze.Root, goal);
            else if (Option % 4 == 2)
                dijkstra(Maze.Root, goal);
            else aStar(Maze.Root, goal);
        }
    }

    private bool DFS(Space space, Space goal)
    {
        if (space.Visited)
            return false;
        space.Visited = true;

        if (space == goal)
        {
            space.IsSolution = true;
            return true;
        }

        space.IsSolution = 
            space.Right is not null && DFS(space.Right, goal) ||
            space.Bottom is not null && DFS(space.Bottom, goal) ||
            space.Left is not null && DFS(space.Left, goal) ||
            space.Top is not null && DFS(space.Top, goal);

        return space.IsSolution;
    }

    private void BFS(Space start, Space goal)
    {
        var queue = new Queue<Space>();
        var comeMap = new Dictionary<Space, Space>();
        queue.Enqueue(start);
        
        while (queue.Count > 0)
        {
            var crr = queue.Dequeue();
            crr.Visited = true;
            
            if (crr == goal)
                break;
            
            var neighborhood = new Space[] {
                crr.Top, crr.Bottom, crr.Left, crr.Right
            };
            foreach (var neighbor in neighborhood)
            {
                if (neighbor is null || neighbor.Visited)
                    continue;
                
                comeMap.Add(neighbor, crr);
                queue.Enqueue(neighbor);
            }
        }

        var it = goal;
        while (it != start)
        {
            it.IsSolution = true;
            it = comeMap[it];
        }
        start.IsSolution = true;
    }

    private void dijkstra(Space start, Space goal)
    {
        var queue = new PriorityQueue<Space, float>();
        var distMap = new Dictionary<Space, float>();
        var comeMap = new Dictionary<Space, Space>();

        distMap[start] = 0;
        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            var crr = queue.Dequeue();
            crr.Visited = true;

            if (crr == goal)
                break;
            
            var neighborhood = new Space[] {
                crr.Top, crr.Bottom, crr.Left, crr.Right
            };
            foreach (var neighbor in neighborhood)
            {
                if (neighbor is null)
                    continue;

                if (!distMap.ContainsKey(neighbor))
                {
                    distMap.Add(neighbor, float.PositiveInfinity);
                    comeMap.Add(neighbor, null);
                }
                
                var newDist = distMap[crr] + 1;
                var oldDist = distMap[neighbor];
                if (newDist > oldDist)
                    continue;
                
                distMap[neighbor] = newDist;
                comeMap[neighbor] = crr;
                queue.Enqueue(neighbor, newDist);
            }
        }

        var it = goal;
        while (it != start)
        {
            it.IsSolution = true;
            it = comeMap[it];
        }
        start.IsSolution = true;
    }

    private void aStar(Space start, Space goal)
    {
        var queue = new PriorityQueue<Space, float>();
        var distMap = new Dictionary<Space, float>();
        var comeMap = new Dictionary<Space, Space>();

        distMap[start] = 0;
        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            var crr = queue.Dequeue();
            crr.Visited = true;

            if (crr == goal)
                break;
            
            var neighborhood = new Space[] {
                crr.Top, crr.Bottom, crr.Left, crr.Right
            };
            foreach (var neighbor in neighborhood)
            {
                if (neighbor is null)
                    continue;

                if (!distMap.ContainsKey(neighbor))
                {
                    distMap.Add(neighbor, float.PositiveInfinity);
                    comeMap.Add(neighbor, null);
                }
                
                var newDist = distMap[crr] + 1;
                var oldDist = distMap[neighbor];
                if (newDist > oldDist)
                    continue;
                
                distMap[neighbor] = newDist;
                comeMap[neighbor] = crr;

                var dx = neighbor.X - goal.X;
                var dy = neighbor.Y - goal.Y;
                var h = MathF.Sqrt(dx * dx + dy * dy);
                queue.Enqueue(neighbor, newDist + h);
            }
        }

        var it = goal;
        while (it != start)
        {
            it.IsSolution = true;
            it = comeMap[it];
        }
        start.IsSolution = true;
    }
    
    private void rogueAStar(Space start, Space goal)
    {
        var queue = new PriorityQueue<(Space space, int jumpCost), float>();
        var distMap = new Dictionary<(Space space, int jumpCost), float>();
        var comeMap = new Dictionary<(Space space, int jumpCost), (Space space, int jumpCost)>();

        var solution = (goal, 0);
        distMap[(start, 1)] = 0;
        queue.Enqueue((start, 1), 0);

        while (queue.Count > 0)
        {
            var crr = queue.Dequeue();
            crr.space.Visited = true;
            if (crr.space == goal)
            {
                solution = (goal, crr.jumpCost);
                break;
            }
            
            var neighborhood = new Space[] {
                crr.space.Top, crr.space.Bottom, crr.space.Left, crr.space.Right
            };
            foreach (var neighbor in neighborhood)
            {
                if (neighbor is null)
                    continue;
                var next = (neighbor, crr.jumpCost);

                if (!distMap.ContainsKey(next))
                {
                    distMap.Add(next, float.PositiveInfinity);
                    comeMap.Add(next, (null, 0));
                }
                
                var newDist = distMap[crr] + 1;
                var oldDist = distMap[next];
                if (newDist > oldDist)
                    continue;
                
                distMap[next] = newDist;
                comeMap[next] = crr;

                var dx = neighbor.X - goal.X;
                var dy = neighbor.Y - goal.Y;
                var h = MathF.Sqrt(dx * dx + dy * dy);
                queue.Enqueue(next, newDist + h);
            }

            var jumpNeighborhood = new Space[] {
                crr.space.RealTop, crr.space.RealBottom, crr.space.RealLeft, crr.space.RealRight
            };
            foreach (var neighbor in jumpNeighborhood)
            {
                if (neighbor is null)
                    continue;
                var next = (neighbor, crr.jumpCost + 2);
                
                if (!distMap.ContainsKey(next))
                {
                    distMap.Add(next, float.PositiveInfinity);
                    comeMap.Add(next, (null, 0));
                }
                
                var newDist = distMap[crr] + crr.jumpCost;
                var oldDist = distMap[next];
                if (newDist > oldDist)
                    continue;
                
                distMap[next] = newDist;
                comeMap[next] = crr;

                var dx = neighbor.X - goal.X;
                var dy = neighbor.Y - goal.Y;
                var h = MathF.Sqrt(dx * dx + dy * dy);
                queue.Enqueue(next, newDist + h);
            }
        }

        var it = solution;
        while (it != (start, 1))
        {
            it.goal.IsSolution = true;
            it = comeMap[it];
        }
        start.IsSolution = true;
    }
}