using System;
using System.Collections.Generic;
using GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common;
using Lean.Touch;
using UniRx;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Impl.Controller
{
    public class LeanTouchController : ITouchController
    {
        [Inject] private readonly ARRaycastManager _arRaycastManager = default;
        [Inject] private readonly Camera _arCamera = default;

        private static readonly List<ARRaycastHit> ArRaycastHits = new List<ARRaycastHit>();

        public IObservable<Vector2> GetWorldTouchOnScreen()
        {
            return Observable.FromEvent<LeanFinger>(
                    h => LeanTouch.OnFingerTap += h,
                    h => LeanTouch.OnFingerTap -= h)
                .Where(f => !f.IsOverGui)
                .Select(f => f.ScreenPosition)
                .Share();
        }

        public Pose GetARRaycastHitPose(Vector2 screenPoint)
        {
            _arRaycastManager.Raycast(screenPoint, ArRaycastHits, TrackableType.PlaneWithinPolygon);
            if (ArRaycastHits.Count == 0)
            {
                var transform = _arCamera.transform;
                return new Pose(
                    transform.position + transform.forward,
                    transform.rotation
                );
            }
            else
            {
                return ArRaycastHits[0].pose;
            }
        }
    }
}