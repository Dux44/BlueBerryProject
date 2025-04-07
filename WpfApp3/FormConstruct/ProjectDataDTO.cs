using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueBerryProject.FormConstruct;

namespace BlueBerryProject.FormConstruct
{
    public class ProjectDataDTO
    {
        public string Description { get; set; }
        public List<QuestionsDTO.QuestionDTO> Questions { get; set; }
        public ResponseDTO Response { get; set; }
    }
}
