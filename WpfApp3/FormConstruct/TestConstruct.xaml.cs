using BlueBerryProject.FileService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static BlueBerryProject.FormConstruct.QuestionsDTO;

namespace BlueBerryProject.FormConstruct
{
    /// <summary>
    /// Interaction logic for TestConstruct.xaml
    /// </summary>
    public delegate void DataEventHandler(string name, string dateOfLastChange);

    public partial class TestConstruct : Window, INotifyPropertyChanged
    {
        //поля
        FileSerealizer file;
        private string path;
        private bool wasAddedPathToFile = false; //це для того щоб не створювати новий файл.json кожен раз при натисканні кнопки save

        private bool closeOnlyCurrentForm = false; //це для кнопки повернутися до головного меню

        private bool isQuestionLocked = false;

        private string name;
        private int countCompletedQuestions; //лічильник питань які проходять по умові
        private int countTotalQuestions;     //лічильник усіх питань
        List<Question> questions = new List<Question>();

        private Response response;

        //private readonly string notAllowedSymbols = "/&^?\\|<>*:"; //заборонені символи бо в шляху вони викличуть exeption
        //властивості
        public int CompletedQuestion
        {
            get => countCompletedQuestions;
            set
            {
                countCompletedQuestions = value;
                OnPropertyChanged(nameof(CompletedQuestion));
            }
        }
        public int TotalQuestion
        {
            get => countTotalQuestions;
            set
            {
                countTotalQuestions = value;
                OnPropertyChanged(nameof(TotalQuestion));
            }
        }
        //події
        public event DataEventHandler TransferData;
        public event PropertyChangedEventHandler PropertyChanged; // делегат на подвійне динамічне звязування між кодом та UI 

        public TestConstruct(string path, string name)
        {
            InitializeComponent();
            this.path = path;
            this.name = name;
            Title = this.name + " - " + Title;
            txNameOfProject.Text = name; //пишу поточне ім'я проєкту на UI 

            DataContext = this; // Встановлюється контекст для прив'язки до UI
            
            CompletedQuestion = 0;
            TotalQuestion = 0;
        }

        //методи які відносяться до форми + сереалізація/десереалізація
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //закрити додаток
            MessageBoxResult userChoice = MessageBox.Show($"Зберегти зміни в {name}", name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (userChoice)
            {
                case MessageBoxResult.Yes: //зберегти дані
                    SaveData(); // багуліна перезбереження збереженого 
                    if (path != null)
                    {
                        TransferData?.Invoke(name, path); //передача данних через подію (імені та шляху)
                    }
                    break;
                case MessageBoxResult.No: //не зберігати дані
                    if (!closeOnlyCurrentForm)
                    {
                        Environment.Exit(0);
                    }

                    if (path != null)
                    {
                        TransferData?.Invoke(name, path); //передача данних через подію (імені та шляху)
                    }

                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true; //скасування закриття поточної форми
                    break;
                default:
                    break;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (path == null) //якщо шляху нема значить це новий об'єкт нічого загрузити не вийде
            {
                return;
            }
            
            file = new FileSerealizer(path); //створюється екземпляр класу файлСереалізатор для десереалізації в дтошку

            ProjectDataDTO data = file.LoadData(); //присвоюються дані в дтошку

            txDescriptionOfProject.Text = data.Description;

            foreach(var questionDTO in data.Questions)
            {
                Question question = new Question(questionDTO, questions.Count + 1);
                question.OnDelete += bDeleteQuestion_Click;
                question.OnQuestionCopied += bCopyQuestion_OnQuestionCopied;
                question.OnCompletionStatusChanged += Question_OnCompletionStatusChanged;
                questions.Add(question);
                question.ShowAllItems(wpPannel, questionDTO);
                TotalQuestion++;
            }
            response = new Response(data.Response.MaxValue);
            response.CreateUIForResponse(wpPannel);
            response.UpdateDoubleSliderValuesFromDTO(data.Response.MaxValue, data.Response.LoverValue, data.Response.UpperValue);
            response.UpdateCheckBoxFromDTO(data.Response.IsChecked);
            response.UpdateTextes(data.Response.RedZoneText, data.Response.YellowZoneText, data.Response.GreenZoneText);
            response.SetControlState(false);

            currentNumbersOfCorrect = GetNumberOfCorrectsFromQuestion(); //оновлюємо значення для наступної перевірки для метода bLockControlOnQuestion_Click
        }
        private void bBackToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            //закрити поточну форму та повернутися назад

            closeOnlyCurrentForm = true;
            Close();
        }
        private void bfileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }
        private void SaveData()
        {
            if (!wasAddedPathToFile) // (оптимізація) це поки не працює (типу при нажатті знову кнопки save воно шлях не створює а перезаписує)
            {
                path = "C:\\Users\\andje\\source\\repos\\WpfApp3\\WpfApp3\\FormConstruct\\Saves" + $"\\{name}.json";
                wasAddedPathToFile = true;
            }
            file = new FileSerealizer(path);

            file.SaveData(GatherDataToDTO()); //збереження в json файл
            MessageBox.Show("was saved!");
        }
        private ProjectDataDTO GatherDataToDTO()
        {
            return new ProjectDataDTO
            {
                Description = txDescriptionOfProject.Text.ToString(),
                Questions = questions.Select(q => q.GatherDataToDTO()).ToList(),
                Response = response.GatherDataToDTO(),

            };
        }
        private void txDescriptionOfProject_TextChanged(object sender, TextChangedEventArgs e) //подія для обробки автоматичного збільшення текстового поля для написання опису проєкту
        {
            TextBox? textBox = sender as TextBox;
            
            int lineCount = textBox.LineCount;

            textBox.Height = Math.Max(30, lineCount * 20);
        }
        private void txNameOfProject_LostFocus(object sender, RoutedEventArgs e) // [WILL NOT BE IMPLEMENTED] передача відаілідованої нової назви файлу у делегат в флрму MainWindow.xaml.cs
        {
            //
        }
        private void txNameOfProject_PreviewTextInput(object sender, TextCompositionEventArgs e)// [WILL NOT BE IMPLEMENTED] валідація на заборонені символи
        {
            //if (notAllowedSymbols.Contains(e.Text)) //якщо було введено заборонений символ 
            //{
            //    e.Handled = true; //ігнорує поточний запис бо він не корректний
            //    MessageBox.Show("Було введено недопустимі символи:\r: \\ / | ? * < > &", "Валідація",MessageBoxButton.OK,MessageBoxImage.Warning);
            //}
        }
        private void txNameOfProject_Pasting(object sender, DataObjectPastingEventArgs e) // [WILL NOT BE IMPLEMENTED] валідація вставки (ctrl+V) на заборонені символи + автоматичне видалення
        {
            //if (e.DataObject.GetDataPresent(DataFormats.Text))
            //{
            //    string pastedText = e.DataObject.GetData(DataFormats.Text) as string;
            //    string originalText = pastedText;
            //    if (!string.IsNullOrEmpty(pastedText))
            //    {
            //        foreach(char c in notAllowedSymbols)
            //        {
            //            pastedText = pastedText.Replace(c.ToString(), "");
            //        }

            //        TextBox? textBox = sender as TextBox;
            //        if(textBox != null)
            //        {
            //            int selectionStart = textBox.SelectionStart;
            //            string newText = textBox.Text.Insert(selectionStart, pastedText);

            //            if(newText.Length > textBox.MaxLength)
            //            {
            //                newText = newText.Substring(0, textBox.MaxLength);
            //            }

            //            textBox.Text = newText;
            //            textBox.SelectionStart = selectionStart+pastedText.Length;

            //            e.CancelCommand();

            //            textBox.Dispatcher.BeginInvoke(new Action(() =>
            //            {
            //                if (originalText.Length != pastedText.Length)
            //                {
            //                    MessageBox.Show("Під час вставки було відловлено та видалено недопустимі символи! \r : \\ / | ? * < > &", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            //                }
            //            }), System.Windows.Threading.DispatcherPriority.Background);
            //        }
            //    }
            //}
        }
        

        //методи повязані з класом QUESTION
        private void bAddQuastion_Click(object sender, RoutedEventArgs e)
        {
            Question question = new Question(questions.Count + 1);
            question.OnDelete += bDeleteQuestion_Click; //встановлення обробника на видалення з question
            question.OnQuestionCopied += bCopyQuestion_OnQuestionCopied;
            question.OnCompletionStatusChanged += Question_OnCompletionStatusChanged; 
            questions.Add(question); //додавання об'єктів до масиву

            if (response != null)
            {
                wpPannel.Children.Remove(response);

                question.ShowAllItems(wpPannel);

                wpPannel.Children.Add(response);
            }
            else
            {
                question.ShowAllItems(wpPannel);
            }
            TotalQuestion++;
        }
        private void bDeleteQuestion_Click(int index) //обробник події видалення question
        {
            var item = questions.FirstOrDefault(q => q.Index == index);

            if (item != null)
            {
                wpPannel.Children.Remove(item);
                questions.Remove(item);
                MessageBox.Show($"Елемент з індексом {index} було видалено");

                if (index != questions.Count)
                {
                    for (int i = index; i <= questions.Count; i++)
                    {
                        questions[i - 1].Header = i.ToString();
                        questions[i - 1].Index = i;
                    }
                }
            }
            if (TotalQuestion == CompletedQuestion) //якщо видаляємо то і видаляємо completed для синхронізації цеє є один обєкт
            {
                TotalQuestion--;
                CompletedQuestion--;
            }
            else TotalQuestion--; //якщо загальна кількість питаннь є більшою за виконані питання то віднімати треба лише загальну кількість
            
        }
        private void bCopyQuestion_OnQuestionCopied(QuestionDTO data, int newIndex)
        {
            Question question = new Question(data, questions.Count + 1);
            question.OnDelete += bDeleteQuestion_Click;
            question.OnQuestionCopied += bCopyQuestion_OnQuestionCopied;
            question.OnCompletionStatusChanged += Question_OnCompletionStatusChanged;
            questions.Add(question);

            if (response != null)
            {
                wpPannel.Children.Remove(response);

                question.ShowAllItems(wpPannel, data);

                wpPannel.Children.Add(response);
            }
            else
            {
                question.ShowAllItems(wpPannel, data);
            }

            TotalQuestion++;
        }//метод створення копії обєкта question
        private void Question_OnCompletionStatusChanged(bool isCompleted) //підрахунок завершених питань
        {
            Dispatcher.Invoke(() =>
            {
                if (isCompleted)
                {
                    CompletedQuestion++;
                }
                else
                {
                    CompletedQuestion--;
                }
            });
        }
        protected void OnPropertyChanged(string propertyName) //метод динамічної привязки даних між кодом та UI
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private int currentNumbersOfCorrect = 0;
        private void bLockControlsOnQuestion_Click(object sender, RoutedEventArgs e)
        {
            if(TotalQuestion == 0)
            {
                return;
            }
            if (CompletedQuestion == TotalQuestion)
            {

                isQuestionLocked = false;
                foreach (var question in questions)
                {
                    question.SetControlState(isQuestionLocked); //lock (false)
                }
                
                if(response != null)
                {
                    if(currentNumbersOfCorrect == GetNumberOfCorrectsFromQuestion()) //умова якщо не було змінено кількість Correct answers то дані в doubleSlider не будусть оновлені
                    {
                        response.SetControlState(!isQuestionLocked); //unlock (true)
                    }
                    else
                    {
                        response.UpdateDoubleSliderMaxValueDefault(GetNumberOfCorrectsFromQuestion()); 
                        response.SetControlState(!isQuestionLocked); //unlock (true)
                        currentNumbersOfCorrect = GetNumberOfCorrectsFromQuestion(); //оновлюємо значення для наступної перевірки
                    }
                }
            }
            else 
            {
                MessageBox.Show($"Не можу продовжити бо не всі питання були завершені \rКількість виконаних: {CompletedQuestion} Загальна кількість: {TotalQuestion}", "Lock question control", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Вимкнення кнопок
            bAddQuastion.IsEnabled = false;
            bLockControlsOnQuestion.IsEnabled = false;
            //Ввімкнення кнопок
            if(response == null)
            {
                bAddResponse.IsEnabled = true;
            }
            bLockControlOnResponse.IsEnabled = true;
        }

        //методи повязані з класом RESPONSE
        private void bAddResponse_Click(object sender, RoutedEventArgs e) //метод створення відповіді
        {
            response = new Response(GetNumberOfCorrectsFromQuestion());
            response.CreateUIForResponse(wpPannel);
            bAddResponse.IsEnabled = false;

            currentNumbersOfCorrect = GetNumberOfCorrectsFromQuestion(); //оновлюємо значення для наступної перевірки для метода bLockControlOnQuestion_Click
        }
        private void bLockControlOnResponse_Click(object sender, RoutedEventArgs e)
        {
            //unlock question
            isQuestionLocked = true;
            foreach (var question in questions)
            {
                question.SetControlState(isQuestionLocked);
            }
            //lock response
            response.SetControlState(!isQuestionLocked); //false
            //ввімкнення кнопок
            bAddQuastion.IsEnabled=true;
            bLockControlsOnQuestion.IsEnabled = true;

            //вимкнення кнопок
            bLockControlOnResponse.IsEnabled = false;
        }
        private int GetNumberOfCorrectsFromQuestion()
        {
            int countCorrects = 0;
            foreach (var question in questions)
            {
                countCorrects += question.GetNumberOfCorrects();
            }
            return countCorrects;
        }

        //методи повязані з переходом на іншу форму TEST_FORM
        private void bTest_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
