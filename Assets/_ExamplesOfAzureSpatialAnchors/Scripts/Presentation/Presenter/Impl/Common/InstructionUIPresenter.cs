using System;
using GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common;
using TMPro;
using UniRx;
using UnityEngine.UI;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Impl.Common
{
    public class InstructionUIPresenter : IInstructionUIPresenter
    {
        [Inject(Id = "Text - Message")] private readonly TextMeshProUGUI _messageText = default;
        [Inject(Id = "Button - Proceed")] private readonly Button _proceedButton = default;

        public void UpdateMessage(string message)
        {
            _messageText.text = message;
        }

        public IObservable<Unit> OnTriggerProceed()
        {
            return _proceedButton.OnClickAsObservable();
        }
    }
}