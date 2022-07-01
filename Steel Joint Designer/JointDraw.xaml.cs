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
using System.Text.RegularExpressions;

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

        Path firstEle = new Path();
        Path secondEle = new Path();

        int nodeNum, beamNum, colNum, angNum = 0; // initialise counters for joint elements, used for naming purposes to link drawn part with invisible hitbox

        public void Change_Cursor(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            currentCursor = button.Name; // Enables to correct cursor as was pressed
            if (currentCursor == "AlignEle")
            {
                firstEle = null;
                secondEle = null;
                Hints.Text = "Select Element to move, alignment can only be done with Node type elements";

            } else
            {
                Hints.Text = "Help messages shown here";
            }
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
                        child.Stroke = blue;
                    }

                }
            }
        }

        public Point findInline(Point firstPoint,double max, Char direction)
        {            
            Rect bnds = new Rect();
            Double xloc = new double();
            Double yloc = new double();

            if (direction == 'L' || direction == 'R')
            {
                Console.WriteLine("Works");
                xloc = max;
                yloc = firstPoint.Y;
            } else
            {
                xloc = firstPoint.X;
                yloc = max;
            }

            foreach (Path child in JointDrawCanvas.Children)
            {
                if (child.Name.Contains("VisNode"))
                {
                    bnds = child.Data.Bounds;
                    double midX = (bnds.X + (bnds.Width) / 2);
                    double midY = (bnds.Y + (bnds.Height) / 2);

                    if (direction == 'L') // Left of current node requires the Y position to be found, and X position created from click (Point p)
                    {                        
                        if (midY == yloc)
                        {
                            if (midX > xloc && midX < firstPoint.X && midX != firstPoint.X)
                                xloc = midX;
                        }
                        //xloc = firstRec.X + (firstRec.Width) / 2;
                    }
                    else if (direction == 'R') // Right of current node requires the Y position to be found, and X position created from click (Point p)
                    {
                        if (midY == yloc)
                        {
                            if (midX < xloc && midX > firstPoint.X && midX != firstPoint.X)
                                xloc = midX;
                        }
                    } else if (direction == 'U') //Up of current node requires the X position to be found, and Y position created from click (Point p)
                    {

                    } else if (direction == 'D')//Down of current node requires the X position to be found, and Y position created from click (Point p)
                    {

                    }
                }
            }
            Point closestNode = new Point(xloc,yloc);
            return closestNode;
        }

        //Draw_Beam and Draw_Column need a method to find the closest node to draw too, currently will draw to the node found first in the foreach loop
        public (GeometryGroup, GeometryGroup) Draw_Beam(Point p)
        {
            GeometryGroup gg = new GeometryGroup();
            GeometryGroup ggHit = new GeometryGroup();
            double shortestDistance = 1000000000;
            Path closestChild = null;

            foreach (Path child in JointDrawCanvas.Children)
            {
                if (child.Name.Contains("VisNode"))
                {
                    Rect bounds = child.Data.Bounds;
                    double xLocation = bounds.X + bounds.Width / 2;
                    Point possiblePoint = new Point(xLocation, p.Y);
                    double distanceFromNode = Math.Abs(p.X - xLocation);

                    if (child.Data.FillContains(possiblePoint) && distanceFromNode < shortestDistance)
                    {
                        closestChild = child;
                        shortestDistance = distanceFromNode;
                    }
                }
            }

            if (closestChild != null)
            {
                Rect rect = closestChild.Data.Bounds;
                double midX = rect.X + rect.Width / 2;
                Point point = new Point(midX, p.Y);
                double distance = Math.Abs(p.X - midX);

                Double midY = rect.Y + rect.Height / 2;
                Point bp1 = new Point();
                Point bp2 = new Point();
                if (p.X < rect.X)
                {
                    //bp1 = new Point(0, midY);
                    bp2 = new Point(midX, midY);
                    bp1 = findInline(bp2, 0, 'L'); //find the closest inline node to the left

                }
                else
                {
                    bp2 = new Point(midX, midY);
                    //bp2 = new Point(JointDrawCanvas.ActualWidth, midY);
                    bp1 = findInline(bp2, JointDrawCanvas.ActualWidth, 'R'); //find the closest inline node to the right
                }
                LineGeometry beamLine = new LineGeometry(bp1, bp2);
                gg.Children.Add(beamLine);
                int rectW = 10;

                Rect hitRec = new Rect(Math.Min(bp1.X, bp2.X), bp1.Y - (rectW / 2), Math.Abs((bp2.X - bp1.X)), rectW);
                ggHit.Children.Add(new RectangleGeometry(hitRec));
                return (gg, ggHit);

            }
            
            return (null, null);
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

                        LineGeometry columnLine = new LineGeometry(bp1, bp2);
                        gg.Children.Add(columnLine);

                        //---------Create hitbox for column----------
                        int rectW = 10;
                        Rect hitRec = new Rect(bp1.X - (rectW/2), bp1.Y, rectW, (bp2.Y - bp1.Y));
                        ggHit.Children.Add(new RectangleGeometry(hitRec));

                        return (gg, ggHit);
                    }
                }
            }
            return (null, null);
        }

        public Path childSearch(Canvas canvas, String childName)
        {
            foreach(Path child in canvas.Children)
            {
                if (child.Name == childName)
                {
                    return child;
                }
            }
            return null;
        }

        public void Remove_Sister_Element(Path child)
        {
            String element = child.Name;
            if (element.Contains("Hit"))
            {
                JointDrawCanvas.Children.Remove(childSearch(JointDrawCanvas, element.Replace("Hit", "Vis")));
                Console.WriteLine(element.Replace("Hit", "Vis"));
            }
            else
            {
                JointDrawCanvas.Children.Remove(childSearch(JointDrawCanvas, element.Replace("Vis", "Hit")));
                Console.WriteLine(element.Replace("Vis", "Hit"));
            }
        }


        public void Delete_Element(Point p)
        {      
            foreach (Path child in JointDrawCanvas.Children)
            {
                if (child.IsMouseOver)
                {
                    String toDelete = child.Name;
                    JointDrawCanvas.Children.Remove(child);
                    Console.WriteLine(child.Name);
                    Remove_Sister_Element(child);
                    break;
                }
            }
        }
        public (GeometryGroup, GeometryGroup) Create_Node(Point p)
        {

            GeometryGroup gg = new GeometryGroup();
            GeometryGroup ggHit = new GeometryGroup();

            //-----Create visible section to add to canvas--------

            int lineLen = 10;
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

        public void Horz_Align(object sender, RoutedEventArgs e)
        {
            Align_Element_Perform(0);
            AlignDir.IsOpen = false;
        }

        public void Vert_Align(object sender, RoutedEventArgs e)
        {
            Align_Element_Perform(1);
            AlignDir.IsOpen = false;
        }

        public void Cancel_Align(object sender, RoutedEventArgs e)
        {
            AlignDir.IsOpen = false;
            firstEle = null;
            secondEle = null;
            Hints.Text = "Alignment Cancelled";
        }

        public void Ele_To_Canvas(Path toDrawPath, Path toDrawHit)
        {
            toDrawPath.Fill = clear;
            toDrawPath.StrokeThickness = 1;

            toDrawHit.Fill = clear;
            toDrawHit.StrokeThickness = 1;

            JointDrawCanvas.Children.Add(toDrawPath);
            JointDrawCanvas.Children.Add(toDrawHit);
        }

        public Path Align_Element_Select(Point p)
        {
            foreach (Path child in JointDrawCanvas.Children)
            {
                if (child.IsMouseOver)
                {
                    if (child.Name.Contains("Node")){ //<------ this ensures the only element type to be aligned is the Node type element
                        return child;
                    } else
                    {
                        Hints.Text = "Element is not a node, please try again";
                    }
                    
                }
            }
            Hints.Text = "Element not correct, try again";
            return null;
        }

        public void Align_Element_Perform(int n)
        {
            Path toDrawPath = new Path();
            Path toDrawHit = new Path();

            double xloc = 0;
            double yloc = 0;

            Rect firstRec = firstEle.Data.Bounds;
            Rect secondRec = secondEle.Data.Bounds;
            if (n == 0) //<--- can make use of new find inline method possibly?
            {
                xloc = firstRec.X + (firstRec.Width) / 2;
                yloc = secondRec.Y + (secondRec.Height)/2;
            } else if (n == 1)
            {
                xloc = secondRec.X + (secondRec.Width) / 2;
                yloc = firstRec.Y + (firstRec.Height) / 2;
            }

            Point p = new Point(xloc, yloc);
            String eleToMove = firstEle.Name;
            String eleNum = Regex.Replace(eleToMove, "[^0-9.]", "");
            JointDrawCanvas.Children.Remove(firstEle);
            Remove_Sister_Element(firstEle);

            if (eleToMove.Contains("Node"))
            {
                var paths = Create_Node(p);

                toDrawPath.Data = paths.Item1;
                toDrawHit.Data = paths.Item2;

                toDrawPath.Name = String.Concat("VisNode", eleNum);
                toDrawHit.Name = String.Concat("HitNode", eleNum);
            }
            Ele_To_Canvas(toDrawPath,toDrawHit);
            Hints.Text = "Elements Aligned";
        }

        public void Canvas_Click(object sender, RoutedEventArgs e)
        {
            Point p = Mouse.GetPosition(JointDrawCanvas);
            Line angle = new Line();
            Path toDrawPath = new Path();
            Path toDrawHit = new Path();

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
                    var beamPaths = Draw_Beam(p);

                    toDrawPath.Data = beamPaths.Item1;
                    toDrawHit.Data = beamPaths.Item2;


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

                    break;

                case "InsAngEle":

                    toDrawPath.Name = "InsAngleEle";
                    break;

                case "DeleteEle":

                    Delete_Element(p);                    
                    break;

                case "AlignEle":
                    if (firstEle == null)
                    {
                        firstEle = Align_Element_Select(p);
                        if (firstEle != null) {
                            Hints.Text = "Select the second element to align with";
                        }                        
                    }
                    else if (secondEle == null)
                    {
                        secondEle = Align_Element_Select(p);
                        if (secondEle != null)
                        {
                            if (firstEle == null) {
                                Hints.Text = "Sorry, something went wrong, please try again";
                                firstEle = null;
                                secondEle = null;
                            } else
                            {
                                AlignDir.IsOpen = true;                                
                                
                                //secondEle.Name;

                            }
                        }
                    }
                    break;
            }
            if (toDrawPath != null)
            {
                Ele_To_Canvas(toDrawPath, toDrawHit);
            }
        }
    }
}
