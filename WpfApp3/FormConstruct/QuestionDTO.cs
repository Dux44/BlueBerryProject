using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace BlueBerryProject.FormConstruct
{
    public class QuestionsDTO
    {
        public class QuestionDTO
        {
            public QuestionDTO() 
            {
                
            }
            [JsonRequired]
            public string QuestionType { get; set; } // radioButton або checkBox
            [JsonRequired]
            public string QuestionText { get; set; } // текст питання ГОЛОВНИЙ ТЕКСТОВИЙ ОБЄКТ ВГОРІ 
            public int SelectedIndex { get; set; } // для comboBox щоб встановити правильний обраний обєкт по дефолту -1 а треба або 0 або 1 
            [JsonRequired]
            public List<MultipleObjectsDTO.MultipleObjectDTO> multipleObjectsDTO { get; set; } // масив необхідних даних мультиобєктів
        }
    }
}
