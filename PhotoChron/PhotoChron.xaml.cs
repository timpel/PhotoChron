using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

namespace PhotoChron
{
    public static class MyExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
        {
            var sortableList = new List<T>(collection);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<FileItem> mFileList = new ObservableCollection<FileItem>();
        FileItem mSelectedFileItem;
        public Boolean SortByCreated;
        public FileItem SelectedFileItem
        {
            get { return mSelectedFileItem; }
            set { mSelectedFileItem = value; }
        }
        public ObservableCollection<FileItem> FileList
        {
            get { return mFileList; }
            
        }

        public class FileItem : INotifyPropertyChanged
        {
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public string Name { get; set; }
            public string FilePath { get; set; }
            public string Ext { get; set; }

            public FileItem(string file)
            {
                FileInfo info = new FileInfo(file);
                Name = info.Name;
                Created = info.CreationTime;
                Modified = info.LastWriteTime;
                FilePath = info.FullName;
                Ext = info.Extension;
            }
            
            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            DataContext = this;
            SortByCreated = true;
            InitializeComponent();
        }

        private void Datagrid_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Debug.WriteLine("Got Files");
                foreach (var file in files)
                {
                    Debug.WriteLine($"  File={file}");
                    mFileList.Add(new FileItem(file));
                }
            }

            if (FileList.Count == 0)
            {
                EmptyDatagridLabel.Visibility = Visibility.Visible;
            } else
            {
                EmptyDatagridLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void DatagridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileItem SelectedFileItem = (FileItem)fileDataGrid.SelectedItem;
            string SelectedFilePath = SelectedFileItem?.FilePath;
            if (SelectedFilePath != null)
            {
                try
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(SelectedFilePath);
                    bi.EndInit();
                    ImgPreview.Source = bi;
                }
                catch
                {
                    ImgPreview.Source = null;
                }
            }
            else
            {
                ImgPreview.Source = null;
            }
        }

        private void RenamePhotos(object sender, RoutedEventArgs e)
        {
            //ImgPreview.Source = null;
            foreach (FileItem fileItem in FileList)
            {
                System.IO.File.Move(fileItem.FilePath, fileItem.FilePath.Replace(fileItem.Name, "temp" + fileItem.Name));
                fileItem.FilePath = fileItem.FilePath.Replace(fileItem.Name, "temp" + fileItem.Name);
                fileItem.Name = "temp" + fileItem.Name;
            }

            if (RadioCreated.IsChecked == true)
            {
                FileList.Sort((a, b) => { return DateTime.Compare(a.Created, b.Created); });
            } else
            {
                FileList.Sort((a, b) => { return DateTime.Compare(a.Modified, b.Modified); });
            }

            int count = FileList.Count;
            int padding = 1;
            while (count > 9)
            {
                count /= 10;
                padding++;
            }

            int fileNum = 0;
            string oldName, oldPath, newName, newPath;
            foreach (FileItem fileItem in FileList)
            {
                oldName = fileItem.Name;
                oldPath = fileItem.FilePath;
                newName = fileNum.ToString().PadLeft(padding, '0') + fileItem.Ext;
                newPath = fileItem.FilePath.Replace(oldName, newName);

                fileItem.Name = newName;
                fileItem.FilePath = newPath;

                System.IO.File.Move(oldPath, newPath);
                fileNum++;
            }

        }


    }

}
