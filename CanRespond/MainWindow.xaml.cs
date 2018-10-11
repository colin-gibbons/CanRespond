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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Microsoft.VisualBasic;
using CanRespond.classes;
using System.Xml.Serialization;

namespace CanRespond
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window{

        Responses responses = new Responses();

        public MainWindow(){
            InitializeComponent();

            if (!File.Exists(Properties.Resources.xmlPath))
            {
                WriteXML();
            }

            LoadXML();

            FillTitleList();
        }

        private void LoadXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Responses));
            FileStream fileStream = new FileStream(Properties.Resources.xmlPath, FileMode.Open);

            responses = (Responses)serializer.Deserialize(fileStream);
        }

        private void WriteXML()
        {
 
            XmlSerializer mySerializer = new XmlSerializer(typeof(Responses));

            StreamWriter myWriter = new StreamWriter(Properties.Resources.xmlPath);
            mySerializer.Serialize(myWriter, responses);
            myWriter.Close();

        }

        private void FillTitleList()
        {
            TitleList.Items.Clear();

            foreach (Response response in responses.ResponseList)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = response.Title
                };

                TitleList.Items.Add(item);
            }


        }

        private void ResponseBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Doubled clicked ResponseBox");
            // Toggle readOnly mode
            ResponseBox.IsReadOnly = !ResponseBox.IsReadOnly;

            // Save Changes upon exiting readOnly mode
            if (ResponseBox.IsReadOnly)
            {
                //set Background Color
                SolidColorBrush brush = new SolidColorBrush();
                Color color = (Color)ColorConverter.ConvertFromString("#FF99B4D1");
                brush.Color = color;
                ResponseBox.Background = brush;

                // Save changes to list
                Debug.WriteLine("Saved Content");
                ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;
                string title = selectedItem.Content.ToString();

                responses.GetResponse(title).Content = ResponseBox.Text;
            }
            else
            {
                ResponseBox.Background = Brushes.NavajoWhite;
            }
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            string tempTitle = "New Response";
            string tempContent = "";

            //XmlNode newNode = responses.CreateElement("response");


            ListBoxItem item = new ListBoxItem
            {
                Content = "New Response"
            };

            TitleList.Items.Add(item);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;
            string title = selectedItem.Content.ToString();

            Response response = responses.GetResponse(title);

            if (response != null)
            {
                responses.ResponseList.Remove(response);
            }

            ResponseBox.Text = "";
            FillTitleList();
        }

        private void TitleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: move read only mode to function
            ResponseBox.IsReadOnly = true;
            // UPDATE CHANGES
            if (e.RemovedItems.Count > 0)
            {
                ListBoxItem prevSel = (ListBoxItem)e.RemovedItems[0];
                Debug.WriteLine(prevSel.Content.ToString());
            }
                ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;
            
            if (selectedItem != null)
            {
                string title = selectedItem.Content.ToString();

                Response response = responses.GetResponse(title);

                if (response != null)
                {
                    ResponseBox.Text = response.Content;
                }
            }
        }

        private void TitleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(ResponseBox.Text);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ResponseBox.Text);
        }
    }
}
