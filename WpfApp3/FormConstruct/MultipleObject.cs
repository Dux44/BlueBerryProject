using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueBerryProject.FormConstruct
{
    public class MultipleObject
    {
        //поля
        private UIElement _currentUIElement; //визначає який поточний тип
        private Rectangle _statusIndicator; //встановлює необхдний колір в rectangle для AnswerState

        //властивості
        public Grid Grid { get; private set; } // МОЖЛИВО НЕ ПОТІБНО
        public string AnswerType { get; set; } // визначає тип при створенні обєкта
        public DockPanel ParentPanel { get; set; } // Батьківський панель, на якому знаходиться об'єкт
        public AnswerState State { get; set; } = AnswerState.None;

        //події
        public delegate void DeleteMultiObjectHandler(MultipleObject obj); //делегат для видалення рядка мульиобєкта
        public event DeleteMultiObjectHandler OnDelete;
        public event Action<MultipleObject> OnAnsverSelected; //подія зміни кольору кнопки для відображення на UI (true/false/none)

        public enum AnswerState
        {
            None,
            Correct,
            Incorrect
        }

        public MultipleObject(string type) //конструктор
        {
            AnswerType = type;
        }

        // Метод для створення контейнера та елементів
        public Grid CreateMultiObject(int indexOfVariant) //метод для формування рядка та відправки його у dockPannel
        {
            CreateGridForMultipleObject(); // Створюємо Grid

            _currentUIElement = AnswerType == "RadioButton" ? CreateRadioButton() : CreateCheckBox();
            Grid.SetRow(_currentUIElement, 0);
            Grid.SetColumn(_currentUIElement, 0);
            Grid.Children.Add(_currentUIElement);


            // Створюємо TextBox і додаємо в Grid
            TextBox textBox = CreateAnswerText();
            textBox.Text = $"Variant {indexOfVariant}";
            Grid.SetRow(textBox, 0);
            Grid.SetColumn(textBox, 1);
            Grid.Children.Add(textBox);

            // Створюємо колірнний індикатор для визначення чи це правильна відповідь 
            Button colButton = CreateColorChangeButton();
            Grid.SetRow(colButton, 0);
            Grid.SetColumn(colButton, 2);
            Grid.Children.Add(colButton);
            ///
            // Створюємо кнопку видалення і додаємо в Grid
            Button delButton = CreateDeleteButton();
            Grid.SetRow(delButton, 0);
            Grid.SetColumn(delButton, 3);
            Grid.Children.Add(delButton);

            return Grid; // Повертаємо Grid для подальшого використання
        }
        public Grid CreateMultiObject(string text,AnswerState answersState)
        {
            CreateGridForMultipleObject(); // Створюємо Grid

            _currentUIElement = AnswerType == "RadioButton" ? CreateRadioButton() : CreateCheckBox();
            Grid.SetRow(_currentUIElement, 0);
            Grid.SetColumn(_currentUIElement, 0);
            Grid.Children.Add(_currentUIElement);

            // Створюємо TextBox і додаємо в Grid
            TextBox textBox = CreateAnswerText();
            textBox.Text = text; //передаю текст із копії
            Grid.SetRow(textBox, 0);
            Grid.SetColumn(textBox, 1);
            Grid.Children.Add(textBox);

            // Створюємо колірнний індикатор для визначення чи це правильна відповідь 
            Button colButton = CreateColorChangeButton();
            Grid.SetRow(colButton, 0);
            Grid.SetColumn(colButton, 2);
            Grid.Children.Add(colButton);

            UpdateAnswerState(answersState);

            // Створюємо кнопку видалення і додаємо в Grid
            Button delButton = CreateDeleteButton();
            Grid.SetRow(delButton, 0);
            Grid.SetColumn(delButton, 3);
            Grid.Children.Add(delButton);

            return Grid; // Повертаємо Grid для подальшого використання
        }
        public void UpdateUI(string newType)
        {
            if(AnswerType == newType)
            {
                return;
            }
            AnswerType = newType;
            Grid.Children.Remove(_currentUIElement);

            _currentUIElement = AnswerType == "RadioButton"? CreateRadioButton() : CreateCheckBox(); //тернарний оператор для визначення який тип питання
            Grid.SetRow(_currentUIElement, 0);
            Grid.SetColumn(_currentUIElement, 0);
            Grid.Children.Add(_currentUIElement);
        }
        
        // Метод для створення контейнера Grid
        private void CreateGridForMultipleObject()
        {
            Grid = new Grid(); // Ініціалізуємо Grid
            Grid.Height = Double.NaN;
            Grid.Width = 400;

            // Додаємо визначення рядків і стовпців для Grid
            Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //20
            Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //20
            Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //20
        }

        // Створення RadioButton
        private RadioButton CreateRadioButton()
        {
            RadioButton radioButton = new RadioButton
            {
                Width = 20,
                Height = 20,
                IsEnabled = false,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            return radioButton;
        }

        // Створення CheckBox
        private CheckBox CreateCheckBox()
        {
            CheckBox checkBox = new CheckBox
            {
                Width = 20,
                Height = 20,
                IsEnabled = false,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            return checkBox;
        }

        // Створення TextBox
        private TextBox CreateAnswerText()
        {
            TextBox textBox = new TextBox
            {
                Width = 250,
                Height = Double.NaN,
                Background = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            return textBox;
        }
        //створення кнопки правильної відповіді між true/false/none
        private Button CreateColorChangeButton()
        {
            Button colorButton = new Button
            {
                Width = 20,
                Height= 20,
                Margin = new Thickness(0,0,10,0),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
            };
            _statusIndicator = new Rectangle
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.Gray),
            };
            colorButton.Content = _statusIndicator;
            colorButton.Click += ColorButton_Click;

            return colorButton;
        }

        // Створення кнопки видалення
        private Button CreateDeleteButton()
        {
            Button button = new Button
            {
                Width = 20,
                Height = 20,
                Content = "X",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            button.Click += ButtonDeleteMultiObj; // Підписуємось на подію кліку
            return button;
        }
        public void UpdateColor()
        {
            switch (State)
            {
                case AnswerState.None:
                    _statusIndicator.Fill = new SolidColorBrush(Colors.Gray);
                    break;
                case AnswerState.Correct:
                    _statusIndicator.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case AnswerState.Incorrect:
                    _statusIndicator.Fill = new SolidColorBrush(Colors.Red);
                    break;
            }
        }
        public void UpdateAnswerState(AnswerState newState)
        {
            State = newState;
            UpdateColor();
        }

        // Обробник події для кнопки видалення
        private void ButtonDeleteMultiObj(object sender, EventArgs e)
        {
            OnDelete?.Invoke(this); // Викликаємо подію для видалення
        }
        //Обробник події вибір правильної відповіді
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            OnAnsverSelected?.Invoke(this);
        }

        public MultipleObjectsDTO.MultipleObjectDTO GatherDataToDTO() //зберання даних для копіювання та запису в файл
        {
            return new MultipleObjectsDTO.MultipleObjectDTO
            {
                AnswerType = this.AnswerType,
                State = this.State,
                AnswerText = this.Grid.Children.OfType<TextBox>().FirstOrDefault()?.Text ?? "",
            };
        }
    }
}
