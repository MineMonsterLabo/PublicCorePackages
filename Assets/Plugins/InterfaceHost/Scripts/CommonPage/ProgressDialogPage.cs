using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace InterfaceHost.CommonPage
{
    public class ProgressDialogPage : UserInterfacePageBase<Unit>
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;

        [SerializeField] private Slider _progressSlider;

        public string Title => _titleText.text;
        public string Description => _descriptionText.text;

        public float Progress => _progressSlider.value;

        public override void Initialize(Unit argument)
        {
            base.Initialize(argument);
        }

        public void SetTitle(string title)
        {
            _titleText.text = title;
        }

        public void SetDescription(string description)
        {
            _descriptionText.text = description;
        }

        public void SetProgress(float progress)
        {
            _progressSlider.value = progress;
        }
    }
}