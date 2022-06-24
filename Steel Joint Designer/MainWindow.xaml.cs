using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Steel_Joint_Designer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string startup = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string rootFolder = startup;
        static readonly string[] TXTfiles = { "WJDBeam.txt", "WJDColumn.txt", "WJDWeld.txt", "WJDConn.txt", "WJDMatGrade.txt" };
        string selectedCalc = "";

        public MainWindow()
        {

            InitializeComponent();
            PrepareData();

        }

        Dictionary<string, string[][]> TXTedited = new Dictionary<string, string[][]>();

        ItemCollection tabs;

        public void PrepareData()
        {

            bool error = false;
            tabs = connectionTabs.Items;

            //-----------------------AUTOSET INFO FOR TESTING------------------------
            //remove this section when finished testing

            foreach (TabItem tab in tabs)
            {

                Grid content = tab.Content as Grid;

                grid = tab.Content as Grid;

                foreach (Grid gr in grid.Children.OfType<Grid>())
                {
                    foreach (TextBox tb in gr.Children.OfType<TextBox>())
                    {
                        if (tb.Name.Contains("Moment")) tb.Text = "490";
                        if (tb.Name.Contains("Shear")) tb.Text = "103";
                        if (tb.Name.Contains("Width")) tb.Text = "300";
                        if (tb.Name.Contains("Height")) tb.Text = "400";
                        if (tb.Name.Contains("WebThickness")) tb.Text = "10";
                        if (tb.Name.Contains("FlangeThickness")) tb.Text = "14";
                    }


                    foreach (ComboBox cb in gr.Children.OfType<ComboBox>())
                    {
                        if (cb.Name.Contains("materialGrade")) cb.SelectedItem = "300S0";
                        if (cb.Name.Contains("Weld")) cb.SelectedItem = "fabCOR86R";
                        if (cb.Name.Contains("calculationType")) cb.SelectedItem = "New";
                    }

                    foreach (CheckBox cB in gr.Children.OfType<CheckBox>())
                    {
                        if (cB.Name.Contains("columnTickBox2")) cB.IsChecked = true;
                        if (cB.Name.Contains("beamTickBox")) cB.IsChecked = true;
                    }
                }
            }


            //-----------------------END OF AUTOSET INFO FOR TESTING------------------------


            foreach (string TXTfile in TXTfiles)
            {
                if (File.Exists(startup + TXTfile))
                {
                    string[] columnTXT = File.ReadAllLines(startup + TXTfile);
                }
                else
                {
                    error = true;
                    errorMessage.Text = TXTfile + " File NOT Found in program bin";
                    break;
                }
            }

            if (error == true)
            {

            }
            else
            {

                string[] beamTXTraw = File.ReadAllLines(TXTfiles[0]);
                string[] columnTXTraw = File.ReadAllLines(TXTfiles[1]);
                string[] weldTXTraw = File.ReadAllLines(TXTfiles[2]);
                string[] connTXTraw = File.ReadAllLines(TXTfiles[3]);
                string[] matGradeTXTraw = File.ReadAllLines(TXTfiles[4]);

                Dictionary<string, string[]> TXTraw = new Dictionary<string, string[]>()
                {
                    {"beamInfo", beamTXTraw }, {"columnInfo", columnTXTraw }, {"weldInfo", weldTXTraw }, {"connInfo", connTXTraw}, {"matGradeInfo", matGradeTXTraw}
                };

                Dictionary<string, string[]> headings = new Dictionary<string, string[]>();

                foreach (var TXTfile in TXTraw)
                {

                    int b = 0;
                    int l = TXTfile.Value.Length;
                    string[][] lines = new string[l - 2][];
                    string[] headingArray = new string[l - 2];

                    for (int i = 2; i < l; i++)
                    {

                        string[] line = TXTfile.Value[i].Split(',').Select(p => p.Trim()).ToArray();
                        lines[b] = line;
                        headingArray[b] = line[0];
                        b++;

                    }

                    TXTedited.Add(TXTfile.Key, lines);
                    headings.Add(TXTfile.Key, headingArray);

                }

                string[] calcTypes = new string[] { "Old", "New" };

                string[][] beamMatInfo = TXTedited["matGradeInfo"];
                string[][] columnMatInfo = TXTedited["matGradeInfo"];
                string[][] weldMatInfo = TXTedited["weldInfo"];

                List<ComboBox> cboxes = new List<ComboBox>();

                

                foreach (TabItem tb in tabs)
                {                    
                    grid = tb.Content as Grid;
                    foreach (Grid gr in grid.Children.OfType<Grid>())
                        //content = gr.Content as Grid;
                        foreach (ComboBox cb in gr.Children.OfType<ComboBox>())
                            cboxes.Add(cb);
                }

                foreach (ComboBox cb in cboxes)
                {
                    if (cb.Name.Contains("beamCombo")) cb.ItemsSource = headings["beamInfo"];
                    if (cb.Name.Contains("columnCombo")) cb.ItemsSource = headings["columnInfo"];
                    if (cb.Name.Contains("GradeBeam")) cb.ItemsSource = headings["matGradeInfo"];
                    if (cb.Name.Contains("GradeColumn")) cb.ItemsSource = headings["matGradeInfo"];
                    if (cb.Name.Contains("Weld")) cb.ItemsSource = headings["weldInfo"];
                    if (cb.Name.Contains("calculationType")) cb.ItemsSource = calcTypes;
                }


            }




        }

        public void fillFromCombo(string setToFill, int number, string selected)
        {

            /* These are going to get complicated
             * Using find child methods to get the desired textboxes seems to be the common way
             * Pass in which beam is required to be filled in (as there could be more than 1 for more complex joints)
             * First off find which tab is active, then look through it for the text box
             * - Require using the tab and beam number to find name
             * 
             */

            //setToFill is the combobox that has been changed, this input is required to direct the information to the correct textboxes

            tab = connectionTabs.SelectedItem as TabItem;
            //content = tab.Content as Grid;

            List<TextBox> toFill = new List<TextBox>();

            grid = tab.Content as Grid;

            foreach (Grid gr in grid.Children.OfType<Grid>())
                foreach (TextBox tb in gr.Children.OfType<TextBox>())
                    if (tb.Name.Contains(setToFill) && tb.Name.Contains(number.ToString()))
                    {
                        toFill.Add(tb);
                    }


            List<string[]> elementData = new List<string[]>();

            if (setToFill.Contains("beam"))
            {
                elementData.Add(findInTXT(selected, "beamInfo"));
            }
            else if (setToFill.Contains("column"))
            {
                elementData.Add(findInTXT(selected, "columnInfo"));
            }
            else if (setToFill.Contains("weld"))
            {
                elementData.Add(findInTXT(selected, "weldInfo"));
            }

            foreach (string[] row in elementData)
            {
                if (row[0] == selected)
                {
                    foreach (TextBox tb in toFill)
                    {
                        if (tb.Name.Contains("FlangeWidth")) tb.Text = row[2];
                        if (tb.Name.Contains("FlangeThickness")) tb.Text = row[3];
                        if (tb.Name.Contains("Height")) tb.Text = row[1];
                        if (tb.Name.Contains("WebThickness")) tb.Text = row[4];
                    }
                }
            }

        }

        private void getComboInfo(Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            string selectedElement = (string)combo.SelectedValue;

            string comboName = combo.Name;

            int id = comboName.Last() - '0';
            string name = comboName.Remove(comboName.Length - 1,1).Replace("ComboBox", "");

            fillFromCombo(name, id, selectedElement);
        }

        public static int arrayfinder(string query, string[][] array2query)
        {
            int num2ret = 69;
            int l = array2query.Length;

            for (int i = 0; i < l; i++)
            {

                if (Equals(array2query[i][0], query))
                {
                    num2ret = i;
                    break;
                }

            }

            return num2ret;

        }

        TabItem tab;
        Grid content;
        Grid grid;

        public void Clear_Click(object sender, RoutedEventArgs e)
        {
            tab = connectionTabs.SelectedItem as TabItem;
            content = tab.Content as Grid;

            grid = tab.Content as Grid;
            foreach (Grid gr in grid.Children.OfType<Grid>())

                foreach (TextBox tb in content.Children.OfType<TextBox>())
                tb.Text = string.Empty;
            foreach (ComboBox cb in content.Children.OfType<ComboBox>())
                cb.Text = string.Empty;
        }

        public string[] findInTXT(string lookFor, string inWhat)
        {

            return TXTedited[inWhat][arrayfinder(lookFor, TXTedited[inWhat])];
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {

            tab = connectionTabs.SelectedItem as TabItem;
            grid = tab.Content as Grid;


            /*-----------V0.2 update-------------------
             * 
             *  Emplimenting objectiying all required elements when Submit_Click is used
             *  All textbox, combobox, material information stored in JointElement classes
             *  Send as all information in one array or list including all JointElements, calculation type, new or old style of calculation etc.
             *
             *
             *
             *  Steps:
             *  Find all grids selected by the user
             *  For each grid make JointElement object and fill set with newElement
            --------------------------------------------*/



            //Finds grid that have had checkbox selected ie beam/column requested by engineer/detailer

            List<Grid> checkedGrids = new List<Grid>();

            string[] weldMetalData = new string[3];
            string calcType = "";

            foreach (Grid gr in grid.Children.OfType<Grid>())
            {
                CheckBox checkBox = gr.Children.OfType<CheckBox>().FirstOrDefault<CheckBox>();
                if (checkBox != null && checkBox.IsChecked == true)
                {
                    checkedGrids.Add(gr);
                } else if (gr.Name.Contains("Other"))
                {
                    foreach (ComboBox cb in gr.Children.OfType<ComboBox>())
                    {
                        if (cb.Name.Contains("materialWeld"))
                        {
                            weldMetalData = findInTXT(cb.Text, "weldInfo");
                        }

                        if (cb.Name.Contains("Type"))
                        {
                            calcType = cb.Text;
                        }
                    }
                }

            }

            //Element needs to

            selectedCalc = tab.Name;
            Dictionary<String, JointElement> jointEles = new Dictionary<String, JointElement>();


            foreach (Grid gr in checkedGrids)
            {
                string name = gr.Name;

                    String elementName = gr.Name.Replace("grid", "");

                    IEnumerable<ComboBox> comboInGrid = gr.Children.OfType<ComboBox>();
                    IEnumerable<TextBox> textBoxesInGrid = gr.Children.OfType<TextBox>();

                    string comboName, materialGrade;
                    string eleDesignation = "Element designation not found";
                    string[] materialData = new string[3];

                    foreach (ComboBox cb in comboInGrid)
                    {
                        
                        comboName = cb.Name;
                        materialGrade = cb.Text;

                        if (comboName.Contains("ComboBox")) eleDesignation = cb.Text;
                        if (comboName.Contains("materialGradeColumn") || comboName.Contains("materialGradeBeam"))
                        {
                            materialData = findInTXT(materialGrade, "matGradeInfo");
                        }


                    }

                    JointElement newJointElement = new JointElement();
                    newJointElement.makeElement(elementName, selectedCalc, textBoxesInGrid, materialData, eleDesignation, weldMetalData);
                    jointEles.Add(elementName, newJointElement);

            }

            WeldGroupStrength wgsWin = new WeldGroupStrength(selectedCalc, jointEles, weldMetalData, calcType);
            wgsWin.Show();

        }

        public void Open_Draw_Window()
        {
            List<string> tabHeaders = new List<string>();

            foreach (TabItem tab in tabs){
                tabHeaders.Add(tab.Header.ToString());
            }
            
            JointDraw jointDraw = new JointDraw(tabHeaders);
            jointDraw.Show();
        }

        public void Draw_Current_Joint(object sender, RoutedEventArgs e)
        {
            Open_Draw_Window();
        }

        public void Draw_New_Joint(object sender, RoutedEventArgs e)
        {
            Open_Draw_Window();
        }

    }
}
