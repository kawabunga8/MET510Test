using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using ETEC510.Cases;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Populate Case 01
    /// Fills Case01_AIImageCredibility with playable text content.
    /// Run once; safe to re-run (overwrites text fields only, leaves sprites/audio untouched).
    /// </summary>
    public static class CasePopulator
    {
        [MenuItem("ETEC510/Populate Case 01")]
        public static void PopulateCase01()
        {
            var data = AssetDatabase.LoadAssetAtPath<CaseData>(
                "Assets/Cases/Case01_AIImageCredibility.asset");

            if (data == null)
            {
                Debug.LogError("ETEC510: Case01_AIImageCredibility.asset not found.");
                return;
            }

            var so = new SerializedObject(data);

            // ── Intro Video ──────────────────────────────────────────────────
            var introVideo = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Video/StartupVideo.mp4");
            if (introVideo == null) Debug.LogWarning("ETEC510: StartupVideo.mp4 not found.");
            so.FindProperty("IntroVideo").objectReferenceValue = introVideo;

            var introMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ETEC510 Critical Thinking v2.mp3");
            if (introMusic == null) Debug.LogWarning("ETEC510: Intro music not found.");
            so.FindProperty("IntroMusic").objectReferenceValue = introMusic;

            var mainMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ETEC510 Critical Thinking.mp3");
            if (mainMusic == null) Debug.LogWarning("ETEC510: Main music not found.");
            so.FindProperty("MainMusic").objectReferenceValue = mainMusic;

            // ── Identity ─────────────────────────────────────────────────────
            so.FindProperty("CaseId").stringValue  = "case01_ai_image";
            so.FindProperty("Title").stringValue   = "Case 01: Is This Image Real?";

            // ── Mission Briefing ──────────────────────────────────────────────
            so.FindProperty("BriefingText").stringValue =
                "A photo of a protest has gone viral. Thousands of people are sharing it—" +
                "but something feels off.\n\n" +
                "You're the Lead Detective. Your mission: investigate this image and decide " +
                "whether it's trustworthy before it spreads any further. " +
                "Collect three clues to unlock the evidence vault, then deliver your verdict.";

            // ── Spot the Clue  (digit 1 → "5") ───────────────────────────────
            var spot = so.FindProperty("SpotTheClue");
            spot.FindPropertyRelative("Prompt").stringValue =
                "LOOK CLOSELY at the image.\n\n" +
                "AI-generated images often hide a tell-tale flaw. " +
                "Can you find the visual glitch that gives it away?\n\n" +
                "Common signs: distorted hands or fingers, impossible shadows, " +
                "blurry or melting background details, text that makes no sense.";
            spot.FindPropertyRelative("ExplanationText").stringValue =
                "Nice detective work! This image shows hands with an unusual number of fingers—" +
                "a classic AI artefact. Current AI image generators still struggle to render " +
                "human hands correctly. That's Clue 1: the visual glitch.";
            spot.FindPropertyRelative("CodeDigit").stringValue = "5";

            // ── Gut Check  (digit 2 → "1") ────────────────────────────────────
            var gut = so.FindProperty("GutCheck");
            gut.FindPropertyRelative("Prompt").stringValue =
                "Before sharing this image, what is your GUT REACTION?\n\n" +
                "Does the scene feel realistic? Is the lighting consistent? " +
                "Are the faces too perfect—or strangely blank?\n\n" +
                "What's your verdict right now?";
            // Options array
            var gutOptions = gut.FindPropertyRelative("Options");
            gutOptions.arraySize = 2;
            gutOptions.GetArrayElementAtIndex(0).stringValue = "It looks real to me";
            gutOptions.GetArrayElementAtIndex(1).stringValue = "Something feels off — probably AI";
            gut.FindPropertyRelative("CorrectIndex").intValue = 1;
            gut.FindPropertyRelative("FeedbackCorrect").stringValue =
                "Good instinct! The too-perfect lighting, oddly blank expressions, and " +
                "background artefacts are giveaways. Trusting your gut — then verifying — " +
                "is a key media-literacy skill.";
            gut.FindPropertyRelative("FeedbackIncorrect").stringValue =
                "Look again. The lighting is unnaturally even, faces lack micro-expressions, " +
                "and background details blur or repeat. AI generators often produce images " +
                "that feel 'clean' in a way real photos don't.";
            gut.FindPropertyRelative("CodeDigit").stringValue = "1";

            // ── Find the Motive  (digit 3 → "0") ──────────────────────────────
            var motive = so.FindProperty("FindTheMotive");
            motive.FindPropertyRelative("Prompt").stringValue =
                "WHO would create and spread this image — and WHY?\n\n" +
                "Think about: Who benefits if people believe this protest happened (or didn't)? " +
                "What emotion is this image designed to trigger? " +
                "Could this be used to manipulate public opinion?\n\n" +
                "Tap 'I Found Them!' once you've worked it out.";
            motive.FindPropertyRelative("ExplanationText").stringValue =
                "Whoever made this image wanted to stir strong emotions about a protest — " +
                "possibly to make it look bigger, smaller, or more violent than it was. " +
                "That's a form of disinformation: using fake visuals to shape what " +
                "people believe and feel. Always ask: Who benefits from me believing this?";
            motive.FindPropertyRelative("CodeDigit").stringValue = "0";

            // ── Password ──────────────────────────────────────────────────────
            so.FindProperty("Password").stringValue = "510";

            // ── Verdict ────────────────────────────────────────────────────────
            var verdict = so.FindProperty("Verdict");
            verdict.FindPropertyRelative("Prompt").stringValue =
                "You've collected all three clues.\n\n" +
                "Visual glitch [OK]  |  Gut check [OK]  |  Motive identified [OK]\n\n" +
                "Time to deliver your official verdict:\n" +
                "Is this image REAL or FAKE?";
            var verdictOptions = verdict.FindPropertyRelative("Options");
            verdictOptions.arraySize = 2;
            verdictOptions.GetArrayElementAtIndex(0).stringValue = "REAL — I'd share it";
            verdictOptions.GetArrayElementAtIndex(1).stringValue = "FAKE — AI-generated";
            verdict.FindPropertyRelative("CorrectIndex").intValue = 1;
            verdict.FindPropertyRelative("FeedbackCorrect").stringValue =
                "Case closed! You correctly identified this as an AI-generated image. " +
                "Your three clues — the visual artefact, your gut check, and the motive — " +
                "all pointed to the same conclusion. That's critical thinking in action.";
            verdict.FindPropertyRelative("FeedbackIncorrect").stringValue =
                "Not quite. Review the clues: the distorted hands, the too-perfect lighting, " +
                "and the clear motive to manipulate all point to AI generation. " +
                "Head back to the Chief for a hint.";

            // ── Hints from the Chief ──────────────────────────────────────────
            var hintVideo = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Video/HintFromChief.mp4");
            if (hintVideo == null) Debug.LogWarning("ETEC510: HintFromChief.mp4 not found.");
            so.FindProperty("HintVideo").objectReferenceValue = hintVideo;

            so.FindProperty("HintText").stringValue =
                "Listen up, Detective.\n\n" +
                "Three questions to ask ANY suspicious image:\n\n" +
                "1. Can I find this image somewhere credible with a date and source?\n" +
                "2. Do the physical details (hands, shadows, text) look right?\n" +
                "3. What strong emotion is this image trying to make me feel — " +
                "and who gains from that reaction?\n\n" +
                "Now go back and look again.";

            // ── Level Complete ────────────────────────────────────────────────
            so.FindProperty("CompletionText").stringValue =
                "Outstanding detective work!\n\n" +
                "You spotted the visual glitch, trusted your gut, identified the motive, " +
                "and delivered the correct verdict.\n\n" +
                "Remember: Before sharing any image, take 30 seconds to check the source, " +
                "look for artefacts, and ask who benefits. That's how misinformation stops spreading.";

            // ── Rewards ───────────────────────────────────────────────────────
            so.FindProperty("XpForCompletion").intValue = 100;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            Debug.Log("ETEC510: Case 01 populated successfully.");
        }
    }
}
