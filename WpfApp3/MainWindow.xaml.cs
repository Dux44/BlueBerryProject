using BlueBerryProject;
using System.Collections.ObjectModel;
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
using BlueBerryProject.FormConstruct;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Windows.Shell;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filesPath = "C:\\Users\\andje\\source\\repos\\WpfApp3\\WpfApp3\\FormConstruct\\Saves";
        private ObservableCollection<Project> projects = new ObservableCollection<Project>();  //тип масиву ObservableCollection потрібен для динамічного зв'язування 
                                                                                               //dataGrid з масивом і під час оновлення з'являється новий об'єкт (Refresh())

        public MainWindow()
        {
            InitializeComponent();
        }
        private void bAddNewQuastion_Click(object sender, RoutedEventArgs e)
        {
            string path = null, defaultName = $"NewTest{projects.Count + 1}";
            TestConstruct form = new TestConstruct(path, defaultName);
            Hide(); //тут можна використати Close() це вивільнить ресурси але перегружати заново форму
            form.TransferData += Form_DataTransferEvent;
            form.ShowDialog();
            Show();
        }

        private void Form_DataTransferEvent(string name, string path) //подія створення нової форми коли користувач
                                                                      //зберіг та закрив форму то вона з'явится в переліку
        {
            Project project = new Project(name, path);
            projects.Add(project);
            //dataGrid.Items.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] pathInDirectory = Directory.GetFiles(filesPath);//отримати всі шляхи з папки SAVES
            if (pathInDirectory.Length != 0)
            {
                Project project;
                foreach (var pathToFile in pathInDirectory)
                {
                    string nameOfFile = pathToFile.Substring(filesPath.Length + 1); //видаляю непотрібну частину шляху
                    //nameOfFile = nameOfFile.Substring(0, nameOfFile.Length - 5);
                    nameOfFile = nameOfFile[0..^5]; //видаляю розширення .json
                    projects.Add(project = new Project(nameOfFile, pathToFile)); //створюю масив 
                }
                dataGrid.ItemsSource = projects; //вивожу вміст на форму (DataGrid)

            }
            else
            {
                MessageBox.Show("Folder is empty!");
            }
        }
        /*
         * короткий екскурс що тут коли працюєш з файлом на пряму то виникає декілька проблем 
         * 1) проблема деякі символи заборонені в шляхах (:^*<>|/\?) треба валідація
         * 2) користувач може вписати існуюче ім'я тому це потрібно побачити та спитати користувача чи він хоче отримати інше доступне ім'я якщо так виправити
         *    ні залишити ім'я без змін
         */
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)//перевірка на однакові введені імена
        {
            //отримати змінене ім'я
            TextBox textBox = e.EditingElement as TextBox; //отримую об'єкт textBox бо всі рядки у об'єкті dataGrid є textBox-ами
            string changedName = textBox.Text.Trim(); //отримую змінений текст з рядка в dataGrid 
            int selectedIndex = dataGrid.SelectedIndex; //отримую індекс поточного textBox-а

            //отримати незмінене ім'я
            Project originalProject = e.Row.Item as Project;
            string originalName = originalProject.Name;

            string wantToChangeThatName = changedName;

            
            string tempPath = $"C:\\Users\\andje\\source\\repos\\WpfApp3\\WpfApp3\\FormConstruct\\Saves\\{changedName}.json";

            string directory = System.IO.Path.GetDirectoryName(tempPath);
            string fileName = System.IO.Path.GetFileName(tempPath);
            string lowerFileName = fileName.ToLowerInvariant();


            ///це якщо нового елементу не існує в переліку (щоб не прокручувати цикл до останнього елемента) + до пошуку усі імена будуть в низькому регістрі щоб пройти проблему однакових імен у різних регістрах FILE.json == file.json
            if(Directory.EnumerateFiles(directory).Select(f => System.IO.Path.GetFileName(f).ToLowerInvariant()).Contains(lowerFileName))
            {
                for (int i = 0; i < projects.Count; i++) //цикл потрібен для пошуку індекса співпадіння імен
                {
                    if (changedName.ToLowerInvariant() == projects[i].Name.ToLowerInvariant()) //пошук співпадіння по імені (поточно введеному користувачем) вони у нижньому регістрі для відловлення проблеми однакових імен у різних регістрах
                    {
                        if (i != selectedIndex) //перевірка чи не є знайдений елемент особливим (користувач нажав на об'єкт і вийшов)
                        {

                            string[] arr = AlgoritmFindFreeSpaceForName(tempPath, changedName);
                            changedName = arr[0];
                            tempPath = arr[1];

                            MessageBoxResult renameChoice = MessageBox.Show($"перейменування імені проєкту {originalName} на {wantToChangeThatName} не доступне бо воно вже є в списку перейменувати на ({changedName})",
                                "Перейменування назви проєкту", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                            switch (renameChoice)
                            {
                                case MessageBoxResult.OK: //ЗАМІНИТИ ІМ'Я НА ДОСТУПНЕ У СПИСКУ БО ЯКЕ КОРИСТУВАЧ ХОЧЕ ЗАЙНЯТЕ

                                    WriteTextInDataGridRow(selectedIndex, changedName);

                                    break;
                                case MessageBoxResult.Cancel: //НЕЗМІНЮВАТИ ІМ'Я 

                                    changedName = originalName;
                                    WriteTextInDataGridRow(selectedIndex, changedName);

                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                        else break;
                    }
                }
            }
            //шлях
            try
            {
                if (changedName != projects[selectedIndex].Name) // якщо було змінено ім'я поточного файлу то переписується шлях і оновлюються дані в списку projects
                {
                    File.Move(projects[selectedIndex].Path, tempPath); //перейменовую безпосередньо файл
                    projects[selectedIndex].Name = changedName;    //оновлюю дані про ім'я файлу
                    projects[selectedIndex].Path = tempPath; //оновлюю дані про шлях файлу
                }
            }
            catch(IOException)
            {
                MessageBox.Show("Було введено недопустиме ім'я!","IOError",MessageBoxButton.OK,MessageBoxImage.Error);

                WriteTextInDataGridRow(selectedIndex, originalName);
                e.Cancel = true;
            }
            
        }
        private string[] AlgoritmFindFreeSpaceForName(string tempPath, string changedName)
        {
            int countMatches = 0;

            var match = Regex.Match(changedName, @"^(.*?)( \((\d+)\))?$"); //регулярка для отримання двох різних змінних name і index
            if (match.Success)
            {
                changedName = match.Groups[1].Value;

                if (match.Groups[3].Success)
                {
                    countMatches = int.Parse(match.Groups[3].Value);
                }
            }

            bool oneTimeIf = false;
            while (File.Exists(tempPath) || projects.Any(p => p.Name == changedName)) //пошук вільного місця для однакоих імен
            {
                countMatches++;
                if (oneTimeIf)
                {
                    int countedIndexes = FindLengthOfNumbers(changedName); // за допомогою регулярки знаходимо кількість чисел в дужках name (10) шукане значення 2
                    changedName = changedName.Substring(0, changedName.Length - (2 + countedIndexes)).Trim(); //віднімаєм необхідну кілкість символів з кінця імені 
                }
                else
                {
                    oneTimeIf = true;
                }
                changedName = $"{changedName} ({countMatches})"; //додаємо новий, актуальний індекс
                tempPath = $"C:\\Users\\andje\\source\\repos\\WpfApp3\\WpfApp3\\FormConstruct\\Saves\\{changedName}.json"; //міняємо шлях на новий і перевіряємо з кожним наявним проєктом у списку
            }
            string[] arr = [changedName, tempPath];
            return arr;
        } 
        private int FindLengthOfNumbers(string name) //метод для отримання кілкіості числе в дужках
        {
            int countOfDigits = 0;
            string pattern = @"\((\d+)\)$";
            Match match = Regex.Match(name, pattern);
            if(match.Success)
            {
                countOfDigits = match.Groups[1].Value.Length;
            }
            return countOfDigits;
        }
        private void dataGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)//перевірка введених символів на коректність
        {
            string notAllowedSymbols = "/&^?\\|<>*:"; //заборонені символи бо в шляху вони викличуть exeption

            if (notAllowedSymbols.Contains(e.Text)) //якщо було введено заборонений символ 
            {
                e.Handled = true; //ігнорує поточний запис бо він не корректний
                MessageBox.Show("Було введено недопустимі символи:\r: \\ / | ? * < > &", "Валідація");
            }
        }
        private void WriteTextInDataGridRow(int rowIndex, string textToWrite) // прямий запис в рядочок який було змінено
        {
            DataGridCellInfo cellInfo = new DataGridCellInfo(dataGrid.Items[rowIndex], dataGrid.Columns[0]);
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent is TextBox textBox)
            {
                textBox.Text = textToWrite;
            }
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) //видалення об'єкта
            {
                e.Handled = true; //зупинити обробку delete для інших обробників

                int index = dataGrid.SelectedIndex;
                MessageBoxResult choice = MessageBox.Show($"Ви точно хочете видалити форму {projects[index].Name}", "видалення форми", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (choice == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(projects[index].Path); //видалення файлу 
                        projects.RemoveAt(index);          //видалення об'єкту з масиву
                        RefreshRowBackground();            //оновлення кольорів усіх об'єктів
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (choice == MessageBoxResult.No)
                {
                    return;
                }
            }
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
            //поставити обробник на Enter (захід на вказану форму але з умовою що НЕ виходим з форми коли працював CellEditEnding)
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex != -1) //заглушка на неіснуючі елементи може краще try?
            {
                TestConstruct testConstruct = new TestConstruct(projects[selectedIndex].Path, projects[selectedIndex].Name);
                Hide();
                testConstruct.ShowDialog();
                Show();
            }
            //try
            //{
            //    TestConstruct testConstruct = new TestConstruct(projects[selectedIndex].Path, projects[selectedIndex].Name);
            //    Hide();
            //    testConstruct.ShowDialog();
            //    Show();
            //}
            //catch(IndexOutOfRangeException)
            //{
            //    MessageBox.Show("nothing to open list is empty");

            //}
        }
        private void RefreshRowBackground()//цей метод перемальовує всі об'єкти dataGrid після події видалення
        {
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                var dataGridRow = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (dataGridRow != null)
                {
                    if (i % 2 == 0)
                    {
                        dataGridRow.Background = Brushes.White;
                    }
                    else dataGridRow.Background = Brushes.LightBlue;
                }
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e) //цей метод замальовує кожен другий елемент в світлосиній колір,також спрацьовує при створенні нового об'єкта 

        {
            int index = e.Row.GetIndex();
            if (e.Row != null)
            {
                if (index % 2 == 0)
                {
                    e.Row.Background = Brushes.White;
                }
                else e.Row.Background = Brushes.LightBlue;
            }
        }
    }
}

