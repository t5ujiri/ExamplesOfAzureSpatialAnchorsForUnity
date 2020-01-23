using System;
using System.Collections.Generic;
using GATARI.ExamplesOfAzureSpatialAnchors.Domain.UseCase.Interface.Common;
using GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Interface.Common;
using GATARI.ExamplesOfAzureSpatialAnchors.Presentation.View.Common;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace GATARI.ExamplesOfAzureSpatialAnchors.Presentation.Presenter.Impl.Common
{
    public class AnchorObjectPresenter : IAnchorObjectPresenter
    {
        [Inject] private readonly SpatialAnchorViewFactory _spatialAnchorViewFactory = default;
        // [Inject] private readonly DummyAnchorViewFactory _dummyAnchorViewFactory = default;

        private CloudNativeAnchor _placingAnchor = default;
        private readonly List<ISpatialAnchorView> _spatialAnchorViews = new List<ISpatialAnchorView>();
        private readonly List<IDummyAnchorView> _dummyAnchorViews = new List<IDummyAnchorView>();

        public bool IsPlacingAnchorExists { get; private set; }

        public void CreateNewAnchor(Vector3 position)
        {
            // create new anchor view
            var view = _spatialAnchorViewFactory.Create(null);
            view.GameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            _placingAnchor = view.CloudNativeAnchor;
            IsPlacingAnchorExists = true;
            _spatialAnchorViews.Add(view);
        }

        public void MovePlacingAnchor(Vector3 position)
        {
            if (_placingAnchor == null) throw new ArgumentNullException(nameof(_placingAnchor));
            _placingAnchor.SetPose(new Pose(position, Quaternion.identity));
        }

        public CloudSpatialAnchor ConfirmNewAnchorAndGetCloudSpatialAnchor()
        {
            if (_placingAnchor == null)
            {
                throw new NullReferenceException("_placingAnchor");
            }

            _placingAnchor.NativeToCloud();
            IsPlacingAnchorExists = false;
            return _placingAnchor.CloudAnchor;
        }

        public void RestoreAnchorObject(CloudSpatialAnchor cloudSpatialAnchor, Color color, Vector3 scale)
        {
            // create anchor view with Cloud Spatial Anchor
            var view = _spatialAnchorViewFactory.Create(cloudSpatialAnchor);
            view.GameObject.transform.localScale = scale;
            view.GameObject.GetComponent<MeshRenderer>().material.color = color;
            _spatialAnchorViews.Add(view);
        }

        public void CleanAllAnchors()
        {
            if (_spatialAnchorViews != null)
            {
                foreach (var view in _spatialAnchorViews)
                {
                    Object.Destroy(view.GameObject);
                }

                _spatialAnchorViews?.Clear();
            }

            if (_dummyAnchorViews != null)
            {
                foreach (var view in _dummyAnchorViews)
                {
                    Object.Destroy(view.GameObject);
                }

                _dummyAnchorViews?.Clear();
            }
        }
    }
}