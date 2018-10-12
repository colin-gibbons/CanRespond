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

        Responses Responses = new Responses();

        // TODO: make this the first default response
        string UsageInstructions = "Quick-Start Guide:" + Environment.NewLine + "Double click on a Response title to copy it to the clipboard." + Environment.NewLine + "Double-Click in this content area to enter edit mode";

        public MainWindow(){
            InitializeComponent();

            if (!File.Exists(Properties.Resources.xmlPath))
            {
                WriteXML();
            }

            LoadXML();

            FillTitleList();

            ContentBox.Text = UsageInstructions;
        }

        // Loads XML from file using serializeable class Responses
        private void LoadXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Responses));
            FileStream fileStream = new FileStream(Properties.Resources.xmlPath, FileMode.Open);

            Responses = (Responses)serializer.Deserialize(fileStream);
        }

        // Writes XML to file using serializeable class Responses
        private void WriteXML()
        {
 
            XmlSerializer mySerializer = new XmlSerializer(typeof(Responses));

            StreamWriter myWriter = new StreamWriter(Properties.Resources.xmlPath);
            mySerializer.Serialize(myWriter, Responses);
            myWriter.Close();

        }

        // Fills the TitleList ListBox with Response Titles
        private void FillTitleList()
        {
            TitleList.Items.Clear();

            foreach (Response response in Responses.ResponseList)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = response.Title
                };

                TitleList.Items.Add(item);
            }
        }

        // Toggles content editing mode on/off
        private void ToggleContentBoxEdit(bool saveChanges = true)
        {
            // Toggle ReadOnly mode
            ContentBox.IsReadOnly = !ContentBox.IsReadOnly;

            if (ContentBox.IsReadOnly) // User exited Edit mode, so save data to list:
            {
                ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;

                if (selectedItem != null)
                {
                    string title = selectedItem.Content.ToString();

                    Responses.GetResponse(title).Content = ContentBox.Text;

                    if (saveChanges)
                    {
                        WriteXML(); // save changes to file

                        UpdateStatus("Changes saved.");
                    }

                    // reset status bar color
                    StatBar.Background = getBrushFromHex("#FF007ACC");
                }
            }
            else // is in edit mode
            {
                UpdateStatus("Editing...");

                StatBar.Background = getBrushFromHex("#ca5100");
            }
        }

        private void UpdateStatus(string status)
        {
            // TODO: Make bar flash?
            StatusText.Content = status;
        }

        private SolidColorBrush getBrushFromHex(string hex)
        {
            SolidColorBrush brush = new SolidColorBrush();
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            brush.Color = color;
            return brush;
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

            Responses.ResponseList.Add(newItem);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;

            if (selectedItem != null)
            {
                string title = selectedItem.Content.ToString();

                Response response = Responses.GetResponse(title);

                // confirm user wants to delete
                MessageBoxResult confirm = MessageBox.Show("\"" + title + "\" will be deleted.", "Delete Response", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (response != null && confirm == MessageBoxResult.OK)
                {
                    Responses.ResponseList.Remove(response);
                }

                ContentBox.Text = "";
                FillTitleList();
            }
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

                Response response = Responses.GetResponse(title);

                if (response != null)
                {
                    ContentBox.Text = response.Content;
                }

                UpdateStatus("Double-Click text to edit.");
            }
        }

        private void TitleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(ContentBox.Text);
            StatusText.Content = "Text Copied!";        
        }

        private bool IsEditingTitle()
        {
            return false; // TODO: finish
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (!IsEditingTitle())
            {
                ListBoxItem selectedItem = (ListBoxItem)TitleList.SelectedItem;

                if (selectedItem != null)
                {
                    string title = selectedItem.Content.ToString();

                    TextBox temp = new TextBox();
                    temp.Text = title;
                    temp.Width = TitleList.Width - 1;
                    temp.KeyDown += new KeyEventHandler(TempBox_KeyDown);

                    // save previous title
                    temp.Resources.Add("PreviousTitle", title);

                    int i = TitleList.Items.IndexOf(selectedItem);
                    TitleList.Items.RemoveAt(i);
                    TitleList.Items.Insert(i, temp);

                    temp.Focus();
                    temp.SelectAll();
                }
            }
        }

        // TextBox used for editing Response Title
        // TODO: rename
        private void TempBox_KeyDown(object sender, KeyEventArgs e)
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
                        Content = temp.Text
                    };

                    string previousTitle = (string) temp.Resources["PreviousTitle"];

                    Responses.GetResponse(previousTitle).Title = temp.Text;

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
