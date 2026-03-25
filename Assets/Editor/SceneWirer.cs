using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using ETEC510.UI;
using ETEC510.Cases;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Wire CaseRunner
    /// Adds CaseRunner to CaseController and wires every Inspector field
    /// to the GameObjects created by SceneBuilder.
    /// Run once after "Build Scene Structure", then save the scene.
    /// </summary>
    public static class SceneWirer
    {
        [MenuItem("ETEC510/Wire CaseRunner")]
        public static void WireScene()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null)
            {
                Debug.LogError("ETEC510: GameCanvas not found. Run ETEC510 > Build Scene Structure first.");
                return;
            }

            var ct = canvasGO.transform;

            var controllerGO = ct.Find("CaseController")?.gameObject;
            if (controllerGO == null)
            {
                Debug.LogError("ETEC510: CaseController not found under GameCanvas.");
                return;
            }

            var runner = controllerGO.GetComponent<CaseRunner>()
                      ?? controllerGO.AddComponent<CaseRunner>();

            var so = new SerializedObject(runner);

            // ── Local helpers ────────────────────────────────────────────────

            T Child<T>(string path) where T : Component
            {
                var t = ct.Find(path);
                if (t == null) Debug.LogWarning($"ETEC510: path not found — {path}");
                return t != null ? t.GetComponent<T>() : null;
            }

            GameObject ChildGO(string path)
            {
                var t = ct.Find(path);
                if (t == null) Debug.LogWarning($"ETEC510: path not found — {path}");
                return t != null ? t.gameObject : null;
            }

            // ── Main AudioSource (on CaseController itself) ───────────────────
            var mainAudio = controllerGO.GetComponent<AudioSource>()
                         ?? controllerGO.AddComponent<AudioSource>();
            mainAudio.playOnAwake = false;
            mainAudio.loop        = true;
            so.FindProperty("mainAudioSource").objectReferenceValue = mainAudio;

            // ── CaseData ─────────────────────────────────────────────────────
            var caseData = AssetDatabase.LoadAssetAtPath<CaseData>(
                "Assets/Cases/Case01_AIImageCredibility.asset");
            if (caseData == null) Debug.LogWarning("ETEC510: CaseData asset not found.");
            so.FindProperty("caseData").objectReferenceValue = caseData;

            // ── Panels ───────────────────────────────────────────────────────
            so.FindProperty("introPanel").objectReferenceValue           = ChildGO("IntroPanel");
            so.FindProperty("missionBriefingPanel").objectReferenceValue = ChildGO("MissionBriefingPanel");
            so.FindProperty("evidenceBoardPanel").objectReferenceValue   = ChildGO("EvidenceBoardPanel");
            so.FindProperty("spotTheCluePanel").objectReferenceValue     = ChildGO("SpotTheCluePanel");
            so.FindProperty("gutCheckPanel").objectReferenceValue        = ChildGO("GutCheckPanel");
            so.FindProperty("findTheMotivePanel").objectReferenceValue   = ChildGO("FindTheMotivePanel");
            so.FindProperty("passwordLockPanel").objectReferenceValue    = ChildGO("PasswordLockPanel");
            so.FindProperty("evidenceDetailPanel").objectReferenceValue  = ChildGO("EvidenceDetailPanel");
            so.FindProperty("hintsFromChiefPanel").objectReferenceValue  = ChildGO("HintsFromChiefPanel");
            so.FindProperty("levelCompletePanel").objectReferenceValue   = ChildGO("LevelCompletePanel");

            // ── Intro ────────────────────────────────────────────────────────
            so.FindProperty("introVideoDisplay").objectReferenceValue =
                Child<RawImage>("IntroPanel/VideoBackground");
            so.FindProperty("introVideoPlayer").objectReferenceValue =
                Child<VideoPlayer>("IntroPanel");
            so.FindProperty("introAudioSource").objectReferenceValue =
                Child<AudioSource>("IntroPanel");
            so.FindProperty("introEnterButton").objectReferenceValue =
                Child<Button>("IntroPanel/EnterButton");
            so.FindProperty("introSkipButton").objectReferenceValue =
                Child<Button>("IntroPanel/SkipButton");

            // ── Mission Briefing ─────────────────────────────────────────────
            so.FindProperty("briefingTitleText").objectReferenceValue =
                Child<TMP_Text>("MissionBriefingPanel/TitleText");
            so.FindProperty("briefingBodyText").objectReferenceValue =
                Child<TMP_Text>("MissionBriefingPanel/BodyText");
            so.FindProperty("briefingImage").objectReferenceValue =
                Child<Image>("MissionBriefingPanel/BriefingImage");
            so.FindProperty("briefingStartButton").objectReferenceValue =
                Child<Button>("MissionBriefingPanel/StartButton");

            // ── Evidence Board ───────────────────────────────────────────────
            so.FindProperty("evidenceBoardImage").objectReferenceValue =
                Child<Image>("EvidenceBoardPanel/EvidenceBoardImage");
            so.FindProperty("spotTheClueButton").objectReferenceValue =
                Child<Button>("EvidenceBoardPanel/SpotTheClueButton");
            so.FindProperty("gutCheckButton").objectReferenceValue =
                Child<Button>("EvidenceBoardPanel/GutCheckButton");
            so.FindProperty("findTheMotiveButton").objectReferenceValue =
                Child<Button>("EvidenceBoardPanel/FindTheMotiveButton");
            so.FindProperty("enterPasswordButton").objectReferenceValue =
                Child<Button>("EvidenceBoardPanel/EnterPasswordButton");
            so.FindProperty("digit1Text").objectReferenceValue =
                Child<TMP_Text>("EvidenceBoardPanel/CodeDisplay/Digit1Text");
            so.FindProperty("digit2Text").objectReferenceValue =
                Child<TMP_Text>("EvidenceBoardPanel/CodeDisplay/Digit2Text");
            so.FindProperty("digit3Text").objectReferenceValue =
                Child<TMP_Text>("EvidenceBoardPanel/CodeDisplay/Digit3Text");

            // ── Spot the Clue ────────────────────────────────────────────────
            so.FindProperty("spotPromptText").objectReferenceValue =
                Child<TMP_Text>("SpotTheCluePanel/SpotPromptText");
            so.FindProperty("spotEvidenceImage").objectReferenceValue =
                Child<Image>("SpotTheCluePanel/SpotEvidenceImage");
            so.FindProperty("spotFeedbackText").objectReferenceValue =
                Child<TMP_Text>("SpotTheCluePanel/SpotFeedbackText");
            var spotOpts = so.FindProperty("spotOptionButtons");
            spotOpts.arraySize = 2;
            spotOpts.GetArrayElementAtIndex(0).objectReferenceValue =
                Child<Button>("SpotTheCluePanel/SpotOptionButton0");
            spotOpts.GetArrayElementAtIndex(1).objectReferenceValue =
                Child<Button>("SpotTheCluePanel/SpotOptionButton1");
            so.FindProperty("spotBackButton").objectReferenceValue =
                Child<Button>("SpotTheCluePanel/SpotBackButton");

            // ── Gut Check ────────────────────────────────────────────────────
            so.FindProperty("gutPromptText").objectReferenceValue =
                Child<TMP_Text>("GutCheckPanel/GutPromptText");
            so.FindProperty("gutEvidenceImage").objectReferenceValue =
                Child<Image>("GutCheckPanel/GutEvidenceImage");
            so.FindProperty("gutFeedbackText").objectReferenceValue =
                Child<TMP_Text>("GutCheckPanel/GutFeedbackText");
            so.FindProperty("gutNextButton").objectReferenceValue =
                Child<Button>("GutCheckPanel/GutNextButton");
            so.FindProperty("gutBackButton").objectReferenceValue =
                Child<Button>("GutCheckPanel/GutBackButton");

            var gutOpts = so.FindProperty("gutOptionButtons");
            gutOpts.arraySize = 2;
            gutOpts.GetArrayElementAtIndex(0).objectReferenceValue =
                Child<Button>("GutCheckPanel/GutOptionButton0");
            gutOpts.GetArrayElementAtIndex(1).objectReferenceValue =
                Child<Button>("GutCheckPanel/GutOptionButton1");

            // ── Find the Motive ──────────────────────────────────────────────
            so.FindProperty("motivePromptText").objectReferenceValue =
                Child<TMP_Text>("FindTheMotivePanel/MotivePromptText");
            so.FindProperty("motiveEvidenceImage").objectReferenceValue =
                Child<Image>("FindTheMotivePanel/MotiveEvidenceImage");
            so.FindProperty("motiveFeedbackText").objectReferenceValue =
                Child<TMP_Text>("FindTheMotivePanel/MotiveFeedbackText");
            var motiveOpts = so.FindProperty("motiveOptionButtons");
            motiveOpts.arraySize = 2;
            motiveOpts.GetArrayElementAtIndex(0).objectReferenceValue =
                Child<Button>("FindTheMotivePanel/MotiveOptionButton0");
            motiveOpts.GetArrayElementAtIndex(1).objectReferenceValue =
                Child<Button>("FindTheMotivePanel/MotiveOptionButton1");
            so.FindProperty("motiveBackButton").objectReferenceValue =
                Child<Button>("FindTheMotivePanel/MotiveBackButton");

            // ── Password Lock ────────────────────────────────────────────────
            so.FindProperty("passwordInputField").objectReferenceValue =
                Child<TMP_InputField>("PasswordLockPanel/PasswordInputField");
            so.FindProperty("passwordFeedbackText").objectReferenceValue =
                Child<TMP_Text>("PasswordLockPanel/PasswordFeedbackText");
            so.FindProperty("passwordSubmitButton").objectReferenceValue =
                Child<Button>("PasswordLockPanel/PasswordSubmitButton");
            so.FindProperty("passwordBackButton").objectReferenceValue =
                Child<Button>("PasswordLockPanel/PasswordBackButton");

            // ── Evidence Detail (Verdict) ────────────────────────────────────
            so.FindProperty("verdictPromptText").objectReferenceValue =
                Child<TMP_Text>("EvidenceDetailPanel/VerdictPromptText");
            so.FindProperty("verdictEvidenceImage").objectReferenceValue =
                Child<Image>("EvidenceDetailPanel/VerdictEvidenceImage");
            so.FindProperty("verdictBackButton").objectReferenceValue =
                Child<Button>("EvidenceDetailPanel/VerdictBackButton");

            var verdictOpts = so.FindProperty("verdictOptionButtons");
            verdictOpts.arraySize = 2;
            verdictOpts.GetArrayElementAtIndex(0).objectReferenceValue =
                Child<Button>("EvidenceDetailPanel/VerdictOptionButton0");
            verdictOpts.GetArrayElementAtIndex(1).objectReferenceValue =
                Child<Button>("EvidenceDetailPanel/VerdictOptionButton1");

            // ── Hints from Chief ─────────────────────────────────────────────
            so.FindProperty("hintVideoDisplay").objectReferenceValue =
                Child<RawImage>("HintsFromChiefPanel/HintVideoDisplay");
            so.FindProperty("hintVideoPlayer").objectReferenceValue =
                Child<VideoPlayer>("HintsFromChiefPanel");
            so.FindProperty("hintOverlay").objectReferenceValue =
                ChildGO("HintsFromChiefPanel/HintOverlay");
            so.FindProperty("hintBodyText").objectReferenceValue =
                Child<TMP_Text>("HintsFromChiefPanel/HintBodyText");
            so.FindProperty("hintImage").objectReferenceValue =
                Child<Image>("HintsFromChiefPanel/ChiefImage");
            so.FindProperty("hintTryAgainButton").objectReferenceValue =
                Child<Button>("HintsFromChiefPanel/HintTryAgainButton");
            so.FindProperty("hintReturnButton").objectReferenceValue =
                Child<Button>("HintsFromChiefPanel/HintOverlay/HintReturnButton");

            // ── Level Complete ───────────────────────────────────────────────
            so.FindProperty("completionBodyText").objectReferenceValue =
                Child<TMP_Text>("LevelCompletePanel/CompletionBodyText");
            so.FindProperty("completionImage").objectReferenceValue =
                Child<Image>("LevelCompletePanel/CompletionImage");
            so.FindProperty("xpText").objectReferenceValue =
                Child<TMP_Text>("LevelCompletePanel/XPText");

            // ── Commit ───────────────────────────────────────────────────────
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("ETEC510: CaseRunner wired successfully. Press Ctrl+S to save the scene.");
        }
    }
}
