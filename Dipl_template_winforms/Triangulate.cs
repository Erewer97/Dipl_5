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
    public class Triangulate
    {
        private Vector2d[] points; //вершины нашего многоугольника
        private Triangle[] triangles; //треугольники, на которые разбит наш многоугольник
        private bool[] taken; //была ли рассмотрена i-ая вершина многоугольника

        public List<Triangle> Triangles
        {
            get {
                if (triangles != null)
                    return triangles.ToList();
                else
                    return new List<Triangle>();
            }
            set { }
        }

        public Triangulate(Vector2d[] points) //points - х и y координаты
        {
            this.points = points; //преобразуем координаты в вершины

            triangles = new Triangle[this.points.Length - 2];

            taken = new bool[this.points.Length];

            if (points.Length > 2)
                triangulate(); //триангуляция
        }

        private void triangulate() //триангуляция
        {
            int trainPos = 0; //
            int leftPoints = points.Length; //сколько осталось рассмотреть вершин

            //текущие вершины рассматриваемого треугольника
            int ai = findNextNotTaken(0);
            int bi = findNextNotTaken(ai + 1);
            int ci = findNextNotTaken(bi + 1);

            int count = 0; //количество шагов

            while (leftPoints > 3) //пока не остался один треугольник
            {
                if (isLeft(points[ai], points[bi], points[ci]) && canBuildTriangle(ai, bi, ci)) //если можно построить треугольник
                {
                    triangles[trainPos++] = new Triangle(points[ai], points[bi], points[ci]); //новый треугольник
                    taken[bi] = true; //исключаем вершину b
                    leftPoints--;
                    bi = ci;
                    ci = findNextNotTaken(ci + 1); //берем следующую вершину
                }
                else
                { //берем следующие три вершины
                    ai = findNextNotTaken(ai + 1);
                    bi = findNextNotTaken(ai + 1);
                    ci = findNextNotTaken(bi + 1);
                }

                if (count > points.Length * points.Length)
                { //если по какой-либо причине (например, многоугольник задан по часовой стрелке) триангуляцию провести невозможно, выходим
                    triangles = null;
                    break;
                }

                count++;
            }

            if (triangles != null) //если триангуляция была проведена успешно
                triangles[trainPos] = new Triangle(points[ai], points[bi], points[ci]);
        }

        private int findNextNotTaken(int startPos) //найти следущую нерассмотренную вершину
        {
            startPos %= points.Length;
            if (!taken[startPos])
                return startPos;

            int i = (startPos + 1) % points.Length;
            while (i != startPos)
            {
                if (!taken[i])
                    return i;
                i = (i + 1) % points.Length;
            }

            return -1;
        }

        private bool isLeft(Vector2d a, Vector2d b, Vector2d c) //левая ли тройка векторов
        {
            double abX = b.X - a.X;
            double abY = b.Y - a.Y;
            double acX = c.X - a.X;
            double acY = c.Y - a.Y;

            return abX * acY - acX * abY > 0;
        }

        private bool isPointInside(Vector2d a, Vector2d b, Vector2d c, Vector2d p) //находится ли точка p внутри треугольника abc
        {
            double ab = (a.X - p.X) * (b.Y - a.Y) - (b.X - a.X) * (a.Y - p.Y);
            double bc = (b.X - p.X) * (c.Y - b.Y) - (c.X - b.X) * (b.Y - p.Y);
            double ca = (c.X - p.X) * (a.Y - c.Y) - (a.X - c.X) * (c.Y - p.Y);

            return (ab >= 0 && bc >= 0 && ca >= 0) || (ab <= 0 && bc <= 0 && ca <= 0);
        }

        private bool canBuildTriangle(int ai, int bi, int ci) //false - если внутри есть вершина
        {
            for (int i = 0; i < points.Length; i++) //рассмотрим все вершины многоугольника
                if (i != ai && i != bi && i != ci) //кроме троих вершин текущего треугольника
                    if (isPointInside(points[ai], points[bi], points[ci], points[i]))
                        return false;
            return true;
        }

        public Vector2d[] getPoints() //возвращает вершины
        {
            return points;
        }

        public Triangle[] getTriangles() //возвращает треугольники
        {
            return triangles;
        }

    }

    public class Triangle //треугольник
    {
        private Vector2d a, b, c;

        public Vector2d A { get { return a; } set { } }
        public Vector2d B { get { return b; } set { } }
        public Vector2d C { get { return c; } set { } }

        public Triangle(Vector2d a, Vector2d b, Vector2d c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}
