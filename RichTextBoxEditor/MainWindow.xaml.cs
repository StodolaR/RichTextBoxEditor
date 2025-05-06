using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RichTextBoxEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filePath;
        private bool edited;
        public MainWindow()
        {
            InitializeComponent();
            filePath = string.Empty;
            edited = false;
        }

        private void cbNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void cbNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (edited)
            {
                MessageBoxResult result = MessageBox.Show("Přejete si uložit stávající dokument?", "Uložení dokumentu",
                                                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
            }
            rtbEditor.Document.Blocks.Clear();
            filePath = string.Empty;
            mainWindow.Title = "Bez názvu";
            edited = false;
        }

        private void cbOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute= true;
        }

        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (edited)
            {
                MessageBoxResult result = MessageBox.Show("Přejete si uložit stávající dokument?", "Uložení dokumentu",
                                                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
            }
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "RichTextFile *.rtf|*.rtf";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                    {
                        TextRange allDocument = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                        allDocument.Load(fs, DataFormats.Rtf);
                        filePath = ofd.FileName;
                        mainWindow.Title = System.IO.Path.GetFileName(ofd.FileName);
                        edited = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dokument nelze načíst" + Environment.NewLine + ex);
                }
            }
        }

        private void cbSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void cbSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save();
        }
        private void Save()
        {
            if (filePath != string.Empty)
            {
                SaveFile(filePath, false);
                MessageBox.Show("Soubor byl uložen");
            }
            else
            {
                SaveAs();
            }
        }
        private void cbSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void cbSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }
        private void SaveAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "RichTextFile *.rtf|*.rtf";
            if (sfd.ShowDialog() == true)
            {
                SaveFile(sfd.FileName, true);
                filePath = sfd.FileName;
            }
        }
        private void SaveFile(string path, bool newFile)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    TextRange allDocument = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    allDocument.Save(fs, DataFormats.Rtf);
                    if (newFile)
                    {
                        mainWindow.Title = System.IO.Path.GetFileName(path);
                    }
                    edited = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dokument nelze uložit" + Environment.NewLine + ex);
            }
        }

        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            edited = true;
        }
    }
}