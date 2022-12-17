using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InterfaceHost.CommonPage
{
    public class MessageDialogPage : UserInterfacePageBase<MessageDialogSettings>
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;

        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _closeButtonText;

        public override void Initialize(MessageDialogSettings argument)
        {
            if (!string.IsNullOrWhiteSpace(argument.Title))
                _titleText.text = argument.Title;

            if (!string.IsNullOrWhiteSpace(argument.Message))
                _messageText.text = argument.Message;

            if (!string.IsNullOrWhiteSpace(argument.CloseButtonText))
                _closeButtonText.text = argument.CloseButtonText;
        }

        public void AddCloseButtonClickCallback(UnityAction action)
        {
            _closeButton.onClick.AddListener(action);
        }
    }

    public class MessageDialogSettings
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public string CloseButtonText { get; set; }

        public MessageDialogSettings(string message)
        {
            Message = message;
        }
    }

    public static class MessageDialogExtensions
    {
        public static UniTask ShowDialog(this UserInterfaceHost host, MessageDialogSettings argument,
            RectTransform parent = null)
        {
            var uniTask = new UniTaskCompletionSource();
            var dialog = host.ShowMessage(argument, parent);
            dialog.AddCloseButtonClickCallback(() =>
            {
                host.Close();
                uniTask.TrySetResult();
            });

            return uniTask.Task;
        }
    }
}