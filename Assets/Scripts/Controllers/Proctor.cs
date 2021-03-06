using System;
using Testing;
using CustomInput;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils.SystemExtensions;

using MinVR;

namespace CustomEvent
{
    [Serializable]
    public class TestingLayoutEvent : UnityEngine.Events.UnityEvent<LayoutOption>
    { }

    [Serializable]
    public class TestingTrialEvent : UnityEngine.Events.UnityEvent<bool>
    { }

    [Serializable]
    public class TestingChallengeEvent : UnityEngine.Events.UnityEvent
    { }
}

namespace Controller
{
    // A class to orchestrate the behavior during a trial and record challenge data
#pragma warning disable 649
    public class Proctor : TextOutputDisplay
    {
        #region EditorSet
        [SerializeField]
        private Utils.Pulser backspaceDown;

        [SerializeField]
        private Button practiceEndButton;

        [SerializeField]
        private Text challengeTypeIndicator;

        [SerializeField]
        private TMPro.TMP_Text fileOutputIndicator;

        [Tooltip("Called when the trial requests a layout change")]
        public CustomEvent.TestingLayoutEvent OnLayoutChange;

        [Tooltip("Called when a trial ends")]
        public CustomEvent.TestingTrialEvent OnTrialEnd;

        [Tooltip("Called when a challenge ends")]
        public CustomEvent.TestingChallengeEvent OnChallengeEnd;
        #endregion

        public Challenge.Type? currentChallengeType;

        public Func<Stylus> stylusProvider = () => null;

        private LayoutOption[] layoutOrder
            = new LayoutOption[] {
                LayoutOption.SliderOnly,
                LayoutOption.ArcType,
                LayoutOption.TiltType,
                LayoutOption.Raycast
            };

        private Trial? currentTrial = null;

        private ResultBuilder builder;

        #region Properties
        private int _layoutIndex = 0;
        private LayoutOption currentLayout
            => layoutOrder[_layoutIndex];

        public override bool emptyText => currentOutput.Length == 0;

        private string currentLayoutName
            => layoutOrder == null ? "" : Enum.GetName(typeof(LayoutOption), currentLayout);

        private int _challengeIndex;
        private TrialItem currentTrialItem
            => currentTrial?.items[_challengeIndex];
        public int? trialNumber
            => currentTrial?.trialNumber;

        private string _prompt;
        public string currentPrompt
        {
            get { return _prompt; }
            set
            {
                _prompt = value;
                ResetText();
            }
        }

        public override string suggestionSource
        {
            get { return currentOutput; }
            protected set
            {
                currentOutput = value;
                UpdateDisplay();
            }
        }

        public string currentOutput { get; private set; }

        private bool completedChallenge
        {
            get
            {
                switch (currentChallengeType)
                {
                    case null:
                        return false;
                    case Challenge.Type.Practice:
                        return false;
                    case Challenge.Type.Perfect:
                        return currentOutput.Equals(currentPrompt);
                    case Challenge.Type.Blind:
                        return currentOutput.Length >= currentPrompt.Length;
                }
                throw new ArgumentException(currentChallengeType.ToString());
            }
        }

        private Utils.Tuples.Orientation stylusTravel
            => stylusProvider()?.travel ?? Utils.Tuples.Orientation.Origin();

        private Vector3 stylusPosition
            => stylusProvider()?.transform.position ?? Vector3.zero;
        #endregion

        public override void Start()
        {
            base.Start();
            fileOutputIndicator.text = "";
            practiceEndButton.onClick.AddListener(OnPracticeButtonDown);
            UpdateDisplay();
        }

        public void OnDestroy()
            => FlushBuilder();

        protected override void OnSuggestionButtonClick(string suggestionText)
        {
            builder?.AddKeypress(MakeKeypress(suggestionText));
            base.OnSuggestionButtonClick(suggestionText);
        }

        public override void ResetText()
        {
            base.ResetText();
            currentOutput = "";

            if (currentChallengeType.HasValue)
            {
                UpdateDisplay();
            }
        }

        public override void AppendLetter(char c)
        {
            base.AppendLetter(c);
            currentOutput += c;
            builder?.AddKeypress(MakeKeypress(c));
            UpdateDisplay();
        }

        public override void TypedBackspace()
        {
            builder?.AddKeypress(MakeKeypress('\b'));
            switch (currentChallengeType)
            {
                case null:
                    base.TypedBackspace();
                    return;
                case Challenge.Type.Practice:
                    currentOutput = currentOutput.Backspace();
                    UpdateDisplay();
                    return;
                case Challenge.Type.Blind:
                    backspaceDown?.Flash();
                    Debug.LogWarning("Disregarding Backspace during Blind Challenge!");
                    return;
                case Challenge.Type.Perfect:
                    currentOutput = currentOutput.Backspace();
                    UpdateDisplay();
                    return;
            }

            throw new ArgumentException(currentChallengeType.ToString());
        }

        private Keypress MakeKeypress(char c)
            => MakeKeypress($"{c}");

        private Keypress MakeKeypress(string s)
        {
            var kp = new Keypress(s, stylusTravel, stylusPosition);
            var sty = stylusProvider();
            if (sty != null)
            {
                sty.travel = Utils.Tuples.Orientation.Origin();
            }
            return kp;
        }

        private void UpdateDisplay()
        {
            if (completedChallenge)
            {
                AdvanceChallenge();
            }

            practiceEndButton.gameObject.SetActive(false);
            challengeTypeIndicator.text =
                currentChallengeType == null ?
                    "Sandbox" :
                    Enum.GetName(typeof(Challenge.Type), currentChallengeType);

            switch (currentChallengeType)
            {
                case null:
                    return;
                case Challenge.Type.Practice:
                    practiceEndButton.gameObject.SetActive(true);
                    text = currentOutput.Length == 0 ? currentPrompt : currentOutput;
                    return;
                case Challenge.Type.Blind:
                    int l = currentOutput.Length;
                    text = $"<color=blue>{"*".Repeat(l)}</color>{currentPrompt.Substring(l)}";
                    return;
                case Challenge.Type.Perfect:
                    string final = "";
                    bool lastCorrect = false;
                    for (int i = 0; i < currentOutput.Length; i++)
                    {
                        // get char
                        char c = currentOutput[i];

                        // get target char
                        bool exceedsPrompt = i >= currentPrompt.Length;
                        char? target = exceedsPrompt ? (char?)null : currentPrompt[i];

                        // check if correct
                        bool correct = target == c;

                        // if it is not the same correctness as the last character...
                        if (lastCorrect != correct || i == 0)
                        {
                            // ... end the last color block and start a new properly colored one ...
                            final += $"{(i > 0 ? "</color>" : "")}<color={(correct ? "green" : "red")}>";
                            // ... save the change in correctness
                            lastCorrect = correct;
                        }

                        // add the character to the string, if ' ', use -
                        if (c == ' ')
                        {
                            final += '-';
                        }
                        else
                        {
                            final += c;
                        }
                    }

                    if (currentOutput.Length > 0)
                    {
                        // close the last color block
                        final += "</color>";
                    }

                    // add any remaining characters in the prompt
                    if (currentOutput.Length < currentPrompt.Length)
                    {
                        final += currentPrompt.Substring(currentOutput.Length);
                    }


                    text = final;
                    return;
            }
            throw new ArgumentException(currentChallengeType.ToString());
        }

        public void RandomizeLayouts()
        {
            System.Random rnd = new System.Random();
            layoutOrder = ((LayoutOption[])Enum.GetValues(typeof(LayoutOption))).OrderBy(_x => rnd.Next()).ToArray();
        }

        public void AdvanceLayout()
        {
            AdvanceIndexCyclic();
            OnLayoutChange.Invoke(currentLayout);
        }

        public void RunTrial(Trial t)
        {
            currentTrial = t;
            _challengeIndex = -1;
            builder = new ResultBuilder();
            AdvanceChallenge();
        }

        public void SetLayout(LayoutOption value)
        {
            while (currentLayout != value)
            {
                AdvanceIndexCyclic();
            }
            OnLayoutChange.Invoke(currentLayout);
        }

        private void AdvanceIndexCyclic()
            => _layoutIndex = (_layoutIndex + 1) % layoutOrder.Length;

        public void RestartChallenge()
        {
            builder?.RestartLastChallenge();
            currentOutput = "";
            UpdateDisplay();
        }

        public void AdvanceChallenge()
        {
            if (!currentTrial.HasValue) return;

            if (_challengeIndex > -1)
            {
                OnChallengeEnd.Invoke();
            }

            _challengeIndex++;

            if (_challengeIndex >= currentTrial.Value.Length)
            {
                FinishTrial();
                return;
            }

            builder.Push(currentTrialItem, currentOutput, currentLayoutName);

            if (currentTrialItem.Apply(this))
            {
                UpdateDisplay();
                return;
            }

            AdvanceChallenge();
        }

        public void FinishTrial()
        {
            Debug.LogWarning($"Completed Trial {trialNumber}");

            FlushBuilder();

            OnTrialEnd.Invoke(true);
        }

        public void RestartTrial()
        {
            if (!currentTrial.HasValue) return;

            Debug.LogWarning($"Restarted Trial {trialNumber}");

            RunTrial(currentTrial.Value);
        }

        public void FlushBuilder()
        {
            if (builder == null) return;
            try
            {
                if (VRMain.Instance.vrDevice.name == "Desktop" || VRMain.Instance.vrDevice.name == "CaveFrontWall_Top")
                {
                    var path = Testing.Utils.WriteTrialResults(builder.Finish(currentOutput), locally: Application.isEditor);

                    fileOutputIndicator.text += $"Trial {trialNumber} Completed!\nSaved in Directory: {path.directory}\nIn File: {path.name}\n";
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                fileOutputIndicator.text += $"Trial {trialNumber} completed!\nSaving Failed with: {e.Message}";
            }
            finally
            {
                currentTrial = null;
                builder = null;
            }
        }

        private void OnPracticeButtonDown()
        {
            if (currentChallengeType == Challenge.Type.Practice)
            {
                AdvanceChallenge();
            }
            else
            {
                Debug.LogWarning("Practice End Button pressed outside of practice!");
            }
        }
    }
}
