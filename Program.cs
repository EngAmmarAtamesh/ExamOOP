using System;
using System.Collections.Generic;

namespace ExaminationSystem
{
    public static class InputHelper
    {
        public static string ReadString(string prompt)
        {
            string value;
            do
            {
                Console.Write(prompt);
                value = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(value));
            return value.Trim();
        }

        public static int ReadInt(string prompt, int min, int max)
        {
            int value;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"  Invalid input, enter a number between {min} and {max}.");
            }
        }
    }

    public class Answer : ICloneable, IComparable<Answer>
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }

        public Answer() : this(0, string.Empty) { }

        public Answer(int answerId, string answerText)
        {
            AnswerId = answerId;
            AnswerText = answerText;
        }

        public object Clone() => new Answer(AnswerId, AnswerText);

        public int CompareTo(Answer other)
        {
            if (other == null) return 1;
            return AnswerId.CompareTo(other.AnswerId);
        }

        public override string ToString() => $"{AnswerId}. {AnswerText}";
    }

    public abstract class Question : ICloneable, IComparable<Question>
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public int Mark { get; set; }
        public Answer[] AnswerList { get; set; }  
        public Answer RightAnswer { get; set; }

        protected Question() : this(string.Empty, string.Empty, 0) { }

        protected Question(string header, string body, int mark)
            : this(header, body, mark, new Answer[0], null) { }

        protected Question(string header, string body, int mark, Answer[] answerList, Answer rightAnswer)
        {
            Header = header;
            Body = body;
            Mark = mark;
            AnswerList = answerList;
            RightAnswer = rightAnswer;
        }

        public abstract void Create();

        public virtual void Display()
        {
            Console.WriteLine($"{Header}  (Mark: {Mark})");
            Console.WriteLine(Body);
            foreach (Answer a in AnswerList)
                Console.WriteLine($"   {a}");
        }

        protected void ReadCommonData()
        {
            Header = InputHelper.ReadString("Question header : ");
            Body = InputHelper.ReadString("Question body   : ");
            Mark = InputHelper.ReadInt("Question mark   : ", 1, 100);
        }

        // Deep Clone
        public object Clone()
        {
            Question copy = (Question)MemberwiseClone();
            copy.AnswerList = new Answer[AnswerList.Length];
            for (int i = 0; i < AnswerList.Length; i++)
                copy.AnswerList[i] = (Answer)AnswerList[i].Clone();

            copy.RightAnswer = RightAnswer == null
                ? null
                : Array.Find(copy.AnswerList, a => a.AnswerId == RightAnswer.AnswerId);
            return copy;
        }

       
        public int CompareTo(Question other)
        {
            if (other == null) return 1;
            return Mark.CompareTo(other.Mark);
        }

        public override string ToString() =>
            $"[{GetType().Name}] {Header} - {Body} ({Mark} marks)";
    }

    // ============================================================
    //  True / False  (Final Exam only)
    // ============================================================
    public class TrueFalseQuestion : Question
    {
        public TrueFalseQuestion() : base()
        {
            AnswerList = new Answer[]
            {
                new Answer(1, "True"),
                new Answer(2, "False")
            };
        }

        public TrueFalseQuestion(string header, string body, int mark, bool correctValue)
            : this()
        {
            Header = header;
            Body = body;
            Mark = mark;
            RightAnswer = correctValue ? AnswerList[0] : AnswerList[1];
        }

        public override void Create()
        {
            Console.WriteLine("--- True/False Question ---");
            ReadCommonData();
            int right = InputHelper.ReadInt("Right answer (1 = True, 2 = False) : ", 1, 2);
            RightAnswer = AnswerList[right - 1];
        }
    }

    // ============================================================
    //  MCQ (Choose one answer)  (Final + Practical)
    // ============================================================
    public class MCQQuestion : Question
    {
        public MCQQuestion() : base() { }

        public MCQQuestion(string header, string body, int mark, Answer[] choices, Answer rightAnswer)
            : base(header, body, mark, choices, rightAnswer) { }

        public override void Create()
        {
            Console.WriteLine("--- MCQ Question ---");
            ReadCommonData();

            int count = InputHelper.ReadInt("Number of choices (2-6) : ", 2, 6);
            AnswerList = new Answer[count];
            for (int i = 0; i < count; i++)
            {
                string text = InputHelper.ReadString($"  Choice {i + 1} : ");
                AnswerList[i] = new Answer(i + 1, text);
            }

            int right = InputHelper.ReadInt($"Right answer (1-{count}) : ", 1, count);
            RightAnswer = AnswerList[right - 1];
        }
    }

    // ============================================================
    //  6. Base Exam class
    // ============================================================
    public abstract class Exam : ICloneable, IComparable<Exam>
    {
        public int Time { get; set; }               
        public int NumberOfQuestions { get; set; }
        public Subject Subject { get; set; }         
        public Question[] Questions { get; set; }
        public int[] StudentAnswers { get; protected set; }  

        protected Exam() : this(0, 0) { }

        protected Exam(int time, int numberOfQuestions)
        {
            Time = time;
            NumberOfQuestions = numberOfQuestions;
            Questions = new Question[numberOfQuestions];
            StudentAnswers = new int[numberOfQuestions];
        }

        protected Exam(int time, int numberOfQuestions, Subject subject)
            : this(time, numberOfQuestions)
        {
            Subject = subject;
        }

        public abstract void ShowExam();

        protected abstract Question CreateQuestion(int number);

        public void CreateQuestions()
        {
            for (int i = 0; i < NumberOfQuestions; i++)
            {
                Console.WriteLine($"\n===== Question {i + 1} of {NumberOfQuestions} =====");
                Question q = CreateQuestion(i + 1);
                q.Create();
                Questions[i] = q;
            }
        }

        protected void TakeExam()
        {
            Console.WriteLine("\n==============================================");
            Console.WriteLine($"Subject : {Subject?.SubjectName}");
            Console.WriteLine($"Time    : {Time} minutes   |   Questions : {NumberOfQuestions}");
            Console.WriteLine("==============================================");

            for (int i = 0; i < Questions.Length; i++)
            {
                Console.WriteLine($"\nQ{i + 1}:");
                Questions[i].Display();
                StudentAnswers[i] = InputHelper.ReadInt("Your answer (id) : ", 1, Questions[i].AnswerList.Length);
            }
        }

        public int TotalMark()
        {
            int total = 0;
            foreach (Question q in Questions) total += q.Mark;
            return total;
        }

        public virtual object Clone()
        {
            Exam copy = (Exam)MemberwiseClone();
            copy.Questions = new Question[Questions.Length];
            for (int i = 0; i < Questions.Length; i++)
                copy.Questions[i] = (Question)Questions[i].Clone();
            copy.StudentAnswers = (int[])StudentAnswers.Clone();
            return copy;
        }

    
        public int CompareTo(Exam other)
        {
            if (other == null) return 1;
            return TotalMark().CompareTo(other.TotalMark());
        }

        public override string ToString() =>
            $"{GetType().Name} | Subject: {Subject?.SubjectName} | Time: {Time} min | Questions: {NumberOfQuestions}";
    }

    // ============================================================
    //  9. Final Exam: shows Questions, Answers and Grade
    // ============================================================
    public class FinalExam : Exam
    {
        public FinalExam() : base() { }

        public FinalExam(int time, int numberOfQuestions, Subject subject)
            : base(time, numberOfQuestions, subject) { }

        // Final: True/False  أو  MCQ
        protected override Question CreateQuestion(int number)
        {
            int type = InputHelper.ReadInt("Question type (1 = True/False, 2 = MCQ) : ", 1, 2);
            return type == 1 ? new TrueFalseQuestion() : (Question)new MCQQuestion();
        }

        public override void ShowExam()
        {
            TakeExam();

            Console.WriteLine("\n================= FINAL EXAM RESULT =================");
            int grade = 0;
            for (int i = 0; i < Questions.Length; i++)
            {
                Question q = Questions[i];
                Answer chosen = q.AnswerList[StudentAnswers[i] - 1];
                bool correct = chosen.AnswerId == q.RightAnswer.AnswerId;
                if (correct) grade += q.Mark;

                Console.WriteLine($"\nQ{i + 1}: {q.Header}");
                Console.WriteLine($"   {q.Body}");
                Console.WriteLine($"   Your answer : {chosen.AnswerText}");
            }

            Console.WriteLine("\n-----------------------------------------------------");
            Console.WriteLine($"Grade : {grade} / {TotalMark()}");
            Console.WriteLine("=====================================================");
        }
    }

    // ============================================================
    //  8. Practical Exam: shows the right answer after finishing
    // ============================================================
    public class PracticalExam : Exam
    {
        public PracticalExam() : base() { }

        public PracticalExam(int time, int numberOfQuestions, Subject subject)
            : base(time, numberOfQuestions, subject) { }

        protected override Question CreateQuestion(int number)
        {
            return new MCQQuestion();
        }

        public override void ShowExam()
        {
            TakeExam();

            Console.WriteLine("\n=============== PRACTICAL EXAM - RIGHT ANSWERS ===============");
            for (int i = 0; i < Questions.Length; i++)
            {
                Question q = Questions[i];
                Console.WriteLine($"\nQ{i + 1}: {q.Header}");
                Console.WriteLine($"   {q.Body}");
                Console.WriteLine($"   Right answer : {q.RightAnswer}");
            }
            Console.WriteLine("==============================================================");
        }
    }

    // ============================================================
    //  7. Subject class
    // ============================================================
    public class Subject : ICloneable, IComparable<Subject>
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Exam Exam { get; set; }

        public Subject() : this(0, string.Empty) { }

        public Subject(int subjectId, string subjectName)
        {
            SubjectId = subjectId;
            SubjectName = subjectName;
        }

        public void CreateExam()
        {
            Console.WriteLine($"\n##### Creating exam for subject: {SubjectName} #####");
            int type = InputHelper.ReadInt("Exam type (1 = Final, 2 = Practical) : ", 1, 2);
            int time = InputHelper.ReadInt("Exam time in minutes (1-300)        : ", 1, 300);
            int count = InputHelper.ReadInt("Number of questions (1-50)           : ", 1, 50);

            if (type == 1)
                Exam = new FinalExam(time, count, this);
            else
                Exam = new PracticalExam(time, count, this);

            Exam.CreateQuestions();
        }

        public object Clone()
        {
            Subject copy = (Subject)MemberwiseClone();
            copy.Exam = Exam == null ? null : (Exam)Exam.Clone();
            if (copy.Exam != null) copy.Exam.Subject = copy;
            return copy;
        }

        public int CompareTo(Subject other)
        {
            if (other == null) return 1;
            return SubjectId.CompareTo(other.SubjectId);
        }

        public override string ToString() => $"Subject {SubjectId}: {SubjectName}";
    }

  
    internal class Program
    {
        static void Main(string[] args)
        {
       
            Subject subject = new Subject(1, "OOP using C#");

            subject.CreateExam();

            Console.WriteLine("\n" + subject);
            Console.WriteLine(subject.Exam);

            subject.Exam.ShowExam();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
