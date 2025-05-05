using Microsoft.Win32;
using System.IO;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void cbNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void cbNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Přejete si uložit stávající dokument?");
            rtbEditor.Document.Blocks.Clear();
        }

        private void cbOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute= true;
        }

        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Přejete si uložit stávající dokument?");
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
                        mainWindow.Title = System.IO.Path.GetFileName(ofd.FileName);
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
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "RichTextFile *.rtf|*.rtf";
            if(sfd.ShowDialog() == true)
            {
                try
                {
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        TextRange allDocument = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                        allDocument.Save(fs, DataFormats.Rtf);
                        mainWindow.Title = System.IO.Path.GetFileName(sfd.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dokument nelze uložit" + Environment.NewLine + ex);
                }
            }
        }
    }
}