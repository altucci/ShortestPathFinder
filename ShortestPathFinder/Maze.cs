using System.Diagnostics;

namespace ShortestPathFinder
{
    public class Maze
    {
        internal struct Cell
        {
            public int row, column, right, top, visited, rvisited, tvisited, obstacle, solution, solvepath, direction, identity, opened, closed, filled;

            public double f_cost, g_pathLength, h_manhattanDistance, h_chebyshevDistance, h_octileDistance, h_vectorCrossProduct, h_euclideanDistance;

            public Color color;
        }

        internal class MazeCell
        {
            public int column, row, direction;

            public MazeCell(int c, int r, int d = 0)
            {
                column = c;
                row = r;
                direction = d;
            }
        }

        Graphics mazeCanvas;
        Color colorBackground, colorForeground, colorCurrentCell, colorGenerate, colorSolve, colorSolvePath, colorBacktrack, colorStart, colorStop;
        
        Cell[,] grid;
        Stack<MazeCell> visitedCellsStack;
        Queue<MazeCell> visitedCellsQueue;
        PriorityQueue<Cell, (double, double, double, long)> openCells;
        PriorityQueue<Cell, (double, long)> openCells2;

        int height, width;
        
        int visited, genVisited, solveVisited;
        int hunt, kill;
        int solvePathLength;
        int genSpeed, genSpeed_exp;
        int solveSpeed, solveSpeed_exp;
        int sleepTime;
        PointF startGen, stopGen;
        Point startSolve, stopSolve;
        string genAlgorithmName, solveAlgorithmName;
        int genAlgorithmType, solveAlgorithmType;

        int r_current;
        int c_current;

        public bool runningGen;
        public bool resetGen;
        public bool runningSolve;
        public bool resetSolve;

        bool forceTurns;
        bool showGen;
        bool showTracer;
        bool showBacktracks;
        bool showSolve;

        bool allowRooms;
        bool allowLoops;
        bool allowObstacles;
        bool allowIslands;

        bool inverseColors;

        float start_x, start_y, spacer, padding_x, padding_y, side_length, wall_thickness, cell_padding;
        int startCorner;

        bool allowFillCell;
        bool resetStart;

        bool displayGridFlag;

        Random rng;

        Stopwatch stopwatch;

        public Maze(Graphics g, Color c)
        {
            mazeCanvas = g;

            colorBackground = c;
            colorForeground = Color.Black;
            colorCurrentCell = Color.Blue;
            colorGenerate = Color.LightBlue;
            colorSolve = Color.Blue;
            colorSolvePath = Color.CornflowerBlue;
            colorBacktrack = colorBackground;
            colorStart = Color.Green;
            colorStop = Color.Red;

            height = 60;
            width = 60;

            grid = new Cell[height, width];

            visitedCellsStack = new Stack<MazeCell>();
            visitedCellsQueue = new Queue<MazeCell>();

            openCells = new PriorityQueue<Cell, (double, double, double, long)>();
            openCells2 = new PriorityQueue<Cell, (double, long)>();

            rng = new Random(Guid.NewGuid().GetHashCode());

            genAlgorithmName = "Recursive Backtracker";
            genAlgorithmType = 0;
            genSpeed = 32;
            genSpeed_exp = 5;
            solveAlgorithmName = "A* - Euclidean";
            solveAlgorithmType = 0;
            solveSpeed = 8;
            solveSpeed_exp = 3;
            sleepTime = 50;

            r_current = 0;
            c_current = 0;

            runningGen = false;
            resetGen = false;
            runningSolve = true;
            resetSolve = true;

            forceTurns = false;
            showGen = false;

            showTracer = false;
            showBacktracks = true;
            showSolve = true;

            allowRooms = true;
            allowLoops = true;
            allowObstacles = true;
            allowIslands = true;

            inverseColors = false;

            allowFillCell = false;
            resetStart = false;

            padding_x = 50;
            padding_y = 50;

            stopwatch = new Stopwatch();

            initializeVars();
        }

        void initializeVars()
        {
            grid = new Cell[height, width];

            calculateCoords();

            if (visitedCellsStack.Count > 0)
                visitedCellsStack.Clear();

            if (visitedCellsQueue.Count > 0)
                visitedCellsQueue.Clear();

            if (openCells.Count > 0)
                openCells.Clear();

            displayGridFlag = true;
        }

        void initializeGrid()
        {
            initializeVars();

            findStart();

            visited = 0;
            genVisited = 0;
            solveVisited = 0;

            solvePathLength = 0;

            hunt = ((height * width) + 1);
            kill = 0;

            int count = 0;

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    grid[r, c].right = 0;
                    grid[r, c].top = 0;
                    grid[r, c].visited = 0;
                    grid[r, c].rvisited = 0;
                    grid[r, c].tvisited = 0;
                    grid[r, c].solution = 0;
                    grid[r, c].solvepath = 0;
                    grid[r, c].direction = 0;
                    grid[r, c].opened = 0;
                    grid[r, c].closed = 0;
                    grid[r, c].obstacle = 0;
                    grid[r, c].filled = 0;
                    grid[r, c].f_cost = 0;
                    grid[r, c].g_pathLength = 0;
                    grid[r, c].h_manhattanDistance = 0;
                    grid[r, c].h_euclideanDistance = 0;
                    grid[r, c].identity = count;
                    grid[r, c].color = Color.FromArgb(rng.Next(0, 256), rng.Next(0, 256), rng.Next(0, 256));
                    count++;
                }
            }

            r_current = -1;
            c_current = -1;

            mazeCanvas.Clear(colorBackground);
        }

        public void setGraphics(Graphics g)
        {
            mazeCanvas = g;
        }

        public void setLocation(float pad_x, float pad_y, float side_len)
        {
            padding_x = pad_x;
            padding_y = pad_y;
            side_length = side_len;
        }

        public int getHeight()
        {
            return height;
        }

        public void setHeight(int h)
        {
            height = h;
        }

        public int getWidth()
        {
            return width;
        }

        public void setWidth(int w)
        {
            width = w;
        }

        public bool getForceTurns()
        {
            return forceTurns;
        }

        public void setForceTurns(bool f)
        {
            forceTurns = f;
        }

        public bool getShowGen()
        {
            return showGen;
        }

        public void setShowGen(bool s)
        {
            showGen = s;
        }

        public bool getAllowLoops()
        {
            return allowLoops;
        }

        public void setAllowLoops(bool l)
        {
            allowLoops = l;
        }

        public bool getAllowRooms()
        {
            return allowRooms;
        }

        public void setAllowRooms(bool l)
        {
            allowRooms = l;
        }

        public bool getAllowIslands()
        {
            return allowIslands;
        }

        public void setAllowIslands(bool l)
        {
            allowIslands = l;
        }

        public int getGenSpeed()
        {
            return (genSpeed_exp * 5);
        }

        public void setGenSpeed(int g)
        {
            genSpeed_exp = (int)(g / 5);
            genSpeed = (int)Math.Pow(2D, (double)genSpeed_exp);
        }

        public bool getInverseColors()
        {
            return inverseColors;
        }

        public void setInverseColors(bool s)
        {
            inverseColors = s;

            if (inverseColors == true)
            {
                colorForeground = colorBackground;
                colorBackground = Color.Black;
                colorCurrentCell = Color.Red;
                colorGenerate = Color.Pink;
                colorSolve = Color.Red;
                colorSolvePath = Color.PaleVioletRed;
                colorStart = Color.Lime;
                colorStop = Color.Yellow;
            }
            else
            {
                colorBackground = colorForeground;
                colorForeground = Color.Black;
                colorCurrentCell = Color.Blue;
                colorGenerate = Color.LightBlue;
                colorSolve = Color.Blue;
                colorSolvePath = Color.CornflowerBlue;
                colorStart = Color.Green;
                colorStop = Color.Red;
            }

            setShowBacktracks(showBacktracks);

            GenerateGrid();
        }

        public bool getShowTracer()
        {
            return showTracer;
        }

        public void setShowTracer(bool t)
        {
            showTracer = t;
        }

        public bool getShowBacktracks()
        {
            return showBacktracks;
        }

        public void setShowBacktracks(bool b)
        {
            showBacktracks = b;

            if (showBacktracks)
            {
                if (inverseColors)
                {
                    colorBacktrack = Color.Pink;
                }
                else
                {
                    colorBacktrack = Color.LightBlue;
                }
            }
            else
            {
                colorBacktrack = colorBackground;
            }
        }

        public bool getShowSolve()
        {
            return showSolve;
        }

        public void setShowSolve(bool s)
        {
            showSolve = s;
        }

        public int getSolveSpeed()
        {
            return (solveSpeed_exp * 5);
        }

        public void setSolveSpeed(int s)
        {
            solveSpeed_exp = (int)(s / 5);
            solveSpeed = (int)Math.Pow(2D, (double)solveSpeed_exp);
        }

        public string getGenAlgorithmName()
        {
            return genAlgorithmName;
        }

        public void setGenAlgorithmName(string a)
        {
            genAlgorithmName = a;
        }

        public string getSolveAlgorithmName()
        {
            return solveAlgorithmName;
        }

        public void setSolveAlgorithmName(string a)
        {
            solveAlgorithmName = a;
        }

        public int getGenAlgorithmType()
        {
            return genAlgorithmType;
        }

        public void setGenAlgorithmType(int a)
        {
            genAlgorithmType = a;

            GenerateGrid();
        }

        public int getSolveAlgorithmType()
        {
            return solveAlgorithmType;
        }

        public void setSolveAlgorithmType(int a)
        {
            solveAlgorithmType = a;
        }

        void calculateCoords()
        {
            if (height >= width)
            {
                spacer = side_length / (float)height;
                start_x = padding_x + (((float)(height - width) * spacer) / 2F);
                start_y = padding_y;
            }
            else
            {
                spacer = side_length / (float)width;
                start_x = padding_x;
                start_y = padding_y + (((float)(width - height) * spacer) / 2F);
            }

            wall_thickness = 2;

            cell_padding = 8;

            if (spacer <= 6)
                cell_padding = 0;
            else if (spacer <= 10)
                cell_padding = 1;
            else if (spacer <= 15)
                cell_padding = 2;
            else if (spacer <= 21)
                cell_padding = 3;
            else if (spacer <= 28)
                cell_padding = 4;
            else if (spacer <= 36)
                cell_padding = 5;
            else if (spacer <= 46)
                cell_padding = 6;
            else if (spacer <= 58)
                cell_padding = 7;

            startCorner = rng.Next(1, 5);
        }

        public void displayOutline()
        {
            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x, start_y), new SizeF((float)width * spacer, 2)));
            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x, start_y + 2), new SizeF(2, (float)height * spacer)));
            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + 2, start_y + ((float)height * spacer)), new SizeF((float)width * spacer, 2)));
            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)width * spacer), start_y), new SizeF(2, (float)height * spacer)));
        }

        public void displayGrid()
        {
            displayOutline();
            
            for (int r = 1; r < height; r++)
                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + 2, start_y + ((float)r * spacer)), new SizeF(((float)width * spacer) - 2, 2)));

            for (int c = 1; c < width; c++)
                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + 2), new SizeF(2, ((float)height * spacer) - 2)));
        }

        public void displayBlock()
        {
            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x, start_y), new SizeF((spacer * width) + 2, (spacer * height) + 2)));
        }

        void displayRemoveWalls()
        {
            int r, c;

            foreach (MazeCell coord in visitedCellsQueue)
            {
                r = coord.row;
                c = coord.column;

                if (grid[r, c].top == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));

                if (grid[r, c].right == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                if (r < (height - 1) && grid[r + 1, c].top == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));

                if (c > 0 && grid[r, c - 1].right == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }
        }

        void displayBuildWalls()
        {
            int r, c;

            int visited = 0;

            foreach (MazeCell coord in visitedCellsQueue)
            {
                if (!runningGen)
                    return;

                r = coord.row;
                c = coord.column;

                if (r > 0 && grid[r, c].top == 0 && grid[r, c].tvisited == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                    grid[r, c].tvisited = 1;
                }

                if (c < (width - 1) && grid[r, c].right == 0 && grid[r, c].rvisited == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                    grid[r, c].rvisited = 1;
                }

                if (r < (height - 1) && grid[r + 1, c].top == 0 && grid[r + 1, c].tvisited == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)(r + 1) * spacer)), new SizeF(spacer + 2, 2)));

                    grid[r + 1, c].tvisited = 1;
                }

                if (c > 0 && grid[r, c - 1].right == 0 && grid[r, c - 1].rvisited == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                    grid[r, c - 1].rvisited = 1;
                }

                visited++;

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }
        }

        void displayBuildWalls_Prim()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = rng.Next(1, 3);

            bool sleep = false;

            if (d == 1)
            {
                if (r > 0 && grid[r, c].top == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                    sleep = true;
                }

                grid[r, c].tvisited = 1;
            }
            else if (d == 2)
            {
                if (c < (width - 1) && grid[r, c].right == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                    sleep = true;
                }

                grid[r, c].rvisited = 1;
            }

            visited = 0;

            int genVisited = 0;

            if (sleep)
            {
                sleep = false;

                genVisited++;

                if (showGen && (genVisited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if ((grid[r, c].tvisited == 0 || grid[r, c].rvisited == 0) && hasPartiallyVisitedNeighbor(r, c))
                {
                    d = rng.Next(1, 3);

                    if (d == 1)
                    {
                        if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                                sleep = true;
                            }

                            grid[r, c].tvisited = 1;
                        }
                        else if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                                sleep = true;
                            }

                            grid[r, c].rvisited = 1;
                        }
                    }
                    else if (d == 2)
                    {
                        if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                                sleep = true;
                            }

                            grid[r, c].rvisited = 1;
                        }
                        else if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                                sleep = true;
                            }

                            grid[r, c].tvisited = 1;
                        }
                    }

                    if (grid[r, c].tvisited == 1 && grid[r, c].rvisited == 1)
                        visited++;

                    if (sleep)
                    {
                        sleep = false;

                        genVisited++;

                        if (showGen && (genVisited % genSpeed) == 0)
                            Thread.Sleep(sleepTime);
                    }
                }
            }
        }

        void displayBuildWalls_Kruskal()
        {
            int r, c, d;

            visited = 0;

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0)
                {
                    d = rng.Next(1, 3);

                    if (d == 1)
                    {
                        if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                            grid[r, c].tvisited = 1;
                        }
                        else if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                            grid[r, c].rvisited = 1;
                        }
                    }
                    else if (d == 2)
                    {
                        if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                            grid[r, c].rvisited = 1;
                        }
                        else if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                            grid[r, c].tvisited = 1;
                        }
                    }

                    if (grid[r, c].tvisited == 1 && grid[r, c].rvisited == 1)
                    {
                        grid[r, c].visited = 1;

                        visited++;

                        if (showGen && (visited % genSpeed) == 0)
                            Thread.Sleep(sleepTime);
                    }
                }
            }
        }

        void displayStart()
        {
            mazeCanvas.FillRectangle(new SolidBrush(colorStart), new RectangleF(startGen, new SizeF(spacer - (cell_padding * 2) - wall_thickness, spacer - (cell_padding * 2) - wall_thickness)));
            mazeCanvas.FillRectangle(new SolidBrush(colorStop), new RectangleF(stopGen, new SizeF(spacer - (cell_padding * 2) - wall_thickness, spacer - (cell_padding * 2) - wall_thickness)));
        }

        void findStart()
        {
            if (startCorner == 1)
            {
                startGen = new PointF(start_x + cell_padding + 2, start_y + cell_padding + 2);
                stopGen = new PointF(start_x + ((float)(width - 1) * spacer) + cell_padding + 2, start_y + ((float)(height - 1) * spacer) + cell_padding + 2);

                startSolve = new Point(0, 0);
                stopSolve = new Point(width - 1, height - 1);
            }
            else if (startCorner == 2)
            {
                startGen = new PointF(start_x + cell_padding + 2, start_y + ((float)(height - 1) * spacer) + cell_padding + 2);
                stopGen = new PointF(start_x + ((float)(width - 1) * spacer) + cell_padding + 2, start_y + cell_padding + 2);

                startSolve = new Point(0, height - 1);
                stopSolve = new Point(width - 1, 0);
            }
            else if (startCorner == 3)
            {
                startGen = new PointF(start_x + ((float)(width - 1) * spacer) + cell_padding + 2, start_y + cell_padding + 2);
                stopGen = new PointF(start_x + cell_padding + 2, start_y + ((float)(height - 1) * spacer) + cell_padding + 2);

                startSolve = new Point(width - 1, 0);
                stopSolve = new Point(0, height - 1);
            }
            else
            {
                startGen = new PointF(start_x + ((float)(width - 1) * spacer) + cell_padding + 2, start_y + ((float)(height - 1) * spacer) + cell_padding + 2);
                stopGen = new PointF(start_x + cell_padding + 2, start_y + cell_padding + 2);

                startSolve = new Point(width - 1, height - 1);
                stopSolve = new Point(0, 0);
            }
        }

        bool hasUnvisitedNeighbor(int r, int c)
        {
            if ((r > 0 && grid[r - 1, c].visited == 0) ||
                (c < (width - 1) && grid[r, c + 1].visited == 0) ||
                (r < (height - 1) && grid[r + 1, c].visited == 0) ||
                (c > 0 && grid[r, c - 1].visited == 0))
                return true;

            return false;
        }

        bool hasVisitedNeighbor(int r, int c)
        {
            if ((r > 0 && grid[r - 1, c].visited == 1) ||
                (c < (width - 1) && grid[r, c + 1].visited == 1) ||
                (r < (height - 1) && grid[r + 1, c].visited == 1) ||
                (c > 0 && grid[r, c - 1].visited == 1))
                return true;

            return false;
        }

        bool hasPartiallyVisitedNeighbor(int r, int c)
        {
            if ((r > 0 && (grid[r - 1, c].tvisited == 1 || grid[r - 1, c].rvisited == 1)) ||
                (c < (width - 1) && (grid[r, c + 1].tvisited == 1 || grid[r, c + 1].rvisited == 1)) ||
                (r < (height - 1) && (grid[r + 1, c].tvisited == 1 || grid[r + 1, c].rvisited == 1)) ||
                (c > 0 && (grid[r, c - 1].tvisited == 1 || grid[r, c - 1].rvisited == 1)))
                return true;

            return false;
        }

        int findCellDirection(int r, int c)
        {
            while (true)
            {
                int d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    return 1;
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    return 2;
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    return 3;
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    return 4;
            }
        }

        void pickCell_RemoveWalls(int r, int c)
        {
            while (true)
            {
                int d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 1)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));
                    
                    return;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 1)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                    return;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 1)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));

                    return;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 1)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                    return;
                }
            }
        }

        void pickCell_BuildWalls(int r, int c)
        {
            while (true)
            {
                int d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 1)
                {
                    grid[r, c].top = 1;

                    return;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 1)
                {
                    grid[r, c].right = 1;

                    return;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 1)
                {
                    grid[r + 1, c].top = 1;

                    return;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 1)
                {
                    grid[r, c - 1].right = 1;

                    return;
                }
            }
        }

        void pickCell_FillCells(int r, int c)
        {
            while (true)
            {
                int d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 1)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, spacer)));

                    return;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 1)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    return;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 1)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer)));
                    
                    return;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 1)
                {
                    grid[r, c - 1].right = 1; ;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    return;
                }
            }
        }

        void generateRBStraight_RemoveWalls()
        {
            //visitRBStraight_RemoveWalls(rng.Next(0, height), rng.Next(0, width), 0);

            visitRBStraight_RemoveWalls_DFS_Iteration_Stack();

            //visitRBStraight_RemoveWalls_BFS_Iteration_Queue();

            //visitBFS_RemoveWalls();

            //displayRemoveWalls();
        }

        void generateRBStraight_BuildWalls()
        {
            //visitRBStraight_BuildWalls(rng.Next(0, height), rng.Next(0, width), 0);

            visitRBStraight_BuildWalls_DFS_Iteration_Stack();

            displayBuildWalls();
        }

        void generateRBStraight_FillCells()
        {
            //visitRBStraight_FillCells(rng.Next(0, height), rng.Next(0, width), 0);

            visitRBStraight_FillCells_DFS_Iteration_Stack();
        }

        void generateRBJagged_RemoveWalls()
        {
            //visitRBJagged_RemoveWalls(rng.Next(0, height), rng.Next(0, width), 0);

            visitRBJagged_RemoveWalls_DFS_Iteration_Stack();
        }

        void generateRBJagged_BuildWalls()
        {
            //visitRBJagged_BuildWalls(rng.Next(0, height), rng.Next(0, width), 0);

            visitRBJagged_BuildWalls_DFS_Iteration_Stack();

            displayBuildWalls();
        }

        void generateRBJagged_FillCells()
        {
            //visitRBJagged_FillCells(rng.Next(0, height), rng.Next(0, width), 0);

            visitRBJagged_FillCells_DFS_Iteration_Stack();
        }

        void generateHKStraight_RemoveWalls_DFS_Recursion()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKStraight_RemoveWalls(r, c, 0);

            if (visited < height * width)
            {
                do
                {
                    do
                    {
                        r = rng.Next(0, height);
                        c = rng.Next(0, width);

                    } while (grid[r,c].visited == 1 || !hasVisitedNeighbor(r, c));

                    pickCell_RemoveWalls(r, c);

                    if (hunt == 1 && showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);

                    kill = 0;

                    visitHKStraight_RemoveWalls(r, c, 0);

                } while (visited < height * width);
            }
        }

        void generateHKStraight_RemoveWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKStraight_RemoveWalls_DFS_Iteration_Stack(r, c);

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_RemoveWalls(r, c);

                    visitHKStraight_RemoveWalls_DFS_Iteration_Stack(r, c);
                }
            }
        }

        void generateHKStraight_BuildWalls_DFS_Recursion()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKStraight_BuildWalls(r, c, 0);

            if (visited < height * width)
            {
                do
                {
                    do
                    {
                        r = rng.Next(0, height);
                        c = rng.Next(0, width);

                    } while (grid[r, c].visited == 1 || !hasVisitedNeighbor(r, c));

                    pickCell_BuildWalls(r, c);

                    kill = 0;

                    visitHKStraight_BuildWalls(r, c, 0);

                } while (visited < height * width);
            }

            displayBuildWalls();
        }

        void generateHKStraight_BuildWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKStraight_BuildWalls_DFS_Iteration_Stack(r, c);

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_BuildWalls(r, c);

                    visitHKStraight_BuildWalls_DFS_Iteration_Stack(r, c);
                }
            }

            displayBuildWalls();
        }

        void generateHKStraight_FillCells_DFS_Recursion()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKStraight_FillCells(r, c, 0);

            if (visited < height * width)
            {
                do
                {
                    do
                    {
                        r = rng.Next(0, height);
                        c = rng.Next(0, width);

                    } while (grid[r, c].visited == 1 || !hasVisitedNeighbor(r, c));

                    pickCell_FillCells(r, c);

                    if (hunt == 1 && showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);

                    kill = 0;

                    visitHKStraight_FillCells(r, c, 0);

                } while (visited < height * width);
            }
        }

        void generateHKStraight_FillCells_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

            visited = 0;

            visitHKStraight_FillCells_DFS_Iteration_Stack(r, c);

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_FillCells(r, c);

                    visitHKStraight_FillCells_DFS_Iteration_Stack(r, c);
                }
            }
        }

        void generateHKJagged_RemoveWalls_DFS_Recursion()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKJagged_RemoveWalls(r, c, 0);

            if (visited < height * width)
            {
                do
                {
                    do
                    {
                        r = rng.Next(0, height);
                        c = rng.Next(0, width);

                    } while (grid[r,c].visited == 1 || !hasVisitedNeighbor(r, c));

                    pickCell_RemoveWalls(r, c);

                    if (hunt == 1 && showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);

                    kill = 0;

                    visitHKJagged_RemoveWalls(r, c, 0);

                } while (visited < height * width);
            }
        }

        void generateHKJagged_RemoveWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKJagged_RemoveWalls_DFS_Iteration_Stack(r, c);

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_RemoveWalls(r, c);

                    visitHKJagged_RemoveWalls_DFS_Iteration_Stack(r, c);
                }
            }
        }

        void generateHKJagged_BuildWalls_DFS_Recursion()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKJagged_BuildWalls(r, c, 0);

            if (visited < height * width)
            {
                do
                {
                    do
                    {
                        r = rng.Next(0, height);
                        c = rng.Next(0, width);

                    } while (grid[r, c].visited == 1 || !hasVisitedNeighbor(r, c));

                    pickCell_BuildWalls(r, c);

                    kill = 0;

                    visitHKJagged_BuildWalls(r, c, 0);

                } while (visited < height * width);
            }

            displayBuildWalls();
        }

        void generateHKJagged_BuildWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKJagged_BuildWalls_DFS_Iteration_Stack(r, c);

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_BuildWalls(r, c);

                    visitHKJagged_BuildWalls_DFS_Iteration_Stack(r, c);
                }
            }

            displayBuildWalls();
        }

        void generateHKJagged_FillCells_DFS_Recursion()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            visited = 0;

            visitHKJagged_FillCells(r, c, 0);

            if (visited < height * width)
            {
                do
                {
                    do
                    {
                        r = rng.Next(0, height);
                        c = rng.Next(0, width);

                    } while (grid[r, c].visited == 1 || !hasVisitedNeighbor(r, c));

                    pickCell_FillCells(r, c);

                    if (hunt == 1 && showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);

                    kill = 0;

                    visitHKJagged_FillCells(r, c, 0);

                } while (visited < height * width);
            }
        }

        void generateHKJagged_FillCells_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

            visited = 0;

            visitHKJagged_FillCells_DFS_Iteration_Stack(r, c);

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_FillCells(r, c);

                    visitHKJagged_FillCells_DFS_Iteration_Stack(r, c);
                }
            }
        }

        void generatePrim_RemoveWalls()
        {
            visitPrim_RemoveWalls();
        }

        void generatePrim_BuildWalls()
        {
            visitPrim_BuildWalls();

            displayBuildWalls_Prim();
        }

        void generatePrim_FillCells()
        {
            visitPrim_FillCells();
        }

        void generateKruskal_RemoveWalls()
        {
            //visitKruskal_RemoveWalls();

            visitKruskal_RemoveWalls_DFS_Iteration_Stack();
        }

        void generateKruskal_BuildWalls()
        {
            visitKruskal_BuildWalls_DFS_Iteration_Stack();

            displayBuildWalls_Kruskal();
        }

        void generateKruskal_FillCells()
        {
            visitKruskal_FillCells_DFS_Iteration_Stack();
        }

        void generateRooms(float roomPercent)
        {
            int r, c, d;

            int totalBuiltWalls = (height - 1) * (width - 1);
            int maxRemovableWalls = totalBuiltWalls;
            int removableWalls = (int)((float)maxRemovableWalls * roomPercent / 100F);

            int removedWallsCount = 0;

            int genVisited = 0;

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].visited = grid[r, c].tvisited = grid[r, c].rvisited = 0;

            visited = 0;

            while (removedWallsCount < removableWalls)
            {
                if (!runningGen) return;

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0)
                {
                    d = rng.Next(1, 3);

                    if (d == 1)
                    {
                        if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                            {
                                if (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                    grid[r, c].top = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                    grid[r, c].top = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }
                        }
                        else if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                            {
                                if (r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                            {
                                if (r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2 ), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }
                        }
                        else if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                            {
                                if (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                    grid[r, c].top = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                    grid[r, c].top = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }
                        }
                    }

                    if (grid[r, c].top == 1)
                        grid[r, c].tvisited = 1;

                    if (grid[r, c].right == 1)
                        grid[r, c].rvisited = 1;

                    if (grid[r, c].tvisited == 1 && grid[r, c].rvisited == 1)
                    {
                        grid[r, c].visited = 1;

                        visited++;
                    }
                }
            }

            //mazeCanvas.DrawString(totalBuiltWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(50, 25));
            //mazeCanvas.DrawString(maxRemovableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(125, 25));
            //mazeCanvas.DrawString(removableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(200, 25));
            //mazeCanvas.DrawString(removedWallsCount.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(275, 25));
            //mazeCanvas.DrawString((height * width).ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(350, 25));
            //mazeCanvas.DrawString(visited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(425, 25));
        }

        void generateRooms2(float roomPercent)
        {
            int r, c, d;

            int totalBuiltWalls = (height - 1) * (width - 1);
            int maxRemovableWalls = totalBuiltWalls;
            int removableWalls = (int)((float)maxRemovableWalls * roomPercent / 100F);

            int removedWallsCount = 0;

            int genVisited = 0;

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].visited = grid[r, c].tvisited = grid[r, c].rvisited = 0;

            visited = 0;

            while (removedWallsCount < removableWalls && visited < (height * width))
            {
                if (!runningGen) return;

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0)
                {
                    d = rng.Next(1, 3);

                    if (d == 1)
                    {
                        if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 0)
                            {
                                if (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                    grid[r, c].top = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                    grid[r, c].top = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }

                            grid[r, c].tvisited = 1;
                        }
                        else
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                            {
                                if (r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }

                            grid[r, c].rvisited = 1;
                        }
                    }
                    else
                    {
                        if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 0)
                            {
                                if (r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                                else if (r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1)
                                {
                                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer)));

                                    grid[r, c].right = 1;

                                    removedWallsCount++;

                                    genVisited++;

                                    if (showGen && (genVisited % genSpeed) == 0)
                                        Thread.Sleep(sleepTime);
                                }
                            }

                            grid[r, c].rvisited = 1;
                        }
                        else
                        {
                            if (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1)
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                grid[r, c].top = 1;

                                removedWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }
                            else if (c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1)
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer, 2)));

                                grid[r, c].top = 1;

                                removedWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }

                            grid[r, c].tvisited = 1;
                        }
                    }

                    if (grid[r, c].tvisited == 1 && grid[r, c].rvisited == 1)
                    {
                        grid[r, c].visited = 1;
                        visited++;
                    }
                }
            }

            //mazeCanvas.DrawString(totalBuiltWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(50, 25));
            //mazeCanvas.DrawString(maxRemovableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(125, 25));
            //mazeCanvas.DrawString(removableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(200, 25));
            //mazeCanvas.DrawString(removedWallsCount.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(275, 25));
            //mazeCanvas.DrawString((height * width).ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(350, 25));
            //mazeCanvas.DrawString(visited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(425, 25));
        }

        void generateLoops(float loopPercent)
        {
            int r, c, d;

            int totalBuiltWalls = (height - 1) * (width - 1);
            int maxRemovableWalls = totalBuiltWalls / 2;
            int removableWalls = (int)((float)maxRemovableWalls * loopPercent / 100F);

            int removedWallsCount = 0;

            int genVisited = 0;

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].visited = grid[r, c].tvisited = grid[r, c].rvisited = 0;

            visited = 0;

            while (removedWallsCount < removableWalls && visited < (height * width))
            {
                if (!runningGen) return;

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0)
                {
                    d = rng.Next(1, 3);

                    if (d == 1)
                    {
                        if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 &&
                                grid[r, c].top == 0 &&
                                !(c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1) &&
                                !(c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));

                                grid[r, c].top = 1;

                                removedWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }

                            grid[r, c].tvisited = 1;
                        }
                        else
                        {
                            if (c < (width - 1) &&
                                grid[r, c].right == 0 &&
                                !(r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1) &&
                                !(r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                                grid[r, c].right = 1;

                                removedWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }

                            grid[r, c].rvisited = 1;
                        }
                    }
                    else
                    {
                        if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) &&
                                grid[r, c].right == 0 &&
                                !(r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1) &&
                                !(r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                                grid[r, c].right = 1;

                                removedWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }

                            grid[r, c].rvisited = 1;
                        }
                        else
                        {
                            if (r > 0 &&
                                grid[r, c].top == 0 &&
                                !(c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1) &&
                                !(c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));

                                grid[r, c].top = 1;

                                removedWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }

                            grid[r, c].tvisited = 1;
                        }
                    }

                    if (grid[r, c].tvisited == 1 && grid[r, c].rvisited == 1)
                    {
                        grid[r, c].visited = 1;

                        visited++;
                    }
                }
            }

            //mazeCanvas.DrawString(totalBuiltWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(50, 5));
            //mazeCanvas.DrawString(maxRemovableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(125, 5));
            //mazeCanvas.DrawString(removableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(200, 5));
            //mazeCanvas.DrawString(removedWallsCount.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(275, 5));
            //mazeCanvas.DrawString((height * width).ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(350, 5));
            //mazeCanvas.DrawString(visited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(425, 5));
        }

        void generateObstacles(float obstaclePercent)
        {
            int r, c;

            int totalCells = height * width;
            int possibleObstacles = (int)((float)totalCells * obstaclePercent / 100F);

            int obstacleCount = 0;

            int genVisited = 0;

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].obstacle = 0;

            while (obstacleCount < possibleObstacles)
            {
                if (!runningGen) return;

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].obstacle == 0 &&
                    !(r == startSolve.Y && c == startSolve.X) &&
                    !(r == stopSolve.Y && c == stopSolve.X))
                {
                    if (r > 0 && grid[r, c].top == 1)
                        grid[r, c].top = 0;
                    
                    if (c < (width - 1) && grid[r, c].right == 1)
                        grid[r, c].right = 0;

                    if (r < (height - 1) && grid[r + 1, c].top == 1)
                        grid[r + 1, c].top = 0;

                    if (c > 0 && grid[r, c - 1].right == 1)
                        grid[r, c - 1].right = 0;

                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

                    grid[r, c].obstacle = 1;

                    obstacleCount++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
            }
        }

        void generateIslands(float islandPercent)
        {
            int r, c, d;

            solve_BFS_FindShortestPath();

            int totalRemovedWalls = ((height - 1) * width) + width - 1;
            int maxBuildableWalls = totalRemovedWalls - solvePathLength + 1;
            int buildableWalls = (int)((float)maxBuildableWalls * islandPercent / 100F);

            int builtWallsCount = 0;

            int genVisited = 0;

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].visited = grid[r, c].tvisited = grid[r, c].rvisited = 0;

            visited = 0;

            while (builtWallsCount < buildableWalls && visited < (height * width))
            {
                if (!runningGen) return;

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0)
                {
                    d = rng.Next(1, 3);

                    if (d == 1)
                    {
                        if (grid[r, c].tvisited == 0)
                        {
                            if (r > 0 && grid[r, c].top == 1 &&
                                (grid[r, c].solvepath == 0 || (grid[r, c].solvepath == 1 && grid[r - 1, c].solvepath == 0)) &&
                                (!(c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1) ||
                                 !(c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1)))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                                grid[r, c].top = 0;

                                builtWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }
                        }
                        else
                        {
                            if (c < (width - 1) && grid[r, c].right == 1 &&
                                (grid[r, c].solvepath == 0 || (grid[r, c].solvepath == 1 && grid[r, c + 1].solvepath == 0)) &&
                                (!(r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1) ||
                                 !(r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1)))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                                grid[r, c].right = 0;

                                builtWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }
                        }
                    }
                    else
                    {
                        if (grid[r, c].rvisited == 0)
                        {
                            if (c < (width - 1) && grid[r, c].right == 1 &&
                                (grid[r, c].solvepath == 0 || (grid[r, c].solvepath == 1 && grid[r, c + 1].solvepath == 0)) &&
                                (!(r > 0 && grid[r - 1, c].right == 1 && grid[r, c].top == 1 && grid[r, c + 1].top == 1) ||
                                 !(r < (height - 1) && grid[r + 1, c].right == 1 && grid[r + 1, c].top == 1 && grid[r + 1, c + 1].top == 1)))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                                grid[r, c].right = 0;

                                builtWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }
                        }
                        else
                        {
                            if (r > 0 && grid[r, c].top == 1 &&
                                (grid[r, c].solvepath == 0 || (grid[r, c].solvepath == 1 && grid[r - 1, c].solvepath == 0)) &&
                                (!(c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1 && grid[r - 1, c - 1].right == 1) ||
                                 !(c < (width - 1) && grid[r, c + 1].top == 1 && grid[r, c].right == 1 && grid[r - 1, c].right == 1)))
                            {
                                mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                                grid[r, c].top = 0;

                                builtWallsCount++;

                                genVisited++;

                                if (showGen && (genVisited % genSpeed) == 0)
                                    Thread.Sleep(sleepTime);
                            }
                        }
                    }

                    if (grid[r, c].top == 0)
                        grid[r, c].tvisited = 1;

                    if (grid[r, c].right == 0)
                        grid[r, c].rvisited = 1;

                    if (grid[r, c].tvisited == 1 && grid[r, c].rvisited == 1)
                    {
                        grid[r, c].visited = 1;

                        visited++;
                    }
                }
            }

            fillIslandCells();

            //mazeCanvas.DrawString(totalRemovedWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(500, 25));
            //mazeCanvas.DrawString(maxBuildableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(575, 25));
            //mazeCanvas.DrawString(buildableWalls.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(650, 25));
            //mazeCanvas.DrawString(builtWallsCount.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(725, 25));
            //mazeCanvas.DrawString((height * width).ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(800, 25));
            //mazeCanvas.DrawString(visited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(875, 25));
        }

        void fillIslandCells()
        {
            int r, c;

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].visited = 0;

            visited = 0;

            while (visited < (height * width))
            {
                if (!runningGen) return;

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                visitedCellsStack.Clear();
                visitedCellsQueue.Clear();

                visitedCellsStack.Push(new MazeCell(c, r));

                bool fill = true;

                bool pop;

                do
                {
                    pop = true;

                    r = visitedCellsStack.Peek().row;
                    c = visitedCellsStack.Peek().column;

                    if (grid[r, c].visited == 0)
                    {
                        visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

                        grid[r, c].visited = 1;

                        visited++;

                        if (grid[r, c].solvepath == 1)
                            fill = false;

                        if (r > 0 && grid[r, c].top == 1)
                        {
                            visitedCellsStack.Push(new MazeCell(c, r - 1));

                            pop = false;
                        }

                        if (c < (width - 1) && grid[r, c].right == 1)
                        {
                            visitedCellsStack.Push(new MazeCell(c + 1, r));

                            pop = false;
                        }

                        if (r < (height - 1) && grid[r + 1, c].top == 1)
                        {
                            visitedCellsStack.Push(new MazeCell(c, r + 1));

                            pop = false;
                        }

                        if (c > 0 && grid[r, c - 1].right == 1)
                        {
                            visitedCellsStack.Push(new MazeCell(c - 1, r));

                            pop = false;
                        }
                    }

                    if (pop)
                        visitedCellsStack.Pop();

                    if (!runningGen) return;

                } while (visitedCellsStack.Count > 0);

                if (fill)
                {
                    foreach (MazeCell cell in visitedCellsQueue)
                    {
                        if (!runningGen) return;
                        mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)cell.column * spacer), start_y + ((float)cell.row * spacer)), new SizeF(spacer + 2, spacer + 2)));
                    }
                }
            }

            for (r = 0; r < height; r++)
                for (c = 0; c < width; c++)
                    grid[r, c].solution = grid[r, c].solvepath = 0;
        }

        void visitRBStraight_RemoveWalls(int r, int c, int df)
        {
            if (visited == height * width)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r,c].visited != 0)
                return;

            grid[r,c].visited = 1;

            //visitedCellsQueue.Enqueue(new MazeCell(c, r));

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            int d = rng.Next(1, 5);

            if (d == 1)
            {
                visitRBStraight_RemoveWalls(r - 1, c, 1);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_RemoveWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitRBStraight_RemoveWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitRBStraight_RemoveWalls(r, c - 1, 2);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_RemoveWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_RemoveWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitRBStraight_RemoveWalls(r, c + 1, 3);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r + 1, c, 4);
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_RemoveWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitRBStraight_RemoveWalls(r + 1, c, 4);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r, c + 1, 3);
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_RemoveWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBStraight_RemoveWalls(r, c - 1, 2);
                        visitRBStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitRBStraight_RemoveWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            visited = 0;

            int genVisited = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    d = rng.Next(1, 5);

                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r--;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c++;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r++;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c--;

                        sleep = true;
                    }
                }
                else
                {
                    visitedCellsStack.Pop();

                    d = d != 0 ? d > 2 ? d - 2 : d + 2 : 0;

                    if (d == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }
                    else if (d == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    if (visitedCellsStack.Count > 0)
                    {
                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;

                        d = visitedCellsStack.Peek().direction;

                        mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        genVisited++;

                        if (showGen && (genVisited % genSpeed) == 0)
                            Thread.Sleep(sleepTime);
                    }
                }

            } while (visitedCellsStack.Count > 0 && runningGen);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitRBStraight_RemoveWalls_BFS_Iteration_Queue()
        {
            //sleepTime = 50;
            //genSpeed = 1;
            //showGen = 1;
            //colorGenerate = colorBackground;
            //colorCurrentCell = colorBackground;

            Queue<MazeCell> visitedCellsQueueCopy = new Queue<MazeCell>();

            bool showBacktracksCopy = showBacktracks;

            setShowBacktracks(true);

            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;

            int path = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            visited = 0;

            int genVisited = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    //mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(cell_padding, cell_padding)));

                    visitedCellsQueue.Enqueue(new MazeCell(c, r, d));

                    path = 1;

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    d = rng.Next(1, 5);

                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r--;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c++;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r++;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c--;

                        sleep = true;
                    }
                }
                else if (path == 1)
                {
                    path = 3;

                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    r = visitedCellsQueue.Peek().row;
                    c = visitedCellsQueue.Peek().column;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
                else if (path == 2)
                {
                    path = 3;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    r = visitedCellsQueue.Peek().row;
                    c = visitedCellsQueue.Peek().column;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
                else if (path == 3)
                {
                    path = 2;

                    d = visitedCellsQueue.Peek().direction;

                    //d = d != 0 ? d > 2 ? d - 2 : d + 2 : 0;

                    if (d == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }
                    else if (d == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsQueueCopy.Enqueue(visitedCellsQueue.Dequeue());
                }

            } while (visitedCellsQueue.Count > 0);

            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            setShowBacktracks(showBacktracksCopy);

            //flood_fill_BFS(r, c);

            while (visitedCellsQueueCopy.Count > 0)
            {
                r = visitedCellsQueueCopy.Peek().row;
                c = visitedCellsQueueCopy.Peek().column;

                d = visitedCellsQueueCopy.Peek().direction;

                d = d != 0 ? d > 2 ? d - 2 : d + 2 : 0;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }
                else if (d == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                }

                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                visitedCellsQueueCopy.Dequeue();

                genVisited++;

                if (showGen && (genVisited % genSpeed) == 0)
                    Thread.Sleep(sleepTime / 10);

                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
            }
        }

        void visitRBStraight_BuildWalls(int r, int c, int df)
        {
            if (visited == height * width)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visitedCellsQueue.Enqueue(new MazeCell(c, r));

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }
            }

            int d = rng.Next(1, 5);

            if (d == 1)
            {
                visitRBStraight_BuildWalls(r - 1, c, 1);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_BuildWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitRBStraight_BuildWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitRBStraight_BuildWalls(r, c - 1, 2);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_BuildWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_BuildWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitRBStraight_BuildWalls(r, c + 1, 3);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r + 1, c, 4);
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_BuildWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitRBStraight_BuildWalls(r + 1, c, 4);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r, c + 1, 3);
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_BuildWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBStraight_BuildWalls(r, c - 1, 2);
                        visitRBStraight_BuildWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitRBStraight_BuildWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    visitedCellsStack.Push(new MazeCell(c, r, d));
                    visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

                    grid[r, c].visited = 1;
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    d = rng.Next(1, 5);

                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        r--;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        c++;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        r++;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        c--;

                        sleep = true;
                    }
                }
                else
                {
                    visitedCellsStack.Pop();

                    if (visitedCellsStack.Count > 0)
                    {
                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;
                    }
                }

            } while (visitedCellsStack.Count > 0 && runningGen);
        }

        void visitRBStraight_FillCells(int r, int c, int df)
        {
            if (visited == height * width)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)(r - 1) * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            int d = rng.Next(1, 5);

            if (d == 1)
            {
                visitRBStraight_FillCells(r - 1, c, 1);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r, c + 1, 3);
                        visitRBStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r + 1, c, 4);
                        visitRBStraight_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_FillCells(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r, c - 1, 2);
                        visitRBStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r + 1, c, 4);
                        visitRBStraight_FillCells(r, c - 1, 2);
                    }
                }
                else
                {
                    visitRBStraight_FillCells(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r, c - 1, 2);
                        visitRBStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r, c + 1, 3);
                        visitRBStraight_FillCells(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitRBStraight_FillCells(r, c - 1, 2);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r, c + 1, 3);
                        visitRBStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r + 1, c, 4);
                        visitRBStraight_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_FillCells(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r - 1, c, 1);
                        visitRBStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r + 1, c, 4);
                        visitRBStraight_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_FillCells(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r - 1, c, 1);
                        visitRBStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r, c + 1, 3);
                        visitRBStraight_FillCells(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitRBStraight_FillCells(r, c + 1, 3);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r, c - 1, 2);
                        visitRBStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r + 1, c, 4);
                        visitRBStraight_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r - 1, c, 1);
                        visitRBStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r + 1, c, 4);
                        visitRBStraight_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_FillCells(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r - 1, c, 1);
                        visitRBStraight_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r, c - 1, 2);
                        visitRBStraight_FillCells(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitRBStraight_FillCells(r + 1, c, 4);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitRBStraight_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r, c - 1, 2);
                        visitRBStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r, c + 1, 3);
                        visitRBStraight_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBStraight_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r - 1, c, 1);
                        visitRBStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r, c + 1, 3);
                        visitRBStraight_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBStraight_FillCells(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitRBStraight_FillCells(r - 1, c, 1);
                        visitRBStraight_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBStraight_FillCells(r, c - 1, 2);
                        visitRBStraight_FillCells(r - 1, c, 1);
                    }
                }
            }
        }

        void visitRBStraight_FillCells_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            visited = 0;

            int genVisited = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    d = rng.Next(1, 5);

                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r = r - 1;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c = c + 1;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r = r + 1;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c = c - 1;

                        sleep = true;
                    }
                }
                else
                {
                    visitedCellsStack.Pop();

                    d = d != 0 ? d > 2 ? d - 2 : d + 2 : 0;

                    if (d == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }
                    else if (d == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    if (visitedCellsStack.Count > 0)
                    {
                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;

                        d = visitedCellsStack.Peek().direction;

                        mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        genVisited++;

                        if (showGen && (genVisited % genSpeed) == 0)
                            Thread.Sleep(sleepTime);
                    }
                }

            } while (visitedCellsStack.Count > 0 && runningGen);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
        }

        void visitRBJagged_RemoveWalls(int r, int c, int df)
        {
            if (visited == height * width)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r,c].visited != 0)
                return;

            grid[r,c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            int d = rng.Next(1, 5);

            while (d == df)
                d = rng.Next(1, 5);

            if (d == 1)
            {
                visitRBJagged_RemoveWalls(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == df);
                if (d == 2)
                {
                    visitRBJagged_RemoveWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitRBJagged_RemoveWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitRBJagged_RemoveWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitRBJagged_RemoveWalls(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_RemoveWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitRBJagged_RemoveWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_RemoveWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitRBJagged_RemoveWalls(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_RemoveWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBJagged_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r + 1, c, 4);
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_RemoveWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitRBJagged_RemoveWalls(r + 1, c, 4);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_RemoveWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBJagged_RemoveWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r, c + 1, 3);
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_RemoveWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBJagged_RemoveWalls(r, c - 1, 2);
                        visitRBJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitRBJagged_RemoveWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;
            int d_prev = 0;
            int d_tries = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            visited = 0;

            int genVisited = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    d_prev = d;

                    if (d == 0)
                        d = rng.Next(1, 5);
                    else
                        d = (rng.Next(0, 2) * 2) + (d_prev % 2) + 1;

                    d_tries = 0;

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r = r - 1;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c = c + 1;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r = r + 1;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c = c - 1;

                        sleep = true;
                    }
                    else
                    {
                        d = ++d_tries == 1 ? d > 2 ? d - 2 : d + 2 : d_prev;
                    }
                }
                else
                {
                    visitedCellsStack.Pop();

                    d_prev = d_prev != 0 ? d_prev > 2 ? d_prev - 2 : d_prev + 2 : 0;

                    if (d_prev == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d_prev == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }
                    else if (d_prev == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d_prev == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    if (visitedCellsStack.Count > 0)
                    {
                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;

                        d = d_prev;
                        d_prev = visitedCellsStack.Peek().direction;
                        d_tries = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        genVisited++;

                        if (showGen && (genVisited % genSpeed) == 0)
                            Thread.Sleep(sleepTime);
                    }
                }

            } while (visitedCellsStack.Count > 0);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitRBJagged_BuildWalls(int r, int c, int df)
        {
            if (visited == height * width)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visitedCellsQueue.Enqueue(new MazeCell(c, r));

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }
            }

            int d = rng.Next(1, 5);

            while (d == df)
                d = rng.Next(1, 5);

            if (d == 1)
            {
                visitRBJagged_BuildWalls(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == df);
                if (d == 2)
                {
                    visitRBJagged_BuildWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitRBJagged_BuildWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitRBJagged_BuildWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitRBJagged_BuildWalls(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_BuildWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitRBJagged_BuildWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_BuildWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitRBJagged_BuildWalls(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_BuildWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBJagged_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r + 1, c, 4);
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_BuildWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitRBJagged_BuildWalls(r + 1, c, 4);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_BuildWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBJagged_BuildWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r, c + 1, 3);
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_BuildWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBJagged_BuildWalls(r, c - 1, 2);
                        visitRBJagged_BuildWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitRBJagged_BuildWalls_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;
            int d_prev = 0;
            int d_tries = 0;

            visited = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    visitedCellsStack.Push(new MazeCell(c, r, d));
                    visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

                    d_prev = d;

                    if (d == 0)
                        d = rng.Next(1, 5);
                    else
                        d = (rng.Next(0, 2) * 2) + (d_prev % 2) + 1;

                    d_tries = 0;

                    grid[r, c].visited = 1;

                    visited++;
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        r = r - 1;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        c = c + 1;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        r = r + 1;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        c = c - 1;

                        sleep = true;
                    }
                    else
                    {
                        d = ++d_tries == 1 ? d > 2 ? d - 2 : d + 2 : d_prev;
                    }
                }
                else
                {
                    visitedCellsStack.Pop();

                    d_prev = d_prev != 0 ? d_prev > 2 ? d_prev - 2 : d_prev + 2 : 0;

                    if (visitedCellsStack.Count > 0)
                    {
                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;

                        d = d_prev;
                        d_prev = visitedCellsStack.Peek().direction;
                        d_tries = 1;
                    }
                }

            } while (visitedCellsStack.Count > 0);
        }

        void visitRBJagged_FillCells(int r, int c, int df)
        {
            if (visited == height * width)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)(r - 1) * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            int d = rng.Next(1, 5);

            while (d == df)
                d = rng.Next(1, 5);

            if (d == 1)
            {
                visitRBJagged_FillCells(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == df);
                if (d == 2)
                {
                    visitRBJagged_FillCells(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitRBJagged_FillCells(r, c + 1, 3);
                        visitRBJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r + 1, c, 4);
                        visitRBJagged_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitRBJagged_FillCells(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_FillCells(r, c - 1, 2);
                        visitRBJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r + 1, c, 4);
                        visitRBJagged_FillCells(r, c - 1, 2);
                    }
                }
                else
                {
                    visitRBJagged_FillCells(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_FillCells(r, c - 1, 2);
                        visitRBJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r, c + 1, 3);
                        visitRBJagged_FillCells(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitRBJagged_FillCells(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_FillCells(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitRBJagged_FillCells(r, c + 1, 3);
                        visitRBJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r + 1, c, 4);
                        visitRBJagged_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitRBJagged_FillCells(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_FillCells(r - 1, c, 1);
                        visitRBJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r + 1, c, 4);
                        visitRBJagged_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_FillCells(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_FillCells(r - 1, c, 1);
                        visitRBJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r, c + 1, 3);
                        visitRBJagged_FillCells(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitRBJagged_FillCells(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_FillCells(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_FillCells(r, c - 1, 2);
                        visitRBJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r + 1, c, 4);
                        visitRBJagged_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBJagged_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_FillCells(r - 1, c, 1);
                        visitRBJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r + 1, c, 4);
                        visitRBJagged_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_FillCells(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_FillCells(r - 1, c, 1);
                        visitRBJagged_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r, c - 1, 2);
                        visitRBJagged_FillCells(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitRBJagged_FillCells(r + 1, c, 4);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == df);
                if (d == 1)
                {
                    visitRBJagged_FillCells(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitRBJagged_FillCells(r, c - 1, 2);
                        visitRBJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r, c + 1, 3);
                        visitRBJagged_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitRBJagged_FillCells(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_FillCells(r - 1, c, 1);
                        visitRBJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r, c + 1, 3);
                        visitRBJagged_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitRBJagged_FillCells(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitRBJagged_FillCells(r - 1, c, 1);
                        visitRBJagged_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitRBJagged_FillCells(r, c - 1, 2);
                        visitRBJagged_FillCells(r - 1, c, 1);
                    }
                }
            }
        }

        void visitRBJagged_FillCells_DFS_Iteration_Stack()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int d = 0;
            int d_prev = 0;
            int d_tries = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            visited = 0;

            int genVisited = 0;

            bool sleep = true;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    d_prev = d;

                    if (d == 0)
                        d = rng.Next(1, 5);
                    else
                        d = (rng.Next(0, 2) * 2) + (d_prev % 2) + 1;

                    d_tries = 0;

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                if (hasUnvisitedNeighbor(r, c))
                {
                    if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                    {
                        grid[r, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r = r - 1;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                    {
                        grid[r, c].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c = c + 1;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                    {
                        grid[r + 1, c].top = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r = r + 1;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                    {
                        grid[r, c - 1].right = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                        mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c = c - 1;

                        sleep = true;
                    }
                    else
                    {
                        d = ++d_tries == 1 ? d > 2 ? d - 2 : d + 2 : d_prev;
                    }
                }
                else
                {
                    visitedCellsStack.Pop();

                    d_prev = d_prev != 0 ? d_prev > 2 ? d_prev - 2 : d_prev + 2 : 0;

                    if (d_prev == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d_prev == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }
                    else if (d_prev == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d_prev == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    if (visitedCellsStack.Count > 0)
                    {
                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;

                        d = d_prev;
                        d_prev = visitedCellsStack.Peek().direction;
                        d_tries = 1;

                        mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        genVisited++;

                        if (showGen && (genVisited % genSpeed) == 0)
                            Thread.Sleep(sleepTime);
                    }
                }

            } while (visitedCellsStack.Count > 0);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
        }

        void visitHKStraight_RemoveWalls(int r, int c, int df)
        {
            if (kill == 1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r,c].visited != 0)
                return;

            grid[r,c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            if (!hasUnvisitedNeighbor(r, c) || (visited % hunt) == 0)
            {
                kill = 1;

                return;
            }

            int d = rng.Next(1, 5);

            if (d == 1)
            {
                visitHKStraight_RemoveWalls(r - 1, c, 1);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_RemoveWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitHKStraight_RemoveWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitHKStraight_RemoveWalls(r, c - 1, 2);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_RemoveWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_RemoveWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitHKStraight_RemoveWalls(r, c + 1, 3);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r + 1, c, 4);
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_RemoveWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitHKStraight_RemoveWalls(r + 1, c, 4);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r, c + 1, 3);
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_RemoveWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKStraight_RemoveWalls(r, c - 1, 2);
                        visitHKStraight_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitHKStraight_RemoveWalls_DFS_Iteration_Stack(int r, int c)
        {
            int d = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].visited = 1;

            visited++;

            int genVisited = 1;

            if (showGen && (genVisited % genSpeed) == 0)
                Thread.Sleep(sleepTime);

            if (!hasUnvisitedNeighbor(r, c))
            {
                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                return;
            }

            visitedCellsStack.Push(new MazeCell(c, r, d));

            d = findCellDirection(r, c);

            bool sleep = false;

            do
            {
                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r - 1;

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c + 1;

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r + 1;

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c - 1;

                    sleep = true;
                }

                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

            } while (hasUnvisitedNeighbor(r, c));

            while (visitedCellsStack.Count > 0)
            {
                MazeCell cell = visitedCellsStack.Pop();

                d = d != 0 ? d > 2 ? d - 2 : d + 2 : 0;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                genVisited++;

                if (showGen && (genVisited % genSpeed) == 0)
                    Thread.Sleep(sleepTime / 10);
            }

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitHKStraight_BuildWalls(int r, int c, int df)
        {
            if (kill == 1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visitedCellsQueue.Enqueue(new MazeCell(c, r));

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }
            }

            if (!hasUnvisitedNeighbor(r, c) || (visited % hunt) == 0)
            {
                kill = 1;

                return;
            }

            int d = rng.Next(1, 5);

            if (d == 1)
            {
                visitHKStraight_BuildWalls(r - 1, c, 1);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_BuildWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitHKStraight_BuildWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitHKStraight_BuildWalls(r, c - 1, 2);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_BuildWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_BuildWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitHKStraight_BuildWalls(r, c + 1, 3);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r + 1, c, 4);
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_BuildWalls(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitHKStraight_BuildWalls(r + 1, c, 4);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r, c + 1, 3);
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_BuildWalls(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKStraight_BuildWalls(r, c - 1, 2);
                        visitHKStraight_BuildWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitHKStraight_BuildWalls_DFS_Iteration_Stack(int r, int c)
        {
            int d = 0;

            grid[r, c].visited = 1;

            visited++;

            if (!hasUnvisitedNeighbor(r, c))
                return;

            visitedCellsStack.Push(new MazeCell(c, r, d));
            visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

            d = findCellDirection(r, c);

            bool sleep = false;

            do
            {
                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                {
                    grid[r, c].top = 1;

                    r = r - 1;

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                {
                    grid[r, c].right = 1;

                    c = c + 1;

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                {
                    grid[r + 1, c].top = 1;

                    r = r + 1;

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                {
                    grid[r, c - 1].right = 1;

                    c = c - 1;

                    sleep = true;
                }

                if (sleep)
                {
                    sleep = false;

                    visitedCellsStack.Push(new MazeCell(c, r, d));
                    visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

                    grid[r, c].visited = 1;

                    visited++;
                }

            } while (hasUnvisitedNeighbor(r, c));
        }

        void visitHKStraight_FillCells(int r, int c, int df)
        {
            if (kill == 1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)(r - 1) * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            if (!hasUnvisitedNeighbor(r, c) || (visited % hunt) == 0)
            {
                kill = 1;

                return;
            }

            int d = rng.Next(1, 5);

            if (d == 1)
            {
                visitHKStraight_FillCells(r - 1, c, 1);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r, c + 1, 3);
                        visitHKStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r + 1, c, 4);
                        visitHKStraight_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_FillCells(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r, c - 1, 2);
                        visitHKStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r + 1, c, 4);
                        visitHKStraight_FillCells(r, c - 1, 2);
                    }
                }
                else
                {
                    visitHKStraight_FillCells(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r, c - 1, 2);
                        visitHKStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r, c + 1, 3);
                        visitHKStraight_FillCells(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitHKStraight_FillCells(r, c - 1, 2);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r, c + 1, 3);
                        visitHKStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r + 1, c, 4);
                        visitHKStraight_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_FillCells(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r - 1, c, 1);
                        visitHKStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r + 1, c, 4);
                        visitHKStraight_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_FillCells(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r - 1, c, 1);
                        visitHKStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r, c + 1, 3);
                        visitHKStraight_FillCells(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitHKStraight_FillCells(r, c + 1, 3);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r, c - 1, 2);
                        visitHKStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r + 1, c, 4);
                        visitHKStraight_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r - 1, c, 1);
                        visitHKStraight_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r + 1, c, 4);
                        visitHKStraight_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_FillCells(r + 1, c, 4);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r - 1, c, 1);
                        visitHKStraight_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r, c - 1, 2);
                        visitHKStraight_FillCells(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitHKStraight_FillCells(r + 1, c, 4);
                d = rng.Next(1, 4);
                if (d == 1)
                {
                    visitHKStraight_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r, c - 1, 2);
                        visitHKStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r, c + 1, 3);
                        visitHKStraight_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKStraight_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r - 1, c, 1);
                        visitHKStraight_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r, c + 1, 3);
                        visitHKStraight_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKStraight_FillCells(r, c + 1, 3);
                    d = rng.Next(1, 3);
                    if (d == 1)
                    {
                        visitHKStraight_FillCells(r - 1, c, 1);
                        visitHKStraight_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKStraight_FillCells(r, c - 1, 2);
                        visitHKStraight_FillCells(r - 1, c, 1);
                    }
                }
            }
        }

        void visitHKStraight_FillCells_DFS_Iteration_Stack(int r, int c)
        {
            int d = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].visited = 1;

            visited++;

            int genVisited = 1;

            if (showGen && (genVisited % genSpeed) == 0)
                Thread.Sleep(sleepTime);

            if (!hasUnvisitedNeighbor(r, c))
            {
                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                return;
            }

            visitedCellsStack.Push(new MazeCell(c, r, d));

            d = findCellDirection(r, c);

            bool sleep = false;

            do
            {
                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r - 1;

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c + 1;

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r + 1;

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c - 1;

                    sleep = true;
                }

                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

            } while (hasUnvisitedNeighbor(r, c));

            while (visitedCellsStack.Count > 0)
            {
                MazeCell cell = visitedCellsStack.Pop();

                d = d != 0 ? d > 2 ? d - 2 : d + 2 : 0;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                genVisited++;

                if (showGen && (genVisited % genSpeed) == 0)
                    Thread.Sleep(sleepTime / 10);
            }

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
        }

        void visitHKJagged_RemoveWalls(int r, int c, int df)
        {
            if (kill == 1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r,c].visited != 0)
                return;

            grid[r,c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            if (!hasUnvisitedNeighbor(r, c) || (visited % hunt) == 0)
            {
                kill = 1;

                return;
            }

            int d = rng.Next(1, 5);

            while (d == df)
                d = rng.Next(1, 5);

            if (d == 1)
            {
                visitHKJagged_RemoveWalls(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == df);
                if (d == 2)
                {
                    visitHKJagged_RemoveWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitHKJagged_RemoveWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitHKJagged_RemoveWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitHKJagged_RemoveWalls(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_RemoveWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitHKJagged_RemoveWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_RemoveWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitHKJagged_RemoveWalls(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_RemoveWalls(r - 1, c, 1);
                    d = rng.Next(1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKJagged_RemoveWalls(r, c - 1, 2);
                    d = rng.Next(1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r + 1, c, 4);
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_RemoveWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitHKJagged_RemoveWalls(r + 1, c, 4);
                d = rng.Next(1, 3);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_RemoveWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKJagged_RemoveWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r, c + 1, 3);
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_RemoveWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKJagged_RemoveWalls(r, c - 1, 2);
                        visitHKJagged_RemoveWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitHKJagged_RemoveWalls_DFS_Iteration_Stack(int r, int c)
        {
            int d = 0;
            int d_prev = 0;
            int d_tries = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].visited = 1;

            visited++;

            int genVisited = 1;

            if (showGen && (genVisited % genSpeed) == 0)
                Thread.Sleep(sleepTime);

            if (!hasUnvisitedNeighbor(r, c))
            {
                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                return;
            }

            visitedCellsStack.Push(new MazeCell(c, r, d));

            d = findCellDirection(r, c);

            bool sleep = false;

            do
            {
                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r - 1;

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c + 1;

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r + 1;

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c - 1;

                    sleep = true;
                }
                else
                {
                    d = ++d_tries == 1 ? d > 2 ? d - 2 : d + 2 : d_prev;
                }

                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    d_prev = d;

                    d = (rng.Next(0, 2) * 2) + (d_prev % 2) + 1;

                    d_tries = 0;

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

            } while (hasUnvisitedNeighbor(r, c));

            while (visitedCellsStack.Count > 0)
            {
                MazeCell cell = visitedCellsStack.Pop();

                d_prev = d_prev != 0 ? d_prev > 2 ? d_prev - 2 : d_prev + 2 : 0;

                if (d_prev == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d_prev == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }
                else if (d_prev == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d_prev == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }

                r = cell.row;
                c = cell.column;

                d_prev = cell.direction;

                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                genVisited++;

                if (showGen && (genVisited % genSpeed) == 0)
                    Thread.Sleep(sleepTime / 10);
            }

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitHKJagged_BuildWalls(int r, int c, int df)
        {
            if (kill == 1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visitedCellsQueue.Enqueue(new MazeCell(c, r));

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;
                }
                else
                {
                    grid[r, c].top = 1;
                }
            }

            if (!hasUnvisitedNeighbor(r, c) || (visited % hunt) == 0)
            {
                kill = 1;

                return;
            }

            int d = rng.Next(1, 5);

            while (d == df)
                d = rng.Next(1, 5);

            if (d == 1)
            {
                visitHKJagged_BuildWalls(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == df);
                if (d == 2)
                {
                    visitHKJagged_BuildWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitHKJagged_BuildWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                    }
                }
                else
                {
                    visitHKJagged_BuildWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitHKJagged_BuildWalls(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_BuildWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitHKJagged_BuildWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_BuildWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitHKJagged_BuildWalls(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_BuildWalls(r - 1, c, 1);
                    d = rng.Next(1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKJagged_BuildWalls(r, c - 1, 2);
                    d = rng.Next(1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r + 1, c, 4);
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_BuildWalls(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitHKJagged_BuildWalls(r + 1, c, 4);
                d = rng.Next(1, 3);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_BuildWalls(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKJagged_BuildWalls(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r, c + 1, 3);
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_BuildWalls(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKJagged_BuildWalls(r, c - 1, 2);
                        visitHKJagged_BuildWalls(r - 1, c, 1);
                    }
                }
            }
        }

        void visitHKJagged_BuildWalls_DFS_Iteration_Stack(int r, int c)
        {
            int d = 0;
            int d_prev = 0;
            int d_tries = 0;

            grid[r, c].visited = 1;

            visited++;

            if (!hasUnvisitedNeighbor(r, c))
                return;

            visitedCellsStack.Push(new MazeCell(c, r, d));
            visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

            d = findCellDirection(r, c);

            bool sleep = false;

            do
            {
                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                {
                    grid[r, c].top = 1;

                    r = r - 1;

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                {
                    grid[r, c].right = 1;

                    c = c + 1;

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                {
                    grid[r + 1, c].top = 1;

                    r = r + 1;

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                {
                    grid[r, c - 1].right = 1;

                    c = c - 1;

                    sleep = true;
                }
                else
                {
                    d = ++d_tries == 1 ? d > 2 ? d - 2 : d + 2 : d_prev;
                }

                if (sleep)
                {
                    sleep = false;

                    visitedCellsStack.Push(new MazeCell(c, r, d));
                    visitedCellsQueue.Enqueue(visitedCellsStack.Peek());

                    d_prev = d;

                    d = (rng.Next(0, 2) * 2) + (d_prev % 2) + 1;

                    d_tries = 0;

                    grid[r, c].visited = 1;

                    visited++;
                }

            } while (hasUnvisitedNeighbor(r, c));
        }

        void visitHKJagged_FillCells(int r, int c, int df)
        {
            if (kill == 1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (grid[r, c].visited != 0)
                return;

            grid[r, c].visited = 1;

            visited++;

            if (df != 0)
            {
                if (df == 1)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }
                else if (df == 2)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else if (df == 3)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 1, start_y + ((float)r * spacer) + 1), new SizeF((spacer * 2) - 2, spacer - 2)));
                }
                else
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 1, start_y + ((float)(r - 1) * spacer) + 1), new SizeF(spacer - 2, (spacer * 2) - 2)));
                }

                if (showGen && (visited % genSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            if (!hasUnvisitedNeighbor(r, c) || (visited % hunt) == 0)
            {
                kill = 1;

                return;
            }

            int d = rng.Next(1, 5);

            while (d == df)
                d = rng.Next(1, 5);

            if (d == 1)
            {
                visitHKJagged_FillCells(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == df);
                if (d == 2)
                {
                    visitHKJagged_FillCells(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitHKJagged_FillCells(r, c + 1, 3);
                        visitHKJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r + 1, c, 4);
                        visitHKJagged_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitHKJagged_FillCells(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_FillCells(r, c - 1, 2);
                        visitHKJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r + 1, c, 4);
                        visitHKJagged_FillCells(r, c - 1, 2);
                    }
                }
                else
                {
                    visitHKJagged_FillCells(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_FillCells(r, c - 1, 2);
                        visitHKJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r, c + 1, 3);
                        visitHKJagged_FillCells(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                visitHKJagged_FillCells(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_FillCells(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == df);
                    if (d == 3)
                    {
                        visitHKJagged_FillCells(r, c + 1, 3);
                        visitHKJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r + 1, c, 4);
                        visitHKJagged_FillCells(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    visitHKJagged_FillCells(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_FillCells(r - 1, c, 1);
                        visitHKJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r + 1, c, 4);
                        visitHKJagged_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_FillCells(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_FillCells(r - 1, c, 1);
                        visitHKJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r, c + 1, 3);
                        visitHKJagged_FillCells(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                visitHKJagged_FillCells(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_FillCells(r - 1, c, 1);
                    d = rng.Next(1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_FillCells(r, c - 1, 2);
                        visitHKJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r + 1, c, 4);
                        visitHKJagged_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKJagged_FillCells(r, c - 1, 2);
                    d = rng.Next(1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_FillCells(r - 1, c, 1);
                        visitHKJagged_FillCells(r + 1, c, 4);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r + 1, c, 4);
                        visitHKJagged_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_FillCells(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_FillCells(r - 1, c, 1);
                        visitHKJagged_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r, c - 1, 2);
                        visitHKJagged_FillCells(r - 1, c, 1);
                    }
                }
            }
            else
            {
                visitHKJagged_FillCells(r + 1, c, 4);
                d = rng.Next(1, 3);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == df);
                if (d == 1)
                {
                    visitHKJagged_FillCells(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == df);
                    if (d == 2)
                    {
                        visitHKJagged_FillCells(r, c - 1, 2);
                        visitHKJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r, c + 1, 3);
                        visitHKJagged_FillCells(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    visitHKJagged_FillCells(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_FillCells(r - 1, c, 1);
                        visitHKJagged_FillCells(r, c + 1, 3);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r, c + 1, 3);
                        visitHKJagged_FillCells(r - 1, c, 1);
                    }
                }
                else
                {
                    visitHKJagged_FillCells(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == df);
                    if (d == 1)
                    {
                        visitHKJagged_FillCells(r - 1, c, 1);
                        visitHKJagged_FillCells(r, c - 1, 2);
                    }
                    else
                    {
                        visitHKJagged_FillCells(r, c - 1, 2);
                        visitHKJagged_FillCells(r - 1, c, 1);
                    }
                }
            }
        }

        void visitHKJagged_FillCells_DFS_Iteration_Stack(int r, int c)
        {
            int d = 0;
            int d_prev = 0;
            int d_tries = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
            mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].visited = 1;

            visited++;

            int genVisited = 1;

            if (showGen && (genVisited % genSpeed) == 0)
                Thread.Sleep(sleepTime);

            if (!hasUnvisitedNeighbor(r, c))
            {
                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                return;
            }

            visitedCellsStack.Push(new MazeCell(c, r, d));

            d = findCellDirection(r, c);

            bool sleep = false;

            do
            {
                if (d == 1 && r > 0 && grid[r - 1, c].visited == 0)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r - 1;

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c + 1].visited == 0)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c + 1;

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].visited == 0)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r = r + 1;

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].visited == 0)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));
                    mazeCanvas.FillRectangle(new SolidBrush(colorGenerate), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c = c - 1;

                    sleep = true;
                }
                else
                {
                    d = ++d_tries == 1 ? d > 2 ? d - 2 : d + 2 : d_prev;
                }

                if (sleep)
                {
                    sleep = false;

                    mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(new MazeCell(c, r, d));

                    d_prev = d;

                    d = (rng.Next(0, 2) * 2) + (d_prev % 2) + 1;

                    d_tries = 0;

                    grid[r, c].visited = 1;

                    visited++;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

            } while (hasUnvisitedNeighbor(r, c));

            while (visitedCellsStack.Count > 0)
            {
                MazeCell cell = visitedCellsStack.Pop();

                d_prev = d_prev != 0 ? d_prev > 2 ? d_prev - 2 : d_prev + 2 : 0;

                if (d_prev == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d_prev == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }
                else if (d_prev == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                }
                else if (d_prev == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                }

                r = cell.row;
                c = cell.column;

                d_prev = cell.direction;

                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                genVisited++;

                if (showGen && (genVisited % genSpeed) == 0)
                    Thread.Sleep(sleepTime / 10);
            }

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
        }

        void markNeighbors_RemoveWalls(int r, int c)
        {
            if (r > 0 && grid[r - 1, c].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            if (c < (width - 1) && grid[r, c + 1].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            if (r < (height - 1) && grid[r + 1, c].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            if (c > 0 && grid[r, c - 1].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitPrim_RemoveWalls()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int r_prev = r;
            int c_prev = c;

            //mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].visited = 1;

            visited = 1;

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);

                    //mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    //r_prev = r;
                    //c_prev = c;

                    pickCell_RemoveWalls(r, c);

                    //mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    markNeighbors_RemoveWalls(r, c);

                    grid[r, c].visited = 1;

                    visited++;
                }
            }

            //mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitPrim_BuildWalls()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            grid[r, c].visited = 1;

            visited = 1;

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    pickCell_BuildWalls(r, c);

                    grid[r, c].visited = 1;

                    visited++;
                }
            }
        }

        void markNeighbors_FillCells(int r, int c)
        {
            if (r > 0 && grid[r - 1, c].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            if (c < (width - 1) && grid[r, c + 1].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            if (r < (height - 1) && grid[r + 1, c].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            if (c > 0 && grid[r, c - 1].visited == 0)
                mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitPrim_FillCells()
        {
            int r = rng.Next(0, height);
            int c = rng.Next(0, width);

            int r_prev = r;
            int c_prev = c;

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
            //mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].visited = 1;

            visited = 1;

            while (visited < (height * width))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                if (grid[r, c].visited == 0 && hasVisitedNeighbor(r, c))
                {
                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);

                    //mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    //r_prev = r;
                    //c_prev = c;

                    pickCell_FillCells(r, c);

                    //mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    markNeighbors_FillCells(r, c);

                    grid[r, c].visited = 1;

                    visited++;
                }
            }

            //mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void flood_fill(int r, int c, int oi, int ni)
        {
            if (r < 0 || r == height || c < 0 || c == width)
                return;

            if (grid[r, c].identity != oi)
                return;

            grid[r, c].identity = ni;

            flood_fill(r, c - 1, oi, ni);
            flood_fill(r - 1, c, oi, ni);
            flood_fill(r, c + 1, oi, ni);
            flood_fill(r + 1, c, oi, ni);
        }

        void flood_fill_Stack(int r, int c, int oi, int ni)
        {
            mazeCanvas.FillRectangle(new SolidBrush(colorCurrentCell), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            visitedCellsStack.Push(new MazeCell(c, r));

            bool sleep = false;

            do
            {
                if (sleep)
                    sleep = false;

                r = visitedCellsStack.Peek().row;
                c = visitedCellsStack.Peek().column;

                grid[r, c].identity = ni;

                if (r > 0 && grid[r - 1, c].identity == oi && grid[r, c].top == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c, r - 1));

                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].identity == oi && grid[r, c].right == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c + 1, r));

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].identity == oi && grid[r + 1, c].top == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c, r + 1));

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].identity == oi && grid[r, c - 1].right == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c - 1, r));

                    sleep = true;
                }

                if (!sleep)
                    visitedCellsStack.Pop();

            } while (visitedCellsStack.Count > 0);
        }

        void flood_fill_Stack_Color(int r, int c, int oi, int ni, Color newColor)
        {
            visitedCellsStack.Push(new MazeCell(c, r));

            bool sleep = false;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    if (showGen && ((visitedCellsStack.Count - 1) % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = visitedCellsStack.Peek().row;
                c = visitedCellsStack.Peek().column;

                grid[r, c].identity = ni;
                grid[r, c].color = newColor;

                if (r > 0 && grid[r - 1, c].identity == oi && grid[r, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));

                    visitedCellsStack.Push(new MazeCell(c, r - 1));

                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].identity == oi && grid[r, c].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsStack.Push(new MazeCell(c + 1, r));

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].identity == oi && grid[r + 1, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));

                    visitedCellsStack.Push(new MazeCell(c, r + 1));

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].identity == oi && grid[r, c - 1].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsStack.Push(new MazeCell(c - 1, r));

                    sleep = true;
                }

                if (!sleep)
                    visitedCellsStack.Pop();

            } while (visitedCellsStack.Count > 0);
        }

        void flood_fill_Queue_Color(int r, int c, int oi, int ni, Color newColor)
        {
            visitedCellsQueue.Enqueue(new MazeCell(c, r));

            genVisited = 1;

            bool sleep = false;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    genVisited++;

                    if (showGen && (genVisited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = visitedCellsQueue.Peek().row;
                c = visitedCellsQueue.Peek().column;

                visitedCellsQueue.Dequeue();

                grid[r, c].identity = ni;
                grid[r, c].color = newColor;

                if (r > 0 && grid[r - 1, c].identity == oi && grid[r, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));

                    visitedCellsQueue.Enqueue(new MazeCell(c, r - 1));

                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].identity == oi && grid[r, c].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsQueue.Enqueue(new MazeCell(c + 1, r));

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].identity == oi && grid[r + 1, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));

                    visitedCellsQueue.Enqueue(new MazeCell(c, r + 1));

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].identity == oi && grid[r, c - 1].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsQueue.Enqueue(new MazeCell(c - 1, r));

                    sleep = true;
                }

            } while (visitedCellsQueue.Count > 0);
        }

        void solve_flood_fill_Queue_Color(int r, int c, int oi, int ni, Color newColor)
        {
            visitedCellsQueue.Enqueue(new MazeCell(c, r));

            do
            {
                r = visitedCellsQueue.Peek().row;
                c = visitedCellsQueue.Peek().column;

                visitedCellsQueue.Dequeue();

                grid[r, c].identity = ni;
                grid[r, c].color = newColor;

                if (r > 0 && grid[r - 1, c].identity == oi && grid[r, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));

                    visitedCellsQueue.Enqueue(new MazeCell(c, r - 1));
                }

                if (c < (width - 1) && grid[r, c + 1].identity == oi && grid[r, c].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsQueue.Enqueue(new MazeCell(c + 1, r));
                }

                if (r < (height - 1) && grid[r + 1, c].identity == oi && grid[r + 1, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));

                    visitedCellsQueue.Enqueue(new MazeCell(c, r + 1));
                }

                if (c > 0 && grid[r, c - 1].identity == oi && grid[r, c - 1].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(newColor), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsQueue.Enqueue(new MazeCell(c - 1, r));
                }

            } while (visitedCellsQueue.Count > 0);
        }

        void flood_fill_BuildWalls(int r, int c, int oi, int ni)
        {
            visitedCellsStack.Push(new MazeCell(c, r));

            bool sleep;

            do
            {
                sleep = false;

                r = visitedCellsStack.Peek().row;
                c = visitedCellsStack.Peek().column;

                grid[r, c].identity = ni;

                if (r > 0 && grid[r - 1, c].identity == oi && grid[r, c].top == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c, r - 1));

                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].identity == oi && grid[r, c].right == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c + 1, r));

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].identity == oi && grid[r + 1, c].top == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c, r + 1));

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].identity == oi && grid[r, c - 1].right == 1)
                {
                    visitedCellsStack.Push(new MazeCell(c - 1, r));

                    sleep = true;
                }

                if (!sleep)
                    visitedCellsStack.Pop();

            } while (visitedCellsStack.Count > 0);
        }

        void flood_fill_BFS(int r, int c)
        {
            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

            visitedCellsStack.Push(new MazeCell(c, r));

            bool sleep = false;

            do
            {
                if (sleep)
                {
                    sleep = false;

                    if (showGen && ((visitedCellsStack.Count - 1) % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = visitedCellsStack.Peek().row;
                c = visitedCellsStack.Peek().column;

                grid[r, c].visited = 0;

                if (r > 0 && grid[r - 1, c].visited == 1 && grid[r, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, spacer)));

                    visitedCellsStack.Push(new MazeCell(c, r - 1));

                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].visited == 1 && grid[r, c].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsStack.Push(new MazeCell(c + 1, r));

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].visited == 1 && grid[r + 1, c].top == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, spacer)));

                    visitedCellsStack.Push(new MazeCell(c, r + 1));

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].visited == 1 && grid[r, c - 1].right == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer, spacer - 2)));

                    visitedCellsStack.Push(new MazeCell(c - 1, r));

                    sleep = true;
                }

                if (!sleep)
                    visitedCellsStack.Pop();

            } while (visitedCellsStack.Count > 0);
        }

        void visitKruskal_RemoveWalls()
        {
            int r, c, d;

            while (visited < ((height * width) - 1))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r, c].top == 0 && grid[r, c].identity != grid[r - 1, c].identity)
                {
                    grid[r, c].top = 1;

                    flood_fill(r, c, grid[r, c].identity, grid[r - 1, c].identity);

                    visited++;

                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
                else if (d == 2 && c > 0 && grid[r, c - 1].right == 0 && grid[r, c].identity != grid[r, c - 1].identity)
                {
                    grid[r, c - 1].right = 1;

                    flood_fill(r, c, grid[r, c].identity, grid[r, c - 1].identity);

                    visited++;

                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
                else if (d == 3 && c < (width - 1) && grid[r, c].right == 0 && grid[r, c].identity != grid[r, c + 1].identity)
                {
                    grid[r, c].right = 1;

                    flood_fill(r, c, grid[r, c].identity, grid[r, c + 1].identity);

                    visited++;

                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
                else if (d == 4 && r < (height - 1) && grid[r + 1, c].top == 0 && grid[r, c].identity != grid[r + 1, c].identity)
                {
                    grid[r + 1, c].top = 1;

                    flood_fill(r, c, grid[r, c].identity, grid[r + 1, c].identity);

                    visited++;

                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
            }
        }

        void visitKruskal_RemoveWalls_DFS_Iteration_Stack()
        {
            int r = 0;
            int c = 0;

            int r_prev = 0;
            int c_prev = 0;

            int d = 0;

            bool sleep = false;

            visited = 0;

            while (visited < ((height * width) - 1))
            {
                if (sleep)
                {
                    sleep = false;

                    r_prev = r;
                    c_prev = c;

                    visited++;

                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r, c].top == 0 && grid[r, c].identity != grid[r - 1, c].identity)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer)), new SizeF(spacer - 2, 2)));

                    flood_fill_Stack(r, c, grid[r, c].identity, grid[r - 1, c].identity);

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c].right == 0 && grid[r, c].identity != grid[r, c + 1].identity)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                    flood_fill_Stack(r, c, grid[r, c].identity, grid[r, c + 1].identity);

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].top == 0 && grid[r, c].identity != grid[r + 1, c].identity)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - 2, 2)));

                    flood_fill_Stack(r, c, grid[r, c].identity, grid[r + 1, c].identity);

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].right == 0 && grid[r, c].identity != grid[r, c - 1].identity)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + 2), new SizeF(2, spacer - 2)));

                    flood_fill_Stack(r, c, grid[r, c].identity, grid[r, c - 1].identity);

                    sleep = true;
                }
            }

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
        }

        void visitKruskal_BuildWalls_DFS_Iteration_Stack()
        {
            int r, c, d;

            visited = 0;

            while (visited < ((height * width) - 1))
            {
                r = rng.Next(0, height);
                c = rng.Next(0, width);

                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r, c].top == 0 && grid[r, c].identity != grid[r - 1, c].identity)
                {
                    grid[r, c].top = 1;

                    flood_fill_BuildWalls(r, c, grid[r, c].identity, grid[r - 1, c].identity);

                    visited++;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c].right == 0 && grid[r, c].identity != grid[r, c + 1].identity)
                {
                    grid[r, c].right = 1;

                    flood_fill_BuildWalls(r, c, grid[r, c].identity, grid[r, c + 1].identity);

                    visited++;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].top == 0 && grid[r, c].identity != grid[r + 1, c].identity)
                {
                    grid[r + 1, c].top = 1;

                    flood_fill_BuildWalls(r, c, grid[r, c].identity, grid[r + 1, c].identity);

                    visited++;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].right == 0 && grid[r, c].identity != grid[r, c - 1].identity)
                {
                    grid[r, c - 1].right = 1;

                    flood_fill_BuildWalls(r, c, grid[r, c].identity, grid[r, c - 1].identity);

                    visited++;
                }
            }
        }

        void visitKruskal_FillCells_DFS_Iteration_Stack()
        {
            int r = 0;
            int c = 0;

            int r_prev = 0;
            int c_prev = 0;

            int d = 0;

            bool sleep = false;

            visited = 0;

            while (visited < ((height * width) - 1))
            {
                if (sleep)
                {
                    sleep = false;

                    r_prev = r;
                    c_prev = c;

                    visited++;

                    if (showGen && (visited % genSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = rng.Next(0, height);
                c = rng.Next(0, width);

                d = rng.Next(1, 5);

                if (d == 1 && r > 0 && grid[r, c].top == 0 && grid[r, c].identity != grid[r - 1, c].identity)
                {
                    grid[r, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(grid[r - 1, c].color), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)(r - 1) * spacer) + 2), new SizeF(spacer - 2, (spacer * 2) - 2)));

                    flood_fill_Queue_Color(r, c, grid[r, c].identity, grid[r - 1, c].identity, grid[r - 1, c].color);

                    sleep = true;
                }
                else if (d == 2 && c < (width - 1) && grid[r, c].right == 0 && grid[r, c].identity != grid[r, c + 1].identity)
                {
                    grid[r, c].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(grid[r, c + 1].color), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF((spacer * 2) - 2, spacer - 2)));

                    flood_fill_Queue_Color(r, c, grid[r, c].identity, grid[r, c + 1].identity, grid[r, c + 1].color);

                    sleep = true;
                }
                else if (d == 3 && r < (height - 1) && grid[r + 1, c].top == 0 && grid[r, c].identity != grid[r + 1, c].identity)
                {
                    grid[r + 1, c].top = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(grid[r + 1, c].color), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, (spacer * 2) - 2)));

                    flood_fill_Queue_Color(r, c, grid[r, c].identity, grid[r + 1, c].identity, grid[r + 1, c].color);

                    sleep = true;
                }
                else if (d == 4 && c > 0 && grid[r, c - 1].right == 0 && grid[r, c].identity != grid[r, c - 1].identity)
                {
                    grid[r, c - 1].right = 1;

                    mazeCanvas.FillRectangle(new SolidBrush(grid[r, c - 1].color), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF((spacer * 2) - 2, spacer - 2)));

                    flood_fill_Queue_Color(r, c, grid[r, c].identity, grid[r, c - 1].identity, grid[r, c - 1].color);

                    sleep = true;
                }
            }

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + 2, start_y + ((float)r_prev * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

            flood_fill_Queue_Color(r_prev, c_prev, grid[r_prev, c_prev].identity, -1, colorBackground);
        }

        void solve_DFS_Recursion(int r, int c, int df)
        {
            if (kill == -1)
                return;

            if (r < 0 || r > (height - 1) || c < 0 || c > (width - 1))
                return;

            if (df != 0 || !(startSolve.Y == r && startSolve.X == c))
            {
                if (df == 1)
                {
                    if (grid[r + 1,c].top == 0)
                        return;
                }
                else if (df == 2)
                {
                    if (grid[r,c].right == 0)
                        return;
                }
                else if (df == 3)
                {
                    if (grid[r,c - 1].right == 0)
                        return;
                }
                else
                {
                    if (grid[r,c].top == 0)
                        return;
                }
            }

            if (grid[r, c].solution == 2)
                return;

            kill++;

            if (grid[r, c].solution == 1)
            {
                if (df == 1)
                {
                    grid[r + 1, c].solution = 2;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + 4, start_y + ((float)r * spacer) + 4), new SizeF(spacer - 8, (spacer * 2) - 8)));
                }
                else if (df == 2)
                {
                    grid[r, c + 1].solution = 2;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + 4, start_y + ((float)r * spacer) + 4), new SizeF((spacer * 2) - 8, spacer - 8)));
                }
                else if (df == 3)
                {
                    grid[r, c - 1].solution = 2;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 4, start_y + ((float)r * spacer) + 4), new SizeF((spacer * 2) - 8, spacer - 8)));
                }
                else
                {
                    grid[r - 1, c].solution = 2;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + 4, start_y + ((float)(r - 1) * spacer) + 4), new SizeF(spacer - 8, (spacer * 2) - 8)));
                }

                return;
            }
            else
            {
                grid[r, c].solution = 1;

                if (df != 0)
                {
                    if (df == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 4, start_y + ((float)r * spacer) + 4), new SizeF(spacer - 8, (spacer * 2) - 8)));
                    }
                    else if (df == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 4, start_y + ((float)r * spacer) + 4), new SizeF((spacer * 2) - 8, spacer - 8)));
                    }
                    else if (df == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + 4, start_y + ((float)r * spacer) + 4), new SizeF((spacer * 2) - 8, spacer - 8)));
                    }
                    else
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 4, start_y + ((float)(r - 1) * spacer) + 4), new SizeF(spacer - 8, (spacer * 2) - 8)));
                    }
                }
            }

            if (stopSolve.Y == r && stopSolve.X == c)
            {
                kill = -1;

                return;
            }

            int d = rng.Next(1, 5);

            while (d == (5 - df))
                d = rng.Next(1, 5);

            if (d == 1)
            {
                solve_DFS_Recursion(r - 1, c, 1);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 1)
                        d = 4;
                } while (d == (5 - df));
                if (d == 2)
                {
                    solve_DFS_Recursion(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == (5 - df));
                    if (d == 3)
                    {
                        solve_DFS_Recursion(r, c + 1, 3);
                        solve_DFS_Recursion(r + 1, c, 4);
                    }
                    else
                    {
                        solve_DFS_Recursion(r + 1, c, 4);
                        solve_DFS_Recursion(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    solve_DFS_Recursion(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == (5 - df));
                    if (d == 2)
                    {
                        solve_DFS_Recursion(r, c - 1, 2);
                        solve_DFS_Recursion(r + 1, c, 4);
                    }
                    else
                    {
                        solve_DFS_Recursion(r + 1, c, 4);
                        solve_DFS_Recursion(r, c - 1, 2);
                    }
                }
                else
                {
                    solve_DFS_Recursion(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == (5 - df));
                    if (d == 2)
                    {
                        solve_DFS_Recursion(r, c - 1, 2);
                        solve_DFS_Recursion(r, c + 1, 3);
                    }
                    else
                    {
                        solve_DFS_Recursion(r, c + 1, 3);
                        solve_DFS_Recursion(r, c - 1, 2);
                    }
                }
            }
            else if (d == 2)
            {
                solve_DFS_Recursion(r, c - 1, 2);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 2)
                        d = 4;
                } while (d == (5 - df));
                if (d == 1)
                {
                    solve_DFS_Recursion(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                        else
                            d = 4;
                    } while (d == (5 - df));
                    if (d == 3)
                    {
                        solve_DFS_Recursion(r, c + 1, 3);
                        solve_DFS_Recursion(r + 1, c, 4);
                    }
                    else
                    {
                        solve_DFS_Recursion(r + 1, c, 4);
                        solve_DFS_Recursion(r, c + 1, 3);
                    }
                }
                else if (d == 3)
                {
                    solve_DFS_Recursion(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == (5 - df));
                    if (d == 1)
                    {
                        solve_DFS_Recursion(r - 1, c, 1);
                        solve_DFS_Recursion(r + 1, c, 4);
                    }
                    else
                    {
                        solve_DFS_Recursion(r + 1, c, 4);
                        solve_DFS_Recursion(r - 1, c, 1);
                    }
                }
                else
                {
                    solve_DFS_Recursion(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == (5 - df));
                    if (d == 1)
                    {
                        solve_DFS_Recursion(r - 1, c, 1);
                        solve_DFS_Recursion(r, c + 1, 3);
                    }
                    else
                    {
                        solve_DFS_Recursion(r, c + 1, 3);
                        solve_DFS_Recursion(r - 1, c, 1);
                    }
                }
            }
            else if (d == 3)
            {
                solve_DFS_Recursion(r, c + 1, 3);
                do
                {
                    d = rng.Next(1, 4);
                    if (d == 3)
                        d = 4;
                } while (d == (5 - df));
                if (d == 1)
                {
                    solve_DFS_Recursion(r - 1, c, 1);
                    d = rng.Next(1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 4;
                    } while (d == (5 - df));
                    if (d == 2)
                    {
                        solve_DFS_Recursion(r, c - 1, 2);
                        solve_DFS_Recursion(r + 1, c, 4);
                    }
                    else
                    {
                        solve_DFS_Recursion(r + 1, c, 4);
                        solve_DFS_Recursion(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    solve_DFS_Recursion(r, c - 1, 2);
                    d = rng.Next(1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 4;
                    } while (d == (5 - df));
                    if (d == 1)
                    {
                        solve_DFS_Recursion(r - 1, c, 1);
                        solve_DFS_Recursion(r + 1, c, 4);
                    }
                    else
                    {
                        solve_DFS_Recursion(r + 1, c, 4);
                        solve_DFS_Recursion(r - 1, c, 1);
                    }
                }
                else
                {
                    solve_DFS_Recursion(r + 1, c, 4);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == (5 - df));
                    if (d == 1)
                    {
                        solve_DFS_Recursion(r - 1, c, 1);
                        solve_DFS_Recursion(r, c - 1, 2);
                    }
                    else
                    {
                        solve_DFS_Recursion(r, c - 1, 2);
                        solve_DFS_Recursion(r - 1, c, 1);
                    }
                }
            }
            else
            {
                solve_DFS_Recursion(r + 1, c, 4);
                d = rng.Next(1, 4);
                do
                {
                    d = rng.Next(1, 4);
                } while (d == (5 - df));
                if (d == 1)
                {
                    solve_DFS_Recursion(r - 1, c, 1);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 1)
                            d = 3;
                    } while (d == (5 - df));
                    if (d == 2)
                    {
                        solve_DFS_Recursion(r, c - 1, 2);
                        solve_DFS_Recursion(r, c + 1, 3);
                    }
                    else
                    {
                        solve_DFS_Recursion(r, c + 1, 3);
                        solve_DFS_Recursion(r, c - 1, 2);
                    }
                }
                else if (d == 2)
                {
                    solve_DFS_Recursion(r, c - 1, 2);
                    do
                    {
                        d = rng.Next(1, 3);
                        if (d == 2)
                            d = 3;
                    } while (d == (5 - df));
                    if (d == 1)
                    {
                        solve_DFS_Recursion(r - 1, c, 1);
                        solve_DFS_Recursion(r, c + 1, 3);
                    }
                    else
                    {
                        solve_DFS_Recursion(r, c + 1, 3);
                        solve_DFS_Recursion(r - 1, c, 1);
                    }
                }
                else
                {
                    solve_DFS_Recursion(r, c + 1, 3);
                    do
                    {
                        d = rng.Next(1, 3);
                    } while (d == (5 - df));
                    if (d == 1)
                    {
                        solve_DFS_Recursion(r - 1, c, 1);
                        solve_DFS_Recursion(r, c - 1, 2);
                    }
                    else
                    {
                        solve_DFS_Recursion(r, c - 1, 2);
                        solve_DFS_Recursion(r - 1, c, 1);
                    }
                }
            }
        }

        bool hasUnvisitedNeighbor_Solve(int r, int c)
        {
            if (r > 0 && grid[r - 1, c].solution == 0 && grid[r, c].top == 1)
                return true;

            if (c < (width - 1) && grid[r, c + 1].solution == 0 && grid[r, c].right == 1)
                return true;

            if (r < (height - 1) && grid[r + 1, c].solution == 0 && grid[r + 1, c].top == 1)
                return true;

            if (c > 0 && grid[r, c - 1].solution == 0 && grid[r, c - 1].right == 1)
                return true;

            return false;
        }

        void solve_DFS_Iteration()
        {
            int r = startSolve.Y;
            int c = startSolve.X;

            int d = 0;

            mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

            grid[r, c].solution = 1;

            visitedCellsStack.Push(new MazeCell(c, r, 0));

            int solvevisited = 1;

            bool sleep = false;

            while (!(r == stopSolve.Y && c == stopSolve.X))
            {
                if (hasUnvisitedNeighbor_Solve(r, c))
                {
                    d = rng.Next(1, 5);

                    if (d == 1 && r > 0 && grid[r - 1, c].solution == 0 && grid[r, c].top == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                        
                        grid[r - 1, c].solution = 1;

                        visitedCellsStack.Push(new MazeCell(c, r - 1, d));

                        r = r - 1;

                        sleep = true;
                    }
                    else if (d == 2 && c < (width - 1) && grid[r, c + 1].solution == 0 && grid[r, c].right == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                        
                        grid[r, c + 1].solution = 1;

                        visitedCellsStack.Push(new MazeCell(c + 1, r, d));

                        c = c + 1;

                        sleep = true;
                    }
                    else if (d == 3 && r < (height - 1) && grid[r + 1, c].solution == 0 && grid[r + 1, c].top == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        grid[r + 1, c].solution = 1;

                        visitedCellsStack.Push(new MazeCell(c, r + 1, d));

                        r = r + 1;

                        sleep = true;
                    }
                    else if (d == 4 && c > 0 && grid[r, c - 1].solution == 0 && grid[r, c - 1].right == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        grid[r, c - 1].solution = 1;

                        visitedCellsStack.Push(new MazeCell(c - 1, r, d));

                        c = c - 1;

                        sleep = true;
                    }
                }
                else
                {
                    grid[r, c].solution = 2;

                    if (d == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else if (d == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }
                    else if (d == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }
                    else
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    try
                    {
                        visitedCellsStack.Pop();

                        r = visitedCellsStack.Peek().row;
                        c = visitedCellsStack.Peek().column;

                        d = visitedCellsStack.Peek().direction;
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    sleep = true;
                }

                if (sleep)
                {
                    sleep = false;

                    if ((r == startSolve.Y && c == startSolve.X) ||
                        ((r + 1) == startSolve.Y && c == startSolve.X) ||
                        ((r - 1) == startSolve.Y && c == startSolve.X) ||
                        (r == startSolve.Y && (c + 1) == startSolve.X) ||
                        (r == startSolve.Y && (c - 1) == startSolve.X))
                    {
                        displayStart();
                    }

                    solvevisited++;

                    if (showSolve && (solvevisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }
            }

            displayStart();
        }

        void solve_BFS_Iteration()
        {
            int r = startSolve.Y;
            int c = startSolve.X;

            int d = 0;

            //float cell_padding_temp = cell_padding;

            //cell_padding = 0;

            //int showBacktracksCopy = showBacktracks;

            //setShowBacktracks(1);

            visitedCellsQueue.Enqueue(new MazeCell(c, r, d));

            grid[r, c].direction = d;

            grid[r, c].solution = 1;

            solveVisited = 1;

            bool sleep = false;
            
            while (!(r == stopSolve.Y && c == stopSolve.X) && runningSolve)
            {
                if (r > 0 && grid[r - 1, c].solution < 3 && grid[r, c].top == 1)
                {
                    if (showBacktracks)
                    {
                        if ((c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].top == 1 && grid[r - 1, c - 1].right == 1) &&
                            (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].top == 1 && grid[r - 1, c].right == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].top == 1 && grid[r - 1, c - 1].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].top == 1 && grid[r - 1, c].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                    }

                    if (grid[r - 1, c].solution == 0)
                    {
                        d = 1;

                        visitedCellsQueue.Enqueue(new MazeCell(c, r - 1, d));

                        grid[r - 1, c].direction = d;
                    }

                    grid[r - 1, c].solution++;

                    if ((r - 1) == stopSolve.Y && c == stopSolve.X)
                    {
                        r--;

                        break;
                    }
                    
                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].solution < 3 && grid[r, c].right == 1)
                {
                    if (showBacktracks)
                    {
                        if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].right == 1 && grid[r, c + 1].top == 1) &&
                            (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1 && grid[r + 1, c + 1].top == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer)));
                        else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].right == 1 && grid[r, c + 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer - cell_padding)));
                        else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1 && grid[r + 1, c + 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer - cell_padding - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    }

                    if (grid[r, c + 1].solution == 0)
                    {
                        d = 2;

                        visitedCellsQueue.Enqueue(new MazeCell(c + 1, r, d));

                        grid[r, c + 1].direction = d;
                    }

                    grid[r, c + 1].solution++;

                    if (r == stopSolve.Y && (c + 1) == stopSolve.X)
                    {
                        c++;

                        break;
                    }

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].solution < 3 && grid[r + 1, c].top == 1)
                {
                    if (showBacktracks)
                    {
                        if ((c > 0 && grid[r, c - 1].right == 1 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1) &&
                            (c < (width - 1) && grid[r, c].right == 1 && grid[r + 1, c + 1].top == 1 && grid[r + 1, c].right == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c > 0 && grid[r, c - 1].right == 1 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c < (width - 1) && grid[r, c].right == 1 && grid[r + 1, c + 1].top == 1 && grid[r + 1, c].right == 1) 
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                    }

                    if (grid[r + 1, c].solution == 0)
                    {
                        d = 3;

                        visitedCellsQueue.Enqueue(new MazeCell(c, r + 1, d));

                        grid[r + 1, c].direction = d;
                    }

                    grid[r + 1, c].solution++;

                    if ((r + 1) == stopSolve.Y && c == stopSolve.X)
                    {
                        r++;

                        break;
                    }

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].solution < 3 && grid[r, c - 1].right == 1)
                {
                    if (showBacktracks)
                    {
                        if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c - 1].right == 1 && grid[r, c - 1].top == 1) &&
                            (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c - 1].right == 1 && grid[r + 1, c - 1].top == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer)));
                        else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c - 1].right == 1 && grid[r, c - 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer - cell_padding)));
                        else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c - 1].right == 1 && grid[r + 1, c - 1].top == 1) 
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer - cell_padding - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    }

                    if (grid[r, c - 1].solution == 0)
                    {
                        d = 4;

                        visitedCellsQueue.Enqueue(new MazeCell(c - 1, r, d));

                        grid[r, c - 1].direction = d;
                    }

                    grid[r, c - 1].solution++;

                    if (r == stopSolve.Y && (c - 1) == stopSolve.X)
                    {
                        c--;

                        break;
                    }

                    sleep = true;
                }

                if (sleep)
                {
                    sleep = false;

                    if ((r == startSolve.Y && c == startSolve.X) ||
                        ((r + 1) == startSolve.Y && c == startSolve.X) ||
                        ((r - 1) == startSolve.Y && c == startSolve.X) ||
                        (r == startSolve.Y && (c + 1) == startSolve.X) ||
                        (r == startSolve.Y && (c - 1) == startSolve.X))
                    {
                        displayStart();
                    }

                    solveVisited++;

                    if (showBacktracks)
                        if (showSolve && (solveVisited % solveSpeed) == 0)
                            Thread.Sleep(sleepTime / 4);
                }

                try
                {
                    r = visitedCellsQueue.Peek().row;
                    c = visitedCellsQueue.Peek().column;

                    visitedCellsQueue.Dequeue();
                }
                catch (Exception)
                {
                    return;
                }
            }

            if (!runningSolve)
                return;

            displayStart();

            //cell_padding = cell_padding_temp;

            //setShowBacktracks(showBacktracksCopy);

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X) && runningSolve)
            {
                d = grid[r, c].direction;

                if (d == 1)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;

                    sleep = true;
                }
                else if (d == 2)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    
                    c--;

                    sleep = true;
                }
                else if (d == 3)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;
                    
                    sleep = true;
                }
                else if (d == 4)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;

                    sleep = true;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                if (sleep)
                {
                    sleep = false;

                    solveVisited++;

                    if (showBacktracks)
                        if (showSolve && (solveVisited % solveSpeed) == 0)
                            Thread.Sleep(sleepTime / 4);
                }
            }

            if (!runningSolve)
                return;

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve)
                    return;

                if (sleep)
                {
                    sleep = false;

                    solveVisited++;

                    if (showSolve && (solveVisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    sleep = true;
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    sleep = true;
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    sleep = true;
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    sleep = true;
                }
            }

            if (!runningSolve)
                return;

            displayStart();
        }

        void solve_BFS_Iteration2()
        {
            int r = startSolve.Y;
            int c = startSolve.X;

            int d = 0;

            //float cell_padding_temp = cell_padding;

            //cell_padding = 0;

            //int showBacktracksCopy = showBacktracks;

            //setShowBacktracks(1);

            visitedCellsQueue.Enqueue(new MazeCell(c, r, d));

            grid[r, c].direction = d;

            grid[r, c].solution = 1;

            int solvevisited = 1;

            bool sleep = false;

            while (!(r == stopSolve.Y && c == stopSolve.X))
            {
                if (r > 0 && grid[r - 1, c].solution < 3 && grid[r, c].top == 1)
                {
                    if (showBacktracks)
                    {
                        if ((c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].top == 1 && grid[r - 1, c - 1].right == 1) &&
                            (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].top == 1 && grid[r - 1, c].right == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 3) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].top == 1 && grid[r - 1, c - 1].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].top == 1 && grid[r - 1, c].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }

                    if (grid[r - 1, c].solution == 0)
                    {
                        d = 1;

                        visitedCellsQueue.Enqueue(new MazeCell(c, r - 1, d));

                        grid[r - 1, c].direction = d;
                    }

                    grid[r - 1, c].solution++;

                    if ((r - 1) == stopSolve.Y && c == stopSolve.X)
                    {
                        r--;

                        break;
                    }

                    sleep = true;
                }

                if (c < (width - 1) && grid[r, c + 1].solution < 3 && grid[r, c].right == 1)
                {
                    if (showBacktracks)
                    {
                        if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].right == 1 && grid[r, c + 1].top == 1) &&
                            (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1 && grid[r + 1, c + 1].top == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 3) - (cell_padding * 2) - 2)));
                        else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].right == 1 && grid[r, c + 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1 && grid[r + 1, c + 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    if (grid[r, c + 1].solution == 0)
                    {
                        d = 2;

                        visitedCellsQueue.Enqueue(new MazeCell(c + 1, r, d));

                        grid[r, c + 1].direction = d;
                    }

                    grid[r, c + 1].solution++;

                    if (r == stopSolve.Y && (c + 1) == stopSolve.X)
                    {
                        c++;

                        break;
                    }

                    sleep = true;
                }

                if (r < (height - 1) && grid[r + 1, c].solution < 3 && grid[r + 1, c].top == 1)
                {
                    if (showBacktracks)
                    {
                        if ((c > 0 && grid[r, c - 1].right == 1 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1) &&
                            (c < (width - 1) && grid[r, c].right == 1 && grid[r + 1, c + 1].top == 1 && grid[r + 1, c].right == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 3) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c > 0 && grid[r, c - 1].right == 1 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (c < (width - 1) && grid[r, c].right == 1 && grid[r + 1, c + 1].top == 1 && grid[r + 1, c].right == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    }

                    if (grid[r + 1, c].solution == 0)
                    {
                        d = 3;

                        visitedCellsQueue.Enqueue(new MazeCell(c, r + 1, d));

                        grid[r + 1, c].direction = d;
                    }

                    grid[r + 1, c].solution++;

                    if ((r + 1) == stopSolve.Y && c == stopSolve.X)
                    {
                        r++;

                        break;
                    }

                    sleep = true;
                }

                if (c > 0 && grid[r, c - 1].solution < 3 && grid[r, c - 1].right == 1)
                {
                    if (showBacktracks)
                    {
                        if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c - 1].right == 1 && grid[r, c - 1].top == 1) &&
                            (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c - 1].right == 1 && grid[r + 1, c - 1].top == 1))
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 3) - (cell_padding * 2) - 2)));
                        else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c - 1].right == 1 && grid[r, c - 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c - 1].right == 1 && grid[r + 1, c - 1].top == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF((spacer * 2) - (cell_padding * 2) - 2, (spacer * 2) - (cell_padding * 2) - 2)));
                        else
                            mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    }

                    if (grid[r, c - 1].solution == 0)
                    {
                        d = 4;

                        visitedCellsQueue.Enqueue(new MazeCell(c - 1, r, d));

                        grid[r, c - 1].direction = d;
                    }

                    grid[r, c - 1].solution++;

                    if (r == stopSolve.Y && (c - 1) == stopSolve.X)
                    {
                        c--;

                        break;
                    }

                    sleep = true;
                }

                if (sleep)
                {
                    sleep = false;

                    if ((r == startSolve.Y && c == startSolve.X) ||
                        ((r + 1) == startSolve.Y && c == startSolve.X) ||
                        ((r - 1) == startSolve.Y && c == startSolve.X) ||
                        (r == startSolve.Y && (c + 1) == startSolve.X) ||
                        (r == startSolve.Y && (c - 1) == startSolve.X))
                    {
                        displayStart();
                    }

                    solveVisited++;

                    if (showBacktracks)
                        if (showSolve && (solveVisited % solveSpeed) == 0)
                            Thread.Sleep(sleepTime / 4);
                }

                try
                {
                    r = visitedCellsQueue.Peek().row;
                    c = visitedCellsQueue.Peek().column;

                    visitedCellsQueue.Dequeue();
                }
                catch (Exception)
                {
                    return;
                }
            }

            displayStart();

            //cell_padding = cell_padding_temp;

            //setShowBacktracks(showBacktracksCopy);

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                d = grid[r, c].direction;

                if (d == 1)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;

                    sleep = true;
                }
                else if (d == 2)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c--;

                    sleep = true;
                }
                else if (d == 3)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;

                    sleep = true;
                }
                else if (d == 4)
                {
                    if (showBacktracks)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;

                    sleep = true;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                if (sleep)
                {
                    sleep = false;

                    solvevisited++;

                    if (showBacktracks)
                        if (showSolve && (solvevisited % solveSpeed) == 0)
                            Thread.Sleep(sleepTime / 4);
                }
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (sleep)
                {
                    sleep = false;

                    solvevisited++;

                    if (showSolve && (solvevisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = cell.row;
                c = cell.column;

                grid[r, c].solvepath = 1;

                d = cell.direction;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    sleep = true;
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    sleep = true;
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    sleep = true;
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    sleep = true;
                }
            }

            displayStart();
        }

        void solve_BFS_FindShortestPath()
        {
            int r = startSolve.Y;
            int c = startSolve.X;

            int d = 0;

            if (visitedCellsQueue.Count > 0)
                visitedCellsQueue.Clear();

            visitedCellsQueue.Enqueue(new MazeCell(c, r, d));

            grid[r, c].direction = d;

            while (!(r == stopSolve.Y && c == stopSolve.X))
            {
                if (r > 0 && grid[r - 1, c].solution == 0 && grid[r, c].top == 1)
                {
                    d = 1;

                    visitedCellsQueue.Enqueue(new MazeCell(c, r - 1, d));

                    grid[r - 1, c].direction = d;

                    grid[r - 1, c].solution = 1;

                    if ((r - 1) == stopSolve.Y && c == stopSolve.X)
                    {
                        r--;

                        break;
                    }
                }

                if (c < (width - 1) && grid[r, c + 1].solution == 0 && grid[r, c].right == 1)
                {
                    d = 2;

                    visitedCellsQueue.Enqueue(new MazeCell(c + 1, r, d));

                    grid[r, c + 1].direction = d;

                    grid[r, c + 1].solution = 1;

                    if (r == stopSolve.Y && (c + 1) == stopSolve.X)
                    {
                        c++;

                        break;
                    }
                }

                if (r < (height - 1) && grid[r + 1, c].solution == 0 && grid[r + 1, c].top == 1)
                {
                    d = 3;

                    visitedCellsQueue.Enqueue(new MazeCell(c, r + 1, d));

                    grid[r + 1, c].direction = d;

                    grid[r + 1, c].solution = 1;

                    if ((r + 1) == stopSolve.Y && c == stopSolve.X)
                    {
                        r++;

                        break;
                    }
                }

                if (c > 0 && grid[r, c - 1].solution == 0 && grid[r, c - 1].right == 1)
                {
                    d = 4;

                    visitedCellsQueue.Enqueue(new MazeCell(c - 1, r, d));

                    grid[r, c - 1].direction = d;

                    grid[r, c - 1].solution = 1;

                    if (r == stopSolve.Y && (c - 1) == stopSolve.X)
                    {
                        c--;

                        break;
                    }
                }

                try
                {
                    r = visitedCellsQueue.Peek().row;
                    c = visitedCellsQueue.Peek().column;

                    visitedCellsQueue.Dequeue();
                }
                catch (Exception)
                {
                    return;
                }
            }

            bool showPath = false;

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                d = grid[r, c].direction;

                if (d == 1)
                {
                    if (showPath)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;
                }
                else if (d == 2)
                {
                    if (showPath)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c--;
                }
                else if (d == 3)
                {
                    if (showPath)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;
                }
                else if (d == 4)
                {
                    if (showPath)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;
            }
        }

        void EvaluateCell_Dijkstra(int r, int c, int d, double g_origin)
        {
            if (grid[r, c].opened == 0 || (grid[r, c].opened == 1 && grid[r, c].g_pathLength > (g_origin + 1)))
            {
                grid[r, c].g_pathLength = g_origin + 1;

                if (grid[r, c].opened == 0)
                {
                    grid[r, c].row = r;
                    grid[r, c].column = c;

                    grid[r, c].opened = 1;
                }
                else
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(Color.Red), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                }

                grid[r, c].direction = d;

                grid[r, c].f_cost = grid[r, c].g_pathLength;

                openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));

                visited++;

                if (!showTracer)
                {
                    bool drawn = false;

                    if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].opened == 1) &&
                        (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].opened == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer + (cell_padding * 2) + 2)));
                    else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    else
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        drawn = true;
                    }

                    if ((c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].opened == 1) &&
                        (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].opened == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer + (cell_padding * 2) + 2, spacer - (cell_padding * 2) - 2)));
                    else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    else if (!drawn)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                }
            }
        }

        void EvaluateCell_GBFS(int r, int c, int d)
        {
            if (grid[r, c].opened == 0)
            {
                grid[r, c].opened = 1;

                grid[r, c].row = r;
                grid[r, c].column = c;

                grid[r, c].direction = d;

                grid[r, c].h_euclideanDistance = Math.Sqrt(Math.Pow(stopSolve.Y - r, 2) + Math.Pow(stopSolve.X - c, 2));

                openCells.Enqueue(grid[r, c], (grid[r, c].h_euclideanDistance, 0, 0, 0));

                visited++;

                if (!showTracer)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                }
            }
        }

        void EvaluateCell_A_Star_Manhattan2(int version, int r, int c, int d, double g_origin)
        {
            if (grid[r, c].opened == 0 || (grid[r, c].opened == 1 && grid[r, c].g_pathLength > (g_origin + 1)))
            {
                grid[r, c].g_pathLength = g_origin + 1;

                if (grid[r, c].opened == 0)
                {
                    grid[r, c].h_manhattanDistance = Math.Abs(stopSolve.Y - r) + Math.Abs(stopSolve.X - c);
                    grid[r, c].h_euclideanDistance = Math.Sqrt(Math.Pow(stopSolve.Y - r, 2) + Math.Pow(stopSolve.X - c, 2));
                    grid[r, c].h_vectorCrossProduct = Math.Abs(((c - stopSolve.X) * (startSolve.Y - stopSolve.Y)) - ((startSolve.X - stopSolve.X) * (r - stopSolve.Y)));

                    grid[r, c].row = r;
                    grid[r, c].column = c;

                    grid[r, c].opened = 1;
                }
                else
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(Color.Red), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                }

                grid[r, c].direction = d;

                if (version == 1)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 2)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance + (grid[r, c].h_vectorCrossProduct * 0.001);

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 3)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, grid[r, c].h_manhattanDistance, grid[r, c].h_euclideanDistance, 0));
                }
                else if (version == 4)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance + grid[r, c].h_euclideanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }

                visited++;
            }
        }

        void EvaluateCell_A_Star_Manhattan3(int version, int r, int c, int d, double g_origin)
        {
            if (grid[r, c].opened == 0 || (grid[r, c].opened == 1 && grid[r, c].g_pathLength > (g_origin + 1)))
            {
                grid[r, c].g_pathLength = g_origin + 1;

                if (grid[r, c].opened == 0)
                {
                    grid[r, c].h_manhattanDistance = Math.Abs(stopSolve.Y - r) + Math.Abs(stopSolve.X - c);
                    grid[r, c].h_euclideanDistance = Math.Sqrt(Math.Pow(stopSolve.Y - r, 2) + Math.Pow(stopSolve.X - c, 2));
                    grid[r, c].h_vectorCrossProduct = Math.Abs(((c - stopSolve.X) * (startSolve.Y - stopSolve.Y)) - ((startSolve.X - stopSolve.X) * (r - stopSolve.Y)));

                    grid[r, c].row = r;
                    grid[r, c].column = c;

                    grid[r, c].opened = 1;
                }
                else
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(Color.Red), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                }

                grid[r, c].direction = d;

                if (version == 1)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 2)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance + (grid[r, c].h_vectorCrossProduct * 0.001);

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 3)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, grid[r, c].h_manhattanDistance, grid[r, c].h_euclideanDistance, 0));
                }
                else if (version == 4)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance + grid[r, c].h_euclideanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }

                visited++;

                if (!showTracer)
                {
                    if (grid[r, c].top == 1 && grid[r, c].right == 1 && (r > 0 && grid[r - 1, c].right == 1) && (c < (width - 1) && grid[r, c + 1].top == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding - 2, spacer - cell_padding)));

                    if (grid[r, c].right == 1 && (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1) && (r < (height - 1) && c < (width - 1) && grid[r + 1, c + 1].top == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding - 2, spacer - cell_padding - 2)));

                    if ((r < (height - 1) && grid[r + 1, c].top == 1) && (c > 0 && grid[r, c - 1].right == 1) && (r < (height - 1) && c > 0 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding, spacer - cell_padding - 2)));

                    if (grid[r, c].top == 1 && (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1) && (r > 0 && c > 0 && grid[r - 1, c - 1].right == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding, spacer - cell_padding)));

                    bool drawn = false;

                    if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].opened == 1) &&
                        (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].opened == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer + (cell_padding * 2) + 2)));
                    else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    else
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        drawn = true;
                    }

                    if ((c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].opened == 1) &&
                        (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].opened == 1))
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer + (cell_padding * 2) + 2, spacer - (cell_padding * 2) - 2)));
                    else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    else if (!drawn)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                }
            }
        }

        void EvaluateCell_A_Star_Manhattan(int version, int r, int c, int d, double g_origin)
        {
            if (grid[r, c].opened == 0 || (grid[r, c].opened == 1 && grid[r, c].g_pathLength > (g_origin + 1)))
            {
                grid[r, c].g_pathLength = g_origin + 1;

                if (grid[r, c].opened == 0)
                {
                    grid[r, c].h_manhattanDistance = Math.Abs(stopSolve.Y - r) + Math.Abs(stopSolve.X - c);
                    grid[r, c].h_euclideanDistance = Math.Sqrt(Math.Pow(stopSolve.Y - r, 2) + Math.Pow(stopSolve.X - c, 2));
                    grid[r, c].h_vectorCrossProduct = Math.Abs(((c - stopSolve.X) * (startSolve.Y - stopSolve.Y)) - ((startSolve.X - stopSolve.X) * (r - stopSolve.Y)));

                    grid[r, c].row = r;
                    grid[r, c].column = c;

                    grid[r, c].opened = 1;
                }
                else
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(Color.Red), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                }

                grid[r, c].direction = d;

                if (version == 1)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 2)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance + (grid[r, c].h_vectorCrossProduct * 0.001);

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 3)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, grid[r, c].h_manhattanDistance, grid[r, c].h_euclideanDistance, 0));
                }
                else if (version == 4)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_manhattanDistance + grid[r, c].h_euclideanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }

                visited++;

                if (!showTracer)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    if (r > 0 && grid[r - 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF(spacer - (cell_padding * 2) - 2, 2)));

                    if (c < (width - 1) && grid[r, c + 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(2, spacer - (cell_padding * 2) - 2)));

                    if (r < (height - 1) && grid[r + 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - (cell_padding * 2) - 2, 2)));

                    if (c > 0 && grid[r, c - 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(2, spacer - (cell_padding * 2) - 2)));

                    if (r > 0 && c < (width - 1) && grid[r - 1, c].opened == 1 && grid[r - 1, c + 1].opened == 1 && grid[r, c + 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                    if (r < (height - 1) && c < (width - 1) && grid[r, c + 1].opened == 1 && grid[r + 1, c + 1].opened == 1 && grid[r + 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                    if (r < (height - 1) && c > 0 && grid[r + 1, c].opened == 1 && grid[r + 1, c - 1].opened == 1 && grid[r, c - 1].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                    if (r > 0 && c > 0 && grid[r, c - 1].opened == 1 && grid[r - 1, c - 1].opened == 1 && grid[r - 1, c].opened == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));
                }
            }
        }

        void EvaluateCell_A_Star_Chebyshev(int version, int r, int c, int d, double g_origin)
        {
            if (grid[r, c].opened == 0 || (grid[r, c].opened == 1 && grid[r, c].g_pathLength > (g_origin + 1)))
            {
                grid[r, c].g_pathLength = g_origin + 1;

                if (grid[r, c].opened == 0)
                {
                    double max = Math.Abs(stopSolve.Y - r);

                    if (Math.Abs(stopSolve.X - c) > max)
                        max = Math.Abs(stopSolve.X - c);

                    grid[r, c].h_chebyshevDistance = max;
                    grid[r, c].h_euclideanDistance = Math.Sqrt(Math.Pow(stopSolve.Y - r, 2) + Math.Pow(stopSolve.X - c, 2));
                    grid[r, c].h_vectorCrossProduct = Math.Abs(((c - stopSolve.X) * (startSolve.Y - stopSolve.Y)) - ((startSolve.X - stopSolve.X) * (r - stopSolve.Y)));

                    grid[r, c].row = r;
                    grid[r, c].column = c;

                    grid[r, c].opened = 1;
                }
                else
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(Color.Red), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                }

                grid[r, c].direction = d;

                if (version == 1)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_chebyshevDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 2)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_chebyshevDistance + (grid[r, c].h_vectorCrossProduct * 0.001);

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 3)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_chebyshevDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, grid[r, c].h_chebyshevDistance, grid[r, c].h_euclideanDistance, 0));
                }
                else if (version == 4)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_chebyshevDistance + grid[r, c].h_euclideanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }

                visited++;

                if (!showTracer)
                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
            }
        }

        void EvaluateCell_A_Star_Octile(int version, int r, int c, int d, double g_origin)
        {
            double cell_dist = 1;

            if (d > 4) cell_dist = Math.Sqrt(2);

            if (grid[r, c].opened == 0 || (grid[r, c].opened == 1 && grid[r, c].g_pathLength > (g_origin + cell_dist)))
            {
                grid[r, c].g_pathLength = g_origin + cell_dist;

                if (grid[r, c].opened == 0)
                {
                    double max = Math.Abs(stopSolve.Y - r);
                    double min = Math.Abs(stopSolve.X - c);

                    if (min > max)
                    {
                        max = min;
                        min = Math.Abs(stopSolve.Y - r);
                    }

                    grid[r, c].h_octileDistance = (min * Math.Sqrt(2)) + (max - min);
                    grid[r, c].h_euclideanDistance = Math.Sqrt(Math.Pow(stopSolve.Y - r, 2) + Math.Pow(stopSolve.X - c, 2));
                    grid[r, c].h_vectorCrossProduct = Math.Abs(((c - stopSolve.X) * (startSolve.Y - stopSolve.Y)) - ((startSolve.X - stopSolve.X) * (r - stopSolve.Y)));
                    double h_euclideanReverse = Math.Sqrt(Math.Pow(startSolve.Y - r, 2) + Math.Pow(startSolve.X - c, 2));
                    grid[r, c].row = r;
                    grid[r, c].column = c;

                    grid[r, c].opened = 1;
                }
                else
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(Color.Red), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                }

                grid[r, c].direction = d;

                if (version == 1)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_octileDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 2)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_octileDistance + (grid[r, c].h_vectorCrossProduct * 0.001);

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }
                else if (version == 3)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_octileDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, grid[r, c].h_octileDistance, grid[r, c].h_euclideanDistance, 0));
                }
                else if (version == 4)
                {
                    grid[r, c].f_cost = grid[r, c].g_pathLength + grid[r, c].h_octileDistance + grid[r, c].h_euclideanDistance;

                    openCells.Enqueue(grid[r, c], (grid[r, c].f_cost, 0, 0, 0));
                }

                visited++;

                if (!showTracer)
                {
                    //if (grid[r, c].top == 1 && grid[r, c].right == 1 && (r > 0 && grid[r - 1, c].right == 1) && (c < (width - 1) && grid[r, c + 1].top == 1))
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding - 2, spacer - cell_padding)));

                    //if (grid[r, c].right == 1 && (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1) && (r < (height - 1) && c < (width - 1) && grid[r + 1, c + 1].top == 1))
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding - 2, spacer - cell_padding - 2)));

                    //if ((r < (height - 1) && grid[r + 1, c].top == 1) && (c > 0 && grid[r, c - 1].right == 1) && (r < (height - 1) && c > 0 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1))
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding, spacer - cell_padding - 2)));

                    //if (grid[r, c].top == 1 && (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1) && (r > 0 && c > 0 && grid[r - 1, c - 1].right == 1))
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding, spacer - cell_padding)));

                    //bool drawn = false;

                    //if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].opened == 1) &&
                    //    (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].opened == 1))
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer + (cell_padding * 2) + 2)));
                    //else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].opened == 1)
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    //else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].opened == 1)
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    //else
                    //{
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    //    drawn = true;
                    //}

                    //if ((c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].opened == 1) &&
                    //    (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].opened == 1))
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer + (cell_padding * 2) + 2, spacer - (cell_padding * 2) - 2)));
                    //else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].opened == 1)
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    //else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].opened == 1)
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    //else if (!drawn)
                    //    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    
                    mazeCanvas.FillRectangle(new SolidBrush(colorBacktrack), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                }
            }
        }

        void solve_Dijkstra()
        {
            int r;
            int c;

            int d;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_Dijkstra(r, c, 0, -1);

            bool drawn;

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                solveVisited++;

                grid[r, c].closed = 1;

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                if (r == stopSolve.Y && c == stopSolve.X)
                    break;

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_Dijkstra(r - 1, c, 1, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_Dijkstra(r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_Dijkstra(r + 1, c, 3, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_Dijkstra(r, c - 1, 4, grid[r, c].g_pathLength);

                drawn = false;

                if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 1) &&
                    (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer + (cell_padding * 2) + 2)));
                else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    drawn = true;
                }

                if ((c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 1) &&
                    (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer + (cell_padding * 2) + 2, spacer - (cell_padding * 2) - 2)));
                else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (!drawn)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if (!runningSolve) return;

            } while (true);

            displayStart();

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c--;
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime / 2);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 2)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (d == 3)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 4)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();
        }

        void solve_GBFS()
        {
            int r;
            int c;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_GBFS(r, c, 0);

            allowFillCell = true;

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                solveVisited++;

                grid[r, c].closed = 1;

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                if (r == stopSolve.Y && c == stopSolve.X)
                {
                    allowFillCell = false;

                    break;
                }

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_GBFS(r - 1, c, 1);

                if (r > 0 && c < (width - 1) &&
                    (grid[r, c].top == 1 || grid[r, c].right == 1) &&
                    (grid[r, c].top == 1 || grid[r, c + 1].top == 1) &&
                    (grid[r, c].right == 1 || grid[r - 1, c].right == 1) &&
                    (grid[r, c + 1].top == 1 || grid[r - 1, c].right == 1))
                    //grid[r - 1, c + 1].obstacle == 0 && grid[r - 1, c + 1].closed == 0)
                    EvaluateCell_GBFS(r - 1, c + 1, 5);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_GBFS(r, c + 1, 2);

                if (r < (height - 1) && c < (width - 1) &&
                    (grid[r, c].right == 1 || grid[r + 1, c].top == 1) &&
                    (grid[r, c].right == 1 || grid[r + 1, c].right == 1) &&
                    (grid[r + 1, c].top == 1 || grid[r + 1, c + 1].top == 1) &&
                    (grid[r + 1, c].right == 1 || grid[r + 1, c + 1].top == 1))
                    //grid[r + 1, c + 1].obstacle == 0 && grid[r + 1, c + 1].closed == 0)
                    EvaluateCell_GBFS(r + 1, c + 1, 6);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_GBFS(r + 1, c, 3);

                if (r < (height - 1) && c > 0 &&
                    (grid[r + 1, c].top == 1 || grid[r, c - 1].right == 1) &&
                    (grid[r + 1, c].top == 1 || grid[r + 1, c - 1].top == 1) &&
                    (grid[r, c - 1].right == 1 || grid[r + 1, c - 1].right == 1) &&
                    (grid[r + 1, c - 1].top == 1 || grid[r + 1, c - 1].right == 1))
                    //grid[r + 1, c - 1].obstacle == 0 && grid[r + 1, c - 1].closed == 0)
                    EvaluateCell_GBFS(r + 1, c - 1, 7);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_GBFS(r, c - 1, 4);

                if (r > 0 && c > 0 &&
                    (grid[r, c - 1].right == 1 || grid[r, c].top == 1) &&
                    (grid[r, c - 1].right == 1 || grid[r - 1, c - 1].right == 1) &&
                    (grid[r, c].top == 1 || grid[r, c - 1].top == 1) &&
                    (grid[r - 1, c - 1].right == 1 || grid[r, c - 1].top == 1))
                    //grid[r - 1, c - 1].obstacle == 0 && grid[r - 1, c - 1].closed == 0)
                    EvaluateCell_GBFS(r - 1, c - 1, 8);

                mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if (!runningSolve) return;

            } while (openCells.Count > 0);

            if (openCells.Count == 0)
            {
                allowFillCell = false;

                displayStart();

                return;
            }

            displayStart();

            int d;

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                    r++;
                else if (d == 2)
                    c--;
                else if (d == 3)
                    r--;
                else if (d == 4)
                    c++;
                else if (d == 5)
                {
                    r++;
                    c--;
                }
                else if (d == 6)
                {
                    r--;
                    c--;
                }
                else if (d == 7)
                {
                    r--;
                    c++;
                }
                else if (d == 8)
                {
                    r++;
                    c++;
                }

                mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime / 4);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    r--;
                else if (d == 2)
                    c++;
                else if (d == 3)
                    r++;
                else if (d == 4)
                    c--;
                else if (d == 5)
                {
                    r--;
                    c++;
                }
                else if (d == 6)
                {
                    r++;
                    c++;
                }
                else if (d == 7)
                {
                    r++;
                    c--;
                }
                else if (d == 8)
                {
                    r--;
                    c--;
                }

                mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime / 4);
            }

            displayStart();
        }

        void solve_A_Star_Manhattan(int version)
        {
            int r;
            int c;

            int d;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_A_Star_Manhattan(version, r, c, 0, -1);

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                solveVisited++;

                grid[r, c].closed = 1;

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                if (r == stopSolve.Y && c == stopSolve.X)
                    break;

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c - 1, 4, grid[r, c].g_pathLength);

                mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                if (r > 0 && grid[r - 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF(spacer - (cell_padding * 2) - 2, 2)));

                if (c < (width - 1) && grid[r, c + 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(2, spacer - (cell_padding * 2) - 2)));

                if (r < (height - 1) && grid[r + 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer)), new SizeF(spacer - (cell_padding * 2) - 2, 2)));

                if (c > 0 && grid[r, c - 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(2, spacer - (cell_padding * 2) - 2)));

                if (r > 0 && c < (width - 1) && grid[r - 1, c].closed == 1 && grid[r - 1, c + 1].closed == 1 && grid[r, c + 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                if (r < (height - 1) && c < (width - 1) && grid[r, c + 1].closed == 1 && grid[r + 1, c + 1].closed == 1 && grid[r + 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                if (r < (height - 1) && c > 0 && grid[r + 1, c].closed == 1 && grid[r + 1, c - 1].closed == 1 && grid[r, c - 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                if (r > 0 && c > 0 && grid[r, c - 1].closed == 1 && grid[r - 1, c - 1].closed == 1 && grid[r - 1, c].opened == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) - cell_padding), new SizeF((cell_padding * 2) + 2, (cell_padding * 2) + 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if (!runningSolve) return;

            } while (true);

            displayStart();

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c--;
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime / 2);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 2)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (d == 3)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 4)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();
        }

        void solve_A_Star_Manhattan2(int version)
        {
            int r;
            int c;
            int r_prev;
            int c_prev;

            int d;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_A_Star_Manhattan(version, r, c, 0, -1);

            do
            {
                r_prev = r;
                c_prev = c;

                mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c_prev * spacer) + cell_padding + 2, start_y + ((float)r_prev * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                grid[r, c].closed = 1;

                mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if ((r == startSolve.Y && c == startSolve.X) ||
                    (r_prev == startSolve.Y && c_prev == startSolve.X))
                    displayStart();

                solveVisited++;

                if (r == stopSolve.Y && c == stopSolve.X)
                    break;

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c - 1, 4, grid[r, c].g_pathLength);

                if (!runningSolve) return;

            } while (openCells.Count > 0);

            displayStart();

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c--;
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime / 2);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 2)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (d == 3)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 4)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();
        }

        void solve_A_Star_Manhattan3(int version)
        {
            int r;
            int c;

            int d;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_A_Star_Manhattan(version, r, c, 0, -1);

            bool drawn;

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                solveVisited++;

                grid[r, c].closed = 1;

                if (r == stopSolve.Y && c == stopSolve.X)
                    break;

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c - 1, 4, grid[r, c].g_pathLength);

                if (grid[r, c].top == 1 && grid[r, c].right == 1 && (r > 0 && grid[r - 1, c].right == 1) && (c < (width - 1) && grid[r, c + 1].top == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding - 2, spacer - cell_padding)));

                if (grid[r, c].right == 1 && (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1) && (r < (height - 1) && c < (width - 1) && grid[r + 1, c + 1].top == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding - 2, spacer - cell_padding - 2)));

                if ((r < (height - 1) && grid[r + 1, c].top == 1) && (c > 0 && grid[r, c - 1].right == 1) && (r < (height - 1) && c > 0 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding, spacer - cell_padding - 2)));

                if (grid[r, c].top == 1 && (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1) && (r > 0 && c > 0 && grid[r - 1, c - 1].right == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding, spacer - cell_padding)));

                drawn = false;

                if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 1) &&
                    (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer + (cell_padding * 2) + 2)));
                else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    drawn = true;
                }

                if ((c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 1) &&
                    (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 1))
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer + (cell_padding * 2) + 2, spacer - (cell_padding * 2) - 2)));
                else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (!drawn)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if (!runningSolve) return;

            } while (true);

            displayStart();

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r++;
                }
                else if (d == 2)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c--;
                }
                else if (d == 3)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                    r--;
                }
                else if (d == 4)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    c++;
                }

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime / 2);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 2)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                else if (d == 3)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                else if (d == 4)
                    mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();
        }

        void solve_A_Star_Manhattan_Tracker(int version)
        {
            int r = startSolve.Y;
            int c = startSolve.X;

            int r_stop = r;
            int c_stop = c;

            int r_temp, c_temp;

            int d;

            int sleepVisited = 0;

            EvaluateCell_A_Star_Manhattan(version, r, c, 0, -1);

            visitedCellsStack.Push(new MazeCell(c, r, 0));

            Stack<MazeCell> solveCellPath = new Stack<MazeCell>();

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                {
                    ClearSolveCells(r, c);

                    visitedCellsStack.Push(new MazeCell(c, r, 0));
                }

                solveVisited++;

                grid[r, c].closed = 1;

                solveCellPath.Clear();

                r_temp = r;
                c_temp = c;

                while (!(r == r_stop && c == c_stop))
                {
                    d = grid[r, c].direction;

                    if (d == 1)
                        r++;
                    else if (d == 2)
                        c--;
                    else if (d == 3)
                        r--;
                    else if (d == 4)
                        c++;

                    solveCellPath.Push(new MazeCell(c, r, d));
                }

                foreach (MazeCell cell in solveCellPath)
                {
                    if (!runningSolve) return;

                    r = cell.row;
                    c = cell.column;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    d = cell.direction;

                    if (d == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    else if (d == 2)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    else if (d == 3)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));
                    else if (d == 4)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    if (r == startSolve.Y && c == startSolve.X)
                        displayStart();

                    visitedCellsStack.Push(cell);

                    sleepVisited++;

                    if (showSolve && (sleepVisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = r_temp;
                c = c_temp;

                if (r == stopSolve.Y && c == stopSolve.X)
                {
                    displayStart();

                    break;
                }

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c - 1, 4, grid[r, c].g_pathLength);

                bool stopSolving = true;
                bool found = false;

                r_stop = startSolve.Y;
                c_stop = startSolve.X;

                while (openCells.Count > 0)
                {
                    r = openCells.Peek().row;
                    c = openCells.Peek().column;

                    genVisited++;

                    if (grid[r, c].closed == 1)
                    {
                        openCells.Dequeue();

                        continue;
                    }

                    while (!(r == startSolve.Y && c == startSolve.X))
                    {
                        d = grid[r, c].direction;

                        if (d == 1)
                            r++;
                        else if (d == 2)
                            c--;
                        else if (d == 3)
                            r--;
                        else if (d == 4)
                            c++;

                        foreach (MazeCell cell in visitedCellsStack)
                        {
                            if (cell.row == r && cell.column == c)
                            {
                                found = true;

                                break;
                            }
                        }

                        if (found)
                        {
                            r_stop = r;
                            c_stop = c;

                            break;
                        }
                    }

                    stopSolving = false;

                    break;
                }

                if (stopSolving) return;

                r = r_temp;
                c = c_temp;

                while (!(r == r_stop && c == c_stop))
                {
                    if (!runningSolve) return;

                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                    d = grid[r, c].direction;

                    if (d == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        r++;
                    }
                    else if (d == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        c--;
                    }
                    else if (d == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        r--;
                    }
                    else if (d == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                        c++;
                    }

                    visitedCellsStack.Pop();

                    sleepVisited++;

                    if (showSolve && (sleepVisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

            } while (true);

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                d = grid[r, c].direction;

                if (d == 1)
                    r++;
                else if (d == 2)
                    c--;
                else if (d == 3)
                    r--;
                else if (d == 4)
                    c++;

                solvePathLength++;

                grid[r, c].solvepath = 1;
            }
        }

        void solve_A_Star_Manhattan_Tracker2(int version)
        {
            int r = startSolve.Y;
            int c = startSolve.X;

            int r_stop = r;
            int c_stop = c;

            int r_temp, c_temp;

            int d;

            int sleepVisited = 0;

            EvaluateCell_A_Star_Manhattan(version, r, c, 0, -1);

            visitedCellsStack.Push(new MazeCell(c, r, 0));

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                solveVisited++;

                grid[r, c].closed = 1;

                Stack<MazeCell> solveCellPath = new Stack<MazeCell>();

                r_temp = r;
                c_temp = c;

                while (!(r == r_stop && c == c_stop))
                {
                    d = grid[r, c].direction;

                    if (d == 1)
                        r++;
                    else if (d == 2)
                        c--;
                    else if (d == 3)
                        r--;
                    else if (d == 4)
                        c++;

                    solveCellPath.Push(new MazeCell(c, r, d));
                }

                foreach (MazeCell cell in solveCellPath)
                {
                    if (!runningSolve) return;

                    r = cell.row;
                    c = cell.column;

                    d = cell.direction;

                    if (d == 1)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r - 1) * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    else if (d == 2)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                    else if (d == 3)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)(r + 1) * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                    else if (d == 4)
                        mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)(c - 1) * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                    visitedCellsStack.Push(cell);

                    sleepVisited++;

                    if (showSolve && (sleepVisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

                r = r_temp;
                c = c_temp;

                if (r == stopSolve.Y && c == stopSolve.X)
                {
                    displayStart();

                    break;
                }

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Manhattan(version, r, c - 1, 4, grid[r, c].g_pathLength);

                bool stopSolving = true;
                bool found = false;

                r_stop = startSolve.Y;
                c_stop = startSolve.X;

                while (openCells.Count > 0)
                {
                    r = openCells.Peek().row;
                    c = openCells.Peek().column;

                    genVisited++;

                    if (grid[r, c].closed == 1)
                    {
                        openCells.Dequeue();

                        continue;
                    }

                    while (!(r == startSolve.Y && c == startSolve.X))
                    {
                        d = grid[r, c].direction;

                        if (d == 1)
                            r++;
                        else if (d == 2)
                            c--;
                        else if (d == 3)
                            r--;
                        else if (d == 4)
                            c++;

                        foreach (MazeCell cell in visitedCellsStack)
                        {
                            if (cell.row == r && cell.column == c)
                            {
                                found = true;

                                break;
                            }
                        }

                        if (found)
                        {
                            r_stop = r;
                            c_stop = c;

                            break;
                        }
                    }

                    stopSolving = false;

                    break;
                }

                if (stopSolving) return;

                r = r_temp;
                c = c_temp;

                while (!(r == r_stop && c == c_stop))
                {
                    if (!runningSolve) return;

                    d = grid[r, c].direction;

                    if (d == 1)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r++;
                    }
                    else if (d == 2)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c--;
                    }
                    else if (d == 3)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));

                        r--;
                    }
                    else if (d == 4)
                    {
                        mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));

                        c++;
                    }

                    visitedCellsStack.Pop();

                    sleepVisited++;

                    if (showSolve && (sleepVisited % solveSpeed) == 0)
                        Thread.Sleep(sleepTime);
                }

            } while (true);

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                d = grid[r, c].direction;

                if (d == 1)
                    r++;
                else if (d == 2)
                    c--;
                else if (d == 3)
                    r--;
                else if (d == 4)
                    c++;

                solvePathLength++;

                grid[r, c].solvepath = 1;
            }
        }

        void solve_A_Star_Chebyshev(int version)
        {
            int r;
            int c;

            int d;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_A_Star_Chebyshev(version, r, c, 0, -1);

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                solveVisited++;

                grid[r, c].closed = 1;

                mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                if (r == stopSolve.Y && c == stopSolve.X)
                    break;

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (r > 0 && c < (width - 1) &&
                    (grid[r, c].top == 1 || grid[r, c].right == 1) &&
                    (grid[r, c].top == 1 || grid[r, c + 1].top == 1) &&
                    (grid[r, c].right == 1 || grid[r - 1, c].right == 1) &&
                    (grid[r, c + 1].top == 1 || grid[r - 1, c].right == 1))
                    //grid[r - 1, c + 1].obstacle == 0 && grid[r - 1, c + 1].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r - 1, c + 1, 5, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && c < (width - 1) &&
                    (grid[r, c].right == 1 || grid[r + 1, c].top == 1) &&
                    (grid[r, c].right == 1 || grid[r + 1, c].right == 1) &&
                    (grid[r + 1, c].top == 1 || grid[r + 1, c + 1].top == 1) &&
                    (grid[r + 1, c].right == 1 || grid[r + 1, c + 1].top == 1))
                    //grid[r + 1, c + 1].obstacle == 0 && grid[r + 1, c + 1].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r + 1, c + 1, 6, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (r < (height - 1) && c > 0 &&
                    (grid[r + 1, c].top == 1 || grid[r, c - 1].right == 1) &&
                    (grid[r + 1, c].top == 1 || grid[r + 1, c - 1].top == 1) &&
                    (grid[r, c - 1].right == 1 || grid[r + 1, c - 1].right == 1) &&
                    (grid[r + 1, c - 1].top == 1 || grid[r + 1, c - 1].right == 1))
                    //grid[r + 1, c - 1].obstacle == 0 && grid[r + 1, c - 1].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r + 1, c - 1, 7, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r, c - 1, 4, grid[r, c].g_pathLength);

                if (r > 0 && c > 0 &&
                    (grid[r, c - 1].right == 1 || grid[r, c].top == 1) &&
                    (grid[r, c - 1].right == 1 || grid[r - 1, c - 1].right == 1) &&
                    (grid[r, c].top == 1 || grid[r, c - 1].top == 1) &&
                    (grid[r - 1, c - 1].right == 1 || grid[r, c - 1].top == 1))
                    //grid[r - 1, c - 1].obstacle == 0 && grid[r - 1, c - 1].closed == 0)
                    EvaluateCell_A_Star_Chebyshev(version, r - 1, c - 1, 8, grid[r, c].g_pathLength);

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if (!runningSolve) return;

            } while (true);

            displayStart();

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                    r++;
                else if (d == 2)
                    c--;
                else if (d == 3)
                    r--;
                else if (d == 4)
                    c++;
                else if (d == 5)
                {
                    r++;
                    c--;
                }
                else if (d == 6)
                {
                    r--;
                    c--;
                }
                else if (d == 7)
                {
                    r--;
                    c++;
                }
                else if (d == 8)
                {
                    r++;
                    c++;
                }

                mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    r--;
                else if (d == 2)
                    c++;
                else if (d == 3)
                    r++;
                else if (d == 4)
                    c--;
                else if (d == 5)
                {
                    r--;
                    c++;
                }
                else if (d == 6)
                {
                    r++;
                    c++;
                }
                else if (d == 7)
                {
                    r++;
                    c--;
                }
                else if (d == 8)
                {
                    r--;
                    c--;
                }

                mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();
        }

        void solve_A_Star_Octile(int version)
        {
            int r;
            int c;

            int d;

            int sleepVisited = 0;

            r = startSolve.Y;
            c = startSolve.X;

            EvaluateCell_A_Star_Octile(version, r, c, 0, -1);

            allowFillCell = true;

            do
            {
                r = openCells.Peek().row;
                c = openCells.Peek().column;

                openCells.Dequeue();

                if (resetStart)
                    ClearSolveCells(r, c);

                genVisited++;

                if (grid[r, c].closed == 1)
                    continue;

                solveVisited++;

                grid[r, c].closed = 1;

                if (r == startSolve.Y && c == startSolve.X)
                    displayStart();

                if (r == stopSolve.Y && c == stopSolve.X)
                {
                    allowFillCell = false;

                    break;
                }

                if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r - 1, c, 1, grid[r, c].g_pathLength);

                if (r > 0 && c < (width - 1) &&
                    (grid[r, c].top == 1 || grid[r, c].right == 1) &&
                    (grid[r, c].top == 1 || grid[r, c + 1].top == 1) &&
                    (grid[r, c].right == 1 || grid[r - 1, c].right == 1) &&
                    (grid[r, c + 1].top == 1 || grid[r - 1, c].right == 1))
                    //grid[r - 1, c + 1].obstacle == 0 && grid[r - 1, c + 1].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r - 1, c + 1, 5, grid[r, c].g_pathLength);

                if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r, c + 1, 2, grid[r, c].g_pathLength);

                if (r < (height - 1) && c < (width - 1) &&
                    (grid[r, c].right == 1 || grid[r + 1, c].top == 1) &&
                    (grid[r, c].right == 1 || grid[r + 1, c].right == 1) &&
                    (grid[r + 1, c].top == 1 || grid[r + 1, c + 1].top == 1) &&
                    (grid[r + 1, c].right == 1 || grid[r + 1, c + 1].top == 1))
                    //grid[r + 1, c + 1].obstacle == 0 && grid[r + 1, c + 1].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r + 1, c + 1, 6, grid[r, c].g_pathLength);

                if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r + 1, c, 3, grid[r, c].g_pathLength);

                if (r < (height - 1) && c > 0 &&
                    (grid[r + 1, c].top == 1 || grid[r, c - 1].right == 1) &&
                    (grid[r + 1, c].top == 1 || grid[r + 1, c - 1].top == 1) &&
                    (grid[r, c - 1].right == 1 || grid[r + 1, c - 1].right == 1) &&
                    (grid[r + 1, c - 1].top == 1 || grid[r + 1, c - 1].right == 1))
                    //grid[r + 1, c - 1].obstacle == 0 && grid[r + 1, c - 1].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r + 1, c - 1, 7, grid[r, c].g_pathLength);

                if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r, c - 1, 4, grid[r, c].g_pathLength);

                if (r > 0 && c > 0 &&
                    (grid[r, c - 1].right == 1 || grid[r, c].top == 1) &&
                    (grid[r, c - 1].right == 1 || grid[r - 1, c - 1].right == 1) &&
                    (grid[r, c].top == 1 || grid[r, c - 1].top == 1) &&
                    (grid[r - 1, c - 1].right == 1 || grid[r, c - 1].top == 1))
                    //grid[r - 1, c - 1].obstacle == 0 && grid[r - 1, c - 1].closed == 0)
                    EvaluateCell_A_Star_Octile(version, r - 1, c - 1, 8, grid[r, c].g_pathLength);

                //if (grid[r, c].top == 1 && grid[r, c].right == 1 && (r > 0 && grid[r - 1, c].right == 1) && (c < (width - 1) && grid[r, c + 1].top == 1))
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding - 2, spacer - cell_padding)));

                //if (grid[r, c].right == 1 && (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].right == 1) && (r < (height - 1) && c < (width - 1) && grid[r + 1, c + 1].top == 1))
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding - 2, spacer - cell_padding - 2)));

                //if ((r < (height - 1) && grid[r + 1, c].top == 1) && (c > 0 && grid[r, c - 1].right == 1) && (r < (height - 1) && c > 0 && grid[r + 1, c - 1].top == 1 && grid[r + 1, c - 1].right == 1))
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - cell_padding, spacer - cell_padding - 2)));

                //if (grid[r, c].top == 1 && (c > 0 && grid[r, c - 1].top == 1 && grid[r, c - 1].right == 1) && (r > 0 && c > 0 && grid[r - 1, c - 1].right == 1))
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer - cell_padding, spacer - cell_padding)));

                //drawn = false;

                //if ((r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 1) &&
                //    (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 1))
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer + (cell_padding * 2) + 2)));
                //else if (r > 0 && grid[r, c].top == 1 && grid[r - 1, c].closed == 1)
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) - cell_padding), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                //else if (r < (height - 1) && grid[r + 1, c].top == 1 && grid[r + 1, c].closed == 1)
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer)));
                //else
                //{
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                //    drawn = true;
                //}

                //if ((c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 1) &&
                //    (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 1))
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer + (cell_padding * 2) + 2, spacer - (cell_padding * 2) - 2)));
                //else if (c < (width - 1) && grid[r, c].right == 1 && grid[r, c + 1].closed == 1)
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                //else if (c > 0 && grid[r, c - 1].right == 1 && grid[r, c - 1].closed == 1)
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) - cell_padding, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer, spacer - (cell_padding * 2) - 2)));
                //else if (!drawn)
                //    mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                mazeCanvas.FillRectangle(new SolidBrush(colorSolvePath), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);

                if (!runningSolve) return;

            } while (true);

            displayStart();

            solvePathLength = 1;

            grid[r, c].solvepath = 1;

            while (!(r == startSolve.Y && c == startSolve.X))
            {
                if (!runningSolve) return;

                d = grid[r, c].direction;

                if (d == 1)
                    r++;
                else if (d == 2)
                    c--;
                else if (d == 3)
                    r--;
                else if (d == 4)
                    c++;
                else if (d == 5)
                {
                    r++;
                    c--;
                }
                else if (d == 6)
                {
                    r--;
                    c--;
                }
                else if (d == 7)
                {
                    r--;
                    c++;
                }
                else if (d == 8)
                {
                    r++;
                    c++;
                }

                mazeCanvas.FillRectangle(new SolidBrush(Color.DeepSkyBlue), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                solvePathLength++;

                grid[r, c].solvepath = 1;

                visitedCellsStack.Push(new MazeCell(c, r, d));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();

            foreach (MazeCell cell in visitedCellsStack)
            {
                if (!runningSolve) return;

                r = cell.row;
                c = cell.column;

                d = cell.direction;

                if (d == 1)
                    r--;
                else if (d == 2)
                    c++;
                else if (d == 3)
                    r++;
                else if (d == 4)
                    c--;
                else if (d == 5)
                {
                    r--;
                    c++;
                }
                else if (d == 6)
                {
                    r++;
                    c++;
                }
                else if (d == 7)
                {
                    r++;
                    c--;
                }
                else if (d == 8)
                {
                    r--;
                    c--;
                }

                mazeCanvas.FillRectangle(new SolidBrush(colorSolve), new RectangleF(new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2), new SizeF(spacer - (cell_padding * 2) - 2, spacer - (cell_padding * 2) - 2)));

                sleepVisited++;

                if (showSolve && (sleepVisited % solveSpeed) == 0)
                    Thread.Sleep(sleepTime);
            }

            displayStart();
        }

        public void GenerateGrid()
        {
            initializeGrid();

            if (genAlgorithmType == 0)
            {
                displayGrid();
            }
            else if (genAlgorithmType == 1)
            {
                displayOutline();
            }
            else
            {
                displayBlock();
            }

            if (inverseColors)
            {
                colorCurrentCell = Color.Red;
                colorGenerate = Color.Pink;
            }
            else
            {
                colorCurrentCell = Color.Blue;
                colorGenerate = Color.LightBlue;
            }

            displayGridFlag = false;
        }

        public void GenerateMaze()
        {
            resetGen = false;
            runningGen = true;

            if (displayGridFlag)
                GenerateGrid();

            stopwatch.Reset();

            stopwatch.Start();

            if (genAlgorithmName == "Recursive Backtracker")
            {
                if (!forceTurns)
                {
                    if (genAlgorithmType == 0)
                    {
                        generateRBStraight_RemoveWalls();
                    }
                    else if (genAlgorithmType == 1)
                    {
                        generateRBStraight_BuildWalls();
                    }
                    else
                    {
                        generateRBStraight_FillCells();
                    }
                }
                else
                {
                    if (genAlgorithmType == 0)
                    {
                        generateRBJagged_RemoveWalls();
                    }
                    else if (genAlgorithmType == 1)
                    {
                        generateRBJagged_BuildWalls();
                    }
                    else
                    {
                        generateRBJagged_FillCells();
                    }
                }
            }
            else if (genAlgorithmName == "Hunt and Kill")
            {
                if (!forceTurns)
                {
                    if (genAlgorithmType == 0)
                    {
                        generateHKStraight_RemoveWalls_DFS_Iteration_Stack();
                    }
                    else if (genAlgorithmType == 1)
                    {
                        generateHKStraight_BuildWalls_DFS_Iteration_Stack();
                    }
                    else
                    {
                        generateHKStraight_FillCells_DFS_Iteration_Stack();
                    }
                }
                else
                {
                    if (genAlgorithmType == 0)
                    {
                        generateHKJagged_RemoveWalls_DFS_Iteration_Stack();
                    }
                    else if (genAlgorithmType == 1)
                    {
                        generateHKJagged_BuildWalls_DFS_Iteration_Stack();
                    }
                    else
                    {
                        generateHKJagged_FillCells_DFS_Iteration_Stack();
                    }
                }
            }
            else if (genAlgorithmName == "Prim's Algorithm")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                if (genAlgorithmType == 0)
                {
                    generatePrim_RemoveWalls();
                }
                else if (genAlgorithmType == 1)
                {
                    generatePrim_BuildWalls();
                }
                else
                {
                    generatePrim_FillCells();
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (genAlgorithmName == "Kruskal's Algorithm")
            {
                if (genAlgorithmType == 0)
                {
                    generateKruskal_RemoveWalls();
                }
                else if (genAlgorithmType == 1)
                {
                    generateKruskal_BuildWalls();
                }
                else
                {
                    generateKruskal_FillCells();
                }
            }

            allowRooms = true;
            allowLoops = true;
            allowObstacles = true;
            allowIslands = false;

            if (allowRooms)
                generateRooms(100);

            if (allowLoops)
                generateLoops(0);

            if (allowObstacles)
                generateObstacles(30);

            if (allowIslands)
                generateIslands(0);

            stopwatch.Stop();

            if (runningGen)
                displayStart();

            //mazeCanvas.DrawString(stopwatch.ElapsedMilliseconds.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(50, 10));
            //mazeCanvas.DrawString(cell_padding.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(150, 10));
            //mazeCanvas.DrawString((spacer - (cell_padding * 2) - 2).ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(450, 10));
            //mazeCanvas.DrawString(spacer.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(200, 10));

            if (visitedCellsStack.Count > 0)
                visitedCellsStack.Clear();

            if (visitedCellsQueue.Count > 0)
                visitedCellsQueue.Clear();

            displayGridFlag = true;

            runningGen = false;
            resetGen = true;
        }

        public void ResetFill()
        {
            if (r_current != -1 || c_current != -1)
                grid[r_current, c_current].filled = 0;
        }

        public void FillCircle(float centerX, float centerY, float radius, Color color)
        {
            mazeCanvas.FillEllipse(new SolidBrush(color), centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        public void FillCell(int x, int y)
        {
            if (allowFillCell)
            {
                int r = (int)(((float)y - start_y) / spacer);
                int c = (int)(((float)x - start_x) / spacer);

                //FillCircle(x, y, 10);

                //return;

                if (!(r >= 0 && r <= (height - 1) && c >= 0 && c <= (width - 1)))
                    return;

                if (r != r_current || c != c_current)
                {
                    if (r_current != -1 || c_current != -1)
                        grid[r_current, c_current].filled = 0;

                    r_current = r;
                    c_current = c;
                }

                if (grid[r, c].filled == 1)
                    return;

                grid[r, c].filled = 1;

                if (grid[r, c].obstacle == 0)
                {
                    //mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

                    //FillCircle(start_x + (c * spacer) + ((spacer - 2) / 2) + 2, start_y + (r * spacer) + ((spacer - 2) / 2) + 2, (spacer - 2) / 2, colorForeground);

                    //grid[r, c].obstacle = 1;

                    //grid[r, c].top = 0;
                    //grid[r, c].right = 0;

                    //if (r < (height - 1))
                    //    grid[r + 1, c].top = 0;

                    //if (c > 0)
                    //    grid[r, c - 1].right = 0;

                    resetStart = true;
                }
                else
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

                    //FillCircle(start_x + (c * spacer) + ((spacer - 2) / 2) + 2, start_y + (r * spacer) + ((spacer - 2) / 2) + 2, (spacer - 2) / 2, colorBackground);

                    grid[r, c].obstacle = 0;

                    if (r > 0 && grid[r - 1, c].obstacle == 0)
                        grid[r, c].top = 1;

                    if (c < (width - 1) && grid[r, c + 1].obstacle == 0)
                        grid[r, c].right = 1;

                    if (r < (height - 1) && grid[r + 1, c].obstacle == 0)
                        grid[r + 1, c].top = 1;

                    if (c > 0 && grid[r, c - 1].obstacle == 0)
                        grid[r, c - 1].right = 1;

                    resetStart = true;
                }
            }
            else
            {
                int r = (int)(((float)y - start_y) / spacer);
                int c = (int)(((float)x - start_x) / spacer);

                //FillCircle(x, y, 10);

                //return;

                if (!(r >= 0 && r <= (height - 1) && c >= 0 && c <= (width - 1)))
                    return;

                if (r != r_current || c != c_current)
                {
                    if (r_current != -1 || c_current != -1)
                        grid[r_current, c_current].filled = 0;

                    r_current = r;
                    c_current = c;
                }

                if (grid[r, c].filled == 1)
                    return;

                grid[r, c].filled = 1;

                if (grid[r, c].obstacle == 0)
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

                    //FillCircle(start_x + (c * spacer) + ((spacer - 2) / 2) + 2, start_y + (r * spacer) + ((spacer - 2) / 2) + 2, (spacer - 2) / 2, colorForeground);

                    grid[r, c].obstacle = 1;

                    grid[r, c].top = 0;
                    grid[r, c].right = 0;

                    if (r < (height - 1))
                        grid[r + 1, c].top = 0;

                    if (c > 0)
                        grid[r, c - 1].right = 0;
                }
                else
                {
                    mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));

                    //FillCircle(start_x + (c * spacer) + ((spacer - 2) / 2) + 2, start_y + (r * spacer) + ((spacer - 2) / 2) + 2, (spacer - 2) / 2, colorBackground);

                    grid[r, c].obstacle = 0;

                    if (r > 0 && grid[r - 1, c].obstacle == 0)
                        grid[r, c].top = 1;

                    if (c < (width - 1) && grid[r, c + 1].obstacle == 0)
                        grid[r, c].right = 1;

                    if (r < (height - 1) && grid[r + 1, c].obstacle == 0)
                        grid[r + 1, c].top = 1;

                    if (c > 0 && grid[r, c - 1].obstacle == 0)
                        grid[r, c - 1].right = 1;
                }
            }
        }

        void ClearSolveCells(int r_start, int c_start)
        {
            int r = r_current;
            int c = c_current;

            stopSolve = new Point(c, r);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(stopGen, new SizeF(spacer - (cell_padding * 2) - wall_thickness, spacer - (cell_padding * 2) - wall_thickness)));

            stopGen = new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2);

            mazeCanvas.FillRectangle(new SolidBrush(colorStop), new RectangleF(stopGen, new SizeF(spacer - (cell_padding * 2) - wall_thickness, spacer - (cell_padding * 2) - wall_thickness)));

            for (r = 0; r < height; r++)
            {
                for (c = 0; c < width; c++)
                {
                    grid[r, c].direction = 0;
                    grid[r, c].opened = 0;
                    grid[r, c].closed = 0;
                    grid[r, c].g_pathLength = 0;
                }
            }

            openCells.Clear();
            visitedCellsStack.Clear();

            visited = 0;
            solveVisited = 0;
            genVisited = 0;

            r = r_start;
            c = c_start;
            
            startSolve = new Point(c, r);

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(startGen, new SizeF(spacer - (cell_padding * 2) - wall_thickness, spacer - (cell_padding * 2) - wall_thickness)));

            startGen = new PointF(start_x + ((float)c * spacer) + cell_padding + 2, start_y + ((float)r * spacer) + cell_padding + 2);

            mazeCanvas.FillRectangle(new SolidBrush(colorStart), new RectangleF(startGen, new SizeF(spacer - (cell_padding * 2) - wall_thickness, spacer - (cell_padding * 2) - wall_thickness)));

            resetStart = false;
        }

        void ClearSolve()
        {
            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(start_x + 2, start_y + 2), new SizeF((spacer * width) - 2, (spacer * height) - 2)));

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    grid[r, c].direction = 0;
                    grid[r, c].opened = 0;
                    grid[r, c].closed = 0;
                    grid[r, c].solvepath = 0;
                    grid[r, c].solution = 0;
                    grid[r, c].f_cost = 0;
                    grid[r, c].g_pathLength = 0;
                    grid[r, c].h_manhattanDistance = 0;
                    grid[r, c].h_euclideanDistance = 0;

                    if (allowObstacles)
                    {
                        if (grid[r, c].obstacle == 1)
                            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer) + 2, start_y + ((float)r * spacer) + 2), new SizeF(spacer - 2, spacer - 2)));
                        //FillCircle(start_x + (c * spacer) + ((spacer - 2) / 2) + 2, start_y + (r * spacer) + ((spacer - 2) / 2) + 2, (spacer - 2) / 2, colorForeground);
                    }
                    else
                    {
                        if (grid[r, c].top == 0)
                            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)c * spacer), start_y + ((float)r * spacer)), new SizeF(spacer + 2, 2)));

                        if (grid[r, c].right == 0)
                            mazeCanvas.FillRectangle(new SolidBrush(colorForeground), new RectangleF(new PointF(start_x + ((float)(c + 1) * spacer), start_y + ((float)r * spacer)), new SizeF(2, spacer + 2)));

                    }
                }
            }

            openCells.Clear();
            visitedCellsStack.Clear();

            visited = 0;
            solveVisited = 0;
            genVisited = 0;

            displayStart();
        }

        public void SolveMaze()
        {
            resetSolve = false;
            runningSolve = true;

            System.IO.Directory.CreateDirectory("Logs");

            string path = @"Logs\log.txt";

            if (!File.Exists(path))
                File.Create(path).Dispose();

            ClearSolve();

            stopwatch.Reset();

            stopwatch.Start();

            if (solveAlgorithmName == "Depth-First Search")
            {
                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_DFS_Iteration();

                    tw.WriteLine("Depth-First Search\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }
            }
            else if (solveAlgorithmName == "Breadth-First Search")
            {
                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_BFS_Iteration();

                    tw.WriteLine("Breadth-First Search\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }
            }
            else if (solveAlgorithmName == "Dijkstra's Algorithm")
            {
                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_Dijkstra();

                    tw.WriteLine("Dijkstra\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }
            }
            else if (solveAlgorithmName == "Greedy Best-First Search")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_GBFS();

                    tw.WriteLine("GBFS\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Manhattan distance)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    if (!showTracer)
                        solve_A_Star_Manhattan(1);
                    else
                        solve_A_Star_Manhattan_Tracker(1);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Manhattan 2)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    if (!showTracer)
                        solve_A_Star_Manhattan(2);
                    else
                        solve_A_Star_Manhattan_Tracker(2);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Manhattan 3)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    if (!showTracer)
                        solve_A_Star_Manhattan(3);
                    else
                        solve_A_Star_Manhattan_Tracker(3);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Manhattan 4)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    if (!showTracer)
                        solve_A_Star_Manhattan(4);
                    else
                        solve_A_Star_Manhattan_Tracker(4);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Chebyshev distance)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Chebyshev(1);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Chebyshev 2)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Chebyshev(2);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Chebyshev 3)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Chebyshev(3);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Chebyshev 4)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Chebyshev(4);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Octile distance)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Octile(1);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Octile 2)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Octile(2);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Octile 3)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Octile(3);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }
            else if (solveAlgorithmName == "A*  (Octile 4)")
            {
                float cell_padding_temp = cell_padding;

                cell_padding = 0;

                findStart();

                using (TextWriter tw = new StreamWriter(path, true))
                {
                    solve_A_Star_Octile(4);

                    tw.WriteLine(solveAlgorithmName + "\n" + solvePathLength.ToString() + "," + solveVisited.ToString() + "," + genVisited.ToString() + "," + visited.ToString() + "\n");
                }

                cell_padding = cell_padding_temp;

                findStart();
            }

            stopwatch.Stop();

            mazeCanvas.FillRectangle(new SolidBrush(colorBackground), new RectangleF(new PointF(650, 5), new SizeF(400, 30)));

            //mazeCanvas.DrawString(solvePathLength.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(650, 5));
            //mazeCanvas.DrawString(solveVisited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(725, 5));
            //mazeCanvas.DrawString(genVisited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(800, 5));
            //mazeCanvas.DrawString(visited.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(875, 5));
            
            //mazeCanvas.DrawString(stopwatch.ElapsedMilliseconds.ToString(), new Font("Arial", 12), new SolidBrush(colorForeground), new PointF(950, 5));

            if (visitedCellsStack.Count > 0)
                visitedCellsStack.Clear();

            if (visitedCellsQueue.Count > 0)
                visitedCellsQueue.Clear();

            if (openCells.Count > 0)
                openCells.Clear();

            if (!runningSolve)
            {
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        grid[i, j].solution = 0;
            }

            runningSolve = false;
            resetSolve = true;
        }
    }
}