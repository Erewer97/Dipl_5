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

            treeView1.MouseDown += treeView_MouseDown;
            treeView1.ExpandAll();
        }

        #region GL EVENTS
        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.White);
            _core.AddLayer();

            treeView1.Nodes.AddRange(_core.NodesForTree());
        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            grid.Draw();

            _core.Draw();

            if (pc.Group.SelectingFigure != null) pc.Group.SelectingFigure.DrawSelect();

            if (pc.AddedFigure != null) pc.AddedFigure.Draw();

            pc.Group.DrawAABB();

            if (trianglesBool != null) trianglesBool.Draw();

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
                            if (pc.Group.SelectingFigure != null)
                            {
                                if (pc.SelectingMode == SelectingMode.Points)
                                    pc.IsMovePoint = pc.Group.SelectingFigure.HitInPoint(pc.FirstMousePos);
                                else
                                    pc.IsMoveEdge  = pc.Group.SelectingFigure.HitInBorder(pc.FirstMousePos);
                            }
                        }
                        else
                        {
                            if (pc.IsShiftPress)
                            {
                                Figure f = _core.Find(pc.FirstMousePos);
                                pc.Group.Add(f);
                            }
                            else
                            {
                                //if (pc.SecondFigure != null)
                                //{
                                //    pc.SecondFigure.IsSelect = true;
                                //    pc.AWF = pc.SecondFigure.HitOnManipulators(pc.FirstMousePos);
                                //    if (pc.AWF != ActionWithFigure.None)
                                //        pc.Group.SelectingFigure = pc.SecondFigure;
                                //}

                                //if (pc.AWF == ActionWithFigure.None)
                                //    pc.Group.SelectingFigure = _core.Find(pc.FirstMousePos);  // ищем фигуры

                                //if (pc.Group.SelectingFigure != null && pc.SecondFigure == null)
                                //{
                                //    pc.SecondFigure = pc.Group.SelectingFigure;
                                //    pc.Group.SelectingFigure.IsSelect = true;
                                //    SetProperties(pc.Group.SelectingFigure);

                                //    dataGridView1.Rows.Clear();
                                //    foreach (Vector2d v in pc.Group.SelectingFigure.Verteces)
                                //        dataGridView1.Rows.Add(v.X.ToString("F"), v.Y.ToString("F"));
                                //}
                                //else if (pc.Group.SelectingFigure == null && pc.SecondFigure != null)
                                //{
                                //    pc.SecondFigure.IsSelect = false;
                                //    pc.SecondFigure = null;
                                //    SetProperties(null);
                                //    pc.Group.Clear();
                                //}
                                //else
                                //{

                                //}
                                ;
                                if (pc.Group.IsHitInGroup(pc.FirstMousePos))
                                {
                                    if (pc.Group.HitInCenter(pc.FirstMousePos))
                                        pc.IsMove = true;
                                    
                                }

                                if (pc.Group.CountFigures == 0)
                                    pc.Group.Add(_core.Find(pc.FirstMousePos));

                                if (pc.Group.CountFigures == 1)
                                {
                                    pc.AWF = pc.Group.SelectingFigure.HitOnManipulators(pc.FirstMousePos);
                                    //if (pc.AWF == ActionWithFigure.None)
                                    //    pc.Group.Clear();
                                }

                                Figure f = _core.Find(pc.FirstMousePos);
                                if (f == null)
                                    pc.Group.Clear();
                                if (f != null && pc.Group.CountFigures >= 1)
                                {
                                    pc.Group.Clear();
                                    pc.Group.Add(f);
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }

                Deb(pc.Group.Figures.Count.ToString(), false);
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

                case TypeFigures.Curve:
                    if (pc.AddedFigure != null)
                    {
                        pc.AddedFigure = helper.Add(TypeFigures.Curve, new List<Vector2d>() { pc.FirstMousePos, pc.SecondMousePos });
                        pc.AddedFigure.FillColor = pc.CurrentFillColor;
                        pc.AddedFigure.BorderColor = pc.CurrentBorderColor;
                        //pc.AddedFigure.CalcAABB();
                    }
                    break;

                case TypeFigures.None:
                    if (pc.IsMove)
                        pc.Group.MoveTo(pc.SecondMousePos);
                    if (pc.Group.SelectingFigure != null)
                    {
                        if (pc.IsEditMode == false)                           
                        {
                            switch (pc.AWF)
                            {
                                case ActionWithFigure.Move:
                                    pc.Group.SelectingFigure.MoveTo = pc.SecondMousePos;
                                    pc.Group.SelectingFigure.ReCalc();
                                    SetProperties(pc.Group.SelectingFigure);
                                    ShowPointsInGridView(pc.Group.SelectingFigure);
                                    break;

                                case ActionWithFigure.Rotate:
                                    pc.Group.SelectingFigure.CalcAngle(pc.SecondMousePos);
                                    pc.Group.SelectingFigure.ReCalc();
                                    pc.Group.ReCalcAABB();
                                    SetProperties(pc.Group.SelectingFigure);
                                    ShowPointsInGridView(pc.Group.SelectingFigure);
                                    break;

                                case ActionWithFigure.Scale:
                                    pc.Group.SelectingFigure.CalcsScale(pc.SecondMousePos);
                                    pc.Group.SelectingFigure.ReCalc();
                                    pc.Group.ReCalcAABB();
                                    SetProperties(pc.Group.SelectingFigure);
                                    ShowPointsInGridView(pc.Group.SelectingFigure);
                                    break;

                                case ActionWithFigure.None:
                                default:
                                    break;
                            }
                            
                        }
                        else
                        {
                            if (pc.SelectingMode == SelectingMode.Points)
                            {
                                if (pc.IsMovePoint)
                                    pc.Group.SelectingFigure.SetNewPoint(pc.SecondMousePos);
                            }
                            else
                            {
                                if (pc.IsMoveEdge)
                                {
                                    pc.Group.SelectingFigure.SetNewEdge(pc.SecondMousePos, pc.FirstMousePos);
                                    pc.FirstMousePos = pc.SecondMousePos;
                                }
                            }
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
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Line " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            treeView1.Nodes.Clear();
                            treeView1.Nodes.AddRange(_core.NodesForTree());
                            pc.AddedFigure = null;

                            break;

                        case TypeFigures.Rect:
                            _core.Add(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Rectangle " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            treeView1.Nodes.Clear();
                            treeView1.Nodes.AddRange(_core.NodesForTree());
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.Ellipsoid:
                            _core.Add(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Ellipsoid " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            treeView1.Nodes.Clear();
                            treeView1.Nodes.AddRange(_core.NodesForTree());
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.Polygon:
                            _core.Add(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Polygon " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            treeView1.Nodes.Clear();
                            treeView1.Nodes.AddRange(_core.NodesForTree());
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.Curve:
                            _core.Add(pc.AddedFigure);
                            pc.AddedFigure.Id = _core.Ids.ToString();
                            pc.AddedFigure.Name = "Curve " + pc.AddedFigure.Id;
                            SetProperties(pc.AddedFigure);
                            treeView1.Nodes.Clear();
                            treeView1.Nodes.AddRange(_core.NodesForTree());
                            pc.AddedFigure = null;
                            break;

                        case TypeFigures.None:
                            pc.IsMove = false;
                            if (pc.Group.SelectingFigure != null)
                            {
                                pc.AWF = ActionWithFigure.None;
                                pc.IsMovePoint = false;
                                pc.IsMoveEdge = false;
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
            treeView1.ExpandAll();
        }
        #endregion

        #endregion

        #region LEFT TOOL STRIP
        // Func for select tool
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            pc.CurrentTypeFigure = TypeFigures.None;
            pc.IsEditMode = false;
            if (pc.Group.SelectingFigure != null)
            {
                pc.Group.SelectingFigure.IsEdit = false;
                pc.Group.SelectingFigure.IsSelect = true;
            }
        }
        // edit points tool
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pc.IsEditMode = true;
            if (pc.Group.SelectingFigure != null)
            {
                pc.Group.SelectingFigure.IsEdit = true;
                pc.Group.SelectingFigure.IsSelect = false;
            }
        }
        // Select (in Edit mode) Point
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            if (pc.IsEditMode)
            {
                pc.SelectingMode = SelectingMode.Points;
            }
        }
        // Select (in Edit mode) Edge
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            if (pc.IsEditMode)
            {
                pc.SelectingMode = SelectingMode.Edges;
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
        // Удалить выбранную фигуру
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            //if (pc.Group.SelectingFigure != null)
            //{
            //    pc.Group.Del(pc.Group.SelectingFigure);
            //    _core.Del(pc.Group.SelectingFigure);
            //    treeView1.Nodes.Clear();
            //    treeView1.Nodes.AddRange(_core.NodesForTree());
            //    pc.Group.SelectingFigure = null;
            //    glControl1.Invalidate();              
            //    treeView1.ExpandAll();
            //}
            if (!pc.Group.IsEmpty)
            {
                foreach (Figure f in pc.Group.Figures)
                    _core.Del(f);
                pc.Group.Clear();
                treeView1.Nodes.Clear();
                treeView1.Nodes.AddRange(_core.NodesForTree());
                glControl1.Invalidate();
                treeView1.ExpandAll();
            }
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
        private void toolStripButton_exportInGEO_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    if (sfd.ShowDialog() == DialogResult.Cancel)
                        return;
                    // получаем выбранный файл
                    string filename = sfd.FileName;
                    // сохраняем текст в файл
                    ExportInGEO exportInGEO = new ExportInGEO(filename);
                    exportInGEO.WriteInfile(new List<Figure>() { pc.Group.SelectingFigure });
                }
            }
        }
        #endregion

        #region TAB "OBJECT"
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

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                case Keys.Shift:
                    pc.IsShiftPress = true;
                    break;

                default:
                    break;
            }
        }

        private void glControl1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ShiftKey:
                case Keys.Shift:
                    pc.IsShiftPress = false;
                    break;

                default:
                    break;
            }
        }

        void Deb(string text, bool append)
        {
            if (append)
            {
                label1.Text += text + "\n";
            }
            else
            {
                label1.Text = text + "\n";
            }
        }
        void ShowPointsInGridView(Figure figure)
        {
            dataGridView1.Rows.Clear();
            if (figure != null) 
                foreach (Vector2d v in figure.Verteces)
                    dataGridView1.Rows.Add(v.X.ToString("F"), v.Y.ToString("F"));
        }

        private void tsb_subdivEdge_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                pc.Group.SelectingFigure.SubDivEdge();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            _core.AddLayer();
            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(_core.NodesForTree());
            treeView1.ExpandAll();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            _core.DelLayer();
            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(_core.NodesForTree());
            glControl1.Invalidate();
            treeView1.ExpandAll();
        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                pc.Group.SelectingFigure.ToBezie();
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                pc.Group.SelectingFigure.ToLine();
        }

        private void treeView_MouseDown(object s, MouseEventArgs e)
        {
            TreeViewHitTestInfo info = treeView1.HitTest(e.X, e.Y);
            TreeNode hitNode;
            if (info.Node != null)
            {
                hitNode = info.Node;
                Figure f = new Figure();
                if ((f = _core.Find(hitNode.Name)) != null)
                {
                    pc.Group.SelectingFigure = f;
                    pc.Group.SelectingFigure.IsSelect = true;
                }
                glControl1.Invalidate();
            }
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                if (pc.Group.SelectingFigure.DelElement())
                {
                    pc.Group.SelectingFigure.ReCalc();
                    glControl1.Invalidate();
                }
                    
        }

        private void tsb_smoothContrPoints_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                if (pc.Group.SelectingFigure.SmoothControlPoints())
                {
                    pc.Group.SelectingFigure.ReCalc();
                    glControl1.Invalidate();
                }
        }

        private void btn_bor_color_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                using (ColorDialog cd = new ColorDialog())
                {
                    if (cd.ShowDialog() == DialogResult.Cancel)
                        return;
                    pc.Group.SelectingFigure.BorderColor = btn_bor_color.BackColor = cd.Color;
                    glControl1.Invalidate();
                }
        }

        private void btn_fill_color_Click(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
                using (ColorDialog cd = new ColorDialog())
                {
                    if (cd.ShowDialog() == DialogResult.Cancel)
                        return;
                    pc.Group.SelectingFigure.FillColor = btn_fill_color.BackColor = cd.Color;
                    glControl1.Invalidate();
                }
        }

        private void cb_IsShow_CheckedChanged(object sender, EventArgs e)
        {
            if (pc.Group.SelectingFigure != null)
            {
                if (cb_IsShow.Checked)
                    pc.Group.SelectingFigure.IsRender = false;
                else
                    pc.Group.SelectingFigure.IsRender = true;

                glControl1.Invalidate();
            }
        }

        #region BUTTONS FOR BOOLEAN OPERATIONS

        TrianglesBool trianglesBool;

        private void button1_Click(object sender, EventArgs e)
        {
            if (pc.Group.CountFigures == 2)
            {
                var f1 = pc.Group.Figures[0].Verteces;
                var f2 = pc.Group.Figures[1].Verteces;

                var t1 = pc.Group.Figures[0].Triangles;
                var t2 = pc.Group.Figures[1].Triangles;

                if (pc.IsAttenton)
                {
                    Modificators modificators = new Modificators(f1, f2);
                    modificators.Operation = Operations.Interset;

                    pc.res = modificators.Result();
                    if (pc.res.Count > 0)
                    {
                        Figure f = new Figure();

                        helper.DeleteDuplicats(pc.res);

                        f.Edges = Helper.ConvertToEdges(pc.res);
                        f.Type = TypeFigures.Polygon;
                        f.Center = helper.CalcCenter(pc.res);
                        f.TranslateToCenterCoordinates();
                        f.ReCalc();

                        f.FillColor   = pc.Group.Figures[1].FillColor;
                        f.BorderColor = pc.Group.Figures[1].BorderColor;

                        pc.ClearListSelectedFigures();

                        f.Id = _core.Ids.ToString();
                        f.Name = "BoolenResult " + f.Id;
                        SetProperties(f);
                        _core.Add(f);
                        
                        treeView1.Nodes.Clear();
                        treeView1.Nodes.AddRange(_core.NodesForTree());
                        treeView1.ExpandAll();

                        glControl1.Invalidate();
                    }
                }
                else
                {
                    trianglesBool = new TrianglesBool(t1, t2, f1, f2, Operations.Interset);
                }
            }
        }
        
        private void button_union_Click(object sender, EventArgs e)
        {
            if (pc.Group.Figures.Count == 2)
            {
                var f1 = pc.Group.Figures[0].Verteces;
                var f2 = pc.Group.Figures[1].Verteces;

                if (pc.IsAttenton)
                {
                    Modificators modificators = new Modificators(f1, f2);
                    modificators.Operation = Operations.Union;

                    pc.res = modificators.Result();
                    if (pc.res.Count > 0)
                    {
                        Figure f = new Figure();

                        helper.DeleteDuplicats(pc.res);


                        f.Edges = Helper.ConvertToEdges(pc.res);
                        f.Type = TypeFigures.Polygon;
                        f.Center = helper.CalcCenter(pc.res);
                        f.TranslateToCenterCoordinates();
                        f.ReCalc();

                        f.FillColor   = pc.Group.Figures[1].FillColor;
                        f.BorderColor = pc.Group.Figures[1].BorderColor;

                        pc.ClearListSelectedFigures();

                        f.Id = _core.Ids.ToString();
                        f.Name = "BoolenResult " + f.Id;
                        _core.Add(f);
                        //SetProperties(pc.AddedFigure);
                        treeView1.Nodes.Clear();
                        treeView1.Nodes.AddRange(_core.NodesForTree());
                        treeView1.ExpandAll();

                        glControl1.Invalidate();
                    }
                }
                else
                    trianglesBool = new TrianglesBool(pc.Group.Figures[0], pc.Group.Figures[1], Operations.Union);
            }
            else if (pc.Group.Figures.Count > 2)
            {
                var f1 = pc.Group.Figures[0].Verteces;
                var f2 = pc.Group.Figures[1].Verteces;

                Modificators modificators = new Modificators(f1, f2);
                modificators.Operation = Operations.Union;

                pc.res = modificators.Result();
                if (pc.res.Count > 0)
                {
                    helper.DeleteDuplicats(pc.res);

                    for (int i = 2; i < pc.Group.Figures.Count; i++)
                    {
                        var list = pc.Group.Figures[i].Verteces;

                        Modificators m = new Modificators(list, pc.res);
                        m.Operation = Operations.Union;

                        pc.res = m.Result();
                        helper.DeleteDuplicats(pc.res);
                    }

                    if (pc.res.Count > 0)
                    {
                        Figure f = new Figure();

                        helper.DeleteDuplicats(pc.res);


                        f.Edges = Helper.ConvertToEdges(pc.res);
                        f.Type = TypeFigures.Polygon;
                        f.Center = helper.CalcCenter(pc.res);
                        f.TranslateToCenterCoordinates();
                        f.ReCalc();

                        f.FillColor   = pc.Group.Figures[pc.Group.Figures.Count - 1].FillColor;
                        f.BorderColor = pc.Group.Figures[pc.Group.Figures.Count - 1].BorderColor;

                        pc.ClearListSelectedFigures();

                        f.Id = _core.Ids.ToString();
                        f.Name = "BoolenResult " + f.Id;
                        _core.Add(f);
                        //SetProperties(pc.AddedFigure);
                        treeView1.Nodes.Clear();
                        treeView1.Nodes.AddRange(_core.NodesForTree());
                        treeView1.ExpandAll();

                        glControl1.Invalidate();
                    }
                }               
            }
        }

        private void button_sub_Click(object sender, EventArgs e)
        {
            if (pc.Group.Figures.Count > 1)
            {
                if (pc.IsAttenton)
                {
                    var f1 = pc.Group.Figures[0].Verteces;
                    var f2 = pc.Group.Figures[1].Verteces;

                    Modificators modificators = new Modificators(f1, f2);
                    modificators.Operation = Operations.Sub;

                    pc.res = modificators.Result();
                    if (pc.res.Count > 0)
                    {
                        Figure f = new Figure();

                        helper.DeleteDuplicats(pc.res);

                        f.Edges = Helper.ConvertToEdges(pc.res);
                        f.Type = TypeFigures.Polygon;
                        f.Center = helper.CalcCenter(pc.res);
                        f.TranslateToCenterCoordinates();
                        f.ReCalc();

                        f.FillColor   = pc.Group.Figures[1].FillColor;
                        f.BorderColor = pc.Group.Figures[1].BorderColor;

                        f.Id = _core.Ids.ToString();
                        f.Name = "BoolenResult " + f.Id;

                        SetProperties(f);
                        _core.Add(f);

                        treeView1.Nodes.Clear();
                        treeView1.Nodes.AddRange(_core.NodesForTree());
                        treeView1.ExpandAll();

                        glControl1.Invalidate();
                    }
                }
                else
                {
                    trianglesBool = new TrianglesBool(pc.Group.Figures[0], pc.Group.Figures[1], Operations.Sub);
                }
            }
        }

        private void radioButton_attenton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_attenton.Checked)
                pc.IsAttenton = true;
        }
        private void radioButton_triangles_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_triangles.Checked)
                pc.IsAttenton = false;
        }
        #endregion

        private void button_doneBoolOperation_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (pc.Group.SelectingFigure != null && pc.SelectingMode == SelectingMode.Points)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                var t = dataGridView1.CurrentCell;
                int col = t.ColumnIndex;
                int row = t.RowIndex - 1;

                string r = dataGridView1[0, row].Value.ToString() + " Y: " + dataGridView1[1, row].Value.ToString();
                double x = ParseToDouble(dataGridView1[0, row].Value.ToString());
                double y = ParseToDouble(dataGridView1[1, row].Value.ToString());

                Vector2d v = new Vector2d(x, y);

                //MessageBox.Show(v.ToString(), "Current Cell");
                if (pc.Group.SelectingFigure.HitInPoint(pc.PointInGridView))
                    pc.Group.SelectingFigure.SetNewPoint(v);

                glControl1.Invalidate();
            }
            
        }
        public double ParseToDouble(string value)
        {
            double result = Double.NaN;
            value = value.Trim();
            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out result))
            {
                if (!double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US"), out result))
                {
                    return Double.NaN;
                }
            }
            return result;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var t = dataGridView1.CurrentCell;
            int col = t.ColumnIndex;
            int row = t.RowIndex;

            if (dataGridView1[0, row].Value != null)
            {
                string r = dataGridView1[0, row].Value.ToString() + " Y: " + dataGridView1[1, row].Value.ToString();
                double x = ParseToDouble(dataGridView1[0, row].Value.ToString());
                double y = ParseToDouble(dataGridView1[1, row].Value.ToString());

                pc.PointInGridView = new Vector2d(x, y);

                //MessageBox.Show(v.ToString(), "Current Cell");
            }
        }
    }
}
