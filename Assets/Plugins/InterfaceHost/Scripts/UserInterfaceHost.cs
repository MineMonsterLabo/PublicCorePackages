using System.Collections.Generic;
using InterfaceHost.CommonPage;
using UniRx;
using UnityEngine;
using Zenject;

namespace InterfaceHost
{
    public class UserInterfaceHost : MonoBehaviour
    {
        [Inject] private DiContainer _container;

        private readonly Dictionary<int, UserInterfacePageBase> _createdInstances =
            new Dictionary<int, UserInterfacePageBase>();

        private readonly UserInterfaceStack _stack = new UserInterfaceStack();

        [SerializeField] private RectTransform _defaultRootTransform;
        [SerializeField] private List<UserInterfacePageBase> _pagePrefabs = new List<UserInterfacePageBase>();

        [SerializeField] private MessageDialogPage _messageDialogPage;
        [SerializeField] private YesNoDialogPage _yesNoDialogPage;
        [SerializeField] private ProgressDialogPage _progressDialogPage;

        //TODO: Skin

        public int RegisterPagePrefab(UserInterfacePageBase pagePrefab)
        {
            _pagePrefabs.Add(pagePrefab);
            return _pagePrefabs.IndexOf(pagePrefab);
        }

        public IUserInterfacePage<TArgument> Show<TArgument>(int id, TArgument argument, RectTransform parent = null)
        {
            return Show(_pagePrefabs[id], argument, parent);
        }

        public MessageDialogPage ShowMessage(MessageDialogSettings argument, RectTransform parent = null)
        {
            return Show(_messageDialogPage, argument, parent) as MessageDialogPage;
        }

        public YesNoDialogPage ShowYesNo(YesNoDialogSettings argument, RectTransform parent = null)
        {
            return Show(_yesNoDialogPage, argument, parent) as YesNoDialogPage;
        }

        public ProgressDialogPage ShowProgress(RectTransform parent = null)
        {
            return Show(_progressDialogPage, Unit.Default, null) as ProgressDialogPage;
        }

        public void Hide()
        {
            _stack.Pop();
        }

        public void HideAll()
        {
            _stack.Clear();
        }

        public void Close()
        {
            var page = _stack.Pop();
            Destroy(page.gameObject);

            var pageId = _pagePrefabs.IndexOf(page);
            _createdInstances.Remove(pageId);
        }

        public void CloseAll()
        {
            while (_stack.Count > 0)
            {
                var page = _stack.Pop();
                Destroy(page.gameObject);

                var pageId = _pagePrefabs.IndexOf(page);
                _createdInstances.Remove(pageId);
            }
        }

        private IUserInterfacePage<TArgument> Show<TArgument>(UserInterfacePageBase prefab, TArgument argument,
            RectTransform parent = null)
        {
            if (!parent)
                parent = _defaultRootTransform;

            UserInterfacePageBase<TArgument> instance;
            var pageId = _pagePrefabs.IndexOf(prefab);
            if (!_createdInstances.ContainsKey(pageId))
            {
                var pagePrefab = (UserInterfacePageBase<TArgument>)prefab;
                instance = Instantiate(pagePrefab, parent);
                _container.Inject(instance);

                instance.Initialize(argument);
            }
            else
            {
                instance = (UserInterfacePageBase<TArgument>)_createdInstances[pageId];
            }

            _createdInstances[pageId] = instance;

            _stack.Push(instance);

            return instance;
        }
    }
}