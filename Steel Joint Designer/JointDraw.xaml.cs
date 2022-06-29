using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Steel_Joint_Designer
{
    /// <summary>
    /// Interaction logic for JointDraw.xaml
    /// </summary>
    public partial class JointDraw : Window
    {        
        public JointDraw(List<string> jointTypes)
        {

            InitializeComponent();

        }

        String currentCursor = null;
        SolidColorBrush red = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        SolidColorBrush green = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        SolidColorBrush blue = new SolidColorBrush(Color.FromRgb(0, 0, 255));
        SolidColorBrush black = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        SolidColorBrush yellow = new SolidColorBrush(Color.FromRgb(255, 255, 0));
        SolidColorBrush clear = new SolidColorBrush(Color.FromArgb(0,0,0,0));
        SolidColorBrush fillColor = new SolidColorBrush();

        int nodeNum, beamNum, colNum, angNum = 0; // initialise counters for joint elements, used for naming purposes to link drawn part with invisible hitbox

        public void Change_Cursor(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            currentCursor = button.Name; // Enables to correct cursor as was pressed
        }

        public void Draw_Element(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SolidColorBrush test = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            if(button.Name == "enableBeamPaint")
            {
                JointDrawCanvas.Background = test;
            }
        }

        public void Canvas_View(object sender, RoutedEventArgs e)
        {

        }


        public void Mouse_Move(object sender, RoutedEventArgs e)
        {
            foreach (Path child in JointDrawCanvas.Children)
            {
                string childName = child.Name;
                if (child.IsMouseOver)
                {
                    if (childName.Contains("Hit"))
                    {
                        foreach (Path c in JointDrawCanvas.Children) //Could look into using ienumerable access into the children?
                        {
                            if (c.Name == childName.Replace("Hit", "Vis"))
                            {
                                c.Stroke = yellow;
                            }
                        }

                    } else
                    {
                        child.Stroke = yellow;
                    }
                }
            
                else
                {  // I wonder if there is a better way to recolor which doe not rely on the child.Name

                    if (child.Name.Contains("Vis"))
                    {

                        if (child.Name.Contains("Node"))
                        {
                            child.Stroke = red;
                        }
                        else
                        {
                            child.Stroke = black;
                        }
                    }
                    else
                    {
                        child.Stroke = clear;
                    }

                }


            }
            //Point p = Mouse.GetPosition(JointDrawCanvas);          

        }



        //Draw_Beam and Draw_Column need a method to find the closest node to draw too, currently will draw to the node found first in the foreach loop

        public Path Draw_Beam(Point p)
        {

            GeometryGroup toDraw = new GeometryGroup();

            Path toDrawPath = new Path();

            foreach (Path child in JointDrawCanvas.Children) {
                
                if (child.Name.Contains("VisNode"))
                {
                    Rect rect = child.Data.Bounds;
                    Double midX = rect.X + rect.Width / 2;
                    Point point = new Point(midX, p.Y);

                    if (child.Data.FillContains(point))
                    {
                        Double midY = rect.Y + rect.Height / 2;
                        Point bp1 = new Point();
                        Point bp2 = new Point();

                        if (p.X < rect.X)
                        {
                            bp1 = new Point(0, midY);
                            bp2 = new Point(midX, midY);
                        } else
                        {
                            bp1 = new Point(midX, midY);
                            bp2 = new Point(JointDrawCanvas.ActualWidth, midY);
                        }
                        

                        //Rect hitIBRec = new Rect(0, midY - 3, JointDrawCanvas.ActualWidth, 6);
                        //RectangleGeometry hitIB = new RectangleGeometry(hitIBRec);
                        LineGeometry beamLine = new LineGeometry(bp1, bp2);

                        toDraw.Children.Add(beamLine);
                        //toDraw.Children.Add(hitIB);
                        toDrawPath.Data = toDraw;
                        toDrawPath.Name = "VisBeam";
                        return toDrawPath;
                    }
                }

            }

            return null;

        }

        public (GeometryGroup, GeometryGroup) Draw_Column(Point p)
        {

            GeometryGroup gg = new GeometryGroup();
            GeometryGroup ggHit = new GeometryGroup();

            foreach (Path child in JointDrawCanvas.Children)
            {
                if (child.Name.Contains("VisNode"))
                {
                    Rect rect = child.Data.Bounds;
                    Double midY = rect.Y + rect.Height / 2;
                    Point point = new Point(p.X, midY);

                    if (child.Data.FillContains(point))
                    {
                        Double midX = rect.X + rect.Width/ 2;
                        Point bp1 = new Point();
                        Point bp2 = new Point();

                        if (p.Y < rect.Y)
                        {
                            bp1 = new Point(midX, 0);
                            bp2 = new Point(midX, midY);
                        }
                        else
                        {
                            bp1 = new Point(midX, midY);
                            bp2 = new Point(midX, JointDrawCanvas.ActualHeight);
                        }

                        //Rect hitIBRec = new Rect(0, midY - 3, JointDrawCanvas.ActualWidth, 6);
                        //RectangleGeometry hitIB = new RectangleGeometry(hitIBRec);
                        LineGeometry beamLine = new LineGeometry(bp1, bp2);
                        gg.Children.Add(beamLine);

                        //toDraw.Children.Add(hitIB);

                        //---------Create hitbox for column----------
                        int rectW = 10;

                        Rect hitRec = new Rect(bp1.X - (rectW/2), bp1.Y, rectW, (bp2.Y - bp1.Y));
                        Console.WriteLine(hitRec.TopLeft.ToString() + " " + hitRec.Height.ToString());
                        ggHit.Children.Add(new RectangleGeometry(hitRec));

                        return (gg, ggHit);
                    }
                }

            }

            return (null, null);

        }


        public void Delete_Element(Point p)
        {
            
           /* Path boxPath = new Path();
            GeometryCollection boxCollection = new GeometryCollection();
            boxPath.Stroke = clear;
            boxPath.Fill = clear;

            foreach (Path child in JointDrawCanvas.Children)
            {
                Rect box = child.Data.Bounds;
                RectangleGeometry boxGom = new RectangleGeometry(box);
                boxCollection.Add(boxGom);
            }

            boxPath.Data = boxCollection;
            JointDrawCanvas.Children.Add(boxPath);



            foreach (Path child in JointDrawCanvas.Children)
            { 


                if (boxPath.IsMouseOver)
                {
                    JointDrawCanvas.Children.Remove(child);
                }

                JointDrawCanvas.Children.Remove(boxPath);


            } */
        }


        public (GeometryGroup,GeometryGroup) Create_Node(Point p)
        {

            GeometryGroup gg = new GeometryGroup();
            GeometryGroup ggHit = new GeometryGroup();

            //-----Create visible section to add to canvas--------

            int lineLen = 10;
            //Rect hitINRec = new Rect(p.X -lineLen/2, p.Y - lineLen / 2, lineLen, lineLen);
            EllipseGeometry circ = new EllipseGeometry(p, lineLen / 2, lineLen / 2);

            Point vp1 = new Point(p.X, p.Y - lineLen);
            Point vp2 = new Point(p.X, p.Y + lineLen);
            Point hp1 = new Point(p.X - lineLen, p.Y);
            Point hp2 = new Point(p.X + lineLen, p.Y);

            LineGeometry vert = new LineGeometry(vp1, vp2);
            LineGeometry horz = new LineGeometry(hp1, hp2);

            gg.Children.Add(circ);
            gg.Children.Add(vert);
            gg.Children.Add(horz);

            //-----Create invisible hitbox-----------

            EllipseGeometry hitCirc = new EllipseGeometry(p, lineLen, lineLen);
            ggHit.Children.Add(hitCirc);

            return (gg, ggHit);

        }

        


        public void Canvas_Click(object sender, RoutedEventArgs e)
        {
            Point p = Mouse.GetPosition(JointDrawCanvas);            

            Line angle = new Line();

            Path toDrawPath = new Path();
            Path toDrawHit = new Path();

            Console.WriteLine(currentCursor);

            switch (currentCursor)
            {
                case "InsNode":

                    var nodePaths = Create_Node(p);

                    toDrawPath.Data = nodePaths.Item1;
                    toDrawHit.Data = nodePaths.Item2;

                    toDrawPath.Name = String.Concat("VisNode", nodeNum);
                    toDrawHit.Name = String.Concat("HitNode", nodeNum);
                    nodeNum++;

                    break;

                case "InsBeam":

                    toDrawPath = Draw_Beam(p);
                    if (toDrawPath == null)
                    {
                        Console.WriteLine("Beam not near a Node");
                    }
                    else
                    {
                        toDrawPath.Name = String.Concat("VisBeam", beamNum);
                        toDrawHit.Name = String.Concat("HitBeam", beamNum);
                        beamNum++;
                    }
                    /* else
                     {
                         toDrawPath.Stroke = black;
                     } */

                    //Rect hitIBRec = new Rect(0, p.Y-3, JointDrawCanvas.ActualWidth, 6);
                    //RectangleGeometry hitIB = new RectangleGeometry(hitIBRec);
                    //Point bp1 = new Point(0, p.Y);
                    //Point bp2 = new Point(JointDrawCanvas.ActualWidth, p.Y);
                    //LineGeometry beamLine = new LineGeometry(bp1, bp2);
                    //toDraw.Children.Add(beamLine);
                    //toDraw.Children.Add(hitIB);
                    //toDrawPath.Data = toDraw;
                    //toDrawPath.Name = "InsBeam";

                    break;

                case "InsCol":

                    var colPaths = Draw_Column(p);

                    toDrawPath.Data = colPaths.Item1;
                    toDrawHit.Data = colPaths.Item2;

                    if (toDrawPath == null)
                    {
                        Console.WriteLine("Column not near a Node");
                    }
                    else
                    {
                        toDrawPath.Name = String.Concat("VisCol", colNum);
                        toDrawHit.Name = String.Concat("HitCol", colNum);
                        colNum++;
                    }
                    /*else
                    {
                        toDrawPath.Stroke = black;
                    } */
                    

                    break;

                case "InsAngEle":

                    toDrawPath.Name = "InsAngleEle";

                    break;

                case "DeleteEle":


                    Delete_Element(p);

                    
                    break;

                case "AlignEle":

                    break;
            }


            if (toDrawPath != null)
            {

                toDrawPath.Fill = clear;
                toDrawPath.StrokeThickness = 1;

                toDrawHit.Fill = clear;
                toDrawHit.StrokeThickness = 1;

                JointDrawCanvas.Children.Add(toDrawPath);
                JointDrawCanvas.Children.Add(toDrawHit);

            }

        }
    }
}
