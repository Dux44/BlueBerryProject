using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using BlueBerryProject.FormConstruct;
using static BlueBerryProject.FormConstruct.QuestionsDTO;
using static BlueBerryProject.FormConstruct.MultipleObjectsDTO;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;



namespace BlueBerryProject.FormConstruct
{
    class Question : GroupBox
    {
        //поля
        private Grid qGrid; //зробити сітку для розміщення об'єктів

        private DockPanel dockPanel; //відображати мультиоб'єкт

        private List<MultipleObject> multipleObjects = new List<MultipleObject>(); //зберігати масив мультиоб'єктів

        private double height;
        private double width;
        private Thickness margin;

        private bool isFirstCheckBoxSelection = true; //флажок для коректної логіки зміни кольорів при типі question - checkBox

        private bool isThatQuestionIsRecreated = false; //флажок для того якщо обєкт checkBox було скопійовано і його логіка збереглася (нажав змінило лише те що нажав бо був баг нажав інші зелені стали червоними)
        private bool oneTimeIf = true; //флажок для ігнорування зміни іншого флажка isThatQuestionIsRecreated бо при створенні копії метод ComboBox_QuestionTypeChanged буде захід бо з dtoшки буде передано новий QuestionType

        private bool isThatQuestionCompleted = false;
        //властивості
        public double qHeight
        {
            get { return height; }
            set
            {
                height = value;
                Height = height; //це змінна GroupBox
            }
        }
        public double qWidth
        {
            get { return width; }
            set
            {
                width = value;
                Width = width;
            }
        }
        public Thickness qMargin
        {
            get { return margin; }
            set
            {
                margin = value;
                Margin = margin;
            }
        }
        
        public int Index { get; set; } //індексатор для видалення question у класі TestConstruct.xaml.cs
        public string QuestionType { get; set; } // тип question між RadioButton/CheckBox

        public bool IsThatQuestionCompleted //властивість перевірки чи питання є завершеним
        {
            get => isThatQuestionCompleted;
            set
            {
                if(isThatQuestionCompleted != value)
                {
                    isThatQuestionCompleted = value;
                    OnCompletionStatusChanged?.Invoke(value);
                }
            }
        }

        //події
        public delegate void DeleteQuestionHandler(int index); //делегат для переходу до батьківського класу та видалення поточного обєкта
        public event DeleteQuestionHandler OnDelete; //обробник події

        public delegate void QuestionCopiedHandler(QuestionDTO copy_question, int newIndex); //делегат для переходу до вищого класу для копіювання поточноо обєкту
        public event QuestionCopiedHandler OnQuestionCopied; //екземпляр події

        public event Action<bool>? OnCompletionStatusChanged; //подія для передачі зміни стану змінної isThatQuestionCompleted
        public Question() //дефолтне створення об'єкту (безполєзнік)
        {
           
            qHeight = Double.NaN;
            qWidth = 450;
            qMargin = new Thickness(30, 20, 0, 0);
            CreateGridForQuestion();
            
        }
        public Question(int countOfQuestions) //цей конструктор використовується
        {
            Header = $"{countOfQuestions}";
            qHeight = Double.NaN;
            qWidth = 450;
            qMargin = new Thickness(30, 20, 0, 0);
            CreateGridForQuestion();
            Index = countOfQuestions;
            QuestionType = "RadioButton"; //Default string Type for question between RadioButton/CheckBox
        }
       
        public Question(QuestionDTO dto,int index) //для створеної форми
        {
            Header = $"{index}";
            qHeight = Double.NaN;
            qWidth = 450;
            qMargin = new Thickness(30, 20, 0, 0);
            CreateGridForQuestion();
            Index = index;
            QuestionType = dto.QuestionType;
           
        }
        //методи де складається обєкт Question до купи
        public void ShowAllItems(WrapPanel panel)
        {
            //ще прив'язати події на ці об'єкти які є одиночними екземплярами при створенні question

            
            //текст бокс для введення питання
            TextBox textBox = CreateTextBox(); 
            Grid.SetRow(textBox, 0);
            Grid.SetColumn(textBox, 0);
            Grid.SetColumnSpan(textBox,2);
            qGrid.Children.Add(textBox);

            //вибір між двома типами відповідей або radio button або checkBox
            ComboBox comboBox = CreateComboBox(); 
            Grid.SetRow(comboBox, 0);
            Grid.SetColumn(comboBox, 2);
            
            qGrid.Children.Add(comboBox);

            CreateDockPanel();
            Button button_AddNewAnswer = CreateButtonForAddingAnswers();
            DockPanel.SetDock(button_AddNewAnswer, Dock.Bottom);
            dockPanel.Children.Add(button_AddNewAnswer);

            Grid.SetRow(dockPanel, 1);
            Grid.SetColumn(dockPanel, 0);
            Grid.SetColumnSpan(dockPanel, 3);
            qGrid.Children.Add(dockPanel);

            Button buttonDelete = CreateButton();
            buttonDelete.Name = "delelte_Button";
            buttonDelete.Content = "X";
            Grid.SetRow(buttonDelete, 2);
            Grid.SetColumn(buttonDelete, 1);
            buttonDelete.Click += ButtonDeleteQuestion_Click;
            qGrid.Children.Add(buttonDelete);

            Button buttonCopy = CreateButton(); //копіювання не реалізовано
            buttonCopy.Name = "copy_button";
            buttonCopy.Content = "C";
            buttonCopy.Margin = new Thickness(0, 0, 45, 0);
            Grid.SetRow(buttonCopy, 2);
            Grid.SetColumn(buttonCopy, 1);
            buttonCopy.Click += ButtonCopyQuestion_Click;
            qGrid.Children.Add(buttonCopy);

            Content = qGrid;
            panel.Children.Add(this);
        }
        public void ShowAllItems(WrapPanel panel, QuestionDTO data)
        {
            isThatQuestionIsRecreated = true;
            //створення textBox
            TextBox textBox = CreateTextBox();
            textBox.Text = data.QuestionText;
            Grid.SetRow(textBox, 0);
            Grid.SetColumn(textBox, 0);
            Grid.SetColumnSpan(textBox, 2);
            qGrid.Children.Add(textBox);

            //створення comboBox
            ComboBox comboBox = CreateComboBox();
            comboBox.SelectedIndex = data.SelectedIndex;
            Grid.SetRow(comboBox, 0);
            Grid.SetColumn(comboBox, 2);

            qGrid.Children.Add(comboBox);

            //створення dockPannel без дефолтно створеного обєкта
            CreateDockPanel(data.multipleObjectsDTO);
            Button button_AddNewAnswer = CreateButtonForAddingAnswers();
            DockPanel.SetDock(button_AddNewAnswer, Dock.Bottom);
            dockPanel.Children.Add(button_AddNewAnswer);

            Grid.SetRow(dockPanel, 1);
            Grid.SetColumn(dockPanel, 0);
            Grid.SetColumnSpan(dockPanel, 3);

            qGrid.Children.Add(dockPanel);

            

            //кнопка видалення question
            Button buttonDelete = CreateButton();
            buttonDelete.Content = "X";
            Grid.SetRow(buttonDelete, 2);
            Grid.SetColumn(buttonDelete, 1);
            buttonDelete.Click += ButtonDeleteQuestion_Click;
            qGrid.Children.Add(buttonDelete);

            //кнопка копіювання quesiton
            Button buttonCopy = CreateButton();
            buttonCopy.Content = "C";
            buttonCopy.Margin = new Thickness(0, 0, 45, 0);
            Grid.SetRow(buttonCopy, 2);
            Grid.SetColumn(buttonCopy, 1);
            buttonCopy.Click += ButtonCopyQuestion_Click;
            qGrid.Children.Add(buttonCopy);

            Content = qGrid;
            panel.Children.Add(this);
            //
        }
        //стандартні методи
        public QuestionDTO GatherDataToDTO() //збір даних для сереалізації + копіювання
        {
            return new QuestionDTO
            {
                QuestionType = this.QuestionType,
                SelectedIndex = qGrid.Children.OfType<ComboBox>().FirstOrDefault()?.SelectedIndex ?? 0,
                QuestionText = qGrid.Children.OfType<TextBox>().FirstOrDefault()?.Text ?? "",
                multipleObjectsDTO = multipleObjects.Select(mo => mo.GatherDataToDTO()).ToList(), 
            };
        }
        private void CreateGridForQuestion()
        {
            qGrid = new Grid();

            qGrid.Width = qWidth;
            qGrid.Height = qHeight -10;

            qGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            qGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto});
            qGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.3,GridUnitType.Star) });
            qGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            qGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            qGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1,GridUnitType.Star) });
        }
        private void PaintGrid(Grid qGrid) //кольори для grid 
        {
            TextBlock[] colorsForGrid = new TextBlock[9];

            colorsForGrid[0] = new TextBlock();
            colorsForGrid[0].Background = new SolidColorBrush(Colors.Yellow);
            Grid.SetRow(colorsForGrid[0], 0);
            Grid.SetColumn(colorsForGrid[0], 0);
            qGrid.Children.Add(colorsForGrid[0]);

            colorsForGrid[1] = new TextBlock();
            colorsForGrid[1].Background = new SolidColorBrush(Colors.Red);
            Grid.SetRow(colorsForGrid[1], 0);
            Grid.SetColumn(colorsForGrid[1], 1);
            qGrid.Children.Add(colorsForGrid[1]);

            colorsForGrid[2] = new TextBlock();
            colorsForGrid[2].Background = new SolidColorBrush(Colors.Blue);
            Grid.SetRow(colorsForGrid[2], 0);
            Grid.SetColumn(colorsForGrid[2], 2);
            qGrid.Children.Add(colorsForGrid[2]);

            colorsForGrid[3] = new TextBlock();
            colorsForGrid[3].Background = new SolidColorBrush(Colors.Pink);
            Grid.SetRow(colorsForGrid[3], 1);
            Grid.SetColumn(colorsForGrid[3], 0);
            qGrid.Children.Add(colorsForGrid[3]);

            colorsForGrid[4] = new TextBlock();
            colorsForGrid[4].Background = new SolidColorBrush(Colors.Purple);
            Grid.SetRow(colorsForGrid[4], 1);
            Grid.SetColumn(colorsForGrid[4], 1);
            qGrid.Children.Add(colorsForGrid[4]);

            colorsForGrid[5] = new TextBlock();
            colorsForGrid[5].Background = new SolidColorBrush(Colors.Brown);
            Grid.SetRow(colorsForGrid[5], 1);
            Grid.SetColumn(colorsForGrid[5], 2);
            qGrid.Children.Add(colorsForGrid[5]);

            colorsForGrid[6] = new TextBlock();
            colorsForGrid[6].Background = new SolidColorBrush(Colors.Violet);
            Grid.SetRow(colorsForGrid[6], 2);
            Grid.SetColumn(colorsForGrid[6], 0);
            qGrid.Children.Add(colorsForGrid[6]);

            colorsForGrid[7] = new TextBlock();
            colorsForGrid[7].Background = new SolidColorBrush(Colors.Orange);
            Grid.SetRow(colorsForGrid[7], 2);
            Grid.SetColumn(colorsForGrid[7], 1);
            qGrid.Children.Add(colorsForGrid[7]);

            colorsForGrid[8] = new TextBlock();
            colorsForGrid[8].Background = new SolidColorBrush(Colors.Maroon);
            Grid.SetRow(colorsForGrid[8], 2);
            Grid.SetColumn(colorsForGrid[8], 2);
            qGrid.Children.Add(colorsForGrid[8]);
        }
        private TextBox CreateTextBox()
        {
            TextBox textBox = new TextBox();
            textBox.Width = 300;
            textBox.Height = Double.NaN;
            textBox.Background = Brushes.LightGray;
            textBox.Margin =new Thickness(15,15,15,0);
            textBox.Text = "initial Text";
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.AcceptsReturn = true;
            textBox.VerticalAlignment = VerticalAlignment.Stretch;
            textBox.HorizontalAlignment = HorizontalAlignment.Stretch;

            return textBox;
        }
        private ComboBox CreateComboBox()//switch between radio/check button
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 80;
            comboBox.Height = 20;
            comboBox.VerticalAlignment= VerticalAlignment.Top;
            comboBox.Margin = new Thickness(0, 15, 0, 0);

            comboBox.Items.Add("RadioButton");
            comboBox.Items.Add("CheckBox");
            comboBox.SelectedIndex = 0;
            comboBox.SelectionChanged += ComboBox_QuestionTypeChanged;
            return comboBox;
        }
        private Button CreateButton() //buttons delete/copy
        {
            Button button = new Button();
            button.Width = 20;
            button.Height = 20;
            button.VerticalAlignment = VerticalAlignment.Bottom;
            return button;
        }
        private void CreateDockPanel()
        {
            dockPanel = new DockPanel();
            dockPanel.Width = 400;
            dockPanel.Height = Double.NaN;
            dockPanel.Margin = new Thickness(0, 15, 0, 15);
            dockPanel.Background = Brushes.DarkGray;
            dockPanel.HorizontalAlignment= HorizontalAlignment.Stretch;
            //один обєкт додано по дефолту при створенні question

            MultipleObject multipleObject = new MultipleObject(QuestionType);
            multipleObject.ParentPanel = dockPanel; //привязка до контейнера dockPanel
            multipleObject.OnDelete += DeleteMultiObject; //підписка на подію видалення
            multipleObject.OnAnsverSelected += HandleAnswerSelected; //підписка на зміну кольору

            Grid row = multipleObject.CreateMultiObject(multipleObjects.Count + 1);
            dockPanel.Children.Add(row);
            multipleObjects.Add(multipleObject);
            DockPanel.SetDock(row, Dock.Top);
        }
        private void CreateDockPanel(List<MultipleObjectDTO> multipleObjectsDTO)
        {
            dockPanel = new DockPanel();
            dockPanel.Width = 400;
            dockPanel.Height = Double.NaN;
            dockPanel.Margin = new Thickness(0, 15, 0, 15);
            dockPanel.Background = Brushes.DarkGray;
            dockPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

            if(multipleObjectsDTO != null)
            {
                foreach(MultipleObjectDTO copyMO in multipleObjectsDTO)
                {
                    MultipleObject originalMO = new MultipleObject(copyMO.AnswerType);
                    originalMO.ParentPanel = dockPanel; //привязка до контейнера dockPanel
                    originalMO.OnDelete += DeleteMultiObject; //підписка на подію видалення
                    originalMO.OnAnsverSelected += HandleAnswerSelected; //підписка на зміну кольору

                    Grid row = originalMO.CreateMultiObject(copyMO.AnswerText, copyMO.State);
                    dockPanel.Children.Add(row);
                    multipleObjects.Add(originalMO);
                    DockPanel.SetDock(row, Dock.Top);
                }
            }
            CheckQuestionStatus();
        }
        private Button CreateButtonForAddingAnswers()
        {
            Button buttonAddAnswer = new Button();
            buttonAddAnswer.Name = "button_add_answer";
            buttonAddAnswer.Width = 200;
            buttonAddAnswer.Height = 20;
            buttonAddAnswer.Content = "press here to add new answer...";
            buttonAddAnswer.VerticalAlignment = VerticalAlignment.Bottom;
            buttonAddAnswer.Click += ButtonAddAnswer_Click;
            return buttonAddAnswer;
        }
        private void UpdateUIElementsInMO()
        {
            foreach(var multipleObject in  multipleObjects)
            {
                multipleObject.UpdateUI(QuestionType);
                multipleObject.UpdateAnswerState(MultipleObject.AnswerState.None);
            }
        }
        private void CheckQuestionStatus() //цей метод необхідний для встановлення статусу питання 
        {
            if (multipleObjects.Count >= 2)
            {
                if (QuestionType == "RadioButton")
                {
                    IsThatQuestionCompleted = multipleObjects.Any(item => item.State == MultipleObject.AnswerState.Correct);
                }
                else if (QuestionType == "CheckBox")
                {
                    bool hasCorrect = multipleObjects.Any(item => item.State == MultipleObject.AnswerState.Correct);
                    bool hasIncorrect = multipleObjects.Any(item => item.State == MultipleObject.AnswerState.Incorrect);
                    IsThatQuestionCompleted = hasCorrect && hasIncorrect || (hasCorrect && !hasIncorrect);
                }
                else IsThatQuestionCompleted = false;
            }
            else IsThatQuestionCompleted = false;
        }
        public void SetControlState(bool enable)
        {
            foreach (UIElement elem in qGrid.Children)
            {
                SetElementsState(elem, enable);
            }
            foreach (MultipleObject multipleObject in multipleObjects)
            {
                SetElementsState(multipleObject.Grid, enable);
            }
        }
        private void SetElementsState(UIElement element, bool enable)
        {
            if (element is Control control)
            {
                // Якщо розблоковуємо (enable = true), і елемент - RadioButton або CheckBox, то ігноруємо його
                if (enable && (control is RadioButton || control is CheckBox))
                {
                    return;
                }

                control.IsEnabled = enable;
            }

            if (element is Panel innerPanel)
            {
                foreach (UIElement child in innerPanel.Children)
                {
                    SetElementsState(child, enable);
                }
            }
        }
        public int GetNumberOfCorrects()
        {
            return multipleObjects.Count(mo => mo.State == MultipleObject.AnswerState.Correct);
        }
        //обробники подій
        private void ButtonAddAnswer_Click(object sender, RoutedEventArgs e) //додавання нової відповіді у обєкті multipleObject
        {
            MultipleObject multipleObject = new MultipleObject(QuestionType);
            multipleObject.ParentPanel = dockPanel; //привязка до контейнера dockPanel
            multipleObject.OnDelete += DeleteMultiObject; //підписка на подію видалення multiObject
            multipleObject.OnAnsverSelected += HandleAnswerSelected; //підписка на подію зміни властивості State у MultipleQuestion

            Grid row = multipleObject.CreateMultiObject(multipleObjects.Count + 1);
            multipleObjects.Add(multipleObject);
            dockPanel.Children.Add(row);

            //перевірка якщо хоча б один є сірим то всі інші будуть сірими якщо ні то інші будуть червоними
            if (multipleObjects.Any(mo => mo.State != MultipleObject.AnswerState.None))
            {
                multipleObject.UpdateAnswerState(MultipleObject.AnswerState.Incorrect);
            }

            DockPanel.SetDock(row, Dock.Top);

            CheckQuestionStatus();
        }
        private void DeleteMultiObject(MultipleObject multipleObject) //видалення обєкта multiobject
        {
            if(multipleObjects.Contains(multipleObject))
            {
                multipleObjects.Remove(multipleObject);

                if(multipleObject.ParentPanel != null && multipleObject.Grid != null)
                {
                    multipleObject.ParentPanel.Children.Remove(multipleObject.Grid);
                }

                if(QuestionType == "RadioButton" && multipleObject.State == MultipleObject.AnswerState.Correct)
                {
                    foreach(var item in multipleObjects)
                    {
                        item.UpdateAnswerState(MultipleObject.AnswerState.None);
                    }
                }
                MessageBox.Show("Multiobject was deleted");

                CheckQuestionStatus();
            }
        }
        private void ButtonDeleteQuestion_Click(object sender, RoutedEventArgs e) //видалення question по індексу делегатом
        {
            OnDelete?.Invoke(Index);
        }
        private void ButtonCopyQuestion_Click(object sender, RoutedEventArgs e) //копіювання обєкта question
        {
            OnQuestionCopied?.Invoke(GatherDataToDTO(), Index + 1);
        }
        private void ComboBox_QuestionTypeChanged(object sender, SelectionChangedEventArgs e) //зміна типу question між radioButton та CheckBox
        {
            if(sender is ComboBox comboBox)
            {
                if(comboBox.SelectedItem.ToString() == "RadioButton")
                {
                    QuestionType = "RadioButton";
                }
                else if(comboBox.SelectedItem.ToString() == "CheckBox")
                {
                    QuestionType = "CheckBox";
                }
                isFirstCheckBoxSelection = true;

                if (oneTimeIf) //флажок бо треба ігнорувати зміну isThatQuestionIsRecreated один раз
                {
                    oneTimeIf = false;
                }
                else
                {
                    isThatQuestionIsRecreated = false;
                }
                UpdateUIElementsInMO();

                CheckQuestionStatus();
            }
        }
        private void HandleAnswerSelected(MultipleObject selectedMulti)//зміна кольорку кнопки при натисканні кожному типу різні функції для radioButton/CheckBox
        {
            if(QuestionType == "RadioButton")
            {
                foreach(var item in multipleObjects)
                {
                    item.State = (item == selectedMulti) ? MultipleObject.AnswerState.Correct : MultipleObject.AnswerState.Incorrect;
                }
            }
            else if(QuestionType == "CheckBox")
            {
                if (isFirstCheckBoxSelection && !isThatQuestionIsRecreated) //логіка якщо обєкт копійовано то зелені в червоні не перефарбовуй
                {
                    isFirstCheckBoxSelection= false;
                    foreach(var item in multipleObjects)
                    {
                        item.State = (item == selectedMulti) ? MultipleObject.AnswerState.Correct : MultipleObject.AnswerState.Incorrect;
                    }
                }
                else
                {
                    selectedMulti.State = selectedMulti.State == MultipleObject.AnswerState.Correct ? MultipleObject.AnswerState.Incorrect : MultipleObject.AnswerState.Correct;
                }
            }
            foreach(var multiObj in multipleObjects)
            {
               multiObj.UpdateColor();
            }

            CheckQuestionStatus();
        }
    }
}
