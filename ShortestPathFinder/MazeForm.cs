using System.Reflection;
using System.Runtime.InteropServices;

namespace ShortestPathFinder
{
    public partial class MazeForm : Form
    {
        Maze maze;

        bool drawGrid;
        bool mouseDown;

        float padding_x_maze;
        float padding_y_maze;

        public MazeForm()
        {
            InitializeComponent();

            maze = new Maze(this.panel1.CreateGraphics(), this.BackColor);
        }

        private void MazeForm_Load(object sender, EventArgs e)
        {
            //int form_x_padding = 200, form_y_padding = 100;

            //if (Screen.PrimaryScreen != null)
            //{
            //    form_x_padding = (Screen.PrimaryScreen.Bounds.Width - this.Width) / 2;
            //    form_y_padding = (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2;
            //}

            //this.Location = new Point(form_x_padding - 10, form_y_padding - 24);

            this.WindowState = FormWindowState.Maximized;

            this.panel1.Location = new Point(0, 0);
            this.panel2.Width = 223;

            maze = new Maze(this.panel1.CreateGraphics(), this.BackColor);

            txtHeight.Text = maze.getHeight().ToString();
            txtWidth.Text = maze.getWidth().ToString();

            cboxGenAlgorithm.SelectedIndex = 0;
            cboxSolveAlgorithm.SelectedIndex = 4;

            tbarGenerateSpeed.Minimum = 0;
            tbarGenerateSpeed.Maximum = 50;
            tbarGenerateSpeed.TickFrequency = 5;

            tbarSolveSpeed.Minimum = 0;
            tbarSolveSpeed.Maximum = 50;
            tbarSolveSpeed.TickFrequency = 5;

            tbarGenerateSpeed.Value = maze.getGenSpeed();
            tbarSolveSpeed.Value = maze.getSolveSpeed();

            chkForceTurns.Checked = maze.getForceTurns();
            chkShowGen.Checked = maze.getShowGen();
            chkShowTracer.Checked = maze.getShowTracer();
            chkShowBacktracks.Checked = maze.getShowBacktracks();
            chkShowSolve.Checked = maze.getShowSolve();
            chkInverseColors.Checked = maze.getInverseColors();

            initializeMaze();

            mouseDown = false;
            drawGrid = true;
        }

        private void MazeForm_Resize(object sender, EventArgs e)
        {
            initializeMaze();

            maze.GenerateGrid();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (drawGrid)
            {
                drawGrid = false;

                maze.GenerateGrid();
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            if (drawGrid)
            {
                drawGrid = false;

                maze.GenerateGrid();
            }
        }

        private void initializeMaze()
        {
            padding_x_maze = 50F;
            padding_y_maze = 50F;

            float maze_side = 0;

            float padding_x_panel2 = 0;
            float padding_y_panel2 = 0;

            this.panel1.Height = this.Height;
            this.panel1.Width = this.Width;

            if (panel1.Height >= (panel1.Width - panel2.Width - 50))
            {
                maze_side = (float)(panel1.Width - panel2.Width - 150);

                padding_x_panel2 = (float)(panel1.Width - panel2.Width - 50 - maze_side) / 2F;

                if (maze_side < 0)
                    padding_x_panel2 = (float)(panel1.Width - panel2.Width - 50) / 2F;

                if (padding_x_panel2 < 18F)
                    padding_x_panel2 = 18F;

                if (panel1.Width < (panel2.Width + 18))
                    padding_x_panel2 = panel1.Width - panel2.Width;

                padding_x_maze = padding_x_panel2;

                padding_y_panel2 = (float)(panel1.Height - panel2.Height) / 2F;

                if (padding_y_panel2 < 0F)
                    padding_y_panel2 = 0F;

                padding_y_maze = ((float)panel1.Height - maze_side) / 2F;

                if (padding_y_maze < 50F)
                    padding_y_maze = 50F;
            }
            else
            {
                maze_side = (float)panel1.Height - 100F;

                padding_x_panel2 = (float)(panel1.Width - panel2.Width - 50 - maze_side) / 2F;

                if (maze_side < 0)
                    padding_x_panel2 = (float)(panel1.Width - panel2.Width - 50) / 2F;

                if (padding_x_panel2 < 18F)
                    padding_x_panel2 = 18F;

                if (panel1.Width < (panel2.Width + 18))
                    padding_x_panel2 = panel1.Width - panel2.Width;

                padding_x_maze = padding_x_panel2;

                //if (maze_side < 0)
                //    maze_side = panel1.Height;

                padding_y_panel2 = (float)(panel1.Height - panel2.Height) / 2F;

                if (padding_y_panel2 < 0F)
                    padding_y_panel2 = 0F;

                padding_y_maze = ((float)panel1.Height - maze_side) / 2F;

                if (padding_y_maze < 50F)
                    padding_y_maze = 50F;
            }

            panel2.Location = new Point(panel1.Width - panel2.Width - (int)padding_x_panel2, (int)padding_y_panel2);

            maze.setGraphics(this.panel1.CreateGraphics());
            maze.setLocation(padding_x_maze, padding_y_maze, maze_side);
        }

        private void initializeMaze2()
        {
            padding_x_maze = 50F;
            padding_y_maze = 50F;

            float maze_side = 0;

            float padding_x_panel2 = 0;
            float padding_y_panel2 = 0;

            this.panel1.Height = this.Height;
            this.panel1.Width = this.Width;

            if (panel1.Height >= (panel1.Width - panel2.Width - 50))
            {
                maze_side = (float)(panel1.Width - panel2.Width - 198);

                if (maze_side < 0)
                    maze_side = panel1.Width - panel2.Width;

                padding_x_panel2 = (float)(panel1.Width - panel2.Width - 50 - maze_side) / 2F;
                padding_x_maze = padding_x_panel2;

                if (panel1.Width < panel2.Width)
                {
                    padding_x_panel2 = -67F;
                }

                padding_y_panel2 = (float)(panel1.Height - panel2.Height) / 2F;

                if (padding_y_panel2 < 24F)
                    padding_y_panel2 = 24F;

                padding_y_maze = ((float)panel1.Height - maze_side) / 2F;

                if (padding_y_maze < 74F)
                    padding_y_maze = 74F;
            }
            else
            {
                padding_y_panel2 = (float)(panel1.Height - panel2.Height) / 2F;

                if (padding_y_panel2 < 24F)
                    padding_y_panel2 = 24F;

                maze_side = (float)panel1.Height - 148F;

                if (maze_side < 0)
                    maze_side = panel1.Height;

                padding_y_maze = ((float)panel1.Height - maze_side) / 2F;

                padding_x_panel2 = (float)(panel1.Width - panel2.Width - 50 - maze_side) / 2F;
                padding_x_maze = padding_x_panel2;

                if (panel1.Width < panel2.Width)
                {
                    padding_x_panel2 = -67F;
                }
            }

            panel2.Location = new Point(panel1.Width - panel2.Width - (int)padding_x_panel2 - 10, (int)padding_y_panel2 - 24);

            maze.setGraphics(this.panel1.CreateGraphics());
            maze.setLocation(padding_x_maze - 10, padding_y_maze - 24, maze_side);
        }

        private void GenerateMaze()
        {
            if (maze.runningGen == true)
            {
                maze.runningGen = false;

                while (!maze.resetGen) ;
            }

            maze.GenerateMaze();
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            await Task.Run(() => GenerateMaze());
        }

        private void txtHeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                e.SuppressKeyPress = true;
        }

        private void txtHeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                maze.setHeight(Int32.Parse(txtHeight.Text));

                maze.GenerateGrid();
            }
            catch (Exception)
            {
                txtHeight.Text = maze.getHeight().ToString();

                txtHeight.Select(txtHeight.Text.Length, 0);
            }
        }

        private void txtHeight_MouseClick(object sender, MouseEventArgs e)
        {
            txtHeight.SelectAll();
        }

        private void btnDecreaseHeight_MouseDown(object sender, MouseEventArgs e)
        {
            maze.setHeight(maze.getHeight() - 1);
            txtHeight.Text = maze.getHeight().ToString();

            drawGrid = true;
        }

        private void btnIncreaseHeight_MouseDown(object sender, MouseEventArgs e)
        {
            maze.setHeight(maze.getHeight() + 1);
            txtHeight.Text = maze.getHeight().ToString();

            drawGrid = true;
        }

        private void txtWidth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
                e.SuppressKeyPress = true;
        }

        private void txtWidth_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                maze.setWidth(Int32.Parse(txtWidth.Text));

                maze.GenerateGrid();
            }
            catch (Exception)
            {
                txtWidth.Text = maze.getWidth().ToString();

                txtWidth.Select(txtWidth.Text.Length, 0);
            }
        }

        private void txtWidth_MouseClick(object sender, MouseEventArgs e)
        {
            txtWidth.SelectAll();
        }

        private void btnDecreaseWidth_MouseDown(object sender, MouseEventArgs e)
        {
            maze.setWidth(maze.getWidth() - 1);
            txtWidth.Text = maze.getWidth().ToString();

            drawGrid = true;
        }

        private void btnIncreaseWidth_MouseDown(object sender, MouseEventArgs e)
        {
            maze.setWidth(maze.getWidth() + 1);
            txtWidth.Text = maze.getWidth().ToString();

            drawGrid = true;
        }

        private void cboxGenAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            maze.setGenAlgorithmName(cboxGenAlgorithm.Items[cboxGenAlgorithm.SelectedIndex].ToString() ?? "Recursive Backtracker");
        }

        private void rbgrpAlgorithmType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;

            if (rb != null)
            {
                if (rb.Equals(rbtnRemoveWalls))
                {
                    if (rbtnRemoveWalls.Checked)
                    {
                        maze.setGenAlgorithmType(0);
                    }
                }
                else if (rb.Equals(rbtnBuildWalls))
                {
                    if (rbtnBuildWalls.Checked)
                    {
                        maze.setGenAlgorithmType(1);
                    }
                }
                else if (rb.Equals(rbtnFillCells))
                {
                    if (rbtnFillCells.Checked)
                    {
                        maze.setGenAlgorithmType(2);
                    }
                }
            }
        }

        private void chkForceTurns_CheckedChanged(object sender, EventArgs e)
        {
            maze.setForceTurns(chkForceTurns.Checked);
        }

        private void chkShowGen_CheckedChanged(object sender, EventArgs e)
        {
            maze.setShowGen(chkShowGen.Checked);
        }

        private void tbarGenSpeed_Scroll(object sender, EventArgs e)
        {
            maze.setGenSpeed(tbarGenerateSpeed.Value);
        }

        private void SolveMaze()
        {
            if (maze.runningSolve == true)
            {
                maze.runningSolve = false;

                while (!maze.resetSolve) ;
            }

            try
            {
                maze.SolveMaze();
            }
            catch
            {
                this.panel1.CreateGraphics().DrawString("No path possible.  Please generate new grid", new Font("Arial", 12), new SolidBrush(Color.Red), new PointF(padding_x_maze - 5, padding_y_maze - 35));
            }
        }

        private async void btnSolve_Click(object sender, EventArgs e)
        {
            await Task.Run(() => SolveMaze());
        }

        private void cboxSolveAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            maze.setSolveAlgorithmName(cboxSolveAlgorithm.Items[cboxSolveAlgorithm.SelectedIndex].ToString() ?? "A* (Manhattan 1)");
        }

        private void chkShowTracer_CheckedChanged(object sender, EventArgs e)
        {
            maze.setShowTracer(chkShowTracer.Checked);
        }

        private void chkShowBacktracks_CheckedChanged(object sender, EventArgs e)
        {
            maze.setShowBacktracks(chkShowBacktracks.Checked);
        }

        private void chkShowSolve_CheckedChanged(object sender, EventArgs e)
        {
            maze.setShowSolve(chkShowSolve.Checked);
        }

        private void tbarSolveSpeed_Scroll(object sender, EventArgs e)
        {
            maze.setSolveSpeed(tbarSolveSpeed.Value);
        }

        private void chkInverseColors_CheckedChanged(object sender, EventArgs e)
        {
            maze.setInverseColors(chkInverseColors.Checked);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;

            maze.FillCell(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
                maze.FillCell(e.X, e.Y);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;

            maze.ResetFill();
        }
    }
}