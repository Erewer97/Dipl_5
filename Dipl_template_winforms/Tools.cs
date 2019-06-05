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
    public class Camera
    {
        public float W { get; set; }
        public float H { get; set; }

        public float AspectRatio { get; set; }
        public float Zoom { get; set; } = 10.0f;

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float EyeX { get; set; }
        public float EyeY { get; set; }

        public Camera() {; }
        public Camera(float w, float h)
        {
            OpenTK.Matrix4 Orto;
            W = w; H = h;
            AspectRatio = w / h;

            if (w <= h)
            {
                //Orto = OpenTK.Matrix4.CreateOrthographic(10.0f * r, 10.0f, -10.0f, 10.0f);
                Orto = OpenTK.Matrix4.CreateOrthographic(Zoom * AspectRatio, Zoom, -10.0f, 10.0f);
            }
            else
            {
                //Orto = OpenTK.Matrix4.CreateOrthographic(10.0f, 10.0f * 1 / r, -10.0f, 10.0f);
                Orto = OpenTK.Matrix4.CreateOrthographic(Zoom, Zoom / AspectRatio, -10.0f, 10.0f);
            }

            OpenTK.Matrix4 Camera = OpenTK.Matrix4.LookAt(PosX, PosY, 1, EyeX, EyeY, 0, 0, 1, 0);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref Orto);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref Camera);

            GL.Viewport(0, 0, (int)w, (int)h);
        }

        public void MouseWhell(int delta)
        {
            if (delta > 0)
                Zoom += 1.0f;
            else
                Zoom -= 1.0f;

            if (Zoom <= 0.0f)
                Zoom = 0.1f;

            Resize(W, H);
        }
        public void Resize(float w, float h)
        {
            OpenTK.Matrix4 Orto;
            W = w; H = h;
            AspectRatio = W / H;

            if (W <= H)
            {
                //Orto = OpenTK.Matrix4.CreateOrthographic(10.0f * r, 10.0f, -10.0f, 10.0f);
                Orto = OpenTK.Matrix4.CreateOrthographic(Zoom * AspectRatio, Zoom, -10.0f, 10.0f);
            }
            else
            {
                //Orto = OpenTK.Matrix4.CreateOrthographic(10.0f, 10.0f * 1 / r, -10.0f, 10.0f);
                Orto = OpenTK.Matrix4.CreateOrthographic(Zoom, Zoom / AspectRatio, -10.0f, 10.0f);
            }

            OpenTK.Matrix4 Camera = OpenTK.Matrix4.LookAt(PosX, PosY, 1, EyeX, EyeY, 0, 0, 1, 0);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref Orto);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref Camera);

            GL.Viewport(0, 0, (int)W, (int)H);
        }
    }

    public class WorkWithMouse
    {
        public float AspectRatio { get; set; }
        public float Zoom { get; set; } = 10.0f;
        public float W { get; set; }
        public float H { get; set; }

        // Constructors
        public WorkWithMouse(GLControl gl, float CameraZoom)
        {
            W = gl.Width; H = gl.ClientSize.Height; Zoom = CameraZoom; AspectRatio = W / H;
        }
        public WorkWithMouse(int w, int h, float CameraZoom)
        {
            W = w; H = h; Zoom = CameraZoom; AspectRatio = W / H;
        }

        // Public Methods
        public Vector2d MousePosition(int x, int y)
        {
            return new Vector2d(__CalcX(x), __CalcY(y));
        }
        public Vector2d MousePosition(double x, double y)
        {
            return new Vector2d(__CalcX((int)x), __CalcY((int)y));
        }
        public Vector2d MousePosition(MouseEventArgs e)
        {
            return new Vector2d(__CalcX(e.X), __CalcY(e.Y));
        }

        
        double __CalcX(int x)
        {
            double X = 0;

            if (W > H)
                X = (x - W / 2.0) / (W / Zoom);
            else
                X = (x - W / 2.0) / (W / Zoom) / (H / W);

            return X;
        }
        double __CalcY(int y)
        {
            double Y = 0;

            if (W > H)
                Y = -(y - H / 2.0) / (H / Zoom) / (W / H);
            else
                Y = -(y - H / 2.0) / (H / Zoom);

            return Y;
        }
    }

    public class PropertiesClass
    {
        public bool IsNewFile { get; set; } = false;
        public bool IsEditMode { get; set; } = false;
        public bool IsSelectingPoint { get; set; } = true;
        public bool IsMove { get; set; } = true;
        public bool IsMovePoint { get; set; } = false;
        public bool IsMoveEdge { get; set; } = false;
        public bool IsShiftPress { get; set; } = false;
        public Figure CurrentFigure { get; set; } = null;
        public Figure AddedFigure { get; set; } = null;
        public TypeFigures CurrentTypeFigure { get; set; } = TypeFigures.None;
        public Figure SelectedFigure { get; set; } = null;
        public Figure SecondFigure { get; set; } = null;
        public Color CurrentFillColor { get; set; } = Color.Silver;
        public Color CurrentBorderColor { get; set; } = Color.Black;
        public Rectangle Canvas { get; set; }
        public Vector2d FirstMousePos { get; set; }
        public Vector2d SecondMousePos { get; set; }
        public ActionWithFigure AWF { get; set; } = ActionWithFigure.None;
        public SelectingMode SelectingMode { get; set; } = SelectingMode.Points;
        public List<Figure> ListSelFig { get; set; } = new List<Figure>();
        public List<Vector2d> res { get; set; } = new List<Vector2d>();
    }

    public class Grid
    {
        public double Size { get; set; }
        public Color ColorGrid { get; set; } = Color.LightGray;
        public bool IsShow { get; set; }

        public Grid(double size)
        {
            Size = size;
        }

        public void Draw()
        {
            if (IsShow)
            {
                GL.PushMatrix();
                //GL.Translate(-Size * 0.1 / 2, -Size * 0.1 / 2, 0);
                GL.Color3(ColorGrid);
                GL.Begin(BeginMode.Lines);
                for (double j = -Size; j <= Size; j++)
                {
                    GL.Vertex3(j, -Size, 1.0);
                    GL.Vertex3(j, Size, 1.0);
                }
                for (double j = -Size; j <= Size; j++)
                {
                    GL.Vertex3(-Size, j, 1.0);
                    GL.Vertex3(Size, j, 1.0);
                }
                GL.End();
                GL.PopMatrix();
            }

        }
    }

    public class Helper
    {
        public Figure Add(TypeFigures type, List<Vector2d> mousePos)
        {
            Vector2d fmp = mousePos[0];
            Vector2d smp = mousePos[1];

            Figure f = new Figure();

            switch (type)
            {
                case TypeFigures.Line:
                    f.Edges = new List<Edge>()
                        {
                            new Edge(new Vector2d(fmp.X, fmp.Y), new Vector2d(smp.X, smp.Y))
                        };
                    f.Type = TypeFigures.Line;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.SortAtClock();
                    f.IsClosed = false;
                    f.ReCalc();
                    break;

                case TypeFigures.Rect:
                    f.Edges = new List<Edge>()
                        {
                            new Edge(new Vector2d(fmp.X, fmp.Y), new Vector2d(fmp.X, smp.Y)),
                            new Edge(new Vector2d(fmp.X, smp.Y), new Vector2d(smp.X, smp.Y)),
                            new Edge(new Vector2d(smp.X, smp.Y), new Vector2d(smp.X, fmp.Y)),
                            new Edge(new Vector2d(smp.X, fmp.Y), new Vector2d(fmp.X, fmp.Y))
                        };
                    f.Type = TypeFigures.Rect;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.SortAtClock();
                    f.ReCalc();
                    break;

                case TypeFigures.Ellipsoid:
                    //f.Edges = CalcEllipsoid(fmp, smp, 360).ToList();
                    f.Edges = CalcBezieEllipsoid(fmp, smp);
                    f.Type = TypeFigures.Ellipsoid;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.ReCalc();
                    break;

                case TypeFigures.Polygon:
                    f.Edges = CalcEllipsoid(fmp, smp, 6).ToList();
                    f.Type = TypeFigures.Polygon;
                    f.Center = (fmp + smp) / 2.0;
                    f.TranslateToCenterCoordinates();
                    f.ReCalc();
                    break;

                case TypeFigures.Curve:
                    f.Edges.Add(new Edge(fmp, smp));
                    f.Type = TypeFigures.Curve;
                    f.Center = (fmp + smp) / 2.0;
                    f.IsClosed = false;
                    f.Edges[0].ConvertToBezie();
                    f.TranslateToCenterCoordinates();
                    f.ReCalc();
                    break;

                default:
                    break;
            }

            return f;
        }

        List<Edge> CalcEllipsoid(Vector2d fmp, Vector2d smp)
        {
            List<Edge> le = new List<Edge>();

            Vector2d c = (fmp + smp) / 2.0;
            double r1 = Vector2d.Max(fmp, smp).X - c.X;
            double r2 = Vector2d.Max(fmp, smp).Y - c.Y;

            double theta = MathHelper.DegreesToRadians(60);
            for (int i = 0; i < 6; i++)
            {
                double th = theta * i;
                var e = new Edge();
                e.Begin = new Vector2d(r1 * Math.Cos(th), r2 * Math.Sin(th)) + c;
                double t = th + theta;
                e.End = new Vector2d(r1 * Math.Cos(t), r2 * Math.Sin(t)) + c;
                le.Add(e);
            }

            return le;
        }
        List<Edge> CalcEllipsoid(Vector2d fmp, Vector2d smp, int countLines)
        {
            List<Edge> le = new List<Edge>();

            Vector2d c = (fmp + smp) / 2.0;
            double r1 = Vector2d.Max(fmp, smp).X - c.X;
            double r2 = Vector2d.Max(fmp, smp).Y - c.Y;

            double theta = MathHelper.DegreesToRadians(360.0 / countLines);
            for (int i = 0; i < countLines; i++)
            {
                double th = theta * i;
                var e = new Edge();
                e.Begin = new Vector2d(r1 * Math.Cos(th), r2 * Math.Sin(th)) + c;
                double t = th + theta;
                e.End = new Vector2d(r1 * Math.Cos(t), r2 * Math.Sin(t)) + c;
                le.Add(e);
            }

            return le;
        }

        List<Edge> CalcBezieEllipsoid(Vector2d fmp, Vector2d smp)
        {
            List<Edge> edges = new List<Edge>();

            double L = 0.55228474;

            Vector2d c = (fmp + smp) / 2.0;
            double rx = Vector2d.Max(fmp, smp).X - c.X;
            double ry = Vector2d.Max(fmp, smp).Y - c.Y;

            edges.Add(
                new Edge(
                    new Vector2d(c.X + rx, c.Y),
                    new Vector2d(c.X, c.Y + ry),
                    new Vector2d(c.X + rx, c.Y + (L * ry)),
                    new Vector2d(c.X + (L * rx), c.Y + ry)
                    ));
            edges.Add(
                new Edge(
                    new Vector2d(c.X, c.Y + ry),
                    new Vector2d(c.X - rx, c.Y),
                    new Vector2d(c.X - (L * rx), c.Y + ry),
                    new Vector2d(c.X - rx, c.Y + (L * ry))
                    ));
            edges.Add(
                new Edge(
                    new Vector2d(c.X - rx, c.Y),
                    new Vector2d(c.X, c.Y - ry),
                    new Vector2d(c.X - rx, c.Y - (L * ry)),
                    new Vector2d(c.X - (L * rx), c.Y - ry)
                    ));
            edges.Add(
                new Edge(
                    new Vector2d(c.X, c.Y - ry),
                    new Vector2d(c.X + rx, c.Y),
                    new Vector2d(c.X + (L * rx), c.Y - ry),
                    new Vector2d(c.X + rx, c.Y - (L * ry))
                    ));

            return edges;
        }
    }

}
