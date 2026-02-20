using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETEC510.Cases
{
    [CreateAssetMenu(menuName = "ETEC510/Case Data", fileName = "CaseData")]
    public class CaseData : ScriptableObject
    {
        [Header("Identity")]
        public string CaseId = "case01";
        public string Title = "Case 01";
        [TextArea(3, 12)]
        public string BriefingText;

        [Header("Evidence")]
        public List<EvidenceItem> Evidence = new();

        [Header("Questions")]
        public List<ChoiceQuestion> Questions = new();

        [Header("Rewards")]
        public int XpForCompletion = 100;
    }

    [Serializable]
    public class EvidenceItem
    {
        public string Title;
        [TextArea(2, 8)]
        public string Description;

        [Tooltip("Optional: screenshot, image, or illustration.")]
        public Sprite Image;

        [Tooltip("Optional: where did this come from?")]
        public string Source;

        [Tooltip("Optional: when was it posted/created?")]
        public string Date;
    }

    [Serializable]
    public class ChoiceQuestion
    {
        [TextArea(2, 8)]
        public string Prompt;

        public List<string> Options = new();

        [Tooltip("Index into Options")]
        public int CorrectIndex = 0;

        [TextArea(2, 10)]
        public string FeedbackCorrect;

        [TextArea(2, 10)]
        public string FeedbackIncorrect;

        [Header("Critical Thinking Tags (optional)")]
        public bool ClaimVsEvidence;
        public bool CredibilityCheck;
        public bool BiasOrIntent;
        public bool WhoBenefits;
    }
}
