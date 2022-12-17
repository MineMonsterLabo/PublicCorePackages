using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InterfaceHost.CommonPage
{
    public class YesNoDialogPage : UserInterfacePageBase<YesNoDialogSettings>
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;

        [SerializeField] private Button _yesButton;
        [SerializeField] private TMP_Text _yesButtonText;
        [SerializeField] private Button _noButton;
        [SerializeField] private TMP_Text _noButtonText;

        public override void Initialize(YesNoDialogSettings argument)
        {
            if (!string.IsNullOrWhiteSpace(argument.Title))
                _titleText.text = argument.Title;

            if (!string.IsNullOrWhiteSpace(argument.Message))
                _messageText.text = argument.Message;

            if (!string.IsNullOrWhiteSpace(argument.YesButtonText))
                _yesButtonText.text = argument.YesButtonText;

            if (!string.IsNullOrWhiteSpace(argument.NoButtonText))
                _noButtonText.text = argument.NoButtonText;
        }

        public void AddYesButtonClickCallback(UnityAction action)
        {
            _yesButton.onClick.AddListener(action);
        }

        public void AddNoButtonClickCallback(UnityAction action)
        {
            _noButton.onClick.AddListener(action);
        }
    }

    public class YesNoDialogSettings
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public string YesButtonText { get; set; }
        public string NoButtonText { get; set; }

        public YesNoDialogSettings(string message)
        {
            Message = message;
        }
    }

    public static class YesNoDialogExtensions
    {
        public static UniTask<bool> ShowDialog(this UserInterfaceHost host, YesNoDialogSettings argument,
            RectTransform parent = null)
        {
            var uniTask = new UniTaskCompletionSource<bool>();

            void OnDialogButton(bool result)
            {
                host.Close();
                uniTask.TrySetResult(result);
            }

            var dialog = host.ShowYesNo(argument, parent);
            dialog.AddYesButtonClickCallback(() => OnDialogButton(true));
            dialog.AddNoButtonClickCallback(() => OnDialogButton(false));

            return uniTask.Task;
        }
    }
}