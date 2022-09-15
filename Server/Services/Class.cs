using System.Xml.Linq;

namespace BlazorWebAssemblySignalRApp.Server.Services
{
    public class XX
    {
        public void X()
        {
            //new Edge(new Person(), )
            //Teacher.AllTrainingsRelations.Where

            // Person (Student) - own trainings
            // 
        }

        public class Node
        {
            public List<Edge> Edges { get; set; } = new();
        }

        public class TypeNode : Node
        {
            public string Name { get; set; } = string.Empty;
        }


        public class Training
        {
            public Person Person { get; set; }
            public string Id { get; set; }
        }
        public class Person : Node
        {
            public List<IPersonRole> Roles { get; set; } = new();
        }

        public interface IPersonRole
        {
            //public Person Person { get; set; }
        }

        public class Trainee : IPersonRole
        {
            public List<Training> Trainings { get; set; } = new();
        }

        public class Guardian : IPersonRole
        {
            public List<Person> Children { get; set; } = new();
        }

        public class Student : IPersonRole
        {
            public List<Class> Classes { get; set; } = new();
            public Dictionary<Class, List<Training>> Trainings { get; set; } = new();
        }

        public enum ClassRole
        {
            Teacher,
            Assistant,
            Admin
        }

        public class Teacher : IPersonRole
        {
            public Dictionary<Class, ClassRole> Classes { get; set; } = new();
            public Dictionary<Person, List<Training>> SupervisedDirectTrainings { get; set; } = new();
            //public Dictionary<Training>
        }


        public class School : Node
        {
            public string Name { get; set; } = string.Empty;
            public List<Class> Classes { get; set; } = new();
        }

        public class Class : Node
        {
            public Teacher Teacher { get; set; }
            public School School { get; set; }
            public int Grade { get; set; }
            public string Name { get; set; }
            public List<Student> Students { get; set; } = new();
        }

        //public class Edge
        //{
        //    public Node From { get; set; }
        //    public Node From { get; set; }
        //}

        public readonly record struct Edge(Node From, Node To);
    }
}
