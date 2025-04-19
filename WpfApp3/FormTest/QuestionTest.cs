using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using BlueBerryProject.FormConstruct;
using WpfApp3;

namespace BlueBerryProject.FormTest
{
    internal class QuestionTest : GroupBox
    {
        private Grid Grid; //сітка для розміщення обєктів
        private List<MultipleObjectTest> answers = new List<MultipleObjectTest>(); //список відпоівідей

        public Action? OnSelectionChanged { get; set; } //
        public QuestionTest(int countQustionTests)
        {
            Header = $"{countQustionTests}";
            Height = Double.NaN;
            Width = 450;
            Margin = new Thickness(20, 10, 0, 0);
            CreateGrid();
        }

        private void CreateGrid()
        {
            Grid = new Grid();
            Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
        }
        public void ShowAllItems(QuestionsDTO.QuestionDTO questionDTO)
        {
            TextBox questionText = new TextBox()
            {
                Text = questionDTO.QuestionText,
                Height = Double.NaN,
                Width = 400,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 10, 0, 10),
                IsReadOnly = true,
            };
            Grid.SetRow(questionText, 0);
            Grid.Children.Add(questionText);

            DockPanel panel = CreatePanelForMultiObj(questionDTO);
            Grid.SetRow(panel, 1);
            Grid.Children.Add(panel);

            Content = Grid;
        }
       
        private DockPanel CreatePanelForMultiObj(QuestionsDTO.QuestionDTO questionDTO)
        {
            DockPanel dockPanel = new DockPanel();
            dockPanel.Margin = new Thickness(0, 0, 0, 0);
            dockPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

            foreach(var mo_dto in questionDTO.multipleObjectsDTO)
            {
                StackPanel answerContainer = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 0),
                };
                Control selector = new Control();
                if(questionDTO.QuestionType == "RadioButton")
                {
                    selector = CreateRadioButton();
                }
                else if(questionDTO.QuestionType == "CheckBox") 
                {
                    selector = CreateCheckBox();
                }
                TextBox answerText = CreateTextBoxAnswer();
                answerText.Text = mo_dto.AnswerText.ToString();

                MultipleObjectTest mo = new MultipleObjectTest();
                mo.AnswerText = mo_dto.AnswerText;
                mo.State = mo_dto.State;
                mo.Selector = selector;
                answers.Add(mo);

                answerContainer.Children.Add(selector);
                answerContainer.Children.Add(answerText);

                DockPanel.SetDock(answerContainer, Dock.Top);
                dockPanel.Children.Add(answerContainer);
            }
            return dockPanel;
        }
        
        private CheckBox CreateCheckBox()
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Width = 20;
            checkBox.Height = 20;
            checkBox.Margin = new Thickness(10, 0, 10, 0);
            checkBox.Checked += SelectorChanged;
            checkBox.Unchecked += SelectorChanged;
            return checkBox;
        }
        private RadioButton CreateRadioButton()
        {
            RadioButton radioButton = new RadioButton()
            {
                GroupName = $"question_group_{Header}"
            };
            radioButton.Width = 20;
            radioButton.Height = 20;
            radioButton.Margin = new Thickness(10, 0, 10, 0);
            radioButton.Checked += SelectorChanged;
            return radioButton;
        }
        public bool HasSelectorAnswer()
        {
            return answers.Any(a=> (a.Selector as CheckBox)?.IsChecked == true || (a.Selector as RadioButton)?.IsChecked == true);
        }
        private TextBox CreateTextBoxAnswer()
        {
            TextBox textBoxAnswer = new TextBox();
            textBoxAnswer.Width = 350;
            textBoxAnswer.Height = Double.NaN;
            textBoxAnswer.TextWrapping = TextWrapping.Wrap;
            textBoxAnswer.Margin = new Thickness(0, 0, 10, 0);
            textBoxAnswer.IsReadOnly = true;
            return textBoxAnswer;
        }
        private void SelectorChanged(object sender, RoutedEventArgs e) 
        {
            OnSelectionChanged?.Invoke();
        }
        public void SetStateSelector(bool state)
        {
            foreach (var answer in answers)
            {
                if (answer.Selector is RadioButton rb)
                    rb.IsEnabled = state;
                else if (answer.Selector is CheckBox cb)
                    cb.IsEnabled = state;
            }
        }
        public int CountCorrectsSelectedAnswers()
        {
            int count = 0;
            foreach(var answer in answers)
            {
                bool isSelected = false;

                if(answer.Selector is RadioButton rb)
                {
                    isSelected = rb.IsChecked == true;
                }
                else if(answer.Selector is CheckBox check)
                {
                    isSelected = check.IsChecked == true;
                }

                if(isSelected && answer.State == MultipleObject.AnswerState.Correct)
                {
                    count++;
                }
                else if(isSelected && answer.Selector is CheckBox && answer.State == MultipleObject.AnswerState.Incorrect)
                {
                    count--;
                }
            }
            return count;
        }
        public void ClearChoises()
        {
            foreach (var answer in answers)
            {
                if (answer.Selector is RadioButton rb)
                    rb.IsChecked = false;
                else if (answer.Selector is CheckBox cb)
                    cb.IsChecked = false;
            }
        }
    }
}
