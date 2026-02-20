using System;
using ETEC510.Cases;

namespace ETEC510.Runtime
{
    public class CaseSession
    {
        public CaseData Case { get; }
        public int CurrentQuestionIndex { get; private set; }
        public int CorrectCount { get; private set; }
        public bool IsComplete => CurrentQuestionIndex >= (Case?.Questions?.Count ?? 0);

        public CaseSession(CaseData @case)
        {
            Case = @case;
            CurrentQuestionIndex = 0;
            CorrectCount = 0;
        }

        public ChoiceQuestion CurrentQuestion()
        {
            if (Case == null || Case.Questions == null) return null;
            if (CurrentQuestionIndex < 0 || CurrentQuestionIndex >= Case.Questions.Count) return null;
            return Case.Questions[CurrentQuestionIndex];
        }

        public (bool isCorrect, string feedback) Answer(int selectedIndex)
        {
            var q = CurrentQuestion();
            if (q == null) return (false, "No question loaded.");

            var isCorrect = selectedIndex == q.CorrectIndex;
            if (isCorrect) CorrectCount++;

            var feedback = isCorrect ? q.FeedbackCorrect : q.FeedbackIncorrect;
            CurrentQuestionIndex++;
            return (isCorrect, feedback);
        }

        public void CompleteCase()
        {
            if (Case == null) return;
            ProgressStore.MarkCaseCompleted(Case.CaseId);
            ProgressStore.AddXp(Case.XpForCompletion);
        }
    }
}
