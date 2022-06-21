using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    //Calculation for the fillet weld group from 1997 3404 method:
    /*

    The methods use tuple to return the variety of information required to create the output of a fillet weld group.
    This will be split into a function to calculate size of fillet weld required, PPBW and pure BW, will add costing as well.

    Different joint types are to be added in certain ways, different information to be returned back
    -tuple size will need to expand and shink in relation to the joint type

    INFORMATION TO SEND TO CALCULATION ASAP
    -Beam: Size, type, grade
    -Column: Size, type, grade
    -Weld: material, grade, strength
    -Joint type: Moment resisting, flange --> look at what is available in SCNZ extract
    -Type of construction: Rigid, semi-regid, simple

    INFORMATION TO BE CALCULATED/FOUND
    -weld group appropirate factor phi? 9.7.3.10

    FUTURE UPDATE
    -Forces and moments on joint as a check
    -OLD and NEW techinque

    OTHER STUFF TO CONSIDER
    -Alternative methods --> consider fillet weld group to satisfy equilibrium between fillet weld
    group and elements of the connected member 9.8.1.2.1 3404

    Joint rotates around instantaneous centre of rotation

    */

    /*

    STEEL FRAMED JOINTS
    *- WM: Welded Moment
    *- BPP: Column BasePlate
    *- MEPS-E8 Moment end plate splice


    */

    public partial class WeldGroupStrength : Window
    {

        /*--------joint style and designation------

    0    MEP, Moment End Plate
    1    WM, Welded Moment
    2    BPP, Column Baseplates Pinned 
    3    WP-NC, Web Plate No Cope
    4    WP-DWC, Web Plate Double Web Cope 
    5    BC, Bolted Column

        -------------------*/
        string connectionType = "";

        public WeldGroupStrength(string connType, Dictionary<String, JointElement> elements, string[] weldMetalData, string calcType)
        {
            InitializeComponent();

            connectionType = connType;
            string[] keys = elements.Keys.ToArray();
            bool stiffenersReq = false;


            Dictionary<String, JointElement> weldedElements = new Dictionary<String, JointElement>();

            //Need to drop elements that do not require welds here

            List<string> columnKeys = new List<string>();

            foreach (string key in keys)
            {
                if (key.Contains("Column"))
                {
                    columnKeys.Add(key);
                }
                else
                {
                    weldedElements.Add(key, elements[key]);
                }

            }

            //elements[key].eleDes
            List<string> uniqueColumns = new List<string>();

            List<string> columnDesesFound = new List<string>();

            foreach (string columnKey in columnKeys)
            {
                if (!columnDesesFound.Contains(elements[columnKey].eleDes))
                {
                    uniqueColumns.Add(columnKey);
                    columnDesesFound.Add(elements[columnKey].eleDes);
                }
                else
                {
                    uniqueColumns.Remove(columnKey);
                }
            }

            stiffenersReq = true;
            JointElement uniqueColumn = new JointElement();

            if (uniqueColumns.Count > 1)
            {
                foreach (string key in uniqueColumns) weldedElements.Add(key, elements[key]);
            } else
            {
                uniqueColumn = elements[uniqueColumns[0]];
            }

            //Adding weld information to the elements dictionary. First need to retrieve weld sizes
            //For I-Beam need welds for top flange, bottom flange and web.
            //State which weld type are used

            string[] weldedKeys = weldedElements.Keys.ToArray();

            int gridEleW = 500;
            int gridEleH = 1000;
            int eleCount = weldedElements.Count;
            int winW = (int)(gridEleW * eleCount * 1.5);
            int winH = gridEleH;

            //Dictionary<string, double> WMJointResults = new Dictionary<string, double>();
            //WMJointResults = WMjoint.WMjointCalc(text, WMelements);

            //Resize window to suit Welded Moment
            windowResize(winW, winH);

            //Adding grids to the results grid in the window already using the gridSetter and canvasSetter method

            gridSetter(winW, winH, resultsGrid, weldedKeys);
            canvasSetter(resultsGrid);

            //Drawing the columns, beams, welds and exporting other outputs into window
            drawJoint(weldedElements, uniqueColumn);
        }


        /*---------------DRAWING SECTION--------------------------------
         * This section is for the methods which are used to draw the output of the calcultion
         * They include I beam, Welds
        */

        //I have made the drawIBeam completely open to custom sizes at the risk of making it more messy, but more practical
        double rWidth, rHeight, rFlangeTHK, rWebTHK, rRadius, rWeldleg, rWeldAngle, r;

        public void eleRatio(Canvas dCanvas, JointElement dEle)
        {
            double dWidth;
            double dHeight;
            double dThick;

            if (connectionType == "MEP")
            {
                double[] mep = dEle.endPlate();
                dWidth = mep[0];
                dHeight = mep[1];
                dThick = mep[2];
            }
            else
            {
                dWidth = dEle.width;
                dHeight = dEle.height;
            }

            //finding a ratio to ensure that the element will fit onto the canas
            r = ratio((int)dCanvas.Width, (int)dCanvas.Height, dWidth, dHeight);

        }

        public void drawIBeam(Canvas dCanvas, JointElement dEle)
        {
            double dWidth = dEle.width;
            double dHeight = dEle.height;
            double dFlangeTHK = dEle.flangeTHK;
            double dWebTHK = dEle.webTHK;
            double dRadius = dEle.radius; //not yet implemented into mainwindow so ignored currently

            GeometryGroup geometry = new GeometryGroup();
            Brush brush = Brushes.LightGray;

            //finding a ratio to ensure that the element will fit onto the canas
            //r = ratio((int)dCanvas.Width, (int)dCanvas.Height, dWidth, dHeight);

            //finding line lengths by multiplying inputs by the ratio            

            rWidth = r * dWidth;
            rHeight = r * dHeight;
            rFlangeTHK = r * dFlangeTHK;
            rWebTHK = r * dWebTHK;
            rRadius = r * dRadius;

            int startLocX = (int)((dCanvas.Width - rWidth) / 2);
            int startLocY = (int)((dCanvas.Height - rHeight) / 2);

            Rect[] iBeamParts = new Rect[3];


            //Calculated lengths
            double rWebHeight = rHeight - 2 * rFlangeTHK;
            int[] startWeb = { (int)(startLocX + (rWidth / 2) - (rWebTHK / 2)), (int)(startLocY + rFlangeTHK) };
            int[] startBotFlange = { (int)(startLocX), (int)(startLocY + rHeight - rFlangeTHK) };

            Rect topFlange = new Rect(startLocX, startLocY, rWidth, rFlangeTHK);
            Rect web = new Rect(startWeb[0], startWeb[1], rWebTHK, rWebHeight);
            Rect botFlange = new Rect(startBotFlange[0], startBotFlange[1], rWidth, rFlangeTHK);

            iBeamParts[0] = topFlange;
            iBeamParts[1] = web;
            iBeamParts[2] = botFlange;

            foreach (Rect rect in iBeamParts) geometry.Children.Add(new RectangleGeometry(rect));

            drawImage(geometry, dCanvas, brush);

        }



        public void drawWeld(Canvas dCanvas, Dictionary<string, double[]> weldGeom)
        {
            foreach (KeyValuePair<string, double[]> kcp in weldGeom)
            {

                //inital layout for required info
                string weldLoc = kcp.Key;
                double[] weldSize = kcp.Value;

                double X = dCanvas.Width / 2;
                double Y = dCanvas.Height / 2;

                //finds type of weld using length or array and sets penetration of rectangle into the element

                double pen = 0;
                double weldTHK, weldLen, dX, dY, iWeldLen;
                double halfFlange = rFlangeTHK / 2;
                double halfWeb = (rWebTHK / 2); //wasrounded
                Brush brush = null;
                Rect[] weld2Draw;

                GeometryGroup geometry = new GeometryGroup();

                if (weldSize != null) // if NOT FPBW
                {
                    weldTHK = (weldSize[1]/1000) * r;
                    weldLen = rWidth;
                    dX = rWidth / 2;
                    dY = (rHeight / 2);
                    iWeldLen = (rWidth / 2) - halfWeb - rRadius;
                    weld2Draw = new Rect[3];
                    Rect oW = new Rect(); // outside weld
                    Rect iW1 = new Rect(); // inside weld 1
                    Rect iW2 = new Rect(); // inside weld 2

                    if (weldSize[0] == 0) // if is pure fillet welded
                    {
                        brush = Brushes.Orange;

                    }
                    else // if weld is PPBW
                    {
                        pen = 2;
                        brush = Brushes.Green;
                    }

                    //find mid point of the flage and therefore the weld location as well
                    if (weldLoc == "TopFlange")
                    {
                        oW = new Rect(X - dX, Y - dY - weldTHK + pen, weldLen, weldTHK);
                        iW1 = new Rect(X - dX, Y - dY + rFlangeTHK - pen, iWeldLen, weldTHK);
                        iW2 = new Rect(X + halfWeb + rRadius, Y - dY + rFlangeTHK - pen, iWeldLen, weldTHK);

                    }
                    else if (weldLoc == "BottomFlange")
                    {

                        oW = new Rect(X - dX, Y + dY - pen, weldLen, weldTHK);
                        iW1 = new Rect(X - dX, Y + dY - rFlangeTHK - weldTHK + pen, iWeldLen, weldTHK);
                        iW2 = new Rect(X + halfWeb + rRadius, Y + dY - rFlangeTHK - weldTHK + pen, iWeldLen, weldTHK);
                    }
                    else if (weldLoc == "Web")
                    {

                        oW = new Rect(X - weldTHK - halfWeb + pen, Y - dY + rFlangeTHK + rRadius, weldTHK, rHeight - 2 * rFlangeTHK - 2 * rRadius);
                        iW1 = new Rect(X + halfWeb - pen, Y - dY + rFlangeTHK + rRadius, weldTHK, rHeight - 2 * rFlangeTHK - 2 * rRadius);

                    }

                    geometry.Children.Add(new RectangleGeometry(oW));
                    geometry.Children.Add(new RectangleGeometry(iW1));
                    geometry.Children.Add(new RectangleGeometry(iW2));

                }

                else // if is FPBW
                {
                    weld2Draw = new Rect[1];
                    Rect fullPWeld = new Rect();

                    dX = rWidth / 2;
                    dY = rHeight / 2;
                    weldTHK = rWebTHK;
                    weldLen = rWidth;
                    brush = Brushes.Purple;

                    if (weldLoc == "TopFlange")
                    {
                        fullPWeld = new Rect(X - dX, Y - dY, weldLen, weldTHK);

                    }
                    else if (weldLoc == "BottomFlange")
                    {
                        fullPWeld = new Rect(X - dX, Y + dY - rFlangeTHK, weldLen, weldTHK);
                    }
                    else if (weldLoc == "Web")
                    {
                        fullPWeld = new Rect(X - halfWeb, Y - dY + rFlangeTHK + rRadius, weldTHK, rHeight - 2 * rFlangeTHK - 2 * rRadius);
                    }

                    geometry.Children.Add(new RectangleGeometry(fullPWeld));

                }

                drawImage(geometry, dCanvas, brush);

                //draws the weld in the canvas dCanvas                 

            }

        }


        public void drawImage(GeometryGroup gg, Canvas canvas, Brush brush)
        {

            var path = new Path
            {
                Data = gg,
                Fill = brush,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            /*foreach (PathFigure data in rects) {
                var path = new Path
                {
                    Data = new PathGeometry(data.),
                    Fill = brush
                }; */
            canvas.Children.Add(path);

        }


        public void drawEndPlates(Canvas dCanvas, double[] epSize)
        {

            double X = dCanvas.Width / 2;
            double Y = dCanvas.Height / 2;
            double w = r * epSize[0];
            double h = r * epSize[1];

            Rect endPlate = new Rect(X - (w / 2), Y - (h / 2), w, h);
            GeometryGroup gg = new GeometryGroup();
            gg.Children.Add(new RectangleGeometry(endPlate));
            Brush brush = Brushes.Gray;

            drawImage(gg, dCanvas, brush);

        }
        public void drawBolts(Canvas dCanvas, double[] boltHoles, double[] epSize)
        {

            double X = dCanvas.Width / 2;
            double Y = dCanvas.Height / 2;
            double dX1 = r * boltHoles[2];
            double dY1 = r * boltHoles[3];
            double dX2 = r * boltHoles[4];
            double dY2 = r * boltHoles[5];
            double dia = r * boltHoles[1];
            //GeometryGroup gg = new GeometryGroup();
            Brush brush = Brushes.Black;

            int[][] ar = new int[][] { new int[] { 1, -1 }, new int[] { -1, -1 }, new int[] { 1, 1 }, new int[] { -1, 1 } };


            for (int i = 0; i < ar.Length; i++)
            {
                Ellipse boltHole = new Ellipse()
                {
                    Width = r * boltHoles[1],
                    Height = r * boltHoles[1],
                    Fill = brush
                };

                dCanvas.Children.Add(boltHole);
                boltHole.SetValue(Canvas.LeftProperty, X + (ar[i][0]) * dX1 - (dia / 2));
                boltHole.SetValue(Canvas.TopProperty, Y + (ar[i][1]) * dY1 - (dia / 2));

            }

            int h = (int)boltHoles[0];

            if (h > 4)
            {
                for (int i = 0; i < h - 4; i++)
                {
                    Ellipse boltHole = new Ellipse()
                    {
                        Width = r * boltHoles[1],
                        Height = r * boltHoles[1],
                        Fill = brush
                    };

                    dCanvas.Children.Add(boltHole);
                    boltHole.SetValue(Canvas.LeftProperty, X + (ar[i][0]) * dX2 - (dia / 2));
                    boltHole.SetValue(Canvas.TopProperty, Y + (ar[i][1]) * dY2 - (dia / 2));

                }

            }

            //drawImage(gg, dCanvas, brush);
        }


        public void drawGussets(Canvas dCanvas, Dictionary<string, double[]> dictionary)
        {

        }

        public void drawStiffeners(Canvas dCanvas, Dictionary<string, double[]> dictionary)
        {

        }

        public void drawEleMethod(Canvas canvas, JointElement jE)
        {
            Dictionary<string, double[]> weldGeom = jE.weldData();
            double[] epGeom = jE.endPlate();
            double[] boltGeom = jE.boltHoles(epGeom, jE);

            eleRatio(canvas, jE);
            if (connectionType == "MEP")
            {
                drawEndPlates(canvas, epGeom);
                drawBolts(canvas, boltGeom, epGeom);
            }

            drawIBeam(canvas, jE);
            drawWeld(canvas, weldGeom);
            
        }

        public void drawSideView(Canvas canvas, Dictionary<String, JointElement> jEd, String[] keys, JointElement uniqueC)
        {

            //r is almost randomaly chosen from a unique columnb so will probably need to be adjusted
            r = (canvas.Width/uniqueC.width)/7;
            drawSideEles(canvas, jEd, keys, uniqueC);

        }

        public void drawSideEles(Canvas canvas, Dictionary<String, JointElement> jEd, String[] keys, JointElement uniqueC)
        {

            int X = (int)(canvas.Width / 2);
            int Y = (int)(canvas.Height / 2);

            Brush eleFill = Brushes.LightGray;
            Brush eleHL = Brushes.DarkGray; //highlight colour for elements such as flanges etc
            List<JointElement> beams = new List<JointElement>();

            foreach (String k in keys)
            {
                if (k.Contains("Beam"))
                {
                    beams.Add(jEd[k]);
                }
            }

            

            GeometryGroup sideWebs = new GeometryGroup();
            GeometryGroup sideFlanges = new GeometryGroup();
            GeometryGroup endPlates = new GeometryGroup();
            GeometryGroup gussets = new GeometryGroup();
            GeometryGroup gussetsFlipped = new GeometryGroup();

            GeometryGroup sideWebsB = new GeometryGroup();
            GeometryGroup sideFlangesB = new GeometryGroup();

            if (uniqueC != null)
            {
                //drawing column/columns
                List<JointElement> columns = new List<JointElement>();
                foreach (JointElement joint in jEd.Values)
                {
                    if(joint.eleName.Contains("Column"))
                    {
                        columns.Add(joint);
                    }
                }


                int cL = (int)(canvas.Height * 0.8); //column length
                int cW = (int)(r * uniqueC.height); // column width
                int cFT = (int)(r * uniqueC.flangeTHK); // column flange thickness

                int bL1 = (int)((canvas.Width - cW / 2) * 0.3); //beam length
                int bW1 = (int)(r * beams[0].height); // beam width
                int bFT1 = (int)(r * beams[0].flangeTHK); // beam flange thickness

                int bL2 = (int)((canvas.Width - cW / 2) * 0.3); //beam length
                int bW2 = (int)(r * beams[1].height); // beam width
                int bFT2 = (int)(r * beams[1].flangeTHK); // beam flange thickness



                if (columns.Count > 1)
                {
                    //Draw columns of different sizes and their connections



                } else {


                    Rect cWeb = new Rect((X - (cW / 2)), (Y - (cL / 2)), cW, cL);

                    Rect b1Web = new Rect((X - bL1 - (cW/2)), (Y - (bW1 / 2)), bL1, bW1);

                    Rect b2Web = new Rect((X + (cW / 2)), (Y - (bW2 / 2)), bL2, bW2);

                    Rect cFlange1 = new Rect((X - (cW / 2)), (Y - (cL / 2)), cFT, cL);
                    Rect cFlange2 = new Rect((X + (cW / 2))-cFT, (Y - (cL / 2)), cFT, cL);

                    Rect b1Flange1 = new Rect((X - bL1 - (cW / 2)), (Y - (bW1 / 2)),  bL1, bFT1);                  
                    Rect b1Flange2 = new Rect((X - bL1 - (cW / 2)), (Y + (bW1 / 2)), bL1, bFT1);
                    Rect b2Flange1 = new Rect((X + (cW / 2)), (Y - (bW2 / 2)),  bL2, bFT2);
                    Rect b2Flange2 = new Rect((X + (cW / 2)), (Y + (bW2 / 2)), bL2, bFT2);

                    Rect topStiff = new Rect((X - (cW / 2)), (Y - (bW1 / 2)), cW, bFT1);
                    Rect botStiff = new Rect((X - (cW / 2)), (Y + (bW1 / 2)), cW, bFT1);


                    sideWebs.Children.Add(new RectangleGeometry(b1Web));
                    sideWebs.Children.Add(new RectangleGeometry(b2Web));
                    sideWebs.Children.Add(new RectangleGeometry(cWeb));

                    sideFlanges.Children.Add(new RectangleGeometry(cFlange1));
                    sideFlanges.Children.Add(new RectangleGeometry(cFlange2));
                    sideFlanges.Children.Add(new RectangleGeometry(b1Flange1));
                    sideFlanges.Children.Add(new RectangleGeometry(b1Flange2));
                    sideFlanges.Children.Add(new RectangleGeometry(b2Flange1));
                    sideFlanges.Children.Add(new RectangleGeometry(b2Flange2));
                    sideFlanges.Children.Add(new RectangleGeometry(topStiff));
                    sideFlanges.Children.Add(new RectangleGeometry(botStiff));

                }
                if(connectionType == "MEP")
                {
                    double[] epDims= new double[3];
                    foreach (JointElement je in jEd.Values)
                    {

                        if (je.endPlate() != null) 
                        {
                            Rect ep = new Rect();
                            epDims = je.endPlate();
                            if (je.eleName.Contains("Beam")) {
                                if (je.eleName.Contains("1"))
                                {
                                    ep =new Rect((X - (cW / 2)-(epDims[2]*r)), Y - (r * epDims[1])/2, r * epDims[2] , r * epDims[1]);
                                }
                                else if (je.eleName.Contains("2"))
                                {
                                    ep = new Rect((X + (cW / 2)), Y - (r * epDims[1]) / 2, r * epDims[2], r * epDims[1]);
                                }
                            }
                            endPlates.Children.Add(new RectangleGeometry(ep));                        
                        } 
                    }
                }

                double[] gussDims = new double[3];

                foreach (JointElement je in jEd.Values)
                {
                    if (je.gusset() != null)
                    {
                        int f = 1;
                        if (je.eleName.Contains("2")) {
                            f=-1;
                        }
                        gussDims = je.gusset();

                        double gL = r * gussDims[0];
                        double gH = r * gussDims[1];

                        PathFigure path = new PathFigure();
                        LineSegment line = new LineSegment();
                        PathSegmentCollection pathSegments = new PathSegmentCollection();

                        double cuttoff = r*0.02;

                        Point nextPoint = new Point();

                        Point startPoint = new Point(X - f*((cW / 2) + (cuttoff)), Y - gH - (r * (je.height/2)));

                        if (je.endPlate() != null)
                        {
                           startPoint.Offset(-je.endPlate()[2]*r*f, 0);
                        }


                        path.StartPoint=startPoint;

                        nextPoint = startPoint;

                        double[][] gussetOperations = new double[][] { new double[]{ cuttoff*f, 0 }, new double[] { 0, gH }, new double[] { -gL*f, 0 }, new double[] { 0, -cuttoff } } ;

                        foreach (double[] op in gussetOperations) {

                            nextPoint.Offset(op[0], op[1]);
                            line = new LineSegment();
                            line.Point = nextPoint;
                            pathSegments.Add(line);

                        }

                        line = new LineSegment();
                        line.Point = startPoint;
                        pathSegments.Add(line);

                        path.Segments = pathSegments;


                        PathFigureCollection myPathFigureCollection = new PathFigureCollection();
                        myPathFigureCollection.Add(path);

                        PathGeometry myPathGeometry = new PathGeometry();
                        myPathGeometry.Figures = myPathFigureCollection;
                        
                        var flippedPathGeometry = new PathGeometry();
                        flippedPathGeometry.Figures= myPathFigureCollection;

                        ScaleTransform scaleTransform1 = new ScaleTransform(1.0, -1.0, 0, Y + (r*je.flangeTHK/2));
                        flippedPathGeometry.Transform = scaleTransform1;

                        gussets.Children.Add(myPathGeometry);
                        gussetsFlipped.Children.Add(flippedPathGeometry);


                    }
                    
                }

            } else if (uniqueC == null)
            {

            }

            drawImage(sideWebs, canvas, eleFill);
            drawImage(sideFlanges, canvas, eleHL);
            drawImage(endPlates, canvas, eleHL);
            drawImage(gussets, canvas, eleFill);
            drawImage(gussetsFlipped, canvas, eleFill);

        }


        public void drawJoint(Dictionary<String, JointElement> elements, JointElement uniqueC)
        {

            string[] keys = elements.Keys.ToArray();
            Grid[] grids = resultsGrid.Children.OfType<Grid>().ToArray();


            //this method below draws the side view of the joint showing gussets and stiffereners and members involved
            drawSideView(grids[0].Children.OfType<Canvas>().FirstOrDefault(), elements, keys, uniqueC);


            //this method loop draws bolt holes, welds, Ibeams shapes and endplate of elements
            for (int i = 0; i < elements.Count(); i++)
            {

                JointElement jE = elements[keys[i]];
                //Dictionary<string, double[]> weldGeom = jE.weldData();
                Grid grid = grids[i + 1];
                Canvas canvas = grid.Children.OfType<Canvas>().FirstOrDefault();

                drawEleMethod(canvas, jE);

            }


        }


        public double ratio(int dCanWidth, int dCanHeight, double dEleWidth, double dEleHeight)
        {
            //getting canvas size and finding acceptable ratios for element to ensure they fit.

            double ratio;

            double widthRatio = (dCanWidth - dCanWidth * 0.2) / dEleWidth;
            double heightRatio = (dCanHeight - dCanHeight * 0.2) / dEleHeight;

            ratio = Math.Round(Math.Min(widthRatio, heightRatio), 0);

            return ratio;
        }

        public void windowResize(int w, int h) //resizes the window to the desired size depending on the joint that is selected
        {
            Application.Current.MainWindow = this;
            Application.Current.MainWindow.Height = h;
            Application.Current.MainWindow.Width = w;
        }



        //this method is used to create and set grids of a known name equally into the resultsGrid
        public void gridSetter(int winWidth, int winHeight, Grid where, string[] eleNames)
        {
            int eleCount = eleNames.Length;
            double gridWidth = (winWidth * 0.5) / eleCount;
            double sideViewWidth = (winWidth * 0.4);


            Grid sideViewGrid = new Grid { Name = "sideView", Width = sideViewWidth, Height = winHeight - 50 };
            where.Children.Add(sideViewGrid);
            Grid.SetColumn(sideViewGrid, 1);
            Grid.SetRow(sideViewGrid, 1);


            for (int i = 0; i < eleCount; i++)
            {
                string gridName = eleNames[i];
                Grid grid = new Grid();
                where.Children.Add(grid);
                grid.Width = gridWidth;
                grid.Height = winHeight - 50;
                Grid.SetRow(grid, 1);
                Grid.SetColumn(grid, i + 2);
                grid.Name = gridName;
            }

        }

        //canvasSetter implements canvas features into the desired grids and names them
        //These will later be filled with items drawn by the various other methods like drawIBeam and drawWeld
        public void canvasSetter(Grid where)
        {
            Grid[] toFill = where.Children.Cast<Grid>().ToArray();

            foreach (Grid g in toFill)
            {
                Canvas newCanvas = new Canvas();
                g.Children.Add(newCanvas);
                newCanvas.Width = g.Width;
                newCanvas.Height = g.Height;
                newCanvas.Name = g.Name + "C";
            }


        }

    }

}
