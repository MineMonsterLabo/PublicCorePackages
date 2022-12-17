using System.Collections.Generic;

namespace InterfaceHost
{
    public class UserInterfaceStack
    {
        private readonly Stack<UserInterfacePageBase> _stack = new Stack<UserInterfacePageBase>();

        public int Count => _stack.Count;

        public void Push<T>(UserInterfacePageBase<T> page)
        {
            page.OnBeginPushStack();
            page.gameObject.SetActive(true);
            _stack.Push(page);
            page.OnEndPushStack();
        }

        public UserInterfacePageBase Pop()
        {
            if (_stack.Count == 0)
                return null;

            _stack.Peek().OnBeginPopStack();
            var page = _stack.Pop();
            page.gameObject.SetActive(false);
            page.OnEndPopStack();

            return page;
        }

        public UserInterfacePageBase PopAndPush<T>(UserInterfacePageBase<T> page)
        {
            UserInterfacePageBase popResult = Pop();
            Push(page);

            return popResult;
        }

        public void Clear()
        {
            while (_stack.Count > 0)
            {
                _stack.Peek().OnBeginPopStack();
                var page = _stack.Pop();
                page.gameObject.SetActive(false);
                page.OnEndPopStack();
            }

            _stack.Clear();
        }
    }

    public interface IPageStackCallback
    {
        void OnBeginPushStack();
        void OnEndPushStack();
        void OnBeginPopStack();
        void OnEndPopStack();
    }
}