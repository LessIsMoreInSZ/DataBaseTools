using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataBaseTools.net48.ViewModels.Dialogs
{
    public class MessageViewModel : BindableBase, IDialogAware
    {
        #region IDialogAware实现

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        private MessageLanguage messageLanguage = new MessageLanguage();
        public MessageLanguage MessageLanguage
        {
            get { return messageLanguage; }
            set { messageLanguage = value; RaisePropertyChanged(); }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Content"))
            {
                Content = parameters.GetValue<string>("Content");
            }
        }

        #endregion


        public MessageViewModel()
        {
        }




        public ICommand SaveCommand
        {
            get => new DelegateCommand(() =>
            {
                DialogResult dialogResult = new DialogResult(ButtonResult.OK);
                RequestClose?.Invoke(dialogResult);
            });
        }

        public ICommand CancelCommand
        {
            get => new DelegateCommand(() =>
            {
                DialogResult dialogResult = new DialogResult(ButtonResult.Cancel);
                RequestClose?.Invoke(dialogResult);
            });
        }


        private string _content;

        public string Content
        {
            get { return _content; }
            set { _content = value; RaisePropertyChanged(); }
        }
    }

    public class MessageLanguage : BindableBase
    {
        private string title = "Tips";
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        private string ok = "OK";
        public string Ok
        {
            get { return ok; }
            set { ok = value; RaisePropertyChanged(); }
        }

        private string cancel = "Cancel";
        public string Cancel
        {
            get { return cancel; }
            set { cancel = value; RaisePropertyChanged(); }
        }
    }
}
