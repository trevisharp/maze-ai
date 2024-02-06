using Pamella;
using System.Drawing;

App.Open(new MazeView());

public class MazeView : View
{
    bool help = false;
    bool nonTreeMode = false;
    int solx = 20;
    int soly = 20;
    bool update = false;
    bool solve = false;
    public Maze Maze { get; set; }
    Solver Solver = new Solver();

    protected override void OnStart(IGraphics g)
    {
        this.Maze = Maze.Prim(solx, soly, nonTreeMode);
        this.Solver.Maze = this.Maze;
        g.SubscribeKeyDownEvent(key =>
        {
            if (key == Input.Escape)
                App.Close();
            
            if (key == Input.Space)
            {
                this.Maze = Maze.Prim(solx, soly, nonTreeMode);
                this.Solver.Maze = this.Maze;
                Invalidate();
            }
            
            if (key == Input.T)
            {
                nonTreeMode = !nonTreeMode;
                this.Maze = Maze.Prim(solx, soly, nonTreeMode);
                this.Solver.Maze = this.Maze;
                Invalidate();
            }

            if (key == Input.H)
            {
                help = !help;
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

            if (key == Input.A)
            {
                Maze.Reset();
                this.Solver.Option++;
            }

            if (key == Input.R)
            {
                this.Solver.RogueMode = !this.Solver.RogueMode;
                Maze.Reset();
                this.Solver.Option = 0;
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {   
        int y = 40;
        if (help)
        {
            g.Clear(Color.White);
            write($"Algoritmo Atual: {Solver.Algorithm}.");
            write("h - Abrir/Fechar tela de ajuda.");
            write("Esc - Fechar aplicação.");
            write("Space - Regerar labirinto.");
            write("S - Ligar/Desligar resolução do labirinto.");
            write("U - Iniciar/Desligar atualização da saída.");
            write("A - Mudar algorítimo.");
            write("T - Iniciar/Desligar modo não-árvore.");
            write("R - Ligar/Desligar Rogue Mode (permite-se pular paredes com custo 0, 2, 4...).");
            return;
        }

        g.Clear(Color.Black);
        foreach (var space in Maze.Spaces)
            drawSpace(space, g);
        
        void write(string text)
        {
            g.DrawText(
                new RectangleF(0, y, g.Width, 40),
                Brushes.Red,
                text
            );
            y += 40;
        }
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
            space == Maze.Root ? Brushes.Yellow :
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