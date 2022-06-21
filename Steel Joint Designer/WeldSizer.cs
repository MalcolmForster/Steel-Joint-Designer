using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Steel_Joint_Designer
{
    internal class WeldSizer
    {

        /*
         * This class takes the joint style, element information, steel grades, and calculated the weld sizes
         * 
         * 
         * ------------Returned information---------------
         * welds are passed to the JointElement class and attached to the object. Return methods from the JointElement are to be designed.
         * 
         * 
         * ----Weld types and information required--------
         * Fillet welded, FW, samples are given by a single number, leg length (mm).
         * Partial penetration butt welds, PPBW, are given by two numbers, a length (mm), and angle (degrees)
         * Full penetration butt welds have no information, just stated as a FPBW
         * 
         * 
         */
        public static Dictionary<string, double[]> WMjointCalc(double[] elementSize, double UTS, double yield, double wUTS, double wYield)
        {

            //Need to sort out how many members are involved in the joint and their locations
            Dictionary<string, double[]> result = new Dictionary<string, double[]>();

            double weldYield;
            double weldUTS;
            double elementYield;
            double elementUTS; 

            // ----------- ID, Yield (MPa), UTS (MPa) --------------------------------                

            //Getting required information for calculations of the samples
            //1. Find what beams and columns are selected
            //2. In a loop set all information for each element
            //3. If welds are required run calculation, can ignore columns that are the same
            //4. use a look to calculate their welds    

            //Data from AllData() is double [] { width, height, flangeTHK, webTHK, radius, moment, shear};

            double elementWidth = elementSize[0];
            double elementHeight = elementSize[1];
            double elementFthk = elementSize[2];
            double elementWthk = elementSize[3];
            double elementR = elementSize[4];
            double beamShear = elementSize[5];
            double beamMoment = elementSize[6];

            weldYield = wYield;
            weldUTS = wUTS;

            elementYield= yield;
            elementUTS = UTS;

            double ratio = 0;

            string calcTypeErr = "";

            /*

            if (getComboBoxText(combo, "WMcalculationType1") == "Old")
            {
                ratio = 0.6;
            }
            else if (getComboBoxText(combo, "WMcalculationType1") == "New")
            {
                ratio = 1.0;
            }
            else
            {
                calcTypeErr = "calculationType was not set";
            } */

            ratio = 0.6;

            double weldLength = (elementWidth * 2) - (2 * elementWthk); // need to get radius of beam
            double weldDemand = (elementWidth / 2) + (beamMoment / (elementHeight - elementFthk)); // need beam moment and shear

            double weldThroat = weldDemand / (0.8 * ratio * weldUTS * weldLength);

            int weldFilletLegSize = (int)Math.Ceiling(weldThroat * Math.Sqrt(2));

            string weldSizeOutPut = calcTypeErr + "Using ratio of " + ratio.ToString() + " fillet weld leg size is " + weldFilletLegSize.ToString() + " mm";

            result.Add("Flange",new double[]{0,weldFilletLegSize});
            
            //filletSize.Text = weldSizeOutPut;

            return result;
        }
    }
}
