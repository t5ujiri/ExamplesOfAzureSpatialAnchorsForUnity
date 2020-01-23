using System;
using CAFU.Core;
using UniRx;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common
{
    public interface IInstructionUIPresenter : IPresenter
    {
        void UpdateMessage(string message);
        IObservable<Unit> OnTriggerProceed();
    }
}