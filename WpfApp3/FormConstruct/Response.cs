using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BlueBerryProject.FormConstruct
{
    class Response : GroupBox
    {
        //поля
        private Grid rGrid;

        private double width;
        private double height;
        private Thickness margin;

        private int maxValue;

        private DoubleSlider doubleSlider;

        private Label lowerTextValue, upperTextValue, showMaxValue;

        private Rectangle redZone, yellowZone, greenZone;

        private TextBox redZoneResponse,yellowZoneResponse,greenZoneResponse;

        private CheckBox checkDoubleSlider;

        private Label showDoneTasks;
        private bool conditionsMet = false;
        private int counterTasks = 0;
        private Dictionary<TextBox, bool> textBoxStates = new Dictionary<TextBox, bool>();

        private bool isTestButtonIsActive;
        

        //властивості
        public double rWidth
        {
            get { return width; }
            set 
            { 
                width = value;
                Width = width; //тут змінюється властивість this.Width цього обєкта напряму
            }
        }
        public double rHeight
        {
            get { return height; }
            set 
            { 
                height = value;
                Height = height;
            }
        }
        public Thickness rMargin
        {
            get { return margin; }
            set 
            { 
                margin = value;
                Margin = margin;
            }
        }
        public bool IsTestButtonOn
        {
            get { return isTestButtonIsActive; }
            set
            {
                if(isTestButtonIsActive != value)
                {
                    isTestButtonIsActive = value;
                    OnTestButtonStateChanged?.Invoke(value);
                }
            }
        }

        //події
        public event Action<bool> OnTestButtonStateChanged;
        public Response(int countOfCorrectAnswers)
        {
            Header = "Response";
            rWidth = 450;
            rHeight = Double.NaN;
            rMargin = new Thickness(30, 20, 0, 0);

            maxValue = countOfCorrectAnswers;
            CreateGridForResponse();
        }
        public void CreateUIForResponse(WrapPanel panel)
        {
            //counter умова для для можливості проходження тесту
            StackPanel sp = CreateTasksCounter();
            Grid.SetRow(sp, 0);
            Grid.SetColumn(sp, 3);
            rGrid.Children.Add(sp);
            //тут глибокий обєкт треба більше пояснення
            StackPanel stackPanel = CreateColoredUIZonesAndDoubleScrollBar();
            Grid.SetRow(stackPanel, 1);
            Grid.SetColumn(stackPanel, 0);
            Grid.SetColumnSpan(stackPanel, 3);
            rGrid.Children.Add(stackPanel);
            //обєктк показує на UI кількість правильних відповідей зроблених у пеопередніх обєктах question
            Label showMinValue = CreateLabelShowValue();
            showMinValue.Content = "  0";
            showMinValue.Margin = new Thickness(0, 20, 45, 0);
            Grid.SetRow(showMinValue, 1);
            Grid.SetColumn(showMinValue, 0);
            rGrid.Children.Add(showMinValue);

            showMaxValue = CreateLabelShowValue();
            showMaxValue.Content = maxValue;
            showMaxValue.Margin = new Thickness(80, 15, 60, 0);
            Grid.SetRow(showMaxValue, 1);
            Grid.SetColumn(showMaxValue, 2);
            Grid.SetColumnSpan(showMaxValue, 3);
            rGrid.Children.Add(showMaxValue);

            //два label динамічно показують властивість value кожного thumb у обєкті doubleSlider
            lowerTextValue = CreateLabelShowCurrentValueUnderSlider();
            lowerTextValue.Margin = new Thickness(0, 0, 13, 0); //ПРАВИЙ ПРАВИЙ ПРАВИЙ ПРАВИЙ ПРАВИЙ ПРАВИЙ ПРАВИЙ ПРАВИЙ
            lowerTextValue.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(lowerTextValue, 2);
            Grid.SetColumn(lowerTextValue, 1);
            rGrid.Children.Add(lowerTextValue);

            upperTextValue = CreateLabelShowCurrentValueUnderSlider();
            upperTextValue.HorizontalAlignment = HorizontalAlignment.Left;
            upperTextValue.Margin = new Thickness(10, 0, 0, 0); //ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ ЛІВИЙ
            Grid.SetRow(upperTextValue, 2);
            Grid.SetColumn(upperTextValue, 1);
            rGrid.Children.Add(upperTextValue);

            //галочка перевірка на правильність виставлених даних
            checkDoubleSlider = CreateCheckBox();
            Grid.SetRow(checkDoubleSlider, 2);
            Grid.SetColumn(checkDoubleSlider, 2);
            rGrid.Children.Add(checkDoubleSlider);

            //ТЕКСТБОКСИ ДЛЯ ВІДПОВІДЕЙ
            redZoneResponse = CreateResponseText();
            redZoneResponse.Background = Brushes.LightSalmon;
            
            textBoxStates[redZoneResponse] = false;
            Grid.SetRow(redZoneResponse, 3);
            Grid.SetColumn(redZoneResponse, 0);
            Grid.SetColumnSpan(redZoneResponse, 3);
            rGrid.Children.Add(redZoneResponse);

            yellowZoneResponse = CreateResponseText();
            yellowZoneResponse.Background= Brushes.LightYellow;
           
            textBoxStates[yellowZoneResponse] = false;
            Grid.SetRow(yellowZoneResponse, 4);
            Grid.SetColumn(yellowZoneResponse, 0);
            Grid.SetColumnSpan(yellowZoneResponse, 3);
            rGrid.Children.Add(yellowZoneResponse);

            greenZoneResponse = CreateResponseText();
            greenZoneResponse.Background = Brushes.LightGreen;
            
            textBoxStates[greenZoneResponse] = false;
            Grid.SetRow(greenZoneResponse, 5);
            Grid.SetColumn(greenZoneResponse, 0);
            Grid.SetColumnSpan(greenZoneResponse, 3);
            rGrid.Children.Add(greenZoneResponse);
            
            Content = rGrid;
            panel.Children.Add(this);

            UpdateLabels(); //оновлення позиції label для показу value обєкта doubleSlider
            UpdateZones(); // оновлення кольорів 
        }
        
        public ResponseDTO GatherDataToDTO()
        {
            ResponseDTO responseDTO = new ResponseDTO();
            responseDTO.MaxValue = this.maxValue;
            responseDTO.LoverValue = doubleSlider.LowerValue;
            responseDTO.UpperValue = doubleSlider.UpperValue;
            responseDTO.IsChecked = (bool)checkDoubleSlider.IsChecked;
            responseDTO.RedZoneText = redZoneResponse.Text.ToString();
            responseDTO.YellowZoneText = yellowZoneResponse.Text.ToString();
            responseDTO.GreenZoneText = greenZoneResponse.Text.ToString();


            int lowerValue = Convert.ToInt32(Math.Floor(doubleSlider.LowerValue)); //округлення до меншого
            int upperValue = Convert.ToInt32(Math.Floor(doubleSlider.UpperValue));
            
            int[] firstInterval = { 0, lowerValue};
            int[] secondInterval = {lowerValue, upperValue };
            int[] thirdInterval = { upperValue, maxValue };

            responseDTO.FirstInteval = firstInterval;
            responseDTO.SecondInterval = secondInterval;
            responseDTO.ThirdInterval = thirdInterval;

            return responseDTO;
           
        }
        private void CreateGridForResponse()
        {
            rGrid = new Grid();

            rGrid.Width = rWidth;
            rGrid.Height = rHeight;

            rGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25) }); //для counterTasks stackPanel
            rGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(80) }); //верхній рядок де знаходиться RangeSlider і інша необхідна логіка
            rGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); //другий рядок для двох lable які показують поточні value для кожного з thumb + checkBox
            rGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); //для textBox
            rGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); //для textBox
            rGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); //для textBox
            rGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) }); //Відступ від останнього обєкта в формі

            rGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150) });
            rGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150) }); 
            rGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(170) });
        }
        private void PaintGridForResponse(Grid rGrid)
        {
            TextBlock[] colorsForGrid = new TextBlock[4];

            colorsForGrid[0] = new TextBlock();
            colorsForGrid[0].Background = new SolidColorBrush(Colors.Yellow);
            Grid.SetRow(colorsForGrid[0], 0);
            Grid.SetColumn(colorsForGrid[0], 1);
            rGrid.Children.Add(colorsForGrid[0]);

            colorsForGrid[1] = new TextBlock();
            colorsForGrid[1].Background = new SolidColorBrush(Colors.Red);
            Grid.SetRow(colorsForGrid[1], 1);
            Grid.SetColumn(colorsForGrid[1], 1);
            rGrid.Children.Add(colorsForGrid[1]);

            colorsForGrid[2] = new TextBlock();
            colorsForGrid[2].Background = new SolidColorBrush(Colors.Blue);
            Grid.SetRow(colorsForGrid[2], 2);
            Grid.SetColumn(colorsForGrid[2], 1);
            rGrid.Children.Add(colorsForGrid[2]);

            colorsForGrid[3] = new TextBlock();
            colorsForGrid[3].Background = new SolidColorBrush(Colors.Green);
            Grid.SetRow(colorsForGrid[3], 3);
            Grid.SetColumn(colorsForGrid[3], 1);
            rGrid.Children.Add(colorsForGrid[3]);
        }

        private Rectangle CreateRectangleColorZone(Brush color)
        {
            return new Rectangle
            {
                Height = 20,
                Fill = color,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
        }
        private Label CreateLabelShowValue()
        {
            return new Label
            {
                Height = 25,
                Width = 30,
                
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(3),
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
        private Label CreateLabelShowCurrentValueUnderSlider()
        {
            return new Label
            {
                Height = 20,
                Width = 30,
                Content = "0",
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Center,

            };
        }
        private StackPanel CreateColoredUIZonesAndDoubleScrollBar()
        {
            StackPanel mainPanel = new StackPanel(); //головний stackPanel
            mainPanel.Width = 300;
            mainPanel.Height = 60;
            mainPanel.Margin = new Thickness(0, 0, 30, 0);
            
            mainPanel.Children.Add(CreateColoredRectangles()); //додаю Border(Grid) до stackPanel

            //створюю лівий та правий slider
            doubleSlider = new DoubleSlider
            {
                Width = 300,
                MinValue = 0,
                MaxValue = maxValue,
                LowerValue = maxValue / 3.0, //встановлення дефолтної позиції для першого thumb
                UpperValue = maxValue / 1.5  //встановлення дефолтної позиції для другого thumb
            };
            doubleSlider.ValueChanged += RangeSlider_ValueChanged;

            //додаю їх в окремий обєкт slider
            StackPanel sliderPanel = new StackPanel() {Orientation = Orientation.Horizontal};
            sliderPanel.Margin = new Thickness(0, 10, 0, 0);
            sliderPanel.Children.Add(doubleSlider);

            mainPanel.Children.Add(sliderPanel); //додаю вкладений sliderPanel в основну панель 

            return mainPanel;
        }
        private Border CreateColoredRectangles()
        {
            Border stackBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(0.8),
            };

            Grid gridOfColors = new Grid(); // створюю grid для розміщення в ньому rectangle обєктів
            gridOfColors.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); //створюю один рядок в якому будуть розміщені rectangle
            gridOfColors.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            gridOfColors.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            gridOfColors.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            redZone = CreateRectangleColorZone(Brushes.Red);
            Grid.SetRow(redZone, 0);
            Grid.SetColumn(redZone, 0);

            yellowZone = CreateRectangleColorZone(Brushes.Yellow);
            Grid.SetRow(yellowZone, 0);
            Grid.SetColumn(yellowZone, 1);

            greenZone = CreateRectangleColorZone(Brushes.Green);
            Grid.SetRow(greenZone, 0);
            Grid.SetColumn(greenZone, 2);

            gridOfColors.Children.Add(redZone);
            gridOfColors.Children.Add(yellowZone);
            gridOfColors.Children.Add(greenZone);

            stackBorder.Child = gridOfColors;
            return stackBorder;
        }
        private TextBox CreateResponseText()
        {
            TextBox textBox = new TextBox();
            textBox.Height = Double.NaN;
            textBox.MinHeight = 60;
            textBox.Margin = new Thickness(15, 15, 45, 0);
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.AcceptsReturn= true;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            textBox.FontSize = 14;
            textBox.TextChanged += TextBoxZoneResponse_TextChanged;
            return textBox;
        }

        private CheckBox CreateCheckBox()
        {
            CheckBox checkBox = new CheckBox();
            checkBox.IsChecked = false;
            checkBox.Margin = new Thickness(70, 0, 0, 0);
            checkBox.Checked += CheckBox_DoubleSliderChecked;
            checkBox.Unchecked += CheckBox_DoubleSliderUnchecked;
            return checkBox;
        }

        private StackPanel CreateTasksCounter()
        {
            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Height = 25,
                Margin = new Thickness(0, 0, 35, 0)
            };

            showDoneTasks = new Label
            {
                Content = "0",
                FontSize = 14,
                Foreground = Brushes.DarkBlue,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            Label rightSide = new Label
            {
                Content = "/ 4",
                FontSize = 14,
                
                Foreground = Brushes.Gray,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            sp.Children.Add(showDoneTasks);
            sp.Children.Add(rightSide);
            return sp;
        }

        private void UpdateZones()
        {
            double totalWidth = doubleSlider.Width; // Загальна ширина
            double minValue = doubleSlider.MinValue;
            double maxValue = doubleSlider.MaxValue;

            double lowerPos = (doubleSlider.LowerValue - minValue) / (maxValue - minValue) * totalWidth;
            double upperPos = (doubleSlider.UpperValue - minValue) / (maxValue - minValue) * totalWidth;

            // Червона зона (від 0 до першого Thumb)
            redZone.Width = lowerPos;

            // Жовта зона (між двома Thumb)
            yellowZone.Width = Math.Abs(upperPos - lowerPos);
            Canvas.SetLeft(yellowZone, lowerPos);

            // Зелена зона (від другого Thumb до кінця)
            greenZone.Width = Math.Abs(totalWidth - upperPos);
            Canvas.SetLeft(greenZone, upperPos);
        }
        private void UpdateLabels() //передача з кожного thumb у класі DoubleSlider своєї властивості Value у lable для відображення на UI
        {
            double sliderWidth = doubleSlider.ActualWidth;
            double minValue = doubleSlider.MinValue;
            double maxValue = doubleSlider.MaxValue;

            double lowerPos = (doubleSlider.LowerValue - minValue) / (maxValue - minValue) * sliderWidth;
            double upperPos = (doubleSlider.UpperValue - minValue) / (maxValue - minValue) * sliderWidth;

            Canvas.SetLeft(lowerTextValue, lowerPos);
            Canvas.SetLeft(upperTextValue, upperPos);

            lowerTextValue.Content = ((int)doubleSlider.UpperValue).ToString(); //тут виходить якась фігня бо uppder робить lower і тут щось намішано
            upperTextValue.Content = ((int)doubleSlider.LowerValue).ToString();
        }
        public void UpdateDoubleSliderMaxValueDefault(int newMaxValue)
        {
            

            double maxValue = Convert.ToDouble(newMaxValue);
            doubleSlider.MaxValue = maxValue;
            doubleSlider.LowerValue = maxValue / 3.0;
            doubleSlider.UpperValue = maxValue / 1.5;
            showMaxValue.Content = newMaxValue.ToString();

            doubleSlider.UpdateUI();
            
        }
        public void UpdateDoubleSliderValuesFromDTO(int newMaxValue,double newLowerValue, double newUppderValue)
        {

            doubleSlider.MaxValue = Convert.ToDouble(newMaxValue);
            showMaxValue.Content = newMaxValue.ToString();
            doubleSlider.LowerValue = newLowerValue;
            doubleSlider.UpperValue= newUppderValue;

            doubleSlider.UpdateUI();
        }
        public void UpdateCheckBoxFromDTO(bool IsChecked) 
        {
            checkDoubleSlider.IsChecked = IsChecked;
        }
        public void UpdateTextes(string redText,string yellowText,string greenText)
        {
            redZoneResponse.Text = redText;
            yellowZoneResponse.Text = yellowText;
            greenZoneResponse.Text = greenText;
        }
        private void CheckCondidions() //цей метод для візуального відображення необхідних дій для можливості пройти тест
        {
            showDoneTasks.Content = counterTasks;

            bool isCheckBoxChecked = checkDoubleSlider.IsChecked == true;

            bool areTextBoxesFilled = !textBoxStates.Values.Contains(false);

            if (isCheckBoxChecked && areTextBoxesFilled)
            {
                if (!conditionsMet)
                {
                    conditionsMet = true;
                    MessageBox.Show("Умови виконані, можна продовжити до проходження тесту!");
                    //тут делегат який передає bool змінну для ввімкнення кнопки test
                    IsTestButtonOn = true;
                }
            }
            else
            {
                //тут делегат який передає дані про вимкнення кнопки test
                IsTestButtonOn = false;
                conditionsMet = false;
            }
        }
        public void SetControlState(bool enabled) //блокую або розблоковую control в цьому обєкті
        {
            redZoneResponse.IsEnabled = enabled;
            yellowZoneResponse.IsEnabled = enabled;
            greenZoneResponse.IsEnabled = enabled;

            doubleSlider.IsEnabled = enabled;

            checkDoubleSlider.IsChecked = false;
            checkDoubleSlider.IsEnabled = enabled;
        }
        
        //обробники подій
        private void RangeSlider_ValueChanged(object? sender, EventArgs e)
        {
            UpdateLabels(); //оновлення вмісту лейблів
            UpdateZones(); //оновлення позиції кольорів
        }
        private void CheckBox_DoubleSliderChecked(object sender, RoutedEventArgs e) //подія перевірки чи користувач згіден з виставленими даними у double Slider
        {
            counterTasks++;
            CheckCondidions();
            
        }
        private void CheckBox_DoubleSliderUnchecked(object sender, RoutedEventArgs e)
        {
            counterTasks--;
            CheckCondidions();
        }
        private void TextBoxZoneResponse_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBoxRed = sender as TextBox; //отримую textBox з sender (тобто обєкт з якого було викликано метод)
            if (textBoxRed == null) return;

            bool isFilled = !string.IsNullOrWhiteSpace(textBoxRed.Text); //перевіряю чи не пустий

            if (isFilled && !textBoxStates[textBoxRed]) //якщо не пустий то збільшую counterTasks на +1 і встановлюю в Dictionary true щоб знову захід у метод не відбувся
            {
                counterTasks++;
                textBoxStates[textBoxRed] = true;
            }
            else if (!isFilled && textBoxStates[textBoxRed]) //аналогічна логіка якщо текст пустий тоді в Dictionary false щоб знову можна було зайти в верхній if
            {
                counterTasks--;
                textBoxStates[textBoxRed] = false;
            }
            CheckCondidions();
        } //подія на вловлення будь-якої зміни одного з троьх textBox + перевірка чи стоїсть галочка в 

       
    }
}
