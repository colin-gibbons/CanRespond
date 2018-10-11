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

        bool isEditing = false;
        string prevTitle = "";

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

        private void ToggleContentBoxEdit()
        {
            // Toggle ReadOnly mode
            ContentBox.IsReadOnly = !ContentBox.IsReadOnly;

            if (ContentBox.IsReadOnly) // User exited Edit mode, so save data to list:
            {
                ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;
                string title = selectedItem.Content.ToString();

                responses.GetResponse(title).Content = ContentBox.Text;

                WriteXML(); // save changes to file

                StatusText.Content = "Changes saved!";
            }
            else // is in edit mode
            {
                StatusText.Content = "Editing...";
                statBar.Background = Brushes.Orange;
            }
        }

        // // // // // // // // // 
        // Event Listener Code  //
        // // // // // // // // //

        private void ContentBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToggleContentBoxEdit();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = new ListBoxItem
            {
                Content = "New Response" //TODO: move to properties xml
            };

            TitleList.Items.Add(item);

            Response newItem = new Response();
            newItem.Title = "New Response";
            newItem.Content = "";

            responses.ResponseList.Add(newItem);
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

            ContentBox.Text = "";
            FillTitleList();
        }

        private void TitleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: move read only mode to function
            ContentBox.IsReadOnly = true;

            // UPDATE CHANGES
            ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;
            
            if (selectedItem != null)
            {
                string title = selectedItem.Content.ToString();

                Response response = responses.GetResponse(title);

                if (response != null)
                {
                    ContentBox.Text = response.Content;
                }
            }
        }

        private void TitleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(ContentBox.Text);
            StatusText.Content = "Text Copied";        
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (!isEditing)
            {
                ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;
                string title = selectedItem.Content.ToString();

                // global var stores previous title
                prevTitle = title;

                TextBox temp = new TextBox();
                temp.Text = title;
                temp.Width = TitleList.Width - 1;
                temp.KeyDown += new KeyEventHandler(tempBox_KeyDown);

                int i = TitleList.Items.IndexOf(selectedItem);
                TitleList.Items.RemoveAt(i);
                TitleList.Items.Insert(i, temp);

                temp.Focus();
                temp.SelectAll();
            }
        }

        // TextBox used for editing Response Title
        // TODO: rename
        private void tempBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Escape)
            {
                int i = -1;
                TextBox temp = null;

                foreach (object listItem in TitleList.Items)
                {
                    if (listItem is TextBox)
                    {
                        temp = (TextBox)listItem;
                        i = TitleList.Items.IndexOf(listItem);
                        break;
                    }
                }

                if (temp != null)
                {
                    ListBoxItem item = new ListBoxItem
                    {
                        Content = temp.Text //TODO: move to properties xml
                    };

                    // TODO: Remove global variable
                    responses.GetResponse(prevTitle).Title = temp.Text;
                    prevTitle = "";

                    TitleList.Items.RemoveAt(i);
                    TitleList.Items.Insert(i, item);
                }
            }
        }

        private void ContentBox_KeyDown(object sender, KeyEventArgs e)
        {
            // TODO: make esc exit edit mode
        }
    }
}
