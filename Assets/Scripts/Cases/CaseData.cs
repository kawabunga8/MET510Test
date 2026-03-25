using System;
using UnityEngine;
using UnityEngine.Video;

namespace ETEC510.Cases
{
    [CreateAssetMenu(menuName = "ETEC510/Case Data", fileName = "CaseData")]
    public class CaseData : ScriptableObject
    {
        [Header("Identity")]
        public string CaseId = "case01";
        public string Title = "Case 01";

        [Header("Intro Video")]
        public VideoClip IntroVideo;
        public AudioClip IntroMusic;

        [Header("Background Music")]
        public AudioClip MainMusic;

        [Header("Mission Briefing")]
        [TextArea(3, 12)]
        public string BriefingText;
        public Sprite BriefingImage;
        public AudioClip BriefingAudio;

        [Header("Evidence Board")]
        public Sprite EvidenceBoardImage;

        [Header("Spot the Clue  (reveals digit 1 of code)")]
        public SpotTheClueStep SpotTheClue;

        [Header("Gut Check  (reveals digit 2 of code)")]
        public GutCheckStep GutCheck;

        [Header("Find the Motive  (reveals digit 3 of code)")]
        public FindTheMotiveStep FindTheMotive;

        [Header("Password Lock")]
        public string Password = "510";

        [Header("Evidence Detail — Verdict")]
        public VerdictStep Verdict;

        [Header("Hints from the Chief")]
        public VideoClip HintVideo;
        [TextArea(2, 8)]
        public string HintText;
        public Sprite HintImage;
        public AudioClip HintAudio;

        [Header("Level Complete")]
        [TextArea(2, 8)]
        public string CompletionText;
        public Sprite CompletionImage;
        public AudioClip CompletionAudio;

        [Header("Rewards")]
        public int XpForCompletion = 100;
    }

    [Serializable]
    public class SpotTheClueStep
    {
        [TextArea(2, 8)]
        public string Prompt;
        public Sprite EvidenceImage;
        public AudioClip Audio;
        [TextArea(2, 6)]
        public string ExplanationText;
        public string CodeDigit = "5";
    }

    [Serializable]
    public class GutCheckStep
    {
        [TextArea(2, 8)]
        public string Prompt;
        public Sprite EvidenceImage;
        public AudioClip Audio;
        public string[] Options = { "Yes, it looks real", "No, it looks fake" };
        public int CorrectIndex = 1;
        [TextArea(2, 6)]
        public string FeedbackCorrect;
        [TextArea(2, 6)]
        public string FeedbackIncorrect;
        public string CodeDigit = "1";
    }

    [Serializable]
    public class FindTheMotiveStep
    {
        [TextArea(2, 8)]
        public string Prompt;
        public Sprite EvidenceImage;
        public AudioClip Audio;
        [TextArea(2, 6)]
        public string ExplanationText;
        public string CodeDigit = "0";
    }

    [Serializable]
    public class VerdictStep
    {
        [TextArea(2, 8)]
        public string Prompt;
        public Sprite EvidenceImage;
        public AudioClip Audio;
        public string[] Options = { "Real", "Fake" };
        public int CorrectIndex = 1;
        [TextArea(2, 6)]
        public string FeedbackCorrect;
        [TextArea(2, 6)]
        public string FeedbackIncorrect;
    }
}
