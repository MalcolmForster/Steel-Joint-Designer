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

        public void Change_Cursor(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            String cursor = button.Name; // Enables to correct cursor as was pressed

            switch(cursor)
            {
                case "InsNode":     
                    cursor = "InsNode";
                    break;
                case "InsBeam":     
                    cursor = "InsBeam";
                    break;
                case "InsCol":      
                    cursor = "InsCol";
                    break;
                case "InsAngEle":   
                    cursor = "InsAngEle";
                    break;
                case "DeleteEle":   
                    cursor = "DeleteEle";
                    break;
                case "AlignEle":    
                    cursor = "AlignEle";
                    break;
            }



          /*  if (button.Name == "InsNode") {
            
            }
            if (button.Name == "InsBeam") {
            
            }
            if (button.Name == "InsCol") {
            
            }
            if (button.Name == "InsAngEle") {
            
            }
            if (button.Name == "DeleteEle") {
            
            }
            if (button.Name == "AlignEle") {
            
            } */


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

        public void Canvas_Click(object sender, RoutedEventArgs e)
        {

            Point p = Mouse.GetPosition(JointDrawCanvas);
            SolidColorBrush fillColor = new SolidColorBrush(Color.FromRgb(0,255,0));

            Ellipse dot = new Ellipse();
            dot.Fill = fillColor;
            dot.Width = 5;
            dot.Height = 5;

            JointDrawCanvas.Children.Add(dot);
            Canvas.SetLeft(dot, p.X);
            Canvas.SetTop(dot, p.Y);

        }
    }
}
