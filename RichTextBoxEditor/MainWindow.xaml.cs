﻿using Microsoft.Win32;
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
        private bool changeAlignBeforeEdit;
        private bool actualizeButtonsBool;
        private SolidColorBrush actualColor;
        private SolidColorBrush markColor;
        public MainWindow()
        {
            InitializeComponent();
            filePath = string.Empty;
            firstEdit = true;
            cbFFamily.ItemsSource = Fonts.SystemFontFamilies;
            cbFSize.ItemsSource = new double[] { 8, 10, 12, 16, 20, 24, 32, 40, 48 };
            ActualizeButtonsByFont();
            List<SolidColorBrush> colors = CreatePalette();
            cbPalette.ItemsSource = colors;
            cbPalette.SelectedIndex = 0;
            actualColor = (SolidColorBrush)cbPalette.SelectedItem;
            markColor = Brushes.Yellow;
            rtbEditor.Focus();
        }
        
        //Události rtbEditoru
        private void RtbEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (changeAlignBeforeEdit)
            {
                changeAlignBeforeEdit = false;
                return;
            }
            if (!firstEdit)
            {
                return;
            }
            if (rtbEditor.CaretPosition.Paragraph != null
                && rtbEditor.CaretPosition.Paragraph.Inlines.Count == 1 
                && rtbEditor.CaretPosition.Paragraph.Inlines.First() is Run
                && ((Run)rtbEditor.CaretPosition.Paragraph.Inlines.First()).Text.Length == 1)
            {
                rtbEditor.Selection.Select(rtbEditor.CaretPosition.Paragraph.ContentStart, rtbEditor.CaretPosition.Paragraph.ContentEnd);
                ActualizeFontByButtons();
                rtbEditor.CaretPosition = rtbEditor.CaretPosition.Paragraph.ContentEnd;
            }           
            firstEdit = false;
        }
        
        private void ActualizeFontByButtons()
        {
            if (btnBold.IsChecked == true)
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
            }
            else
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
            }
            if (btnItalic.IsChecked == true)
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
            }
            else
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
            }
            if (btnUnderline.IsChecked == true)
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
            else
            {
                TextDecorationCollection noUnder = new TextDecorationCollection();
                rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, noUnder);
            }
            if (cbFFamily.SelectedItem != null)
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cbFFamily.SelectedItem);
            }
            if (double.TryParse(cbFSize.Text, out double value))
            {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, value);
            }
            rtbEditor.Selection.ApplyPropertyValue(Inline.ForegroundProperty, actualColor);
        }
        private void RtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            GetStatus();
            if (actualizeButtonsBool)
            {
                ActualizeButtonsByFont();
            }
        }
        private void GetStatus()
        {
            TextRange allText = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            tbChars.Text = (allText.Text.Length - rtbEditor.Document.Blocks.Count * 2).ToString();
            Regex regex = new Regex(@"\b[a-zA-Z0-9]*\b");
            tbWords.Text = (regex.Matches(allText.Text).Count / 2).ToString();
            tbParagraphs.Text = rtbEditor.Document.Blocks.Count.ToString();
        }
        private void ActualizeButtonsByFont() 
        {
            actualizeButtonsBool = true;
            object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cbFFamily.SelectedItem = temp;
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cbFSize.Text = temp.ToString();
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = miBold.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(FontWeights.Bold);
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = miItalic.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(FontStyles.Italic);
            temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (temp != null && temp != DependencyProperty.UnsetValue)
            {
                btnUnderline.IsChecked = miUnderline.IsChecked = ((TextDecorationCollection)temp).Contains(TextDecorations.Underline[0]);
            }
            else
            {
                btnUnderline.IsChecked = miUnderline.IsChecked = false;
            }
            temp = rtbEditor.Selection.GetPropertyValue(Inline.ForegroundProperty);
            if (temp != DependencyProperty.UnsetValue)
            {
                actualColor = (SolidColorBrush)temp;
            }
            if (rtbEditor.CaretPosition.Paragraph != null)
            {
                temp = rtbEditor.CaretPosition.Paragraph.TextAlignment;
                btnALeft.IsChecked = miALeft.IsChecked = temp.Equals(TextAlignment.Left);
                btnACenter.IsChecked = miACenter.IsChecked = temp.Equals(TextAlignment.Center);
                btnARight.IsChecked = miARight.IsChecked = temp.Equals(TextAlignment.Right);
                btnAJustify.IsChecked = miAJustify.IsChecked = temp.Equals(TextAlignment.Justify);
            }
            actualizeButtonsBool = false;
        }
        private void RtbEditor_KeyUp(object sender, KeyEventArgs e)
        {
            edited = true;
            if (e.Key == Key.Space)
            {
                rtbEditor.Undo();
                rtbEditor.Redo();
            }
            if (e.Key.Equals(Key.Enter))
            {
                firstEdit = true;
                changeAlignBeforeEdit = false;
            }
            ActualizeButtonsByFont();
        }
        private void RtbEditor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextRange allText = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            object? temp = allText.GetPropertyValue(TextElement.BackgroundProperty);
            if (temp != null && (temp == DependencyProperty.UnsetValue || temp.Equals(markColor)))
            {
                allText.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
            }
            actualizeButtonsBool = true;
            if (cbPalette.SelectedItem != null)
            {
                SolidColorBrush lastColor = (SolidColorBrush)cbPalette.SelectedItem;
                cbPalette.SelectedItem = null;
                pColor.Fill = lastColor;
            }
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
            if (filePath != string.Empty)
            {
                AddToLastDocs();
                filePath = string.Empty;
            }
            mainWindow.Title = "Bez názvu";
            edited = false;           
            UncheckAlignMenuItems();
            btnALeft.IsChecked = miALeft.IsChecked = true;
            ResetNewDocumentFont();
            rtbEditor.IsUndoEnabled = false;
            rtbEditor.IsUndoEnabled = true;
            firstEdit = true;
        }
        private void ResetNewDocumentFont()
        {
            rtbEditor.SelectAll();
            cbFFamily.SelectedIndex = 51;
            rtbEditor.Selection.ApplyPropertyValue(FontFamilyProperty, cbFFamily.SelectedItem);
            cbFSize.SelectedIndex = 2;
            rtbEditor.Selection.ApplyPropertyValue(FontSizeProperty, cbFSize.SelectedItem);
            rtbEditor.Selection.ApplyPropertyValue(FontWeightProperty, FontWeights.Normal);
            rtbEditor.Selection.ApplyPropertyValue(FontStyleProperty, FontStyles.Normal);
            TextDecorationCollection noUnder = new TextDecorationCollection();
            rtbEditor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, noUnder);
            cbPalette.SelectedIndex = 0;
            actualColor = (SolidColorBrush)cbPalette.SelectedItem;
            rtbEditor.Selection.ApplyPropertyValue(ForegroundProperty, actualColor);
            pColor.Fill = actualColor;
            ActualizeButtonsByFont();
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
                    if (rtbEditor.Document.Blocks.Count > 0 )
                    {
                        foreach (var block in rtbEditor.Document.Blocks)
                        {
                            if(block is Paragraph && ((Paragraph)block).Inlines.Count > 0)
                            {
                                foreach (var inline in ((Paragraph)block).Inlines)
                                {
                                    if (inline is Span && ((Span)inline).Inlines.Count > 0 && inline.TextDecorations.Count > 0 && 
                                        inline.TextDecorations.Contains(TextDecorations.Underline[0]))
                                    {
                                        TextDecorationCollection noUnder = new TextDecorationCollection();
                                        inline.TextDecorations.Clear();
                                        inline.TextDecorations.Add(noUnder);
                                        foreach (var underInline in ((Span)inline).Inlines)
                                        {
                                            TextDecorationCollection under = new TextDecorationCollection();
                                            under.Add(TextDecorations.Underline);
                                            underInline.TextDecorations.Add(under);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (filePath != string.Empty && filePath != openFilePath)
                    {
                        AddToLastDocs();
                    }
                    filePath = openFilePath;
                    mainWindow.Title = System.IO.Path.GetFileName(openFilePath);
                    edited = false;
                    rtbEditor.IsUndoEnabled = false;
                    rtbEditor.IsUndoEnabled = true;
                    if (rtbEditor.Document.ContentStart.GetPositionAtOffset(4) != null)
                    {
                        actualizeButtonsBool = true;
                        rtbEditor.Selection.Select(rtbEditor.Document.ContentStart.GetPositionAtOffset(3), rtbEditor.Document.ContentStart.GetPositionAtOffset(4));
                        rtbEditor.CaretPosition = rtbEditor.Document.ContentStart.GetPositionAtOffset(3);
                        ActualizeFontByButtons();
                    }
                    else
                    {                        
                        ResetNewDocumentFont();
                        firstEdit = true;
                        rtbEditor.CaretPosition = rtbEditor.Document.ContentStart;
                        ActualizeButtonsByFont();
                    }
                }                   
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dokument nelze načíst" + Environment.NewLine + ex.Message);
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

        //MenuItem Zpet Vpred
        private void UndoRedo_Click(object sender, RoutedEventArgs e)
        {
            actualizeButtonsBool = true;
        }

        //MenuItem Vybrat vse
        private void MiSelectAll_Click(object sender, RoutedEventArgs e)
        {
            rtbEditor.Selection.Select(rtbEditor.Document.Blocks.First().ContentStart, rtbEditor.Document.Blocks.Last().ContentEnd);
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
            using (rtbEditor.DeclareChangeBlock())
            {
                TextRange allText = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                allText.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
                if (string.IsNullOrWhiteSpace(allText.Text) || string.IsNullOrWhiteSpace(tbxFind.Text))
                {
                    tbStatus.Text = " Žádná shoda ";
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
                            if (matchBeginOffset >= 0 )
                            {
                                matchBeginPointer = matchBeginPointer.GetPositionAtOffset(matchBeginOffset);
                                TextRange matchRange = new TextRange(matchBeginPointer, matchBeginPointer.GetPositionAtOffset(tbxFind.Text.Length));
                                if (replace)
                                {
                                    matchRange.Text = tbxReplace.Text;
                                    matchBeginPointer = matchRange.End;
                                }
                                matchRange.ApplyPropertyValue(TextElement.BackgroundProperty, markColor);
                            }
                        } while (matchBeginOffset >= 0 && replace);
                    }
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
            GetStatus();
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

        private void MiBold_Click(object sender, RoutedEventArgs e)
        {
            btnBold.IsChecked = miBold.IsChecked;
        }
        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            if(btnBold.IsChecked != null)
            {
                miBold.IsChecked = (bool)btnBold.IsChecked;
            }
        }

        //MenuItem Kurziva
        private void MiItalic_Click(object sender, RoutedEventArgs e)
        {
            btnItalic.IsChecked = miItalic.IsChecked;
        }
        private void BtnItalic_Click(object sender, RoutedEventArgs e)
        {
            if(btnItalic.IsChecked != null)
            {
                miItalic.IsChecked= (bool)btnItalic.IsChecked;
            }
        }

        //MenuItem Podtrzene
        private void MiUnderline_Click(object sender, RoutedEventArgs e)
        {
            btnUnderline.IsChecked = miUnderline.IsChecked;
        }
        private void BtnUnderline_Click(object sender, RoutedEventArgs e)
        {
            if (btnUnderline.IsChecked != null)
            {
                miUnderline.IsChecked= (bool)btnUnderline.IsChecked;
            }
        }

        //Menuitem Zarovnani
        private void Align_Click(object sender, RoutedEventArgs e)
        {
            if (firstEdit)
            {
                changeAlignBeforeEdit = true;
            }
            string senderDirection = string.Empty;
            UncheckAlignMenuItems();
            if (sender is MenuItem)
            {
                MenuItem alignItem = (MenuItem)sender;
                senderDirection = alignItem.Name.Substring(3);
            }
            if (sender is RadioButton)
            {
                RadioButton alignButton = (RadioButton)sender;
                senderDirection = alignButton.Name.Substring(4);
            }
            switch(senderDirection)
            {
                case "Left": miALeft.IsChecked = true;
                             btnALeft.IsChecked = true; 
                             break;
                case "Center": miACenter.IsChecked = true;
                               btnACenter.IsChecked = true;
                               break;
                case "Right": miARight.IsChecked = true;
                              btnARight.IsChecked = true;
                              break;
                case "Justify": miAJustify.IsChecked = true;
                                btnAJustify.IsChecked = true;
                                break;
            }            
        }
        private void UncheckAlignMenuItems()
        {
            miALeft.IsChecked = false;
            miACenter.IsChecked = false;
            miARight.IsChecked = false;
            miAJustify.IsChecked = false;
        }

        //Combobox FontFamily
        private void CbFFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (actualizeButtonsBool) return;
            rtbEditor.Selection.ApplyPropertyValue(FontFamilyProperty, cbFFamily.SelectedItem);
            rtbEditor.Focus();
        }

        //Combobox FontSize
        private void CbFSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(FontSizeProperty, cbFSize.SelectedItem);
            rtbEditor.Focus();
        }
        private void CbFSize_KeyUp(object sender, KeyEventArgs e)
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

        //ComboBox Palette
        private List<SolidColorBrush> CreatePalette()
        {
            List<(byte, byte, byte)> rgbColors = new List<(byte, byte, byte)>()
            { (255, 255, 0), (255, 0, 0), (255, 0, 255), (0, 0, 255), (0, 255, 255), (0, 255, 0) };
            List<SolidColorBrush> colors = new List<SolidColorBrush>
            { new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Gray), new SolidColorBrush (Colors.LightGray)};
            for (int i = 0; i < rgbColors.Count; i++)
            {
                byte r = rgbColors[i].Item1 == 255 ? (byte)180 : (byte)0;
                byte g = rgbColors[i].Item2 == 255 ? (byte)180 : (byte)0;
                byte b = rgbColors[i].Item3 == 255 ? (byte)180 : (byte)0;
                SolidColorBrush color = new SolidColorBrush(Color.FromRgb(r, g, b));
                colors.Add(color);
                color = new SolidColorBrush(Color.FromRgb(rgbColors[i].Item1, rgbColors[i].Item2, rgbColors[i].Item3));
                colors.Add(color);
                r = rgbColors[i].Item1 == 0 ? (byte)130 : (byte)255;
                g = rgbColors[i].Item2 == 0 ? (byte)130 : (byte)255;
                b = rgbColors[i].Item3 == 0 ? (byte)130 : (byte)255;
                color = new SolidColorBrush(Color.FromRgb(r, g, b));
                colors.Add(color);
            }
            return colors;
        }
        private void CbPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPalette.SelectedItem != null)
            {
                actualColor = (SolidColorBrush)cbPalette.SelectedItem;
                rtbEditor.Selection.ApplyPropertyValue(ForegroundProperty, actualColor);
                pColor.Fill = actualColor;
            }
            rtbEditor.Focus();
        }
        private void BtnColor_Click(object sender, RoutedEventArgs e)
        {
            rtbEditor.Selection.ApplyPropertyValue(ForegroundProperty, pColor.Fill);
            rtbEditor.Focus();
        }
        private void CbPalette_DropDownClosed(object sender, EventArgs e)
        {
            rtbEditor.Focus();
        }
    }
}