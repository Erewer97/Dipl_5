using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;

namespace Dipl_template_winforms
{
    public partial class Form1 : Form
    {
        Grid grid = new Grid(5);
        Camera camera = new Camera();
        WorkWithMouse mouse;
        PropertiesClass pc = new PropertiesClass();
        Helper helper = new Helper();

        Core _core = new Core();

        public Form1()
        {
            InitializeComponent();

            glControl1.Load += glControl1_Load;
            glControl1.Resize += glControl1_Resize;
            glControl1.Paint += glControl1_Paint;
            glControl1.MouseWheel += GlControl1_MouseWheel;

            // Mouse subs
            glControl1.MouseDown += glControl1_MouseDown;
            glControl1.MouseMove += glControl1_MouseMove;
            glControl1.MouseUp += glControl1_MouseUp;

            glControl1.MouseDown += new MouseEventHandler(delegate (Object o, MouseEventArgs a) { glControl1.Invalidate(); });
            glControl1.MouseUp += new MouseEventHandler(delegate (Object o, MouseEventArgs a) { glControl1.Invalidate(); });
            glControl1.MouseMove += new MouseEventHandler(delegate (Object o, MouseEventArgs a) { glControl1.Invalidate(); });

            mouse = new WorkWithMouse(glControl1, 10.0f);

            grid.IsShow = true;

            if (pc.IsNewFile == false)
            {
                //btn_Add_ellipsoid.Enabled =
                //    btn_Add_line.Enabled =
                //    btn_Add_polygon.Enabled =
                //    btn_Add_rect.Enabled =
                //    false;
            }
        }

        #region GL EVENTS
        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.White);
            _core.AddLayer();
        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            grid.Draw();

            _core.Draw();

            if (pc.AddedFigure != null) pc.AddedFigure.Draw();
            if (pc.SelectedFigure != null) pc.SelectedFigure.Draw();

            GL.Flush();
            GL.Finish();

            glControl1.SwapBuffers();
        }
        private void glControl1_Resize(object sender, EventArgs e)
        {
            camera.Resize(glControl1.Width, glControl1.Height);
            mouse.W = glControl1.Width;
            mouse.H = glControl1.Height;
            glControl1.Invalidate();
        }
        private void GlControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            camera.MouseWhell(e.Delta);
            mouse.Zoom = camera.Zoom;
            glControl1.Invalidate();
        }

        #region MOUSE EVENTS
        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            pc.FirstMousePos = mouse.MousePosition(e);

            if (e.Button == MouseButtons.Left)
            {
                switch (pc.CurrentTypeFigure)
                {
                    case TypeFigures.Line:
                    case TypeFigures.Rect:
                    case TypeFigures.Ellipsoid:
                    case TypeFigures.Polygon:
                    case TypeFigures.Curve:
                        pc.AddedFigure = new Figure();
                        break;

                    case TypeFigures.None:
                        if (pc.IsEditMode)
                        {
                            if (pc.SelectedFigure != null)
                            {
                                pc.IsMovePoint = pc.SelectedFigure.HitInPoint(pc.FirstMousePos);
                            }
                        }
                        else
                        {
                            if (pc.SecondFigure != null)
                            {
                                pc.SecondFigure.IsSelect = true;
                                pc.AWF = pc.SecondFigure.HitOnManipulators(pc.FirstMousePos);
                                if (pc.AWF != ActionWithFigure.None)
                                    pc.SelectedFigure = pc.SecondFigure;
                            }

                            if (pc.AWF == ActionWithFigure.None)
                                pc.SelectedFigure = _core.Find(pc.FirstMousePos);  // ищем фигуры

                            if (pc.SelectedFigure != null && pc.SecondFigure == null)
                            {
                                pc.SecondFigure = pc.SelectedFigure;
                                pc.SelectedFigure.IsSelect = true;
                                SetProperties(pc.SelectedFigure);
                            }
                            else if (pc.SelectedFigure == null && pc.SecondFigure != null)
                            {
                                pc.SecondFigure.IsSelect = false;
                                pc.SecondFigure = null;
                                SetProperties(null);
                            }
                            else
                            {

                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {

            }
            else if (e.Button == MouseButtons.Middle)
            {

            }
        }
        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            pc.SecondMousePos = mouse.MousePosition(e);

            switch (pc.CurrentTypeFigure)
            {
                case TypeFigures.Line:
                    if (pc.AddedFigure != null)
                    {
                        pc.AddedFigure = helper.Add(TypeFigures.Line, new List<Vector2d>() { pc.FirstMousePos, pc.SecondMousePos });
                        pc.AddedFigure.BorderColor = pc.CurrentBorderColor;
                        //pc.AddedFigure.CalcAABB();
                    }
                    break;

                case TypeFigures.Rect:
                    if (pc.AddedFigure != null)
                    {
                        pc.AddedFigure = helper.Add(TypeFigures.Rect, new List<Vector2d>() { pc.FirstMousePos, pc.SecondMousePos });
                        pc.AddedFigure.FillColor = pc.CurrentFillColor;
                        pc.AddedFigure.BorderColor = pc.CurrentBorderColor;
                        //pc.AddedFigure.CalcAABB();
                    }
                    break;

                case TypeFigures.Ellipsoid:
                    if (pc.AddedFigure != null)
                    {
                        pc.AddedFigure = helper.Add(TypeFigures.Ellipsoid, new List<Vector2d>() { pc.FirstMousePos, pc.SecondMousePos });
                        pc.AddedFigure.FillColor = pc.CurrentFillColor;
                        pc.AddedFigure.BorderColor = pc.CurrentBorderColor;
                        //pc.AddedFigure.CalcAABB();
                    }
                    break;

                case TypeFigures.Polygon:
                    if (pc.AddedFigure != null)
                    {
                        pc.AddedFigure = helper.Add(TypeFigures.Polygon, new List<Vector2d>() { pc.FirstMousePos, pc.SecondMousePos });
                        pc.AddedFigure.FillColor = pc.CurrentFillColor;
                        pc.AddedFigure.BorderColor = pc.CurrentBorderColor;
                        //pc.AddedFigure.CalcAABB();
                    }
                    break;

                case TypeFigures.None:
                    if (pc.SelectedFigure != null)
                    {
                        if (pc.IsEditMode == false)                           
                        {
                            switch (pc.AWF)
                            {
                                case ActionWithFigure.Move:
                                    pc.SelectedFigure.MoveTo = pc.SecondMousePos;
                                    pc.SelectedFigure.ReCalc();
                                    SetProperties(pc.SelectedFigure);
                                    break;

                                case ActionWithFigure.Rotate:
                                    pc.SelectedFigure.CalcAngle(pc.SecondMousePos);
                                    pc.SelectedFigure.ReCalc();
                                    SetProperties(pc.SelectedFigure);
                                    break;

                                case ActionWithFigure.Scale:
                                    pc.SelectedFigure.CalcsScale(pc.SecondMousePos);
                                    pc.SelectedFigure.ReCalc();
                                    SetProperties(pc.SelectedFigure);
                                    break;

                                case ActionWithFigure.None:
                                default:
                                    break;
                            }
                            if (pc.SelectedFigure.HitOnManipulators1(pc.SecondMousePos))
                                Cursor.Current = Cursors.Arrow;
                        }
                        else
                        {
                            if (pc.IsMovePoint)
                                pc.SelectedFigure.SetNewPoint(pc.SecondMousePos);
                        }
                    }
                    break;

                default:
                    break;
            }
        }
        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            pc.SecondMousePos = mouse.MousePosition(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    switch (pc.CurrentTypeFigure)
                    {
                        case TypeFigures.Line:
                            _core.Add(pc.AddedFigure);
                            //AddFigureInTreeView(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Line " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            pc.AddedFigure = null;

                            break;

                        case TypeFigures.Rect:
                            _core.Add(pc.AddedFigure);
                            //AddFigureInTreeView(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Rectangle " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.Ellipsoid:
                            _core.Add(pc.AddedFigure);
                            //AddFigureInTreeView(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Ellipsoid " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.Polygon:
                            _core.Add(pc.AddedFigure);
                            //AddFigureInTreeView(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Polygon " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.None:
                            if (pc.SelectedFigure != null)
                            {
                                pc.AWF = ActionWithFigure.None;
                                pc.IsMovePoint = false;
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                case MouseButtons.Right:
                    break;

                default:
                    break;
            }
        }
        #endregion

        #endregion

        #region LEFT TOOL STRIP
        // Func for select tool
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.None;
            pc.IsEditMode = false;
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.IsEdit = false;
                pc.SelectedFigure.IsSelect = true;
            }
        }
        // edit points tool
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pc.IsEditMode = true;
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.IsEdit = true;
                pc.SelectedFigure.IsSelect = false;
            }
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.Line;
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.Rect;
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.Ellipsoid;
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.Polygon;
        }
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.Curve;
        }

        // for Fill color
        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.Cancel)
                    return;
                pc.CurrentFillColor = toolStripButton14.BackColor = cd.Color;
            }
        }
        // for Border color
        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.Cancel)
                    return;
                pc.CurrentBorderColor = toolStripButton13.BackColor = cd.Color;
            }
        }
        #endregion

        #region TOP MENU
        // Экспорт в .geo (для GMSH)
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (pc.SelectedFigure != null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    if (sfd.ShowDialog() == DialogResult.Cancel)
                        return;
                    // получаем выбранный файл
                    string filename = sfd.FileName;
                    // сохраняем текст в файл
                    ExportInGEO exportInGEO = new ExportInGEO(filename);
                    exportInGEO.WriteInfile(new List<Figure>() { pc.SelectedFigure });
                }
            }
        }
        #endregion

        #region TAB "OBJECT"
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (pc.SelectedFigure != null && textBox1.Text.Length > 0)
                pc.SelectedFigure.Name = textBox1.Text;
            SetProperties(pc.SelectedFigure);
        }
        // translate figure at X axis
        private void nud_posX_ValueChanged(object sender, EventArgs e)
        {
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.MoveTo = new Vector2d((double)nud_posX.Value, pc.SelectedFigure.MoveTo.Y);
                pc.SelectedFigure.ReCalc();
            }
            glControl1.Invalidate();
        }
        // translate figure at Y axis
        private void nud_posY_ValueChanged(object sender, EventArgs e)
        {
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.MoveTo = new Vector2d(pc.SelectedFigure.MoveTo.X, (double)nud_posY.Value);
                pc.SelectedFigure.ReCalc();
            }
            glControl1.Invalidate();
        }
        private void nud_angle_ValueChanged(object sender, EventArgs e)
        {
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.Angle = (double)nud_angle.Value;
                pc.SelectedFigure.ReCalc();
            }
            glControl1.Invalidate();
        }
        private void nud_scaleX_ValueChanged(object sender, EventArgs e)
        {
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.ScaleTo = new Vector2d((double)nud_scaleX.Value, pc.SelectedFigure.ScaleTo.Y);
                pc.SelectedFigure.ReCalc();
            }
            glControl1.Invalidate();
        }
        private void nud_scaleY_ValueChanged(object sender, EventArgs e)
        {
            if (pc.SelectedFigure != null)
            {
                pc.SelectedFigure.ScaleTo = new Vector2d(pc.SelectedFigure.ScaleTo.X, (double)nud_scaleY.Value);
                pc.SelectedFigure.ReCalc();
            }
            glControl1.Invalidate();
        }
        #endregion

        void SetProperties(Figure figure)
        {
            if (figure != null)
            {
                textBox1.Text = figure.Name;
                nud_posX.Value = (decimal)figure.MoveTo.X;
                nud_posY.Value = (decimal)figure.MoveTo.Y;
                nud_angle.Value = (decimal)figure.Angle;
                nud_scaleX.Value = (decimal)figure.ScaleTo.X;
                nud_scaleY.Value = (decimal)figure.ScaleTo.Y;
            }
            else
            {
                textBox1.Text = "";
                nud_posX.Value = 0;
                nud_posY.Value = 0;
                nud_angle.Value = 0;
                nud_scaleX.Value = 1;
                nud_scaleY.Value = 1;
            }
        }       
    }
}
