using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WorldMapStudio
{
    public partial class MainForm : Form
    {
        const int _width = 128;
        const int _height = 80;
        const int _multiplier = 12;
        const int _zoomedInMultiplier = 60;
        const int _heightAdjuster = 70;

        const int _darkBlueCount = 1879;
        const int _blueCount = 1607;
        const int _darkGreenCount = 601;
        const int _greenCount = 1060;
        const int _pinkCount = 601;
        const int _orangeCount = 601;
        const int _yellowCount = 599;
        const int _sandCount = 725;
        const int _shadowCount = 393;

        static Color _land = Color.FromArgb(254, 254, 254);
        static Color _erase = Color.FromArgb(1, 1, 1);
        static Color _grid = Color.FromArgb(40, 40, 40);

        static Color _darkBlue = Color.FromArgb(0, 157, 150);
        static Color _blue = Color.FromArgb(83, 226, 255);
        static Color _darkGreen = Color.FromArgb(0, 167, 0);
        static Color _green = Color.FromArgb(143, 194, 0);
        static Color _pink = Color.FromArgb(255, 88, 105);
        static Color _orange = Color.FromArgb(253, 112, 0);
        static Color _yellow = Color.FromArgb(255, 179, 0);
        static Color _sand = Color.FromArgb(255, 215, 159);
        static Color _shadow = Color.FromArgb(0, 38, 74);

        static Color _color = _darkBlue;
        static Color[,] _canvas = new Color[_width, _height];
        static int[] _zoomedInSection = new int[] { 0, 0 };

        static Dictionary<Color, int> _counts;
        static Dictionary<Color, Label> _lables;

        static Mode _mode = Mode.None;
        static bool _zoomedIn = false;

        private enum Mode
        {
            None,
            Draw,
            Erase,
            Land,
            Zooming
        }

        public MainForm()
        {
            InitializeComponent();
            _counts = new Dictionary<Color, int>
            {
                {_darkBlue, _darkBlueCount },
                {_blue, _blueCount },
                {_darkGreen, _darkGreenCount },
                {_green, _greenCount },
                {_pink, _pinkCount },
                {_orange, _orangeCount },
                {_yellow, _yellowCount },
                {_sand, _sandCount},
                {_shadow, _shadowCount}
            };
            _lables = new Dictionary<Color, Label>
            {
                {_darkBlue, lblDarkBlue },
                {_blue, lblBlue },
                {_darkGreen, lblDarkGreen },
                {_green, lblGreen },
                {_pink, lblPink },
                {_orange, lblOrange },
                {_yellow, lblYellow },
                { _sand, lblSand},
                { _shadow, lblShadow}
            };

            ResetCanvas();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Width = _width * _multiplier + 17;
            Height = _height * _multiplier + 85;

            lblDarkBlue.ForeColor = _darkBlue;
            lbldb.ForeColor = _darkBlue;
            lblDarkBlue.Text = _darkBlueCount.ToString();
            lblDarkBlue.Location = new Point(lblDarkBlue.Location.X, Height - _heightAdjuster);
            lbldb.Location = new Point(lbldb.Location.X, Height - _heightAdjuster);

            lblBlue.ForeColor = _blue;
            lblb.ForeColor = _blue;
            lblBlue.Text = _blueCount.ToString();
            lblBlue.Location = new Point(lblBlue.Location.X, Height - _heightAdjuster);
            lblb.Location = new Point(lblb.Location.X, Height - _heightAdjuster);

            lblDarkGreen.ForeColor = _darkGreen;
            lbldg.ForeColor = _darkGreen;
            lblDarkGreen.Text = _darkGreenCount.ToString();
            lblDarkGreen.Location = new Point(lblDarkGreen.Location.X, Height - _heightAdjuster);
            lbldg.Location = new Point(lbldg.Location.X, Height - _heightAdjuster);

            lblGreen.ForeColor = _green;
            lblg.ForeColor = _green;
            lblGreen.Text = _greenCount.ToString();
            lblGreen.Location = new Point(lblGreen.Location.X, Height - _heightAdjuster);
            lblg.Location = new Point(lblg.Location.X, Height - _heightAdjuster);

            lblPink.ForeColor = _pink;
            lblp.ForeColor = _pink;
            lblPink.Text = _pinkCount.ToString();
            lblPink.Location = new Point(lblPink.Location.X, Height - _heightAdjuster);
            lblp.Location = new Point(lblp.Location.X, Height - _heightAdjuster);

            lblOrange.ForeColor = _orange;
            lblo.ForeColor = _orange;
            lblOrange.Text = _orangeCount.ToString();
            lblOrange.Location = new Point(lblOrange.Location.X, Height - _heightAdjuster);
            lblo.Location = new Point(lblo.Location.X, Height - _heightAdjuster);

            lblYellow.ForeColor = _yellow;
            lbly.ForeColor = _yellow;
            lblYellow.Text = _yellowCount.ToString();
            lblYellow.Location = new Point(lblYellow.Location.X, Height - _heightAdjuster);
            lbly.Location = new Point(lbly.Location.X, Height - _heightAdjuster);

            lblSand.ForeColor = _sand;
            lbls.ForeColor = _sand;
            lblSand.Text = _sandCount.ToString();
            lblSand.Location = new Point(lblSand.Location.X, Height - _heightAdjuster);
            lbls.Location = new Point(lbls.Location.X, Height - _heightAdjuster);

            lblDraw.Location = new Point(lblDraw.Location.X, Height - _heightAdjuster);
            lblOpen.Location = new Point(lblOpen.Location.X, Height - _heightAdjuster);
            lblSave.Location = new Point(lblSave.Location.X, Height - _heightAdjuster);
            lblLand.Location = new Point(lblLand.Location.X, Height - _heightAdjuster);
            lblErase.Location = new Point(lblErase.Location.X, Height - _heightAdjuster);
            lblZoomIn.Location = new Point(lblZoomIn.Location.X, Height - _heightAdjuster);
            lblZoomOut.Location = new Point(lblZoomOut.Location.X, Height - _heightAdjuster);
            lblStatus.Location = new Point(Width - 210, Height - _heightAdjuster);
            lblCoordinate.Location = new Point(Width - 340, Height - _heightAdjuster);
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {

            if (!_zoomedIn)
            {
                var x = e.X / _multiplier;
                var y = e.Y / _multiplier;

                if (x < 0 || x >= _width || y < 0 || y >= _height)
                    return;

                lblCoordinate.Text = $"X:{x} - Y:{y}";
                var currentColor = _canvas[x, y];

                if (_mode == Mode.Draw && _counts[_color] > 0)
                {
                    if (currentColor == _land)
                        return;
                    if (currentColor == _shadow)
                        return;

                    if (currentColor != _erase)
                    {
                        _counts[currentColor]++;
                        _lables[currentColor].Text = _counts[currentColor].ToString();
                    }

                    _canvas[x, y] = _color;
                    _counts[_color]--;
                    _lables[_color].Text = _counts[_color].ToString();

                    using (var graphics = CreateGraphics())
                    {
                        DrawCell(x, y, _color, graphics);
                        DrawGrid(graphics);
                    }
                }

                if (_mode == Mode.Erase)
                {
                    if (currentColor == _land)
                        return;
                    if (currentColor == _shadow)
                        return;

                    if (currentColor != _erase && currentColor != _land)
                    {
                        _counts[currentColor]++;
                        _lables[currentColor].Text = _counts[currentColor].ToString();
                    }

                    _canvas[x, y] = _erase;

                    using (var graphics = CreateGraphics())
                    {
                        DrawCell(x, y, _erase, graphics);
                        DrawGrid(graphics);
                    }
                }

                if (_mode == Mode.Land)
                {
                    _canvas[x, y] = _land;

                    using (var graphics = CreateGraphics())
                    {
                        DrawCell(x, y, _land, graphics);
                        DrawGrid(graphics);
                    }
                }
            }

            if (_zoomedIn)
            {
                var x = e.X / _zoomedInMultiplier;
                var y = e.Y / _zoomedInMultiplier;
                var canvasX = x + (_zoomedInSection[0] * 16);
                var canvasY = y + (_zoomedInSection[1] * 16);

                if (e.X < 0 || e.X >= 16 * _zoomedInMultiplier || e.Y < 0 || e.Y >= 16 * _zoomedInMultiplier)
                    return;

                var currentColor = _canvas[canvasX, canvasY];

                if (_mode == Mode.Draw && _counts[_color] > 0)
                {
                    if (currentColor == _land)
                        return;
                    if (currentColor == _shadow)
                        return;

                    if (currentColor != _erase)
                    {
                        _counts[currentColor]++;
                        _lables[currentColor].Text = _counts[currentColor].ToString();
                    }

                    _canvas[canvasX, canvasY] = _color;
                    _counts[_color]--;
                    _lables[_color].Text = _counts[_color].ToString();

                    using (var graphics = CreateGraphics())
                    {
                        DrawZoomedInCell(x, y, _color, graphics);
                        DrawZoomedInGrid(graphics);
                    }
                }

                if (_mode == Mode.Erase)
                {
                    if (currentColor != _erase && currentColor != _land)
                    {
                        _counts[currentColor]++;
                        _lables[currentColor].Text = _counts[currentColor].ToString();
                    }

                    _canvas[canvasX, canvasY] = _erase;

                    using (var graphics = CreateGraphics())
                    {
                        DrawZoomedInCell(x, y, _erase, graphics);
                        DrawZoomedInGrid(graphics);
                    }
                }

                if (_mode == Mode.Land)
                {
                    _canvas[canvasX, canvasY] = _land;

                    using (var graphics = CreateGraphics())
                    {
                        DrawZoomedInCell(x, y, _land, graphics);
                        DrawZoomedInGrid(graphics);
                    }
                }
            }
        }

        private void SetStatus()
        {
            switch (_mode)
            {
                case Mode.None:
                    lblStatus.Text = "";
                    break;
                case Mode.Draw:
                    lblStatus.Text = "Drawing...";
                    lblStatus.ForeColor = _color;
                    break;
                case Mode.Erase:
                    lblStatus.Text = "Erasing...";
                    lblStatus.ForeColor = Color.White;
                    break;
                case Mode.Land:
                    lblStatus.Text = "Land Mode";
                    lblStatus.ForeColor = _land;
                    break;
                case Mode.Zooming:
                    lblStatus.Text = "Double Click to Zoom";
                    lblStatus.ForeColor = _land;
                    break;
            }
        }

        private void DrawCell(int x, int y, Color color, Graphics graphics)
        {
            var brush = new SolidBrush(color);
            graphics.FillEllipse(brush, x * _multiplier, y * _multiplier, _multiplier, _multiplier);
        }

        private void DrawZoomedInCell(int x, int y, Color color, Graphics graphics)
        {
            var brush = new SolidBrush(color);
            graphics.FillEllipse(brush, x * _zoomedInMultiplier, y * _zoomedInMultiplier, _zoomedInMultiplier, _zoomedInMultiplier);
        }

        private void DrawLine(int x1, int y1, int x2, int y2, Color color, Graphics graphics)
        {
            var pen = new Pen(color, 1);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        private void DrawCanvas(Graphics graphics)
        {
            for (int x = 0; x < _canvas.GetLength(0); x++)
                for (int y = 0; y < _canvas.GetLength(1); y++)
                    DrawCell(x, y, _canvas[x, y], graphics);
        }

        private void DrawZoomedInSection(Graphics graphics)
        {
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    DrawZoomedInCell(x, y, _canvas[(_zoomedInSection[0] * 16) + x, (_zoomedInSection[1] * 16) + y], graphics);
        }

        private void ResetCanvas()
        {
            for (int x = 0; x < _canvas.GetLength(0); x++)
                for (int y = 0; y < _canvas.GetLength(1); y++)
                    _canvas[x, y] = _erase;
        }

        private void ClearScreen(Graphics graphics)
        {
            var brush = new SolidBrush(_erase);
            graphics.FillRectangle(brush, 0, 0, Width, Height);
        }

        private void DrawGrid(Graphics graphics)
        {
            for (int i = 0; i <= 8; i++)
                DrawLine(16 * i * _multiplier, 0, 16 * i * _multiplier, Height - 85, _grid, graphics);
            for (int i = 0; i <= 5; i++)
                DrawLine(0, 16 * i * _multiplier, Width, 16 * i * _multiplier, _grid, graphics);
        }

        private void DrawZoomedInGrid(Graphics graphics)
        {
            for (int i = 0; i <= 16; i++)
                DrawLine(i * _zoomedInMultiplier, 0, i * _zoomedInMultiplier, 16 * _zoomedInMultiplier, _grid, graphics);
            for (int i = 0; i <= 16; i++)
                DrawLine(0, i * _zoomedInMultiplier, 16 * _zoomedInMultiplier, i * _zoomedInMultiplier, _grid, graphics);
        }

        private void lblSave_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    sw.Write(JsonConvert.SerializeObject(_canvas));
            }
        }

        private void lblDraw_Click(object sender, EventArgs e)
        {
            _mode = _mode == Mode.Draw ? Mode.None : Mode.Draw;
            SetStatus();
        }

        private void lblErase_Click(object sender, EventArgs e)
        {
            _mode = _mode == Mode.Erase ? Mode.None : Mode.Erase;
            SetStatus();
        }

        private void lblLand_Click(object sender, EventArgs e)
        {
            _mode = _mode == Mode.Land ? Mode.None : Mode.Land;
            SetStatus();
        }

        private void lblDarkBlue_Click(object sender, EventArgs e)
        {
            _color = _darkBlue;
            SetStatus();
        }

        private void lblBlue_Click(object sender, EventArgs e)
        {
            _color = _blue;
            SetStatus();
        }

        private void lblDarkGreen_Click(object sender, EventArgs e)
        {
            _color = _darkGreen;
            SetStatus();
        }

        private void lblGreen_Click(object sender, EventArgs e)
        {
            _color = _green;
            SetStatus();
        }

        private void lblPink_Click(object sender, EventArgs e)
        {
            _color = _pink;
            SetStatus();
        }

        private void lblOrange_Click(object sender, EventArgs e)
        {
            _color = _orange;
            SetStatus();
        }

        private void lblYellow_Click(object sender, EventArgs e)
        {
            _color = _yellow;
            SetStatus();
        }

        private void lblSand_Click(object sender, EventArgs e)
        {
            _color = _sand;
            SetStatus();
        }

        private void lblZoomIn_Click(object sender, EventArgs e)
        {
            _mode = Mode.Zooming;
            SetStatus();
        }

        private void lblZoomOut_Click(object sender, EventArgs e)
        {
            _mode = Mode.None;
            _zoomedIn = false;
            SetStatus();

            using (var graphics = CreateGraphics())
            {
                ClearScreen(graphics);
                DrawGrid(graphics);
                DrawCanvas(graphics);
            }

        }

        private void MainForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_mode == Mode.Zooming)
            {
                _mode = Mode.None;
                _zoomedIn = true;
                SetStatus();

                var x = (e.X / _multiplier / 16);
                var y = (e.Y / _multiplier / 16);
                _zoomedInSection = new int[] { x, y };
                lblCoordinate.Text = "";

                using (var graphics = CreateGraphics())
                {
                    ClearScreen(graphics);
                    DrawZoomedInGrid(graphics);
                    DrawZoomedInSection(graphics);
                }
            }
        }

        private void lblOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    _canvas = JsonConvert.DeserializeObject<Color[,]>(sr.ReadToEnd());

                    using (var graphics = CreateGraphics())
                        DrawCanvas(graphics);
                }
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'D':
                    _mode = _mode == Mode.Draw ? Mode.None : Mode.Draw;
                    break;
                case 'E':
                    _mode = _mode == Mode.Erase ? Mode.None : Mode.Erase;
                    break;
                case 'L':
                    _mode = _mode == Mode.Land ? Mode.None : Mode.Land;
                    break;
                case 'B':
                    _color = _darkBlue;
                    break;
                case 'b':
                    _color = _blue;
                    break;
                case 'G':
                    _color = _darkGreen;
                    break;
                case 'g':
                    _color = _green;
                    break;
                case 'p':
                    _color = _pink;
                    break;
                case 'o':
                    _color = _orange;
                    break;
                case 'y':
                    _color = _yellow;
                    break;
                case 's':
                    _color = _sand;
                    break;
                case 'S':
                    _color = _shadow;
                    break;
            }

            SetStatus();
        }
    }
}
