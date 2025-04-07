using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueBerryProject.FormConstruct.QuestionsDTO;
using Newtonsoft.Json;
using BlueBerryProject.FormConstruct;
using System.Windows;

namespace BlueBerryProject.FileService
{
    class FileSerealizer
    {
        private string path;

        public FileSerealizer(string path)
        {
            this.path = path;
        }

        public ProjectDataDTO LoadData()
        {
            if (!File.Exists(path))
            {
                return new ProjectDataDTO
                {
                    Description = "",
                    Questions = new List<QuestionsDTO.QuestionDTO>(),
                    Response = new ResponseDTO()
                };
            }

            try
            {
                using (var reader = File.OpenText(path))
                {
                    var fileText = reader.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<ProjectDataDTO>(fileText);

                    // Захист від null
                    return data ?? new ProjectDataDTO
                    {
                        Description = "",
                        Questions = new List<QuestionsDTO.QuestionDTO>(),
                        Response = new ResponseDTO()
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка десеріалізації: " + ex.Message);
                return new ProjectDataDTO
                {
                    Description = "",
                    Questions = new List<QuestionsDTO.QuestionDTO>(),
                    Response = new ResponseDTO()
                };
            }
        }

        public void SaveData(ProjectDataDTO projectData)
        {
            try
            {
                string output = JsonConvert.SerializeObject(projectData, Formatting.Indented);

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(output);
                }
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Помилка доступу до файлу: " + ioEx.Message);
                
            }
            catch (JsonException jsonEx)
            {
                MessageBox.Show("Помилка серіалізації JSON: " + jsonEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Невідома помилка при збереженні: " + ex.Message);
            }
        }
    }
}
