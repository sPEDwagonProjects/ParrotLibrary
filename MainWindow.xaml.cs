using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ParrotLibrary
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool loadoc = false;
        private ObservableCollection<DataModel.ParrotItem> parrotItems = new ObservableCollection<DataModel.ParrotItem>();
        private ObservableCollection<DataModel.ParrotItem> parrotItemsViewColletion = new ObservableCollection<DataModel.ParrotItem>();
        private Thread loadDataToListThread;
        private View.ArticleView articleView;
        private CollectionView view;
        private PropertyGroupDescription groupDescription = new PropertyGroupDescription("FN");
        private PropertyGroupDescription groupDescription2 = new PropertyGroupDescription("Gender");
        private PropertyGroupDescription groupDescription3 = new PropertyGroupDescription("DefaultGroup");

        private  SortDescription FnSort= new SortDescription("FN", ListSortDirection.Ascending);
        private SortDescription sortn = new SortDescription("DefaultGroup", ListSortDirection.Ascending);
        public MainWindow()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            InitializeComponent();
            FrameData.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            this.ParrotListView.Items.Clear();

            this.ParrotListView.ItemsSource = parrotItemsViewColletion;
            view = (CollectionView)CollectionViewSource.GetDefaultView(ParrotListView.ItemsSource);
            view.CurrentChanging += View_CurrentChanging;
            view.CurrentChanged += View_CurrentChanged;

            var awaiter = Load().GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                LoadinPanel.Visibility = Visibility.Collapsed;
                ParrotFromGenderButton.IsChecked = true;
            });
        }

        private void View_CurrentChanged(object sender, EventArgs e)
        {}
        public async Task Load()
        {
           await new TaskFactory().StartNew(() =>
            {
                int counter = 5;
                var infod = new DirectoryInfo("Data");
                var files = infod.GetFiles("*.json", SearchOption.AllDirectories);
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
                {
                    LoadingProgressBar.Maximum = files.Count();
                }));
                var tasks = new List<Task>();
                var b = files.GroupBy(_ => counter++ / 5).Select(v => v.ToArray());
                foreach (var fs in b)
                {
                    tasks.Add( Task.Run(() =>
                    {
                        foreach (var file in fs)
                        {
                            try
                            {
                                var p = Newtonsoft.Json.JsonConvert.DeserializeObject<ParrotLibrary.DataModel.ParrotItem>(File.ReadAllText(file.FullName));
                                p.Path = file.DirectoryName;
                                p.Image = file.DirectoryName + "\\" + p.Image;
                                try
                                {
                                    var imageDpi = new BitmapImage(new Uri(p.Image)).DpiX; ;
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        p.Image = file.DirectoryName + "\\" + p.Images[p.Images.Length - 1];
                                    }
                                    catch (Exception EX) { }
                                }
                                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    parrotItems.Add(p);
                                    LoadingProgressBar.Value++;
                                }));
                            }
                            catch (Exception ex)
                            { }
                            Thread.Sleep(100);

                        }
                    }));
                    
                }
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();
                GC.Collect(0);
                GC.Collect(1);
                GC.Collect(2);
                GC.Collect(3);
                loadoc = true;  
            });
        }

        private void LoadingDataToView(IEnumerable<DataModel.ParrotItem> items, 
                SortDescription sortDescription, PropertyGroupDescription propertyGroupDescription)
        {
            if (loadDataToListThread != null && loadDataToListThread.IsAlive == true)
            {
               loadDataToListThread.Abort();               
               loadDataToListThread.Join();

            }            
            parrotItemsViewColletion.Clear();

            view.GroupDescriptions.Clear();
            view.SortDescriptions.Clear();

            if(sortDescription!=null)
                view.SortDescriptions.Add(sortDescription);
            if (propertyGroupDescription != null)
                view.GroupDescriptions.Add(propertyGroupDescription);
                

            loadDataToListThread = new Thread(() =>
                {
                  try
                  {
                    
                      while (loadoc != true)
                      { Thread.Sleep(300); }
                        var tasks = new List<Task>();
                        int counter = 20;
                        var groups = items.GroupBy(_ => counter++ / 20).Select(v => v.ToArray());
                        foreach (var group in groups)
                        {
                                foreach (var item in group)
                                {
                                    if (!parrotItemsViewColletion.Contains(item))
                                    {
                                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                                        {
                                            parrotItemsViewColletion.Add(item);
                                        }));
                                    Thread.Sleep(100);
                                    }

                                }
                        }
                       
                      
                  }
                  catch(Exception EX) {  }
             });
            loadDataToListThread.Start();
        }

        private void ArticleView_ImageSelectedEvent(System.Windows.Media.ImageSource imageSource)
        {
            View.ImageView imageView = new View.ImageView(imageSource);
            FrameData.Navigate(imageView.Content);
            imageView.CloseImageViewEvent += ImageView_CloseImageViewEvent;
        }

        private void ImageView_CloseImageViewEvent()
        {
            FrameData.Navigate(articleView.Content);
        }

        private void ArticleView_ClosingEvent()
        {
            FrameData.Visibility = Visibility.Collapsed;
            ParrotListView.Visibility = Visibility.Visible;
        }

        private void ParrotFromABC_Checked(object sender, RoutedEventArgs e)
        {
            SetEnable(false, ParrotFromABC);
            SetEnable(true,ParrotFromGenderButton,AboutAppButton);
            SetChecked(false, ParrotFromGenderButton, AboutAppButton);
            FrameData.Visibility = Visibility.Collapsed;
            ParrotListView.Visibility = Visibility.Visible;

            if (ParrotListView.Items.Count > 0)
                ParrotListView.ScrollIntoView(ParrotListView.Items[0]);

            LoadingDataToView(parrotItems, FnSort, groupDescription);
        }

        private void ParrotFromABC_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void ParrotFromGenderButton_Unchecked(object sender, RoutedEventArgs e)
        {
        }
        private void ParrotFromGenderButton_Checked(object sender, RoutedEventArgs e)
        {
            SetEnable(false, ParrotFromGenderButton);
            SetEnable(true, ParrotFromABC, AboutAppButton);
            SetChecked(false, ParrotFromABC, AboutAppButton);

            FrameData.Visibility = Visibility.Collapsed;
            ParrotListView.Visibility = Visibility.Visible;

            if (ParrotListView.Items.Count > 0)
                ParrotListView.ScrollIntoView(ParrotListView.Items[0]);

            LoadingDataToView(parrotItems,FnSort, groupDescription2);
        }

        private void View_CurrentChanging(object sender, System.ComponentModel.CurrentChangingEventArgs e)
        {
        }

        private void ParrotListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParrotListView.SelectedIndex > -1)
            {
                ParrotListView.Visibility = Visibility.Collapsed;
                FrameData.Visibility = Visibility.Visible;

                articleView = new View.ArticleView(ParrotListView.SelectedItem as DataModel.ParrotItem);
                articleView.ClosingEvent += ArticleView_ClosingEvent;
                articleView.ImageSelectedEvent += ArticleView_ImageSelectedEvent;
                FrameData.Navigate(articleView.Content);
            }
        }

        private void AboutAppButton_Checked(object sender, RoutedEventArgs e)
        {
            SetEnable(false, AboutAppButton);
            SetEnable(true, ParrotFromABC, ParrotFromGenderButton);
            SetChecked(false, ParrotFromABC, ParrotFromGenderButton);

            ParrotListView.Visibility = Visibility.Collapsed;
            FrameData.Navigate(new View.AboutApp().Content);
            FrameData.Visibility = Visibility.Visible;
            
        }
       

        private void SearchTextbox_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
        }

        private void SearchTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                SetEnable(true, AboutAppButton, ParrotFromABC, ParrotFromGenderButton);
                SetChecked(false, AboutAppButton, ParrotFromABC, ParrotFromGenderButton);

                
                FrameData.Visibility = Visibility.Collapsed;
                ParrotListView.Visibility = Visibility.Visible;
                

                List<DataModel.ParrotItem> searchItems = new List<DataModel.ParrotItem>();

                if (SearchTextbox.Text.Length > 0)
                {
                    foreach (var k in parrotItems)
                        if (k.Title.ToLower().Contains(SearchTextbox.Text.ToLower()))
                            searchItems.Add(k);
                        

                    LoadingDataToView(searchItems, FnSort, groupDescription3);
                }
                else LoadingDataToView(parrotItems, FnSort, groupDescription3);
            }));
        }

        private void SetEnable(bool val, params ToggleButton[] buttons)
        {
            foreach (var button in buttons)
                button.IsEnabled = val;
        }
        private void SetChecked(bool val, params ToggleButton[] buttons)
        {
            foreach (var button in buttons)
                button.IsChecked = val;
        }


    }
}