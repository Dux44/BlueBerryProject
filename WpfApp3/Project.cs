using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueBerryProject
{
    class Project
    {
        private DateTime dateOfLastChange; //остання зміна форми (щось подібне у ворд є)
        private string nameOfProject; //ім'я про'єкту
        private string path; //шлях до json файлу

        public Project(string name, string path)
        {
            nameOfProject = name;
            this.path = path;

        }
        public string Name
        {
            get { return nameOfProject; }
            set { nameOfProject = value; }
        }
        public string DateOfLastChange
        {
            get
            {
                FileInfo fileInfo = new FileInfo(path);
                dateOfLastChange = fileInfo.LastWriteTime;
                return dateOfLastChange.ToString();
            }
        }
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
       
    }
}
