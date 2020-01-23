using System;
using CAFU.Core;
using UnityEngine;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common
{
    public interface ITouchController : IPresenter
    {
        IObservable<Vector2> GetWorldTouchOnScreen();
        Pose GetARRaycastHitPose(Vector2 screenPoint);
    }
}