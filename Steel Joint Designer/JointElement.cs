using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Steel_Joint_Designer
{
    public class JointElement
    {
        public double width;
        public double height;
        public double flangeTHK;
        public double webTHK;
        public double radius = 10.0/1000;
        public string eleName;
        public string eleDes;
        public double shear;
        public double moment;
        public double UTS;
        public double yield;
        public double weldUTS;
        public double weldYield;

        Dictionary<string, List<int>> elementWeldInfo = new Dictionary<string, List<int>>();
        IEnumerable<TextBox> tbs;

        string s;
        string jT;

        public void makeElement(string elementName, string jointType, IEnumerable<TextBox> textBoxes, string[] materialData, string eleDesignation, string[] weldMetalData)
        {
            eleName = elementName;
            eleDes = eleDesignation;
            int num = int.Parse(elementName.Substring(elementName.Length - 1));
            UTS = double.Parse(materialData[2]);
            yield = double.Parse(materialData[1]);
            weldUTS = double.Parse(weldMetalData[2]);
            weldYield = double.Parse(weldMetalData[1]);
            jT = jointType;
            tbs = textBoxes;


            foreach (TextBox tb in textBoxes)
            {

                if (tb.Name.Contains("FlangeWidth"))
                {
                    s = tb.Text;
                    width = Convert.ToDouble(s) / 1000;
                }
                if (tb.Name.Contains("Height"))
                {
                    s = tb.Text;
                    height = Convert.ToDouble(s) / 1000;
                }
                if (tb.Name.Contains("FlangeThickness"))
                {
                    s = tb.Text;
                    flangeTHK = Convert.ToDouble(s) / 1000;
                }
                if (tb.Name.Contains("WebThickness"))
                {
                    s = tb.Text;
                    webTHK = Convert.ToDouble(s) / 1000;
                }
                if (tb.Name.Contains("Moment"))
                {
                    s = tb.Text;
                    moment = Convert.ToDouble(s);
                }
                if (tb.Name.Contains("Shear"))
                {
                    s = tb.Text;
                    shear = Convert.ToDouble(s);
                }

            }

        }

        public void makeWelds(string weldName, List<int> weldSize)
        {
            elementWeldInfo.Add(weldName, weldSize);
        }

        public Dictionary<string, double[]> weldData() {

            Dictionary<string, double[]> elementWeldInfoTest = new Dictionary<string, double[]>();

            if (jT == "MEP" || jT == "WM") {

                Dictionary<string, double[]> elementWeldInfo = WeldSizer.WMjointCalc(this.AllData(), UTS, yield, weldUTS, weldYield);

                double[] topFlangeWeld = elementWeldInfo["Flange"];
                double[] bottomFlangeWeld = elementWeldInfo["Flange"];         

                elementWeldInfoTest.Add("TopFlange", topFlangeWeld);
                elementWeldInfoTest.Add("BottomFlange", bottomFlangeWeld);
                elementWeldInfoTest.Add("Web", null);                

            }

            return elementWeldInfoTest;

        }

        public double[] endPlate() {

            if (jT == "WM")
            {
                return null;
            }
            else
            {                //To add calculation to this section, at the moment just a ratio to the size of the beams and columns
                double[] epSize = { width * 1.2, height * 1.6, flangeTHK * 1.0 };
                return epSize;
            }
  
        }


        public double[] gusset()
        {
            //To add calculation to this section, at the moment just a ratio to the size of the beams and columns atm its [length, height, thickness]
                double[] gussSize = { width * 0.5, height * 0.2, flangeTHK * 1.0 };
                return gussSize;            
        }





        public double[] AllData()
        {
            double[] r = new double[7] { width, height, flangeTHK, webTHK, radius, moment, shear};
            return r;
        }


        public double[] boltHoles(double[] epSize, JointElement jE)
        {
            if (jT == "MEP")
            {
                double numOfHoles = 8.0;
                double holeDia = 20.0 / 1000;
                double dX1 = epSize[0] / 2 - holeDia * 2;
                double dY1 = epSize[1] / 2 - holeDia * 3;
                double dX2 = dX1;
                double dY2 = jE.height / 2 - holeDia * 4;

                //[0] = number of holes, [1] = hole size
                double[] holes = { numOfHoles, holeDia, dX1, dY1, dX2, dY2 };

                return holes;
            } else
            {
                return null;
            }

        }

    }
}
