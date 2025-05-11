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
        private bool firstEdit;
        
        public MainWindow()
        {
            InitializeComponent();
            filePath = string.Empty;
            edited = false;
            firstEdit = true;
            cbFFamily.ItemsSource = Fonts.SystemFontFamilies;
            cbFSize.ItemsSource = new double[] { 8, 10, 12, 16, 20, 24, 32, 40, 48 };
            rtbEditor.Focus();
            ActualizeButtonsStates();
        }

        // MemuItem Soubor

        // MenuItem Novy
        private void CbNew_Executed(object sender, ExecutedRoutedEventArgs e)
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
        private void RtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            edited = true;
            ActualizeButtonsStates();
        }
        private void ActualizeButtonsStates()
        {
            object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            if (temp == DependencyProperty.UnsetValue)
            {
                cbFFamily.Text = string.Empty;
            }
            else
            {
                cbFFamily.Text = temp.ToString();
            }
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            if (temp == DependencyProperty.UnsetValue)
            {
                cbFSize.Text = string.Empty;
            }
            else
            {
                cbFSize.Text = temp.ToString();
            }
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = miBold.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(FontWeights.Bold);
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = miItalic.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(FontStyles.Italic);
            temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = miUnderline.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(TextDecorations.Underline);
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
            cbOpenLast.Executed += CbOpenLast_Executed;
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
        private void CbOpen_Executed(object sender, ExecutedRoutedEventArgs e)
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
        private void CbOpenLast_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CancelAskSaveDocument())
            {
                return;
            }
            string openFilePath = (string)((MenuItem)sender).Header;
            OpenFile(openFilePath);
        }

        //MenuItem Ulozit
        private void CbSave_Executed(object sender, ExecutedRoutedEventArgs e)
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
        private void CbSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
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
        private void CbProperties_Executed(object sender, ExecutedRoutedEventArgs e)
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
        private void CbClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CancelAskSaveDocument())
            {
                return;
            }
            mainWindow.Close();
        }

        //MenuItem Upravy

        //MenuItem Zpet
        private void CbUndo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (rtbEditor != null)
            {
                e.CanExecute = rtbEditor.CanUndo;
            }
        }
        private void CbUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Undo();
        }
        private void RtbEditor_KeyUp(object sender, KeyEventArgs e)
        {
            //TODO Undo Redo
            //rtbEditor.Undo();
            //rtbEditor.Redo();
        }

        //MenuItem Vpred
        private void CbRedo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (rtbEditor != null)
            {
                e.CanExecute = rtbEditor.CanRedo;
            }
        }
        private void CbRedo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            rtbEditor.Redo();
        }

        //MenuItem Najit
        private void CbFind_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            spFindAndReplace.Visibility = Visibility.Visible;
            btnStartFind.Visibility = Visibility.Visible;
            btnStartReplace.Visibility = Visibility.Collapsed;
            tbxReplace.Visibility = Visibility.Collapsed;
            tbxReplace.Text = string.Empty;
            tbReplace.Visibility = Visibility.Collapsed;
        }
        private void BtnStartFind_Click(object sender, RoutedEventArgs e)
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
                tbStatus.Text = $"  Pocet shod: {Regex.Matches(allText.Text, tbxFind.Text).Count}  ";
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
        private void BtnCloseFind_Click(object sender, RoutedEventArgs e)
        {
            CloseFindAndReplace();
        }
        private void CloseFindAndReplace()
        {
            spFindAndReplace.Visibility = Visibility.Collapsed;
            tbReplace.Visibility = Visibility.Collapsed;
            tbxReplace.Visibility = Visibility.Collapsed;
            btnStartFind.Visibility = Visibility.Collapsed;
            btnStartReplace.Visibility = Visibility.Collapsed;
            tbxFind.Text = string.Empty;
            tbxReplace.Text = string.Empty;
            tbStatus.Text = string.Empty;
        }
        private void RtbEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            TextRange allText = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            allText.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
        }

        //MenuItem Nahradit
        private void CbReplace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            spFindAndReplace.Visibility = Visibility.Visible;
            tbReplace.Visibility = Visibility.Visible;
            tbxReplace.Visibility = Visibility.Visible;
            btnStartReplace.Visibility=Visibility.Visible;
            btnStartFind.Visibility=Visibility.Collapsed; 
        }
        private void BtnStartReplace_Click(object sender, RoutedEventArgs e)
        {
            FindOrReplacePattern(true);
        }
        //Najit a nahradit - Oznaceni textu v textboxech pri ziskani focusu
        private void TextBox_LostMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
            textBox.LostMouseCapture -= TextBox_LostMouseCapture;
        }
        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.LostMouseCapture += TextBox_LostMouseCapture;
        }

        //MenuItem Format

        //Menuitem Pismo
        //Menuitem Tucne
        private void rtbEditor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!firstEdit) return;
            Run run = new Run(e.Text, rtbEditor.CaretPosition);
            rtbEditor.BeginChange();
            if (btnBold.IsChecked == true)
            {
                run.FontWeight = FontWeights.Bold;
            }
            if (btnItalic.IsChecked == true)
            {
                run.FontStyle = FontStyles.Italic;
            }
            if (btnUnderline.IsChecked == true)
            {
                run.TextDecorations = TextDecorations.Underline;
            }
            if (cbFFamily.SelectedItem != null)
            {
                rtbEditor.FontFamily = (FontFamily)cbFFamily.SelectedItem;
            }
            if (double.TryParse(cbFSize.Text, out double value))
            {
                rtbEditor.FontSize = value;
            }
            rtbEditor.EndChange();
            e.Handled = true;
            rtbEditor.CaretPosition = run.ElementEnd;
            firstEdit = false;
        }
        private void miBold_Click(object sender, RoutedEventArgs e)
        {
            btnBold.IsChecked = miBold.IsChecked;
        }
        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            if(btnBold.IsChecked != null)
            {
                miBold.IsChecked = (bool)btnBold.IsChecked;
            }
        }

        //MenuItem Kurziva
        private void miItalic_Click(object sender, RoutedEventArgs e)
        {
            btnItalic.IsChecked = miItalic.IsChecked;
        }
        private void btnItalic_Click(object sender, RoutedEventArgs e)
        {
            if(btnItalic.IsChecked != null)
            {
                miItalic.IsChecked= (bool)btnItalic.IsChecked;
            }
        }

        //MenuItem Podtrzene
        private void miUnderline_Click(object sender, RoutedEventArgs e)
        {
            btnUnderline.IsChecked = miUnderline.IsChecked;
        }
        private void btnUnderline_Click(object sender, RoutedEventArgs e)
        {
            if (btnUnderline.IsChecked != null)
            {
                miUnderline.IsChecked= (bool)btnUnderline.IsChecked;
            }
        }

        //Combobox FontFamily
        private void cbFFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(FontFamilyProperty, cbFFamily.SelectedItem);
            rtbEditor.Focus();
        }

        //Combobox FontSize
        private void cbFSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(FontSizeProperty, cbFSize.SelectedItem);
            rtbEditor.Focus();
        }

        private void cbFSize_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (double.TryParse(cbFSize.Text, out double value))
                {
                    rtbEditor.Selection.ApplyPropertyValue(FontSizeProperty, value);
                }
                rtbEditor.Focus();
            }
        }        
    }
}