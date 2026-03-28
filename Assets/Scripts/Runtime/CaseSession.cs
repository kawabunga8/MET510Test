using ETEC510.Cases;

namespace ETEC510.Runtime
{
    public class CaseSession
    {
        public CaseData Case { get; }

        public bool SpotTheClueCompleted { get; private set; }
        public bool GutCheckCompleted    { get; private set; }
        public bool FindTheMotiveCompleted { get; private set; }

        public bool AllCluesFound =>
            SpotTheClueCompleted && GutCheckCompleted && FindTheMotiveCompleted;

        public string CollectedCode
        {
            get
            {
                var code = "";
                if (SpotTheClueCompleted)    code += Case.SpotTheClue.CodeDigit;
                if (GutCheckCompleted)       code += Case.GutCheck.CodeDigit;
                if (FindTheMotiveCompleted)  code += Case.FindTheMotive.CodeDigit;
                return code;
            }
        }

        public bool IsComplete { get; private set; }

        public CaseSession(CaseData @case)
        {
            Case = @case;
        }

        // Called when player clicks "I found it!" on the Spot the Clue screen
        public void CompleteSpotTheClue() => SpotTheClueCompleted = true;

        public void CompleteGutCheck() => GutCheckCompleted = true;

        // Called when player selects an answer on Gut Check
        public (bool isCorrect, string feedback) AnswerGutCheck(int selectedIndex)
        {
            var step = Case.GutCheck;
            var isCorrect = selectedIndex == step.CorrectIndex;
            if (isCorrect) GutCheckCompleted = true;
            return (isCorrect, isCorrect ? step.FeedbackCorrect : step.FeedbackIncorrect);
        }

        // Called when player clicks "I found them!" on Find the Motive screen
        public void CompleteMotive() => FindTheMotiveCompleted = true;

        // Returns true if the entered password matches
        public bool ValidatePassword(string input) =>
            !string.IsNullOrEmpty(input) && input.Trim() == Case.Password;

        // Called when player selects Real or Fake on the Evidence Detail screen
        public (bool isCorrect, string feedback) SubmitVerdict(int selectedIndex)
        {
            var step = Case.Verdict;
            var isCorrect = selectedIndex == step.CorrectIndex;
            return (isCorrect, isCorrect ? step.FeedbackCorrect : step.FeedbackIncorrect);
        }

        public void CompleteCase()
        {
            if (Case == null || IsComplete) return;
            IsComplete = true;
            ProgressStore.MarkCaseCompleted(Case.CaseId);
            ProgressStore.AddXp(Case.XpForCompletion);
        }
    }
}
