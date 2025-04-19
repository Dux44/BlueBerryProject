using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using BlueBerryProject.FormConstruct;

namespace BlueBerryProject.FormTest
{
    /// <summary>
    /// Interaction logic for TestCompletion.xaml
    /// </summary>
    public partial class TestCompletion : Window, INotifyPropertyChanged
    {
        private List<QuestionTest> questionTests = new List<QuestionTest>();
        private ResponseDTO responseData = new ResponseDTO();

        public event PropertyChangedEventHandler? PropertyChanged;
        private int selectedCount = 0;
        public int SelectedCount
        {
            get => selectedCount;
            set
            {
                if(selectedCount != value)
                {
                    selectedCount = value;
                    tbCountChoosedAnswers.Text = SelectedCount.ToString();
                    OnPropertyChanged(nameof(SelectedCount));
                }
            }
        }

        public TestCompletion(ProjectDataDTO projectData, string name)
        {
            InitializeComponent();

            txNameOfTest.Text = name.ToString(); //написати імя тесту
            txDescriptionOfTest.Text = projectData.Description; //опис тесту
            tbAllAnswers.Text = projectData.Questions.Count.ToString(); //загальна кількість питань
            tbCountChoosedAnswers.Text = "0";

            int counter = 1;

            foreach(var quesitonDTO in projectData.Questions)
            {
                QuestionTest questionTest = new QuestionTest(counter);
                questionTest.OnSelectionChanged += UpdateSelectedCount;
                questionTest.ShowAllItems(quesitonDTO);
                questionTests.Add(questionTest);
                wpPanel.Children.Add(questionTest);
                counter++;
            }
            responseData = projectData.Response;
        }

        private void bBackToConstructionTestForm_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Environment.Exit(0);
        }
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void UpdateSelectedCount() 
        {
            SelectedCount = questionTests.Count(q => q.HasSelectorAnswer());
        }

        private void bCompleteTest_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedCount == questionTests.Count)
            {
                //TO DO
                //setControl radioButton/CheckBox to false
                //перевіряти обрані відповіді з правильними по структурі State Correct Incorrect тут зібрати дані з response інтервали та тестки 
                //створити GroupBox з одним текстовим полем із відповіддю та набраним балом
                
                questionTests.ForEach(q => q.SetStateSelector(false));
                int number = GetNumberOfCorrectAnswers();
                if (number < 0) number = 0;
                int userResult = GetIntervalsByScore(number);

                GroupBox resultFrame = CreateResultResponse();
                StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Vertical;
                TextBox textBox = CreateResultText();
                TextBlock showScore = ShowScoreOnResult();

                stack.Children.Add(showScore);
                stack.Children.Add(textBox);
                showScore.Text = $"Вітаю ви пройшли тест! Набрана кількість балів: {number}";

                resultFrame.Content = stack;
                

                switch (userResult)
                {
                    case 1:
                        textBox.Text = responseData.RedZoneText;
                        textBox.Background = Brushes.LightSalmon;
                        break;
                    case 2:
                        textBox.Text = responseData.YellowZoneText;
                        textBox.Background = Brushes.LightYellow;
                        break;
                    case 3:
                        textBox.Text = responseData.GreenZoneText;
                        textBox.Background = Brushes.LightGreen;
                        break;
                    default:
                        textBox.Text = "something wrong!";
                        return;
                }
                wpPanel.Children.Add(resultFrame);
                bCompleteTest.IsEnabled = false;
            }
            else MessageBox.Show($"Не можу завершити тест бо не було обрано ось таку кількість варіантів: {questionTests.Count-SelectedCount} ","TestValidation",MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void bReset_Click(object sender, RoutedEventArgs e)
        {
            questionTests.ForEach(q => q.SetStateSelector(true));
            questionTests.ForEach(q => q.ClearChoises());

            var boxToRemove = wpPanel.Children.OfType<GroupBox>().FirstOrDefault(gb => gb.Tag?.ToString() == "resultBox");
            if(boxToRemove != null)
            {
                wpPanel.Children.Remove(boxToRemove);
            }

            bCompleteTest.IsEnabled = true;
        }
        private int GetNumberOfCorrectAnswers()
        {
            int globalCount = 0;
            foreach(var q in questionTests)
            {
                globalCount += q.CountCorrectsSelectedAnswers();
            }
            return globalCount;
        }
        private int GetIntervalsByScore(int userScore)
        {
            if(userScore >= responseData.FirstInteval[0] && userScore <= responseData.FirstInteval[1])
            {
                return 1; //red response
            }
            if(userScore > responseData.SecondInterval[0] && userScore <= responseData.SecondInterval[1])
            {
                return 2; //yellow response
            }
            if(userScore > responseData.ThirdInterval[0] && userScore <= responseData.ThirdInterval[1])
            {
                return 3; //green response
            }
            return -1; //вихід за межі інтервалів
        }
        private GroupBox CreateResultResponse()
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Header = "Result";
            groupBox.Height = Double.NaN;
            groupBox.Width = 450;
            groupBox.Margin = new Thickness(20, 10, 0, 0);
            groupBox.Tag = "resultBox";
            return groupBox;
        }
        private TextBox CreateResultText()
        {
            TextBox textBox = new TextBox();
            textBox.IsReadOnly = true;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.HorizontalAlignment = HorizontalAlignment.Left;
            textBox.Width = 420;
            textBox.Height = Double.NaN;
            textBox.Margin = new Thickness(5, 5, 5, 5);
            textBox.FontSize = 14;
            return textBox;
        }
        private TextBlock ShowScoreOnResult()
        {
            TextBlock text = new TextBlock();
            text.Width = 400;
            text.Height = 20;
            text.FontSize = 14;
            text.Margin = new Thickness(5, 5, 5, 5);
            text.HorizontalAlignment = HorizontalAlignment.Left;
            return text;
        }
    }
}
