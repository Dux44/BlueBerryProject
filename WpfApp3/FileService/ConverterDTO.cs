using BlueBerryProject.FormConstruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBerryProject.FormConstruct.QuestionsDTO;

namespace BlueBerryProject.FileService
{
    class ConverterDTO
    {
        public ConverterDTO()
        {

        }
        static public List<QuestionDTO> ConvertArrayQuestionsToDTO(List<Question> list)
        {
            List<QuestionDTO> dto = new List<QuestionDTO>();
            foreach (var quest in list)
            {
                dto.Add(ConvertToDTOQuestion(quest));
            }
            return dto;
        }
        static public List<Question> ConvertArrayQuestionsToDTO(List<QuestionDTO> dto)
        {
            List<Question> list = new List<Question>();
            foreach (var questDTO in dto)
            {
                list.Add(ConvertToQuestion(questDTO));
            }
            return list;
        }
        static private Question ConvertToQuestion(QuestionDTO dto)
        {
            Question question = new Question();
            //question.qHeight = dto.height;
            //question.qWidth = dto.width;
            //question.qMargin = dto.margin;
            return question;
        }
        static private QuestionDTO ConvertToDTOQuestion(Question question)
        {
            QuestionDTO dto = new QuestionDTO();
            //dto.height = question.qHeight;
            //dto.width = question.qWidth;
            //dto.margin = question.qMargin;
            return dto;
        }
    }
}
