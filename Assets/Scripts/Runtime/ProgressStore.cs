using UnityEngine;

namespace ETEC510.Runtime
{
    public static class ProgressStore
    {
        private const string XpKey = "etec510_xp";

        public static int GetXp() => PlayerPrefs.GetInt(XpKey, 0);

        public static void AddXp(int amount)
        {
            var xp = GetXp();
            xp += Mathf.Max(0, amount);
            PlayerPrefs.SetInt(XpKey, xp);
            PlayerPrefs.Save();
        }

        public static bool IsCaseCompleted(string caseId)
        {
            return PlayerPrefs.GetInt(CaseKey(caseId), 0) == 1;
        }

        public static void MarkCaseCompleted(string caseId)
        {
            PlayerPrefs.SetInt(CaseKey(caseId), 1);
            PlayerPrefs.Save();
        }

        private static string CaseKey(string caseId) => $"etec510_case_completed_{caseId}";
    }
}
