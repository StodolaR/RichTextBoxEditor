using Microsoft.Win32;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        // MemuItem Soubor
        // MenuItem Novy
        private void cbNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CancelAskSaveDocument())
            {
                return;
            }
            rtbEditor.Document.Blocks.Clear();
            if(filePath != string.Empty)
            {
                AddToLastDocs();
                filePath = string.Empty;
            }           
            mainWindow.Title = "Bez názvu";
            edited = false;
        }
        private bool CancelAskSaveDocument()
        {
            if (edited)
            {
                MessageBoxResult result = MessageBox.Show("Přejete si uložit stávající dokument?", "Uložení dokumentu",
                                                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
                return result == MessageBoxResult.Cancel;
            }
            return false;
        }

        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            edited = true;
        }

        private void AddToLastDocs()
        {
            for (int i = 0 ; i < miLastDoc.Items.Count; i++)
            {
                if ((string)((MenuItem)miLastDoc.Items[i]).Header == filePath)
                {
                    MenuItem movedLastDoc = (MenuItem)miLastDoc.Items [i];
                    miLastDoc.Items.RemoveAt(i);
                    miLastDoc.Items.Insert(0, movedLastDoc);
                    return;
                }
            }
            CommandBinding cbOpenLast = new CommandBinding();
            cbOpenLast.Command = OtherCommands.OpenLast;
            cbOpenLast.Executed += cbOpenLast_Executed;
            Image icon = new Image();
            icon.Source = new BitmapImage(new Uri(@"/Resources/Icons/Last.png", UriKind.Relative));
            MenuItem lastDoc = new MenuItem();
            lastDoc.Header = filePath;
            lastDoc.CommandBindings.Add(cbOpenLast);
            lastDoc.Command = OtherCommands.OpenLast;
            lastDoc.Icon = icon;
            miLastDoc.Items.Insert(0, lastDoc);
        }
        //Menuitem Otevrit
        private void cbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CancelAskSaveDocument())
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "RichTextFile *.rtf|*.rtf";
            if (ofd.ShowDialog() == true)
            {
                OpenFile(ofd.FileName);
            }
        }
        private void OpenFile(string openFilePath)
        {
            try
            {
                using (FileStream fs = new FileStream(openFilePath, FileMode.Open))
                {
                    TextRange allDocument = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                    allDocument.Load(fs, DataFormats.Rtf);
                    if (filePath != string.Empty && filePath != openFilePath)
                    {
                        AddToLastDocs();
                    }
                    filePath = openFilePath;
                    mainWindow.Title = System.IO.Path.GetFileName(openFilePath);
                    edited = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dokument nelze načíst" + Environment.NewLine + ex);
            }
        }
        //MenuItem Posledni dokumenty
        private void cbOpenLast_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CancelAskSaveDocument())
            {
                return;
            }
            string openFilePath = (string)((MenuItem)sender).Header;
            OpenFile(openFilePath);
        }
        //MenuItem Ulozit
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
        //MenuItem Ulozit jako
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
        //MenuItem Vlastnosti
        private void cbProperties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string filename = string.Empty;
            string path = string.Empty;
            string size = string.Empty;
            string createDate = string.Empty;
            if (filePath != string.Empty)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    filename = fileInfo.Name;
                    if (fileInfo.DirectoryName != null)
                    {
                        path = fileInfo.DirectoryName;
                    }
                    size = fileInfo.Length.ToString();
                    createDate = fileInfo.CreationTime.ToString();
                }
                catch { }
            }
            MessageBox.Show($"Název souboru: {filename}" + Environment.NewLine
                          + $"Umístění          : {path}" + Environment.NewLine
                          + $"Velikost            : {size}bytů" + Environment.NewLine
                          + $"Vytvořen          : {createDate}" + Environment.NewLine);
        }
        //MenuItem Konec
        private void cbClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CancelAskSaveDocument())
            {
                return;
            }
            mainWindow.Close();
        }

        //MenuItem Upravy
        //MenuItem Zpet
        private void cbUndo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (rtbEditor != null)
            {
                e.CanExecute = rtbEditor.CanUndo;
            }
        }

        private void cbUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Undo();
        }

        private void rtbEditor_KeyUp(object sender, KeyEventArgs e)
        {
            rtbEditor.Undo();
            rtbEditor.Redo();
        }
        //MenuItem Vpred
        private void cbRedo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (rtbEditor != null)
            {
                e.CanExecute = rtbEditor.CanRedo;
            }
        }

        private void cbRedo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Redo();
        }
        //MenuItem Najdi
        private void cbFind_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            spFindAndReplace.Visibility = Visibility.Visible;
            btnStartFind.Visibility = Visibility.Visible;
        }

        private void btnStartFind_Click(object sender, RoutedEventArgs e)
        {
            FindOrReplacePattern(false);
        }

        private void FindOrReplacePattern(bool replace)
        {
            TextRange allText = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            allText.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
            if (string.IsNullOrWhiteSpace(allText.Text) || string.IsNullOrWhiteSpace(tbxFind.Text))
            {
                tbStatus.Text = "Zadna shoda";
            }
            else
            {
                tbStatus.Text = $"Pocet shod: {Regex.Matches(allText.Text, tbxFind.Text).Count}";
                for (TextPointer matchBeginPointer = rtbEditor.Document.ContentStart;
                    matchBeginPointer.CompareTo(rtbEditor.Document.ContentEnd) < 0;
                    matchBeginPointer = matchBeginPointer.GetNextContextPosition(LogicalDirection.Forward))
                {
                    int matchBeginOffset;
                    do
                    {
                        string textAfterStartMatchPointer = matchBeginPointer.GetTextInRun(LogicalDirection.Forward);
                        matchBeginOffset = textAfterStartMatchPointer.IndexOf(tbxFind.Text);
                        if (matchBeginOffset >= 0)
                        {
                            matchBeginPointer = matchBeginPointer.GetPositionAtOffset(matchBeginOffset);
                            TextRange matchRange = new TextRange(matchBeginPointer, matchBeginPointer.GetPositionAtOffset(tbxFind.Text.Length));
                            if (replace)
                            {
                                matchRange.Text = tbxReplace.Text;
                            }                          
                            matchRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
                        }
                    } while (matchBeginOffset >= 0 && replace);
                }
            }

        }

        private void btnCloseFind_Click(object sender, RoutedEventArgs e)
        {
            spFindAndReplace.Visibility=Visibility.Collapsed;
            tbReplace.Visibility=Visibility.Collapsed;
            tbxReplace.Visibility = Visibility.Collapsed;
            btnStartFind.Visibility=Visibility.Collapsed;
            btnStartReplace.Visibility = Visibility.Collapsed;
        }
        //MenuItem Zamen
        private void cbReplace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            spFindAndReplace.Visibility = Visibility.Visible;
            tbReplace.Visibility = Visibility.Visible;
            tbxReplace.Visibility = Visibility.Visible;
            btnStartReplace.Visibility=Visibility.Visible;
        }

        private void btnStartReplace_Click(object sender, RoutedEventArgs e)
        {
            FindOrReplacePattern(true);
        }
    }
}